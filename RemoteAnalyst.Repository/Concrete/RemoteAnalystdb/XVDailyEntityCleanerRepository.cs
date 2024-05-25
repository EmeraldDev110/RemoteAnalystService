using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySqlConnector;
using RemoteAnalyst.Repository.Resources;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class XVDailyEntityCleanerRepository {

        private readonly string _connectionString;
        private readonly bool _isLocalAnalyst = false;

        public XVDailyEntityCleanerRepository(string connectionString) {
            _connectionString = connectionString;
            RAInfoRepository raInfo = new RAInfoRepository();
            string productName = raInfo.GetValue("ProductName");
            if (productName == "PMC")
            {
                _isLocalAnalyst = true;
            }
        }

        public string decryptPassword(string connectionString)
        {
            if (_isLocalAnalyst)
            {
                var decrypt = new Decrypt();
                var decryptedString = decrypt.strDESDecrypt(connectionString);
                return decryptedString;
            }
            else
            {
                return connectionString;
            }
        }

        public DataTable GetAllSystemInformation() {
            string cmdText = "SELECT S.SystemName, S.SystemSerial, S.TimeZone, D.MySQLConnectionString FROM System_Tbl S INNER JOIN DatabaseMappings D ON S.SystemSerial = D.SystemSerial;";
            DataTable dataTable = new DataTable();
            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    var adapter = new MySqlDataAdapter(command);
                    adapter.Fill(dataTable);
                    foreach (DataRow row in dataTable.Rows)
                    {
                        if (row["MySQLConnectionString"] != null && Convert.ToString(row["MySQLConnectionString"]) != "")
                        {
                            row["MySQLConnectionString"] = decryptPassword(Convert.ToString(row["MySQLConnectionString"]));
                        }

                    }
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return dataTable;
        }

        public DataTable GetXVDailyTables(int retentionDays) {
            string cmdText = "SELECT table_name FROM information_schema.tables WHERE table_name LIKE 'XVDATA_%_%_%_%_%';";
            DataTable dataTable = new DataTable();
            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    var adapter = new MySqlDataAdapter(command);
                    adapter.Fill(dataTable);
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return dataTable;
        }

        public void DeleteXVDailyTableByName(string tableName) {
            string cmdText = @"DROP TABLE " + tableName;

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}
