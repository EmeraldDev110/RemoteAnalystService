using System;
using System.Threading;
using System.Timers;
using RemoteAnalyst.AWS.EC2;
using RemoteAnalyst.BusinessLogic.Enums;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.ReportGenerator.BLL.Schedule {
    internal class CheckDPAQueue {
        //This class is checking the entry in ReportQueuesAWS table
        public void TimerCheckDPAQueue_Elapsed(object source, ElapsedEventArgs e) {
            CheckDPAQ();
        }

        public void CheckDPAQ() { 
            var reportMaxQueue = new RAInfoService(ConnectionString.ConnectionStringDB);
            int maxQueue = reportMaxQueue.GetMaxQueueFor(ScheduleQueueConstants.MaxDPA);
            string instanceID = "";

            if (!ConnectionString.IsLocalAnalyst) {
                var ec2 = new AmazonEC2();
                instanceID = ec2.GetEC2ID();
            }

            var reportQueues = new ReportQueueAWSService(ConnectionString.ConnectionStringDB);
            int processingDPA = reportQueues.GetProcessingOrderFor((int) Report.Types.DPA, instanceID);
            if (processingDPA <= maxQueue) {
                ReportQueueView queueList = reportQueues.GetCurrentQueuesFor((int) Report.Types.DPA, instanceID);
                if (queueList != null && queueList.QueueID != 0) {
                    reportQueues.UpdateOrdersFor(queueList.QueueID);

                    #region DPA

                    string[] contents = queueList.FileName.Split('|');
                    int reportDownloadId = 0;
                    string systemSerial = "";
                    string emails = "";
                    var startTime = new DateTime();
                    var stopTime = new DateTime();
                    string parameters = "";
                    string reports = "";
                    string charts = "";
                    string iReports = "";
                    string excelMaxRow = "";
                    string excelVersion = "";
                    int ntsOrderID = 0;
                    bool macroReport = false;
                    bool isSchedule = false;

                    try {
                        reportDownloadId = Convert.ToInt32(contents[1]);
                        systemSerial = contents[2];
                        startTime = Convert.ToDateTime(contents[3]);
                        stopTime = Convert.ToDateTime(contents[4]);
                        parameters = contents[5];
                        reports = contents[6];
                        charts = contents[7];
                        iReports = contents[8];
                        emails = contents[9];
                        excelMaxRow = contents[10];
                        excelVersion = contents[11];
                        macroReport = Convert.ToBoolean(contents[12]);
                        isSchedule = Convert.ToBoolean(contents[13]);
                        //if (contents.Length > 12) {
                        //    ntsOrderID = Convert.ToInt32(contents[12]);
                        //}

                        //Check whether this is NTS order or RA order
                        var systemTblService = new System_tblService(ConnectionString.ConnectionStringDB);
                        bool isNTS = systemTblService.IsNTSSystemFor(systemSerial);

                        var isLocalAnalyst = ConnectionString.IsLocalAnalyst;

                        if (isNTS) ntsOrderID = 1;
                        else if (isLocalAnalyst) ntsOrderID = 1;
                    }
                    catch {
                    }

                    var dpa = new JobProcessorDPA(reportDownloadId, startTime, stopTime, parameters, reports, charts, iReports,
                        systemSerial, emails, excelMaxRow, excelVersion, ntsOrderID, queueList.QueueID, macroReport, isSchedule);
                    var threadDPA = new Thread(dpa.ProcessReport) {
                        IsBackground = true
                    };
                    threadDPA.Start();

                    #endregion
                }
            }
        }
    }
}