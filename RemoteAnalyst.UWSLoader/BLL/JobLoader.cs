using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using log4net;
using RemoteAnalyst.AWS.EC2;
using RemoteAnalyst.AWS.Queue;
using RemoteAnalyst.BusinessLogic.BLL;
using RemoteAnalyst.BusinessLogic.Enums;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.UWSLoader.BLL.SQS;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.UWSLoader.BLL {
    /// <summary>
    /// JobWatcher class setup the timer that checks the queue every 1 minute.
    /// </summary>
    public class JobLoader {
        private static readonly ILog Log = LogManager.GetLogger("JobLoader");
        private Timer _timerCheckQue;
        private Timer _timerCleanupArchive;
        private Timer _timerKillQue;
        private Timer _timerLoadDISCOPENQue;
        private Timer _timerLoadQue;
		private Timer _timerRDSMoveQue;

        public void StartCheckQueue() {
            StartCleanupArchive();

            //Before the loader starts up, need to go to LoadingStatusDetail table, get those entries
            //1. Find entries in LoadingStatusDetail table using EC2 instance ID
            //2. Delete the entries
            //3. Send message to SQS
            StartLoadingStoppedJobs();
            StartLoadingQue();
            StartLoadingDISCOPENQue();

            StartCheckQue();
            if (!ConnectionString.IsLocalAnalyst) {
#if (RDSMove)
				StartCheckRDSMove();
#endif
            }
        }
       
        private void StartCleanupArchive() {
            var cleanArchive = new CleanupArchive();
            _timerCleanupArchive = new Timer(3600000);
            _timerCleanupArchive.Elapsed += cleanArchive.Timer_Elapsed;
            _timerCleanupArchive.AutoReset = true;
            _timerCleanupArchive.Enabled = true;
        }

        //Load any files that were failed to load when loader stopped
        private void StartLoadingStoppedJobs() {
            string instanceID = "";

            if (!ConnectionString.IsLocalAnalyst) {
                var ec2 = new AmazonEC2();
                ec2.GetEC2ID();
            }
            var loadingStatusDetailService = new LoadingStatusDetailService(ConnectionString.ConnectionStringDB);
            var loadingStatusService = new LoadingStatusService(ConnectionString.ConnectionStringDB);
            List<LoadingStatusDetailView> stoppedJobs = loadingStatusDetailService.GetStoppedJobsFor(instanceID);
            if (stoppedJobs.Count > 0) {
                Log.InfoFormat("Start loading previously stopped jobs for EC2 instance: {0}", instanceID);
                Log.InfoFormat("Number of jobs that failed to load: {0}", stoppedJobs.Count);
                foreach (LoadingStatusDetailView job in stoppedJobs) {
                    if (ConnectionString.IsLocalAnalyst)
                    {
                        var triggerService = new TriggerService(ConnectionString.ConnectionStringDB);
                        triggerService.InsertFor(job.SystemSerial, (int)TriggerType.Type.Loader, "SYSTEM", job.DataFileName, 0, 0);
                    }
                    else {
                        Log.InfoFormat("Send message to SQS: {0}, {1}", job.SystemSerial, job.DataFileName);
                        //Send messages to SQS
                        //Due to multiple load problem, need to stop the SendMessageFroStoppedJobs.

                        string buildMessage = "SYSTEM\n" + job.SystemSerial + "\n" + "Systems/" + job.SystemSerial + "/" + job.DataFileName;
                        Log.InfoFormat("buildMessage: {0}",buildMessage);
                        SendMessagesForStoppedJobs(job.SystemSerial, job.DataFileName);
                    }
                    //Delete the entry from LoadingStatusDetail
                    Log.InfoFormat("Delete entry on LoadingStatusDetail table: {0}",job.DataFileName);
                    loadingStatusDetailService.DeleteLoadingInfoFor(job.DataFileName);
                }

                int currentLoad = loadingStatusService.GetCurrentLoadFor(instanceID);
                Log.InfoFormat("Current load on LoadingStatus: {0}",currentLoad);
                //Reset the currentLoad value (by subtracting the number of stopped jobs)
                loadingStatusService.UpdateLoadingStatusFor(instanceID, 0);
                Log.InfoFormat("Update current load to: {0}",(currentLoad - stoppedJobs.Count));
                
            }

            //Check LoadingStatusDetailDISCOPEN
            try {
                var loadingStatusDetailDISCOPEN = new LoadingStatusDetailDISCOPENService(ConnectionString.ConnectionStringDB);
                var stoppedDiscOpenJobs = loadingStatusDetailDISCOPEN.GetStoppedJobsFor(instanceID);
                if (stoppedDiscOpenJobs.Count > 0) {
                    Log.InfoFormat("Number of DISCOPEN jobs that failed to load: {0}",stoppedDiscOpenJobs.Count);
                    
                    foreach (var job in stoppedDiscOpenJobs) {
                        try {
                            Log.InfoFormat("Delete entry from LoadingStatusDetailDISCOPEN table: {0}",job.DataFileName);
                            
                            loadingStatusDetailDISCOPEN.DeleteLoadingInfoFor(job.UWSID);
                        }
                        catch (Exception ex) {
                            Log.ErrorFormat("Error LoadingStatusDetailDISCOPEN Delete: {0}",ex);                            
                        }
                    }
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("Error LoadingStatusDetailDISCOPEN: {0}",ex);                
            }
        }

        private void SendMessagesForStoppedJobs(string systemSerial, string fileName) {
            string buildMessage = "SYSTEM\n" + systemSerial + "\n" + "Systems/" + systemSerial + "/" + fileName;
            if (!ConnectionString.IsLocalAnalyst) {
                var sqs = new AmazonSQS();
                string queueURL = "";
                try {
                    //Read Queue.
                    if (!string.IsNullOrEmpty(ConnectionString.SQSLoad)) {
                        queueURL = sqs.GetAmazonSQSUrl(ConnectionString.SQSLoad);
                    }
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
                        ConnectionString.ServerPath,
                        ConnectionString.EmailIsSSL,
                        ConnectionString.IsLocalAnalyst, ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                }

                if (queueURL.Length > 0) {
                    sqs.WriteMessage(queueURL, buildMessage);
                }
            }
        }

        private void StartLoadingQue() {
            var loadQue = new LoadingQue();
            _timerLoadQue = new Timer(30000); //Once an 30 sec.
            _timerLoadQue.Elapsed += loadQue.LoadQue;
            _timerLoadQue.AutoReset = true;
            _timerLoadQue.Enabled = true;
        }

        private void StartLoadingDISCOPENQue() {
             var loadDISCOPENQue = new LoadingQueDISCOPEN();
            _timerLoadDISCOPENQue = new Timer(60000); //Once an 60 sec.
            _timerLoadDISCOPENQue.Elapsed += loadDISCOPENQue.LoadDISCOPENQue;
            _timerLoadDISCOPENQue.AutoReset = true;
            _timerLoadDISCOPENQue.Enabled = true;
        }

		//Only RDSMove UWSLoader responsible for copy table
#if (RDSMove)
		private void StartCheckRDSMove() {
			var checkQue = new CheckQue();
			_timerRDSMoveQue = new Timer(300000); //Check RDSMove Q per 5 mins
			_timerRDSMoveQue.Elapsed += checkQue.CheckRDSMove;
			_timerRDSMoveQue.AutoReset = true;
			_timerRDSMoveQue.Enabled = true;
		}
#endif

        private void StartCheckQue() {
            var checkQue = new CheckQue(Log);
            //_timerCheckQue = new Timer(10000);//Once an 10 sec.
            _timerCheckQue = new Timer(30000);//Once an 30 sec.
            _timerCheckQue.Elapsed += checkQue.CheckUWS;
			_timerCheckQue.AutoReset = true;
            _timerCheckQue.Enabled = true;

		}

        internal void StopJobWatch() {
            _timerCheckQue = null;
            _timerCleanupArchive = null;
        }
    }
}