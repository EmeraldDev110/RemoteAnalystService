using System;
using System.Data;
using MySqlConnector;

namespace RemoteAnalyst.Repository.MySQLConcrete.RemoteAnalystdb {
    public class TransactionProfiles {
        private readonly string _connectionString;

        public TransactionProfiles(string connectionString) {
            _connectionString = connectionString;
        }

        public DataTable GetTransactionProfileInfo(string systemSerial) {
            var transactionProfile = new DataTable();
            string cmdText = @"SELECT 
                              TransactionProfileID, TransactionFile, OpenerType, OpenerName, TransactionCounter, IOTransactionRatio, IsCpuToFile
                              FROM TransactionProfiles WHERE SystemSerial = @SystemSerial";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection) {
                    CommandTimeout = 0
                };

                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                var adapter = new MySqlDataAdapter(command);

                adapter.Fill(transactionProfile);
            }

            return transactionProfile;
        }

        public DataTable GetTransactionProfileInfo(string systemSerial, int profileId) {
            var transactionProfile = new DataTable();
            string cmdText = @"SELECT 
                              TransactionProfileID, TransactionFile, OpenerType, OpenerName, TransactionCounter, IOTransactionRatio, IsCpuToFile
                              FROM TransactionProfiles WHERE SystemSerial = @SystemSerial AND TransactionProfileID = @TransactionProfileID";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection) {
                    CommandTimeout = 0
                };

                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@TransactionProfileID", profileId);
                var adapter = new MySqlDataAdapter(command);

                adapter.Fill(transactionProfile);
            }

            return transactionProfile;
        }

        public string GetTransactionProfileName(int profileId) {
            string cmdText = "SELECT TransactionProfileName FROM TransactionProfiles WHERE TransactionProfileID = @TransactionProfileID";
            string profileName = "";
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection) {
                    CommandTimeout = 0
                };
                command.Parameters.AddWithValue("@TransactionProfileID", profileId);
                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read()) {
                    profileName = Convert.ToString(reader["TransactionProfileName"]);
                }
            }
            return profileName;
        }
    }
}
