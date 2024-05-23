using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.AWS.Queue.View;
using RemoteAnalyst.AWS.Queue;
using log4net;

namespace RemoteAnalyst.BusinessLogic.Scheduler.Schedules {
    public class BatchSequenceMonitor {

        private static readonly ILog Log = LogManager.GetLogger("BatchSequenceReport");
        private BatchEmail _BatchEmail;
        
        public BatchSequenceMonitor() {
            _BatchEmail = new BatchEmail(
            ConnectionString.AdvisorEmail,
            ConnectionString.SupportEmail,
            ConnectionString.WebSite,
            ConnectionString.EmailServer,
            ConnectionString.EmailPort,
            ConnectionString.EmailUser,
            ConnectionString.EmailPassword,
            ConnectionString.EmailAuthentication,
            ConnectionString.ServerPath,
            ConnectionString.SystemLocation,
            ConnectionString.EmailIsSSL,
            ConnectionString.IsLocalAnalyst,
            ConnectionString.MailGunSendAPIKey,
            ConnectionString.MailGunSendDomain);
        }

        public void Timer_ElapsedHourly(object source, System.Timers.ElapsedEventArgs e) {
            CheckBatchSequenceData();
        }

        public void Timer_ElapsedMinutely(object source, System.Timers.ElapsedEventArgs e) {
            ReadSQSQueue();
        }

        public void CheckBatchSequenceData() {
            try {
                Log.InfoFormat("Batch Sequence Check Began at {0}", DateTime.Now);
                Log.Info("Retrieving All Systems Information");
                
#if (DEBUG)
                DateTime currTime = Convert.ToDateTime("10/17/2021 01:00:00 AM");
#else
                DateTime currTime = DateTime.Now;
#endif

                var batchService = new BatchService(ConnectionString.ConnectionStringDB, Log, ConnectionString.IsLocalAnalyst);
                var systemInformationList = batchService.GetAllSystemInformationForBatch();
                var dayOffset = 0;

                foreach (var system in systemInformationList) {
                    if (LocalHourConvertedToSystemHourEqualsSpecifiedTime(currTime, system.TimeZone, 1)) {
                        Log.InfoFormat("Time to run batch for {0} ", system.SystemSerial, DateTime.Now);
                        batchService = new BatchService(system.ConnectionString, Log, ConnectionString.IsLocalAnalyst);

                        batchService.CreateBatchTablesIfNotExist();

                        var batchList = batchService.GetBatchInformationBySystem();
                        foreach (var batch in batchList) {
                            var trendBySystem = batchService.GetProcessesTrendInformationByBatchId(batch, system.SystemSerial.ToString(), dayOffset);
                            batchService.InsertBatchTrendData(trendBySystem, batch.BatchSequenceProfileId);
                            _BatchEmail.SendBatchAlertEmailIfMeetsCriteria(batch, trendBySystem);
                        }
                    }
                }
                Log.Info("Batch Sequence Complete");
            }
            catch (Exception ex) {
                Log.ErrorFormat("Error Running Batch Sequence. Error: {0}", ex);
            }
        }

        public bool LocalHourConvertedToSystemHourEqualsSpecifiedTime(DateTime currTime, int timeZoneIndex, int desiredHourToRun) {
            string timeZoneName = TimeZoneIndexConverter.ConvertIndexToName(timeZoneIndex);

            //Check timezone
            TimeZoneInfo est = TimeZoneInfo.FindSystemTimeZoneById(timeZoneName);
            DateTime targetTime = TimeZoneInfo.ConvertTime(currTime, est);

            if (targetTime.Hour == desiredHourToRun) return true;
            else return false;
        }

        public void GenerateBatchSequenceForLastXDays(string systemSerial, string batchSequenceName, int totalDays, string connectionString) {
            try {
                var databaseMappings = new DatabaseMappingService(connectionString);
                var newConnectionString = databaseMappings.GetConnectionStringFor(systemSerial);

                var batchService = new BatchService(newConnectionString, Log, ConnectionString.IsLocalAnalyst);
                batchService.CreateBatchTablesIfNotExist();

                var newBatch = batchService.GetBatchInformationByName(batchSequenceName);
                var currentDay = 0;
                while (currentDay < totalDays) {
                    var trendBySystem = batchService.GetProcessesTrendInformationByBatchId(newBatch, systemSerial, currentDay);
                    if (trendBySystem.Count > 0) batchService.InsertBatchTrendData(trendBySystem, newBatch.BatchSequenceProfileId);
                    currentDay++;
                }
            } catch (Exception ex) {
                Log.ErrorFormat("Error Genering New Batch Sequence. Error: {0}", ex);
            }
        }

        public void ReadSQSQueue() {
            var sqs = new AmazonSQS();
            string queueUrl = "";

            if (!string.IsNullOrEmpty(ConnectionString.SQSBatch))
                queueUrl = sqs.GetAmazonSQSUrl(ConnectionString.SQSBatch);

            MessageView view = sqs.ReadMessage(queueUrl);

            if (!string.IsNullOrEmpty(view.Body)) {
                string[] contents = view.Body.Split('|');
                string type = contents[0].Trim();

                if (type.ToUpper().Equals("NEWBATCH")) {
                    sqs.DeleteMessage(queueUrl, view.ReceiptHandle);
                    var systemSerial = contents[1].ToString();
                    var batchSequenceName = contents[2].ToString();

                    var batchService = new BatchService(ConnectionString.ConnectionStringDB, null, ConnectionString.IsLocalAnalyst);
                    var retentionDays = batchService.GetRDSRetentionDays(systemSerial);

                    var batchSequenceMonitor = new BatchSequenceMonitor();                    
                    var threadTrend = new Thread(() => batchSequenceMonitor.GenerateBatchSequenceForLastXDays(systemSerial, batchSequenceName, retentionDays, ConnectionString.ConnectionStringDB));
                    threadTrend.IsBackground = true;
                    threadTrend.Start();

                } else {
                    sqs.DeleteMessage(queueUrl, view.ReceiptHandle);
                }
            }                   
        }
    }
}
