using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.UWSLoader.Core.Repository {
    class LoadingInfo {
        private readonly string ConnectionString = "";

        public LoadingInfo(string connectionString) {
            ConnectionString = connectionString;
        }

        public int GetMaxUWSID() {
            int tempUWSID = 0;
            string cmdText = "SELECT Max(UWSID) AS MaxUWSID FROM LoadingInfo";
            using (var connection = new MySqlConnection(ConnectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                connection.Open();
                var reader = command.ExecuteReader();

                if (reader.Read()) {
                    tempUWSID = Convert.ToInt32(reader["MaxUWSID"].ToString()) + 1;
                }
            }
            return tempUWSID;
        }

        public DataTable GetSystemInfo(string uwsID) {
            string cmdText = @"SELECT systemserial, filename FROM LoadingInfo WHERE UWSID = @UWSID";
            var systemInfo = new DataTable();
            using (var connection = new MySqlConnection(ConnectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(systemInfo);
            }
            return systemInfo;
        }

        public DataTable GetLoadingPeriod(string uwsID) {
            string cmdText = @"SELECT starttime, stoptime, SampleType FROM LoadingInfo WHERE UWSID = @UWSID";
            var LoadingPeriod = new DataTable();
            using (var connection = new MySqlConnection(ConnectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("UWSID", uwsID);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(LoadingPeriod);
            }
            return LoadingPeriod;
        }

        public IDictionary<string, int> GetUWSRetentionDay() {
            IDictionary<string, int> retentionDays = new Dictionary<string, int>();
            string cmdText = "SELECT DISTINCT l.SystemSerial, s.UWSRetentionDay FROM LoadingInfo AS l " +
                             "LEFT JOIN System_tbl AS s ON l.SystemSerial = s.SystemSerial";

            using (var connection = new MySqlConnection(ConnectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                connection.Open();
                var reader = command.ExecuteReader();

                string systemSerial = "";
                int uwsRetentionDay = 0;

                while (reader.Read()) {
                    if (!reader.IsDBNull(0))
                        systemSerial = reader["SystemSerial"].ToString().Trim();
                    else
                        continue;
                    if (!reader.IsDBNull(1))
                        uwsRetentionDay = Convert.ToInt32(reader["UWSRetentionDay"]);

                    if (!retentionDays.ContainsKey(systemSerial)) {
                        retentionDays.Add(systemSerial, uwsRetentionDay);
                    }
                }
            }

            return retentionDays;
        }

        public IDictionary<string, int> GetExpertReportRetentionDay() {
            IDictionary<string, int> retentionDays = new Dictionary<string, int>();
            string cmdText = @"SELECT SystemSerial, ExpertReportRetentionDay
                              FROM System_Tbl WHERE ExpertReport = '1'";

            using (var connection = new MySqlConnection(ConnectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                connection.Open();
                var reader = command.ExecuteReader();

                string systemSerial = "";
                int uwsRetentionDay = 0;

                while (reader.Read()) {
                    if (!reader.IsDBNull(0))
                        systemSerial = reader["SystemSerial"].ToString().Trim();
                    else
                        continue;
                    if (!reader.IsDBNull(1))
                        uwsRetentionDay = Convert.ToInt32(reader["ExpertReportRetentionDay"]);

                    if (!retentionDays.ContainsKey(systemSerial)) {
                        retentionDays.Add(systemSerial, uwsRetentionDay);
                    }
                }
            }

            return retentionDays;
        }

        public DataTable GetExpertReportArchiveRetentionDay() {
            var archiveRetentionDay = new DataTable();
            const string cmdText = @"SELECT SystemSerial, ExpertReportRetentionDay, ArchiveRetention
                              FROM System_Tbl WHERE ExpertReport = '1'";
            using (var connection = new MySqlConnection(ConnectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(archiveRetentionDay);
            }

            return archiveRetentionDay;
        }

        public List<string> GetUWSFileName(string systemSerial, DateTime uploadedtime) {
            var fileNames = new List<string>();

            string cmdText = "SELECT [FileName] FROM LoadingInfo WHERE SystemSerial = @SystemSerial AND " +
                             "UploadedTime < @EndDate";

            using (var connection = new MySqlConnection(ConnectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@EndDate", uploadedtime);
                connection.Open();
                var reader = command.ExecuteReader();

                if (reader.Read()) {
                    string fileName = reader["FileName"].ToString();
                    if (!fileNames.Contains(fileName))
                        fileNames.Add(fileName);
                }
            }

            return fileNames;
        }

        public void Insert(int tempUWSID, int customerID) {
            string cmdText =
                "INSERT INTO LoadingInfo (UWSID, CustomerID, Status, uploadedtime) VALUES (@UWSID, @CustomerID, 'Uped', @UploadTime)";
            using (var connection = new MySqlConnection(ConnectionString)) {
                try {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@CustomerID", customerID);
                    command.Parameters.AddWithValue("@UWSID", tempUWSID);
                    command.Parameters.AddWithValue("@UploadTime", DateTime.Now);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (Exception ex) {
                }
            }
        }

        public void Update(string filepath, string systemSerial, string fileSize, string fileType, string uwsID) {
            string cmdText = @"UPDATE LoadingInfo SET filename = @FileName" +
                             ", systemserial = @SystemSerial" +
                             ", filesize= @FileSize" +
                             ", filestat = 'ACT' " +
                             ", StartLoadTime = @DateTime" +
                             ", SampleType = @FileType WHERE UWSID = @UWSID";

            using (var connection = new MySqlConnection(ConnectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@FileName", filepath);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@FileSize", fileSize);
                command.Parameters.AddWithValue("@DateTime", DateTime.Now);
                command.Parameters.AddWithValue("@FileType", fileType);
                command.Parameters.AddWithValue("@UWSID", uwsID);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void Update(string uwsID) {
            string cmdText = @"UPDATE LoadingInfo SET StartLoadTime = @StartLoadTime WHERE UWSID = @UWSID";
            using (var connection = new MySqlConnection(ConnectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@UWSID", uwsID);

                command.Parameters.AddWithValue("@StartLoadTime", DateTime.Now);
                command.Parameters.AddWithValue("@UWSID", uwsID);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void Update(int uwsID, long uwsFileSize, string systemName, DateTime startTime, DateTime stopTime,
            int type) {
            string cmdText = "UPDATE LoadingInfo SET Status = @Status, " +
                             "UWSfileSize = @UWSfileSize, LoadedTime = @LoadedTime, " +
                             "SystemName = @SystemName, StartTime = @StartTime, " +
                             "StopTime = @StopTime, SampleType = @SampleType WHERE UWSID = @UWSID";
            try {
                using (var connection = new MySqlConnection(ConnectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@Status", "Lded");
                    command.Parameters.AddWithValue("@UWSfileSize", uwsFileSize);
                    command.Parameters.AddWithValue("@LoadedTime", DateTime.Now);
                    command.Parameters.AddWithValue("@SystemName", systemName);
                    command.Parameters.AddWithValue("@StartTime", startTime);
                    command.Parameters.AddWithValue("@StopTime", stopTime);
                    command.Parameters.AddWithValue("@SampleType", type);
                    command.Parameters.AddWithValue("@UWSID", uwsID);

                    connection.Open();
                    command.ExecuteReader();
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void UpdateFileStat(string systemSerial, DateTime endDate) {
            string cmdText = "UPDATE LoadingInfo SET filestat = 'DEL' WHERE SystemSerial = @SystemSerial AND " +
                             "uploadedtime < @EndDate AND SampleType != 3";

            using (var connection = new MySqlConnection(ConnectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@EndDate", endDate);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void UpdateLoadingStatus(int uwsID, string status) {
            string cmdText = "UPDATE LoadingInfo SET Status = @status WHERE " +
                             "UWSID = @UWSID";

            try {
                using (var connection = new MySqlConnection(ConnectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("status", status);
                    command.Parameters.Add("@UWSID", MySqlDbType.Int32).Value = uwsID;
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void UpdateStopTime(string stopTime, int uwsID) {
            string cmdText = "UPDATE LoadingInfo SET stoptime = @StopTime WHERE UWSID = @UWSID";
            try {
                using (var connection = new MySqlConnection(ConnectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@StopTime", stopTime);
                    command.Parameters.AddWithValue("@UWSID", uwsID);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public DataTable GetLoadingInfo(int uwsID) {
            string cmdText = "SELECT * FROM LoadingInfo WHERE UWSID = @UWSID";
            var loadingInfoTable = new DataTable();
            try {
                using (var connection = new MySqlConnection(ConnectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@UWSID", uwsID);
                    var adapter = new MySqlDataAdapter(command);
                    adapter.Fill(loadingInfoTable);
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return loadingInfoTable;
        }

        public string GetUMPFullFileName(string fileName, string systemSerial, bool jumpToNextDay) {
            string fileNameRA = "RA" + fileName;
            string fileNameZA = "ZA" + fileName;
            string fileNameDO = "DO" + fileName;
            DateTime todayStartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 00, 00, 00);
            //If the time jumps to next day, we need to subtract the startTime by 1 day
            //Suppose we are looking for a file RA22, the file is in S3, but not in table, the thread is going to sleep until the expected time
            //Once the thread wakes up, and the day goes to next day, it won't find the file in LoadingInfo table even it is loaded
            if (jumpToNextDay) {
                todayStartTime = todayStartTime.AddDays(-1);
            }
            DateTime todayEndTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
            //            DateTime et = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, int.Parse(expectedTime.Substring(0, 2)), 0, 0);
            //            const string cmdText = "SELECT COUNT(*) AS NumOfRows FROM LoadingInfo WHERE SystemSerial = @SystemSerial AND " +
            //                                   "(FileName like '@FileNameRA%' OR FileName like '@FileNameZA%')";
            string cmdText = "SELECT FileName FROM LoadingInfo WHERE SystemSerial = @SystemSerial AND " +
                                   "(FileName like '" + fileNameRA + "%' OR FileName like '" + fileNameZA + "%' OR FileName like '" + fileNameDO + "%')" +
                             "AND UploadedTime >= '" + todayStartTime + "' AND UploadedTime <= '" + todayEndTime + "' order by LoadedTime DESC LIMIT 1";
            //            var numOfRows = 0;
            string fullFileName = "";
            using (var connection = new MySqlConnection(ConnectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                //command.Parameters.AddWithValue("@FileNameRA", fileNameRA+"%");
                //command.Parameters.AddWithValue("@FileNameZA", fileNameZA+"%");
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                //                command.Parameters.AddWithValue("@ExpectedTime", et);
                connection.Open();
                var reader = command.ExecuteReader();

                while (reader.Read()) {
                    fullFileName = reader["FileName"].ToString();
                    break;
                }
                reader.Close();
            }
            return fullFileName;
        }

        public string GetLoadCompleteTime(string fileName, string systemSerial) {
            string fileNameRA = "RA" + fileName;
            string fileNameZA = "ZA" + fileName;
            string fileNameDO = "DO" + fileName;
            string cmdText = "SELECT LoadedTime FROM LoadingInfo WHERE SystemSerial = @SystemSerial AND " +
                                   "(FileName like '" + fileNameRA + "%' OR FileName like '" + fileNameZA + "%' OR FileName like '" + fileNameDO + "%') " +
                             "order by LoadedTime DESC LIMIT 1";
            string loadedTime = "";

            using (var connection = new MySqlConnection(ConnectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                //                command.Parameters.AddWithValue("@FileNameRA", fileNameRA);
                //                command.Parameters.AddWithValue("@FileNameZA", fileNameZA);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                connection.Open();
                var reader = command.ExecuteReader();

                while (reader.Read()) {
                    loadedTime = reader["LoadedTime"].ToString();
                    break;
                }
                reader.Close();
            }
            return loadedTime;
        }
    }
}
