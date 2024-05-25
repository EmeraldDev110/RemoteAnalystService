using System;
using System.Collections.Generic;
using MySqlConnector;

namespace RemoteAnalyst.Repository.MySQLConcrete.RemoteAnalystdb {
    public class TransactionProfileEmail {
        private readonly string _connectionString;

        public TransactionProfileEmail(string connectionString) {
            _connectionString = connectionString;
        }

        public List<string> GetTransactionProfileEmail(int id) {
            var emails = new List<string>();
            try {
                const string cmdText = "SELECT Email FROM TransactionProfileEmails WHERE TransactionProfileId = @TransactionProfileId ";
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection) {
                        CommandTimeout = 0
                    };

                    command.Parameters.AddWithValue("@TransactionProfileId", id);
                    connection.Open();
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read()) {
                        emails.Add(reader["Email"].ToString());
                    }
                }
            }
            catch (Exception) {
            }
            return emails;
        }
    }
}