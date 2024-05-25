using System;
using System.Collections.Generic;
using MySqlConnector;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.UWSLoader.Core.Repository {
    class TempTableTimestamp {
        private readonly string ConnectionString = "";

        public TempTableTimestamp(string connectionString) {
            ConnectionString = connectionString;
        }

        public void InsertTempTimeStamp(string tableName, DateTime startTime, DateTime stopTime) {
            bool duplicate = false;
            duplicate = CheckDuplicate(tableName, startTime, stopTime);
            if (!duplicate) {
                //string connectionString = Config.ConnectionString;
                string cmdText = "INSERT INTO TempTableTimestamp (TableName, Start, End)" +
                                 "VALUES (@TableName, @Start, @End)";

                try {
                    using (var connection = new MySqlConnection(ConnectionString)) {
                        var command = new MySqlCommand(cmdText, connection);
                        command.Parameters.AddWithValue("@TableName", tableName);
                        command.Parameters.AddWithValue("@Start", startTime);
                        command.Parameters.AddWithValue("@End", stopTime);

                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex) {
                    throw new Exception(ex.Message);
                }
            }
        }

        private bool CheckDuplicate(string tableName, DateTime startTime, DateTime stopTime) {
            bool duplicate = false;
            //string connectionString = Config.ConnectionString;
            string cmdText = "SELECT * FROM TempTableTimestamp WHERE " +
                             "TableName = @TableName AND Start = @Start AND End = @End LIMIT 1";

            try {
                using (var connection = new MySqlConnection(ConnectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@TableName", tableName);
                    command.Parameters.AddWithValue("@Start", startTime);
                    command.Parameters.AddWithValue("@End", stopTime);

                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read()) {
                        duplicate = true;
                    }
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }

            return duplicate;
        }

        internal bool CheckTimeOverLap(string tableName, DateTime startTime, DateTime stopTime) {
            DateTime currentStartTime = startTime;
            DateTime currentEndTime = stopTime;
            var oldStartTime = new DateTime();
            var oldEndTime = new DateTime();
            bool timeStampokay = false;

            //string connectionString = Config.ConnectionString;
            string cmdText = "SELECT Start, End FROM TempTableTimestamp WHERE TableName = @TableName";

            try {
                using (var connection = new MySqlConnection(ConnectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@TableName", tableName);

                    connection.Open();
                    var reader = command.ExecuteReader();

                    while (reader.Read()) {
                        oldStartTime = Convert.ToDateTime(reader["Start"]);
                        oldEndTime = Convert.ToDateTime(reader["End"]);

                        //Compare Time. Got this code from David's collector code.
                        if (currentEndTime <= oldStartTime || currentStartTime >= oldEndTime) {
                            //Okay to load.
                            timeStampokay = true;
                            break;
                        }
                        //over laps.
                        timeStampokay = false;
                        break;
                    }
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }

            return timeStampokay;
        }

        public void DeleteTempTimeStamp(string tableName) {
            //string connectionString = Config.ConnectionString;
            string cmdText = "DELETE FROM TempTableTimestamp WHERE TableName = @TableName";

            try {
                using (var connection = new MySqlConnection(ConnectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@TableName", tableName);
                    connection.Open();

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }
    }
}
