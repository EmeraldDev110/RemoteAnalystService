using System;
using System.Collections.Generic;
using MySqlConnector;
using RemoteAnalyst.Repository.Repositories;
using RemoteAnalyst.Repository.Resources;

namespace RemoteAnalyst.UWSLoader.DiskBrowserLookBack {
    public class SystemTbl {
        private readonly string _connectionString;
        private readonly bool _isLocalAnalyst = false;
        public SystemTbl(string connectionString) {
            _connectionString = connectionString;
            RAInfoRepository raInfo = new RAInfoRepository();
            string productName = raInfo.GetValue("ProductName");
            if (productName == "PMC")
            {
                _isLocalAnalyst = true;
            }
        }

        public string encryptPassword(string connectionString)
        {
            if (_isLocalAnalyst)
            {
                var encrypt = new Decrypt();
                var encryptedString = encrypt.strDESEncrypt(connectionString);
                return encryptedString;
            }
            else
            {
                return connectionString;
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

        public List<string> GetSystemSerials() {
            string cmdText = "SELECT SystemSerial FROM System_Tbl;";
            var systemSerials = new List<string>();
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                connection.Open();
                var reader = command.ExecuteReader();
                while (reader.Read()) {
                    systemSerials.Add(reader["SystemSerial"].ToString());
                }
            }
            return systemSerials;
        }

        public bool CheckDatabase(string databaseName) {
            string cmdText = "SELECT name FROM master.dbo.sysdatabases WHERE name = @DatabaseName";
            bool exists = false;

            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@DatabaseName", databaseName);
                    connection.Open();
                    var reader = command.ExecuteReader();

                    if (reader.Read()) {
                        exists = true;
                    }
                }
            }
            catch {
                exists = false;
            }

            return exists;
        }

        public bool CheckMySqlDatabase(string databaseName, string mysqlConnectionString) {
            string cmdText = "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = @DatabaseName";
            bool exists = false;
            try {
                using (var mySqlConnection = new MySqlConnection(mysqlConnectionString)) {
                    mySqlConnection.Open();
                    var cmd = new MySqlCommand(cmdText, mySqlConnection);
                    cmd.Parameters.AddWithValue("@DatabaseName", databaseName);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read()) {
                        exists = true;
                    }
                }
            }
            catch (Exception ex) {
                exists = false;
            }
            return exists;
        }

        public List<DatabaseMapping> GetDatabaseMappings() {
            string cmdText = "SELECT * FROM DatabaseMappings WHERE DATALENGTH(MySQLConnectionString) != 0";
            List<DatabaseMapping> databases = new List<DatabaseMapping>();
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                connection.Open();
                var reader = command.ExecuteReader();

                while (reader.Read()) {
                    DatabaseMapping database = new DatabaseMapping {
                        SystemSerial = reader["SystemSerial"].ToString(),
                        ConnStr = decryptPassword(reader["ConnectionString"].ToString()),
                        MySqlConnStr = decryptPassword(reader["MySQLConnectionString"].ToString())
                    };
                    databases.Add(database);
                }
            }
            return databases;
        }

        public bool CheckTableName(string tableName, string mysqlConnectionString) {
            const string cmdText = @"SELECT COUNT(*) AS TableName
                            FROM information_schema.tables 
                            WHERE table_name = @TableName";
            bool tableExists = true;
            using (var mySqlConnection = new MySqlConnection(mysqlConnectionString)) {
                mySqlConnection.Open();
                var cmd = new MySqlCommand(cmdText, mySqlConnection);
                // cmd.prepare();
                cmd.Parameters.AddWithValue("@TableName", tableName);
                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read()) {
                    if (Convert.ToInt16(reader["TableName"]).Equals(0)) {
                        tableExists = false;
                    }
                }
            }

            return tableExists;
        }

        public bool IsNTSSystem(string systemSerial) {
            const string cmdText = "SELECT IsNTS FROM FROM System_Tbl WHERE SystemSerial = @SystemSerial";
            bool isNTS = true;

            using (var mySqlConnection = new MySqlConnection(_connectionString)) {
                mySqlConnection.Open();
                var cmd = new MySqlCommand(cmdText, mySqlConnection);
                // cmd.prepare();
                cmd.Parameters.AddWithValue("@SystemSerial", systemSerial);
                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read()) {
                    /*
                    if (Convert.ToInt16(reader["IsNTS"]).Equals(0)) {
                        isNTS = false;
                    }
                    */
                    var isNTSValue = Convert.ToInt32(reader["IsNTS"]);
                    if (isNTSValue == 1) {   //IsNTS: 0 -> RA, 1 -> NTS, 2 -> Evaluation
                        isNTS = true;
                    }
                }
            }
            return isNTS;
        }
    }
}