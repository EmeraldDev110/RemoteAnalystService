using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM {
    public class DetailProcessForForecast {
        private readonly string _connectionString;

        public DetailProcessForForecast(string connectionString) {
            _connectionString = connectionString;
        }

        public DataTable GetProcessData(DateTime startTime, DateTime stopTime) {

            //string cmdText = @"SELECT FromTimestamp, HOUR(FromTimestamp) AS `Hour`,
            //                 ProcessName, CpuNumber, Pin, Volume, SubVol, FileName,
            //                 ProcessBusy, RecvQueueLength FROM DetailProcessForForecast WHERE
            //                 FromTimestamp >= @StartTime AND FromTimestamp < @StopTime LIMIT 10000";

            string cmdText = @"SELECT ProcessName, CpuNumber, Pin, Volume, SubVol, FileName,
                             FromTimestamp, DATE_FORMAT(FromTimestamp, '%H:%i') AS `Hour`, 
                             ProcessBusy, RecvQueueLength FROM DetailProcessForForecast WHERE
                             FromTimestamp >= @StartTime AND FromTimestamp < @StopTime
                             ORDER BY ProcessName, CpuNumber, Pin, Volume, SubVol, FileName, `Hour`";
            var detailDiskForForecast = new DataTable();

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
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
