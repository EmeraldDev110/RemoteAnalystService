using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.Enums;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.ReportGenerator.BLL {
    internal class ClearQueue {
        internal static void Clear(Report.Types reportType, int queueID) {
            if (ConnectionString.IsLocalAnalyst) {
                var reportQueues = new ReportQueueAWSService(ConnectionString.ConnectionStringDB);
                reportQueues.RemoveQueueFor(queueID);
            }
            else {
                var reportQueues = new ReportQueueService(ConnectionString.ConnectionStringDB);
                reportQueues.RemoveQueueFor(queueID);
            }
        }
    }
}
