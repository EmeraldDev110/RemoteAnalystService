using System;
using System.Data;
using MySqlConnector;
using RemoteAnalyst.UWSLoader.Core.BaseClass;

namespace RemoteAnalyst.UWSLoader.Core.Repository {
    internal class DatabaseMappings {
        private readonly string _connectionString = "";
        private readonly bool _isLocalAnalyst = false;

        public DatabaseMappings(string connectionString) {
            _connectionString = connectionString;
            RAInfo raInfo = new RAInfo(connectionString);
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

        public string GetConnectionString(string systemSerial) {
            string cmdText = "SELECT ConnectionString FROM DatabaseMappings WHERE SystemSerial = @SystemSerial";
            string connectionString = "";

            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@SystemSerial", systemSerial);

                    connection.Open();
                    MySqlDataReader reader = command.ExecuteReader();

                    if (reader.Read()) {
                        connectionString = decryptPassword(Convert.ToString(reader["ConnectionString"]));
                    }
                }
            }
            catch {
                connectionString = "";
            }

            return connectionString;
        }

        public DataTable GetAllConnectionString() {
            var retVal = new DataTable();
            string cmdText = "SELECT SystemSerial, ConnectionString FROM DatabaseMappings ORDER BY SystemSerial";
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(retVal);

                foreach (DataRow row in retVal.Rows)
                {
                    if (row["ConnectionString"] != null && Convert.ToString(row["ConnectionString"]) != "")
                    {
                        row["ConnectionString"] = decryptPassword(Convert.ToString(row["ConnectionString"]));
                    }

                }
            }

            return retVal;
        }

        public bool CheckDatabase(string connectionString) {
            string cmdText = "SELECT * FROM ZmsBladeDataDictionary LIMIT 1";
            bool exists = false;

            try {
                using (var connection = new MySqlConnection(connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    connection.Open();
                    MySqlDataReader reader = command.ExecuteReader();

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

        public void InsertNewEntry(string systemSerial, string perSystemConnectionString) {
            string cmdText = "INSERT INTO DatabaseMappings (SystemSerial, ConnectionString) VALUES " +
                             "(@SystemSerial, @ConnectionString)";
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@ConnectionString", encryptPassword(perSystemConnectionString));
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public string GetMySQLConnectionString(string systemSerial) {
            string cmdText = "SELECT MySQLConnectionString FROM DatabaseMappings WHERE SystemSerial = @SystemSerial";
            string connectionString = "";

            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@SystemSerial", systemSerial);

                    connection.Open();
                    MySqlDataReader reader = command.ExecuteReader();

                    if (reader.Read()) {
                        if (!reader.IsDBNull(0)) {
                            connectionString = decryptPassword(Convert.ToString(reader["MySQLConnectionString"]));
                        }
                    }
                }
            }
            catch {
                connectionString = "";
            }

            return connectionString;
        }

        public string GetMySQLConnectionString() {
            string cmdText = "SELECT MySQLConnectionString FROM DatabaseMappings ORDER BY MySQLConnectionString LIMIT 1";
            string connectionString = "";

            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);

                    connection.Open();
                    MySqlDataReader reader = command.ExecuteReader();

                    if (reader.Read()) {
                        connectionString = decryptPassword(Convert.ToString(reader["MySQLConnectionString"]));
                    }
                }
            }
            catch {
                connectionString = "";
            }

            return connectionString;
        }

        public void UpdateMySQLConnectionString(string systemSerial, string newConnectionString) {
            string cmdText = "UPDATE DatabaseMappings SET MySQLConnectionString = @MySQLConnectionString WHERE SystemSerial = @SystemSerial";
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@MySQLConnectionString", encryptPassword(newConnectionString));
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}