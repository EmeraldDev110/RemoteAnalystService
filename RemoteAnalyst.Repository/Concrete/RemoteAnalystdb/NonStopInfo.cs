using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class NonStopInfo
    {
        private readonly string _connectionString = "";

        public NonStopInfo(string connectionString) {
            _connectionString = connectionString;
        }

        public DataTable GetNonStopInfo() {
            var nonstopData = new DataTable();
            string cmdText = "SELECT * FROM NonStopInfo ";
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(nonstopData);
            }
            return nonstopData;
        }
    }
}
