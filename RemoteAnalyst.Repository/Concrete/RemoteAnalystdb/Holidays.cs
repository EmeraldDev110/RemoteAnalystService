using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class Holidays {

        private readonly string _connectionString;

        public Holidays(string connectionString) {
            _connectionString = connectionString;
        }
        public DataTable GetWorkDayFactor(string systemSerial, DateTime workdayDate) {
            var perference = new DataTable();
            //UserPreferencesService service = new UserPreferencesService();
            string cmdText = @"SELECT Increase, Percentage, FromHour, ToHour
                               FROM Holidays WHERE SystemSerial =  @SystemSerial
                                AND Date = @WorkDate";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@WorkDate", workdayDate);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(perference);
            }

            return perference;
        }
    }
}
