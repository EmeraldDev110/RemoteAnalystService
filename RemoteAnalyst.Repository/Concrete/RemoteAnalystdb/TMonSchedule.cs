using System;
using System.Data;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class TMonSchedule {
        private readonly string _connectionString = "";

        public TMonSchedule(string connectionString) {
            _connectionString = connectionString;
        }

        public DataTable GetTMonSchedules() {
            var schedules = new DataTable();
            const string cmdText = "SELECT Systemserial, TransSchedule, WeekDays, FirstTransmissionTime, `Interval`, ActiveFlag FROM TMonSchedule";
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection) { CommandTimeout = 0 };
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(schedules);
            }

            return schedules;
        }

        public int GetDelay(string systemSerial) {
            int delay = 0;
            const string cmdText = "SELECT TransmissionPossibleDelay FROM TMonSchedule " +
                                   "WHERE SystemSerial = @SystemSerial";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                connection.Open();
                var reader = command.ExecuteReader();
                if (reader.Read()) {
                    delay = Convert.ToInt32(reader["TransmissionPossibleDelay"]);
                }
                reader.Close();
            }
            return delay;
        }

        public int GetLoadTime(string systemSerial) {
            int loadTime = 0;
            const string cmdText = "SELECT LoadTime FROM TMonSchedule " +
                                   "WHERE SystemSerial = @SystemSerial";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                connection.Open();
                var reader = command.ExecuteReader();
                if (reader.Read()) {
                    loadTime = Convert.ToInt32(reader["LoadTime"]);
                }
                reader.Close();
            }
            return loadTime;
        }
    }
}