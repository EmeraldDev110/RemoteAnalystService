using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class Transmon {
        private readonly string _connectionString = "";

        public Transmon(string connectionString) {
            _connectionString = connectionString;
        }

        public DataTable GetTransmons() {
            var transmon = new DataTable();
            const string cmdText = "SELECT Systemserial, `Interval`, ExpectedCount, Allowance FROM Transmon";
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection) { CommandTimeout = 0 };
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(transmon);
            }

            return transmon;
        }
    }
}
