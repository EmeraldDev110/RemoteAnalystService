using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;
using RemoteAnalyst.AWS.Infrastructure;
using RemoteAnalyst.AWS.Queue;
using RemoteAnalyst.AWS.S3;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.Scheduler.Schedules
{
    /// <summary>
    /// This class deletes disk files, log files, and UWS files (Pathway Data).
    /// </summary>
    internal class CleanupArchive
    {
        private readonly LoadingInfoService _loadingInfo = new LoadingInfoService(ConnectionString.ConnectionStringDB);
        private static readonly ILog Log = LogManager.GetLogger("Cleaner");

        /// <summary>
        /// Timer_Elapsed is a event that gets call by Scheduler to start the schedule task.
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="e">Timer ElapsedEventArgs</param>
        public void Timer_Elapsed(object source, System.Timers.ElapsedEventArgs e) {
            int currHour = DateTime.Now.Hour;

            if (currHour.Equals(5)) {
                DeleteArchive();
            }
        }

        /// <summary>
        /// DeleteArchive gets all the systems and call functions to delete files.
        /// </summary>
        public void DeleteArchive()
        {
            try
            {
                IDictionary<string, int> retentionDays = _loadingInfo.GetUWSRetentionDayFor();
                    var sysInfo = new System_tblService(ConnectionString.ConnectionStringDB);
                    List<string> expiredSystem = sysInfo.GetExpiredSystemFor(ConnectionString.IsLocalAnalyst);
                foreach (KeyValuePair<string, int> kv in retentionDays)
                {
                    try {
                            if (expiredSystem.Contains(kv.Key))
                            {
                                continue;   // Skip cleanup logic for expired systems. Should drop the database manually
                            }
                        CleanupArchiveFromLoadingInfo(kv.Key, kv.Value);
                        DeleteDiskData(kv.Key);
                        DeleteLogFile(kv.Key, kv.Value);
                    }
                    catch (Exception ex) {
                        Log.Error("*******************************************************");
                        Log.ErrorFormat("DeleteArchive Error: System {0}, {1}", kv.Key, ex);
                    }
                }
                DeletePathwayData();
                DeleteJobPool();
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("DeleteArchive Error: {0}", ex);
                if (!ConnectionString.IsLocalAnalyst) {
                    var amazon = new AmazonOperations();
                    amazon.WriteErrorQueue("DeleteArchive Error: " + ex.Message);
                }
            }
            finally
            {
                ConnectionString.TaskCounter--;
            }
        }

        /// <summary>
        /// DeleteDiskData deletes disk file.
        /// </summary>
        /// <param name="systemSerial">System Serial Number</param>
        private void DeleteDiskData(string systemSerial)
        {
            //This function is used for deleting the storage, it is using the 'TrendMonthsStorage' value in system_tbl
            //Get data information from LoadingInfoDisk.
            var loadingInfo = new LoadingInfoDiskService(ConnectionString.ConnectionStringDB);
            IList<LoadingInfoDiskView> diskServices = loadingInfo.GetUWSFileNameFor(systemSerial);
            var systemTblService = new System_tblService(ConnectionString.ConnectionStringDB);
            int trendMonths = systemTblService.GetTrendMonthsFor(systemSerial);
            Log.InfoFormat("Trend month: {0}", trendMonths);

            foreach (LoadingInfoDiskView service in diskServices)
            {
                DateTime uploadedTime = Convert.ToDateTime(service.UploadTime);
                string fileName = service.FileName;
                //Check date.
                DateTime retentionDate = uploadedTime.AddMonths(-trendMonths);
                Log.InfoFormat("retentionDate: {0}", retentionDate);
                
                if (retentionDate < DateTime.Today)
                {
                    //Build location and delete file.
                    string systemLocation = ConnectionString.SystemLocation;
                    string fileLocation = systemLocation + systemSerial + "\\" + fileName;

                    if (File.Exists(fileLocation))
                    {
                        Log.InfoFormat("Delete file: {0}", fileLocation);
                        File.Delete(fileLocation);
                        //Delete from LoadingInfoDisk.
                        Log.InfoFormat("Delete loading disk info: {0}", service.DiskUWSID);
                        loadingInfo.DeleteLoadingInfoDiskFor(service.DiskUWSID);
                    }
                }
            }
        }

        private void DeletePathwayData() {
            DateTime currentDateTime = DateTime.Today;
            var service = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
            IDictionary<string, string> connections = service.GetAllConnectionStringFor();
            var systemTblService = new System_tblService(ConnectionString.ConnectionStringDB);
                
            foreach (KeyValuePair<string, string> kv in connections) {
                //Using DB connection to find a list of table names
                var pathwayService = new PathwayService(kv.Value);
                var tablesToDelete = pathwayService.GetListOfPathwayTablesFor();
                int trendMonths = systemTblService.GetTrendMonthsFor(kv.Key);
                DateTime oldDate = currentDateTime.AddMonths(-trendMonths);
                Log.InfoFormat("Tables to delete: {0}", string.Join(",", tablesToDelete));

                try
                {
                    foreach (var tableName in tablesToDelete) {
                        pathwayService.DeleteDataFor(oldDate, tableName);
                    }
                }
                catch (Exception ex) {
                    Log.Error("*******************************************************");
                    Log.ErrorFormat("DeletePathwayData System: {0}, {1}", kv.Key, ex);
                }
            }
        }

        /// <summary>
        /// DeleteLogFile deletes log files within System folder.
        /// </summary>
        /// <param name="systemSerial">System Serial Number</param>
        /// <param name="uwsRetentionDay">System Retention Day</param>
        private void DeleteLogFile(string systemSerial, int uwsRetentionDay)
        {
            string systemLocation = ConnectionString.SystemLocation;
            string fileLocation = systemLocation + systemSerial;

            try
            {
                string[] files = Directory.GetFiles(fileLocation);
                foreach (string s in files)
                {
                    DateTime createdDate = File.GetCreationTime(s);
                    
                    DateTime retentionDate = createdDate.AddDays(1);    //Delete all the file after one day.

                    if (retentionDate < DateTime.Today)
                    {
                        if (File.Exists(s))
                        {
                            File.Delete(s);
                        }
                    }
                }

                var directories = Directory.GetDirectories(fileLocation);
                foreach (var d in directories) {
                    var di = new DirectoryInfo(d);
                    if (di.CreationTime.AddDays(1) < DateTime.Today) {
                        di.Delete(true);
                    }
                }
            }
            catch
            {
                //skip
            }
        }

        /// <summary>
        /// CleanupArchiveFromLoadingInfo calls DeleteUWS to delte UWS file and update LoadingInfo
        /// </summary>
        /// <param name="systemSerial">System Serial Number</param>
        /// <param name="uwsRetentionDay">System Retention Day</param>
        private void CleanupArchiveFromLoadingInfo(string systemSerial, int uwsRetentionDay)
        {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            //If retentinoday is 0, get the system default from RAInfo table.
            if (uwsRetentionDay.ToString() == "0")
            {
                //UWSRetentionDay
                var raInfo = new RAInfoService(ConnectionString.ConnectionStringDB);
                uwsRetentionDay = Convert.ToInt32(raInfo.GetQueryValueFor("UWSRetentionDay"));
                //This is hard coded value for getting default Retention Day.
            }

            //Calcualte the RetentionDay.
            DateTime dtEndDate = DateTime.Today;
            dtEndDate = dtEndDate.AddDays(-uwsRetentionDay);

            //Delete the uws file.
            DeleteUWS(systemSerial, dtEndDate);

            _loadingInfo.UpdateFileStatFor(systemSerial, dtEndDate);
        }

        /// <summary>
        /// DeleteUWS delte UWS file
        /// </summary>
        /// <param name="systemSerial">System Serial Number</param>
        /// <param name="dtEndDate">System Retention Date</param>
        private void DeleteUWS(string systemSerial, DateTime dtEndDate)
        {
            List<string> fileNames = _loadingInfo.GetUWSFileNameFor(systemSerial, dtEndDate);

            foreach (string fileName in fileNames) {

                if (File.Exists(fileName))
                    File.Delete(fileName);
                else {
                    try {
                        if (!fileName.Contains("\\Systems\\" + systemSerial + "\\")) {
                            if (ConnectionString.IsLocalAnalyst) {
                                var networkLocation = ConnectionString.NetworkStorageLocation + "Systems/" + systemSerial + "/" + fileName;
                                if(File.Exists(networkLocation))
                                    File.Delete(networkLocation);
                            }
                            else {
                                //Try adding system path.
                                if (File.Exists(ConnectionString.ServerPath + "\\" + fileName))
                                    File.Delete(ConnectionString.ServerPath + "\\" + fileName);
                                else {
                                    //Delete from the S3.
                                    string buildS3Key = "Systems/" + systemSerial + "/" + fileName;
                                    var s3 = new AmazonS3(ConnectionString.S3FTP);
                                    s3.DeleteS3(buildS3Key);
                                }
                            }
                        }
                    }
                    catch (Exception ex) {
                        if (!ConnectionString.IsLocalAnalyst) {
                            if (!string.IsNullOrEmpty(ConnectionString.SQSError)) {
                                var sqs = new AmazonSQS();
                                string urlQueue = sqs.GetAmazonSQSUrl(ConnectionString.SQSError);
                                sqs.WriteMessage(urlQueue, "S3 Delete Error:" + ex.Message);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Delete old files from JobPool Folder
        /// </summary>
        private void DeleteJobPool() {
            var jobPool = ConnectionString.WatchFolder;

            try {
                var files = Directory.GetFiles(jobPool);
                foreach (var s in from s in files let createdDate = File.GetCreationTime(s) where createdDate < DateTime.Today where File.Exists(s) select s) {
                    File.Delete(s);
                }
            }
            catch {
                //skip
            }
        }
    }
}