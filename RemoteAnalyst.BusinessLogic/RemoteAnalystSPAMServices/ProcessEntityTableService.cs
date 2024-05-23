using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices {
    public class ProcessEntityTableService {
        private readonly string _connectionString;

        public ProcessEntityTableService(string connectionString) {
            _connectionString = connectionString;
        }

        public List<string> GetTableNames(string systemSerial, DateTime startTime, DateTime stopTime) {
            var databaseCheck = new Database(_connectionString);
            var databaseName = Helper.FindKeyName(_connectionString, "DATABASE");

            var processTableNames = new List<string>();
            for (var start = startTime.Date; start <= stopTime.Date; start = start.AddDays(1)) {
                var cpuTableName = systemSerial + "_PROCESS_" + start.Year + "_" + start.Month + "_" + start.Day;
                var exists = databaseCheck.CheckTableExists(cpuTableName, databaseName);

                if (exists)
                    processTableNames.Add(cpuTableName);
            }

            return processTableNames;
        }

        private List<string> GetDISCTableNames(string systemSerial, DateTime startTime, DateTime stopTime) {
            var databaseCheck = new Database(_connectionString);
            var databaseName = Helper.FindKeyName(_connectionString, "DATABASE");

            var processTableNames = new List<string>();
            for (var start = startTime.Date; start <= stopTime.Date; start = start.AddDays(1)) {
                var cpuTableName = systemSerial + "_DISC_" + start.Year + "_" + start.Month + "_" + start.Day;
                var exists = databaseCheck.CheckTableExists(cpuTableName, databaseName);

                if (exists)
                    processTableNames.Add(cpuTableName);
            }

            return processTableNames;
        }

        public DataTable GetAllProcessByBusy(string systemSerial, DateTime startTime, DateTime stopTime, long interval) {
            var processTalbeNames = GetTableNames(systemSerial, startTime, stopTime);

            var processEntityTable = new ProcessEntityTable(_connectionString);
            var processData = processEntityTable.GetAllProcessByBusy(processTalbeNames, startTime, stopTime);

            return processData;
        }

        public Dictionary<DateTime, double> GetPeakProcessByBusy(DataTable processData, DateTime startTime, DateTime stopTime, long interval) {
            var processDataDic = new Dictionary<DateTime, double>();

            for (DateTime start = startTime; start < stopTime; start = start.AddSeconds(interval)) {
                if (processData.AsEnumerable().Any(x => x.Field<DateTime>("FromTimestamp") >= start && x.Field<DateTime>("FromTimestamp") < start.AddSeconds(interval))) {
                    var maxValue = processData.AsEnumerable().Where(x => x.Field<DateTime>("FromTimestamp") >= start && x.Field<DateTime>("FromTimestamp") < start.AddSeconds(interval))
                        .Max(x => x.Field<double>("Busy %"));
                    processDataDic.Add(start, maxValue);
                }
            }

            return processDataDic;

        }
        
        public Dictionary<DateTime, double> GetPeakProcessQueueByBusy(DataTable processData, DateTime startTime, DateTime stopTime, long interval) {
            var processDataDic = new Dictionary<DateTime, double>();

            for (DateTime start = startTime; start < stopTime; start = start.AddSeconds(interval)) {
                if (processData.AsEnumerable().Any(x => x.Field<DateTime>("FromTimestamp") >= start && x.Field<DateTime>("FromTimestamp") < start.AddSeconds(interval))) {
                    var maxValue = processData.AsEnumerable().Where(x => x.Field<DateTime>("FromTimestamp") >= start && x.Field<DateTime>("FromTimestamp") < start.AddSeconds(interval))
                        .Max(x => x.Field<double>("ReceiveQueue"));
                    processDataDic.Add(start, maxValue);
                }
            }

            return processDataDic;
        }

        public Dictionary<DateTime, ProcessTransaction> GetProcessAbort(DataTable processData, DateTime startTime, DateTime stopTime, long interval) {
            var processDataDic = new Dictionary<DateTime, ProcessTransaction>();

            for (DateTime start = startTime; start < stopTime; start = start.AddSeconds(interval)) {
                if (processData.AsEnumerable().Any(x => x.Field<DateTime>("FromTimestamp") >= start && x.Field<DateTime>("FromTimestamp") < start.AddSeconds(interval))) {
                    var abort = processData.AsEnumerable().Where(x => x.Field<DateTime>("FromTimestamp") >= start && x.Field<DateTime>("FromTimestamp") < start.AddSeconds(interval))
                        .Sum(x => x.Field<double>("AbortTrans"));
                    var begin = processData.AsEnumerable().Where(x => x.Field<DateTime>("FromTimestamp") >= start && x.Field<DateTime>("FromTimestamp") < start.AddSeconds(interval))
                        .Sum(x => x.Field<double>("BeginTrans"));

                    processDataDic.Add(start, new ProcessTransaction {
                        AbortTrans = abort,
                        BeginTrans = begin
                    });
                }
            }

            return processDataDic;
        }

        public List<string> GetDP2ProcessesFor(string systemSerial, DateTime startTime, DateTime stopTime) {
            var discTalbeNames = GetDISCTableNames(systemSerial, startTime, stopTime);
            var discEntityTable = new DISCEntityTable(_connectionString);
            var discNames = new List<string>();
            foreach (var discTalbeName in discTalbeNames) {
                discNames.AddRange(discEntityTable.GetDeviceNames(discTalbeName));
            }

            return discNames.Distinct().ToList();
        }

        public bool CheckIPUFor(string systemSerial, DateTime startTime, DateTime stopTime, string databasePrefix) {
            var processTalbeNames = GetTableNames(systemSerial, startTime, stopTime);

            var processEntityTable = new ProcessEntityTable(_connectionString);
            var ipuCount = processEntityTable.CheckIPUColumn(processTalbeNames.First(), databasePrefix + systemSerial);

            return ipuCount > 0;
        }

        public string GetTop20ProcessByBusyStaticFor(string systemSerial, DateTime startTime, DateTime stopTime, int pageSizeBytes, long interval, List<string> discNames, bool isIPU, bool isLocalAnalyst, string monthDayPattern, string websiteAddress) {
            var processTalbeNames = GetTableNames(systemSerial, startTime, stopTime);
            
            var processEntityTable = new ProcessEntityTable(_connectionString);
            var dailiesTopProcesses = new DailiesTopProcessRepository(_connectionString);

            var processData = new DataTable();
            processData = dailiesTopProcesses.GetProcessBusyData(startTime, stopTime);

            if(processData.Rows.Count == 0)
                processData = processEntityTable.GetTop20ProcessByBusyStatic(processTalbeNames, startTime, stopTime, pageSizeBytes, interval, isIPU);

            var process = new StringBuilder();

            if (processData.Rows.Count > 0) {
                process.Append("<table CellPadding=2 CellSpacing=0 Border=1 style='FONT-SIZE: 7pt; FONT-FAMILY: Calibri;float:left;'> ");
                process.Append("<tr style=\"background-color:LightGrey;border-color:Black;border-width:1px;border-style:solid;\">\n");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Process</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Busy %</th>");
                if(isIPU)
                    process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">CPU:IPU</th>");
                else
                    process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">CPU</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Program</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Owner</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Ancestor</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Priority</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Receive Queue</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Memory</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Interval period</th>");
                process.Append("</tr>\n");

                var rowIndex = 0;
                var encrypt = new Decrypt();
                var encryptSystemSerial = encrypt.strDESEncrypt(systemSerial);

                foreach (DataRow row in processData.Rows) {
                    if (rowIndex % 2 == 0)
                        process.Append("<tr>\n");
                    else
                        process.Append("<tr style='background-color: #E6E6E6;'>\n");

                    var processName = row["ProcessName"].ToString();
                    if (processName.Length.Equals(0))
                        processName = "N/A";
                    var cpuNum = row["CpuNum"].ToString();
                    var pin = row["PIN"].ToString();
                    var programFileName = row["Program"].ToString();

                    var encryptStartTime = encrypt.strDESEncrypt(row["FromTimestamp"].ToString());

                    if(!isLocalAnalyst)
                        process.Append("<td align=\"left\" >" + "<a href='" + websiteAddress + "/EmailLogin.aspx?User=@User&System=" + encryptSystemSerial + "&StartDate=" + encryptStartTime +  "&ProcessName=" + processName + "&CpuNum=" + cpuNum + "&Pin=" + pin + "&programFileName=" + programFileName + "'>" + processName + "</a></td>\n");
                    else
                        process.Append("<td align=\"left\" >" + "<a href='" + websiteAddress + "/EmailLogin.aspx?User=@User&System=" + encryptSystemSerial + "&StartDate=" + encryptStartTime + "&ProcessName=" + processName + "&CpuNum=" + cpuNum + "&Pin=" + pin + "&programFileName=" + programFileName + "'>" + processName + "</a></td>\n");

                    if (Math.Round(Convert.ToDouble(row["Busy %"]), 2) > 79 && Math.Round(Convert.ToDouble(row["Busy %"]), 2) < 90) {
                        process.Append("<td align=\"right\" style='background-color: yellow;'>" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["Busy %"]), 2)) + "</td>\n");
                    }
                    else if (Math.Round(Convert.ToDouble(row["Busy %"]), 2) >= 90) {
                        process.Append("<td align=\"right\" style='background-color: red;'>" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["Busy %"]), 2)) + "</td>\n");
                    }
                    else
                        process.Append("<td align=\"right\">" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["Busy %"]), 2)) + "</td>\n");

                    if (isIPU)
                        process.Append("<td align=\"right\">" + Convert.ToInt32(row["CpuNum"]).ToString("D2") + ":" + Convert.ToInt32(row["IPUNum"]).ToString("D2") + "</td>\n");
                    else
                        process.Append("<td align=\"right\">" + Convert.ToInt32(row["CpuNum"]).ToString("D2") + "</td>\n");

                    process.Append("<td align=\"left\">" + row["Program"] + "</td>\n");
                    process.Append("<td align=\"right\">" + Convert.ToInt32(row["Group"]).ToString("D3") + "," + Convert.ToInt32(row["User"]).ToString("D3") + "</td>\n");
                    process.Append("<td align=\"left\">" + row["AncestorProcessName"] + "</td>\n");
                    process.Append("<td align=\"right\">" + row["Priority"] + "</td>\n");
                    process.Append("<td align=\"right\">" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["ReceiveQueue"]), 2)) + "</td>\n");
                    if (Convert.ToDouble(row["MemUsed"]) > 250 && !discNames.Contains(row["ProcessName"]))
                        process.Append("<td align=\"right\" style='background-color: yellow;'>" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["MemUsed"]), 2)) + "</td>\n");
                    else
                        process.Append("<td align=\"right\">" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["MemUsed"]), 2)) + "</td>\n");

                    process.Append("<td align=\"left\">" + Convert.ToDateTime(row["FromTimestamp"]).ToString(monthDayPattern + " HH:mm") + "</td>\n");
                    process.Append("</tr>\n");
                    rowIndex++;
                }
                process.Append("</table>");
            }

            return process.ToString();
        }

        public string GetTop20ProcessByBusyDynamicFor(string systemSerial, DateTime startTime, DateTime stopTime, int pageSizeBytes, long interval, List<string> discNames, bool isIPU, bool isLocalAnalyst, string monthDayPattern, string websiteAddress) {
            var processTalbeNames = GetTableNames(systemSerial, startTime, stopTime);

            var processEntityTable = new ProcessEntityTable(_connectionString);
            var processData = processEntityTable.GetTop20ProcessByBusyDynamic(processTalbeNames, startTime, stopTime, pageSizeBytes, interval, isIPU);

            var process = new StringBuilder();

            if (processData.Rows.Count > 0) {
                process.Append("<table CellPadding=2 CellSpacing=0 Border=1 style='FONT-SIZE: 7pt; FONT-FAMILY: Calibri;float:left; padding-left:10px;'> ");
                process.Append("<tr style=\"background-color:LightGrey;border-color:Black;border-width:1px;border-style:solid;\">\n");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Process</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Busy %</th>");
                if(isIPU)
                    process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">CPU:IPU</th>");
                else
                    process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">CPU</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Program</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Owner</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Ancestor</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Priority</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Receive Queue</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Memory</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Interval period</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Duration</th>");
                process.Append("</tr>\n");

                var rowIndex = 0;
                var encrypt = new Decrypt();
                var encryptSystemSerial = encrypt.strDESEncrypt(systemSerial);

                foreach (DataRow row in processData.Rows) {
                    var duration = Convert.ToDateTime(row["ToTimestamp"]) - Convert.ToDateTime(row["FromTimestamp"]);

                    if (duration.TotalSeconds >= 5) {
                        if (rowIndex%2 == 0)
                            process.Append("<tr>\n");
                        else
                            process.Append("<tr style='background-color: #E6E6E6;'>\n");
                        var processName = row["ProcessName"].ToString();
                        if (processName.Length.Equals(0))
                            processName = "N/A";
                        var cpuNum = row["CpuNum"].ToString();
                        var pin = row["PIN"].ToString();
                        var programFileName = row["Program"].ToString();

                        var encryptStartTime = encrypt.strDESEncrypt(row["FromTimestamp"].ToString());

                        if(!isLocalAnalyst)
                            process.Append("<td align=\"left\" >" + "<a href='" + websiteAddress + "/EmailLogin.aspx?User=@User&System=" + encryptSystemSerial + "&StartDate=" + encryptStartTime + "&ProcessName=" + processName + "&CpuNum=" + cpuNum + "&Pin=" + pin + "&programFileName=" + programFileName + "'>" + processName + "</a></td>\n");
                        else
                            process.Append("<td align=\"left\" >" + "<a href='" + websiteAddress + "/EmailLogin.aspx?User=@User&System=" + encryptSystemSerial + "&StartDate=" + encryptStartTime + "&ProcessName=" + processName + "&CpuNum=" + cpuNum + "&Pin=" + pin + "&programFileName=" + programFileName + "'>" + processName + "</a></td>\n");

                        if (Math.Round(Convert.ToDouble(row["Busy %"]), 2) > 79 && Math.Round(Convert.ToDouble(row["Busy %"]), 2) < 90) {
                            process.Append("<td align=\"right\" style='background-color: yellow;'>" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["Busy %"]), 2)) + "</td>\n");
                        }
                        else if (Math.Round(Convert.ToDouble(row["Busy %"]), 2) >= 90) {
                            process.Append("<td align=\"right\" style='background-color: red;'>" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["Busy %"]), 2)) + "</td>\n");
                        }
                        else {
                            process.Append("<td align=\"right\">" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["Busy %"]), 2)) + "</td>\n");
                        }

                        if (isIPU)
                            process.Append("<td align=\"right\">" + Convert.ToInt32(row["CpuNum"]).ToString("D2") + ":" + Convert.ToInt32(row["IPUNum"]).ToString("D2") + "</td>\n");
                        else
                            process.Append("<td align=\"right\">" + Convert.ToInt32(row["CpuNum"]).ToString("D2") + "</td>\n");

                        process.Append("<td align=\"left\">" + row["Program"] + "</td>\n");
                        process.Append("<td align=\"right\">" + Convert.ToInt32(row["Group"]).ToString("D3") + "," + Convert.ToInt32(row["User"]).ToString("D3") + "</td>\n");
                        process.Append("<td align=\"left\">" + row["AncestorProcessName"] + "</td>\n");
                        process.Append("<td align=\"right\">" + row["Priority"] + "</td>\n");
                        process.Append("<td align=\"right\">" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["ReceiveQueue"]), 2)) + "</td>\n");
                        if (Convert.ToDouble(row["MemUsed"]) > 250 && !discNames.Contains(row["ProcessName"]))
                            process.Append("<td align=\"right\" style='background-color: yellow;'>" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["MemUsed"]), 2)) + "</td>\n");
                        else
                            process.Append("<td align=\"right\">" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["MemUsed"]), 2)) + "</td>\n");
                        process.Append("<td align=\"left\">" + Convert.ToDateTime(row["FromTimestamp"]).ToString(monthDayPattern + " HH:mm") + "</td>\n");

                        var minutes = Convert.ToInt32(duration.TotalSeconds/60);
                        var seconds = Convert.ToInt32(duration.TotalSeconds%60);
                        process.Append("<td align=\"right\">" + minutes.ToString("D2") + ":" + seconds.ToString("D2") + "</td>\n");
                        process.Append("</tr>\n");
                        rowIndex++;
                    }
                }
                process.Append("</table>");
            }

            return process.ToString();
        }

        public string GetTop20ProcessByQueueStaticFor(string systemSerial, DateTime startTime, DateTime stopTime, int pageSizeBytes, long interval, List<string> discNames, bool isIPU, bool isLocalAnalyst, string monthDayPattern, string websiteAddress) {
            var processTalbeNames = GetTableNames(systemSerial, startTime, stopTime);

            var processEntityTable = new ProcessEntityTable(_connectionString);
            var processData = new DataTable();
            var dailiesTopProcesses = new DailiesTopProcessRepository(_connectionString);
            processData = dailiesTopProcesses.GetProcessQueueData(startTime, stopTime);

            if(processData.Rows.Count == 0)
                processData = processEntityTable.GetTop20ProcessByQueueStatic(processTalbeNames, startTime, stopTime, pageSizeBytes, interval, isIPU);

            var process = new StringBuilder();

            if (processData.Rows.Count > 0) {
                process.Append("<div><table class=main CellPadding=2 CellSpacing=0 Border=1 style='FONT-SIZE: 7pt; FONT-FAMILY: Calibri; '> ");
                process.Append("<tr style=\"background-color:LightGrey;border-color:Black;border-width:1px;border-style:solid;\">\n");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Process</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Receive<br>Queue</th>");
                if(isIPU)
                    process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">CPU:IPU</th>");
                else
                    process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">CPU</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Program</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Owner</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Ancestor</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Priority</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Busy %</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Memory</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Interval period</th>");
                process.Append("</tr>\n");

                var rowIndex = 0;
                var encrypt = new Decrypt();
                var encryptSystemSerial = encrypt.strDESEncrypt(systemSerial);

                foreach (DataRow row in processData.Rows) {
                    if (rowIndex % 2 == 0)
                        process.Append("<tr>\n");
                    else
                        process.Append("<tr style='background-color: #E6E6E6;'>\n");
                    //process.Append("<tr>\n");
                    var processName = row["ProcessName"].ToString();
                    if (processName.Length.Equals(0))
                        processName = "N/A";
                    var cpuNum = row["CpuNum"].ToString();
                    var pin = row["PIN"].ToString();
                    var programFileName = row["Program"].ToString();

                    var encryptStartTime = encrypt.strDESEncrypt(row["FromTimestamp"].ToString());

                    if(!isLocalAnalyst)
                        process.Append("<td align=\"left\">" + "<a href='" + websiteAddress + "/EmailLogin.aspx?User=@User&System=" + encryptSystemSerial + "&StartDate=" + encryptStartTime + "&ProcessName=" + processName + "&CpuNum=" + cpuNum + "&Pin=" + pin + "&programFileName=" + programFileName + "'>" + processName + "</a></td>\n");
                    else
                        process.Append("<td align=\"left\">" + "<a href='" + websiteAddress + "/EmailLogin.aspx?User=@User&System=" + encryptSystemSerial + "&StartDate=" + encryptStartTime + "&ProcessName=" + processName + "&CpuNum=" + cpuNum + "&Pin=" + pin + "&programFileName=" + programFileName + "'>" + processName + "</a></td>\n");
                    process.Append("<td align=\"right\">" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["ReceiveQueue"]), 2)) + "</td>\n");
                    if (isIPU)
                        process.Append("<td align=\"right\">" + Convert.ToInt32(row["CpuNum"]).ToString("D2") + ":" + Convert.ToInt32(row["IPUNum"]).ToString("D2") + "</td>\n");
                    else
                        process.Append("<td align=\"right\">" + Convert.ToInt32(row["CpuNum"]).ToString("D2") + "</td>\n");

                    process.Append("<td align=\"left\">" + row["Program"] + "</td>\n");
                    process.Append("<td align=\"right\">" + Convert.ToInt32(row["Group"]).ToString("D3") + "," + Convert.ToInt32(row["User"]).ToString("D3") + "</td>\n");
                    process.Append("<td align=\"left\">" + row["AncestorProcessName"] + "</td>\n");
                    process.Append("<td align=\"right\">" + row["Priority"] + "</td>\n");
                    process.Append("<td align=\"right\">" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["Busy %"]), 2)) + "</td>\n");
                    if (Convert.ToDouble(row["MemUsed"]) > 250 && !discNames.Contains(row["ProcessName"]))
                        process.Append("<td align=\"right\" style='background-color: yellow;'>" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["MemUsed"]), 2)) + "</td>\n");
                    else
                        process.Append("<td align=\"right\">" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["MemUsed"]), 2)) + "</td>\n");

                    process.Append("<td align=\"left\">" + Convert.ToDateTime(row["FromTimestamp"]).ToString(monthDayPattern + " HH:mm") + "</td>\n");
                    process.Append("</tr>\n");
                    rowIndex++;
                }
                process.Append("</table></div><br>");
            }

            return process.ToString();
        }

        public string GetTop20ProcessByQueueDynamicFor(string systemSerial, DateTime startTime, DateTime stopTime, int pageSizeBytes, long interval, List<string> discNames, bool isIPU, bool isLocalAnalyst, string monthDayPattern, string websiteAddress) {
            var processTalbeNames = GetTableNames(systemSerial, startTime, stopTime);

            var processEntityTable = new ProcessEntityTable(_connectionString);
            var processData = processEntityTable.GetTop20ProcessByQueueDynamic(processTalbeNames, startTime, stopTime, pageSizeBytes, interval, isIPU);

            var process = new StringBuilder();

            if (processData.Rows.Count > 0) {
                process.Append("<div><table class=main CellPadding=2 CellSpacing=0 Border=1 style='FONT-SIZE: 7pt; FONT-FAMILY: Calibri; '> ");
                process.Append("<tr style=\"background-color:LightGrey;border-color:Black;border-width:1px;border-style:solid;\">\n");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Process</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Receive<br>Queue</th>");
                if (isIPU)
                    process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">CPU:IPU</th>");
                else
                    process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">CPU</th>");

                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Program</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Owner</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Ancestor</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Priority</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Busy %</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Memory</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Interval period</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Duration</th>");
                process.Append("</tr>\n");

                var rowIndex = 0;
                var encrypt = new Decrypt();
                var encryptSystemSerial = encrypt.strDESEncrypt(systemSerial);

                foreach (DataRow row in processData.Rows) {
                    var duration = Convert.ToDateTime(row["ToTimestamp"]) - Convert.ToDateTime(row["FromTimestamp"]);

                    if (duration.TotalSeconds >= 5) {
                        if (rowIndex%2 == 0)
                            process.Append("<tr>\n");
                        else
                            process.Append("<tr style='background-color: #E6E6E6;'>\n");
                        //process.Append("<tr>\n");
                        var processName = row["ProcessName"].ToString();
                        if (processName.Length.Equals(0))
                            processName = "N/A";
                        var cpuNum = row["CpuNum"].ToString();
                        var pin = row["PIN"].ToString();
                        var programFileName = row["Program"].ToString();

                        var encryptStartTime = encrypt.strDESEncrypt(row["FromTimestamp"].ToString());

                        if(!isLocalAnalyst)
                            process.Append("<td align=\"left\">" + "<a href='" + websiteAddress + "/EmailLogin.aspx?User=@User&System=" + encryptSystemSerial + "&StartDate=" + encryptStartTime + "&ProcessName=" + processName + "&CpuNum=" + cpuNum + "&Pin=" + pin + "&programFileName=" + programFileName + "'>" + processName + "</a></td>\n");
                        else
                            process.Append("<td align=\"left\">" + "<a href='" + websiteAddress + "/EmailLogin.aspx?User=@User&System=" + encryptSystemSerial + "&StartDate=" + encryptStartTime + "&ProcessName=" + processName + "&CpuNum=" + cpuNum + "&Pin=" + pin + "&programFileName=" + programFileName + "'>" + processName + "</a></td>\n");

                        process.Append("<td align=\"right\">" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["ReceiveQueue"]), 2)) + "</td>\n");
                        if (isIPU)
                            process.Append("<td align=\"right\">" + Convert.ToInt32(row["CpuNum"]).ToString("D2") + ":" + Convert.ToInt32(row["IPUNum"]).ToString("D2") + "</td>\n");
                        else
                            process.Append("<td align=\"right\">" + Convert.ToInt32(row["CpuNum"]).ToString("D2") + "</td>\n");

                        process.Append("<td align=\"left\">" + row["Program"] + "</td>\n");
                        process.Append("<td align=\"right\">" + Convert.ToInt32(row["Group"]).ToString("D3") + "," + Convert.ToInt32(row["User"]).ToString("D3") + "</td>\n");
                        process.Append("<td align=\"left\">" + row["AncestorProcessName"] + "</td>\n");
                        process.Append("<td align=\"right\">" + row["Priority"] + "</td>\n");
                        process.Append("<td align=\"right\">" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["Busy %"]), 2)) + "</td>\n");
                        if (Convert.ToDouble(row["MemUsed"]) > 250 && !discNames.Contains(row["ProcessName"]))
                            process.Append("<td align=\"right\" style='background-color: yellow;'>" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["MemUsed"]), 2)) + "</td>\n");
                        else
                            process.Append("<td align=\"right\">" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["MemUsed"]), 2)) + "</td>\n");

                        process.Append("<td align=\"left\">" + Convert.ToDateTime(row["FromTimestamp"]).ToString(monthDayPattern + " HH:mm") + "</td>\n");
                        var minutes = Convert.ToInt32(duration.TotalSeconds/60);
                        var seconds = Convert.ToInt32(duration.TotalSeconds%60);
                        process.Append("<td align=\"right\">" + minutes.ToString("D2") + ":" + seconds.ToString("D2") + "</td>\n");
                        process.Append("</tr>\n");
                        rowIndex++;
                    }
                }
                process.Append("</table></div><br>");
            }

            return process.ToString();
        }

        public string GetTop20ProcessByAbortFor(string systemSerial, DateTime startTime, DateTime stopTime, int pageSizeBytes, bool isIPU, bool isLocalAnalyst, string monthDayPattern, string websiteAddress) {
            var processTalbeNames = GetTableNames(systemSerial, startTime, stopTime);

            var processEntityTable = new ProcessEntityTable(_connectionString);
            var processData = processEntityTable.GetTop20ProcessByAbort(processTalbeNames, startTime, stopTime, pageSizeBytes, isIPU);
            var process = new StringBuilder();

            if (processData.Rows.Count > 0) {
                process.Append("<div><table class=main CellPadding=2 CellSpacing=0 Border=1 style='FONT-SIZE: 7pt; FONT-FAMILY: Calibri; '> ");
                process.Append("<tr style=\"background-color:LightGrey;border-color:Black;border-width:1px;border-style:solid;\">\n");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Process</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Abort %</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Begin / Sec</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Abort / Sec</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Busy %</th>");
                if (isIPU)
                    process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">CPU:IPU</th>");
                else
                    process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">CPU</th>");

                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Program</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Owner</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Ancestor</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Priority</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Receive Queue</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Memory</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Interval period</th>");
                process.Append("</tr>\n");

                var rowIndex = 0;
                var encrypt = new Decrypt();
                var encryptSystemSerial = encrypt.strDESEncrypt(systemSerial);

                foreach (DataRow row in processData.Rows) {
                    if (rowIndex % 2 == 0)
                        process.Append("<tr>\n");
                    else
                        process.Append("<tr style='background-color: #E6E6E6;'>\n");
                    //process.Append("<tr>\n");
                    var processName = row["ProcessName"].ToString();
                    if (processName.Length.Equals(0))
                        processName = "N/A";
                    var cpuNum = row["CpuNum"].ToString();
                    var pin = row["PIN"].ToString();
                    var programFileName = row["Program"].ToString();

                    var encryptStartTime = encrypt.strDESEncrypt(row["FromTimestamp"].ToString());

                    if(!isLocalAnalyst)
                        process.Append("<td align=\"left\">" + "<a href='" + websiteAddress + "/EmailLogin.aspx?User=@User&System=" + encryptSystemSerial + "&StartDate=" + encryptStartTime + "&ProcessName=" + processName + "&CpuNum=" + cpuNum + "&Pin=" + pin + "&programFileName=" + programFileName + "'>" + processName + "</a></td>\n");
                    else
                        process.Append("<td align=\"left\">" + "<a href='" + websiteAddress + "/EmailLogin.aspx?User=@User&System=" + encryptSystemSerial + "&StartDate=" + encryptStartTime + "&ProcessName=" + processName + "&CpuNum=" + cpuNum + "&Pin=" + pin + "&programFileName=" + programFileName + "'>" + processName + "</a></td>\n");


                    if (Convert.ToDouble(row["AbortTMF"]) > 100)
                        process.Append("<td align=\"right\">100.00</td>\n");
                    else
                        process.Append("<td align=\"right\">" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["AbortTMF"]), 2)) + "</td>\n");

                    process.Append("<td align=\"right\">" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["Begin / Sec"]), 2)) + "</td>\n");
                    process.Append("<td align=\"right\">" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["Abort / Sec"]), 2)) + "</td>\n");
                    process.Append("<td align=\"right\">" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["Busy %"]), 2)) + "</td>\n");
                    if (isIPU)
                        process.Append("<td align=\"right\">" + Convert.ToInt32(row["CpuNum"]).ToString("D2") + ":" + Convert.ToInt32(row["IPUNum"]).ToString("D2") + "</td>\n");
                    else
                        process.Append("<td align=\"right\">" + Convert.ToInt32(row["CpuNum"]).ToString("D2") + "</td>\n");

                    process.Append("<td align=\"left\">" + row["Program"] + "</td>\n");
                    process.Append("<td align=\"right\">" + Convert.ToInt32(row["Group"]).ToString("D3") + "," + Convert.ToInt32(row["User"]).ToString("D3") + "</td>\n");
                    process.Append("<td align=\"left\">" + row["AncestorProcessName"] + "</td>\n");
                    process.Append("<td align=\"right\">" + row["Priority"] + "</td>\n");
                    process.Append("<td align=\"right\">" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["ReceiveQueue"]), 2)) + "</td>\n");
                    process.Append("<td align=\"right\">" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["MemUsed"]), 2)) + "</td>\n");
                    process.Append("<td align=\"left\">" + Convert.ToDateTime(row["FromTimestamp"]).ToString(monthDayPattern + " HH:mm:ss") + "</td>\n");
                    process.Append("</tr>\n");
                    rowIndex++;
                }
                process.Append("</table></div><br>");
            }
            return process.ToString();
        }
    }
}
