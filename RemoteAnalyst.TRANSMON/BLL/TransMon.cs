using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using Amazon;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Amazon.RDS;
using Amazon.RDS.Model;
using log4net;
using RemoteAnalyst.AWS.Queue;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using RemoteAnalyst.TransMon.TransMonFactoryPattern.Model;
using RemoteAnalyst.TransMon.TransMonFactoryPattern.Service;

namespace RemoteAnalyst.TransMon.BLL {
    public class TransMon {
        private static readonly ILog Log = LogManager.GetLogger("TransMonLog");

        public void TimerRunTransMon_Elapsed(object source, ElapsedEventArgs e) {
            //Populate the TMonTomorrow table every hour, each time need to check if it is 23:59
            if (DateTime.Now.Hour.Equals(23)) {
                Log.Info("******** Scheduler is populating TMonTomorrow table data for the next day ********* ");
                Log.InfoFormat("Populating time is: {0}", DateTime.Now);
                CreateJobsTomorrow();
                Log.Info("******** Summarizing LoadHistory ********* ");
                SummarizeLoadHistory();
            }
            else {
                Log.InfoFormat("Current time is {0}, not yet 23:59, scheduler won't populate TMonTomorrow table.", DateTime.Now);
            }
        }

        public void RunTransMon() {
            //Before populating TMonTomorrow table, delete first
            var tMonTomorrow = new TMonTomorrow(ConnectionString.ConnectionStringDB);
            var tMonTomorrowService = new TMonTomorrowService(tMonTomorrow);
            tMonTomorrowService.DeleteJobsTomorrowFor();

            //Populate the TMonTomorrow table with a list of jobs (runs shortly after midnight 23:59)
            CreateJobsTomorrow();

            //Read TMonTomorrow table, start populating the TMonComplete and TMonDelay tables
            ProcessJobsTomorrow();
        }

        public void DeleteJobsTomorrow() {
            Log.InfoFormat("TransMon started at: {0}", DateTime.Now);
            var tMonTomorrow = new TMonTomorrow(ConnectionString.ConnectionStringDB);
            var tMonTomorrowService = new TMonTomorrowService(tMonTomorrow);
            try {
                tMonTomorrowService.DeleteJobsTomorrowFor();
            }
            catch (Exception ex) {
                Log.ErrorFormat("Error occurred when deleting entries from TMonTomorrow table. {0}", ex);
            }
        }

        public void CreateJobsTomorrow() {
            Log.InfoFormat("Populating TMonTomorrow table at {0}", DateTime.Now);
            try {
                var tMonTomorrow = new TMonTomorrow(ConnectionString.ConnectionStringDB);
                var tMonTomorrowService = new TMonTomorrowService(tMonTomorrow);
                var tMonSchedule = new TMonSchedule(ConnectionString.ConnectionStringDB);
                var tMonScheduleService = new TMonScheduleService(tMonSchedule);
                List<TMonScheduleView> tMonScheduleViews = tMonScheduleService.GetTMonSchedulesFor();
                var tMonTomorrowViews = new List<TMonTomorrowView>();

                foreach (TMonScheduleView tMonScheduleView in tMonScheduleViews) {
                    string systemSerial = tMonScheduleView.SystemSerial;
                    string firstTransmissionTime = tMonScheduleView.FirstTransmissionTime;
                    string transSchedule = tMonScheduleView.TransSchedule;
                    char activeFlag = tMonScheduleView.ActiveFlag;

                    if (activeFlag == 'A') {
                        if (transSchedule.Equals("I")) {
                            //Intraday
                            //If hourly, calculate the times based on interval
                            int interval = tMonScheduleView.Interval;
                            int intervalHour = interval / 60;
                            List<DateTime> expectedTimeList = GetExpectedTime(firstTransmissionTime, intervalHour);
                            foreach (DateTime time in expectedTimeList) {
                                var tMonTomorrowView = new TMonTomorrowView {
                                    ExpectedTime = time,
                                    SystemSerial = systemSerial
                                };
                                tMonTomorrowViews.Add(tMonTomorrowView);
                            }
                        }
                        else if (transSchedule.Equals("D")) {
                            //Daily
                            var tMonTomorrowView = new TMonTomorrowView {
                                ExpectedTime = GetExpectedTime(firstTransmissionTime),
                                SystemSerial = systemSerial
                            };
                            tMonTomorrowViews.Add(tMonTomorrowView);
                        }
                        else {
                            //Weekly
                            string weekDays = tMonScheduleView.WeekDays;
                            int numberOfDay = weekDays.IndexOf("1", StringComparison.Ordinal) + 1;

                            if (numberOfDay <= GetNumOfDay()) {
                                var tMonTomorrowView = new TMonTomorrowView {
                                    ExpectedTime = GetExpectedTime(firstTransmissionTime),
                                    SystemSerial = systemSerial
                                };
                                tMonTomorrowViews.Add(tMonTomorrowView);
                            }
                        }
                    }
                }
                List<TMonTomorrowView> filteredTMonTomorrowViews = tMonTomorrowViews.Where(view => view.ExpectedTime >= DateTime.Now).ToList();
                tMonTomorrowService.PopulateTMonTomorrowFor(filteredTMonTomorrowViews, ConnectionString.ServerPath);
                Log.InfoFormat("Finish Populating TMonTomorrow table at {0}", DateTime.Now);
            }
            catch (Exception ex) {
                Log.ErrorFormat("Exception occurred when deleting entries from TMonTomorrow table. {0}", ex);
            }
        }

        public void ProcessJobsTomorrow() {
            var tMonTomorrow = new TMonTomorrow(ConnectionString.ConnectionStringDB);
            var tMonTomorrowService = new TMonTomorrowService(tMonTomorrow);
            Log.InfoFormat("Process TMonTomorrow jobs at {0}", DateTime.Now);
            try {
                var tMonFileName = new TMonFileNames(ConnectionString.ConnectionStringDB);
                var tMonFileNamesService = new TMonFileNamesService(tMonFileName);

                //Wait until the current time equals to first entry 
                TMonTomorrowView first = tMonTomorrowService.GetExpectedTimeFor();
                //Assume current time is always less than first entry
                var diffInSeconds = (int)(first.ExpectedTime - DateTime.Now).TotalMilliseconds;
                Log.InfoFormat("First entry time is : {0}|{1}", first.SystemSerial, first.ExpectedTime);
                Log.InfoFormat("Time difference : {0} seconds", diffInSeconds / 1000);
                if (diffInSeconds > 0) {
                    Thread.Sleep(diffInSeconds);
                }
                int sleepInterval = 0;
                var preTime = new DateTime(first.ExpectedTime.Year, first.ExpectedTime.Month, first.ExpectedTime.Day, 0, 0, 0);

                while (true) {
                    TMonTomorrowView tMonTomorrowView = tMonTomorrowService.GetExpectedTimeFor();
                    //The timer should populate between 23:00:00 to 23:59:59
                    //The worsest case: 23:59:59, if TransMon starts at hh:59:59
                    while (string.IsNullOrEmpty(tMonTomorrowView.SystemSerial)) {
                        Log.Info("No entries in TMonTomorrow table yet or exception occurred, sleep for 1 minute");
                        Thread.Sleep(60 * 1000);
                        tMonTomorrowView = tMonTomorrowService.GetExpectedTimeFor();
                    }

                    string systemSerial = tMonTomorrowView.SystemSerial;
                    DateTime currTime = tMonTomorrowView.ExpectedTime;
                    //Get the hour minute string
                    string currTimeStr = currTime.ToString("HHmm");
                    tMonTomorrowService.DeleteExpectedTimeFor(currTime, systemSerial);
                    string partialFileName = tMonFileNamesService.GetExpectedFileNameFor(systemSerial, currTimeStr.Substring(0, 2)); //use hour string
                    Log.InfoFormat("Reading one entry: systemSerial: {0}, expectedTime: {1}, fileName: {2}",
                        systemSerial, currTimeStr, partialFileName);
                    //Calculate the sleep interval based on the time difference between previous expectedTime and current expectedTime
                    //let the main thread sleep
                    sleepInterval = (int)GetSleepInterval(preTime, currTime);
                    Log.InfoFormat("preTime is: {0}, currTime is: {1}", preTime, currTime);
                    Log.InfoFormat("Before reading next entry: sleep for {0} seconds", sleepInterval);
                    Thread.Sleep(sleepInterval * 1000);
                    Log.InfoFormat("Start processing at {0}", DateTime.Now);

                    //Start the new thread for each job (row)
                    if (partialFileName.Length == 1) {
                        partialFileName = "0" + partialFileName;
                    }
                    /*var checks = new Checks();
                    var checkThread = new Thread(() => checks.CheckFile(currTimeStr, currTime, partialFileName, systemSerial.Trim()));
                    checkThread.Start();*/
                    //TransMon v2
                    StartChecks(currTimeStr, currTime, partialFileName, systemSerial);
                    preTime = tMonTomorrowView.ExpectedTime;
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("Exception occurred when processing TransMon {0}", ex);
            }
        }

        public List<DateTime> GetExpectedTime(string firstTransmission, int interval) {
            int hour = int.Parse(firstTransmission.Substring(0, 2));
            int min = int.Parse(firstTransmission.Substring(2, 2));
            var expectedTimeList = new List<DateTime>();
            DateTime currentDateTime = DateTime.Now;

            //Check if the hour is 23
            if (DateTime.Now.Hour == 23) {
                //Get the time list for tomorrow
                hour = 0;
                currentDateTime = currentDateTime.AddDays(1);
            }

            //Time list for today
            for (int i = hour; i <= 23; i = i + interval) {
                expectedTimeList.Add(new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, i, min, 0));
            }

            return expectedTimeList;
        }

        public DateTime GetExpectedTime(string firstTransmission) {
            int hour = int.Parse(firstTransmission.Substring(0, 2));
            int min = int.Parse(firstTransmission.Substring(2, 2));
            return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, min, 0);
        }

        public int GetNumOfDay() {
            string today = DateTime.Now.DayOfWeek.ToString();
            switch (today) {
                case "Monday":
                    return 1;
                case "Tuesday":
                    return 2;
                case "Wednesday":
                    return 3;
                case "Thursday":
                    return 4;
                case "Friday":
                    return 5;
                case "Saturday":
                    return 6;
                case "Sunday":
                    return 7;
            }
            return 0;
        }

        public double GetSleepInterval(DateTime preTime, DateTime CurrTime) {
            if (preTime.Hour == 0 && preTime.Minute == 0 && preTime.Second == 0) {
                return 0;
            }
            return CurrTime < DateTime.Now ? 0 : (CurrTime - preTime).TotalSeconds;
        }

        public void StartChecks(string expectedTime, DateTime expectedTimeValue, string fileName, string systemSerial) {
            var checks = new Checks2 {
                Email = new Email(),
                LoadingInfoService = new LoadingInfoService(ConnectionString.ConnectionStringDB),
                System_tblService = new System_tblService(ConnectionString.ConnectionStringDB)
            };
            var tMonComplete = new TMonComplete(ConnectionString.ConnectionStringDB);
            checks.TMonCompleteService = new TMonCompleteService(tMonComplete);
            var tMonDelay = new TMonDelay(ConnectionString.ConnectionStringDB);
            checks.TMonDelayService = new TMonDelayService(tMonDelay);
            var tMonSchedule = new TMonSchedule(ConnectionString.ConnectionStringDB);
            checks.TMonScheduleService = new TMonScheduleService(tMonSchedule);
            var checkThread = new Thread(() => checks.CheckFile(expectedTime, expectedTimeValue, fileName, systemSerial.Trim()));
            checkThread.Start();
        }

#if !DEBUG
        public void ReloadFailedFiles_Elapsed(object source, ElapsedEventArgs e)
        {
#else
        public void ReloadFailedFiles_Elapsed() {
#endif
            Log.Info("Reload check Start");
            try
            {
                if (string.IsNullOrEmpty(ConnectionString.SQSLoad))
                {
                    Log.InfoFormat("Reload check early exit at {0} since SQSLoadQ string was not specified (null or empty)", DateTime.Now);
                    return;
                }
                CheckReloadFilesStuckInQ();
                CheckReloadFilesStuckInProcessing();

            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Reload check Overall Error at {0}", ex);
            }
            Log.Info("Reload check Completed");
        }

        private void ReloadFiles(DataTable reloadFiles)
        {
            var email = new Email();
            var amazonSqs = new AmazonSQS();
            var queueURL = amazonSqs.GetAmazonSQSUrl(ConnectionString.SQSLoad);
            var loadingInfo = new LoadingInfo(ConnectionString.ConnectionStringDB);
            foreach (DataRow reloadFile in reloadFiles.Rows)
            {
                try
                {
                    string filename = Convert.ToString(reloadFile["filename"]);
                    string systemSerial = Convert.ToString(reloadFile["SystemSerial"]);
                    string uwsId = Convert.ToString(reloadFile["uwsId"]);
                    var message = new StringBuilder();
                    message.Append("SYSTEM\n").Append(systemSerial + "\n").Append("Systems/" + systemSerial + "/" + filename + "\n").Append(uwsId);
                    var messageId = amazonSqs.WriteMessageWithGroupName(queueURL, message.ToString(), systemSerial);
                    Log.InfoFormat("Reload attempt for: System Serial = {0} - FileName = {1} UWSID = {2} messageId = {3}",
                        systemSerial, filename, uwsId, messageId);
                    Log.InfoFormat("Message to SQS: queueURL {0} message {1}", queueURL, message.ToString());
                    loadingInfo.UpdateReloadTime(systemSerial, uwsId, filename, DateTime.Now);
                    email.CreateSendErrorEmail("Attempt made to reloaded the following file: \r\n<br>" +
                                               "FileName: " + filename + "\r\n<br>" +
                                               "SystemSerial: " + systemSerial + "\r\n<br>" +
                                               "UWSID: " + uwsId + "\r\n<br>" +
                                               "Please check the Loading Info table for more details.", "");
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Reload check Error processing failed loads {0}: {1} {2}",
                                        DateTime.Now, ex.Message, ex.StackTrace);
                }
            }
        }

        private void CheckReloadFilesStuckInQ()
        {
            var loadingInfo = new LoadingInfo(ConnectionString.ConnectionStringDB);
            var currentTime = DateTime.Now;
            var currentHourTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, 0, 0);
            var failedLoadTable = loadingInfo.GetLoadFailedInfo(currentHourTime);
            ReloadFiles(failedLoadTable);
        }

        private void CheckReloadFilesStuckInProcessing()
        {
            var loadingInfo = new LoadingInfo(ConnectionString.ConnectionStringDB);
            DataTable inProcessingLoads = loadingInfo.GetInProcessingLoad();
            DataTable loadHistory = loadingInfo.GetLoadHistory();
            DateTime now = DateTime.Now;
            var raInfo = new RAInfoService(ConnectionString.ConnectionStringDB);
            var activeCheckIntervalInSecondsDBValue = raInfo.GetValueFor("LoadActiveCheckIntervalInSeconds");
            var activeCheckIntervalInSeconds = activeCheckIntervalInSecondsDBValue == "" ? 0 : Convert.ToInt32(activeCheckIntervalInSecondsDBValue);

            DataTable reloadFiles = new DataTable();
            reloadFiles.Columns.Add("filename", typeof(String));
            reloadFiles.Columns.Add("SystemSerial", typeof(String));
            reloadFiles.Columns.Add("uwsId", typeof(String));
            reloadFiles.Columns.Add("LastChecked", typeof(DateTime));

            // Generate delayed processing set
            foreach (DataRow inProcessingLoad in inProcessingLoads.Rows)
            {
                var lh = loadHistory.AsEnumerable().Where(x => x.Field<string>("systemserial") == (string)inProcessingLoad["SystemSerial"]).Select(x => x).FirstOrDefault();
                if (lh == null) continue;
                var processingTime = now.Subtract(Convert.ToDateTime(inProcessingLoad["StartProcessingTime"])).TotalMinutes;
                var expectedTime = Convert.ToDouble(lh["averageloadtime"]) * (1 + (Convert.ToDouble(lh["allowancefactor"]) / 100));
                if (processingTime > expectedTime)
                {
                    DataRow reloadFile = reloadFiles.NewRow();
                    reloadFile["SystemSerial"] = inProcessingLoad["SystemSerial"];
                    reloadFile["filename"] = inProcessingLoad["FileName"];
                    reloadFile["uwsId"] = inProcessingLoad["TempUWSID"];
                    reloadFile["InstanceID"] = inProcessingLoad["InstanceID"];
                    reloadFiles.Rows.Add(reloadFile);
                }
            }

            if (reloadFiles.Rows.Count > 0) {
                DetermineReload(reloadFiles);
                Thread.Sleep(activeCheckIntervalInSeconds * 1000);
                DetermineReload(reloadFiles);
                if (reloadFiles.Rows.Count > 0)
                {
                    ReloadFiles(reloadFiles);
                }
            }
        }

        private void DetermineReload(DataTable reloadFiles)
        {
            // Get EC2 information
            var monitorService = new MonitorService(ConnectionString.ConnectionStringDB, Log);
            var databaseMapping = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
            DataTable ec2LoaderIPInformation = monitorService.GetEC2LoaderIPInformation();
            foreach (DataRow reloadFile in reloadFiles.Rows)
            {
                var ec2Info = ec2LoaderIPInformation.AsEnumerable().Where(x => x.Field<string>("InstanceId") == (string)reloadFile["InstanceID"]).Select(x => x).FirstOrDefault();
                if (ec2Info == null)
                {
                    reloadFiles.Rows.Remove(reloadFile);
                    continue;
                }

                var systemConnectionString = databaseMapping.GetConnectionStringFor((string)reloadFile["SystemSerial"]);
                var dbAdministrator = new DBAdministratorService(systemConnectionString);
                bool isActive = dbAdministrator.IsActive((string)reloadFile["SystemSerial"], (string)ec2Info["IPAddress"]);
                if (isActive)
                {
                    reloadFiles.Rows.Remove(reloadFile);
                }
            }
        }

#if !DEBUG
        public void CheckFiles_Elapsed(object source, ElapsedEventArgs e) {
#else
        public void CheckFiles_Elapsed() {
#endif
            Log.InfoFormat("Start at {0}", DateTime.Now);
            try {
                /* RA-1500
                var inProgressHour = -2;
                */
                var transmonService = new TransMonFactoryPattern.Service.TransmonService();
                var transmonList = transmonService.GetTransmonsFor();

                if (transmonList.Count > 0) {
#if !DEBUG
                    var adjustedTime = ConvertedTime(DateTime.Now);
#else
                    var adjustedTime = ConvertedTime(Convert.ToDateTime("2019-12-18 15:00:00"));
#endif
                    var loadingInfo = new LoadingInfo(ConnectionString.ConnectionStringDB);
                    var transmonEmail = new Email();
                    var systemTable = new System_tblService(ConnectionString.ConnectionStringDB);
                    
                    try {
                        foreach (var transmonView in transmonList) {
                            //Convert adjustedTime to local time.
                            //Get System's Time.
                            int timeZoneIndex = systemTable.GetTimeZoneFor(transmonView.SystemSerial);
                            var localTime = TimeZoneInformation.ToLocalTime(timeZoneIndex, adjustedTime);
                            //Get Company Name and System Name.
                            var systemTbl = new System_tblService(ConnectionString.ConnectionStringDB);
                            transmonView.SystemName = systemTbl.GetSystemNameFor(transmonView.SystemSerial);
                            transmonView.CompanyName = systemTbl.GetCompanyNameFor(transmonView.SystemSerial);

                            //Build search time range.
                            var startTime = localTime.AddMinutes(transmonView.AllowanceTimeInMinutes * -1).AddSeconds((transmonView.IntervalInMinutes * 60) * -0.01);
                            var stopTime = localTime.AddMinutes(transmonView.AllowanceTimeInMinutes * -1).AddMinutes(transmonView.IntervalInMinutes).AddSeconds((transmonView.IntervalInMinutes * 60) * 0.01);

                            if (transmonView.IntervalInMinutes == 15) {
                                var loadingInfoDataTable = loadingInfo.GetLoadingInfo(transmonView.SystemSerial, startTime, stopTime);
                                transmonView.LoadedFileCount = loadingInfoDataTable.Rows.Count;

                                transmonView.LoadStarted = loadingInfoDataTable.AsEnumerable().Any(x => x.Field<DateTime?>("startloadtime") != null);

                                if (loadingInfoDataTable.Rows.Count < transmonView.ExpectedFileCount || loadingInfoDataTable.AsEnumerable().Any(x => x.Field<DateTime?>("loadedtime") == null)) {
                                    var tempDataTable = new DataTable();
                                    if (loadingInfoDataTable.AsEnumerable().Any(x => x.Field<DateTime?>("loadedtime") == null))
                                        tempDataTable = loadingInfoDataTable.AsEnumerable().Where(x => x.Field<DateTime?>("loadedtime") == null).CopyToDataTable();

#if !DEBUG
                                    transmonEmail.SendFileCountEmail(transmonView, startTime, stopTime, tempDataTable);
#endif
                                }
                            }
                            else if (transmonView.IntervalInMinutes == 60) {
                                if (localTime.Minute == 0) {
                                    var loadingInfoDataTable = loadingInfo.GetLoadingInfo(transmonView.SystemSerial, startTime, stopTime);
                                    //transmonView.LoadedFileCount = loadingInfoDataTable.Rows.Count;
                                    transmonView.LoadedFileCount = loadingInfoDataTable.AsEnumerable().Count(x => x.Field<DateTime?>("loadedtime") != null);
                                    transmonView.InProgressFileCount = loadingInfoDataTable.AsEnumerable().Count(x => x.Field<DateTime?>("loadedtime") == null);
                                    transmonView.TotalFileSize = loadingInfoDataTable.AsEnumerable().Sum(x => x.Field<long?>("filesize"));

                                    if (transmonView.InProgressFileCount > 0) {
                                        var tempDataTable = loadingInfoDataTable.AsEnumerable().Where(x => x.Field<DateTime?>("loadedtime") == null).CopyToDataTable();
                                        
                                    }

                                    if (loadingInfoDataTable.Rows.Count < transmonView.ExpectedFileCount || loadingInfoDataTable.AsEnumerable().Any(x => x.Field<DateTime?>("loadedtime") == null)) {
                                        var tempDataTable = new DataTable();
                                        if (loadingInfoDataTable.AsEnumerable().Any(x => x.Field<DateTime?>("loadedtime") == null))
                                            tempDataTable = loadingInfoDataTable.AsEnumerable().Where(x => x.Field<DateTime?>("loadedtime") == null).CopyToDataTable();
#if !DEBUG
                                        transmonEmail.SendFileCountEmail(transmonView, startTime, stopTime, tempDataTable);
#endif
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex) {
                        Log.ErrorFormat("File Count Error {0}", ex);
                    }
                    var driveView = new List<DriveView>();
                    if (adjustedTime.Minute == 0) {
                        var inProgressSummaryDataTable = new DataTable();
                        //New logic for Summary. Use the Server time. use loadedTime for loaded and FTPReceivedTime (-2 hours from local time) for In Progress.
                        var summaryStartTime = adjustedTime.AddHours(-1);
                        var summaryStopTime = adjustedTime;
						var transmonLogs = new TransmonLogService(ConnectionString.ConnectionStringDB);
						var systemsAndResidual = transmonLogs.GetSystemResidual(adjustedTime.AddHours(-1));
						foreach (var transmonView in transmonList) {
                            if (transmonView.SystemName == null) {
                                if (transmonView.SystemName.Length == 0) {
                                    //Get Company Name and System Name.
                                    var systemTbl = new System_tblService(ConnectionString.ConnectionStringDB);
                                    transmonView.SystemName = systemTbl.GetSystemNameFor(transmonView.SystemSerial);
                                    transmonView.CompanyName = systemTbl.GetCompanyNameFor(transmonView.SystemSerial);
									transmonView.ResidualFromLastInterval = systemsAndResidual[transmonView.SystemSerial];
                                }
                            }
                            var loadedInfoDataTable = loadingInfo.GetLoadedInfo(transmonView.SystemSerial, summaryStartTime, summaryStopTime);
                            
                            if (transmonView.IntervalInMinutes == 15) {
                                transmonView.ExpectedFileCount = transmonView.ExpectedFileCount * 4;
                            }
                            transmonView.TotalFileSize = loadedInfoDataTable.AsEnumerable().Sum(x => x.Field<long?>("filesize"));
                            transmonView.LoadedFileCount = loadedInfoDataTable.AsEnumerable().Count(x => x.Field<DateTime?>("loadedtime") != null);
                            
                            //Get InProgress Data
                            /*
                             * RA-1500
                            var inProressStartTime = summaryStartTime.AddHours(inProgressHour);
                            var inProgressInfoDataTable = loadingInfo.GetInProgressInfo(transmonView.SystemSerial, inProressStartTime, summaryStopTime);
                            */
                            var inProgressInfoDataTable = loadingInfo.GetInProgressInfo(transmonView.SystemSerial, summaryStartTime, summaryStopTime);
                            transmonView.InProgressFileCount = inProgressInfoDataTable.Rows.Count;
 
                            if (inProgressInfoDataTable.Rows.Count > 0) {
                                if (inProgressSummaryDataTable.Rows.Count > 0)
                                    inProgressSummaryDataTable.Merge(inProgressInfoDataTable);
                                else
                                    inProgressSummaryDataTable = inProgressInfoDataTable;
                            }
                        }

                        try {
                            //Get storage info.
                            var allDrives = DriveInfo.GetDrives();
                            driveView = (from d in allDrives
                                where d.IsReady && d.Name != @"Z:\"
                                select new DriveView {
                                    VolumeLabel = d.Name,
                                    TotalSize = Math.Round(d.TotalSize / Convert.ToDouble((1024 * 1024 * 1024)), 2),
                                    TotalFreeSpace = Math.Round(d.TotalFreeSpace / Convert.ToDouble((1024 * 1024 * 1024)), 2)
                                }).ToList();

                            //Send email to Support and Khody.
                            transmonEmail.SendHourSummary(transmonList.OrderBy(x => x.CompanyName).ToList(), driveView, inProgressSummaryDataTable, summaryStartTime, summaryStopTime);

                            //Save the data into database.                           
                            transmonLogs.Insert(ConnectionString.SystemLocation, adjustedTime, transmonList);
                        }
                        catch (Exception ex) {
                            Log.ErrorFormat("Hourly Summary Error {0}", ex);
                        }
                    }
                    else {
                        //Check disk space.
                        try {
                            //Get storage info.
                            var allDrives = DriveInfo.GetDrives();
                            driveView = (from d in allDrives
                                where d.IsReady && d.Name != @"Z:\"
                                select new DriveView {
                                    VolumeLabel = d.Name,
                                    TotalSize = Math.Round(d.TotalSize / Convert.ToDouble((1024 * 1024 * 1024)), 2),
                                    TotalFreeSpace = Math.Round(d.TotalFreeSpace / Convert.ToDouble((1024 * 1024 * 1024)), 2),
                                    PercentUsed = ((Math.Round(d.TotalSize / Convert.ToDouble((1024 * 1024 * 1024)), 2) -
                                                    Math.Round(d.TotalFreeSpace / Convert.ToDouble((1024 * 1024 * 1024)), 2)) /
                                                   Math.Round(d.TotalSize / Convert.ToDouble((1024 * 1024 * 1024)), 2)) * 100
                                }).ToList();

                            if (driveView.Any(x => x.PercentUsed > 70.00)) {
                                transmonEmail.SendFTPStorageOver70Percent(driveView);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.ErrorFormat("SendFTPStorageOver70Percent Error {0}", ex);
                        }
                    }
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("Overall Error {0}", ex);
            }
        }

        private int GetRDSAllocatedStorage(string databaseName) {
            var c = new AmazonRDSClient(RegionEndpoint.USWest2);
            var request = new DescribeDBInstancesRequest();
            var response = c.DescribeDBInstances(request);
            var storage = response.DBInstances.Where(x => x.DBInstanceIdentifier == databaseName).Select(x => x.AllocatedStorage).FirstOrDefault();

            return storage;
        }

        internal double GetRDSCpuBusy(string databaseName) {
            var client = new AmazonCloudWatchClient(RegionEndpoint.USWest2);

            var dimension = new Dimension {
                Name = "DBInstanceIdentifier",
                Value = databaseName
            };

            var request = new GetMetricStatisticsRequest {
                Dimensions = new List<Dimension> { dimension },
                MetricName = "CPUUtilization",
                Namespace = "AWS/RDS",
                // Get statistics by day.
                //Period = 1800,
                Period = 60,
                // Get statistics for the past 15 mins.
                StartTime = DateTime.UtcNow - TimeSpan.FromMinutes(2),
                EndTime = DateTime.UtcNow,
                Statistics = new List<string>() { /*"Average"*/ "Maximum" },
                Unit = StandardUnit.Percent
            };

            var response = client.GetMetricStatistics(request);

            var avgCpuBusy = 0d;
            if (response.Datapoints.Count > 0) {
                foreach (var point in response.Datapoints) {
                    avgCpuBusy += point.Maximum;
                }
            }

            return avgCpuBusy;
        }

        private double GetRDSFreeSpace(string databaseName) {
            var client = new AmazonCloudWatchClient(RegionEndpoint.USWest2);

            var dimension = new Dimension {
                Name = "DBInstanceIdentifier",
                Value = databaseName
            };

            var request = new GetMetricStatisticsRequest {
                Dimensions = new List<Dimension> { dimension },
                MetricName = "FreeStorageSpace",
                Namespace = "AWS/RDS",
                // Get statistics by day.   
                Period = 1800,
                // Get statistics for the past month.
                StartTime = DateTime.UtcNow - TimeSpan.FromMinutes(15),
                EndTime = DateTime.UtcNow,
                Statistics = new List<string>() { "Average" },
                Unit = StandardUnit.Bytes
            };

            var response = client.GetMetricStatistics(request);

            var totalFreeSpace = 0d;
            if (response.Datapoints.Count > 0) {
                foreach (var point in response.Datapoints) {
                    totalFreeSpace += point.Average;
                }
            }

            return totalFreeSpace / 1024 / 1024 / 1024;
        }
        private DateTime ConvertedTime(DateTime currentTime) {
            var currentMinute = currentTime.Minute;
            var convertedMin = 0;
            if (currentMinute >= 0 && currentMinute < 15)
                convertedMin = 0;
            else if (currentMinute >= 15 && currentMinute < 30)
                convertedMin = 15;
            else if (currentMinute >= 30 && currentMinute < 45)
                convertedMin = 30;
            else if (currentMinute >= 45 && currentMinute < 60)
                convertedMin = 45;

            return new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, convertedMin, 0);
        }

        private void SummarizeLoadHistory()
        {
            Log.InfoFormat("{0}: In SummarizeLoadHistory ... ", DateTime.Now);
            var loadingInfoService = new LoadingInfo(ConnectionString.ConnectionStringDB);
            DateTime now = DateTime.Now;

            DataTable loadHistory = loadingInfoService.GetLoadHistory();

            DateTime lastUpdatedAt = Convert.ToDateTime(loadHistory.AsEnumerable().Min(x => x["updatedat"]));
            DataTable loadHistoryCurrent = loadingInfoService.GetLoadHistoryForTransmonList(lastUpdatedAt);
            if (loadHistoryCurrent == null) {
                Log.InfoFormat("{0}: No current load history", DateTime.Now);
            }
            else { 
                foreach (DataRow loadHistoryCurrentRow in loadHistoryCurrent.Rows)
                { 
                    var lh = loadHistory.AsEnumerable().
                                Where(x => x.Field<string>("systemserial") == (string)loadHistoryCurrentRow["systemserial"]).
                                Select(x => x).FirstOrDefault();
                    if (lh == null)
                    {
                        lh = loadHistory.NewRow();
                        lh["systemserial"] = loadHistoryCurrentRow["systemserial"];
                        lh["averagefilesize"] = loadHistoryCurrentRow["averagefilesize"];
                        lh["totalfiles"] = loadHistoryCurrentRow["totalfiles"];
                        lh["averageloadtime"] = loadHistoryCurrentRow["averageloadtime"];
                        lh["updatedat"] = DateTime.Now;
                        lh["allowancefactor"] = 30;
                        lh["ignorehistory"] = false;
                        loadHistory.Rows.Add(lh);
                    }
                    else
                    {
                        lh["averagefilesize"] = 
                            ((Convert.ToDouble(loadHistoryCurrentRow["averagefilesize"]) * Convert.ToDouble(loadHistoryCurrentRow["totalfiles"])) +
                            (Convert.ToDouble(lh["averagefilesize"]) * Convert.ToDouble(lh["totalfiles"]))) / 
                                (Convert.ToDouble(loadHistoryCurrentRow["totalfiles"]) + Convert.ToDouble(lh["totalfiles"]));
                        lh["totalfiles"] = (Convert.ToDouble(loadHistoryCurrentRow["totalfiles"]) + Convert.ToDouble(lh["totalfiles"]));
                        lh["averageloadtime"] =
                            ((Convert.ToDouble(loadHistoryCurrentRow["averageloadtime"]) * Convert.ToDouble(loadHistoryCurrentRow["totalfiles"])) +
                            (Convert.ToDouble(lh["averageloadtime"]) * Convert.ToDouble(lh["totalfiles"]))) /
                                (Convert.ToDouble(loadHistoryCurrentRow["totalfiles"]) + Convert.ToDouble(lh["totalfiles"]));
                        lh["updatedat"] = DateTime.Now;
                    }
                }
            }
            try
            {
                Log.InfoFormat("{0}: Updating LoadHistory", DateTime.Now);
                loadingInfoService.BulkInsertMySQL(loadHistory, "LoadHistory");
            }
            catch(Exception e)
            {
                Log.ErrorFormat("{0}:{1}:{2}", DateTime.Now, e.Message, e.StackTrace);
            }
            Log.InfoFormat("{0}: SummarizeLoadHistory done", DateTime.Now);
        }
    }
}