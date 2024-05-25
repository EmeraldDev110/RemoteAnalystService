using System.Collections.Generic;
using MySqlConnector;

namespace RemoteAnalyst.UWSLoader.DiskBrowserLookBack {
    public class CurrentTables2 {
        private readonly string _connectionString;

        public CurrentTables2(string connectionString) {
            _connectionString = connectionString;
        }

        public List<string> GetTableNames(string entity) {
            string cmdText = "SELECT TableName from CurrentTables WHERE TableName like '%_" + entity + "_%'";
            var cpuTables = new List<string>();
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                connection.Open();
                var reader = command.ExecuteReader();
                while (reader.Read()) {
                    cpuTables.Add(reader["TableName"].ToString());
                }
            }
            return cpuTables;
        }
    }
}