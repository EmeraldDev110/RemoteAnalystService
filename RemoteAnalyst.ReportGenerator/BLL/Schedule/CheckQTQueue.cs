using System;
using System.Threading;
using System.Timers;
using RemoteAnalyst.AWS.EC2;
using RemoteAnalyst.BusinessLogic.Enums;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.ReportGenerator.BLL.Schedule {
    internal class CheckQTQueue {
        //This class is checking the entry in ReportQueuesAWS table
        public void TimerCheckQTQueue_Elapsed(object source, ElapsedEventArgs e) {
            var reportMaxQueue = new RAInfoService(ConnectionString.ConnectionStringDB);
            int maxQueue = reportMaxQueue.GetMaxQueueFor(ScheduleQueueConstants.MaxQT);

            string instanceID = "";

            if (!ConnectionString.IsLocalAnalyst) {
                var ec2 = new AmazonEC2();
                instanceID = ec2.GetEC2ID();
            }

            var reportQueues = new ReportQueueAWSService(ConnectionString.ConnectionStringDB);
            int processingQT = reportQueues.GetProcessingOrderFor((int) Report.Types.QT, instanceID);

            if (processingQT <= maxQueue) {
                ReportQueueView queueList = reportQueues.GetCurrentQueuesFor((int) Report.Types.QT, instanceID);
                if (queueList != null && queueList.QueueID != 0) {
                    reportQueues.UpdateOrdersFor(queueList.QueueID);

                    #region QT

                    string[] contents = queueList.FileName.Split('|');
                    int reportDownloadId = 0;
                    string systemSerial = "";
                    string emails = "";
                    var startTime = new DateTime();
                    var stopTime = new DateTime();
                    string alerts = "";
                    string optional = "";
                    string sourceQT = "";
                    string dest = "";
                    string program = "";
                    string systemName = "";
                    int autoID = 0;
                    int ntsOrderID = 0;
                    bool isBeforeJ06 = false;
                    bool isSchedule = false;

                    try {
                        reportDownloadId = Convert.ToInt32(contents[1]);
                        systemSerial = contents[2];
                        systemName = contents[3];
                        startTime = Convert.ToDateTime(contents[4]);
                        stopTime = Convert.ToDateTime(contents[5]);
                        alerts = contents[6];
                        optional = contents[7];
                        sourceQT = contents[8];
                        dest = contents[9];
                        program = contents[10];
                        emails = contents[11];
                        autoID = Convert.ToInt32(contents[12]);
                        isBeforeJ06 = Convert.ToBoolean(contents[13]);
                        isSchedule = Convert.ToBoolean(contents[14]);
                        //Check whether this is NTS order or RA order
                        var systemTblService = new System_tblService(ConnectionString.ConnectionStringDB);
                        bool isNTS = systemTblService.IsNTSSystemFor(systemSerial);

                        var isLocalAnalyst = ConnectionString.IsLocalAnalyst;

                        if (isNTS) ntsOrderID = 1;
                        else if (isLocalAnalyst) ntsOrderID = 1;
                    }
                    catch (Exception) {
                    }
                    var qt = new JobProcessorQT(reportDownloadId, startTime, stopTime, alerts, optional, sourceQT, dest, program,
                        systemSerial, systemName, emails, autoID, ntsOrderID, isSchedule, isBeforeJ06, queueList.QueueID);
                    var threadQT = new Thread(qt.ProcessReport) {
                        IsBackground = true
                    };
                    threadQT.Start();

                    #endregion
                }
            }
        }
    }
}