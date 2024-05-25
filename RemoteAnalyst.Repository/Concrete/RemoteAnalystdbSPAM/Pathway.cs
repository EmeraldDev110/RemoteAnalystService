using System;
using System.Collections.Generic;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM {
    public class Pathway {
        private readonly string _connectionString;

        public Pathway(string connectionStringTrend) {
            _connectionString = connectionStringTrend;
        }

        public List<string> GetListOfPathwayTables() {
            string cmdText = @"SELECT Table_Name AS Name FROM information_schema.tables 
                                WHERE Table_Name LIKE 'Pv%'
                                GROUP BY Table_Name";
            var tables = new List<string>();
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                connection.Open();
                var reader = command.ExecuteReader();
                while (reader.Read()) {
                    tables.Add(Convert.ToString(reader["Name"]));
                }
            }
            return tables;
        } 

        public void DeleteData(DateTime oldDate, string tableName) {
            string cmdText = "DELETE FROM " + tableName + " WHERE FromTimestamp < @OldDate";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@OldDate", oldDate);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}