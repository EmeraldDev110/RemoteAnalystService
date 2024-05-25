using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.UWSLoader.Core.Repository {
    class DataDictionary {
        private readonly string ConnectionString = "";

        public DataDictionary(string connectionString) {
            ConnectionString = connectionString;
        }

        public DataTable GetColumns(int indexName, string tableName) {
            //string connectionString = Config.ConnectionString;

            string cmdText = "SELECT ColumnName, ColumnType, ColumnSize, Website FROM " + tableName +
                             " WHERE EntityID = @EntityID " +
                             "ORDER BY ColumnOrder";
            var columnInfo = new DataTable();

            using (var connection = new MySqlConnection(ConnectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@EntityID", indexName);
                connection.Open();

                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(columnInfo);
            }

            return columnInfo;
        }

        public DataTable GetRAColumns(string tableName) {
            //string connectionString = Config.ConnectionString;

            string cmdText = "SELECT FName, ColumnType, ColumnSize FROM _RadcTableTable" +
                             " WHERE TableName = @TableName " +
                             "ORDER BY FSeq";
            var columnInfo = new DataTable();

            using (var connection = new MySqlConnection(ConnectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@TableName", tableName);
                connection.Open();

                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(columnInfo);
            }

            return columnInfo;
        }

        public DataTable GetPathwayColumns(string tableName) {
            //string connectionString = Config.RAConnectionString;

            string cmdText = "SELECT FName, FType, FSize FROM _PvTableTable" +
                             " WHERE TableName = @TableName " +
                             "ORDER BY FSeq";
            var columnInfo = new DataTable();

            using (var connection = new MySqlConnection(ConnectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@TableName", tableName);
                connection.Open();

                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(columnInfo);
            }

            return columnInfo;
        }
    }
}
