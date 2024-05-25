using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MySqlConnector;
using System.Threading.Tasks;
using System.Configuration;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class ScheduleStorageDetail {
        private string _connectionString;
        public ScheduleStorageDetail(string connectionString) {
            _connectionString = connectionString;
        }

        public DataTable getStorageThreshold(int scheduleId)
        {
            var cmdText = @"SELECT Volume, Threshold FROM ScheduleStorageThreshold 
                                WHERE ScheduleId = @ScheduleId";

            var thresholds = new DataTable();
            //Get last date from DailyAppUnrated.
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@ScheduleId", scheduleId);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(thresholds);
            }

            return thresholds;
        }

        public string GetIgnoreVolumes(int scheduleId) {
            string ignoreVolumes = "";
            try {
                const string cmdText = "SELECT VolumeIgnored FROM `ScheduleStorageDetail` WHERE ScheduleId = @ScheduleId;";
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@ScheduleId", scheduleId);

                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read()) {
                        ignoreVolumes = reader["VolumeIgnored"].ToString();
                    }
                    reader.Close();
                }
                return ignoreVolumes;
            }catch(Exception ex) {
                return ignoreVolumes;
            }
        }
    }
}
