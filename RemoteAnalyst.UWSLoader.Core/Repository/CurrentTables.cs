using System;
using System.Collections.Generic;
using MySqlConnector;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.UWSLoader.Core.Repository {
    class CurrentTables {
        private readonly string _connectionString;

        public CurrentTables(string connectionString) {
            _connectionString = connectionString;
        }

        public void DeleteEntry(string tableName) {
            string cmdText = "DELETE FROM CurrentTables WHERE TableName = @TableName";
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@TableName", tableName);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void InsertEntry(string tableName, int entityID, long interval, DateTime startTime,
            string UWSSerialNumber, string measVersion) {
            string cmdText = "INSERT INTO CurrentTables (TableName, EntityID, " +
                             "SystemSerial, `Interval`, DataDate, MeasureVersion) " +
                             "VALUES (@TableName, @EntityID, @SystemSerial, @Interval, @DataDate, @MeasureVersion)";
            string measureVersion = string.Empty;

            measureVersion = measVersion;

            try {
                using (var connection = new MySqlConnection(_connectionString)) {
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
            string cmdText = "SELECT `Interval` FROM CurrentTables WHERE TableName = @TableName";
            long currentInterval = 0;

            try {
                using (var connection = new MySqlConnection(_connectionString)) {
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

        public List<int> GetEntities(DateTime startDateTime, DateTime stopDateTime) {
            //string connectionString = Config.ConnectionString;
            var cmdText = @"SELECT DISTINCT(EntityID) FROM CurrentTables AS C
                            INNER JOIN TableTimestamp AS T ON C.TableName = T.TableName
                            WHERE Start >= @StartTime AND 
                            End <= @StopTime";
            var entities = new List<int>();

            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@StartTime", startDateTime);
                    command.Parameters.AddWithValue("@StopTime", stopDateTime);

                    connection.Open();
                    var reader = command.ExecuteReader();

                    while (reader.Read()) {
                        entities.Add(Convert.ToInt32(reader["EntityID"]));
                    }
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }

            return entities;
        }

        public string GetCurrentName(DateTime currentTime) {
            string cmdText = "SELECT TableName FROM CurrentTables WHERE DataDate >= @DataDate ORDER BY DataDate LIMIT 1";
            var tableName = "";
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@DataDate", currentTime.Date);

                connection.Open();
                var reader = command.ExecuteReader();

                if (reader.Read()) {
                    tableName = Convert.ToString(reader["TableName"]);
                }
            }
            return tableName;
        }
    }
}
