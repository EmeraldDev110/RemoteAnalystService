using System;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class UWSFileCounts {
        private readonly string _connectionString = "";

        public UWSFileCounts(string connectionString) {
            _connectionString = connectionString;
        }

        public void InsertFileInfo(string systemSerial, DateTime dataDate, string fileName, long fileSize, int expectedFileCount) {
            string cmdText = @"INSERT INTO UWSFileCounts (`SystemSerial`, `DataDate`, `FileName`, `FileSize`, `ExpectedFileCount`, `ActualFileCount`, `ReceivedDate`) VALUES
                             (@SystemSerial, @DataDate, @FileName, @FileSize, @ExpectedFileCount, @ActualFileCount, @ReceivedDate)";
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText + Infrastructure.Helper.CommandParameter, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@DataDate", dataDate);
                command.Parameters.AddWithValue("@FileName", fileName);
                command.Parameters.AddWithValue("@FileSize", fileSize);
                command.Parameters.AddWithValue("@ExpectedFileCount", expectedFileCount);
                command.Parameters.AddWithValue("@ActualFileCount", 0);
                command.Parameters.AddWithValue("@ReceivedDate", DateTime.Now);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public bool CheckDuplicate(string systemSerial, string fileName) {
            string cmdText = @"SELECT ExpectedFileCount FROM UWSFileCounts WHERE `SystemSerial` = @SystemSerial AND `FileName` = @FileName";
            bool isExist = false;
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText + Infrastructure.Helper.CommandParameter, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@FileName", fileName);
                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();
                if (reader.Read()) {
                    isExist = true;
                }
            }

            return isExist;
        }

        public bool CheckDuplicate(string systemSerial, DateTime dataDate) {
            string cmdText = @"SELECT ExpectedFileCount FROM UWSFileCounts WHERE `SystemSerial` = @SystemSerial AND `DataDate` = @DataDate";
            bool isExist = false;
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText + Infrastructure.Helper.CommandParameter, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@DataDate", dataDate);
                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();
                if (reader.Read()) {
                    isExist = true;
                }
            }

            return isExist;
        }

        public void UpdateActualFileCount(string systemSerial, DateTime dataDate) {
            string cmdText = @"UPDATE UWSFileCounts SET ActualFileCount = ActualFileCount + 1
                             WHERE `SystemSerial` = @SystemSerial AND `DataDate` = @DataDate";
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText + Infrastructure.Helper.CommandParameter, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@DataDate", dataDate);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public int GetExpectedFileCount(string systemSerial, DateTime dataDate) {
            string cmdText = @"SELECT SUM(ExpectedFileCount) AS ExpectedFileCount FROM UWSFileCounts WHERE `SystemSerial` = @SystemSerial AND `DataDate` = @DataDate";
            int expectedFileCount = 0;

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText + Infrastructure.Helper.CommandParameter, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@DataDate", dataDate);
                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();
                if (reader.Read()) {
                    expectedFileCount = Convert.ToInt32(reader["ExpectedFileCount"]);
                }
            }

            return expectedFileCount;
        }

        public int GetActualFileCount(string systemSerial, DateTime dataDate) {
            string cmdText = @"SELECT ActualFileCount FROM UWSFileCounts WHERE `SystemSerial` = @SystemSerial AND `DataDate` = @DataDate LIMIT 1";
            int actualFileCount = 0;
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText + Infrastructure.Helper.CommandParameter, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@DataDate", dataDate);
                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();
                if (reader.Read()) {
                    actualFileCount = Convert.ToInt32(reader["ActualFileCount"]);
                }
            }

            return actualFileCount;
        }
    }
}