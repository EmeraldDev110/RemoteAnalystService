using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using MySqlConnector;
using RemoteAnalyst.Repository.Repositories;
using RemoteAnalyst.Repository.Resources;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class LoadingStatusDetail {
        private readonly string _connectionString;
        private readonly bool _isLocalAnalyst = false;

        public LoadingStatusDetail(string connectionString) {
            _connectionString = connectionString;
            
            RAInfoRepository raInfo = new RAInfoRepository();
            string productName = raInfo.GetValue("ProductName");
            if (productName == "PMC")
            {
                _isLocalAnalyst = true;
            }
        }

        public string encryptPassword(string connectionString)
        {
            if (_isLocalAnalyst)
            {
                var encrypt = new Decrypt();
                var encryptedString = encrypt.strDESEncrypt(connectionString);
                return encryptedString;
            }
            else
            {
                return connectionString;
            }
        }

        public IDictionary<int, DateTime> GetProcessingTime(string uwsFileName, string systemSerial) {
            string cmdText = "SELECT LoadingQueID, StartProcessingTime FROM LoadingStatusDetail WHERE " +
                             "FileName = @FileName AND SystemSerial = @SystemSerial ";
            IDictionary<int, DateTime> loadingTime = new Dictionary<int, DateTime>();
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@FileName", uwsFileName);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                connection.Open();
                var reader = command.ExecuteReader();

                if (reader.Read()) {
                    loadingTime.Add(Convert.ToInt32(reader["LoadingQueID"]),
                        Convert.ToDateTime(reader["StartProcessingTime"]));
                }
            }
            return loadingTime;
        }

        public DateTime GetProcessingTime(int loadingQueID) {
            string cmdText = "SELECT StartProcessingTime FROM LoadingStatusDetail WHERE LoadingQueID = @LoadingQueID";
            DateTime loadingTime = DateTime.MinValue;
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@LoadingQueID", loadingQueID);
                connection.Open();
                var reader = command.ExecuteReader();

                if (reader.Read()) {
                    loadingTime = Convert.ToDateTime(reader["StartProcessingTime"]);
                }
            }
            return loadingTime;
        }

        public int GetCurrentQueueLength(string instanceID) {
            string cmdText = "SELECT COUNT(LoadingQueID) AS `Count` FROM LoadingStatusDetail WHERE flag = 0 AND InstanceID = @InstanceID";
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

        public bool CheckDuplicatedUWS(string fileName) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            bool returnValue = false;
            string cmdText = "SELECT LoadingQueID FROM LoadingStatusDetail WHERE FileName = @FileName";

            var connection = new MySqlConnection(_connectionString);
            var command = new MySqlCommand(cmdText, connection);
            command.CommandTimeout = 0;
            command.Parameters.AddWithValue("@FileName", fileName);
            try {
                connection.Open();
                var reader = command.ExecuteReader();

                if (reader.Read()) {
                    returnValue = true;
                }
                reader.Close();
            }
            catch {
                returnValue = false;
            }
            finally {
                connection.Close();
            }

            return returnValue;
        }

        public bool InsertLoadingStatus(string fileNmae, string customerLogin, DateTime inQueTime, string systemSerial,
            string jobPoolName, int tempUWSID, long fileSize, string type, string instanceID) {
            string cmdText = "INSERT INTO LoadingStatusDetail (FileName, CustomerLogin, InQueTime, " +
                             "SystemSerial, Flag, JobPoolName, TempUWSID, FileSize, Type, InstanceID) " +
                             "VALUES (@FileName, @CustomerLogin, @InQueTime, @SystemSerial, @Flag, " +
                             "@JobPoolName, @tempUWSID, @FileSize, @Type, @InstanceID)";
            bool returnValue;
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@FileName", fileNmae);
                command.Parameters.AddWithValue("@CustomerLogin", customerLogin);
                command.Parameters.AddWithValue("@InQueTime", inQueTime);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@Flag", "0");
                command.Parameters.AddWithValue("@JobPoolName", jobPoolName);
                command.Parameters.AddWithValue("@tempUWSID", tempUWSID);
                command.Parameters.AddWithValue("@FileSize", Convert.ToInt64(fileSize));
                command.Parameters.AddWithValue("@Type", type);
                command.Parameters.AddWithValue("@InstanceID", instanceID);
                try {
                    connection.Open();
                    command.ExecuteNonQuery();
                    returnValue = true;
                }
                catch {
                    returnValue = false;
                }
                finally {
                    connection.Close();
                    connection.Dispose();
                }
            }
            return returnValue;
        }

        public bool DeleteLoadingInfo(string fileNmae) {
            string cmdText = "DELETE FROM LoadingStatusDetail WHERE FileName = @FileName";
            bool returnValue;
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@FileName", fileNmae);
                try {
                    connection.Open();
                    command.ExecuteNonQuery();
                    returnValue = true;
                }
                catch {
                    returnValue = false;
                }
                finally {
                    connection.Close();
                    connection.Dispose();
                }
            }
            return returnValue;
        }

        public bool UpdateLoadingStatusDetail(string flag, DateTime processingTime, string fileName, string systemSerial) {
            string cmdText = "UPDATE LoadingStatusDetail SET Flag = @Flag, StartProcessingTime = @ProcessingTime " +
                             "WHERE FileName = @FileName AND SystemSerial = @SystemSerial";
            bool retVal;
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@Flag", flag);
                command.Parameters.AddWithValue("@ProcessingTime", processingTime);
                command.Parameters.AddWithValue("@FileName", fileName);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                try {
                    connection.Open();
                    command.ExecuteNonQuery();
                    retVal = true;
                }
                catch {
                    retVal = false;
                }
                finally {
                    connection.Close();
                }
            }
            return retVal;
        }

        public void UpdateFileSize(string fileName, long fileSize) {
            string cmdText = "UPDATE LoadingStatusDetail SET FileSize = @FileSize WHERE FileName = @FileName";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@FileName", fileName);
                command.Parameters.AddWithValue("@FileSize", fileSize);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        public DataTable GetLoadingStatusDetail(string instanceID) {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            string cmdText = "SELECT FileName, SystemSerial, TempUWSID, Type FROM LoadingStatusDetail " +
                             "WHERE Flag != 1 AND InstanceID = @InstanceID ORDER BY LoadingQueID LIMIT 1";

            var connection = new MySqlConnection(_connectionString);
            var command = new MySqlCommand(cmdText, connection);
            command.CommandTimeout = 0;
            command.Parameters.AddWithValue("@InstanceID", instanceID);

            var adapter = new MySqlDataAdapter(command);
            var myDataTable = new DataTable();

            try {
                connection.Open();
                adapter.Fill(myDataTable);
            }
            finally {
                connection.Close();
            }

            return myDataTable;
        }

        public DataTable GetStoppedJobs(string instanceId) {
            const string cmdText = "SELECT SystemSerial, FileName FROM LoadingStatusDetail " +
                                   "WHERE InstanceID = @InstanceID";
            DataTable stoppedJobs = new DataTable();
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@InstanceID", instanceId);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(stoppedJobs);
            }

            return stoppedJobs;
        }

        public int GetCurrentLoadCount(string systemSerial, string instanceId) {
            const string cmdText = @"SELECT COUNT(LoadingQueID) AS LoadCount FROM LoadingStatusDetail
                                    WHERE SystemSerial = @SystemSerial";
                                    // AND InstanceID = @InstanceID";
            var loadCount = 0;
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                // Per Khody: Limit load per RDS to no more than 2 simultaneous per Server. 
                //command.Parameters.AddWithValue("@InstanceID", instanceId);
                connection.Open();
                var reader = command.ExecuteReader();

                while (reader.Read()) {
                    loadCount = Convert.ToInt32(reader["LoadCount"].ToString());
                }
                reader.Close();
            }
            return loadCount;
        }

    }
}