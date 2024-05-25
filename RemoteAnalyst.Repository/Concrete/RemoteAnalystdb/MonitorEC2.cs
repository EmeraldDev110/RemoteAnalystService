using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MySqlConnector;
using RemoteAnalyst.Repository.Infrastructure;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class MonitorEC2 {
        private readonly string _connectionString;

        public MonitorEC2(string connectionString) {
            _connectionString = connectionString;
        }
        public void InsertEntry(string instanceId, string ec2Name, string instanceName, double cpuBusy, int todayLoadCount,
                                    double todayLoadSize, double cpuBusyAverage, double cpuBusyPeak) {
            string cmdText = @"INSERT INTO MonitorEC2 (`InstanceId`, `EC2Name`, `InstanceName`, `CpuBusy`, 
                            `TodayLoadCount`, `TodayLoadSize`, `CpuBusyAverage`, `CpuBusyPeak`) VALUES
                            (@InstanceId, @EC2Name, @InstanceName, @CpuBusy, @TodayLoadCount, 
                            @TodayLoadSize, @CpuBusyAverage, @CpuBusyPeak)";
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText + Helper.CommandParameter, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@InstanceId", instanceId);
                command.Parameters.AddWithValue("@EC2Name", ec2Name);
                command.Parameters.AddWithValue("@InstanceName", instanceName);
                command.Parameters.AddWithValue("@CpuBusy", cpuBusy);
                command.Parameters.AddWithValue("@TodayLoadCount", todayLoadCount);
                command.Parameters.AddWithValue("@TodayLoadSize", todayLoadSize);
                command.Parameters.AddWithValue("@CpuBusyAverage", cpuBusyAverage);
                command.Parameters.AddWithValue("@CpuBusyPeak", cpuBusyPeak);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void UpdateEntry(string instanceId, string ec2Name, string instanceName, double cpuBusy, int todayLoadCount,
            double todayLoadSize, double cpuBusyAverage, double cpuBusyPeak) {
            string cmdText = @"UPDATE MonitorEC2 SET `CpuBusy` = @CpuBusy, 
                            `TodayLoadCount` = @TodayLoadCount, `TodayLoadSize` = @TodayLoadSize, `CpuBusyAverage` = @CpuBusyAverage,
                            `CpuBusyPeak` = @CpuBusyPeak WHERE `InstanceId` = @InstanceId"; //`EC2Name` = @EC2Name, `InstanceName` = @InstanceName, 
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText + Helper.CommandParameter, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@InstanceId", instanceId);
                //command.Parameters.AddWithValue("@EC2Name", ec2Name);
                //command.Parameters.AddWithValue("@InstanceName", instanceName);
                command.Parameters.AddWithValue("@CpuBusy", cpuBusy);
                command.Parameters.AddWithValue("@TodayLoadCount", todayLoadCount);
                command.Parameters.AddWithValue("@TodayLoadSize", todayLoadSize);
                command.Parameters.AddWithValue("@CpuBusyAverage", cpuBusyAverage);
                command.Parameters.AddWithValue("@CpuBusyPeak", cpuBusyPeak);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public bool CheckDataEntry(string instanceId) {
            var cmdText = "SELECT InstanceId FROM MonitorEC2 WHERE InstanceId = @InstanceId";
            var exists = false;

            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText + Helper.CommandParameter, connection);
                    command.CommandTimeout = 0;
                    command.Parameters.AddWithValue("@InstanceId", instanceId);

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

        public void DeleteAllEntry() {
            string cmdText = @"DELETE FROM MonitorEC2";
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText + Helper.CommandParameter, connection);
                command.CommandTimeout = 0;
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public DataTable GetEC2LoaderIPInformation()
        {
            var cmdText = @"SELECT  M.InstanceId as InstanceId, 
                                    E.IPAddress as IPAddress
                            FROM MonitorEC2 as M
                            INNER JOIN EC2List as E
                            ON E.InstanceId = M.InstanceID";
            DataTable ec2LoaderIPInformation = new DataTable("EC2LoaderIPInformation");
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    var adapter = new MySqlDataAdapter(command);
                    adapter.Fill(ec2LoaderIPInformation);
                }
            }
            catch
            {
                
            }
            return ec2LoaderIPInformation;
        }

        public bool CheckIsActive(string instanceId)
        {
            var cmdText = "SELECT LoaderActive FROM MonitorEC2 WHERE InstanceId = @InstanceId";
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    var command = new MySqlCommand(cmdText + Helper.CommandParameter, connection);
                    command.CommandTimeout = 0;
                    command.Parameters.AddWithValue("@InstanceId", instanceId);
                    connection.Open();
                    string tempString = command.ExecuteScalar().ToString();
                    return Convert.ToInt32(tempString) == 1;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
