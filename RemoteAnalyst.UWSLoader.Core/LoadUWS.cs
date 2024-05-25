using System.Threading;
using RemoteAnalyst.UWSLoader.Core.BaseClass;
using RemoteAnalyst.UWSLoader.Core.BusinessLogic;
using RemoteAnalyst.UWSLoader.Core.Enums;
using RemoteAnalyst.UWSLoader.Core.ModelView;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using RemoteAnalyst.UWSLoader.Core.DiskBrowser;
using RemoteAnalyst.Repository.Infrastructure;
using Helper = RemoteAnalyst.UWSLoader.Core.BusinessLogic.Helper;
using log4net;
using DataBrowser.Context;

namespace RemoteAnalyst.UWSLoader.Core {
    class LoadUWS : Header {
        private readonly bool _remoteAnalyst;
        private readonly bool _websiteLoad;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="remoteAnalyst">True when it's calling from Remote Analyst</param>
        /// <param name="websiteLoad">True when it's calling from Remote Analyst</param>
        public LoadUWS(bool remoteAnalyst, bool websiteLoad) {
            _remoteAnalyst = remoteAnalyst;
            _websiteLoad = websiteLoad;
        }

        /// <summary>
        /// This function will load UWS File to SQL Server and MySQL. On MySQL we currently load CPU, DISC, DISKFIL, FILE, PROCESS, USERDEF, FILETREND, and DISKBROWSER.
        /// *Please note that this function does not have function to create a Database on SQL Server and MySQL. All the checks and Database Create needs to be done before.
        /// *Not for Remote Analyst: This function do not call Trend, SCM Load, and Process Watch.
        /// </summary>
        /// <param name="uwsPath">Full Path of UWS file including File Name</param>
        /// <param name="log">ILog - log4net</param>
        /// <param name="uwsID">Only for Remote Analyst pass 0 from PMC</param>
        /// <param name="connectionString">Main Database ConnectionString</param>
        /// <param name="newConnectionString">Detail (System) Database Connection</param>
        /// <param name="systemFolder">This Folder is where all the log files will get created.</param>
        /// <param name="uwsVersion">Option Param. This is for Remote Analyst only.</param>
        /// <returns>UWSInfo: DateTime StartDateTime, DateTime StopDateTime, long Interval, List<int> EntityIds, bool Success, string ErrorMessage</returns>
        internal UWSInfo CreateMultiDayDataSet(string uwsPath, ILog log, int uwsID, string connectionString, string newConnectionString, string systemFolder, UWS.Types uwsVersion = UWS.Types.Version2013) {
            var uwsInfo = new UWSInfo();
            string saveLocation = systemFolder + UWSSerialNumber;
            if (!Directory.Exists(saveLocation))
                Directory.CreateDirectory(saveLocation);

            uwsInfo.EntityIds = new List<int>();
            uwsInfo.DuplicatedEntityIds = new Dictionary<int, int>();
            uwsInfo.CurrentTables = new List<CurrentTables>();
            uwsInfo.TableTimestamps = new List<TableTimestamp>();

            try {
                OpenNewUWSFile(uwsPath, uwsVersion, log, connectionString);
            }
            catch (Exception ex) {
                uwsInfo.Success = false;
                uwsInfo.ErrorMessage = ex.Message;
                return uwsInfo;
            }

            log.Info("    -Populateing SPAM Database");
            
            bool success = false;
            bool entityAccountedFor = false;
            var tempShort = new byte[2];
            var tempInt = new byte[4];
            var tempLong = new byte[8];
            //string strIndexValues = string.Empty;
            string systemName = string.Empty;
            int entityID = 0;
            long startTime = 0L;
            long stopTime = 0L;
            long sampleInterval = 0L;
            DateTime startTimeLCT;
            DateTime stopTimeLCT;

            //var discopenHours = new List<string>();
            var uniqueTableName = new List<string>();
            var tableNames = new List<string>();
            var currentTables = new List<MultiDays>();
            var tableTimestamps = new List<MultiDays>();
            var tableNameList = new List<string>(); //Used for updating the archive IDs after uploading to Amazon Glacier
            var entityList = new Dictionary<string, string>();
            var processTableList = new List<string>();
            var dataStartDate = new DateTime();
            var dataStopDate = new DateTime();
            var uwsStartDate = new DateTime();
            var uwsStopDate = new DateTime();
            var currentTable = new CurrentTableService(newConnectionString);
            //var daysMySQL = new List<MultiDays>();//Used to populate the mysql DISC table

            string fileEntity = "";
            bool glacierLoad = false;
            bool isProcessDirectlySystem = false;

            try {
                #region Load Data

                log.Info("******   Start loading data   *******");

                using (var stream = new FileStream(uwsPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    var dicInfo = new DirectoryInfo(stream.Name);
                    using (reader = new BinaryReader(stream)) {
                        // LOOP THRU ALL ENTITIES
                        foreach (Indices t in index) {
                            if (t.FName.Length != 0 && t.FRecords > 0) {
                                // Get eneityID.
                                var entity = new EntitiesService(connectionString);
                                entityID = entity.GetEntityIDFor(t.FName.Trim());
                                if (entityID == 0) continue; // NOT A VALID ENTITY

                                if (!uwsInfo.EntityIds.Contains(entityID))
                                    uwsInfo.EntityIds.Add(entityID);

                                double dblActualFileSize = stream.Length;
                                double dblCalculatedFileSize = (t.FReclen * t.FRecords) + t.FilePosition;
                                if (dblCalculatedFileSize > dblActualFileSize) {
                                    // CALCULATED SIZE IS GREATER THAN ACTUAL SIZE.  PARTIAL FILE RECEIVED. 
                                    var emailText = new StringBuilder();
                                    emailText.Append("<br>A UWS file was partially loaded at " + DateTime.Now + ":");
                                    emailText.Append("<UL>");
                                    emailText.Append("	<LI>");
                                    emailText.Append("		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>Node:  " + t.FSysName + " </DIV>");
                                    emailText.Append("	<LI>");
                                    emailText.Append("		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>Erroneous Entity:  " + t.FName + " </DIV>");
                                    emailText.Append("	<LI>");
                                    emailText.Append("		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>UWS File: " + uwsPath.Trim() + "</DIV>");
                                    emailText.Append("	<LI>");
                                    emailText.Append("		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>Actual File Size: " + dblActualFileSize.ToString() + "</DIV>");
                                    emailText.Append("	<LI>");
                                    emailText.Append("		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>Computed File Size: " + dblCalculatedFileSize.ToString() + "</DIV>");
                                    emailText.Append("	</LI>");
                                    emailText.Append("</UL>");
                                    //var email = new CreateSendErrorEmail(emailText.ToString(), string.Empty);

                                    reader.Close();
                                    stream.Close();
                                    if (File.Exists(uwsPath)) File.Delete(uwsPath);

                                    uwsInfo.Success = false;
                                    uwsInfo.ErrorMessage = emailText.ToString();
                                    return uwsInfo;
                                }

                                log.InfoFormat("Entity Name: {0} Started", t.FName.Trim());
                                
                                DateTime beforeTime = DateTime.Now;

                                if (systemName.Length == 0) systemName = t.FSysName;
                                if (sampleInterval == 0) sampleInterval = t.FInterval;

                                string dbTableName;
                                if (uwsVersion == UWS.Types.Version2007 || uwsVersion == UWS.Types.Version2009) {
                                    //Start Time.
                                    if (startTime == 0) startTime = t.FStartTime;
                                    else if (startTime > t.FStartTime) startTime = t.FStartTime;

                                    //Stop Time.
                                    if (stopTime == 0) stopTime = t.FStopTime;
                                    else if (stopTime < t.FStopTime) stopTime = t.FStopTime;

                                    //Get table name according to data format.
                                    var mVersion = new MeasureVersionsService(connectionString);
                                    dbTableName = mVersion.GetMeasureDBTableNameFor(t.FMeasVer);
                                    log.InfoFormat("      Measure Type: {0}", dbTableName);
                                    

                                    //Create Table name.
                                    var convertTime = new ConvertJulianTime();
                                    int timeStamp = convertTime.JulianTimeStampToOBDTimeStamp(t.FStartTime);
                                    dataStartDate = convertTime.OBDTimeStampToDBDate(timeStamp);
                                    timeStamp = convertTime.JulianTimeStampToOBDTimeStamp(t.FStopTime);
                                    dataStopDate = convertTime.OBDTimeStampToDBDate(timeStamp);
                                }
                                else {
                                    //Get table name according to data format.
                                    var mVersion = new MeasureVersionsService(connectionString);
                                    dbTableName = mVersion.GetMeasureDBTableNameFor(t.FMeasVer);
                                    log.InfoFormat("      Measure Type: {0}", dbTableName);
                                    

                                    if (_remoteAnalyst) {
                                        //change the table name to new table. This is only for new uws header version.
                                        if (dbTableName.Equals("ZmsBladeDataDictionary") || dbTableName.Equals("ZmsDataDictionary")) {
                                            var vProc = new VProcVersionService(connectionString);
                                            dbTableName = vProc.GetDataDictionaryFor(UwsCreatorVproc);
                                        }

                                        log.InfoFormat("      New Measure Type: {0}", dbTableName);
                                        
                                    }
                                    else {
                                        log.InfoFormat("VPROC: {0}", UwsCreatorVproc);
                                        
                                        if (UwsCreatorVproc.Trim().Equals("T0951H01_30SEP2015_RASMCOLL_2015_1_0") ||
                                            UwsCreatorVproc.Trim().Equals("T2080H02_01Jul2013_TPDCCVTR_ABC")) {
                                            dbTableName = "ZmsBladeDataDictionaryV1";
                                        }

                                        log.InfoFormat("      New Measure Type: {0}", dbTableName);
                                        
                                    }

                                    dataStartDate = t.CollEntityStartTime;
                                    dataStopDate = t.CollEntityStoptTime;

                                    //Start Time.
                                    if (uwsStartDate.Equals(DateTime.MinValue)) uwsStartDate = t.CollEntityStartTime;
                                    else if (uwsStartDate > t.CollEntityStartTime) uwsStartDate = t.CollEntityStartTime;

                                    //Stop Time.
                                    if (uwsStopDate.Equals(DateTime.MinValue)) uwsStopDate = t.CollEntityStoptTime;
                                    else if (uwsStopDate < t.CollEntityStoptTime) uwsStopDate = t.CollEntityStoptTime;
                                }

                                log.InfoFormat("      SystemSerial: {0}", UWSSerialNumber);
                                

                                //Get Column type into the List.
                                var dictionary = new DataDictionaryService(connectionString);
#if(DEBUG)
                                dbTableName = "ZmsBladeDataDictionaryV1";
#endif
                                IList<ColumnInfoView> columnInfo = dictionary.GetColumnsFor(entityID, dbTableName);

                                int recordLenth = t.FReclen;
                                long filePosition = t.FilePosition;

                                //Round up the seconds.
                                TimeSpan span = dataStopDate - dataStartDate;
                                double seconds = span.TotalSeconds;

                                //Get remained seconds.
                                double remainSeconds = seconds % t.FInterval;
                                if (remainSeconds < t.FInterval * 0.1) {
                                    dataStopDate = dataStopDate.AddSeconds(-remainSeconds);
                                }

                                //Lei: IR 7323. Above if block covers the scenario when the time is within 90 seconds scope,
                                //the time will be rounded down, for example 06:15:59 will be cut down to 06:15:00,
                                //below if block covers the round-up scenario, for example 06:59:59 will be rounded up to 07:00:00
                                if (remainSeconds < t.FInterval && remainSeconds > t.FInterval * 0.9) {
                                    dataStopDate = dataStopDate.AddSeconds(t.FInterval - remainSeconds);
                                }
                                log.InfoFormat("      dataStartDate: {0}", dataStartDate);
                                log.InfoFormat("      dataStopDate: {0}", dataStopDate);
                                

                                #region CheckMulti Day

                                var days = new List<MultiDays>();

                                //Check to see if there are multi days. if yes, move data to currect table.
                                string buildTableName;
                                MultiDays multiDays;
                                if (dataStartDate.Day != dataStopDate.Day) {
                                    //Check to see how many days.
                                    for (DateTime x = dataStartDate; x < dataStopDate; x = x.AddDays(1)) {
                                        multiDays = new MultiDays {
                                            StartDate = x,
                                            EndDate = new DateTime(x.Year, x.Month, x.Day, 23, 59, 59),
                                            EntityID = entityID,
                                            Interval = t.FInterval,
                                            SystemSerial = UWSSerialNumber,
                                            MeasureVersion = t.FMeasVer
                                        };

                                        buildTableName = UWSSerialNumber + "_" + t.FName.Trim().ToUpper() + "_" + x.Year + "_" + x.Month + "_" + x.Day;
                                        multiDays.TableName = buildTableName;
                                        if (entityID.Equals(7)) {
                                            if (!processTableList.Contains(buildTableName))
                                                processTableList.Add(buildTableName);
                                        }
                                        multiDays.DontLoad = false;
                                        days.Add(multiDays);

                                        //SAVE GENERIC TABLE NAME FOR UniqueID PROCESSING
                                        if (!entityAccountedFor) {
                                            buildTableName = UWSSerialNumber + "_%_" + x.Year + "_" + x.Month + "_" + x.Day;
                                            uniqueTableName.Add(buildTableName);
                                        }
                                    }

                                    //Add stop date. I'm putting this here so that I can get ending time.
                                    multiDays = new MultiDays {
                                        StartDate = new DateTime(dataStopDate.Year, dataStopDate.Month, dataStopDate.Day),
                                        EndDate = dataStopDate,
                                        EntityID = entityID,
                                        Interval = t.FInterval,
                                        SystemSerial = UWSSerialNumber,
                                        MeasureVersion = t.FMeasVer
                                    };

                                    buildTableName = UWSSerialNumber + "_" + t.FName.Trim().ToUpper() + "_" + dataStopDate.Year + "_" + dataStopDate.Month + "_" + dataStopDate.Day;
                                    multiDays.TableName = buildTableName;
                                    if (entityID.Equals(7)) {
                                        if (!processTableList.Contains(buildTableName))
                                            processTableList.Add(buildTableName);
                                    }

                                    multiDays.DontLoad = false;
                                    days.Add(multiDays);

                                    //SAVE GENERIC TABLE NAME FOR UniqueID PROCESSING
                                    if (!entityAccountedFor) {
                                        buildTableName = UWSSerialNumber + "_%_" + dataStopDate.Year + "_" + dataStopDate.Month + "_" + dataStopDate.Day;
                                        uniqueTableName.Add(buildTableName);
                                        entityAccountedFor = true;
                                    }
                                }
                                else {
                                    //Add single day.
                                    multiDays = new MultiDays {
                                        StartDate = dataStartDate,
                                        EndDate = dataStopDate,
                                        EntityID = entityID,
                                        Interval = t.FInterval,
                                        SystemSerial = UWSSerialNumber,
                                        MeasureVersion = t.FMeasVer
                                    };

                                    //multiDays.StartDate = new DateTime(dataStopDate.Year, dataStopDate.Month, dataStopDate.Day);
                                    buildTableName = UWSSerialNumber + "_" + t.FName.Trim().ToUpper() + "_" + dataStartDate.Year + "_" + dataStartDate.Month + "_" + dataStartDate.Day;
                                    multiDays.TableName = buildTableName;
                                    if (entityID.Equals(7)) {
                                        if (!processTableList.Contains(buildTableName))
                                            processTableList.Add(buildTableName);
                                    }
                                    multiDays.DontLoad = false;
                                    days.Add(multiDays);

                                    //SAVE GENERIC TABLE NAME FOR UniqueID PROCESSING
                                    if (!entityAccountedFor) {
                                        buildTableName = UWSSerialNumber + "_%_" + dataStartDate.Year + "_" + dataStartDate.Month + "_" + dataStartDate.Day;
                                        uniqueTableName.Add(buildTableName);
                                        entityAccountedFor = true;
                                    }
                                }

                                #endregion
                                
                                #region Create Table

                                //Check to see if table name exists.
                                foreach (MultiDays d in days) {
                                    var table = new DataTableService(newConnectionString);
                                    bool checkTableExists = table.CheckTableFor(d.TableName);

                                    if (d.StartDate != d.EndDate) {
                                        #region Create Table
                                        if (!checkTableExists) {
                                            //Create Table.
                                            //entity.CreateEntityTable(d.TableName, columnInfo, true, _websiteLoad, newConnectionString);
                                            var mySQLServices = new MySQLServices(_remoteAnalyst);
                                            mySQLServices.CreateEntityTable(entityID, d.SystemSerial, d.TableName, columnInfo, _websiteLoad, connectionString, log, false); //Since this is used by PMC, non of the system is visa.

                                            //Insert basic info into the Current Table.
                                            //currentTable.InsertCurrentTable(d.TableName, d.EntityID, d.Interval, d.StartDate, d.SystemSerial, d.MeasureVersion);
                                            var tempCurrentTable = new TempCurrentTablesService(newConnectionString);
                                            tempCurrentTable.InsertCurrentTableFor(d.TableName, d.EntityID, d.Interval, d.StartDate, d.SystemSerial, d.MeasureVersion);

                                            //Insert Time Stamps.
                                            var tableTimeStamp = new TempTableTimestampService(newConnectionString);
                                            tableTimeStamp.InsertTempTimeStampFor(d.TableName, d.StartDate, d.EndDate);

                                            currentTables.Add(d);
                                            tableTimestamps.Add(d);
                                        }
                                        else {
                                            //Check if Interval matches.
                                            bool intervalMatch = false;
                                            long currentInterval = currentTable.GetIntervalFor(d.TableName);
                                            if (currentInterval == d.Interval) {
                                                intervalMatch = true;
                                            }
                                            else {
                                                //Check with Temp current table interval.
                                                var tempCurrentTable = new TempCurrentTablesService(newConnectionString);
                                                currentInterval = tempCurrentTable.GetIntervalFor(d.TableName);
                                                if (currentInterval == d.Interval) {
                                                    intervalMatch = true;
                                                }
                                            }

                                            if (intervalMatch) {
                                                //Check if Time Stamp don't over laps.
                                                var tableTimeStamp = new TableTimeStampService(newConnectionString);
                                                var tempTableTimeStamp = new TempTableTimestampService(newConnectionString);
                                                bool timeOverLap = tableTimeStamp.CheckTimeOverLapFor(d.TableName, d.StartDate, d.EndDate);
                                                if (!timeOverLap) {
                                                    //Check if Time Stamp don't over laps from TempCurrentTable.
                                                    bool tempTimeOverLap = tableTimeStamp.CheckTimeOverLapFor(d.TableName, d.StartDate, d.EndDate);
                                                    if (!tempTimeOverLap) {
                                                        tempTimeOverLap = tableTimeStamp.CheckTempTimeOverLapFor(d.TableName, d.StartDate, d.EndDate);
                                                        if (!tempTimeOverLap) {
                                                            uwsInfo.DuplicatedEntityIds.Add(entityID, (int)ReturnStatus.Types.OverLap);
                                                            d.DontLoad = true;
                                                        }
                                                        else {
                                                            //Insert Time Stamps. 
                                                            tempTableTimeStamp.InsertTempTimeStampFor(d.TableName, d.StartDate, d.EndDate);
                                                            tableTimestamps.Add(d);
                                                        }
                                                    }
                                                    else {
                                                        //Insert Time Stamps. 
                                                        tempTableTimeStamp.InsertTempTimeStampFor(d.TableName, d.StartDate, d.EndDate);
                                                        tableTimestamps.Add(d);
                                                    }
                                                }
                                                else {
                                                    //Insert Time Stamps. 
                                                    tempTableTimeStamp.InsertTempTimeStampFor(d.TableName, d.StartDate, d.EndDate);
                                                    tableTimestamps.Add(d);
                                                }
                                            }
                                            else {
                                                //Stop this process.
                                                //continue;
                                                //load = false;
                                                //break;
                                                uwsInfo.DuplicatedEntityIds.Add(entityID, (int)ReturnStatus.Types.IntervalMismatch);
                                                d.DontLoad = true;
                                            }
                                        }

                                        #endregion
                                    }
                                    else {
                                        uwsInfo.DuplicatedEntityIds.Add(entityID, (int)ReturnStatus.Types.SameStartAndStopTime);
                                        d.DontLoad = true;
                                    }

                                    //Get Entity List.
                                    if (entityID == (int)Entity.CPU) {
                                        if (!entityList.ContainsKey("CPU")) {
                                            entityList.Add("CPU", "[" + d.TableName + "]");
                                        }
                                    }
                                    else if (entityID == (int)Entity.DISC) {
                                        if (!entityList.ContainsKey("DISK")) {
                                            entityList.Add("DISK", "[" + d.TableName + "]");
                                        }
                                    }
                                    /*else if (entityID == 4) { DISKFILE is not need to load Trend Data.
                                        if (!entityList.ContainsKey("DISKFILE")) {
                                            entityList.Add("DISKFILE", "[" + d.TableName + "]");
                                        }
                                    }*/
                                    else if (entityID == (int)Entity.PROCESS) {
                                        if (!entityList.ContainsKey("PROCESS")) {
                                            entityList.Add("PROCESS", "[" + d.TableName + "]");
                                        }
                                    }
                                    else if (entityID == (int)Entity.TMF) {
                                        if (!entityList.ContainsKey("TMF")) {
                                            entityList.Add("TMF", "[" + d.TableName + "]");
                                        }
                                    }
                                    else if (entityID == (int)Entity.FILE) {
                                        if (fileEntity.Length.Equals(0))
                                            fileEntity = d.TableName;
                                    }
                                    /*else if (entityID == 3) { DISCOPE is not need to load Trend Data.
                                        if (!entityList.ContainsKey("DISCOPE")) {
                                            entityList.Add("DISCOPE", "[" + d.TableName + "]");
                                        }
                                    }*/
                                }

                                #endregion

                                if (_remoteAnalyst) {
                                    if (!columnInfo.Any(x => x.Website.Equals(true))) {
                                        //IF NONE OF THE COLUMNS ARE "Website" FLAGGED, UPDATE "CurrentTable" AND "TableTimestamp" BEFORE BYPASSING TABLE.

                                        #region Update CurrentTable & TableTimestamp.

                                        if (currentTables.Count > 0) {
                                            var tempCurrentTable = new TempCurrentTablesService(newConnectionString);
                                            foreach (var d in currentTables) {
                                                try {
                                                    //Insert basic info into the Current Table.
                                                    currentTable.InsertEntryFor(d.TableName, d.EntityID, d.Interval, d.StartDate, d.SystemSerial, d.MeasureVersion);
                                                }
                                                catch (Exception ex) {
                                                    log.ErrorFormat("Insert CurrentTable: {0}", ex.Message);
                                                }
                                                try {
                                                    //Delete from TempCurrentTable.
                                                    tempCurrentTable.DeleteCurrentTableFor(d.TableName);
                                                }
                                                catch (Exception ex) {
                                                    log.ErrorFormat("Delete TempCurrentTable: {0}", ex.Message);
                                                }
                                            }
                                            currentTables.Clear();
                                        }

                                        if (tableTimestamps.Count > 0) {
                                            var tableTimeStamp = new TableTimeStampService(newConnectionString);
                                            var tempTimeStamp = new TempTableTimestampService(newConnectionString);

                                            foreach (var d in tableTimestamps) {
                                                try {
                                                    //Insert basic info into the Current Table. - make default status "Active"
                                                    tableTimeStamp.InsertEntryFor(d.TableName, d.StartDate, d.EndDate, (int) ArchiveStatus.Status.Active);
                                                    tableNameList.Add(d.TableName);
                                                }
                                                catch (Exception ex) {
                                                    log.ErrorFormat("Insert TableTimeStamp: {0}", ex.Message);
                                                    
                                                }
                                                try {
                                                    //Delete from TempCurrentTable.
                                                    tempTimeStamp.DeleteTempTimeStampFor(d.TableName);
                                                }
                                                catch (Exception ex) {
                                                    log.ErrorFormat("Delete TempTimeStamp: {0}", ex.Message);
                                                    
                                                }
                                            }
                                            tableTimestamps.Clear();
                                        }

                                        #endregion

                                        continue;
                                    }
                                }

                                if (entityID == 8) {
                                    log.Info("      ***** Skipping SQLPROC");
                                    
                                    continue;
                                }
                                if (entityID == 8) {
                                    log.Info("      ***** Skipping SQLSTMT");
                                    
                                    continue;
                                }
                                //New Logic. IR 6391, skip the discopen entity.
                                if (entityID == 3) {
                                    /*string buildDateTime = dataStartDate + "|" + dataStopDate;
                                    if (!discopenHours.Contains(buildDateTime)) {
                                        discopenHours.Add(buildDateTime);
                                    }*/

                                    log.Info("      ***** Skipping DISCOPEN");
                                    
                                    continue;
                                }

                                var myDataSet = new DataSet();

                                try {
                                    #region Create DataSet and insert data

                                    myDataSet = CreateSPAMDataTableColumn(days, columnInfo);
                                    if (myDataSet.Tables.Count > 0) {
                                        //Test values for tableName From FromTimeStamp.
                                        var tableNameFromTimeStamp = new DateTime();

                                        //Loop through the records.
                                        for (int x = 0; x < t.FRecords; x++) {
                                            reader.BaseStream.Seek(filePosition, SeekOrigin.Begin);
                                            byte[] indexBytes = reader.ReadBytes(recordLenth);
                                            long currentPosition = 0;

                                            //this will create newline for each loop.
                                            //myDataRow = myDataTable.NewRow();

                                            //test.
                                            double fromJulian = 0;
                                            double toJulian = 0;

                                            foreach (ColumnInfoView column in columnInfo) {
                                                if (column.ColumnName == "FromJulian" && (entityID == 3 || entityID == 5)) {
                                                    //myDataRow[column.ColumnName] = fromJulian;
                                                    column.TestValue = fromJulian.ToString();
                                                    continue;
                                                }
                                                if (column.ColumnName == "ToJulian" && (entityID == 3 || entityID == 5)) {
                                                    //myDataRow[column.ColumnName] = toJulian;
                                                    column.TestValue = toJulian.ToString();
                                                    continue;
                                                }

                                                // This condition is to check if the number of fields in the UWS file is less than in TabelTable, we have to stop getting data and feed in just 'Null'
                                                if (currentPosition < t.FReclen) {
                                                    #region Switch

                                                    switch (column.TypeName) {
                                                        case "SMALLINT":
                                                            try {
                                                                tempShort[0] = indexBytes[currentPosition + 1];
                                                                tempShort[1] = indexBytes[currentPosition];
                                                                //myDataRow[column.ColumnName] = Convert.ToInt16(BitConverter.ToInt16(tempShort, 0));
                                                                column.TestValue = BitConverter.ToInt16(tempShort, 0).ToString();
                                                            }
                                                            catch (Exception ex) {
                                                                column.TestValue = "0";
                                                                log.ErrorFormat("EntityID: " + entityID +
                                                                                     "\n column TypeName:" + column.TypeName +
                                                                                     "\n current position: " + currentPosition +
                                                                                     "\n index FReclen: " + t.FReclen +
                                                                                     "\n Message:" + ex.Message);
                                                                
                                                                throw new Exception(ex.Message);
                                                            }
                                                            break;
                                                        case "INT": //NOTE: VARIABLE COMING IN AS "SMALLINT" AND SAVED AS "INT"
                                                            try {
                                                                tempShort[0] = indexBytes[currentPosition + 1];
                                                                tempShort[1] = indexBytes[currentPosition];
                                                                //myDataRow[column.ColumnName] = Convert.ToInt16(BitConverter.ToInt16(tempShort, 0));
                                                                column.TestValue = BitConverter.ToUInt16(tempShort, 0).ToString();

                                                                if ((column.ColumnName.ToUpper().Equals("IPUS") ||
                                                                    column.ColumnName.ToUpper().Equals("IPUNUM")) && column.TestValue.Equals("0")) {
                                                                    //If the Ipus number is 0, we need to change this to 1. 
                                                                    column.TestValue = "1";
                                                                }
                                                            }
                                                            catch (Exception ex) {
                                                                column.TestValue = "0";
                                                                log.ErrorFormat("EntityID: " + entityID +
                                                                                     "\n column TypeName:" + column.TypeName +
                                                                                     "\n current position: " + currentPosition +
                                                                                     "\n index FReclen: " + t.FReclen +
                                                                                     "\n Message:" + ex.Message);
                                                                
                                                                throw new Exception(ex.Message);
                                                            }
                                                            break;
                                                        case "BIGINT": //NOTE: VARIABLE COMING IN AS "INT" AND SAVED AS "BIGINT"
                                                            try {
                                                                tempInt[0] = indexBytes[currentPosition + 3];
                                                                tempInt[1] = indexBytes[currentPosition + 2];
                                                                tempInt[2] = indexBytes[currentPosition + 1];
                                                                tempInt[3] = indexBytes[currentPosition + 0];
                                                                //myDataRow[column.ColumnName] = Convert.ToInt32(BitConverter.ToInt32(tempInt, 0));
                                                                column.TestValue = BitConverter.ToUInt32(tempInt, 0).ToString();

                                                                /*if ((column.ColumnName.ToUpper().Equals("IPUS") ||
                                                                    column.ColumnName.ToUpper().Equals("IPUNUM")) && column.TestValue.Equals("0")) {
                                                                    //If the Ipus number is 0, we need to change this to 1. 
                                                                    column.TestValue = "1";
                                                                }*/

                                                            }
                                                            catch (Exception ex) {
                                                                column.TestValue = "0";
                                                                log.ErrorFormat("EntityID: " + entityID +
                                                                                     "\n column TypeName:" + column.TypeName +
                                                                                     "\n current position: " + currentPosition +
                                                                                     "\n index FReclen: " + t.FReclen +
                                                                                     "\n Message:" + ex.Message);
                                                                
                                                                throw new Exception(ex.Message);
                                                            }
                                                            break;
                                                        case "FLOAT":
                                                            try {
                                                                tempLong[0] = indexBytes[currentPosition + 7];
                                                                tempLong[1] = indexBytes[currentPosition + 6];
                                                                tempLong[2] = indexBytes[currentPosition + 5];
                                                                tempLong[3] = indexBytes[currentPosition + 4];
                                                                tempLong[4] = indexBytes[currentPosition + 3];
                                                                tempLong[5] = indexBytes[currentPosition + 2];
                                                                tempLong[6] = indexBytes[currentPosition + 1];
                                                                tempLong[7] = indexBytes[currentPosition + 0];
                                                                //myDataRow[column.ColumnName] = Convert.ToInt64(BitConverter.ToInt64(tempLong, 0));
                                                                column.TestValue = BitConverter.ToInt64(tempLong, 0).ToString();
                                                            }
                                                            catch (Exception ex) {
                                                                column.TestValue = "0";
                                                                log.ErrorFormat("EntityID: " + entityID +
                                                                                     "\n column TypeName:" + column.TypeName +
                                                                                     "\n current position: " + currentPosition +
                                                                                     "\n index FReclen: " + t.FReclen +
                                                                                     "\n Message:" + ex.Message);
                                                                
                                                                throw new Exception(ex.Message);
                                                            }
                                                            break;
                                                        case "BIT":
                                                            try {
                                                                byte tempByte = indexBytes[currentPosition];
                                                                //Convert to Binary.
                                                                string binaryValue = Convert.ToString(tempByte, 2);

                                                                if (binaryValue.Length < 8) {
                                                                    binaryValue = binaryValue.PadLeft(8, '0');
                                                                }

                                                                if (column.ColumnName.ToUpper() == "MEAS_CLIM_REL") {
                                                                    column.TestValue = binaryValue[0].ToString();
                                                                    continue;
                                                                }
                                                                if (column.ColumnName.ToUpper() == "MEAS_PATH_SEL") {
                                                                    column.TestValue = binaryValue[1].ToString();
                                                                    continue;
                                                                }
                                                                if (column.ColumnName.ToUpper() == "MEAS_CLIM_DEVICE") {
                                                                    column.TestValue = binaryValue[2].ToString();
                                                                    continue;
                                                                }
                                                                if (column.ColumnName.ToUpper() == "FILLER1") {
                                                                    column.TestValue = binaryValue[3].ToString();
                                                                    continue;
                                                                }
                                                                if (column.ColumnName.ToUpper() == "FILLER2") {
                                                                    column.TestValue = binaryValue[4].ToString();
                                                                    continue;
                                                                }
                                                                if (column.ColumnName.ToUpper() == "FILLER3") {
                                                                    column.TestValue = binaryValue[5].ToString();
                                                                    continue;
                                                                }
                                                                if (column.ColumnName.ToUpper() == "FILLER4") {
                                                                    column.TestValue = binaryValue[6].ToString();
                                                                    continue;
                                                                }
                                                                if (column.ColumnName.ToUpper() == "FILLER5") {
                                                                    column.TestValue = binaryValue[7].ToString();
                                                                }
                                                            }
                                                            catch (Exception ex) {
                                                                column.TestValue = "0";
                                                                log.ErrorFormat("EntityID: " + entityID +
                                                                                     "\n column TypeName:" + column.TypeName +
                                                                                     "\n current position: " + currentPosition +
                                                                                     "\n index FReclen: " + t.FReclen +
                                                                                     "\n Message:" + ex.Message);
                                                                
                                                                throw new Exception(ex.Message);
                                                            }
                                                            break;
                                                        case "TINYINT":
                                                            try {
                                                                //myDataRow[column.ColumnName] = indexBytes[currentPosition];
                                                                column.TestValue = indexBytes[currentPosition].ToString();
                                                            }
                                                            catch (Exception ex) {
                                                                column.TestValue = "0";
                                                                log.ErrorFormat("EntityID: " + entityID +
                                                                                     "\n column TypeName:" + column.TypeName +
                                                                                     "\n current position: " + currentPosition +
                                                                                     "\n index FReclen: " + t.FReclen +
                                                                                     "\n Message:" + ex.Message);
                                                                
                                                                throw new Exception(ex.Message);
                                                            }
                                                            break;
                                                        case "NVARCHAR":
                                                            try {
                                                                string tempString = "";
                                                                for (int z = 0; z < column.TypeValue; z++) {
                                                                    if ((indexBytes[currentPosition] > 125) || (indexBytes[currentPosition] < 32) || (indexBytes[currentPosition] == 39)) {
                                                                        tempString += "";
                                                                    }
                                                                    else {
                                                                        tempString += Convert.ToChar(indexBytes[currentPosition + z]);
                                                                    }
                                                                }
                                                                //myDataRow[column.ColumnName] = tempString.Trim();
                                                                column.TestValue = tempString.Trim();
                                                            }
                                                            catch (Exception ex) {
                                                                column.TestValue = "";
                                                                log.ErrorFormat("EntityID: " + entityID +
                                                                                     "\n column TypeName:" + column.TypeName +
                                                                                     "\n current position: " + currentPosition +
                                                                                     "\n index FReclen: " + t.FReclen +
                                                                                     "\n Message:" + ex.Message);
                                                                
                                                                throw new Exception(ex.Message);
                                                            }
                                                            break;
                                                        case "DATETIME":
                                                            try {
                                                                tempLong[0] = indexBytes[currentPosition + 7];
                                                                tempLong[1] = indexBytes[currentPosition + 6];
                                                                tempLong[2] = indexBytes[currentPosition + 5];
                                                                tempLong[3] = indexBytes[currentPosition + 4];
                                                                tempLong[4] = indexBytes[currentPosition + 3];
                                                                tempLong[5] = indexBytes[currentPosition + 2];
                                                                tempLong[6] = indexBytes[currentPosition + 1];
                                                                tempLong[7] = indexBytes[currentPosition + 0];
                                                                long tempDate = Convert.ToInt64(BitConverter.ToInt64(tempLong, 0));
                                                                //Need to do / 10000 to get current julian time
                                                                tempDate /= 10000;

                                                                if (column.ColumnName == "FromTimestamp" && (entityID == 3 || entityID == 5)) {
                                                                    fromJulian = tempDate;
                                                                }
                                                                else if (column.ColumnName == "ToTimestamp" && (entityID == 3 || entityID == 5)) {
                                                                    toJulian = tempDate;
                                                                }

                                                                if (tempDate == 0) {
                                                                    if (entityID.Equals(22))
                                                                        column.TestValue = DateTime.Now.ToString();
                                                                    else
                                                                        column.TestValue = "";
                                                                }
                                                                else {
                                                                    var convert = new ConvertJulianTime();
                                                                    int obdTimeStamp = convert.JulianTimeStampToOBDTimeStamp(tempDate);
                                                                    DateTime dbDate = convert.OBDTimeStampToDBDate(obdTimeStamp);

                                                                    if (column.ColumnName == "FromTimestamp") {
                                                                        tableNameFromTimeStamp = dbDate;
                                                                    }

                                                                    //check if the entity is CPU and counter is FromTimestamp.
                                                                    if (entityID == 1 && (column.ColumnName.ToUpper().Trim() == "FROMTIMESTAMP" || column.ColumnName.ToUpper().Trim() == "TOTIMESTAMP")) {
                                                                        //Create a list of datetime according to interval.
                                                                        var intervalList = new Dictionary<DateTime, DateTime>();
                                                                        DateTime tempStartDate = dataStartDate;

                                                                        double tenPercent;
                                                                        if (column.ColumnName.ToUpper().Trim() == "FROMTIMESTAMP") {
                                                                            tenPercent = -0.1;
                                                                        }
                                                                        else {
                                                                            tenPercent = 0.1;
                                                                        }
                                                                        intervalList.Add(tempStartDate.AddSeconds(t.FInterval * tenPercent),
                                                                            tempStartDate.AddSeconds(t.FInterval).AddSeconds(t.FInterval * tenPercent));
                                                                        while (tempStartDate < dataStopDate) {
                                                                            tempStartDate = tempStartDate.AddSeconds(t.FInterval);
                                                                            intervalList.Add(tempStartDate.AddSeconds(t.FInterval * tenPercent),
                                                                                tempStartDate.AddSeconds(t.FInterval).AddSeconds(t.FInterval * tenPercent));
                                                                        }

                                                                        //Get foramtted From and To Timestamp.
                                                                        foreach (var kv in intervalList) {
                                                                            if (column.ColumnName.ToUpper().Trim() == "FROMTIMESTAMP") {
                                                                                if (dbDate >= kv.Key && dbDate <= kv.Value) {
                                                                                    dbDate = kv.Key.AddSeconds(t.FInterval * 0.1);
                                                                                    break;
                                                                                }
                                                                            }
                                                                            else {
                                                                                if (dbDate >= kv.Key && dbDate <= kv.Value) {
                                                                                    dbDate = kv.Value.AddSeconds(t.FInterval * -0.1);
                                                                                    break;
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                    //myDataRow[column.ColumnName] = dbDate;
                                                                    //for multiday use only. If toTimestamp is next day. substract one sec.
                                                                    if (column.ColumnName.ToUpper().Trim() == "TOTIMESTAMP" && dbDate.Hour == 0 && dbDate.Minute == 0 && dbDate.Second == 0) {
                                                                        dbDate = dbDate.AddSeconds(-1);
                                                                    }
                                                                    column.TestValue = dbDate.ToString();
                                                                }
                                                            }
                                                            catch (Exception ex) {
                                                                column.TestValue = DateTime.MinValue.ToString();
                                                                log.ErrorFormat("EntityID: " + entityID +
                                                                                     "\n column TypeName:" + column.TypeName +
                                                                                     "\n current position: " + currentPosition +
                                                                                     "\n index FReclen: " + t.FReclen +
                                                                                     "\n Message:" + ex.Message);
                                                                
                                                                throw new Exception(ex.Message);
                                                            }
                                                            break;
                                                    }

                                                    #endregion

                                                    currentPosition += column.TypeValue;
                                                }
                                                else {
                                                    //myDataRow[column.ColumnName] = DBNull.Value;
                                                    column.TestValue = "";
                                                }
                                            }

                                            //Create newline for each loop.
                                            if (myDataSet.Tables.Contains(tableNameFromTimeStamp.ToString("yyyy/MMM/dd"))) {
                                                DataRow myDataRow = myDataSet.Tables[tableNameFromTimeStamp.ToString("yyyy/MMM/dd")].NewRow();
                                                bool deleteRow = false;
                                                var tempFromtimestamp = DateTime.MinValue;

                                                foreach (var column in columnInfo) {
                                                    #region Switch
                                                    if (column.Website.Equals(false) && _remoteAnalyst) continue;

                                                    switch (column.TypeName) {
                                                        case "SMALLINT":
                                                            myDataRow[column.ColumnName] = Convert.ToInt16(column.TestValue);
                                                            break;
                                                        case "INT": //NOTE: VARIABLE COMING IN AS "SMALLINT" AND SAVED AS "INT"
                                                            if (column.ColumnName.ToUpper().Trim() == "UNIQUEID") {
                                                                //hard code the UniqueID to 1. this column is for our use.
                                                                myDataRow[column.ColumnName] = 1;
                                                            }
                                                            else {
                                                                myDataRow[column.ColumnName] = Convert.ToUInt16(column.TestValue);
                                                            }
                                                            break;
                                                        case "BIGINT": //NOTE: VARIABLE COMING IN AS "INT" AND SAVED AS "BIGINT"
                                                            if (column.ColumnName.ToUpper().Trim() == "UNIQUEID") {
                                                                //hard code the UniqueID to 1. this column is for our use.
                                                                myDataRow[column.ColumnName] = 1;
                                                            }
                                                            else {
                                                                myDataRow[column.ColumnName] = Convert.ToUInt32(column.TestValue);
                                                            }
                                                            break;
                                                        case "FLOAT":
                                                            myDataRow[column.ColumnName] = Convert.ToInt64(column.TestValue);
                                                            break;
                                                        case "BIT":
                                                            myDataRow[column.ColumnName] = Convert.ToByte(column.TestValue);
                                                            break;
                                                        case "TINYINT":
                                                            myDataRow[column.ColumnName] = Convert.ToByte(column.TestValue);
                                                            break;
                                                        case "NVARCHAR":
                                                            myDataRow[column.ColumnName] = column.TestValue;
                                                            break;
                                                        case "DATETIME":
                                                            if (column.ColumnName.ToUpper().Trim() == "FROMTIMESTAMP") {
                                                                if (Convert.ToDateTime(column.TestValue) >= dataStopDate) {
                                                                    deleteRow = true;
                                                                }
                                                                tempFromtimestamp = Convert.ToDateTime(column.TestValue);
                                                            }
                                                            else if (column.ColumnName.ToUpper().Trim() == "TOTIMESTAMP") {
                                                                //if (Convert.ToDateTime(column.TestValue) == oldDataStopDate) {
                                                                //    column.TestValue = dataStopDate.ToString();
                                                                //}//If ToTimestamp is greater than endtime, change the value to endtime.
                                                                if (column.TestValue.Length == 0) {
                                                                    //Use the FromTimestamp
                                                                    column.TestValue = tempFromtimestamp.ToString();
                                                                }
                                                                else {
                                                                    if (Convert.ToDateTime(column.TestValue) > dataStopDate) {
                                                                        column.TestValue = dataStopDate.ToString();
                                                                    }
                                                                }
                                                            }

                                                            myDataRow[column.ColumnName] = Convert.ToDateTime(column.TestValue);
                                                            break;
                                                    }

                                                    #endregion
                                                }

                                                //Per John's request, we are removing this if statment.
                                                /*if (myDataRow["FromTimestamp"].ToString().Equals(myDataRow["ToTimestamp"].ToString())) {
                                                    deleteRow = true;
                                                }*/

                                                if (deleteRow) {
                                                    //skip this row.
                                                }
                                                else {
                                                    //Add new row into the dataSet.
                                                    myDataSet.Tables[tableNameFromTimeStamp.ToString("yyyy/MMM/dd")].Rows.Add(myDataRow);
                                                    string tempTableName = UWSSerialNumber + "_" + t.FName.Trim().ToUpper() + "_" + tableNameFromTimeStamp.Year + "_" + tableNameFromTimeStamp.Month + "_" + tableNameFromTimeStamp.Day;

                                                    //Insert TableName for UniqueID.
                                                    if (!tableNames.Contains(tempTableName)) {
                                                        tableNames.Add(tempTableName);
                                                    }

                                                    //check to see if the row has more then BulkLoaderSize rows.
                                                    if (myDataSet.Tables[tableNameFromTimeStamp.ToString("yyyy/MMM/dd")].Rows.Count >
                                                        Constants.getInstance(connectionString).BulkLoaderSize) {
                                                        /*//Insert into the table.
                                                        //Request from Khody, do not load DISKFILE to MS SQL
                                                        if (_remoteAnalyst) {
                                                            if (!entityID.Equals(4) && !entityID.Equals(5)) {
                                                                var insertTables = new DataTableService(newConnectionString);
                                                                insertTables.InsertSPAMEntityDataFor(tempTableName, myDataSet.Tables[tableNameFromTimeStamp.ToString("yyyy/MMM/dd")], dicInfo.FullName);
                                                            }
                                                        }
                                                        else {
                                                            var insertTables = new DataTableService(newConnectionString);
                                                            insertTables.InsertSPAMEntityDataFor(tempTableName, myDataSet.Tables[tableNameFromTimeStamp.ToString("yyyy/MMM/dd")], dicInfo.FullName);
                                                        }*/

                                                        try {
                                                            //Insert into MySQL table.
                                                            var mySQLServices = new MySQLServices(_remoteAnalyst);
                                                            mySQLServices.InsertEntityDatas(UWSSerialNumber, tempTableName, myDataSet.Tables[tableNameFromTimeStamp.ToString("yyyy/MMM/dd")], connectionString, systemFolder, entityID);
                                                        }
                                                        catch (Exception ex) {
                                                            log.ErrorFormat("    -Insert into MySQL Error: {0}", ex.Message);
                                                            
                                                        }

                                                        //Clear the myDataTable.
                                                        myDataSet.Tables[tableNameFromTimeStamp.ToString("yyyy/MMM/dd")].Rows.Clear();

                                                        myDataSet = null;
                                                        GC.Collect();
                                                        myDataSet = CreateSPAMDataTableColumn(days, columnInfo);
                                                    }
                                                }
                                            }

                                            //Increase the start reading position.
                                            filePosition += recordLenth;
                                        }

                                        //Insert into the database.
                                        var tables = new DataTableService(newConnectionString);
                                        foreach (MultiDays d in days) {
                                            if (!d.DontLoad) {
                                                /*if (_remoteAnalyst) {
                                                    if (!entityID.Equals(4) && !entityID.Equals(5))
                                                        tables.InsertSPAMEntityDataFor(d.TableName, myDataSet.Tables[d.StartDate.ToString("yyyy/MMM/dd")], dicInfo.FullName);
                                                }
                                                else {
                                                    tables.InsertSPAMEntityDataFor(d.TableName, myDataSet.Tables[d.StartDate.ToString("yyyy/MMM/dd")], dicInfo.FullName);
                                                }*/

                                                try {
                                                    //Insert into MySQL table.
                                                    var mySQLServices = new MySQLServices(_remoteAnalyst);
                                                    mySQLServices.InsertEntityDatas(d.SystemSerial, d.TableName, myDataSet.Tables[tableNameFromTimeStamp.ToString("yyyy/MMM/dd")], connectionString,
                                                        systemFolder, entityID);
                                                }
                                                catch (Exception ex) {
                                                    log.ErrorFormat("    -Insert into MySQL Error: {0}", ex.Message);
                                                    
                                                }
                                            }
                                        }
                                        DateTime afterTime = DateTime.Now;
                                        TimeSpan timeSpan = afterTime - beforeTime;
                                        log.InfoFormat("    -(Line 1047) EntityID:{0}, Total Time in Minutes: {1}",
                                            entityID, timeSpan.TotalMinutes);
                                    }

                                    #endregion

                                    #region Update CurrentTable & TableTimestamp.

                                    #region Current Table
                                    if (currentTables.Count > 0) {
                                        var tempCurrentTable = new TempCurrentTablesService(newConnectionString);

                                        foreach (var d in currentTables) {
                                            try {
                                                //Insert basic info into the Current Table.
                                                currentTable.InsertEntryFor(d.TableName, d.EntityID, d.Interval, Convert.ToDateTime(d.StartDate.ToShortDateString()), d.SystemSerial, d.MeasureVersion);
                                                uwsInfo.CurrentTables.Add(new CurrentTables {
                                                    TableName = d.TableName,
                                                    EntityID = d.EntityID,
                                                    SystemSerial = d.SystemSerial,
                                                    Interval = d.Interval,
                                                    DataDate = Convert.ToDateTime(d.StartDate.ToShortDateString()),
                                                    MeasureVersion = d.MeasureVersion
                                                });
                                            }
                                            catch (Exception ex) {
                                                log.ErrorFormat("Insert CurrentTable: {0}", ex.Message);
                                                
                                            }

                                            try {
                                                //Delete from TempCurrentTable.
                                                tempCurrentTable.DeleteCurrentTableFor(d.TableName);
                                            }
                                            catch (Exception ex) {
                                                log.ErrorFormat("Delete TempCurrentTable: {0}", ex.Message);
                                                
                                            }
                                        }
                                        currentTables.Clear();
                                    }
                                    #endregion

                                    #region TableTimestamp
                                    if (tableTimestamps.Count > 0) {
                                        var tableTimeStamp = new TableTimeStampService(newConnectionString);
                                        var tempTimeStamp = new TempTableTimestampService(newConnectionString);

                                        foreach (var d in tableTimestamps) {
                                            try {
                                                //Insert basic info into the Current Table. - make default status "Active"
                                                tableTimeStamp.InsertEntryFor(d.TableName, d.StartDate, d.EndDate, (int)ArchiveStatus.Status.Active);
                                                tableNameList.Add(d.TableName);
                                                uwsInfo.TableTimestamps.Add(new TableTimestamp {
                                                    TableName = d.TableName,
                                                    Start = d.StartDate,
                                                    End = d.EndDate
                                                });
                                            }
                                            catch (Exception ex) {
                                                log.ErrorFormat("Insert TableTimeStamp: {0}", ex.Message);
                                                
                                            }
                                            try {
                                                //Delete from TempCurrentTable.
                                                tempTimeStamp.DeleteTempTimeStampFor(d.TableName);
                                            }
                                            catch (Exception ex) {
                                                log.ErrorFormat("Delete TempTimeStamp: {0}", ex.Message);
                                                
                                            }
                                        }
                                        tableTimestamps.Clear();
                                    }
                                    #endregion
                                    #endregion

                                }
                                catch (Exception ex) {
                                    log.ErrorFormat("EntityID: {0}.\n Message: {1}", entityID, ex.Message);
                                    throw new Exception(ex.Message);
                                }
                                finally {
                                    columnInfo = null;
                                    myDataSet.Dispose();
                                }
                            }
                        }
                    }
                }

                log.Info("******   Loading data completed   *******");
                

                #endregion

                try {
                    //Convert starttime and endtime.
                    if (uwsVersion == UWS.Types.Version2007 || uwsVersion == UWS.Types.Version2009) {
                        var julianTime = new ConvertJulianTime();
                        int lctTimeStamp = julianTime.JulianTimeStampToOBDTimeStamp(startTime);

                        startTimeLCT = julianTime.OBDTimeStampToDBDate(lctTimeStamp);
                        lctTimeStamp = julianTime.JulianTimeStampToOBDTimeStamp(stopTime);
                        stopTimeLCT = julianTime.OBDTimeStampToDBDate(lctTimeStamp);
                    }
                    else {
                        startTimeLCT = uwsStartDate;
                        stopTimeLCT = uwsStopDate;
                    }


                    //Round up the seconds.
                    TimeSpan span1 = stopTimeLCT - startTimeLCT;
                    double seconds1 = span1.TotalSeconds;

                    //Get remained seconds.
                    double remainSeconds1 = seconds1 % sampleInterval;
                    if (remainSeconds1 < sampleInterval * 0.1) {
                        stopTimeLCT = stopTimeLCT.AddSeconds(-remainSeconds1);
                    }

                    //If stopTimeLCT end at midnight, remove 1 second.
                    if (stopTimeLCT.Hour == 0 && stopTimeLCT.Minute == 0 && stopTimeLCT.Second == 0) {
                        stopTimeLCT = stopTimeLCT.AddSeconds(-1);
                    }

                    uwsInfo.StartDateTime = startTimeLCT;
                    uwsInfo.StopDateTime = stopTimeLCT;
                    uwsInfo.Interval = sampleInterval;

                    #region Update LoadingInfo.

                    if (_remoteAnalyst) {
                        log.Info("******  Update LoadingInfo  ********");
                        try {
                            var loadingInfo = new LoadingInfoService(connectionString);
                            if (systemName.Length > 8) systemName = systemName.Substring(0, 8);

                            var fileInfo = new FileInfo(uwsPath);
                            loadingInfo.UpdateFor(uwsID, fileInfo.Length, systemName, startTimeLCT, stopTimeLCT, 4);
                        }
                        catch (Exception ex) {
                            log.ErrorFormat("LoadingInfo Error: {0}", ex.Message);
                            
                        }
                        log.Info("******  Update LoadingInfo completed  ********");
                        
                    }

                    #endregion

                    #region Load FileTrend and populate SCM.

                    log.Info("******  Load FileTrend   *******");

                    if (fileEntity.Length > 0) {
                        #region File Trend
                        string cpuEntity = fileEntity.Replace("FILE", "CPU");
                        log.InfoFormat("fileEntity: {0}", fileEntity);
                        

                        try {
                            log.Info("Start File Trend Load to MySQL");


                            //Check if File Trend Table is exists.
                            DataContext dataContext = new DataContext(connectionString);
                            var fileTrendData = new MySQLDataBrowser.Model.FileTrendData(dataContext);
                            var newFileTableName = fileEntity.Replace("FILE", "FILETREND");
                            var fileTableExists = fileTrendData.CheckTableNameFor(newFileTableName);

                            log.InfoFormat("tableExists: {0}", fileTableExists);
                            

                            bool fileTrendDuplicate = false;
                            if (fileTableExists) {
                                log.InfoFormat("Check for Duplicate Data Table Name: {0}", fileEntity);
                                
                                fileTrendDuplicate = fileTrendData.CheckDuplicateFor(newFileTableName, startTimeLCT);

                                log.InfoFormat("fileTrendDuplicate: {0}", fileTrendDuplicate);
                                
                            }

                            if (!fileTrendDuplicate) {
                                //if (sp.TotalDays < 1) {
                                //Single day
                                var fileTrend = new 
                                    MySQLDataBrowser.Model.FileTrend(cpuEntity, 
                                    fileEntity, newConnectionString, 
                                    sampleInterval, startTimeLCT, stopTimeLCT, 
                                    saveLocation);
                                fileTrend.PopulateFileTrend();
                                //var fileTrendThread = new Thread(() => fileTrend.PopulateFileTrend()) { IsBackground = true };
                                //fileTrendThread.Start();
                            }
                        }
                        catch (Exception ex) {
                            log.Error("*******************************************************");
                            log.ErrorFormat("MySql Error: {0}", ex.Message);
                            
                        }
                        #endregion
                    }

                    log.Info("******  Load FileTrend and populate SCM completed *******");

                    #endregion

                    #region DISC Browser Load

                    log.Info("******  Load data for disk browser  *******");

                    var tables = new DataTableService(newConnectionString);
                    //var fileEntityTableExists = false; 
                    var processEntityTableExists = false;
                    var cpuTableName = "";
                    var discTableName = "";
                    //fileEntityTableExists = tables.CheckTableFor(fileEntity);
                    foreach (KeyValuePair<string, string> entry in entityList) {
                        string tableName = entry.Value;
                        if (tableName.Contains("["))
                            tableName = tableName.Substring(tableName.IndexOf('[') + 1);
                        if (tableName.Contains("]"))
                            tableName = tableName.Substring(0, tableName.Length - 1);

                        if (entry.Key.Equals("PROCESS")) {
                            processEntityTableExists = tables.CheckTableFor(tableName);
                        }
                        if (entry.Key.Equals("CPU")) {
                            cpuTableName = tableName;
                        }
                        if (entry.Key.Equals("DISK")) {
                            discTableName = tableName;
                        }
                    }
                    log.InfoFormat("processEntityTableExists: {0}", processEntityTableExists);
                    log.InfoFormat("cpuTableName: {0}", cpuTableName);
                    

                    //if (fileEntityTableExists && processEntityTableExists) {
                    if (processEntityTableExists) {
                        if (!cpuTableName.Equals("")) {
                            var diskBrowserLoader = new DiskBrowserLoader();
                            diskBrowserLoader.LoadDiskBrowserData(UWSSerialNumber, newConnectionString, connectionString, cpuTableName, discTableName, sampleInterval, systemFolder);
                        }
                    }

                    #endregion
                }
                catch (Exception ex) {
                    log.ErrorFormat("After SPAM Load Error: {0}", ex.Message);
                    
                }

                uwsInfo.Success = true;
            }
            catch (Exception ex) {
                log.ErrorFormat("EntityID: {0}. {1}", entityID, ex.Message);
                
                uwsInfo.Success = false;
                uwsInfo.ErrorMessage = ex.Message;

                //Check to see if I can update LoadingInfo.
                if (startTime != 0 && stopTime != 0) {
                    try {
                        //Convert starttime and endtime.
                        var julianTime = new ConvertJulianTime();
                        int lctTimeStamp = julianTime.JulianTimeStampToOBDTimeStamp(startTime);
                        startTimeLCT = julianTime.OBDTimeStampToDBDate(lctTimeStamp);

                        lctTimeStamp = julianTime.JulianTimeStampToOBDTimeStamp(stopTime);
                        stopTimeLCT = julianTime.OBDTimeStampToDBDate(lctTimeStamp);

                        //Round up the seconds.
                        TimeSpan span1 = stopTimeLCT - startTimeLCT;
                        double seconds1 = span1.TotalSeconds;

                        //Get remained seconds.
                        double remainSeconds1 = seconds1 % sampleInterval;

                        stopTimeLCT = stopTimeLCT.AddSeconds(-remainSeconds1);

                        var loadingInfo = new LoadingInfoService(connectionString);
                        systemName = systemName.Substring(0, 8);
                        var fileInfo = new FileInfo(uwsPath);

                        loadingInfo.UpdateFor(uwsID, fileInfo.Length, systemName, startTimeLCT, stopTimeLCT, 4);
                    }
                    catch (Exception ex1) {
                        log.ErrorFormat("LoadingInfo Error1: {0}", ex1.Message);
                        
                    }
                }

                //Update CurrentTable & TableTimestamp.
                if (currentTables.Count > 0) {
                    var tempCurrentTable = new TempCurrentTablesService(newConnectionString);
                    var tableTimestamp = new TableTimeStampService(newConnectionString);
                    var tempTableTimestamp = new TempTableTimestampService(newConnectionString);
                    //DataTables dataTable = new DataTables();
                    foreach (MultiDays d in currentTables) {
                        //Insert basic info into the Current Table.
                        currentTable.DeleteEntryFor(d.TableName);
                        //Delete from TempCurrentTable.
                        tempCurrentTable.DeleteCurrentTableFor(d.TableName);
                        //Delete from TableTimestamp.
                        tableTimestamp.DeleteEntryFor(d.TableName);
                        //Delete from TempTableTimestamp.
                        tempTableTimestamp.DeleteTempTimeStampFor(d.TableName);
                    }
                }
            }
            finally {
                // Delete Temp file.
                if (!uwsPath.Contains(".402") && success) {
                    if (File.Exists(uwsPath)) File.Delete(uwsPath);
                }

                if (!glacierLoad && uwsPath.Contains("DO") && File.Exists(uwsPath)) {
                    try {
                        log.Info("Delete DISCOPE FILE");
                        
                        if (File.Exists(uwsPath)) File.Delete(uwsPath);
                    }
                    catch (Exception ex) {
                        log.ErrorFormat("File Delete Error: {0}", ex.Message);
                    }
                }

                GC.Collect();
            }

            return uwsInfo;
        }

        private bool OpenNewUWSFile(string uwsPath, UWS.Types uwsVersion, ILog log, string connectionString) {
            //string UWSPath = Config.UWSPath;
            IHeaderInfo headerInfo = null;

            using (var stream = new FileStream(uwsPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                //using (StreamReader reader = new StreamReader(stream))
                using (reader = new BinaryReader(stream)) {
                    var myEncoding = new ASCIIEncoding();
                    if (uwsVersion == UWS.Types.Version2007) {
                        #region Basic Header Info

                        //Identifier
                        reader.BaseStream.Seek(0, SeekOrigin.Begin);
                        UwsIdentifierByte = reader.ReadBytes(UwsIdentifierByte.Length);
                        UwsIdentifier = myEncoding.GetString(UwsIdentifierByte).Trim();

                        //Key.
                        reader.BaseStream.Seek(8, SeekOrigin.Begin);
                        UwsKeyByte = reader.ReadBytes(UwsKeyByte.Length);
                        UwsKey = myEncoding.GetString(UwsKeyByte).Trim();

                        //System Serial
                        reader.BaseStream.Seek(18, SeekOrigin.Begin);
                        SystemSerialByte = reader.ReadBytes(SystemSerialByte.Length);
                        UWSSerialNumber = myEncoding.GetString(SystemSerialByte).Trim();
                        while (UWSSerialNumber.Length < 6) {
                            UWSSerialNumber = "0" + UWSSerialNumber;
                        }

                        //UwsHLen
                        reader.BaseStream.Seek(28, SeekOrigin.Begin);
                        UwsHLen = reader.ReadInt16();
                        UwsHLen = Helper.Reverse(UwsHLen);

                        //UwsHVersion
                        reader.BaseStream.Seek(30, SeekOrigin.Begin);
                        UwsHVersion = reader.ReadInt16();
                        UwsHVersion = Helper.Reverse(UwsHVersion);

                        //UwsXLen
                        reader.BaseStream.Seek(32, SeekOrigin.Begin);
                        UwsXLen = reader.ReadInt16();
                        UwsXLen = Helper.Reverse(UwsXLen);

                        //UwsXRecords
                        reader.BaseStream.Seek(34, SeekOrigin.Begin);
                        UwsXRecords = reader.ReadInt16();
                        UwsXRecords = Helper.Reverse(UwsXRecords);

                        //UwsSignatureTypeByte
                        reader.BaseStream.Seek(36, SeekOrigin.Begin);
                        UwsSignatureTypeByte = reader.ReadBytes(UwsSignatureTypeByte.Length);
                        UwsSignatureType = myEncoding.GetString(UwsSignatureTypeByte).Trim();

                        //UwsVersion
                        reader.BaseStream.Seek(54, SeekOrigin.Begin);
                        UwsVersion = reader.ReadInt32();
                        UwsVersion = Helper.Reverse(UwsVersion);

                        //UwsVstringByte
                        reader.BaseStream.Seek(58, SeekOrigin.Begin);
                        UwsVstringByte = reader.ReadBytes(UwsVstringByte.Length);
                        UwsVstring = myEncoding.GetString(UwsVstringByte).Trim();

                        //UwsSystemNameByte
                        reader.BaseStream.Seek(88, SeekOrigin.Begin);
                        UwsSystemNameByte = reader.ReadBytes(UwsSystemNameByte.Length);
                        UwsSystemName = myEncoding.GetString(UwsSystemNameByte).Trim();

                        //UwsCdataClassId
                        reader.BaseStream.Seek(98, SeekOrigin.Begin);
                        UwsCdataClassId = reader.ReadInt32();
                        UwsCdataClassId = Helper.Reverse(UwsCdataClassId);

                        //UwsCollectorVersion
                        reader.BaseStream.Seek(102, SeekOrigin.Begin);
                        UwsCollectorVersion = reader.ReadInt32();
                        UwsCollectorVersion = Helper.Reverse(UwsCollectorVersion);

                        //UwsCollectorVstringByte
                        reader.BaseStream.Seek(106, SeekOrigin.Begin);
                        UwsCollectorVstringByte = reader.ReadBytes(UwsCollectorVstringByte.Length);
                        UwsCollectorVstring = myEncoding.GetString(UwsCollectorVstringByte).Trim();

                        #endregion
                    }
                    else if (uwsVersion == UWS.Types.Version2009) {
                        #region Basic Header Info

                        //Key.
                        reader.BaseStream.Seek(0, SeekOrigin.Begin);
                        UwsKeyByte = reader.ReadBytes(UwsKeyByte.Length);
                        UwsKey = myEncoding.GetString(UwsKeyByte).Trim();

                        //System Serial
                        reader.BaseStream.Seek(10, SeekOrigin.Begin);
                        SystemSerialByte = reader.ReadBytes(SystemSerialByte.Length);
                        UWSSerialNumber = myEncoding.GetString(SystemSerialByte).Trim();
                        while (UWSSerialNumber.Length < 6) {
                            UWSSerialNumber = "0" + UWSSerialNumber;
                        }

                        //UwsHLen
                        reader.BaseStream.Seek(20, SeekOrigin.Begin);
                        UwsHLen = reader.ReadInt16();
                        UwsHLen = Helper.Reverse(UwsHLen);

                        //UwsXLen
                        reader.BaseStream.Seek(22, SeekOrigin.Begin);
                        UwsXLen = reader.ReadInt16();
                        UwsXLen = Helper.Reverse(UwsXLen);

                        //UwsXRecords
                        reader.BaseStream.Seek(24, SeekOrigin.Begin);
                        UwsXRecords = reader.ReadInt16();
                        UwsXRecords = Helper.Reverse(UwsXRecords);

                        //UwsSystemNameByte
                        reader.BaseStream.Seek(44, SeekOrigin.Begin);
                        UwsSystemNameByte = reader.ReadBytes(UwsSystemNameByte.Length);
                        UwsSystemName = myEncoding.GetString(UwsSystemNameByte).Trim();

                        //UwsVstringByte
                        reader.BaseStream.Seek(54, SeekOrigin.Begin);
                        UwsVstringByte = reader.ReadBytes(UwsVstringByte.Length);
                        UwsVstring = myEncoding.GetString(UwsVstringByte).Trim();

                        //Identifier
                        //reader.BaseStream.Seek(0, SeekOrigin.Begin);
                        //UwsIdentifierByte = reader.ReadBytes(UwsIdentifierByte.Length);
                        //UwsIdentifier = myEncoding.GetString(UwsIdentifierByte).Trim();

                        //UwsHVersion
                        //reader.BaseStream.Seek(30, SeekOrigin.Begin);
                        //UwsHVersion = reader.ReadInt16();
                        //UwsHVersion = Reverse(UwsHVersion);

                        //UwsSignatureTypeByte
                        //reader.BaseStream.Seek(36, SeekOrigin.Begin);
                        //UwsSignatureTypeByte = reader.ReadBytes(UwsSignatureTypeByte.Length);
                        //UwsSignatureType = myEncoding.GetString(UwsSignatureTypeByte).Trim();

                        //UwsVersion
                        //reader.BaseStream.Seek(54, SeekOrigin.Begin);
                        //UwsVersion = reader.ReadInt32();
                        //UwsVersion = Reverse(UwsVersion);

                        //UwsVstringByte
                        //reader.BaseStream.Seek(58, SeekOrigin.Begin);
                        //UwsVstringByte = reader.ReadBytes(UwsVstringByte.Length);
                        //UwsVstring = myEncoding.GetString(UwsVstringByte).Trim();

                        //UwsCdataClassId
                        //reader.BaseStream.Seek(98, SeekOrigin.Begin);
                        //UwsCdataClassId = reader.ReadInt32();
                        //UwsCdataClassId = Reverse(UwsCdataClassId);

                        //UwsCollectorVersion
                        //reader.BaseStream.Seek(102, SeekOrigin.Begin);
                        //UwsCollectorVersion = reader.ReadInt32();
                        //UwsCollectorVersion = Reverse(UwsCollectorVersion);

                        //UwsCollectorVstringByte
                        //reader.BaseStream.Seek(106, SeekOrigin.Begin);
                        //UwsCollectorVstringByte = reader.ReadBytes(UwsCollectorVstringByte.Length);
                        //UwsCollectorVstring = myEncoding.GetString(UwsCollectorVstringByte).Trim();

                        #endregion
                    }
                    else if (uwsVersion == UWS.Types.Version2013) {
                        //Read the VProc version.
                        reader.BaseStream.Seek(84, SeekOrigin.Begin);
                        NewCreatorVproc = reader.ReadBytes(NewCreatorVproc.Length);
                        UwsCreatorVproc = Helper.RemoveNULL(myEncoding.GetString(NewCreatorVproc).Trim());

                        //Get version Info.
                        if (_remoteAnalyst) {
                            var vProc = new VProcVersionService(connectionString);
                            string className = vProc.GetVProcVersionFor(UwsCreatorVproc);
                            if (className.Equals("HeaderInfoV1"))
                                headerInfo = new HeaderInfoV1();
                        }
                        else
                            headerInfo = new HeaderInfoV1();

                        headerInfo.ReadHeader(uwsPath, log, this);
                    }

                    #region Create Index

                    //Create Index.
                    //byte[] indexBytes = new byte[60];
                    var indexBytes = new byte[62];
                    var tempShortBytes = new byte[2];
                    var tempIntBytes = new byte[4];
                    var tempLongBytes = new byte[8];

                    int indexPosition = UwsHLen;
                    long tempLen = (Convert.ToInt64(UwsXRecords) * Convert.ToInt64(UwsXLen));
                    long dataPosition = indexPosition + tempLen;
                    //int UwsFileRecords = 0;

                    //List<Indices> index = new List<Indices>();
                    for (int x = 0; x < UwsXRecords; x++) {
                        var indexer = new Indices();

                        if (uwsVersion == UWS.Types.Version2007 || uwsVersion == UWS.Types.Version2009) {
                            #region Index
                            reader.BaseStream.Seek(indexPosition, SeekOrigin.Begin);
                            indexBytes = reader.ReadBytes(indexBytes.Length);

                            //Get Index Name (first 8 bytes).
                            indexer.FName = myEncoding.GetString(indexBytes, 0, 7);

                            //Index Type.
                            tempShortBytes[0] = indexBytes[9];
                            tempShortBytes[1] = indexBytes[8];
                            indexer.FType = Convert.ToInt16(BitConverter.ToInt16(tempShortBytes, 0));

                            //Index Length.
                            tempShortBytes[0] = indexBytes[11];
                            tempShortBytes[1] = indexBytes[10];
                            indexer.FReclen = Convert.ToInt16(BitConverter.ToInt16(tempShortBytes, 0));

                            //Index Dump Occurs.
                            tempIntBytes[0] = indexBytes[15];
                            tempIntBytes[1] = indexBytes[14];
                            tempIntBytes[2] = indexBytes[13];
                            tempIntBytes[3] = indexBytes[12];
                            indexer.FRecords = Convert.ToInt32(BitConverter.ToInt32(tempIntBytes, 0));

                            //Index System Name (10 bytes).
                            indexer.FSysName = myEncoding.GetString(indexBytes, 16, 9).Trim();

                            //Index Meas Version.
                            indexer.FMeasVer = myEncoding.GetString(indexBytes, 26, 3).Trim();

                            //Index Start Time.
                            tempLongBytes[0] = indexBytes[37];
                            tempLongBytes[1] = indexBytes[36];
                            tempLongBytes[2] = indexBytes[35];
                            tempLongBytes[3] = indexBytes[34];
                            tempLongBytes[4] = indexBytes[33];
                            tempLongBytes[5] = indexBytes[32];
                            tempLongBytes[6] = indexBytes[31];
                            tempLongBytes[7] = indexBytes[30];
                            //Need to do / 10000 to get current julian time
                            indexer.FStartTime = Convert.ToInt64(BitConverter.ToInt64(tempLongBytes, 0)) / 10000;

                            //Index Stop Time
                            tempLongBytes[0] = indexBytes[45];
                            tempLongBytes[1] = indexBytes[44];
                            tempLongBytes[2] = indexBytes[43];
                            tempLongBytes[3] = indexBytes[42];
                            tempLongBytes[4] = indexBytes[41];
                            tempLongBytes[5] = indexBytes[40];
                            tempLongBytes[6] = indexBytes[39];
                            tempLongBytes[7] = indexBytes[38];
                            //Need to do / 10000 to get current julian time
                            double temp = BitConverter.ToInt64(tempLongBytes, 0);
                            indexer.FStopTime = Convert.ToInt64(temp) / 10000;

                            //Index Interval.
                            tempLongBytes[0] = indexBytes[53];
                            tempLongBytes[1] = indexBytes[52];
                            tempLongBytes[2] = indexBytes[51];
                            tempLongBytes[3] = indexBytes[50];
                            tempLongBytes[4] = indexBytes[49];
                            tempLongBytes[5] = indexBytes[48];
                            tempLongBytes[6] = indexBytes[47];
                            tempLongBytes[7] = indexBytes[46];
                            indexer.FInterval = Convert.ToInt64(BitConverter.ToInt64(tempLongBytes, 0));

                            //Index Start Day.
                            tempIntBytes[0] = indexBytes[57];
                            tempIntBytes[1] = indexBytes[56];
                            tempIntBytes[2] = indexBytes[55];
                            tempIntBytes[3] = indexBytes[54];
                            indexer.FStartDay = Convert.ToInt32(BitConverter.ToInt32(tempIntBytes, 0));

                            //Index Stop Day.
                            tempIntBytes[0] = indexBytes[61];
                            tempIntBytes[1] = indexBytes[60];
                            tempIntBytes[2] = indexBytes[59];
                            tempIntBytes[3] = indexBytes[58];
                            indexer.FStopDay = Convert.ToInt32(BitConverter.ToInt32(tempIntBytes, 0));

                            //Index File Postion.
                            indexer.FilePosition = dataPosition;
                            #endregion
                        }
                        else {
                            indexer = headerInfo.ReaderEntityHeader(uwsPath, log, indexPosition, dataPosition);

                            //Other info that needs to sync the data.
                            indexer.FSysName = UwsSystemName;
                            indexer.FMeasVer = UwsMeasureDllVersion;
                        }

                        //Insert into the List.
                        index.Add(indexer);

                        indexPosition += UwsXLen;
                        tempLen = (Convert.ToInt64(indexer.FRecords) * Convert.ToInt64(indexer.FReclen));
                        dataPosition = dataPosition + tempLen;
                        //UwsFileRecords += indexer.FRecords;
                    }

                    #endregion
                }
            }

            return true;
        }

        private DataSet CreateSPAMDataTableColumn(List<MultiDays> days, IList<ColumnInfoView> columnInfo) {
            var myDataSet = new DataSet();
            //This DataTableName has be to start Date(only date part), because I have to compare with data's FromTimestamp.

            foreach (MultiDays d in days) {
                if (!d.DontLoad) {
                    string buildDataTableName = d.StartDate.ToString("yyyy/MMM/dd");
                    var myDataTable = new DataTable(buildDataTableName);

                    // Create new DataColumn, set DataType, ColumnName and add to DataTable.    
                    var myDataColumn = new DataColumn { DataType = Type.GetType("System.Int32"), ColumnName = "EntityCounterID" };
                    // Add the Column to the DataColumnCollection.
                    myDataTable.Columns.Add(myDataColumn);

                    foreach (var column in columnInfo) {
                        if (_websiteLoad && column.Website.Equals(false) && _remoteAnalyst) continue;

                        myDataColumn = new DataColumn {
                            DataType = Type.GetType(GetSystemValueType(column.TypeName)),
                            ColumnName = column.ColumnName
                        };
                        // Add the Column to the DataColumnCollection.
                        myDataTable.Columns.Add(myDataColumn);
                    }

                    myDataSet.Tables.Add(myDataTable);
                }
            }

            return myDataSet;
        }

        private string GetSystemValueType(string type) {
            string returnType;
            switch (type.ToUpper()) {
                case "DATETIME":
                    returnType = "System.DateTime";
                    break;
                case "FLOAT":
                    returnType = "System.Double";
                    break;
                case "SMALLINT":
                    returnType = "System.Int16";
                    break;
                case "INT":
                    returnType = "System.UInt16";
                    break;
                case "BIGINT":
                    returnType = "System.UInt32";
                    break;
                case "BIT":
                    returnType = "System.Byte";
                    break;
                case "TINYINT":
                    returnType = "System.Byte";
                    break;
                case "VARCHAR":
                    returnType = "System.String";
                    break;
                default:
                    returnType = "System.String";
                    break;
            }
            return returnType;
        }
    }
}
