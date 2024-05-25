using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class SystemSerialConversions {
        private readonly string _connectionString;

        public SystemSerialConversions(string connectionString) {
            _connectionString = connectionString;
        }

        public string GetNewSystemSerial(string systemSerial) {
            string newSystemSerial = string.Empty;
            string cmdText = "SELECT NewSystemSerial FROM SystemSerialConversions WHERE SystemSerial = @SystemSerial";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                connection.Open();
                var reader = command.ExecuteReader();

                if (reader.Read())
                    newSystemSerial = reader["NewSystemSerial"].ToString();
                reader.Close();
            }
            return newSystemSerial;
        }
    }
}
