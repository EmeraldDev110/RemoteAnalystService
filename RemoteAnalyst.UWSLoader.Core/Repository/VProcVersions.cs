using System;
using System.Collections.Generic;
using MySqlConnector;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.UWSLoader.Core.Repository {
    class VProcVersions {
        private readonly string _connectionString;

        public VProcVersions(string connectionString) {
            _connectionString = connectionString;
        }

        public string GetClassName(string vProcVersion) {
            string className = "";

            string cmdText = "SELECT ClassName FROM VProcVersions " +
                             "WHERE VPROCVersion = @VPROCVersion";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@VPROCVersion", vProcVersion);
                connection.Open();
                var reader = command.ExecuteReader();
                if (reader.Read())
                    className = Convert.ToString(reader["ClassName"]);
            }

            return className;
        }

        public string GetDataDictionary(string vProcVersion) {
            string dataDictionary = "";

            string cmdText = "SELECT DataDictionary FROM VProcVersions " +
                             "WHERE VPROCVersion = @VPROCVersion";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@VPROCVersion", vProcVersion);
                connection.Open();
                var reader = command.ExecuteReader();
                if (reader.Read())
                    dataDictionary = Convert.ToString(reader["DataDictionary"]);
            }

            return dataDictionary;
        }

        public string GetVprocVersion(string vProcVersion) {
            string dataDictionary = "";

            string cmdText = "SELECT VPROCVersion FROM VProcVersions " +
                             "WHERE VPROCVersion = @VPROCVersion";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@VPROCVersion", vProcVersion);
                connection.Open();
                var reader = command.ExecuteReader();
                if (reader.Read())
                    dataDictionary = Convert.ToString(reader["VPROCVersion"]);
            }

            return dataDictionary;
        }
    }
}
