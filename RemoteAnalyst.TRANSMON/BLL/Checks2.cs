using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using log4net;
using RemoteAnalyst.AWS.Queue;
using RemoteAnalyst.AWS.S3;
using RemoteAnalyst.BusinessLogic.BLL;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.TransMon.BLL {
    internal class Checks2 {
        private static readonly ILog Log = LogManager.GetLogger("ServiceCheck");
        private readonly int currentThreadId = Thread.CurrentThread.ManagedThreadId; //used for differenetiate different files, since one thread one file each day
        private AmazonS3Client client;
        private Email email;
        private LoadingInfoService loadingInfoService;
        private List<S3ObjectView> s3Objects;
        private System_tblService systemTbl;
        private TMonCompleteService tMonCompleteService;
        private TMonDelayService tMonDelayService;
        private TMonScheduleService tMonScheduleService;
        
        public LoadingInfoService LoadingInfoService {
            set {
                if (loadingInfoService != null && loadingInfoService == value) {
                    return;
                }
                loadingInfoService = value;
            }
        }

        public Email Email {
            set {
                if (email != null && email == value) {
                    return;
                }
                email = value;
            }
        }

        public System_tblService System_tblService {
            set {
                if (systemTbl != null && systemTbl == value) {
                    return;
                }
                systemTbl = value;
            }
        }

        public TMonCompleteService TMonCompleteService {
            set {
                if (tMonCompleteService != null && tMonCompleteService == value) {
                    return;
                }
                tMonCompleteService = value;
            }
        }

        public TMonDelayService TMonDelayService {
            set {
                if (tMonDelayService != null && tMonDelayService == value) {
                    return;
                }
                tMonDelayService = value;
            }
        }

        public TMonScheduleService TMonScheduleService {
            set {
                if (tMonScheduleService != null && tMonScheduleService == value) {
                    return;
                }
                tMonScheduleService = value;
            }
        }

        //Main function for TransMon
        public void CheckFile(string expectedTime, DateTime expectedTimeValue, string fileName, string systemSerial) {
            
            Log.Info("***************************************************************");
            Log.InfoFormat("Start checking files at {0} current thread: {1}", DateTime.Now, currentThreadId);
            Log.InfoFormat("systemSerial: {0}, expectedTime: {1}, expectedTimeValue: {2}, fileName: {3}, current thread: {4}",
                systemSerial, expectedTime, expectedTimeValue, fileName, currentThreadId);

            string fullFileName = "";
            try {
                //Check the table first, if in table, still need to check the value of loadedTime.
                bool isFileNameInTable = CheckTables(expectedTime, fileName, systemSerial, false);

                if (isFileNameInTable) {
                    fullFileName = loadingInfoService.GetUMPFullFileNameFor(fileName, systemSerial, false);
                    Log.InfoFormat("File {0} is in LoadingInfo table, current thread: {1}", fullFileName, currentThreadId);

                    bool isDelayed = IsUploadedTimeNull(expectedTime, fileName, systemSerial, expectedTimeValue);

                    if (isDelayed) {
                        Log.InfoFormat("File {0} is delayed at {1}, insert into TransMonDelay table, current thread: {2}",
                            fileName, DateTime.Now, currentThreadId);
                        tMonDelayService.InsertLog(expectedTime, systemSerial, DateTime.Now, fullFileName);
                    }
                    else {
                        Log.InfoFormat("File {0} is completed at {1}, insert into TransMonComplete table, current thread: {2}",
                            fileName, DateTime.Now, currentThreadId);
                        string loadedTime = loadingInfoService.GetLoadCompleteTimeFor(fileName, systemSerial);
                        DateTime lTime = DateTime.Parse(loadedTime);
                        tMonCompleteService.InsertLog(expectedTime, systemSerial, lTime, fullFileName);
                    }
                }
                else {
                    Log.InfoFormat("File {0} is not in LoadingInfo table, check S3 bucket. current thread: {1}",
                        fileName, currentThreadId);
                    bool isInBucket = CheckAWSS3Bucket(expectedTime, expectedTimeValue, fileName, systemSerial);
                    Log.InfoFormat("File in bucket? {0}", isInBucket);
                    int possibleDelay = tMonScheduleService.GetDelayFor(systemSerial);
                    int loadTimeAllowed = tMonScheduleService.GetLoadTimeFor(systemSerial);
                    DateTime timeAllowed = expectedTimeValue.AddMinutes(possibleDelay).AddMinutes(loadTimeAllowed);
                    string systemName = systemTbl.GetSystemNameFor(systemSerial);

                    #region Not in bucket
                    //Keep checking the S3 bucket, until the file is there or time is up
                    while (!isInBucket) {
                        if (timeAllowed > DateTime.Now) {
                            Log.InfoFormat("timeAllowed: {0}, currentTime {1} current thread: {2}",
                                timeAllowed, DateTime.Now, currentThreadId);
                            Log.InfoFormat("Not in table, not in bucket, still have time, sleep for 10 mins current thread: {0}",
                                currentThreadId);
                            Thread.Sleep(600000);
                            isInBucket = CheckAWSS3Bucket(expectedTime, expectedTimeValue, fileName, systemSerial);
                            Log.InfoFormat("Thread wakes up and check again, isInBucket ? {0}", isInBucket);
                        }
                        else {
                            //Check System's current table to see if we got the data.
                            var databaseMappingService = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
                            string connectionStringSystem = databaseMappingService.GetConnectionStringFor(systemSerial);
                            var dailySysUnrated = new DailySysUnratedService(connectionStringSystem);
                            var cpuBusy =  dailySysUnrated.CheckHourlyDataFor(systemSerial, DateTime.Now.Date, Convert.ToInt32(fileName));

                            if (cpuBusy < 0) {
                                Log.InfoFormat("Is not in the bucket, time passed allowance. current thread: {0}", currentThreadId);
                                email.CreateSendErrorEmail("Below tranmission is delayed: \r\n" +
                                                           "SystemSerial: " + systemSerial + "\r\n<br>" +
                                                           "SystemName: " + systemName + "\r\n<br>" +
                                                           "ExpectedTime: " + expectedTimeValue + "\r\n<br>" +
                                                           "TimeAllowance: " + timeAllowed + "\r\n<br>" +
                                                           "ExpectedFileName: " + fileName + "\r\n<br>" +
                                                           "Reason: The file is missing.", "");
                                Log.InfoFormat("File {0} is not in the bucket, " +
                                    "insert into TransMonDelay table currentthread: {1}" +
                                    "Delay at: {2}",
                                    fileName, currentThreadId, DateTime.Now);
                                tMonDelayService.InsertLog(expectedTime, systemSerial, DateTime.Now, fullFileName);
                            }
                            return;
                        }
                    }

                    #endregion

                    //Till here, the file is in the bucket
                    CheckInBucket(expectedTime, expectedTimeValue, fileName, systemSerial, systemName, timeAllowed);
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("Exception in CheckFile function: {0}", ex);
            }
            finally {
                Log.InfoFormat("Check file completed!");
                Log.InfoFormat("--------------------------------------------------------------");
                Log.InfoFormat("--------------------------------------------------------------");
            }
        }

        public void CheckInBucket(string expectedTime, DateTime expectedTimeValue, string fileName, string systemSerial, string systemName, DateTime timeAllowed) {
            Log.InfoFormat("***************************************************************");
            string fullFileName = "";

            #region In bucket

            Log.InfoFormat("Start Check in bucket current thread: {0}", currentThreadId);
            S3ObjectView obj = s3Objects.First();
            if (obj.key.Contains("RA")) {
                fullFileName = obj.key.Substring(obj.key.IndexOf("RA", StringComparison.Ordinal));
            }
            else if (obj.key.Contains("ZA")) {
                fullFileName = obj.key.Substring(obj.key.IndexOf("ZA", StringComparison.Ordinal));
            }
            else if (obj.key.Contains("DO")) {
                fullFileName = obj.key.Substring(obj.key.IndexOf("DO", StringComparison.Ordinal));
            }
            Log.InfoFormat("fullFileName: " + fullFileName);

            Log.InfoFormat("Check delay");
            bool isDelayed = CheckUMPFileS3(obj, expectedTime, fileName, systemSerial, expectedTimeValue);

            if (isDelayed) {
                Log.InfoFormat("File in S3, file size still increasing and is delayed " + ", current thread: " + currentThreadId);
                email.CreateSendErrorEmail("Below tranmission is delayed: \r\n<br>" +
                                           "SystemSerial: " + systemSerial + "\r\n<br>" +
                                           "SystemName: " + systemName + "\r\n<br>" +
                                           "ExpectedTime: " + expectedTime + "\r\n<br>" +
                                           "Reason: The file was not in LoadingInfo table, the file was in bucket, but the file size was still increasing and the time passed allowance.", "");
                Log.InfoFormat("File " + fileName + " is delayed, insert into TransMonDelay table currentthread: " + currentThreadId);
                Log.InfoFormat("Delay time: " + DateTime.Now);
                
                tMonDelayService.InsertLog(expectedTime, systemSerial, DateTime.Now, fullFileName);
                return;
            }

            //Till here, the file size is not increasing for sure (delayed or not delayed)
            Log.InfoFormat("File size not increasing, not delayed, check table again current thread: {0}", currentThreadId);
            bool isFileNameInTable2 = CheckTables(expectedTime, fileName, systemSerial, false);
            if (isFileNameInTable2) {
                Log.InfoFormat("File is in table currentthread: " + currentThreadId);
                Log.InfoFormat("Check delay");
                bool isDelayed2 = IsUploadedTimeNull(expectedTime, fileName, systemSerial, expectedTimeValue);

                if (isDelayed2) {
                    Log.InfoFormat("File {0} is delayed, insert into TransMonDelay table currentthread: {1}", 
                        fileName, currentThreadId);
                    tMonDelayService.InsertLog(expectedTime, systemSerial, DateTime.Now, fullFileName);
                }
                else {
                    Log.InfoFormat("File is in the table, insert into TransMonComplete table currentthread: {0}", currentThreadId);
                    string loadedTime = loadingInfoService.GetLoadCompleteTimeFor(fileName, systemSerial);
                    DateTime lTime = DateTime.Parse(loadedTime);
                    tMonCompleteService.InsertLog(expectedTime, systemSerial, lTime, fullFileName);
                }
            }
            else {
                Log.InfoFormat("File size not increasing, not delayed, not in table, sleep until expected time currentthread: {0}", currentThreadId);
                

                //Sleep until expected time
                if (timeAllowed > DateTime.Now) {
                    Log.InfoFormat("The file is not delayed, but it is in S3, and sleep until the expected time and check again.currentthread: " + currentThreadId);
                    Log.InfoFormat("Time allowed: {0} sleep for {1}, seconds currentthread: {2}",
                        timeAllowed, (timeAllowed - DateTime.Now).TotalSeconds, currentThreadId);
                    
                    Thread.Sleep(((int)(timeAllowed - DateTime.Now).TotalSeconds) * 1000);
                }
                //Check table again
                //If Thread wakes up, and it is already next day, need to substract one day from current day,
                //otherwise the query won't find the file name in LoadingInfo table, so here just compare the expectedTimeValue's date and current date
                bool jumpToNextDay = false;
                if (expectedTimeValue.Date < DateTime.Today) {
                    Log.InfoFormat("After thread wakes up, it's another day, expectedTimeValue: {0}", expectedTimeValue);
                    jumpToNextDay = true;
                }

                bool isFileNameInTable3 = CheckTables(expectedTime, fileName, systemSerial, jumpToNextDay);
                if (!isFileNameInTable3) {
                    SendToSQS(expectedTime, fullFileName, systemSerial);
                    Log.InfoFormat("thread wakes up, Send Message to SQS **********");
                    Log.InfoFormat("System Serial = {0} - ExpectedTime = {1} current thread: {2}",
                        systemSerial, expectedTimeValue, currentThreadId);
                    email.CreateSendErrorEmail("Below tranmission is sent to SQS: \r\n<br>" +
                                               "SystemSerial: " + systemSerial + "\r\n<br>" +
                                               "SystemName: " + systemName + "\r\n<br>" +
                                               "ExpectedTime: " + expectedTimeValue + "\r\n<br>" +
                                               "TimeAllowance: " + timeAllowed + "\r\n<br>" +
                                               "ExpectedFileName: " + fileName + "\r\n<br>" +
                                               "Sent Message to SQS.", "");
                    Log.InfoFormat("File {0} is not in the table, insert into TransMonDelay table currentthread: {1}",
                                    fileName, Thread.CurrentThread.ManagedThreadId);
                    Log.InfoFormat("Delay at: " + DateTime.Now);
                    tMonDelayService.InsertLog(expectedTime, systemSerial, DateTime.Now, fullFileName);
                }
                else {
                    Log.InfoFormat("File {0} is in the table after sleeping to expected time, insert into TransMonComplete table currentthread: {1}",
                                    fileName, Thread.CurrentThread.ManagedThreadId);
                    tMonCompleteService.InsertLog(expectedTime, systemSerial, DateTime.Now, fullFileName);
                }
            }

            #endregion
        }

        public bool CheckTables(string expectedTime, string fileName, string systemSerial, bool jumpToNextDay) {
            Log.InfoFormat("***************************************************************");
            Log.InfoFormat("Check file name in loadingInfo table current thread: {0}", currentThreadId);
            Log.InfoFormat("systemSerial = {0} expectedTime = {1} fileName = {2} current thread: {3}",
                systemSerial, expectedTime, fileName, currentThreadId);
            Log.InfoFormat("");
            try {
                //Check if any record in LoadingInfo table
                bool loadingInfoExisting = CheckLoadingInfoTable(expectedTime, fileName, systemSerial, jumpToNextDay);
                if (loadingInfoExisting) {
                    return true;
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("Error occurred when checking loading info table {0}", ex);
            }
            finally {
                Log.InfoFormat("Check table done currentthread: {0}", Thread.CurrentThread.ManagedThreadId);
            }
            return false;
        }

        public bool CheckLoadingInfoTable(string expectedTime, string fileName, string systemSerial, bool jumpToNextDay) {
            string fullFileName = loadingInfoService.GetUMPFullFileNameFor(fileName, systemSerial, jumpToNextDay);
            return fullFileName != null && !fullFileName.Equals("");
        }

        public bool IsUploadedTimeNull(string expectedTime, string fileName, string systemSerial, DateTime expectedTimeValue) {
            Log.InfoFormat("***************************************************************");
            Log.InfoFormat("Start checking loadedTime column value current thread: {0}", currentThreadId);
            int possibleDelay = tMonScheduleService.GetDelayFor(systemSerial);
            int loadTimeAllowed = tMonScheduleService.GetLoadTimeFor(systemSerial);
            bool isLoadCompleted = false;
            DateTime cTime = expectedTimeValue;
            DateTime timeLimit = expectedTimeValue.AddMinutes(loadTimeAllowed).AddMinutes(possibleDelay);
            Log.InfoFormat("cTime (expectedTimeValue) is: {0}", cTime);
            Log.InfoFormat("Timeallowance: {0}", timeLimit);

            try {
                while (!isLoadCompleted) {
                    string loadedTime = loadingInfoService.GetLoadCompleteTimeFor(fileName, systemSerial);
                    Log.InfoFormat("Get loadedTime value in DB :{0} current thread: {1}", 
                                    loadedTime, currentThreadId);
                    DateTime lTime = DateTime.Parse(loadedTime); 
                    Log.InfoFormat("Time in DB is {0}, contrast (expected) time is : {1}, current thread: {2}",
                        lTime.Date, cTime, currentThreadId);
                    Log.InfoFormat("Loaded time date is : {0}", lTime.Date);
                    Log.InfoFormat("ExpectedTime date is: {0}", expectedTimeValue.Date);
                    Log.InfoFormat("Time limit date is: {0}", timeLimit.Date);

                    //Need to make sure loadedTime is greater than the current expected time (since loaded time could be from previous day) 
                    if (!string.IsNullOrEmpty(loadedTime)) {
                        /***Note: if some file comes in early, we can't use lTime >= cTime logic, since it always return false***/
                        if (lTime.Date < expectedTimeValue.Date) {
                            //Same file prefix from previous day
                            Log.Info("The file with same file name is from previous day");
                        }
                        else {
                            Log.InfoFormat("DB has loaded time and it's value is greater than expected time {0} (means today or tomorrow), current thread: {1}",
                                cTime, currentThreadId);
                            Log.Info("Load is completed");
                            isLoadCompleted = true;
                        }
                    }

                    bool isDelayed = IsFileDelayed(expectedTime, fileName, systemSerial, expectedTimeValue);
                    Log.InfoFormat("Is the file delayed?   " + isDelayed + " current thread: " + currentThreadId);
                    if (isDelayed) {
                        return !isLoadCompleted;
                    }

                    if (!isLoadCompleted) {
                        Log.InfoFormat("Still loading or the file does not exist, sleep for 5 mins...  currentthread: " + currentThreadId);
                        Thread.Sleep(300000);
                    }
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("Error occurred when checking LoadedTime " + ex);
            }
            finally {
                Log.InfoFormat("Check LoadedTime value done  currentthread: " + currentThreadId);
                
            }
            return false;
        }

        public bool IsFileDelayed(string expectedTime, string fileName, string systemSerial, DateTime expectedTimeValue) {
            Log.InfoFormat("***************************************************************");
            Log.InfoFormat("Start checking is file delayed current thread: {0}", currentThreadId);
            Log.InfoFormat("expectedTimeValue is: {0}", expectedTimeValue);
            var timeLimit = new DateTime();
            var currTime = new DateTime();
            try {
                int possibleDelay = tMonScheduleService.GetDelayFor(systemSerial);
                Log.InfoFormat("Possible delay: " + possibleDelay);
                int loadTimeAllowed = tMonScheduleService.GetLoadTimeFor(systemSerial);
                Log.InfoFormat("load time allowd: " + loadTimeAllowed);
                timeLimit = expectedTimeValue.AddMinutes(loadTimeAllowed).AddMinutes(possibleDelay);
                Log.InfoFormat("time limit: " + timeLimit);
                currTime = DateTime.Now;
                Log.InfoFormat("currTime: " + currTime);
            }
            catch (Exception ex) {
                Log.ErrorFormat("Exception in isFileDelayed(): {0}", ex);
            }
            finally {
                
            }
            return DateTime.Compare(currTime, timeLimit) > 0;
        }

        public bool CheckAWSS3Bucket(string expectedTime, DateTime expectedTimeValue, string fileName, string systemSerial) {
            Log.InfoFormat("***************************************************************");
            Log.InfoFormat("Start checking S3 bucket current thread: {0}", currentThreadId);
            bool existing = false;
            try {
                s3Objects = GetS3Objects("RA" + fileName, "ZA" + fileName, "DO" + fileName, "Systems/" + systemSerial + "/", expectedTime, fileName, systemSerial);
                if (s3Objects.Count > 0) {
                    existing = true;
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("Exception occurred when check S3 bucket " + ex);
            }
            finally {
                
            }
            return existing;
        }

        public List<S3ObjectView> GetS3Objects(string RAFileNamePattern, string ZAFileNamePattern, string DOFileNamePattern, string prefix, string expectedTime, string fileName, string systemSerial) {
            Log.ErrorFormat("***************************************************************");
            Log.ErrorFormat("Start checking S3 objects {0}", currentThreadId);
            var tempS3Objects = new List<S3ObjectView>();
            try {
                const string delimiter = "/";
                using (client = new AmazonS3Client(RemoteAnalyst.AWS.Helper.GetRegionEndpoint())) {
                    try {
                        var request = new ListObjectsRequest {
                            BucketName = ConnectionString.S3RAFTP,
                            Delimiter = delimiter,
                            Prefix = prefix
                        };
                        //using (ListObjectsResponse response = client.ListObjects(request)) {
                        ListObjectsResponse response = client.ListObjects(request);
                        foreach (S3Object entry in response.S3Objects) {
                            tempS3Objects.Add(new S3ObjectView {
                                key = entry.Key,
                                size = entry.Size,
                                lastModifiedDate = Convert.ToDateTime(entry.LastModified)
                            });
                        }
                        //}
                    }
                    catch (AmazonS3Exception amazonS3Exception) {
                        if (amazonS3Exception.ErrorCode != null && (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") || amazonS3Exception.ErrorCode.Equals("InvalidSecurity"))) {
                            Log.ErrorFormat("Please check the provided AWS Credentials.");
                            Log.ErrorFormat("If you haven't signed up for Amazon S3, please visit http://aws.amazon.com/s3");
                        }
                        else {
                            Log.ErrorFormat("An error occurred with the message when listing objects " + amazonS3Exception.Message);
                        }
                    }
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("Error occurred when reading the objects from bucket " + ex);
            }
            finally {
                Log.InfoFormat("Check S3 objects done currentthread: " + currentThreadId);
                
            }

            //The file name should start with ZA/RA + hour, end with 'SM', extension should be 402
            return tempS3Objects.
                Where(x => (((x.key.Contains(RAFileNamePattern)
                              || x.key.Contains(ZAFileNamePattern) || x.key.Contains(DOFileNamePattern))
                             && x.key.Contains("SM") && x.key.Contains(".402")
                             && x.lastModifiedDate.Year == DateTime.Now.Year && x.lastModifiedDate.Month == DateTime.Now.Month && x.lastModifiedDate.Day == DateTime.Now.Day))).
                OrderByDescending(x => x.lastModifiedDate).ToList();
        }

        public bool CheckUMPFileS3(S3ObjectView obj, string expectedTime, string fileName, string systemSerial, DateTime expectedTimeValue) {
            Log.InfoFormat("***************************************************************");
            Log.InfoFormat("Start Checking the UMP file S3. {0}", currentThreadId);
            try {
                bool isFileSizeIncreasing = true;
                while (isFileSizeIncreasing) {
                    isFileSizeIncreasing = IsFileSizeIncreasing(obj, expectedTime, fileName, systemSerial);
                    bool isDelayed = IsFileDelayed(expectedTime, fileName, systemSerial, expectedTimeValue);
                    if (isDelayed) {
                        if (!isFileSizeIncreasing) {
                            return false; //Consider this not a delay, since the file size might stop increasing while in the sleep period
                        }
                        Log.InfoFormat("Time is up. File name [" + fileName + "] system serial [" + systemSerial + "]currentthread: " + Thread.CurrentThread.ManagedThreadId);
                        return true;
                    }

                    if (isFileSizeIncreasing) {
                        Log.InfoFormat("File size still increasing, thread is going to sleep for 5 mins " + ", current thread: " + currentThreadId);
                        
                        Thread.Sleep(300000);
                    }
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("Exception in CheckUMPFileS3: {0}", ex);
            }
            finally {
                Log.InfoFormat("CheckUMP file done currentthread: " + Thread.CurrentThread.ManagedThreadId);
                
            }
            return false;
        }

        public bool IsFileSizeIncreasing(S3ObjectView view, string expectedTime, string fileName, string systemSerial) {
            Log.InfoFormat("***************************************************************");
            Log.InfoFormat("Start checking file size increasing {0}", currentThreadId);
            Log.InfoFormat("Orginal size: " + view.key + "   -    " + view.size);
            long preSize = view.size;
            S3ObjectView obj = GetS3Objects("RA" + fileName, "ZA" + fileName, "DO" + fileName, "Systems/" + systemSerial + "/", expectedTime, fileName, systemSerial).First();
            Log.InfoFormat("Current size: " + obj.key + "   -    " + obj.size + " current thread: " + Thread.CurrentThread.ManagedThreadId);
            
            return obj.size > preSize;
        }

        public void SendToSQS(string expectedTime, string fullFileName, string systemSerial) {
            var buildMessage = new StringBuilder();
            string queueURL = "";
            var _queue = new AmazonSQS();
            try {
                //Read Queue.
                if (!string.IsNullOrEmpty(ConnectionString.SQSLoad))
                    queueURL = _queue.GetAmazonSQSUrl(ConnectionString.SQSLoad);
            }
            catch (Exception ex) {
                AmazonError.WriteLog(ex, "Amazon.cs: GetAmazonSQSUrl",
                    ConnectionString.AdvisorEmail,
                    ConnectionString.SupportEmail,
                    ConnectionString.WebSite,
                    ConnectionString.EmailServer,
                    ConnectionString.EmailPort,
                    ConnectionString.EmailUser,
                    ConnectionString.EmailPassword,
                    ConnectionString.EmailAuthentication, ConnectionString.SystemLocation,
                    ConnectionString.ServerPath, ConnectionString.EmailIsSSL, false, 
                    ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
            }

            if (queueURL.Length > 0) {
                buildMessage.Append("SYSTEM\r\n").Append(systemSerial + "\r\n").Append("Systems/" + systemSerial + "/" + fullFileName);
                _queue.WriteMessage(queueURL, buildMessage.ToString());
            }
        }
    }
}