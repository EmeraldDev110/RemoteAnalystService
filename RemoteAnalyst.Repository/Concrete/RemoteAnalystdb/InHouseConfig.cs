using System;
using System.Data;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class InHouseConfig {
        private readonly string _connectionString;
        public InHouseConfig(string connectionString) {
            _connectionString = connectionString;
        }
        public DataTable GetNonstopVolumnAndIpPair() {
            DataTable volumnIpPair = new DataTable();
            const string cmdText = @"SELECT `Volumn`, `IP` FROM InHouseConfig";
            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    var adapter = new MySqlDataAdapter(command);
                    adapter.Fill(volumnIpPair);
                }
            } catch (Exception ex) {
                return volumnIpPair;
            }
            return volumnIpPair;
        }
    }
}
