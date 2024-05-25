using System;
using System.Data;
using MySqlConnector;
using RemoteAnalyst.Repository.Infrastructure;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb
{
    public class DataDictionary
    {
        private readonly string ConnectionString = "";

        public DataDictionary(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public DataTable GetColumns(int indexName, string tableName)
        {
            //string connectionString = Config.ConnectionString;

            string cmdText = "SELECT ColumnName, ColumnType, ColumnSize, Website FROM " + tableName +
                             " WHERE EntityID = @EntityID " +
                             "ORDER BY ColumnOrder";
            var columnInfo = new DataTable();

            using (var connection = new MySqlConnection(ConnectionString))
            {
                var command = new MySqlCommand(cmdText + Helper.CommandParameter, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@EntityID", indexName);
                connection.Open();

                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(columnInfo);
            }

            return columnInfo;
        }

        public DataTable GetPathwayColumns(string tableName)
        {
            //string connectionString = Config.RAConnectionString;

            string cmdText = "SELECT FName, FType, FSize FROM _PvTableTable" +
                             " WHERE TableName = @TableName " +
                             "ORDER BY FSeq";
            var columnInfo = new DataTable();

            using (var connection = new MySqlConnection(ConnectionString))
            {
                var command = new MySqlCommand(cmdText + Helper.CommandParameter, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@TableName", tableName);
                connection.Open();

                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(columnInfo);
            }

            return columnInfo;
        }

    }
}