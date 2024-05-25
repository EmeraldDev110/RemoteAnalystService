using System;
using MySqlConnector;
using log4net;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class QueryHelper {

        public void ExecuteQuery(string connectionString, string query, ILog log) {
            try {
                using (var connection = new MySqlConnection(connectionString)) {
                    var command = new MySqlCommand(query, connection);
                    command.CommandTimeout = 0;
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            } catch (Exception ex) {
                log.ErrorFormat("Error with the following exception: {0}", ex.Message);
                log.DebugFormat("Query to be run: {0}", query);
            }
        }

        public bool HasPrimaryKey(string connectionString, string tableName, ILog log) {
            try {
                var cmdText = "SHOW KEYS FROM `" + tableName + "` WHERE Key_name = 'PRIMARY'";
                bool hasPrimaryKey = false;

                using (var connection = new MySqlConnection(connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read()) {
                        hasPrimaryKey = true;
                    }
                }
                return hasPrimaryKey;
                
            } 
            catch (Exception ex)
            {
                log.ErrorFormat("Error with the following exception: {0}", ex.Message);
                log.DebugFormat("Error finding primary key in table: {0}", tableName);
                return false;
            }
        }
    }
}
