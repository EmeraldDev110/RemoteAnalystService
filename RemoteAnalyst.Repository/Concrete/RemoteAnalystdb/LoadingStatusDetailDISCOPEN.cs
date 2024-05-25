using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class LoadingStatusDetailDISCOPEN {
        private readonly string _connectionString;

        public LoadingStatusDetailDISCOPEN(string connectionString) {
            _connectionString = connectionString;
        }

        public int GetCurrentQueLength(string instanceID) {
            string cmdText = "SELECT COUNT(LoadingQueDISCOPENID) AS `Count` FROM LoadingStatusDetailDISCOPEN WHERE flag = 0 AND InstanceID = @InstanceID";
            int queueLength = 0;
            using (var connection = new MySqlConnection(_connectionString)) {
                try {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    command.Parameters.AddWithValue("@InstanceID", instanceID);
                    connection.Open();
                    var reader = command.ExecuteReader();

                    if (reader.Read()) {
                        queueLength = Convert.ToInt32(reader["Count"]);
                    }
                }
                catch { }
            }
            return queueLength;
        }

        public int GetCurrentLoadLength(string instanceID) {
            string cmdText = "SELECT COUNT(LoadingQueDISCOPENID) AS `Count` FROM LoadingStatusDetailDISCOPEN WHERE flag = 1 AND InstanceID = @InstanceID";
            int queueLength = 0;
            using (var connection = new MySqlConnection(_connectionString)) {
                try {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    command.Parameters.AddWithValue("@InstanceID", instanceID);
                    connection.Open();
                    var reader = command.ExecuteReader();

                    if (reader.Read()) {
                        queueLength = Convert.ToInt32(reader["Count"]);
                    }
                }
                catch { }
            }
            return queueLength;
        }

        public bool InsertLoadingStatus(string fileNmae, DateTime inQueTime, string systemSerial, string jobPoolName
                                        , DateTime selectedStartTime, DateTime selectedStopTime, string instanceID) {
                                            string cmdText = "INSERT INTO LoadingStatusDetailDISCOPEN (FileName, InQueTime, SystemSerial, Flag, JobPoolName, SelectedStartTime, SelectedStopTime, InstanceID) " +
            "VALUES (@FileName, @InQueTime, @SystemSerial, @Flag, @JobPoolName, @SelectedStartTime, @SelectedStopTime, @InstanceID)";
            bool returnValue;
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@FileName", fileNmae);
                command.Parameters.AddWithValue("@InQueTime", inQueTime);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@Flag", "0");
                command.Parameters.AddWithValue("@JobPoolName", jobPoolName);
                command.Parameters.AddWithValue("@SelectedStartTime", selectedStartTime);
                command.Parameters.AddWithValue("@SelectedStopTime", selectedStopTime);
                command.Parameters.AddWithValue("@InstanceID", instanceID);
                try {
                    connection.Open();
                    command.ExecuteNonQuery();
                    returnValue = true;
                }
                catch {
                    returnValue = false;
                }
            }
            return returnValue;
        }

        public bool DeleteLoadingInfo(string fileNmae, string instanceID) {
            string cmdText = "DELETE FROM LoadingStatusDetailDISCOPEN WHERE FileName = @FileName AND InstanceID = @InstanceID";
            bool returnValue;
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@FileName", fileNmae);
                command.Parameters.AddWithValue("@InstanceID", instanceID);
                try {
                    connection.Open();
                    command.ExecuteNonQuery();
                    returnValue = true;
                }
                catch {
                    returnValue = false;
                }
            }
            return returnValue;
        }

        public bool DeleteLoadingInfo(int loadingQueDISCOPENID) {
            string cmdText = "DELETE FROM LoadingStatusDetailDISCOPEN WHERE LoadingQueDISCOPENID = @LoadingQueDISCOPENID";
            bool returnValue;
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@LoadingQueDISCOPENID", loadingQueDISCOPENID);
                try {
                    connection.Open();
                    command.ExecuteNonQuery();
                    returnValue = true;
                }
                catch {
                    returnValue = false;
                }
            }
            return returnValue;
        }

        public bool UpdateLoadingStatusDetailDISCOPEN(string flag, DateTime processingTime, string fileName, string systemSerial, string instanceID) {
            string cmdText = "UPDATE LoadingStatusDetailDISCOPEN SET Flag = @Flag, StartProcessingTime = @ProcessingTime " +
                             "WHERE FileName = @FileName AND SystemSerial = @SystemSerial AND InstanceID = @InstanceID";
            bool retVal;
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@Flag", flag);
                command.Parameters.AddWithValue("@ProcessingTime", processingTime);
                command.Parameters.AddWithValue("@FileName", fileName);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@InstanceID", instanceID);
                try {
                    connection.Open();
                    command.ExecuteNonQuery();
                    retVal = true;
                }
                catch {
                    retVal = false;
                }
            }
            return retVal;
        }

        public DataTable GetLoadingStatusDetail(string instanceID) {
            string cmdText = "SELECT FileName, SystemSerial, SelectedStartTime, SelectedStopTime, ArchiveId " +
                             "FROM LoadingStatusDetailDISCOPEN  WHERE Flag != 1  AND InstanceID = @InstanceID ORDER BY LoadingQueDISCOPENID LIMIT 1";

            var myDataTable = new DataTable();

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@InstanceID", instanceID);

                var adapter = new MySqlDataAdapter(command);

                try {
                    connection.Open();
                    adapter.Fill(myDataTable);
                }
                finally {
                    connection.Close();
                }
            }
            return myDataTable;
        }

        public DataTable GetStoppedJobs(string instacneID) {
            const string cmdText = "SELECT LoadingQueDISCOPENID, SystemSerial, FileName FROM LoadingStatusDetailDISCOPEN " +
                                   "WHERE InstanceID = @InstanceID AND Flag = 1";
            var stoppedJobs = new DataTable();
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@InstanceID", instacneID);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(stoppedJobs);
            }

            return stoppedJobs;
        }
    }
}
