using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using RemoteAnalyst.Repository.Infrastructure;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class UWSDirectories {
        private readonly string _connectionString;

        public UWSDirectories(string connectionString) {
            _connectionString = connectionString;
        }
        
        public List<UWSDirectoryInfo> CheckData(string systemSerial, DateTime startTime, DateTime stopTime) {
            var uwsFiles = new List<UWSDirectoryInfo>();
            string cmdText = @"SELECT UWSLocation, StartTime, StopTime FROM UWSDirectories WHERE
                               SystemSerial = @SystemSerial AND 
                               ((@StartTime >= StartTime AND @StartTime < StopTime
                               OR
                               @StopTime <= StopTime AND @StopTime > StartTime)
                               OR
                               (StartTime >= @StartTime AND StopTime < @StopTime))";

            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                    command.Parameters.AddWithValue("@StartTime", startTime);
                    command.Parameters.AddWithValue("@StopTime", stopTime);
                    connection.Open();

                    var reader = command.ExecuteReader();
                    while (reader.Read()) {
                        var uwsDirectoryInfo = new UWSDirectoryInfo {
                            UWSLocation = reader["UWSLocation"].ToString(),
                            StartTime = Convert.ToDateTime(reader["StartTime"]),
                            StopTime = Convert.ToDateTime(reader["StopTime"])
                        };

                        if (!uwsFiles.Contains(uwsDirectoryInfo)) {
                            uwsFiles.Add(uwsDirectoryInfo);
                        }
                    }
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return uwsFiles;
        }

        public void UpdateLoading(string systemSerial, DateTime startTime, DateTime stopTime, int isLoading) {
            string cmdText = @"UPDATE UWSDirectories SET Loading = @Loading
                               WHERE SystemSerial = @SystemSerial AND 
                               ((@StartTime >= StartTime AND @StartTime < StopTime
                               OR
                               @StopTime <= StopTime AND @StopTime > StartTime)
                               OR
                               (StartTime >= @StartTime AND StopTime < @StopTime))";

            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                    command.Parameters.AddWithValue("@StartTime", startTime);
                    command.Parameters.AddWithValue("@StopTime", stopTime);
                    command.Parameters.AddWithValue("@Loading", isLoading);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void UpdateLoading(string systemSerial, string uwsLocation, int isLoading) {
            string cmdText = @"UPDATE UWSDirectories SET Loading = @Loading
                               WHERE SystemSerial = @SystemSerial AND 
                               UWSLocation = @UWSLocation";

            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                    command.Parameters.AddWithValue("@UWSLocation", uwsLocation);
                    command.Parameters.AddWithValue("@Loading", isLoading);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }
        public void InsertUWSDirectory(string systemSerial, DateTime startTime, DateTime stopTime, string location) {
            string cmdText =
                "INSERT INTO UWSDirectories (SystemSerial, StartTime, StopTime, UWSLocation, LoadedDate, Loading) " +
                "VALUES (@SystemSerial, @StartTime, @StopTime, @UWSLocation, @LoadedDate, @Loading)";

            //Get last date from DailySysUnrated.
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@StartTime", startTime);
                command.Parameters.AddWithValue("@StopTime", stopTime);
                command.Parameters.AddWithValue("@UWSLocation", location);
                command.Parameters.AddWithValue("@LoadedDate", DateTime.Now);
                command.Parameters.AddWithValue("@Loading", 0);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        public bool CheckDuplicateTime(string systemSerial, DateTime startTime, DateTime stopTime, string location) {
            bool exists = false;
            string cmdText = @"SELECT SystemSerial FROM UWSDirectories WHERE 
                                SystemSerial = @SystemSerial AND 
                                StartTime = @StartTime AND 
                                StopTime = @StopTime AND
                                UWSLocation = @UWSLocation";

            //Get last date from DailySysUnrated.
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@StartTime", startTime);
                command.Parameters.AddWithValue("@StopTime", stopTime);
                command.Parameters.AddWithValue("@UWSLocation", location);
                connection.Open();

                var reader = command.ExecuteReader();
                if (reader.Read()) {
                    exists = true;
                }
            }
            return exists;
        }
        
    }
}