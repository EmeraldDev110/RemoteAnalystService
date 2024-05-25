using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM {
    public class DetailDiskForForecast {
        private readonly string _connectionString;

        public DetailDiskForForecast(string connectionString) {
            _connectionString = connectionString;
        }

        public DataTable GetQueueLength(DateTime startTime, DateTime stopTime) {

            string cmdText = @"SELECT DeviceName, QueueLength, DP2Busy, DATE_FORMAT(FromTimestamp, '%H:%i') AS `Hour` 
                             FROM DetailDiskForForecast WHERE
                             FromTimestamp >= @StartTime AND FromTimestamp < @StopTime
                             ORDER BY DeviceName, `Hour`";
            var detailDiskForForecast = new DataTable();

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText + "; set net_write_timeout=99999; set net_read_timeout=99999", connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@StartTime", startTime);
                command.Parameters.AddWithValue("@StopTime", stopTime);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(detailDiskForForecast);
            }

            return detailDiskForForecast;
        }
        
    }
}
