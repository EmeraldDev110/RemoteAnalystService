using System;
using System.IO;
using System.Threading;
using log4net;
using RemoteAnalyst.AWS.EC2;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.UWSLoader.JobProcessor;

namespace RemoteAnalyst.UWSLoader.BLL {
    internal class LoadingQue {
        private static readonly ILog Log = LogManager.GetLogger("JobLoader");
        public void LoadQue(object source, System.Timers.ElapsedEventArgs e) {
            //Force all datetime to be in US format.
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            string connectionString = ConnectionString.ConnectionStringDB;
            string invalidLicenseReason = "";

            string instanceID = "";
            if (!ConnectionString.IsLocalAnalyst) {
                var ec2 = new AmazonEC2();
                instanceID = ec2.GetEC2ID();
            }

            //Get the current number of queues on the database.
            var loadingStatusDetailService = new LoadingStatusDetailService(connectionString);
            int queCount = loadingStatusDetailService.GetCurrentQueueLengthFor(instanceID);

            var loadingStatusService = new LoadingStatusService(connectionString);

            if (loadingStatusService.CheckLoadingFor(instanceID) && queCount != 0) {
                /*-------------------------get Que information from database--------------------------*/
                //Get the first data from LoadingStautsQue that is not processing.
                var dataView = loadingStatusDetailService.GetLoadingStatusDetailFor(instanceID);

                string fileName = dataView.DataFileName;
                string systemSerial = dataView.SystemSerial;
                int uwsID = dataView.UWSID;
                string loadType = dataView.LoadType.ToUpper();

                if (fileName.Length > 0 && systemSerial.Length > 0 && uwsID != -1) {
                    try {
                        if (LicenseChecker.ValidLicenseToLoad(systemSerial, ref invalidLicenseReason)) {
                            
                            //check the file name and if the file name starts with DK, call ProcessJobDK.
                            if (loadType == "DISK") {
                                var jobDisk = new JobProcessorDisk(fileName, uwsID, systemSerial);
                                var jobProcess = new JobProcess();
                                //Change currentLoad value.

                                bool updateChecker = jobProcess.UpdateLoadingStatusDetail(fileName, systemSerial);

                                if (updateChecker)
                                    jobProcess.ChangeStatus(+1);

                                var t = new Thread(jobDisk.ProcessJobDisk) { IsBackground = true }; //change the functio name.
                                t.Start();
                            }
                            else {
                                //Change currentLoad value.
                                var jobUWS = new JobProcessorUWS(fileName, uwsID, systemSerial, loadType);
                                var jobProcess = new JobProcess();

                                bool updateChecker = jobProcess.UpdateLoadingStatusDetail(fileName, systemSerial);

                                if (updateChecker)
                                    jobProcess.ChangeStatus(+1);

                                var t = new Thread(jobUWS.ProcessJob) { IsBackground = true };
                                t.Start();
                            }
                        }
                        else
                        {
                            //ToDO: Delete file since the system does not have a valid license.
                            var deleteFile = new FileInfo(ConnectionString.SystemLocation + systemSerial + "\\" + fileName);
                            try
                            {
                                if (File.Exists(deleteFile.FullName))
                                {
                                    File.Delete(deleteFile.FullName);
                                }
                                Log.InfoFormat("License for {0} has expired or not found. Hence not loading {1}.", systemSerial, fileName);
                                Log.InfoFormat("Reason: {0}", invalidLicenseReason);
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                    catch (Exception ex) {
                        Log.ErrorFormat("License for {0} has expired or not found. Hence not loading {1}.", systemSerial, fileName);
                        Log.ErrorFormat("Error when processing the data: {0}", ex);
                    }
                }
            }
        }
    }
}