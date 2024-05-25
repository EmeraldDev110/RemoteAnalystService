using System;
using System.Data;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb
{
    public class UWSLoadInfos
    {
        private readonly string _connectionString;

        public UWSLoadInfos(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void InsertData(string systemSerial, DateTime startTime, DateTime stopTime, DateTime loadedStartTime,
            DateTime loadedStopTime)
        {
            string cmdText =
                @"INSERT INTO UWSLoadInfos (SystemSerial, StartTime, StopTime, LoadedStartTime, LoadedStopTime) 
                               VALUES (@SystemSerial, @StartTime, @StopTime, @LoadedStartTime, @LoadedStopTime)";

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                    command.Parameters.AddWithValue("@StartTime", startTime);
                    command.Parameters.AddWithValue("@StopTime", stopTime);
                    command.Parameters.AddWithValue("@LoadedStartTime", loadedStartTime);
                    command.Parameters.AddWithValue("@LoadedStopTime", loadedStopTime);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public DataTable GetLoadedTime(string systemSerial, DateTime startTime, DateTime stopTime)
        {
            //List<LoadedTime> loadedTimes = new List<LoadedTime>();
            var loadedTimes = new DataTable();

            string cmdText =
                @"SELECT LoadedStartTime, LoadedStopTime FROM UWSLoadInfos WHERE SystemSerial = @SystemSerial
                               AND StartTime = @StartTime AND StopTime = @StopTime ORDER BY LoadedStartTime";

            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@StartTime", startTime);
                command.Parameters.AddWithValue("@StopTime", stopTime);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(loadedTimes);

                /*connection.Open();
                var reader = command.ExecuteReader();

                while (reader.Read()) {
                    LoadedTime loadTime = new LoadedTime();
                    loadTime.LoadedStartTime = Convert.ToDateTime(reader["LoadedStartTime"]);
                    loadTime.LoadedStopTime = Convert.ToDateTime(reader["LoadedStopTime"]);
                    loadedTimes.Add(loadTime);
                }*/
            }

            //Merge continues data.
            //LoadedTime loadedTime = new LoadedTime();
            //List<LoadedTime> mergedTimes = loadedTime.MergeContinuesTime(loadedTimes);

            return loadedTimes;
        }

        public bool CheckLoadedTime(string systemSerial, DateTime startTime, DateTime stopTime) {
            var exits = false;

            string cmdText =
                @"SELECT LoadedStartTime FROM UWSLoadInfos WHERE SystemSerial = @SystemSerial
                   AND StartTime = @StartTime AND StopTime = @StopTime";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@StartTime", startTime);
                command.Parameters.AddWithValue("@StopTime", stopTime);
                connection.Open();
                var reader = command.ExecuteReader();

                if (reader.Read())
                    exits = true;
            }

            return exits;
        }

    }
}