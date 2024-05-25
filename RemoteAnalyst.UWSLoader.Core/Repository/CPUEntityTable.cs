using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.UWSLoader.Core.Repository {
    class CPUEntityTable {
        private readonly string _connectionString;

        public CPUEntityTable(string connectionString) {
            this._connectionString = connectionString;
        }

        public int GetCPUEntityTableColumnCount(string systemSerial, string databaseName, string entityTableName) {
            /*string cmdText = "SELECT COUNT(*) AS ColumnCount " +
                             "FROM [" + databaseName + "].sys.columns " +
                             "WHERE object_id = OBJECT_ID('[" + databaseName + "].[dbo].[" + entityTableName + "]')";*/


            string cmdText = @"SELECT COUNT(*) AS ColumnCount FROM INFORMATION_SCHEMA.COLUMNS 
                                WHERE TABLE_NAME = @TableName AND TABLE_SCHEMA = @DatabaseName";
            int num = 0;
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@TableName", entityTableName);
                command.Parameters.AddWithValue("@DatabaseName", databaseName);
                connection.Open();
                var reader = command.ExecuteReader();
                if (reader.Read()) {
                    num = Convert.ToInt16(reader["ColumnCount"]);
                }
                reader.Close();
            }
            return num;
        }

        public DataTable GetCPUEntityTableIntervalList(string entityTableName) {
            var intervalList = new DataTable();
            try {
                string cmdText = "SELECT DISTINCT FromTimestamp, ToTimestamp FROM " + entityTableName;
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    var adapter = new MySqlDataAdapter(command);
                    adapter.Fill(intervalList);
                }
            }
            catch (Exception) {
            }
            return intervalList;
        }
    }
}
