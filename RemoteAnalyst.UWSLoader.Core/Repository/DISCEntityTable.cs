using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using System.Linq;
using System.Text;
using MySqlConnector;

namespace RemoteAnalyst.UWSLoader.Core.Repository {
    class DISCEntityTable {
        private readonly string _connectionString;

        public DISCEntityTable(string connectionString) {
            this._connectionString = connectionString;
        }

        public List<string> GetDeviceNames(string entityTableName) {
            var deviceNames = new List<string>();
            try {
                string cmdText = "SELECT DISTINCT DeviceName FROM " + entityTableName;
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read()) {
                        deviceNames.Add(reader["DeviceName"].ToString());
                    }
                }
            }
            catch (Exception) {
            }
            return deviceNames;
        }

        public DataTable GetDISCEntityTableIntervalList(string entityTableName) {
            var intervalList = new DataTable();
            try {
                string cmdText = "SELECT DISTINCT FromTimestamp, ToTimestamp FROM `" + entityTableName + "`";
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    var adapter = new MySqlDataAdapter(command);
                    adapter.Fill(intervalList);
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return intervalList;
        }

        public bool CheckTableName(string entityTableName) {
            const string cmdText = @"SELECT COUNT(*) AS TableName
                                    FROM information_schema.tables 
                                    WHERE table_name = @TableName";
            bool tableExists = true;
            using (var mySqlConnection = new MySqlConnection(_connectionString)) {
                mySqlConnection.Open();
                var cmd = new MySqlCommand(cmdText, mySqlConnection);
                // cmd.prepare();
                cmd.Parameters.AddWithValue("@TableName", entityTableName);
                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read()) {
                    if (Convert.ToInt16(reader["TableName"]).Equals(0)) {
                        tableExists = false;
                    }
                }
            }

            return tableExists;
        }
    }
}
