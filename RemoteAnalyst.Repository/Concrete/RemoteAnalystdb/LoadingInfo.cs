using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using RemoteAnalyst.Repository.Infrastructure;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb
{
    public class LoadingInfoParameter {
        public string SystemSerial { get; set; }
        public DateTime StartTime { get; set; }
        public int SampleType { get; set; }
    }

    public class LoadingInfo
    {
        private readonly string _connectionString = "";

        public LoadingInfo(string connectionString)
        {
            _connectionString = connectionString;
        }

		public int GetMaxUWSID()
        {
            int tempUWSID = 0;
            string cmdText = "SELECT Max(UWSID) AS MaxUWSID FROM LoadingInfo";
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                connection.Open();
                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    tempUWSID = Convert.ToInt32(reader["MaxUWSID"].ToString()) + 1;
                }
            }
            return tempUWSID;
        }

        public DataTable GetSystemInfo(string uwsID)
        {
            string cmdText = @"SELECT systemserial, filename FROM LoadingInfo WHERE UWSID = @UWSID";
            var systemInfo = new DataTable();
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("UWSID", uwsID);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(systemInfo);
            }
            return systemInfo;
        }

        public DataTable GetLoadingPeriod(string uwsID)
        {
            string cmdText = @"SELECT starttime, stoptime, SampleType FROM LoadingInfo WHERE UWSID = @UWSID";
            var LoadingPeriod = new DataTable();
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("UWSID", uwsID);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(LoadingPeriod);
            }
            return LoadingPeriod;
        }

        public IDictionary<string, int> GetUWSRetentionDay()
        {
            IDictionary<string, int> retentionDays = new Dictionary<string, int>();
            string cmdText = "SELECT DISTINCT l.SystemSerial, s.UWSRetentionDay FROM LoadingInfo AS l " +
                             "LEFT JOIN System_Tbl AS s ON l.SystemSerial = s.SystemSerial";

            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                connection.Open();
                var reader = command.ExecuteReader();

                string systemSerial = "";
                int uwsRetentionDay = 0;

                while (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                        systemSerial = reader["SystemSerial"].ToString().Trim();
                    else
                        continue;
                    if (!reader.IsDBNull(1))
                        uwsRetentionDay = Convert.ToInt32(reader["UWSRetentionDay"]);

                    if (!retentionDays.ContainsKey(systemSerial))
                    {
                        retentionDays.Add(systemSerial, uwsRetentionDay);
                    }
                }
            }
            return retentionDays;
        }

        public IDictionary<string, int> GetExpertReportRetentionDay()
        {
            IDictionary<string, int> retentionDays = new Dictionary<string, int>();
            string cmdText = @"SELECT SystemSerial, ExpertReportRetentionDay
                              FROM System_Tbl WHERE ExpertReport = '1'";

            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                connection.Open();
                var reader = command.ExecuteReader();

                string systemSerial = "";
                int uwsRetentionDay = 0;

                while (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                        systemSerial = reader["SystemSerial"].ToString().Trim();
                    else
                        continue;
                    if (!reader.IsDBNull(1))
                        uwsRetentionDay = Convert.ToInt32(reader["ExpertReportRetentionDay"]);

                    if (!retentionDays.ContainsKey(systemSerial))
                    {
                        retentionDays.Add(systemSerial, uwsRetentionDay);
                    }
                }
            }

            return retentionDays;
        }

        public IDictionary<string, int> GetQNMRetentionDay() {
            IDictionary<string, int> retentionDays = new Dictionary<string, int>();
            string cmdText = @"SELECT SystemSerial, QNMRetentionDay
                              FROM System_Tbl WHERE ExpertReport = '1'";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
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
                        uwsRetentionDay = Convert.ToInt32(reader["QNMRetentionDay"]);

                    if (!retentionDays.ContainsKey(systemSerial)) {
                        retentionDays.Add(systemSerial, uwsRetentionDay);
                    }
                }
            }

            return retentionDays;
        }

        public IDictionary<string, int> GetPathwayRetentionDay() {
            IDictionary<string, int> retentionDays = new Dictionary<string, int>();
            string cmdText = @"SELECT SystemSerial, RetentionDay
                              FROM System_Tbl";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
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
                        uwsRetentionDay = Convert.ToInt32(reader["RetentionDay"]);

                    if (!retentionDays.ContainsKey(systemSerial)) {
                        retentionDays.Add(systemSerial, uwsRetentionDay);
                    }
                }
            }

            return retentionDays;
        }

        public List<string> GetUWSFileName(string systemSerial, DateTime uploadedtime)
        {
            var fileNames = new List<string>();

            string cmdText = "SELECT FileName FROM LoadingInfo WHERE SystemSerial = @SystemSerial AND " +
                             "UploadedTime < @EndDate";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
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

        public void Insert(int tempUWSID, int customerID)
        {
            string cmdText =
                "INSERT INTO LoadingInfo (UWSID, CustomerID, Status, uploadedtime) VALUES (@UWSID, @CustomerID, 'Uped', @UploadTime)";
            using (var connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    command.Parameters.AddWithValue("@CustomerID", customerID);
                    command.Parameters.AddWithValue("@UWSID", tempUWSID);
                    command.Parameters.AddWithValue("@UploadTime", DateTime.Now);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                }
            }
        }

        public void Update(string filepath, string systemSerial, string fileSize, string fileType, string uwsID)
        {
            string cmdText = @"UPDATE LoadingInfo SET filename = @FileName" +
                             ", systemserial = @SystemSerial" +
                             ", filesize= @FileSize" +
                             ", filestat = 'ACT' " +
                             ", StartLoadTime = @DateTime" +
                             ", SampleType = @FileType WHERE UWSID = @UWSID";

            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
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

        public void Update(string uwsID)
        {
            string cmdText = "UPDATE LoadingInfo SET StartLoadTime = @StartLoadTime WHERE UWSID = @UWSID";
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@UWSID", Convert.ToInt32(uwsID));
                command.Parameters.AddWithValue("@StartLoadTime", DateTime.Now);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void UpdateCollectionTime(int uwsID, string systemName, DateTime startTime, DateTime stopTime, int type) {
            string cmdText = "UPDATE LoadingInfo SET SystemName = @SystemName, StartTime = @StartTime, " +
                             "StopTime = @StopTime, SampleType = @SampleType WHERE UWSID = @UWSID";
            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    command.Parameters.AddWithValue("@SystemName", systemName);
                    command.Parameters.AddWithValue("@StartTime", startTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@StopTime", stopTime.ToString("yyyy-MM-dd HH:mm:ss"));
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

        public void UpdateCollectionTime(int uwsID, DateTime startTime, DateTime stopTime) {
            string cmdText = "UPDATE LoadingInfo SET StartTime = @StartTime, StopTime = @StopTime WHERE UWSID = @UWSID";
            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    command.Parameters.AddWithValue("@StartTime", startTime);
                    command.Parameters.AddWithValue("@StopTime", stopTime);
                    command.Parameters.AddWithValue("@UWSID", uwsID);

                    connection.Open();
                    command.ExecuteReader();
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void Update(int uwsID, string systemName, DateTime startTime, DateTime stopTime, int type) {
            string cmdText = "UPDATE LoadingInfo SET Status = @Status, " +
                             "LoadedTime = @LoadedTime, " +
                             "SystemName = @SystemName, StartTime = @StartTime, " +
                             "StopTime = @StopTime, SampleType = @SampleType WHERE UWSID = @UWSID";
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    command.Parameters.AddWithValue("@Status", "Lded");
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
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void UpdateLoadedTime(int uwsID) {
            string cmdText = "UPDATE LoadingInfo SET LoadedTime = @LoadedTime WHERE UWSID = @UWSID";
            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    command.Parameters.AddWithValue("@LoadedTime", DateTime.Now);
                    command.Parameters.AddWithValue("@UWSID", uwsID);

                    connection.Open();
                    command.ExecuteReader();
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void UpdateFileStat(string systemSerial, DateTime endDate)
        {
            string cmdText = "UPDATE LoadingInfo SET filestat = 'DEL' WHERE SystemSerial = @SystemSerial AND " +
                             "uploadedtime < @EndDate AND SampleType != 3";

            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@EndDate", endDate);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void UpdateLoadingStatus(int uwsID, string status)
        {
            string cmdText = "UPDATE LoadingInfo SET Status = @status WHERE " +
                             "UWSID = @UWSID";

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    command.Parameters.AddWithValue("status", status);
                    command.Parameters.Add("@UWSID", MySqlDbType.Int32).Value = uwsID;
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void UpdateStopTime(string stopTime, int uwsID)
        {
            string cmdText = "UPDATE LoadingInfo SET stoptime = @StopTime WHERE UWSID = @UWSID";
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    command.Parameters.AddWithValue("@StopTime", stopTime);
                    command.Parameters.AddWithValue("@UWSID", uwsID);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void UpdateUWSRelayTime(string systemSerial, int uwsId, DateTime ftpreceivedTime, DateTime s3SentTime, string fileName, long fileSize, int sampleType, string rdsName) {
            string cmdText = "UPDATE LoadingInfo SET SystemSerial = @SystemSerial, FTPReceivedTime = @FTPReceivedTime, S3SentTime = @S3SentTime, FileName = @FileName, SampleType = @SampleType, FileSize = @FileSize, RDSName = @RDSName WHERE UWSID = @UWSID ";
            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                    command.Parameters.AddWithValue("@FTPReceivedTime", ftpreceivedTime);
                    command.Parameters.AddWithValue("@S3SentTime", s3SentTime);
                    command.Parameters.AddWithValue("@UWSID", uwsId);
                    command.Parameters.AddWithValue("@FileName", fileName);
                    command.Parameters.AddWithValue("@FileSize", fileSize);
                    command.Parameters.AddWithValue("@SampleType", sampleType);
                    command.Parameters.AddWithValue("@RDSName", rdsName);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void UpdateUWSRelayTime(string systemSerial, int uwsId, DateTime ftpreceivedTime, DateTime s3SentTime, DateTime collectionStartTime, DateTime collectionStopTime, string fileName, long fileSize, string rdsName) {
            string cmdText = @"UPDATE LoadingInfo SET SystemSerial = @SystemSerial, FTPReceivedTime = @FTPReceivedTime, FileName = @FileName, 
                                        S3SentTime = @S3SentTime, StartTime = @StartTime, StopTime = @StopTime, SampleType = 4, FileSize = @FileSize, RDSName = @RDSName WHERE UWSID = @UWSID ";
            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                    command.Parameters.AddWithValue("@FTPReceivedTime", ftpreceivedTime);
                    command.Parameters.AddWithValue("@S3SentTime", s3SentTime);
                    command.Parameters.AddWithValue("@StartTime", collectionStartTime);
                    command.Parameters.AddWithValue("@StopTime", collectionStopTime);
                    command.Parameters.AddWithValue("@UWSID", uwsId);
                    command.Parameters.AddWithValue("@FileName", fileName);
                    command.Parameters.AddWithValue("@FileSize", fileSize);
                    command.Parameters.AddWithValue("@RDSName", rdsName);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public DataTable GetLoadingInfo(int uwsID)
        {
            string cmdText = "SELECT * FROM LoadingInfo WHERE UWSID = @UWSID";
            var loadingInfoTable = new DataTable();
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    command.Parameters.AddWithValue("@UWSID", uwsID);
                    var adapter = new MySqlDataAdapter(command);
                    adapter.Fill(loadingInfoTable);
                }
            }
            catch (Exception ex)
            {
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
                             "AND UploadedTime >= @StartTime AND UploadedTime <= @EndTime order by LoadedTime DESC LIMIT 1";
            //            var numOfRows = 0;
            string fullFileName = "";
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                //command.Parameters.AddWithValue("@FileNameRA", fileNameRA+"%");
                //command.Parameters.AddWithValue("@FileNameZA", fileNameZA+"%");
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@StartTime", todayStartTime);
                command.Parameters.AddWithValue("@EndTime", todayEndTime);
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

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
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

        public DataTable GetLoadingInfo(string systemSerial, DateTime startTime, DateTime stopTime) {
            string cmdText = @"SELECT SystemSerial, filename, uploadedtime, startloadtime, loadedtime, StartLoadTime,
                                filesize, starttime, stoptime, FTPReceivedTime, S3SentTime FROM LoadingInfo
                                WHERE SampleType = 4 AND systemserial = @SystemSerial AND
                                starttime >= @UploadedStartTime AND stoptime <= @UploadedStopTime";
            var loadingInfoTable = new DataTable();
            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                    command.Parameters.AddWithValue("@UploadedStartTime", startTime);
                    command.Parameters.AddWithValue("@UploadedStopTime", stopTime);
                    var adapter = new MySqlDataAdapter(command);
                    adapter.Fill(loadingInfoTable);
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return loadingInfoTable;
        }

        public DataTable GetLoadFailedInfo(DateTime currentHourTime)
        {
            string cmdText = @"SELECT SystemSerial, filename, uwsId FROM LoadingInfo
                                WHERE status='Sned' 
                                and loadedtime IS NULL and reloadedtime IS NULL
                                and RDSName IS NOT NULL and EC2InstanceID IS NOT NULL 
                                and uploadedtime >= @PreviousHourTime
                                and uploadedtime < @CurrentHourTime";
            var loadingInfoTable = new DataTable();
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    command.Parameters.AddWithValue("@PreviousHourTime", currentHourTime.AddHours(-1));
                    command.Parameters.AddWithValue("@CurrentHourTime", currentHourTime);
                    var adapter = new MySqlDataAdapter(command);
                    adapter.Fill(loadingInfoTable);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return loadingInfoTable;
        }

        public DataTable GetLoadedInfo (string systemSerial, DateTime startTime, DateTime stopTime) {
            string cmdText = @"SELECT SystemSerial, filename, uploadedtime, startloadtime, loadedtime, StartLoadTime,
                                filesize, starttime, stoptime, FTPReceivedTime, S3SentTime FROM LoadingInfo
                                WHERE SampleType = 4 AND systemserial = @SystemSerial AND
                                loadedtime >= @UploadedStartTime AND loadedtime <= @UploadedStopTime";
            var loadingInfoTable = new DataTable();
            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                    command.Parameters.AddWithValue("@UploadedStartTime", startTime);
                    command.Parameters.AddWithValue("@UploadedStopTime", stopTime);
                    var adapter = new MySqlDataAdapter(command);
                    adapter.Fill(loadingInfoTable);
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return loadingInfoTable;
        }

        public DataTable GetInProgressInfo (string systemSerial, DateTime startTime, DateTime stopTime) {
            string cmdText = @"SELECT SystemSerial, filename, uploadedtime, startloadtime, loadedtime, StartLoadTime,
                                filesize, starttime, stoptime, FTPReceivedTime, S3SentTime FROM LoadingInfo
                                WHERE SampleType = 4 AND systemserial = @SystemSerial AND (loadedtime IS NULL OR
                                loadedTime > @UploadedStopTime) AND
                                FTPReceivedTime >= @UploadedStartTime AND FTPReceivedTime <= @UploadedStopTime";
            var loadingInfoTable = new DataTable();
            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                    command.Parameters.AddWithValue("@UploadedStartTime", startTime);
                    command.Parameters.AddWithValue("@UploadedStopTime", stopTime);
                    var adapter = new MySqlDataAdapter(command);
                    adapter.Fill(loadingInfoTable);
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return loadingInfoTable;
        }

        public void UpdateStatus(List<LoadingInfoParameter> parameters) {
            string cmdText = @"UPDATE LoadingInfo SET Status = 'del' WHERE SystemSerial = @SystemSerial AND SampleType = @SampleType AND StartTime = @StartTime";
            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText + Infrastructure.Helper.CommandParameter, connection);
                    connection.Open();
                    foreach (LoadingInfoParameter lipParameter in parameters) {
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@SystemSerial", lipParameter.SystemSerial);
                        command.Parameters.AddWithValue("@SampleType", lipParameter.SampleType);
                        command.Parameters.AddWithValue("@StartTime", lipParameter.StartTime);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void UpdateReloadTime(string systemSerial, string uwsId, String filename, DateTime reloadedTime)
        {
            string cmdText = @"UPDATE LoadingInfo SET reloadedtime = @ReloadedTime WHERE SystemSerial = @SystemSerial AND UWSID = @uwsID AND Filename = @Filename";
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection) { CommandTimeout = 0 };
                command.Parameters.AddWithValue("@ReloadedTime", reloadedTime);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@uwsID", uwsId);
                command.Parameters.AddWithValue("@Filename", filename);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void BulkUpdateStatus(string systemSerial, int sampleType, DateTime startTime) {
            string cmdText = @"UPDATE LoadingInfo SET Status = 'del' WHERE SystemSerial = @SystemSerial AND SampleType = @SampleType AND StartTime < @StartTime";
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection) { CommandTimeout = 0 };
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@SampleType", sampleType);
                command.Parameters.AddWithValue("@StartTime", startTime);
                connection.Open();
                command.ExecuteNonQuery();
            }        
        }

        public void UpdateInstanceID(int uwsId, string instanceId) {
            string cmdText = @"UPDATE LoadingInfo SET EC2InstanceID = @EC2InstanceID WHERE UWSID = @UWSID";
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection) { CommandTimeout = 0 };
                command.Parameters.AddWithValue("@UWSID", uwsId);
                command.Parameters.AddWithValue("@EC2InstanceID", instanceId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public List<long> GetLoadInfoForToday(string instanceId, DateTime currentDate) {
            string cmdText = @"SELECT filesize FROM LoadingInfo 
                                WHERE `loadedtime` >= @StartLoadedData
                                AND `loadedtime` < @EndLoadedData
                                AND EC2InstanceId = @EC2InstanceId";

            var fileSize = new List<long>();

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@StartLoadedData", currentDate.ToString("yyyy-MM-dd 00:00:00"));
                command.Parameters.AddWithValue("@EndLoadedData", currentDate.AddDays(1).ToString("yyyy-MM-dd 00:00:00"));
                command.Parameters.AddWithValue("@EC2InstanceId", instanceId);
                connection.Open();
                var reader = command.ExecuteReader();

                while (reader.Read()) {
                    if (!reader.IsDBNull(0))
                        fileSize.Add(Convert.ToInt64(reader["filesize"]));
                }
            }
            return fileSize;
        }

        public List<long> GetRdsLoadInfoForToday(string rdsName, DateTime currentDate) {
            string cmdText = @"SELECT filesize FROM LoadingInfo 
                                WHERE `loadedtime` >= @StartLoadedData
                                AND `loadedtime` < @EndLoadedData 
                                AND RDSName = @RDSName AND EC2InstanceID IS NOT NULL";

            var fileSize = new List<long>();

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                //command.Parameters.AddWithValue("@LoadedDate", currentDate.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@StartLoadedData", currentDate.ToString("yyyy-MM-dd 00:00:00"));
                command.Parameters.AddWithValue("@EndLoadedData", currentDate.AddDays(1).ToString("yyyy-MM-dd 00:00:00"));
                command.Parameters.AddWithValue("@RDSName", rdsName);
                connection.Open();
                var reader = command.ExecuteReader();

                while (reader.Read()) {
                    if (!reader.IsDBNull(0))
                        fileSize.Add(Convert.ToInt64(reader["filesize"]));
                }
            }
            return fileSize;
        }
        public List<long> GetRdsOtherLoadInfoForToday(string rdsName, DateTime currentDate) {
            string cmdText = @"SELECT filesize FROM LoadingInfo 
                                WHERE `loadedtime` >= @StartLoadedData
                                AND `loadedtime` < @EndLoadedData  
                                AND RDSName = @RDSName AND EC2InstanceID IS NULL";

            var fileSize = new List<long>();

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                //command.Parameters.AddWithValue("@LoadedDate", currentDate.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@StartLoadedData", currentDate.ToString("yyyy-MM-dd 00:00:00"));
                command.Parameters.AddWithValue("@EndLoadedData", currentDate.AddDays(1).ToString("yyyy-MM-dd 00:00:00"));
                command.Parameters.AddWithValue("@RDSName", rdsName);
                connection.Open();
                var reader = command.ExecuteReader();

                while (reader.Read()) {
                    if (!reader.IsDBNull(0))
                        fileSize.Add(Convert.ToInt64(reader["filesize"]));
                }
            }
            return fileSize;
        }

        public void DeleteLoadingInfoOlderThanXDaysAgo(int loadingInfoRetentionDays) {
            string cmdText = "DELETE FROM LoadingInfo WHERE uploadedtime < @OldDate";
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@OldDate", DateTime.Now.AddDays(-1 * loadingInfoRetentionDays).ToString("yyyy-MM-dd 00:00:00"));

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

		public void DeleteLoadingInfoByUWSID(int uwsID) {
			string cmdText = "DELETE FROM LoadingInfo WHERE UWSID = @UWSID";
			using (var connection = new MySqlConnection(_connectionString)) {
				var command = new MySqlCommand(cmdText, connection);
				command.Parameters.AddWithValue("@UWSID", uwsID);

				connection.Open();
				command.ExecuteNonQuery();
			}
		}

        public void DeleteLoadingInfoByFileName(string fileName) {
            string cmdText = "DELETE FROM LoadingInfo WHERE FileName = @FILENAME";
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@FILENAME", fileName);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public string GetFirstUWSLoadRecordByUWSName(string uwsFileName) {
            string cmdText = "SELECT status FROM LoadingInfo WHERE `filename` = @FileName ORDER BY UWSID LIMIT 1;";
            string loadStatus = "";
            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    command.Parameters.AddWithValue("@FileName", uwsFileName);

                    connection.Open();
                    var reader = command.ExecuteReader();

                    while (reader.Read()) {
                        loadStatus = reader["status"].ToString();
                        break;
                    }
                    reader.Close();
                }
                return loadStatus;
            }
            catch(Exception ex) {
                throw ex;
            }
        }

        public DataTable GetLoadHistoryForTransmonList(DateTime beforeTime)
        {
            // Since sometimes 'bad' loads skew the loadtime values, 
            // using only those that loaded within an hour
            string cmdText = @"SELECT L.SystemSerial as systemserial, 
                                        AVG(filesize/(1000*1000)) as averagefilesize, 
                                        AVG(loadedtime-StartLoadTime)/60 as averageloadtime, 
                                        COUNT(*) AS totalfiles
                                FROM LoadingInfo as L
                                INNER JOIN Transmon as T
                                ON L.SystemSerial = T.SystemSerial
                                WHERE SampleType = 4 AND
                                        uploadedtime > @beforeTime AND 
                                        (loadedtime IS NOT NULL or reloadedtime IS NOT NULL)  AND
                                        (loadedtime-StartLoadTime)/60 < 60
                                GROUP BY L.SystemSerial";
            var loadingInfoTable = new DataTable();
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    command.Parameters.AddWithValue("@beforeTime", beforeTime);
                    var adapter = new MySqlDataAdapter(command);
                    adapter.Fill(loadingInfoTable);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return loadingInfoTable;
        }

        public DataTable GetLoadHistory()
        {
            string cmdText = @"SELECT 
                                systemserial,
                                averagefilesize,
                                totalfiles,
                                averageloadtime,
                                updatedat,
                                allowancefactor,
                                ignorehistory
                                FROM LoadHistory";
            var loadingInfoTable = new DataTable();
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    var adapter = new MySqlDataAdapter(command);
                    adapter.Fill(loadingInfoTable);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return loadingInfoTable;
        }

        public DataTable GetInProcessingLoad()
        {
            string cmdText = @"SELECT *
                                FROM LoadingStatusDetail";
            var loadingInfoTable = new DataTable();
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    var adapter = new MySqlDataAdapter(command);
                    adapter.Fill(loadingInfoTable);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return loadingInfoTable;
        }

        public void BulkInsertMySQL(DataTable table, string tableName)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                using (MySqlTransaction tran = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        cmd.Connection = connection;
                        cmd.Transaction = tran;
                        cmd.CommandText = $"SELECT * FROM " + tableName + " limit 0";

                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            adapter.UpdateBatchSize = Constants.getInstance(_connectionString).BulkLoaderSize;
                            using (MySqlCommandBuilder cb = new MySqlCommandBuilder(adapter))
                            {
                                cb.SetAllValues = true;
                                adapter.Update(table);
                                tran.Commit();
                            }
                        };
                    }
                }
            }
        }
    }
}