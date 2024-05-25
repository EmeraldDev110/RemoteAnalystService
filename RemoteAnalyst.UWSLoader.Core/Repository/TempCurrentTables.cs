using System;
using System.Collections.Generic;
using MySqlConnector;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.UWSLoader.Core.Repository {
    class TempCurrentTables {
        private readonly string ConnectionString = "";

        public TempCurrentTables(string connectionString) {
            ConnectionString = connectionString;
        }

        public void InsertCurrentTable(string tableName, int entityID, long interval, DateTime startTime,
            string UWSSerialNumber, string measVersion) {
            //string connectionString = Config.ConnectionString;
            string cmdText = "INSERT INTO TempCurrentTables (TableName, EntityID, " +
                             "SystemSerial, `Interval`, DataDate, MeasureVersion) " +
                             "VALUES (@TableName, @EntityID, @SystemSerial, @Interval, @DataDate, @MeasureVersion)";

            //Check for Measure Version.
            string measureVersion = string.Empty;
            measureVersion = measVersion;

            try {
                using (var connection = new MySqlConnection(ConnectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@TableName", tableName);
                    command.Parameters.AddWithValue("@EntityID", entityID);
                    command.Parameters.AddWithValue("@SystemSerial", UWSSerialNumber);
                    command.Parameters.AddWithValue("@Interval", interval);
                    command.Parameters.AddWithValue("@DataDate", startTime);
                    command.Parameters.AddWithValue("@MeasureVersion", measureVersion);

                    connection.Open();
                    command.ExecuteReader();
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public long GetInterval(string buildTableName) {
            //string connectionString = Config.ConnectionString;
            string cmdText = "SELECT `Interval` FROM TempCurrentTables WHERE TableName = @TableName";
            long currentInterval = 0;

            try {
                using (var connection = new MySqlConnection(ConnectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@TableName", buildTableName);

                    connection.Open();
                    var reader = command.ExecuteReader();

                    if (reader.Read()) {
                        currentInterval = Convert.ToInt64(reader["Interval"]);
                    }
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }

            return currentInterval;
        }

        public void DeleteCurrentTable(string tableName) {
            //string connectionString = Config.ConnectionString;
            string cmdText = "DELETE FROM TempCurrentTables WHERE TableName = @TableName";

            try {
                using (var connection = new MySqlConnection(ConnectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@TableName", tableName);

                    connection.Open();
                    command.ExecuteReader();
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }
    }
}
