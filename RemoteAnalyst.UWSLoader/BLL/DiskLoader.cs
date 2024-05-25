using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using System.IO;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.BusinessLogic.Util;
using System.Linq;
using log4net;

namespace RemoteAnalyst.UWSLoader.BLL {

    /// <summary>
    /// DiskLoader process the disk data file and load the data into database.
    /// </summary>
    internal class DiskLoader {
        private const string STR_SHORT_COMMAND_START = "Free Space Short Report";
        private const string STR_SHORT_COMMAND_END = "Disk Space Analysis Program";
        private const string STR_VIRTUAL_DISK_ID = "( Virtual Disk Volume )";
        private const string STR_UNAVAILABLE_DISK_ID = "( unavailable";
        private const int I_DISKNAME_INDEX = 0;
        private const int I_DISKNAME_LENGTH = 8;
        private const int I_MIRRORED_INDEX = 9;
        private const int I_CAPACITY_INDEX = 15;
        private const int I_USED_INDEX = 32;
        private const string STR_SUBDEVICE_START = "Volume ";
        private const string STR_SUBDEVICE_END = "subtype";
        private const int I_SUBDEVICE_OFFSET = 8;
        private const string STR_SUBVOL_START = "New Subvol Summary Report";
        private const string STR_SUBVOL_ALT_START = "Teraform Subvol Summary Report";
        private const string STR_USER_START = "New User Summary Report";
        private const string STR_USER_ALT_START = "Teraform User Summary Report";
        private const string STR_NO_FILES = "No files allocated";

        private readonly ILog _log;
        private readonly string strFileName = string.Empty;
        private readonly string systemSerial = string.Empty;
        private DateTime dtDataDate;
        private bool noFile;
        private bool returnValue;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="_fileName"> File location of the Disk data file.</param>
        /// <param name="writeToLog"></param>
        /// <param name="_systemSerial"> System serial number</param>
        /// <param name="_dtDataDate"> DateTime of this disk data.</param>
        public DiskLoader(string _fileName, ILog writeToLog, string _systemSerial, DateTime _dtDataDate) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            strFileName = _fileName;
            _log = writeToLog;
            systemSerial = _systemSerial;
            dtDataDate = _dtDataDate;
        }

        public static string RemovePassword(string connectionString)
        {
            try
            {
                if (String.IsNullOrEmpty(connectionString))
                {
                    return connectionString;
                }
                if ((connectionString.Contains("PASSWORD") && connectionString.Contains(";")) || (connectionString.Contains("password") && connectionString.Contains(";")))
                {
                    List<string> strlist = connectionString.Split(';').ToList();
                    for (int i = 0; i < strlist.Count; i++)
                    {
                        if (strlist[i].Contains("PASSWORD") || connectionString.Contains("password"))
                        {
                            strlist.Remove(strlist[i]);
                            break;
                        }
                    }
                    string concat = String.Join(";", strlist.ToArray());
                    return concat;
                }
                else
                {
                    return connectionString;
                }
            }
            catch (Exception e)
            {
                return connectionString;
            }
        }

        /// <summary>
        /// Process the data file and load the data into database.
        /// </summary>
        /// <returns>Bool value suggests whether the load is successful or not.</returns>
        public bool loadDiskData() {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            DataRow myDataRow;
            DataColumn myDataColumn;

            //Create SubVolAllocation DataTable
            var subVolAllocation = new DataTable();

            #region SubVolAllocation Columns

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "SA_SystemSerialNum";
            subVolAllocation.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "SA_DiskName";
            subVolAllocation.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "SA_SubVolName";
            subVolAllocation.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.Double");
            myDataColumn.ColumnName = "SA_UsedMB";
            subVolAllocation.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.Int32");
            myDataColumn.ColumnName = "SA_FileCount";
            subVolAllocation.Columns.Add(myDataColumn);

            #endregion

            //Create UserAllocation DataTable
            var userAllocation = new DataTable();

            #region User Allocation

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "UA_SystemSerialNum";
            userAllocation.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "UA_DiskName";
            userAllocation.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.Int16");
            myDataColumn.ColumnName = "UA_Group";
            userAllocation.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.Int16");
            myDataColumn.ColumnName = "UA_User";
            userAllocation.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.String");
            myDataColumn.ColumnName = "UA_UserName";
            userAllocation.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.Double");
            myDataColumn.ColumnName = "UA_UsedMB";
            userAllocation.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.Int32");
            myDataColumn.ColumnName = "UA_FileCount";
            userAllocation.Columns.Add(myDataColumn);

            #endregion

            System.Diagnostics.Process currentProc = System.Diagnostics.Process.GetCurrentProcess();
            _log.InfoFormat("Before loadDiskData Memory : {0}",currentProc.PrivateMemorySize64);
            

            //string connStr = ConnectionString.ConnectionStringSPAM;
            var databaseMappingService = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
            string connStr = databaseMappingService.GetConnectionStringFor(systemSerial);
            if (connStr.Length == 0) {
                connStr = ConnectionString.ConnectionStringSPAM;
            }
            _log.InfoFormat("Connection Strirng : {0}",DiskLoader.RemovePassword(connStr));
            
            string sqlStr = string.Empty;
            var dconn = new MySqlConnection(connStr);
            var dcomd = new MySqlCommand();
            MySqlDataReader dread;
            var alDisks = new List<Disk>();

            Disk disk = null;
            Disk nextDisk = null;

            try {
                dconn.Open();
                dcomd.Connection = dconn;
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using (var sr = new StreamReader(strFileName)) {
                    _log.Info("Start reading UWS");
                    

                    String line;
                    // Read and display lines from the file until the end of 
                    // the file is reached.

                    //Looping Through DSAP *, SHORT output and filling up ArrayList

                    //Proceed to line saying "Free Space Short Report", then skip 2 lines to start
                    while ((line = sr.ReadLine()) != null && line.IndexOf(STR_SHORT_COMMAND_START, 0, line.Length) != 0) {
                        //Skipping non-relevant lines
                    }

                    if (line.IndexOf(STR_SHORT_COMMAND_START, 0, STR_SHORT_COMMAND_START.Length) != 0) {
                        //_log.Info("DSAP Short Data absent");
                        //return false;
                    }

                    //Skipping two lines
                    sr.ReadLine();
                    sr.ReadLine();

                    string strDiskName;
                    bool bMirrored;
                    double capacityMB;
                    double freeMB;
                    double usedMB;
                    int iSpaceIndex;
                    int iEndSpaceIndex;

                    //If line begins with $ sign, it is a valid line
                    //Stop when first line without above condition or not starting with $ appears				

                    while ((line = sr.ReadLine()) != null) {
                        if (line.IndexOf(STR_SHORT_COMMAND_END, 0, line.Length) == 0) {
                            break;
                        }
                        //if (line.Length > 0 && line[0] == '$' && line.IndexOf(STR_VIRTUAL_DISK_ID, 0, line.Length) == -1 && line.IndexOf(STR_UNAVAILABLE_DISK_ID, 0, line.Length) == -1) {
                        //Include virtual disks.
                        if (line.Length > 0 && line[0] == '$') {
                            //Diskname (pos 0)
                            strDiskName = line.Substring(I_DISKNAME_INDEX, I_DISKNAME_LENGTH).TrimEnd();

                            if (line.IndexOf(STR_VIRTUAL_DISK_ID, 0, line.Length) == -1) {
                                //Mirrored (pos 9)
                                if (line[I_MIRRORED_INDEX] == 'Y') {
                                    bMirrored = true;
                                }
                                else {
                                    bMirrored = false;
                                }

                                try {
                                    //CapacityMB (starts at pos 15)
                                    iSpaceIndex = line.IndexOf(" ", I_CAPACITY_INDEX, (line.Length - I_CAPACITY_INDEX));
                                    capacityMB = Double.Parse(line.Substring(I_CAPACITY_INDEX, (iSpaceIndex - I_CAPACITY_INDEX)));

                                    //UsedMB (decimal at pos 32)
                                    iEndSpaceIndex = line.IndexOf(" ", I_USED_INDEX, (line.Length - I_USED_INDEX));
                                    freeMB = Double.Parse(line.Substring(iSpaceIndex, (iEndSpaceIndex - iSpaceIndex)).Trim());
                                }
                                catch (Exception ex) {
                                    _log.ErrorFormat("Error Reading Disk {0} Info: {1}", strDiskName, ex);
                                    bMirrored = false;
                                    capacityMB = 0;
                                    freeMB = 0;
                                }
                            }
                            else {
                                bMirrored = false;
                                capacityMB = 0;
                                freeMB = 0;
                            }

                            usedMB = capacityMB - freeMB;
                            //DSAP Error. Sometimes Free MB is greater then Total(when drive is 100% free).
                            if (usedMB < 0) {
                                usedMB = 0;
                            }

                            var diskToAdd = new Disk(strDiskName, bMirrored, capacityMB, usedMB);

                            //debug
                            //_log.InfoFormat("Info: Diskname[" + strDiskName + "] Capacity:[" + capacityMB + "] Used:[" + usedMB + "] Mirrored: " + bMirrored); 
                            //							

                            alDisks.Add(diskToAdd);
                        }
                    }

                    //Looping through ArrayList and getting info. for each disk

                    int iSubTypeIndex;
                    int iOpenBracketIndex;
                    int iCloseBracketIndex;

                    string strSubVolName = string.Empty;

                    int iFileCount = 0;
                    int iPageCount = 0;
                    double dUsageMB = 0;

                    short sGroup = 0;
                    short sUser = 0;
                    string strUserName = string.Empty;

                    //DateTime thisDate = new DateTime(2006,6,1,0,0,0,0);
                    _log.InfoFormat("Number of Disk: {0}", alDisks.Count);
                    

                    while (alDisks.Count > 0) {
                        //for (int i=0;i<1;i++)
                        //Getting Top element from list
                        disk = alDisks[0];
                        if (alDisks.Count > 1) {
                            nextDisk = alDisks[1];
                        }

                        string subDeviceInfo = string.Empty;

                        //Getting Subdevicetype
                        while ((line = sr.ReadLine()) != null && line.IndexOf(STR_SUBDEVICE_START + disk.strDiskName, 0, line.Length) != 0) {
                            //Skipping to Line starting with "Volume [Diskname]..."
                        }

                        line = sr.ReadLine();

                        if ((iSubTypeIndex = line.IndexOf(STR_SUBDEVICE_END, 0, line.Length)) == -1) {
                            disk.strSubDeviceType = string.Empty;
                        }
                        else {
                            iSpaceIndex = line.IndexOf(" ", iSubTypeIndex + I_SUBDEVICE_OFFSET, line.Length - (iSubTypeIndex + I_SUBDEVICE_OFFSET));
                            disk.strSubDeviceType = line.Substring(iSubTypeIndex + I_SUBDEVICE_OFFSET, iSpaceIndex - (iSubTypeIndex + I_SUBDEVICE_OFFSET)).Trim();

                            //Getting Subdevice Info
                            iOpenBracketIndex = line.IndexOf("(", iSubTypeIndex + I_SUBDEVICE_OFFSET, line.Length - (iSubTypeIndex + I_SUBDEVICE_OFFSET));
                            iCloseBracketIndex = line.IndexOf(")", iSubTypeIndex + I_SUBDEVICE_OFFSET, line.Length - (iSubTypeIndex + I_SUBDEVICE_OFFSET));

                            subDeviceInfo = line.Substring(iOpenBracketIndex + 1, iCloseBracketIndex - iOpenBracketIndex - 1).Trim();

                            disk.strSubDeviceType += " (" + subDeviceInfo + ")";

                            //debug
                            //_log.InfoFormat("Subdevice: "+ disk.strSubDeviceType);
                            //
                        }

                        //Checking if Disk is in DISKINFO, adding if not present, if capacity increased, creating new entry with higher ID
                        bool bToAdd = true;
                        int iDiskID = 1;
                        double capacityGB = disk.capacityMB / 1024;
                        double usedGB = disk.usedMB / 1024;
                        char cMirrored;
                        double prevUsedGB = 0;
                        double deltaMB = 0;
                        double deltaPercent = 0;
                        double avgDeltaMB = 0;
                        double avgDeltaPercent = 0;

                        if (disk.bMirrored) {
                            cMirrored = 'Y';
                        }
                        else {
                            cMirrored = 'N';
                        }

                        //Hardcoded system for now, should extract from header
                        //string systemserialnum = "046693";						

                        sqlStr = "SELECT * FROM DiskInfo WHERE DI_DiskName = '" + disk.strDiskName + "' AND DI_SystemSerialNum = '" + systemSerial + "' ORDER BY DI_DiskID DESC LIMIT 1";
                        dcomd.CommandText = sqlStr;
                        dcomd.CommandTimeout = 10000;
                        dread = dcomd.ExecuteReader();
                        if (dread.Read()) {
                            if ((Convert.ToString(dread["DI_SubDeviceType"]).Equals(disk.strSubDeviceType))) {
                                iDiskID = Convert.ToInt32(dread["DI_DiskID"]);
                                bToAdd = false;
                            }
                            else {
                                iDiskID = Convert.ToInt32(dread["DI_DiskID"]) + 1;
                            }
                        }
                        dread.Close();

                        if (bToAdd) {
                            //Adding row
                            sqlStr = "INSERT INTO DiskInfo(DI_SystemSerialNum, DI_DiskName, DI_DiskID, DI_CapacityGB, DI_Mirrored, DI_SubDeviceType, DI_DateFirstData) VALUES ('"
                                     + systemSerial + "', '" + disk.strDiskName + "', '" + iDiskID + "', '" + capacityGB + "', '" + cMirrored + "', '" + disk.strSubDeviceType + "', '" + dtDataDate.ToString("yyyy-MM-dd hh:mm:ss") + "')";
                            //+ systemserialnum + "', '" + disk.strDiskName + "', '" + iDiskID + "', '" + capacityGB + "', '" + cMirrored + "', '" + disk.strSubDeviceType + "', '2006-05-06 00:00:00.000')";
                            dcomd.CommandText = sqlStr;
                            dcomd.ExecuteNonQuery();
                        }
                        
                        //Check if this collection is new data.
                        bool newData = false;
                        newData = CheckNewData(systemSerial, dtDataDate, disk.strDiskName);

                        if (newData) {
                            bool updateCapacity = false;
                            //Check if GB changed.
                            sqlStr = "SELECT DI_CapacityGB FROM DiskInfo WHERE DI_DiskName = '" + disk.strDiskName + "' AND DI_SystemSerialNum = '" + systemSerial + "' AND DI_DiskID = '" + iDiskID + "'";

                            dcomd.CommandText = sqlStr;
                            dcomd.CommandTimeout = 10000;
                            dread = dcomd.ExecuteReader();

                            if (dread.Read()) {
                                double capacitygb = Convert.ToDouble(dread["DI_CapacityGB"]);
                                if (capacitygb != capacityGB) {
                                    //upate the capacity.
                                    updateCapacity = true;
                                }
                            }

                            dread.Close();

                            if (updateCapacity) {
                                sqlStr = "UPDATE DiskInfo SET DI_CapacityGB = '" + capacityGB + "' WHERE DI_DiskName = '" + disk.strDiskName + "' AND DI_SystemSerialNum = '" + systemSerial + "' AND DI_DiskID = '" + iDiskID + "'";
                                dcomd.CommandText = sqlStr;
                                dcomd.ExecuteNonQuery();
                            }

                            //TODO: Delete After load.
                            //Updating the DateLastData Entry in the DiskInfo table
                           // sqlStr = "UPDATE DiskInfo SET DI_DateLastData = '" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "' WHERE DI_DiskName = '" + disk.strDiskName + "' AND DI_SystemSerialNum = '" + systemSerial + "' AND DI_DiskID = '" + iDiskID + "'";
                            //dcomd.CommandText = sqlStr;
                            //dcomd.ExecuteNonQuery();

                            //Updating DateReplaced Entry if DiskID is incremented
                            if (bToAdd && iDiskID > 1) {
                                sqlStr = "UPDATE DiskInfo SET DI_DateReplaced = '" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "' WHERE DI_DiskName = '" + disk.strDiskName + "' AND DI_SystemSerialNum = '" + systemSerial + "' AND DI_DiskID = '" + (iDiskID - 1) + "'";
                                dcomd.CommandText = sqlStr;
                                dcomd.ExecuteNonQuery();
                            }

                            //Getting usedGB Value from previous day if available
                            //sqlStr = "SELECT DD_UsedGB FROM DailyDisk WHERE DD_SystemSerialNum = '" + systemserialnum + "' AND DD_Date = '" + thisDate.AddDays(-1).Date + "' AND DD_DiskName = '" + disk.strDiskName + "'";
                            sqlStr = "SELECT DD_UsedGB, DD_Date FROM DailyDisk WHERE DD_SystemSerialNum = '" + systemSerial + "' AND DD_Date < '" + dtDataDate.Date.ToString("yyyy-MM-dd hh:mm:ss") + "' AND DD_DiskName = '" + disk.strDiskName + "' ORDER BY DD_Date DESC LIMIT 1";

                            dcomd.CommandText = sqlStr;
                            dcomd.CommandTimeout = 10000;
                            dread = dcomd.ExecuteReader();

                            if (dread.Read()) {
                                // Difference in days.							
                                TimeSpan ts = dtDataDate.Date - Convert.ToDateTime(dread["DD_Date"]).Date;
                                int iDaysEarlier = ts.Days;

                                prevUsedGB = Convert.ToDouble(dread["DD_UsedGB"]);
                                //deltaMB = (usedGB-prevUsedGB)*1024;
                                deltaMB = ((usedGB - prevUsedGB) * 1024) / iDaysEarlier;
                                if (capacityGB != 0) {
                                    deltaPercent = (deltaMB / (capacityGB * 1024)) * 100;
                                }
                                else {
                                    deltaPercent = 0;
                                }
                            }

                            dread.Close();

                            //Inserting/Updating entry in DailyDisk table
                            sqlStr = "SELECT * FROM DailyDisk WHERE DD_SystemSerialNum = '" + systemSerial + "' AND DD_Date = '" + dtDataDate.Date.ToString("yyyy-MM-dd hh:mm:ss") + "' AND DD_DiskName = '" + disk.strDiskName + "'";
                            dcomd.CommandText = sqlStr;
                            dcomd.CommandTimeout = 10000;
                            dread = dcomd.ExecuteReader();

                            if (dread.Read()) {
                                //Updating existing entry
                                dread.Close();
                                sqlStr = "UPDATE DailyDisk SET DD_UsedGB = '" + usedGB + "', DD_DeltaMB = '" + deltaMB + "', DD_DeltaPercent = '" + deltaPercent + "' WHERE DD_SystemSerialNum = '" + systemSerial + "' AND DD_DiskName = '" + disk.strDiskName + "' AND DD_Date = '" + dtDataDate.Date.ToString("yyyy-MM-dd hh:mm:ss") + "'";
                                dcomd.CommandText = sqlStr;
                                dcomd.ExecuteNonQuery();
                            }
                            else {
                                //Adding new entry
                                dread.Close();
                                sqlStr = "INSERT INTO DailyDisk VALUES ('" + systemSerial + "', '" + dtDataDate.Date.ToString("yyyy-MM-dd hh:mm:ss") + "', '" + disk.strDiskName + "', '" + usedGB + "', '" + deltaMB + "', '" + deltaPercent + "')";
                                dcomd.CommandText = sqlStr;
                                dcomd.ExecuteNonQuery();
                            }

                            //TODO: Delete After load.
                            //Updating the DeltaGB and DeltaPercent in the DiskInfo table
                            sqlStr = "SELECT AVG(DD_DeltaMB) AS DeltaMB, AVG(DD_DeltaPercent) AS DeltaPercent " +
                                     "FROM " +
                                     "(SELECT DD_DeltaMB, DD_DeltaPercent " +
                                     "FROM DailyDisk " +
                                     "WHERE DD_DiskName = '" + disk.strDiskName + "' " +
                                     "AND DD_SystemSerialNum = '" + systemSerial + "' ORDER BY DD_Date  LIMIT 30" +
                                     ") AS TEMP";

                            dcomd.CommandText = sqlStr;
                            dcomd.CommandTimeout = 10000;
                            dread = dcomd.ExecuteReader();

                            if (dread.Read()) {
                                avgDeltaMB = Convert.ToDouble(dread["DeltaMB"]);
                                avgDeltaPercent = Convert.ToDouble(dread["DeltaPercent"]);
                                dread.Close();
                                sqlStr = "UPDATE DiskInfo SET DI_DailyDeltaMB = '" + avgDeltaMB + "', DI_DailyDeltaPercent = '" + avgDeltaPercent + "' WHERE DI_DiskName = '" + disk.strDiskName + "' AND DI_SystemSerialNum = '" + systemSerial + "' AND DI_DiskID = '" + iDiskID + "'";
                                dcomd.CommandText = sqlStr;
                                dcomd.ExecuteNonQuery();
                            }
                            dread.Close();

                            //Getting Subvolumes
                            noFile = false;
                            //Goto Line with [Disk Name] and String "New Subvol Summary Report" or "Teraform Subvol Summary Report"
                            while ((line = sr.ReadLine()) != null &&
                                   !((line.IndexOf(STR_SUBVOL_START, 0, line.Length) != -1 || line.IndexOf(STR_SUBVOL_ALT_START, 0, line.Length) != -1) &&
                                     line.IndexOf(disk.strDiskName, 0, line.Length) != -1)) {
                                //Skipping to top of subvolume report
                                if (line.IndexOf(STR_NO_FILES, 0, line.Length) != -1 ||
                                    (line.IndexOf(nextDisk.strDiskName, 0, line.Length) != -1 && alDisks.Count > 1)) {
                                    //No Files Allocated, so remove disk from array and goto next disk
                                    noFile = true;
                                    break;
                                }
                            }

                            //TODO: Change
                            //noFile = true;
                            if (!noFile) {
                                //Delete Entry in SubVolAllocation because sometimes subvol get deleted.
                                sqlStr = "DELETE FROM SubVolAllocation WHERE SA_SystemSerialNum = '" + systemSerial + "' AND SA_DiskName = '" + disk.strDiskName + "'";
                                dcomd.CommandTimeout = 10000;
                                dcomd.CommandText = sqlStr;
                                dcomd.ExecuteNonQuery();
                                do {
                                    //Stop on "New User Summary Report"
                                    if (line.IndexOf(STR_USER_START, 0, line.Length) != -1 || line.IndexOf(STR_USER_ALT_START, 0, line.Length) != -1) {
                                        break;
                                    }

                                    //Skip Blank lines, lines starting with space, lines starting "FREE SPACE" or "DISK DIRECTORY" or "TEMPORARY FILES"
                                    //If "New Subvol Summary Report" found again, means next page, again skip line
                                    if (
                                        line.Length == 0 ||
                                        line[0] == ' ' ||
                                        line.IndexOf("FREE SPACE", 0, line.Length) == 0 ||
                                        line.IndexOf("DISK DIRECTORY", 0, line.Length) == 0 ||
                                        line.IndexOf("TEMPORARY FILES", 0, line.Length) == 0 ||
                                        line.IndexOf(STR_SUBVOL_START, 0, line.Length) != -1 ||
                                        line.IndexOf(STR_SUBVOL_ALT_START, 0, line.Length) != -1
                                        ) {
                                        continue;
                                    }

                                    //Process Subvolume Data
                                    try {
                                        strSubVolName = line.Substring(0, 8).Trim();
                                        iFileCount = Convert.ToInt32(line.Substring(9, 34 - 9).Trim());
                                        if (line.Substring(34, 45 - 34).Trim().Split(' ').Length > 1) {
                                            string temp = line.Substring(34, 45 - 34).Trim().Split(' ')[0];
                                            iPageCount = Convert.ToInt32(temp);
                                        }
                                        else {
                                            iPageCount = Convert.ToInt32(line.Substring(34, 45 - 34).Trim());
                                        }
                                        //iPageCount = Convert.ToInt32(line.Substring(34, 45 - 35).Trim());
                                        //iPageCount = Convert.ToInt32(line.Substring(35, 45 - 35).TrimStart());
                                        dUsageMB = ((double) iPageCount * 2048) / 1000000;
                                    }
                                    catch (Exception ex) {
                                        _log.ErrorFormat(ex.Message);
                                        
                                    }
                                    myDataRow = subVolAllocation.NewRow();
                                    myDataRow["SA_SystemSerialNum"] = systemSerial;
                                    myDataRow["SA_DiskName"] = disk.strDiskName;
                                    myDataRow["SA_SubVolName"] = strSubVolName;
                                    myDataRow["SA_UsedMB"] = dUsageMB;
                                    myDataRow["SA_FileCount"] = iFileCount;
                                    subVolAllocation.Rows.Add(myDataRow);
                                } while ((line = sr.ReadLine()) != null || line.IndexOf(nextDisk.strDiskName, 0, line.Length) != -1);

                                //Getting Users

                                //Skipping line that says "New User Summary Report"
                                line = sr.ReadLine();

                                //TODO: only the data is new date.
                                //Delete Entry in UserAllocation because sometimes users account get deleted.
                                sqlStr = "DELETE FROM UserAllocation WHERE UA_SystemSerialNum = '" + systemSerial + "' AND UA_DiskName = '" + disk.strDiskName + "'";
                                dcomd.CommandTimeout = 10000;
                                dcomd.CommandText = sqlStr;
                                dcomd.ExecuteNonQuery();

                                do {
                                    //Stop if line with next diskname found
                                    //if (bMoreToFollow && line.IndexOf(strNextDisk,0, line.Length)!=-1) break;
                                    if (line.IndexOf("$", 0, line.Length) != -1) {
                                        int iDollarIndex = line.IndexOf("$", 0, line.Length);
                                        int iEndIndex = line.IndexOf(" ", iDollarIndex + 1, line.Length - (iDollarIndex + 1));

                                        if (line.Substring(iDollarIndex, iEndIndex - iDollarIndex).Trim().Equals(disk.strDiskName)) {
                                            continue;
                                        }
                                        break;
                                    }

                                    //Skip Blank lines, lines starting with space, lines starting "FREE SPACE" or "DISK DIRECTORY"							
                                    if (
                                        line.Length == 0 ||
                                        line[0] == ' ' ||
                                        line.IndexOf("FREE SPACE", 0, line.Length) == 0 ||
                                        line.IndexOf("DISK DIRECTORY", 0, line.Length) == 0 ||
                                        line.IndexOf(STR_USER_START, 0, line.Length) != -1 ||
                                        line.IndexOf(STR_USER_ALT_START, 0, line.Length) != -1
                                        ) {
                                        continue;
                                    }

                                    //Process User Data
                                    try {
                                        strUserName = line.Substring(0, 17).TrimEnd();
                                        sGroup = Convert.ToInt16(line.Substring(18, 3).Trim());
                                        sUser = Convert.ToInt16(line.Substring(22, 3).Trim());
                                        iFileCount = Convert.ToInt32(line.Substring(26, 34 - 26).Trim());
                                        if (line.Substring(34, 45 - 34).Trim().Split(' ').Length > 1) {
                                            string temp = line.Substring(34, 45 - 34).Trim().Split(' ')[0];
                                            iPageCount = Convert.ToInt32(temp);
                                        }
                                        else {
                                            iPageCount = Convert.ToInt32(line.Substring(34, 45 - 34).Trim());
                                        }
                                        dUsageMB = ((double) iPageCount * 2048) / 1000000;
                                    }
                                    catch (Exception ex) {
                                        _log.ErrorFormat(ex.Message);
                                        
                                    }

                                    myDataRow = userAllocation.NewRow();
                                    myDataRow["UA_SystemSerialNum"] = systemSerial;
                                    myDataRow["UA_DiskName"] = disk.strDiskName;
                                    myDataRow["UA_Group"] = sGroup;
                                    myDataRow["UA_User"] = sUser;
                                    myDataRow["UA_UserName"] = strUserName;
                                    myDataRow["UA_UsedMB"] = dUsageMB;
                                    myDataRow["UA_FileCount"] = iFileCount;
                                    userAllocation.Rows.Add(myDataRow);
                                } while ((line = sr.ReadLine()) != null);
                            }
                        }
                        //Removing this disk from the ArrayList
                        alDisks.RemoveAt(0);
                    }
                }

                _log.InfoFormat("SubVolAllocation Count: {0}", subVolAllocation.Rows.Count);
                var dicInfo = new DirectoryInfo(strFileName);

                if (subVolAllocation.Rows.Count > 0) {
                    //Bulk Insert to SubVolAllocation
                    var dataTablesService = new DataTableService(connStr);
                    dataTablesService.InsertEntityDataFor("SubVolAllocation", subVolAllocation, dicInfo.Parent.FullName);
                }

                _log.InfoFormat("UserAllocation Count: {0}", userAllocation.Rows.Count);
                
                if (userAllocation.Rows.Count > 0) {
                    //Bulk Insert to UserAllocation
                    var dataTablesService = new DataTableService(connStr);
                    dataTablesService.InsertEntityDataFor("UserAllocation", userAllocation, dicInfo.Parent.FullName);
                }

                //Everything was ok.
                returnValue = true;
            }
            catch (Exception e) {
                // Let the user know what went wrong.
                _log.ErrorFormat("Exception Occurred:");
                _log.ErrorFormat(e.Message);
                returnValue = false;
            }
            finally {
                dconn.Close(); //Close the connection.
                dconn.Dispose(); //Dispose the connection.
                alDisks = null;
                disk = null;
                nextDisk = null;

                currentProc.Refresh();
                _log.InfoFormat("After loadDiskData Memory : {0}",currentProc.PrivateMemorySize64);
                

                GC.Collect(); //Call GC to clean up data.

                currentProc.Refresh();
                _log.InfoFormat("After loadDiskData GC Memory : {0}",currentProc.PrivateMemorySize64);
                
            }

            return returnValue;
        }

        /// <summary>
        /// Check if the data of this load is new
        /// </summary>
        /// <param name="systemSerial"> System serial number.</param>
        /// <param name="dtDataDate"> DateTime of this disk data.</param>
        /// <param name="diskName"> Disk name.</param>
        /// <returns></returns>
        private bool CheckNewData(string systemSerial, DateTime dtDataDate, string diskName) {
            bool returnValue = false;
            var databaseMappingService = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
            string connStr = databaseMappingService.GetConnectionStringFor(systemSerial);
            if (connStr.Length == 0)
            {
                connStr = ConnectionString.ConnectionStringSPAM;
            }
            string cmdText = "SELECT DI_DateLastData FROM DiskInfo " +
                             "WHERE DI_DateLastData >= @DataDate " +
                             "AND DI_SystemSerialNum = @SystemSerial " +
                             "AND DI_DiskName = @DiskName LIMIT 1";
            using (var connection = new MySqlConnection(connStr))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@DataDate", dtDataDate);
                command.Parameters.AddWithValue("@DiskName", diskName);
                command.CommandTimeout = 0;
                connection.Open();
                var reader = command.ExecuteReader();

                if (reader.Read()) {
                    returnValue = false;
                }
                else {
                    returnValue = true;
                }
            }

            return returnValue;
        }
    }

    internal class Disk {
        public bool bMirrored;
        public double capacityMB;
        public string strDiskName;
        public string strSubDeviceType;
        public double usedMB;

        public Disk(string strDiskName, bool bMirrored, double capacityMB, double usedMB) {
            this.strDiskName = strDiskName;
            this.bMirrored = bMirrored;
            this.capacityMB = capacityMB;
            this.usedMB = usedMB;

            this.strSubDeviceType = string.Empty;
        }
    }
}