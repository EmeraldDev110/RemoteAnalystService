using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySqlConnector;
using RemoteAnalyst.Repository.Infrastructure;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public  class MonitorRDS {
        private readonly string _connectionString;

        public MonitorRDS(string connectionString) {
            _connectionString = connectionString;
        }

		public double GetRDSCpuBusy(string rdsRealName) {
			string cmdText = @"SELECT `CpuBusy` FROM MonitorRDS WHERE `RdsRealName` = @RdsRealName";
			double rdsCpuBusy = 0;
			using (var connection = new MySqlConnection(_connectionString)) {
				var command = new MySqlCommand(cmdText + Helper.CommandParameter, connection);
				command.CommandTimeout = 0;
				command.Parameters.AddWithValue("@RdsRealName", rdsRealName);
				connection.Open();
				var reader = command.ExecuteReader();

				if (reader.Read()) {
					rdsCpuBusy = Convert.ToDouble(reader["CpuBusy"].ToString());
				}
			}
			return rdsCpuBusy;

		}


		public void InsertEntry(string rdsName, string rdsRealName, double cpuBusy, double gbSize, double freeSpace, string todayLoadCount,
                                    string todayLoadSize, double cpuBusyAverage, double cpuBusyPeak, string displaySpace) {
            string cmdText = @"INSERT INTO MonitorRDS (`RdsName`, `RdsRealName`, `CpuBusy`, `GbSize`, `FreeSpace`, 
                             `TodayLoadCount`, `TodayLoadSize`, `CpuBusyAverage`, `CpuBusyPeak`, `DisplaySpace`) VALUES
                            (@RdsName, @RdsRealName, @CpuBusy, @GbSize, @FreeSpace, @TodayLoadCount, 
                            @TodayLoadSize, @CpuBusyAverage, @CpuBusyPeak, @DisplaySpace)";
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText + Helper.CommandParameter, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@RdsName", rdsName);
                command.Parameters.AddWithValue("@RdsRealName", rdsRealName);
                command.Parameters.AddWithValue("@CpuBusy", cpuBusy);
                command.Parameters.AddWithValue("@GbSize", gbSize);
                command.Parameters.AddWithValue("@FreeSpace", freeSpace);
                command.Parameters.AddWithValue("@TodayLoadCount", todayLoadCount);
                command.Parameters.AddWithValue("@TodayLoadSize", todayLoadSize);
                command.Parameters.AddWithValue("@CpuBusyAverage", cpuBusyAverage);
                command.Parameters.AddWithValue("@CpuBusyPeak", cpuBusyPeak);
                command.Parameters.AddWithValue("@DisplaySpace", displaySpace);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

		public void UpdateEntryNoCpuBusy(string rdsName, string rdsRealName, double gbSize, double freeSpace, string todayLoadCount,
			string todayLoadSize, double cpuBusyAverage, double cpuBusyPeak, string displaySpace) {
			string cmdText = @"UPDATE MonitorRDS SET `RdsRealName` = @RdsRealName, `GbSize` =@GbSize, 
                            `FreeSpace` = @FreeSpace, `TodayLoadCount` = @TodayLoadCount, `TodayLoadSize` = @TodayLoadSize, 
                            `CpuBusyAverage` = @CpuBusyAverage, `CpuBusyPeak` = @CpuBusyPeak, `DisplaySpace` = @DisplaySpace
                            WHERE `RdsName` = @RdsName";
			using (var connection = new MySqlConnection(_connectionString)) {
				var command = new MySqlCommand(cmdText + Helper.CommandParameter, connection);
				command.CommandTimeout = 0;
				command.Parameters.AddWithValue("@RdsName", rdsName);
				command.Parameters.AddWithValue("@RdsRealName", rdsRealName);
				command.Parameters.AddWithValue("@GbSize", gbSize);
				command.Parameters.AddWithValue("@FreeSpace", freeSpace);
				command.Parameters.AddWithValue("@TodayLoadCount", todayLoadCount);
				command.Parameters.AddWithValue("@TodayLoadSize", todayLoadSize);
				command.Parameters.AddWithValue("@CpuBusyAverage", cpuBusyAverage);
				command.Parameters.AddWithValue("@CpuBusyPeak", cpuBusyPeak);
				command.Parameters.AddWithValue("@DisplaySpace", displaySpace);
				connection.Open();
				command.ExecuteNonQuery();
			}
		}


		public void UpdateEntry(string rdsName, string rdsRealName, double cpuBusy, double gbSize, double freeSpace, string todayLoadCount,
            string todayLoadSize, double cpuBusyAverage, double cpuBusyPeak, string displaySpace) {
            string cmdText = @"UPDATE MonitorRDS SET `RdsRealName` = @RdsRealName, `CpuBusy` = @CpuBusy, `GbSize` =@GbSize, 
                            `FreeSpace` = @FreeSpace, `TodayLoadCount` = @TodayLoadCount, `TodayLoadSize` = @TodayLoadSize, 
                            `CpuBusyAverage` = @CpuBusyAverage, `CpuBusyPeak` = @CpuBusyPeak, `DisplaySpace` = @DisplaySpace
                            WHERE `RdsName` = @RdsName";
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText + Helper.CommandParameter, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@RdsName", rdsName);
                command.Parameters.AddWithValue("@RdsRealName", rdsRealName);
                command.Parameters.AddWithValue("@CpuBusy", cpuBusy);
                command.Parameters.AddWithValue("@GbSize", gbSize);
                command.Parameters.AddWithValue("@FreeSpace", freeSpace);
                command.Parameters.AddWithValue("@TodayLoadCount", todayLoadCount);
                command.Parameters.AddWithValue("@TodayLoadSize", todayLoadSize);
                command.Parameters.AddWithValue("@CpuBusyAverage", cpuBusyAverage);
                command.Parameters.AddWithValue("@CpuBusyPeak", cpuBusyPeak);
                command.Parameters.AddWithValue("@DisplaySpace", displaySpace);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public bool CheckDataEntry(string rdsName) {
            var cmdText = "SELECT RdsName FROM MonitorRDS WHERE RdsName = @RdsName";
            var exists = false;

            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText + Helper.CommandParameter, connection);
                    command.CommandTimeout = 0;
                    command.Parameters.AddWithValue("@RdsName", rdsName);

                    connection.Open();
                    var reader = command.ExecuteReader();

                    if (reader.Read()) {
                        exists = true;
                    }
                }
            }
            catch {
                exists = false;
            }

            return exists;
        }

    }
}
