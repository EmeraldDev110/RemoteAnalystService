using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb
{
    public class ReportDownloads
    {
        private readonly string _connectionString;

        public ReportDownloads(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int InsertNewReport(string systemSerial, DateTime startDateTime, DateTime stopDateTime, int typeId, int custId) {
            int reportDownloadId;
            string cmdInsert = @"INSERT INTO ReportDownloads (SystemSerial, StartTime, EndTime, TypeID, OrderBy, Status, RequestDate) 
                                VALUES (@SystemSerial, @StartTime, @EndTime, @TypeID, @OrderBy, @Status, @RequestDate);SELECT LAST_INSERT_ID()";

            using (var connection = new MySqlConnection(_connectionString)) {
                var insertCmd = new MySqlCommand(cmdInsert, connection);
                insertCmd.CommandTimeout = 0;
                insertCmd.Parameters.AddWithValue("@SystemSerial", systemSerial);
                insertCmd.Parameters.AddWithValue("@StartTime", startDateTime);
                insertCmd.Parameters.AddWithValue("@EndTime", stopDateTime);
                insertCmd.Parameters.AddWithValue("@TypeID", typeId);
                insertCmd.Parameters.AddWithValue("@OrderBy", custId);
                insertCmd.Parameters.AddWithValue("@Status", 0);
                insertCmd.Parameters.AddWithValue("@RequestDate", DateTime.Now);

                connection.Open();
                reportDownloadId = Convert.ToInt32(insertCmd.ExecuteScalar());
            }
            return reportDownloadId;
        }

        public void UpdateFileLocation(int reportDownlaodId, string fileLocation) {
            string cmdText = @"UPDATE ReportDownloads SET GenerateDate = @GenerateDate, FileLocation = @FileLocation
                           WHERE ReportDownloadId = @ReportDownloadId";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@GenerateDate", DateTime.Now);
                command.Parameters.AddWithValue("@FileLocation", fileLocation);
                command.Parameters.AddWithValue("@ReportDownloadId", reportDownlaodId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void UpdateStatus(int reportDownlaodId, short status) {
            string cmdText = @"UPDATE ReportDownloads SET Status = @Status WHERE ReportDownloadId = @ReportDownloadId";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@Status", status);
                command.Parameters.AddWithValue("@ReportDownloadId", reportDownlaodId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public List<int> GetProcessingIds() {
            string cmdText = @"SELECT ReportDownloadId FROM ReportDownloads WHERE Status = 0";

            var reportDownloadIds = new List<int>();
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                connection.Open();

                var reader = command.ExecuteReader();

                while (reader.Read()) {
                    reportDownloadIds.Add(Convert.ToInt32(reader["ReportDownloadId"]));
                }
            }

            return reportDownloadIds;
        }

        public DataTable GetReportDetail(int reportDownloadId) {
            string cmdText = @"SELECT S.SystemName, S.SystemSerial, StartTime, EndTime, TypeId, CONCAT(C.FName,' ',C.LName) AS FullName
                                FROM ReportDownloads AS R 
                                LEFT JOIN System_Tbl AS S ON S.SystemSerial = R.SystemSerial
                                LEFT JOIN CusAnalyst AS C ON R.OrderBy = C.CustomerID
                                WHERE ReportDownloadId = @ReportDownloadId";

            var reportDetail = new DataTable();
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@ReportDownloadId", reportDownloadId);

                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(reportDetail);
            }

            return reportDetail;
        }

        public DateTime GetReportRequestDate(int reportDownloadId) {
            string cmdText = @"SELECT RequestDate FROM ReportDownloads WHERE ReportDownloadId = @ReportDownloadId";
            var reqDate = new DateTime();
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@ReportDownloadId", reportDownloadId);
                connection.Open();
                var reader = command.ExecuteReader();

                while (reader.Read()) {
                    reqDate = DateTime.Parse(reader["RequestDate"].ToString());
                }
            }
            return reqDate;
        }
    }
}