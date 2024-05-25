using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.Model
{
    public class ConsolidatedAlerts {
        private readonly string _connectionString;
        private readonly string _connectionStringSystem;

        public ConsolidatedAlerts(string connectionString, string connectionStringSystem) {
            _connectionString = connectionString;
            _connectionStringSystem = connectionStringSystem;
        }

        public string ProcessDetailRAM(DateTime fromTime, DateTime toTime, string systemSerialNum, int customerID, int scope, bool isCritical = false, bool isMajor = false) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            var dset = new DataSet();
            var returnStr = new StringBuilder();
            string heading = "";
            bool flagContent = false;
            bool flagContentProc = false;
            string start = Convert.ToDateTime(fromTime).ToString("yyyy-MM-dd HH:mm:ss");
            string stop = Convert.ToDateTime(toTime).ToString("yyyy-MM-dd HH:mm:ss");
            var dconnTrend = new MySqlConnection(_connectionStringSystem);
            dconnTrend.Open();
            var dconn = new MySqlConnection(_connectionString);
            dconn.Open();

            var alerts = new List<int>();

            if (isCritical && isMajor) {
                alerts.Add(0);
                alerts.Add(1);
            } else if (isCritical) {
                alerts.Add(0);
            } else if (isMajor) {
                alerts.Add(1);
            } else {
                alerts.Add(0);
                alerts.Add(1);
                alerts.Add(2);
                alerts.Add(3);
                alerts.Add(4);
            }

            #region detail_proc
            foreach (var i in alerts) {
                bool procFlag = false;
                switch (i) {
                    case 0:
                        heading = "<img style='border-style:none;' alt='Critical'  src='cid:critical'  />&nbsp;Critical";
                        break;

                    case 1:
                        heading = "<img style='border-style:none;' alt='Major'  src='cid:major'  />&nbsp;Major";
                        break;

                    case 2:
                        heading = "<img style='border-style:none;' alt='Minor'  src='cid:minor' />&nbsp;Minor";
                        break;

                    case 3:
                        heading = "<img style='border-style:none;' alt='Warning'  src='cid:warning'/>&nbsp;Warning";
                        break;

                    case 4:
                        heading = "<img style='border-style:none;' alt='Information' src='cid:info'/>&nbsp;Info";
                        break;
                }

                var sqlStr = new StringBuilder();
                sqlStr.Append("SELECT AlertID " +
                            "FROM (RAM_ProfileRule INNER JOIN RAM_Counter ON " +
                            "(RAM_ProfileRule.EntityID=RAM_Counter.EntityID) AND " +
                            "(RAM_ProfileRule.CounterID = RAM_Counter.CounterID)) " +
                            "INNER JOIN RAM_Entity ON (RAM_ProfileRule.EntityID=RAM_Entity.EntityID) " +
                            "WHERE SystemSerialNum = '" + systemSerialNum + "' AND Severity=" + i +
                            " AND RAM_ProfileRule.EntityID='1'");


                if (scope == 2)
                    sqlStr.Append(
                        " AND ((RAM_ProfileRule.Scope IS NULL OR RAM_ProfileRule.Scope = '0') OR (RAM_ProfileRule.Scope='1' AND RAM_ProfileRule.CreatorID='" +
                        customerID + "')) ");
                else if (scope == 1)
                    sqlStr.Append(" AND (RAM_ProfileRule.Scope='1' AND RAM_ProfileRule.CreatorID='" + customerID + "') ");
                else if (scope == 0)
                    sqlStr.Append(" AND (RAM_ProfileRule.Scope IS NULL OR RAM_ProfileRule.Scope = '0') ");
                else
                    return returnStr.ToString();

                var dcomd = new MySqlCommand {
                    Connection = dconn,
                    CommandTimeout = 10000,
                    CommandText = sqlStr.ToString()
                };

                var dread = dcomd.ExecuteReader();

                if (dread.Read())
                    flagContentProc = true;
                dread.Close();

                sqlStr = new StringBuilder();
                sqlStr.Append("SELECT AlertID,AlertsTable,Name,CounterField,RAM_ProfileRule.EntityID," +
                        "RAM_Entity.EntityName,CounterName " +
                        "FROM (RAM_ProfileRule INNER JOIN RAM_Counter ON " +
                        "(RAM_ProfileRule.EntityID=RAM_Counter.EntityID) AND " +
                        "(RAM_ProfileRule.CounterID = RAM_Counter.CounterID)) " +
                        "INNER JOIN RAM_Entity ON (RAM_ProfileRule.EntityID=RAM_Entity.EntityID) " +
                        "WHERE SystemSerialNum = '" + systemSerialNum + "' AND Severity=" + i);


                if (scope == 2)
                    sqlStr.Append(
                        " AND ((RAM_ProfileRule.Scope IS NULL OR RAM_ProfileRule.Scope = '0') OR (RAM_ProfileRule.Scope='1' AND RAM_ProfileRule.CreatorID='" +
                        customerID + "')) ");
                else if (scope == 1)
                    sqlStr.Append(" AND (RAM_ProfileRule.Scope='1' AND RAM_ProfileRule.CreatorID='" + customerID + "') ");
                else if (scope == 0)
                    sqlStr.Append(" AND (RAM_ProfileRule.Scope IS NULL OR RAM_ProfileRule.Scope = '0') ");
                else
                    return returnStr.ToString();


                dcomd = new MySqlCommand {
                    CommandText = sqlStr.ToString(),
                    Connection = dconn,
                    CommandTimeout = 10000
                };
                var dadapt = new MySqlDataAdapter();
                dadapt.SelectCommand = dcomd;
                dadapt.Fill(dset, "SampleInfo");

                if (dset.Tables["SampleInfo"].Rows.Count == 0) {
                    continue;
                }

                var contentStr = new StringBuilder();
                //Build DataTime accordingly to Start and End.

                //for these alertids, get the alert values from the alerttables
                for (int a = 0; a < dset.Tables["SampleInfo"].Rows.Count; a++) {
                    string tableName = dset.Tables["SampleInfo"].Rows[a]["AlertsTable"].ToString();
                    string entityid = dset.Tables["SampleInfo"].Rows[a]["EntityID"].ToString();

                    sqlStr = new StringBuilder();
                    sqlStr.Append("SELECT DateTime");
                    if (entityid == "0" || entityid == "4")
                        sqlStr.Append(",CPU");
                    else if (entityid == "1") {
                        procFlag = true;
                        sqlStr.Append(",ProcessName,ProgramName,AncestorProcessName, AncestorProgramName, Duration");
                    } else if (entityid == "2")
                        sqlStr.Append(",Disc");
                    else if (entityid == "3")
                        sqlStr.Append(",DiskFile");
                    sqlStr.Append("," + dset.Tables["SampleInfo"].Rows[a]["CounterField"] +
                              " FROM " + tableName + " WHERE AlertID = " + dset.Tables["SampleInfo"].Rows[a]["AlertID"] +
                              " AND DateTime BETWEEN '" + start + "' AND '" + stop + "'");
                    sqlStr.Append(" ORDER BY DateTime");

                    dcomd = new MySqlCommand {
                        CommandText = sqlStr.ToString(),
                        Connection = dconnTrend,
                        CommandTimeout = 10000
                    };
                    dadapt = new MySqlDataAdapter();
                    dadapt.SelectCommand = dcomd;
                    dadapt.Fill(dset, "Detail");
                    if (dset.Tables["Detail"].Rows.Count > 0)
                        flagContent = true;
                    //display dataset in the gridview
                    for (int k = 0; k < dset.Tables["Detail"].Rows.Count; k++) {
                        if (k % 2 == 0)
                            contentStr.Append("<tr>");
                        else
                            contentStr.Append("<tr style='background-color: #E6E6E6;'>");
                        contentStr.Append("<td>" +
                                       Convert.ToDateTime(dset.Tables["Detail"].Rows[k]["DateTime"].ToString())
                                           .ToString("MMM dd, yyyy") + "</td>");
                        contentStr.Append("<td>" +
                                       Convert.ToDateTime(dset.Tables["Detail"].Rows[k]["DateTime"].ToString())
                                           .ToString("HH:mm") + "</td>");
                        contentStr.Append("<td>" + dset.Tables["SampleInfo"].Rows[a]["Name"] + "</td>");

                        contentStr.Append("<td>" + dset.Tables["SampleInfo"].Rows[a]["EntityName"] + "</td>");

                        if (entityid == "0" || entityid == "4") {
                            contentStr.Append("<td>" + dset.Tables["Detail"].Rows[k]["CPU"] + "</td>");
                            if (flagContentProc) {
                                contentStr.Append("<td>N/A</td>"); //Object Name
                                contentStr.Append("<td>N/A</td>"); //Ancestor Process Name
                                contentStr.Append("<td>N/A</td>"); //Ancestor Program Name
                                contentStr.Append("<td>N/A</td>"); //Duration
                            }
                        } else if (entityid == "1") {
                            if (dset.Tables["Detail"].Rows[k]["ProcessName"].ToString() == "??") {
                                contentStr.Append("<td>N/A</td>");
                            } else {
                                contentStr.Append("<td>" + dset.Tables["Detail"].Rows[k]["ProcessName"] + "</td>");
                            }
                            //Object Name.
                            contentStr.Append("<td>" + dset.Tables["Detail"].Rows[k]["ProgramName"] + "</td>");
                            if (dset.Tables["Detail"].Rows[k]["AncestorProcessName"].ToString().Length == 0 ||
                                dset.Tables["Detail"].Rows[k]["AncestorProcessName"] == DBNull.Value) {
                                contentStr.Append("<td>N/A</td>");
                            } else {
                                contentStr.Append("<td>" + dset.Tables["Detail"].Rows[k]["AncestorProcessName"] + "</td>");
                            }
                            //Object Name.
                            if (dset.Tables["Detail"].Rows[k]["AncestorProgramName"].ToString().Length == 0 ||
                                dset.Tables["Detail"].Rows[k]["AncestorProgramName"] == DBNull.Value) {
                                contentStr.Append("<td>N/A</td>");
                            } else {
                                contentStr.Append("<td>" + dset.Tables["Detail"].Rows[k]["AncestorProgramName"] + "</td>");
                            }
                            //Duration.
                            if (dset.Tables["Detail"].Rows[k]["Duration"].ToString().Length == 0 ||
                                dset.Tables["Detail"].Rows[k]["Duration"] == DBNull.Value) {
                                contentStr.Append("<td>N/A</td>");
                            } else {
                                contentStr.Append("<td>" + dset.Tables["Detail"].Rows[k]["Duration"] + "</td>");
                            }
                        } else if (entityid == "2") {
                            contentStr.Append("<td>" + dset.Tables["Detail"].Rows[k]["Disc"] + "</td>");
                            if (procFlag) {
                                contentStr.Append("<td>N/A</td>"); //Object Name
                                contentStr.Append("<td>N/A</td>"); //Ancestor Process Name
                                contentStr.Append("<td>N/A</td>"); //Ancestor Program Name
                                contentStr.Append("<td>N/A</td>"); //Duration
                            }
                        } else if (entityid == "3") {
                            contentStr.Append("<td>" + dset.Tables["Detail"].Rows[k]["DiskFile"] + "</td>");

                            if (procFlag) {
                                contentStr.Append("<td>N/A</td>"); //Object Name
                                contentStr.Append("<td>N/A</td>"); //Ancestor Process Name
                                contentStr.Append("<td>N/A</td>"); //Ancestor Program Name
                                contentStr.Append("<td>N/A</td>"); //Duration
                            }
                        }
                        contentStr.Append("<td>" + dset.Tables["SampleInfo"].Rows[a]["CounterName"] + "</td>");

                        contentStr.Append("<td style='text-align:right;'>" +
                                      String.Format("{0:#,##0}", dset.Tables["Detail"].Rows[k][dset.Tables["SampleInfo"].Rows[a]["CounterField"].ToString()]) + "</td>");
                        contentStr.Append("</tr>");
                    } //
                    dset.Tables["Detail"].Clear();
                }

                var headerStr = new StringBuilder();

                headerStr.Append("<a name=severity" + i + "></a>");
                var newHeading = @"<table style='margin-bottom: 23px;background-color:#ffffff;border-radius: 3px;border: #cccccc 1px solid; width:100%;' cellpadding = 0 cellspacing = 0>
	                                <tr ><td style='padding:10px 15px;color:#212121;background-color:#B7B7B7;border-color:#dddddd;'>
                                    <h3 style='margin-top: 0;margin-bottom: 0;font-size: 15px;font-family: Calibri;color: inherit;'>
                                    " + heading + @"</h3>
	                                </td></tr><tr><td style='padding: 15px; font-size: 12px;padding-left:50px;'>";

                headerStr.Append(newHeading + "<div><table class=main CellPadding=2 CellSpacing=0 Border=1 style='FONT-SIZE: 7pt; FONT-FAMILY: Calibri; '> ");
                headerStr.Append(
                    "<tr style=\"background-color:LightGrey;border-color:Black;border-width:1px;border-style:solid;\">\n");
                headerStr.Append(
                    "<th align=\"center\" scope=\"col\" abbr=\"date\" style=\"text-decoration:underline;white-space:nowrap;\">Date</th>");
                headerStr.Append(
                    "<th align=\"center\" scope=\"col\" abbr=\"time\" style=\"text-decoration:underline;white-space:nowrap;\">Time</th>");
                headerStr.Append(
                    "<th align=\"center\" scope=\"col\" abbr=\"name\" style=\"text-decoration:underline;white-space:nowrap;\">Name</th>");
                headerStr.Append(
                    "<th align=\"center\" scope=\"col\" abbr=\"entity\" style=\"text-decoration:underline;white-space:nowrap;\">Entity</th>");
                headerStr.Append(
                    "<th align=\"center\" scope=\"col\" abbr=\"object\" style=\"text-decoration:underline;white-space:nowrap;w\">Object</th>");
                if (flagContentProc) {
                    headerStr.Append(
                        "<th align=\"center\" scope=\"col\" abbr=\"objectName\" style=\"text-decoration:underline;white-space:nowrap;\">Object Name</th>");
                    headerStr.Append(
                        "<th align=\"center\" scope=\"col\" abbr=\"ancestor\" style=\"text-decoration:underline;white-space:nowrap;\">Ancestor</th>");
                    headerStr.Append(
                        "<th align=\"center\" scope=\"col\" abbr=\"ancestorName\" style=\"text-decoration:underline;white-space:nowrap;\">Ancestor Name</th>");
                    headerStr.Append(
                        "<th align=\"center\" scope=\"col\" abbr=\"duration\" style=\"text-decoration:underline;white-space:nowrap;\">Duration</th>");
                    flagContentProc = false;
                }
                headerStr.Append(
                    "<th align=\"center\" scope=\"col\" abbr=\"counter\" style=\"text-decoration:underline;white-space:nowrap;\">Counter</th>");
                headerStr.Append(
                    "<th align=\"center\" scope=\"col\" abbr=\"value\" style=\"text-decoration:underline;white-space:nowrap;\">Value</th>");
                headerStr.Append("</tr>\n");

                if (flagContent) {
                    returnStr.Append(headerStr);
                    returnStr.Append(contentStr);
                    returnStr.Append("</table></div></td><tr></table>");
                    returnStr.Append(
                        "<div style=\"text-align:right;FONT-SIZE: 7pt; FONT-FAMILY: Calibri;width:850px;\"><a href='#top'><b><u>Back to top</u></b></a></div><br>\n");
                    flagContent = false;
                }
                dset.Tables["SampleInfo"].Clear();
            }

            #endregion

            dconn.Close();
            dconnTrend.Close();
            return returnStr.ToString();
        }

        public bool CheckAlert(DateTime fromTime, DateTime toTime, string systemSerial, int customerID, int severity) {
            var dataSet = GetRAMProfileRuleAlerts(systemSerial, customerID, severity);
            if (dataSet.Tables["Alerts"] == null || dataSet.Tables["Alerts"].Rows.Count == 0) {
                return false;
            }
            bool isExists = false;
            var cmdText = new StringBuilder();
            cmdText.Append("SELECT AlertID FROM ( ");
            cmdText.Append(" SELECT AlertID, DateTime, System FROM CPUAlerts ");
            cmdText.Append(" UNION ALL SELECT AlertID, DateTime, System FROM DiskFileAlerts ");
            cmdText.Append(" UNION ALL SELECT AlertID, DateTime, System FROM DiskAlerts ");
            cmdText.Append(" UNION ALL SELECT AlertID, DateTime, System FROM ProcessAlerts ");
            cmdText.Append(" UNION ALL SELECT AlertID, DateTime, System FROM TMFAlerts) AS A ");
            cmdText.Append(" WHERE DateTime BETWEEN @FromTime AND @ToTime ");
            cmdText.Append(" AND System = @SystemSerial ");
            cmdText.Append(" AND AlertID IN ( ");
            for(int rowIndex = 0; rowIndex < dataSet.Tables["Alerts"].Rows.Count; rowIndex++) {
                var tableRow = dataSet.Tables["Alerts"].Rows[rowIndex];
                if(rowIndex != 0) cmdText.Append(" , ");
                cmdText.Append(tableRow["AlertID"]);
            }                                
            cmdText.Append(" ) GROUP BY AlertID) ");
            using (var connection = new MySqlConnection(_connectionStringSystem)) {
                var command = new MySqlCommand(cmdText.ToString(), connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@FromTime", fromTime);
                command.Parameters.AddWithValue("@ToTime", toTime);
                command.Parameters.AddWithValue("@Severity", severity);
                command.Parameters.AddWithValue("@CustomerID", customerID);

                connection.Open();
                var reader = command.ExecuteReader();
                if (reader.Read())
                    isExists = true;
            }
            return isExists;
        }

        private DataSet GetRAMProfileRuleAlerts(string systemSerial, int customerID, int severity) {
            string cmdText = @"SELECT AlertID FROM RAM_ProfileRule
                                        WHERE SystemSerialNum = @SystemSerial AND
                                        Severity = @Severity
                                        AND ((Scope IS NULL OR Scope = '0') OR (Scope = '1' AND CreatorID = @CustomerID))";
            var dataSet = new DataSet();
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@Severity", severity);
                command.Parameters.AddWithValue("@CustomerID", customerID);

                connection.Open();
                var dadapt = new MySqlDataAdapter { SelectCommand = command };
                dadapt.Fill(dataSet, "Alerts");
            }
            return dataSet;
        }
    }
}