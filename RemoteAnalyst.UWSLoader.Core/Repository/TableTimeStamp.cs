using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.UWSLoader.Core.Repository {
    class TableTimeStamp {
        private readonly string ConnectionString;

        public TableTimeStamp(string connectionString) {
            ConnectionString = connectionString;
        }

        public void DeleteEntry(string tableName) {
            string cmdText = cmdText = "DELETE FROM TableTimeStamp WHERE TableName = @TableName";
            using (var connection = new MySqlConnection(ConnectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@TableName", tableName);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void DeleteEntry(string tableName, DateTime startTime, DateTime stopTime) {
            string cmdText = cmdText = "DELETE FROM TableTimeStamp WHERE TableName = @TableName AND Start = @Start AND End = @End " +
                                       "AND (ArchiveID is null OR ArchiveID = '') ";
            using (var connection = new MySqlConnection(ConnectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@TableName", tableName);
                command.Parameters.AddWithValue("@Start", startTime);
                command.Parameters.AddWithValue("@End", stopTime);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void InsetEntryFor(string tableName, DateTime startTime, DateTime stopTime, int status) {
            bool duplicate = false;
            duplicate = CheckDuplicate(tableName, startTime, stopTime);
            if (!duplicate) {
                string cmdText = "INSERT INTO TableTimestamp (TableName, Start, End, Status )" +
                                 "VALUES (@TableName, @Start, @End, @Status)";

                try {
                    using (var connection = new MySqlConnection(ConnectionString)) {
                        var command = new MySqlCommand(cmdText, connection);
                        command.Parameters.AddWithValue("@TableName", tableName);
                        command.Parameters.AddWithValue("@Start", startTime);
                        command.Parameters.AddWithValue("@End", stopTime);
                        command.Parameters.AddWithValue("@Status", status);

                        connection.Open();
                        command.ExecuteReader();
                    }
                }
                catch (Exception ex) {
                    throw new Exception(ex.Message);
                }
            }
        }

        public bool CheckTimeOverLap(string tableName, DateTime startTime, DateTime stopTime) {
            DateTime currentStartTime = startTime;
            DateTime currentEndTime = stopTime;
            var oldStartTime = new DateTime();
            var oldEndTime = new DateTime();
            bool timeStampokay = false;

            string cmdText = "SELECT Start, End FROM TableTimestamp WHERE TableName = @TableName";

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
                            //break;
                        }
                        else {
                            //over laps.
                            timeStampokay = false;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }

            return timeStampokay;
        }

        public bool CheckTempTimeOverLap(string tableName, DateTime startTime, DateTime stopTime) {
            DateTime currentStartTime = startTime;
            DateTime currentEndTime = stopTime;
            var oldStartTime = new DateTime();
            var oldEndTime = new DateTime();
            bool timeStampokay = false;

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
                            //break;
                        }
                        else {
                            //over laps.
                            timeStampokay = false;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }

            return timeStampokay;
        }

        internal bool CheckDuplicate(string tableName, DateTime startTime, DateTime stopTime) {
            bool duplicate = false;
            string cmdText = "SELECT * FROM TableTimestamp WHERE " +
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

        public void UpdateStatusUsingTableName(string tableName, DateTime startTime, DateTime stopTime, int status) {
            string cmdText = "UPDATE TableTimestamp SET Status = @Status WHERE TableName = @TableName AND Start = @Start AND End = @End ";
            try {
                using (var connection = new MySqlConnection(ConnectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@TableName", tableName);
                    command.Parameters.AddWithValue("@Start", startTime);
                    command.Parameters.AddWithValue("@End", stopTime);
                    command.Parameters.AddWithValue("@Status", status);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void UpdateStatusUsingArchiveID(string archiveID, int status) {
            string cmdText = "UPDATE TableTimestamp SET Status = @Status WHERE ArchiveID = @ArchiveID";
            try {
                using (var connection = new MySqlConnection(ConnectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@ArchiveID", archiveID);
                    command.Parameters.AddWithValue("@Status", status);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void UpdateArchiveID(string tableName, DateTime startTime, DateTime stopTime, string ArchiveID, DateTime creationDate) {
            string cmdText = "UPDATE TableTimestamp SET ArchiveID = @ArchiveID, CreationDate = @CreationDate WHERE TableName = @TableName " +
                             "AND Start >= @StartTime " +
                             "AND [END] <= @EndTime";
            try {
                using (var connection = new MySqlConnection(ConnectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@TableName", tableName);
                    command.Parameters.AddWithValue("@StartTime", startTime);
                    command.Parameters.AddWithValue("@EndTime", stopTime);
                    command.Parameters.AddWithValue("@ArchiveID", ArchiveID);
                    command.Parameters.AddWithValue("@CreationDate", creationDate);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public string GetArchiveID(string tableName, DateTime startTime, DateTime stopTime) {
            string archiveID = "";
            const string cmdText = "SELECT ArchiveID FROM TableTimestamp " +
                                   "WHERE TableName = @TableName " +
                                   "AND Start = @StartTime AND End = @StopTime ";

            using (var connection = new MySqlConnection(ConnectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@TableName", tableName);
                command.Parameters.AddWithValue("@StartTime", startTime);
                command.Parameters.AddWithValue("@StopTime", stopTime);
                connection.Open();
                var reader = command.ExecuteReader();
                if (reader.Read()) {
                    archiveID = Convert.ToString(reader["ArchiveID"]);
                }
                reader.Close();
            }
            return archiveID;
        }

        public string GetArchiveID(string tableName) {
            string archiveID = "";
            const string cmdText = "SELECT ArchiveID FROM TableTimestamp " +
                                   "WHERE TableName = @TableName";

            using (var connection = new MySqlConnection(ConnectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@TableName", tableName);
                connection.Open();
                var reader = command.ExecuteReader();
                if (reader.Read()) {
                    archiveID = Convert.ToString(reader["ArchiveID"]);
                }
                reader.Close();
            }
            return archiveID;
        }

        public DataTable GetArchiveDetailsPerTable(string tableName) {
            var archiveDetails = new DataTable();
            const string cmdText = "SELECT TableName, Start, End, ArchiveID FROM TableTimestamp " +
                                   "WHERE TableName = @TableName";

            using (var connection = new MySqlConnection(ConnectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@TableName", tableName);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(archiveDetails);
            }
            return archiveDetails;

        }

        //Below query is used for check the archive status, note - for K2, only one file for 24 hours.
        /*public DataTable GetArchiveStatus(string systemSerial, DateTime startTime, DateTime stopTime) {
            var archiveStatus = new DataTable();
            string cmdText = "SELECT DISTINCT CONVERT(char(10), DataDate, 101) AS DataDate, Start, End, T.Status, T.ArchiveID " +
                             "FROM TableTimestamp AS T " +
                             "INNER JOIN CurrentTables AS C ON  " +
                             "T.TableName = C.TableName  " +
                             "WHERE ((@StartTime >= Start AND @StartTime < End OR @EndTime <= End AND @EndTime > Start) " +
                             "OR (Start >= @StartTime AND End < @EndTime))" +
                             "AND SystemSerial = @SystemSerial " +
                             "GROUP BY  DataDate, Start, End, T.Status, T.ArchiveID";

            using (var connection = new MySqlConnection(ConnectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@StartTime", startTime);
                command.Parameters.AddWithValue("@EndTime", stopTime);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(archiveStatus);
            }
            return archiveStatus;
        }*/

        public DataTable GetArchiveStatus(List<string> archiveIDs) {
            var archiveStatus = new DataTable();
            string cmdText = "SELECT DISTINCT Status, ArchiveID FROM TableTimestamp WHERE ArchiveID IN ( ";
            for (var i = 0; i < archiveIDs.Count; i++) {
                cmdText += "'" + archiveIDs[i] + "'";
                if (i != archiveIDs.Count - 1)
                    cmdText += ",";
            }
            cmdText += ")";

            using (var connection = new MySqlConnection(ConnectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(archiveStatus);
            }
            return archiveStatus;
        }

        public DataTable GetArchivesForCleanup() {
            var archives = new DataTable();
            string cmdText = "SELECT TableName, ArchiveID, CreationDate " +
                             "FROM TableTimestamp " +
                             "WHERE ArchiveID is not null ";
            using (var connection = new MySqlConnection(ConnectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(archives);
            }
            return archives;
        }
    }
}
