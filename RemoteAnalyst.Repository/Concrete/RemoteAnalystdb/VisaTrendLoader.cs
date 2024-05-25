using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class VisaTrendLoader {
        private readonly string _connectionString = "";

        public VisaTrendLoader(string connectionString) {
            _connectionString = connectionString;
        }

        public void InsertEntry(string systemSerial, DateTime dataDate) {
            string cmdText = "INSERT INTO VisaTrendLoader (SystemSerial, DataDate) VALUES (@SystemSerial, @DataDate)";

            //Get last date from DailySysUnrated.
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@DataDate", dataDate);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public bool CheckEntry(string systemSerial, DateTime dataDate) {
            bool exists = false;
            string cmdText = @"SELECT SystemSerial FROM VisaTrendLoader WHERE 
                                SystemSerial = @SystemSerial AND 
                                DataDate = @DataDate";

            //Get last date from DailySysUnrated.
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@DataDate", dataDate);
                connection.Open();

                var reader = command.ExecuteReader();
                if (reader.Read()) {
                    exists = true;
                }
            }
            return exists;
        }

    }
}
