using System;
using System.IO;
using System.Threading;
using log4net;
using RemoteAnalyst.AWS.EC2;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.UWSLoader.BLL.Process;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.UWSLoader.BLL {
    internal class LoadingQueDISCOPEN {
        private static readonly ILog Log = LogManager.GetLogger("JobLoader");

        public void LoadDISCOPENQue(object source, System.Timers.ElapsedEventArgs e) {
            string connectionString = ConnectionString.ConnectionStringDB;
            string instanceID = "";
            if (!ConnectionString.IsLocalAnalyst) {
                var ec2 = new AmazonEC2();
                instanceID = ec2.GetEC2ID();
            }
            //Get the current number of queues on the database.
            var loadingStatusDetailService = new LoadingStatusDetailDISCOPENService(connectionString);
            int loadCount = loadingStatusDetailService.GetCurrentLoadLengthFor(instanceID);
            int queCount = loadingStatusDetailService.GetCurrentQueLengthFor(instanceID);
           
            var raInfo = new RAInfoService(connectionString);
            int maxDISCOPENLoad = raInfo.GetMaxQueueFor(ConnectionString.MaxDISCOPENQueue);
  
            if (loadCount < maxDISCOPENLoad && queCount != 0) {
                /*-------------------------get Que information from database--------------------------*/
                //Get the first data from LoadingStautsQue that is not processing.
                LoadingStatusDetailView dataView = loadingStatusDetailService.GetLoadingStatusDetailFor(instanceID);

                string fileName = dataView.DataFileName;
                string systemSerial = dataView.SystemSerial;
                DateTime selectedStartTime = dataView.SelectedStartTime;
                DateTime selectedStopTime = dataView.SelectedStopTime;

                if (fileName.Length > 0 && systemSerial.Length > 0) {
                    try {
                        //Change currentLoad value.
                        loadingStatusDetailService.UpdateLoadingStatusDetailFor("1", DateTime.Now, fileName, systemSerial, instanceID);

                        var processDISCOPEN = new ProcessDISCOPEN(systemSerial, fileName, selectedStartTime, selectedStopTime);
                        var thread = new Thread(processDISCOPEN.StartProcess) {IsBackground = true};
                        thread.Start();

                        Log.Info("Calling ProcessJob");
                        Log.InfoFormat("systemSerial: {0}", systemSerial);
                        Log.InfoFormat("fileName: {1}", fileName);
                        Log.InfoFormat("selectedStartTime: {2}", selectedStartTime);
                        Log.InfoFormat("selectedStopTime: {3}", selectedStopTime);
                    }
                    catch (Exception ex) {
                        Log.ErrorFormat("Error loading DISCOPEN data {0}", ex);
                    }
                }

                Thread.Sleep(50000);
                //Get current load count again.
                loadCount = loadingStatusDetailService.GetCurrentLoadLengthFor(instanceID);
                queCount = loadingStatusDetailService.GetCurrentQueLengthFor(instanceID);
            }
        }
    }
}