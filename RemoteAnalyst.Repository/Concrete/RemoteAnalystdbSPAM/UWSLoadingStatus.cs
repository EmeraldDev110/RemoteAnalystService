using System;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM {
    public class UWSLoadingStatus {
        private readonly string _connectionString = "";

        public UWSLoadingStatus(string connectionString) {
            _connectionString = connectionString;
        }

        public bool CheckUWSLoadingStatus(string systemSerial, string fileName) {
            //string connectionString = Config.ConnectionString;
            string cmdText = "SELECT Status FROM UWSLoadingStatus WHERE SystemSerial = @SystemSerial AND FileName = @FileName";
            bool entry = false;

            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                    command.Parameters.AddWithValue("@FileName", fileName);
                    connection.Open();

                    MySqlDataReader reader = command.ExecuteReader();

                    if (reader.Read()) {
                        entry = true;
                    }
                }
            }
            catch (Exception) {
                return false;
            }

            return entry;
        }

        public void InsertUWSLoadingStatus(string systemSerial, string fileName) {
            string cmdText = @"INSERT INTO UWSLoadingStatus (SystemSerial, FileName, Status) VALUES (@SystemSerial, @FileName, @Status)";

            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                    command.Parameters.AddWithValue("@FileName", fileName);
                    command.Parameters.AddWithValue("@Status", 1); //1 = Loading.
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void DeleteUWSLoadingStatus(string systemSerial, string fileName) {
            string cmdText = @"DELETE FROM UWSLoadingStatus
                               WHERE SystemSerial = @SystemSerial
                               AND FileName = @FileName";

            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                    command.Parameters.AddWithValue("@FileName", fileName);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }
    }
}