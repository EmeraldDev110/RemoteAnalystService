using System;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class ReportDownloadLogService {
        private readonly string _connectionString;
        public ReportDownloadLogService(string connectionString) {
            _connectionString = connectionString;
        }
        public void InsertNewLogFor(int reportDownloadId, DateTime logDate, string message) {
            var reportDownloadLogs = new ReportDownloadLogs(_connectionString);
            reportDownloadLogs.InsertNewLog(reportDownloadId, logDate, message);
        }

        public DateTime GetFirstLogDateFor(int reportDownloadId) {
            var reportDownloadLogs = new ReportDownloadLogs(_connectionString);
            var logDate = reportDownloadLogs.GetFirstLogDate(reportDownloadId);

            return logDate;
        }
    }
}
