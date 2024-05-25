

using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class TMonFileNames {
        private readonly string _connectionString = "";

        public TMonFileNames(string connectionString) {
            _connectionString = connectionString;
        }

        public string GetExpectedFileName(string systemSerial, string interval) {
            string fileName = "";
            const string cmdText = "SELECT FileNameString FROM TMonFileNames " +
                                   "WHERE SystemSerial = @SystemSerial AND `Interval` = @Interval";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection) { CommandTimeout = 0 };
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@Interval", interval);
                connection.Open();
                var reader = command.ExecuteReader();
                if (reader.Read()) {
                    fileName = reader["FileNameString"].ToString().Trim();
                }
                reader.Close();
            }
            return fileName;
        }
    }
}