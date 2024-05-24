using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using RemoteAnalyst.BusinessLogic.Enums;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.Scheduler.Schedules;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.ReportGenerator.BLL.Schedule {
    public class CheckTriggerQueue {
        public void TimerTriggerQueue_Elapsed(object source, ElapsedEventArgs e) {
            var triggerService = new TriggerService(ConnectionString.ConnectionStringDB);

            var triggerView = triggerService.GetTriggerFor((int)BusinessLogic.Enums.TriggerType.Type.ReportGenerator);
            if (triggerView.TriggerId > 0) {
                var reportMaxQueue = new RAInfoService(ConnectionString.ConnectionStringDB);
                var reportQueues = new ReportQueueAWSService(ConnectionString.ConnectionStringDB);
                var reportType = triggerView.Message;
                int maxQueue = 0;
                var instanceID = "";

                if (reportType.StartsWith("QT")) {
                    maxQueue = reportMaxQueue.GetMaxQueueFor(ScheduleQueueConstants.MaxQT);

                    if (reportQueues.GetCurrentCountFor((int)Report.Types.QT, instanceID) < maxQueue) {
                        reportQueues.InsertNewQueueFor(triggerView.Message, (int)Report.Types.QT, instanceID);
                        triggerService.DeleteTriggerFor(triggerView.TriggerId);
                    }
                }
                else if (reportType.StartsWith("DPA")) {
                    maxQueue = reportMaxQueue.GetMaxQueueFor(ScheduleQueueConstants.MaxDPA);

                    if (reportQueues.GetCurrentCountFor((int)Report.Types.DPA, instanceID) < maxQueue) {
                        reportQueues.InsertNewQueueFor(triggerView.Message, (int)Report.Types.DPA, instanceID);
                        triggerService.DeleteTriggerFor(triggerView.TriggerId);
                    }
                } else if (reportType.StartsWith("NEWBATCH")) {
                    triggerService.DeleteTriggerFor(triggerView.TriggerId);

                    var contents = triggerView.Message.Split('|');
                    var systemSerial = contents[1].ToString();
                    var batchSequenceName = contents[2].ToString();

                    var batchService = new BatchService(ConnectionString.ConnectionStringDB, null, ConnectionString.IsLocalAnalyst);
                    var retentionDays = batchService.GetRDSRetentionDays(systemSerial);

                    var batchSequenceMonitor = new BatchSequenceMonitor();
                    var threadTrend = new System.Threading.Thread(() => batchSequenceMonitor.GenerateBatchSequenceForLastXDays(systemSerial, batchSequenceName, retentionDays, ConnectionString.ConnectionStringDB));
                    threadTrend.IsBackground = true;
                    threadTrend.Start();
                }
            }
        }
    }
}
