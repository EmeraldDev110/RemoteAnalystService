using System;
using System.Data;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class ReportQueueAWSService {
        private readonly string _connectionString;

        public ReportQueueAWSService(string connectionString) {
            _connectionString = connectionString;
        }

        public ReportQueueView GetCurrentQueuesFor(int typeID, string instanceID) {
            var reportQueues = new ReportQueuesAWS(_connectionString);
            DataTable entityOrders = reportQueues.GetCurrentQueues(typeID, instanceID);

            var reportQueueView = new ReportQueueView();

            foreach (DataRow dr in entityOrders.Rows) {
                reportQueueView.QueueID = Convert.ToInt32(dr["QueueID"]);
                reportQueueView.FileName = Convert.ToString(dr["Message"]);
                reportQueueView.TypeID = Convert.ToInt32(dr["TypeID"]);
                reportQueueView.Loading = Convert.ToInt32(dr["Loading"]);
                reportQueueView.OrderDate = Convert.ToDateTime(dr["OrderDate"]);
            }

            return reportQueueView;
        }

        public void UpdateOrdersFor(int queueID) {
            var reportQueues = new ReportQueuesAWS(_connectionString);
            reportQueues.UpdateOrders(queueID);
        }

        public int GetProcessingOrderFor(int typeID, string instanceID) {
            var reportQueues = new ReportQueuesAWS(_connectionString);
            int processingEntity = reportQueues.GetProcessingOrder(typeID, instanceID);

            return processingEntity;
        }

        public void InsertNewQueueFor(string message, int typeID, string instanceID) {
            var reportQueues = new ReportQueuesAWS(_connectionString);
            reportQueues.InsertNewQueue(message, typeID, instanceID);
        }

        public int GetCurrentCountFor(int typeID, string instanceID) {
            var reportQueues = new ReportQueuesAWS(_connectionString);
            int currentCount = reportQueues.GetCurrentCount(typeID, instanceID);
            return currentCount;
        }

        public void RemoveQueueFor(int queueID) {
            var reportQueues = new ReportQueuesAWS(_connectionString);
            reportQueues.RemoveQueue(queueID);
        }

        public bool CheckOtherQueuesFor(string instanceID) {
            var reportQueues = new ReportQueuesAWS(_connectionString);
            var inQueue = reportQueues.CheckOtherQueues(instanceID);

            return inQueue;
        }
    }
}