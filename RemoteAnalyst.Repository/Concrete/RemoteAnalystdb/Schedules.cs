using System.Collections.Generic;
using System.Data;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb
{
    public class Schedules
    {
        private readonly string _connectionString = "";

        public Schedules(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DataTable GetSchedules(int typeId)
        {
            //CusAnalystService service = new CusAnalystService();
            var cmdText = @"SELECT S.SystemSerial, S.ScheduleId, SS.SystemName, SBS.ProgramFile, SBS.BatchSequenceProfileId,
                            (SELECT MAX(ReportDownloadId) + 1 from ReportDownloads) as ReportDownloadId,
                            TypeId, FrequencyId, DetailTypeId,
                            AlertException, Overlapping, DailyOn, DailyAt,
                            WeeklyOn, WeeklyFor, WeeklyFrom, WeeklyTo,
                            MonthlyOn, MonthlyOnWeekDay, MonthlyFor, MonthlyFrom, MonthlyTo,
                            IsMonthlyOn, IsMonthlyFor, Email, ReportFromHour, ReportToHour, HourBoundaryTrigger
                            FROM Schedules AS S
                            INNER JOIN ScheduleFrequencies AS SF ON S.ScheduleId =  SF.ScheduleId
                            INNER JOIN ScheduleEmails AS SE ON S.ScheduleId =  SE.ScheduleId
                            INNER JOIN System_Tbl AS SS ON S.SystemSerial = SS.SystemSerial
                            LEFT JOIN ScheduleBatchSequence AS SBS ON S.ScheduleId = SBS.ScheduleId
                            WHERE TypeId = @TypeId";
            var scheduleData = new DataTable();

            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@TypeId", typeId);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(scheduleData);
            }

            return scheduleData;
        }

        public DataTable GetPinInfo(int scheduleId) {
            var cmdText = @"SELECT IsLowPin, IsHighPin, IsAllSubVol, SubVols
                            FROM ScheduleWeeklyPin
                            WHERE ScheduleId = @ScheduleId";
            var scheduleData = new DataTable();

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@ScheduleId", scheduleId);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(scheduleData);
            }

            return scheduleData;
        }
        public string GetQTParam(int scheduleId)
        {
            var cmdText = @"SELECT Alert, `Option` FROM ScheduleQuickTuners WHERE ScheduleId = @ScheduleId";
            var scheduleData = new DataTable();

            var param = "";
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@ScheduleId", scheduleId);
                connection.Open();
                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    param = reader["Alert"] + "|" + reader["Option"];
                }
            }

            return param;
        }

        public string GetDDParam(int scheduleId)
        {
            var cmdText =
                @"SELECT Params, ReportIds, ChartIds, MaxRow, ExcelVersion FROM ScheduleDeepDives WHERE ScheduleId = @ScheduleId";
            var scheduleData = new DataTable();

            var param = "";
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@ScheduleId", scheduleId);
                connection.Open();
                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    param = reader["Params"] + "|" + reader["ReportIds"] + "|" + reader["ChartIds"] + "|" +
                            reader["MaxRow"] + "|" + reader["ExcelVersion"];
                }
            }

            return param;
        }

    }
}