using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM {
    public class DetailTmfForForecast {
        private readonly string _connectionString;

        public DetailTmfForForecast(string connectionString) {
            _connectionString = connectionString;
        }

        public DataTable GetTmfData(DateTime startTime, DateTime stopTime) {

            string cmdText = @"SELECT FromTimestamp, ProcessName, CpuNumber, Pin, Volume, SubVol, FileName,
                             AbortPercent FROM DetailTmfForForecast WHERE
                             FromTimestamp >= @StartTime AND FromTimestamp < @StopTime";

            var detailDiskForForecast = new DataTable();

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@StartTime", startTime);
                command.Parameters.AddWithValue("@StopTime", stopTime);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(detailDiskForForecast);
            }

            return detailDiskForForecast;
        }
    }
}
