using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class SpecialDays {

        private readonly string _connectionString;

        public SpecialDays(string connectionString) {
            _connectionString = connectionString;
        }

        public DataTable GetSpecialDays(string systemSerial) {
            var specialDays = new DataTable();

            string cmdText = @"SELECT SystemSerial, SpecialDayType, SpecialDate FROM SpecialDays WHERE SystemSerial = @SystemSerial";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(specialDays);
            }

            return specialDays;
        }
    }
}
