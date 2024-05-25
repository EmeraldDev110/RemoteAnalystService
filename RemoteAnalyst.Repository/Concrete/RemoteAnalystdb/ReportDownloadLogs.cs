using System;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class ReportDownloadLogs {
        private readonly string _connectionString;
        public ReportDownloadLogs(string connectionString) {
            _connectionString = connectionString;
        }
        public void InsertNewLog(int reportDownloadId, DateTime logDate, string message) {
            if (reportDownloadId > 0 && _connectionString.Length > 0) {
                try {
                    string cmdInsert = @"INSERT INTO ReportDownloadLogs (ReportDownloadId, LogDate, Message) 
                                VALUES (@ReportDownloadId, @LogDate, @Message)";

                    using (var connection = new MySqlConnection(_connectionString)) {
                        var insertCmd = new MySqlCommand(cmdInsert, connection);
                        insertCmd.CommandTimeout = 0;

                        insertCmd.Parameters.AddWithValue("@ReportDownloadId", reportDownloadId);
                        insertCmd.Parameters.AddWithValue("@LogDate", logDate);
                        insertCmd.Parameters.AddWithValue("@Message", message);

                        connection.Open();
                        insertCmd.ExecuteNonQuery();
                    }
                }
                catch { }
            }
        }

        public DateTime GetFirstLogDate(int reportDownloadId) {
            string cmdText = @"SELECT LogDate FROM ReportDownloadLogs
                                WHERE ReportDownloadId = @ReportDownloadId
                                ORDER BY LogDate LIMIT 1";

            var logDate = DateTime.MinValue;
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@ReportDownloadId", reportDownloadId);
                connection.Open();

                var reader = command.ExecuteReader();

                if (reader.Read()) {
                    logDate = !DBNull.Value.Equals(reader["LogDate"]) ? Convert.ToDateTime(reader["LogDate"]) : DateTime.Now;
                }
            }

            return logDate;
        }
    }
}
