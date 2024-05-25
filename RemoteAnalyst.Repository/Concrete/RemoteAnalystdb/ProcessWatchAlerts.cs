using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class ProcessWatchAlerts {
        private readonly string _connectionString;
        private readonly string _connectionStringSystem;

        public ProcessWatchAlerts(string connectionString, string connectionStringSystem)
        {
            _connectionString = connectionString;
            _connectionStringSystem = connectionStringSystem;
        }
        public DataTable GetProcessWatch(string systemSerial) {
            string cmdText = @"SELECT idProcessWatchAlerts,AlertName,SystemSerial,SystemName,ProgramName,
                    RunsOn,EnableStart,MustStartBy,EnableStop,MustStopBy,EnableMax,MaxProcess,
                    EnableThres,OutOfBalanceLimit,IsOSSProgram,OSSProgramName,UpdatedDate, EnableTMF, AbortThres, EnableMin, MinProcess
                    FROM ProcessWatchAlerts WHERE SystemSerial = @SystemSerial";
            var processWatch = new DataTable();

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(processWatch);
            }

            return processWatch;
        }

        public DataTable GetStartedBy(DateTime mustStartBy, string processTable, DateTime startTime, DateTime endTime, string volume, string subVol, string fileName) {
            string cmdText = @"SELECT Count(ProcessName) As CountProcessName FROM " + processTable +
                            @" WHERE TIME(FromTimeStamp) <= @StartTimeOnly AND Volume= @Volume AND SubVol= @SubVol AND FileName= @FileName
                            AND FromTimeStamp>= @StartTime AND ToTimeStamp<= @StopTime GROUP BY Volume, SubVol, FileName";
            var processWatch = new DataTable();

            using (var connection = new MySqlConnection(_connectionStringSystem)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@StartTimeOnly", mustStartBy.ToString("HH:mm:ss"));
                command.Parameters.AddWithValue("@Volume", volume);
                command.Parameters.AddWithValue("@SubVol", subVol);
                command.Parameters.AddWithValue("@FileName", fileName);
                command.Parameters.AddWithValue("@StartTime", startTime);
                command.Parameters.AddWithValue("@StopTime", endTime);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(processWatch);
            }

            return processWatch;
        }

        public DataTable GetStoppedBy(DateTime mustStopBy, string processTable, DateTime startTime, DateTime endTime, string volume, string subVol, string fileName) {
            string cmdText = @"SELECT Count(ProcessName) As CountProcessName FROM " + processTable +
                            @" WHERE TIME(ToTimeStamp) >= @ToTimeOnly AND Volume = @Volume AND SubVol = @SubVol AND FileName = @FileName 
                            AND FromTimeStamp >= @StartTime AND ToTimeStamp < @StopTime GROUP BY Volume, SubVol, FileName";
            var processWatch = new DataTable();

            using (var connection = new MySqlConnection(_connectionStringSystem)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@ToTimeOnly", mustStopBy.ToString("HH:mm:ss"));
                command.Parameters.AddWithValue("@Volume", volume);
                command.Parameters.AddWithValue("@SubVol", subVol);
                command.Parameters.AddWithValue("@FileName", fileName);
                command.Parameters.AddWithValue("@StartTime", startTime);
                command.Parameters.AddWithValue("@StopTime", endTime);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(processWatch);
            }

            return processWatch;
        }

        public DataTable GetProcessCount(string processTable, DateTime startTime, DateTime endTime, string volume, string subVol, string fileName) {
            string cmdText = @" SELECT COUNT(Counts) as Total,'" +
                             startTime.ToString("HH:mm") + @"' AS FromIntv, '" +
                             endTime.ToString("HH:mm") + @"' AS ToIntv FROM (
                    SELECT COUNT(ProcessName) AS Counts FROM " + processTable + @"
                    WHERE Volume = @Volume AND SubVol = @SubVol AND FileName = @FileName
                    AND FromTimeStamp >= @StartTime
                    AND ToTimeStamp < @StopTime
                    GROUP BY Volume, SubVol, FileName, ProcessName, CPUNum, PIN
                ) AS CountList";
            var processWatch = new DataTable();

            using (var connection = new MySqlConnection(_connectionStringSystem)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@Volume", volume);
                command.Parameters.AddWithValue("@SubVol", subVol);
                command.Parameters.AddWithValue("@FileName", fileName);
                command.Parameters.AddWithValue("@StartTime", startTime);
                command.Parameters.AddWithValue("@StopTime", endTime);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(processWatch);
            }

            return processWatch;
        }

        public DataTable GetProcessBusy(string processTable, DateTime startTime, DateTime endTime, string volume, string subVol, string fileName) {
            var cmdText = @"SELECT (CpuBusyTime/DeltaTime)*100 as Busy, '" +
                startTime.ToString("HH:mm") + @"' AS FromIntv, '" +
                endTime.ToString("HH:mm") + @"' AS ToIntv, 
                ProcessName,CpuNum AS CPU, PIN, FromTimeStamp FROM " + processTable +
            @" WHERE Volume = @Volume AND SubVol = @SubVol AND FileName = @FileName 
            AND FromTimeStamp >= @StartTime
            AND ToTimeStamp < @StopTime";
            var processWatch = new DataTable();

            using (var connection = new MySqlConnection(_connectionStringSystem)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@Volume", volume);
                command.Parameters.AddWithValue("@SubVol", subVol);
                command.Parameters.AddWithValue("@FileName", fileName);
                command.Parameters.AddWithValue("@StartTime", startTime);
                command.Parameters.AddWithValue("@StopTime", endTime);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(processWatch);
            }

            return processWatch;
        }

        public DataTable GetAbortTrans(string processTable, DateTime startTime, DateTime endTime, string volume, string subVol, string fileName) {
            var cmdText = @"SELECT ((AbortTrans * 100) / BeginTrans) AS AbortTMF,'" +
                startTime.ToString("HH:mm") + "' AS FromIntv,'" +
                endTime.ToString("HH:mm") + @"' AS ToIntv, 
                ProcessName,CpuNum AS CPU, PIN, FromTimeStamp FROM " + processTable + 
            @" WHERE Volume = @Volume AND SubVol = @SubVol AND FileName = @FileName 
            AND FromTimeStamp >= @StartTime
            AND ToTimeStamp < @StopTime";
            var processWatch = new DataTable();

            using (var connection = new MySqlConnection(_connectionStringSystem)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@Volume", volume);
                command.Parameters.AddWithValue("@SubVol", subVol);
                command.Parameters.AddWithValue("@FileName", fileName);
                command.Parameters.AddWithValue("@StartTime", startTime);
                command.Parameters.AddWithValue("@StopTime", endTime);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(processWatch);
            }

            return processWatch;
        }
    }
}
