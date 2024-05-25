using System;
using MySqlConnector;

namespace RemoteAnalyst.UWSLoader.Core.BaseClass
{
    public class RAInfo {
        private readonly string _connectionString;

        public RAInfo(string connectionString) {
            _connectionString = connectionString;
        }

        public string GetQueryValue(string key) {
            string retVal = "";

            string cmdText = "SELECT queryValue FROM RAInfo WHERE queryKey = @Key";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@Key", key);
                connection.Open();
                var reader = command.ExecuteReader();
                if (reader.Read()) {
                    retVal = reader["queryValue"].ToString();
                }
            }

            return retVal;
        }

        public int GetMaxQueue(string key) {
            int maxQueue = 1; //Set default to 1.

            string cmdText = "SELECT queryValue FROM RAInfo WHERE queryKey = @Key";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@Key", key);
                connection.Open();
                var reader = command.ExecuteReader();
                if (reader.Read()) {
                    maxQueue = Convert.ToInt32(reader["queryValue"]);
                }
            }

            return maxQueue;
        }
        public string GetValue(string key) {
            const string cmdText = "SELECT queryValue FROM RAInfo WHERE queryKey = @Key";
            var returnValue = "";
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@Key", key);

                connection.Open();
                var reader = command.ExecuteReader();
                if (reader.Read()) {
                    returnValue = reader["queryValue"].ToString();
                }
                reader.Close();
            }
            return returnValue;
        }
    }
}