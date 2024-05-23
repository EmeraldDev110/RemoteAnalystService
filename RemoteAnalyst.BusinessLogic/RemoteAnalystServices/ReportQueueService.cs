using System;
using System.Data;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class ReportQueueService {
        private readonly string _connectionString;

        public ReportQueueService(string connectionString) {
            _connectionString = connectionString;
        }

        public void InsertNewQueueFor(string fileName, int typeID) {
            var reportQueues = new ReportQueues(_connectionString);
            reportQueues.InsertNewQueue(fileName, typeID);
        }

        public void RemoveQueueFor(int queueID) {
            var reportQueues = new ReportQueues(_connectionString);
            reportQueues.RemoveQueue(queueID);
        }
    }
}