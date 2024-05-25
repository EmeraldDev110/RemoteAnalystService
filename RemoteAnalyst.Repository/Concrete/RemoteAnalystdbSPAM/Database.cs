using System;
using System.Collections.Generic;
using System.Text;
using MySqlConnector;
using System.IO;
using RemoteAnalyst.Repository.Resources;
using System.Linq;
using log4net;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM {
    public class Database {
        private readonly string _connectionString = "";
        
        public Database(string connectionString) {
            _connectionString = connectionString;
        }

        public static string RemovePassword(string connectionString)
        {
            try
            {
                if (String.IsNullOrEmpty(connectionString))
                {
                    return connectionString;
                }
                if ((connectionString.Contains("PASSWORD") && connectionString.Contains(";")) || (connectionString.Contains("password") && connectionString.Contains(";")))
                {
                    List<string> strlist = connectionString.Split(';').ToList();
                    for (int i = 0; i < strlist.Count; i++)
                    {
                        if (strlist[i].Contains("PASSWORD") || connectionString.Contains("password"))
                        {
                            strlist.Remove(strlist[i]);
                            break;
                        }
                    }
                    string concat = String.Join(";", strlist.ToArray());
                    return concat;
                }
                else
                {
                    return connectionString;
                }
            }
            catch (Exception e)
            {
                return connectionString;
            }
        }

        public string CreateDatabase(string databaseName, ILog log) {
            string DATABASE_KEY = "DATABASE=";
            log.DebugFormat("_connectionString: {0}", Database.RemovePassword(_connectionString)); //master DB connection
            string cmdScript = @"CREATE DATABASE " + databaseName;

            //Remove the database name.
            var tempConnectionString = _connectionString.Split(';');
            var newConnectionString = new StringBuilder();
            foreach (var s in tempConnectionString) {
                if (!s.ToUpper().StartsWith(DATABASE_KEY))
                    newConnectionString.Append(s + ";");
            }

            log.DebugFormat("newConnectionString (Temporary): {0}", Database.RemovePassword(newConnectionString.ToString()));
            try {
                using (var connection = new MySqlConnection(newConnectionString.ToString())) {
                    var command = new MySqlCommand(cmdScript, connection);
                    connection.Open();
                    command.CommandTimeout = 0;
                    command.ExecuteNonQuery();
                }
                if (!newConnectionString.ToString().Contains(DATABASE_KEY))
                {
                    newConnectionString.Append(DATABASE_KEY).Append(databaseName).Append(";").ToString();
                }
            }
            catch (Exception ex) {
                log.ErrorFormat("Error: {0}", ex);
                newConnectionString.Clear();
            }
            log.DebugFormat("newConnectionString: {0}", Database.RemovePassword(newConnectionString.ToString()));
            return newConnectionString.ToString();
        }

        public void CreateTables(string databaseName) {
            string cmdText = Resource1.NonStopSPAM;
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                connection.Open();
                command.CommandTimeout = 0;
                command.ExecuteNonQuery();
            }
        }

        public List<string> GetTablesFromSchema(string tableSchema, string tablePattern)
        {
            const string cmdText = @"SELECT table_name as TableName FROM information_schema.tables 
                    WHERE table_name LIKE @TablePattern AND TABLE_SCHEMA = @TableSchema";
            var tableList = new List<String>();
            using (var mySqlConnection = new MySqlConnection(_connectionString))
            {
                mySqlConnection.Open();
                var cmd = new MySqlCommand(cmdText, mySqlConnection);
                cmd.Parameters.AddWithValue("@TablePattern", tablePattern);
                cmd.Parameters.AddWithValue("@TableSchema", tableSchema);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (!tableList.Contains(reader["TableName"].ToString()))
                        tableList.Add(reader["TableName"].ToString());
                }

            }
            return tableList;
        }

        public bool CheckTableExists(string tableName, string tableSchema) {
            const string cmdText = @"SELECT COUNT(*) AS TableName FROM information_schema.tables WHERE table_name = @TableName AND TABLE_SCHEMA = @TableSchema";
            bool tableExists = true;
            using (var mySqlConnection = new MySqlConnection(_connectionString)) {
                mySqlConnection.Open();
                var cmd = new MySqlCommand(cmdText, mySqlConnection);
                // cmd.prepare();
                cmd.Parameters.AddWithValue("@TableName", tableName);
                cmd.Parameters.AddWithValue("@TableSchema", tableSchema);
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read()) {
                    if (Convert.ToInt16(reader["TableName"]).Equals(0)) {
                        tableExists = false;
                    }
                }
            }
            return tableExists;
        }

        public List<String> GetQNMTableNamesInDatabase(string tableSchema) {
            const string cmdText = @"SELECT table_name as TableName FROM information_schema.tables WHERE TABLE_SCHEMA = @TableSchema and table_name LIKE 'QNM%' ";
            var tableList = new List<String>();
            using (var mySqlConnection = new MySqlConnection(_connectionString)) {
                mySqlConnection.Open();
                var cmd = new MySqlCommand(cmdText, mySqlConnection);
                // cmd.prepare();
                cmd.Parameters.AddWithValue("@TableSchema", tableSchema);
                var reader = cmd.ExecuteReader();
                while (reader.Read()) {
                    if (!tableList.Contains(reader["TableName"].ToString()))
                        tableList.Add(reader["TableName"].ToString());
                }

            }
            return tableList;
        }

        public void CreateForecastsTable() {
            string sqlStr = @"CREATE TABLE `Forecasts` (
                              `FromTimestamp` datetime NOT NULL,
                              `CpuNumber` int(11) NOT NULL,
                              `CPUBusy` double DEFAULT NULL,
                              `MemoryUsed` bigint(20) DEFAULT NULL,
                              `CPUQueue` double DEFAULT NULL,
                              `StdDevCPUBusy` double DEFAULT NULL,
                              `StdDevMemoryUsed` double DEFAULT NULL,
                              `StdDevCPUQueue` double DEFAULT NULL,
                              PRIMARY KEY (`FromTimestamp`,`CpuNumber`)
                            ) ENGINE=InnoDB DEFAULT CHARSET=latin1;";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(sqlStr, connection) { CommandTimeout = 0 };
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void CreateForecastIpusTable() {
            string sqlStr = @"CREATE TABLE `ForecastIpus` (
                              `FromTimestamp` datetime NOT NULL,
                              `CpuNumber` int(11) NOT NULL,
                              `IpuNumber` int(11) NOT NULL,
                              `IpuBusy` double DEFAULT NULL,
                              `IpuQueue` double DEFAULT NULL,
                              `StdDevIpuBusy` double DEFAULT NULL,
                              `StdDevIpuQueue` double DEFAULT NULL,
                              PRIMARY KEY (`FromTimestamp`,`CpuNumber`,`IpuNumber`)
                            ) ENGINE=InnoDB DEFAULT CHARSET=latin1;";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(sqlStr, connection) { CommandTimeout = 0 };
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public List<string> GetDetailTableList(string systemSerial, string databaseName) {
            string sqlStr = @"SELECT table_name FROM information_schema.tables
                            WHERE table_name LIKE '" + systemSerial + @"_%'
                            AND TABLE_SCHEMA = @TableName";

            var tableList = new List<string>();
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(sqlStr, connection) { CommandTimeout = 0 };
                command.Parameters.AddWithValue("@TableName", databaseName);
                connection.Open();
                var reader = command.ExecuteReader();

                while (reader.Read()) {
                    if(!tableList.Contains(reader["table_name"].ToString()))
                        tableList.Add(reader["table_name"].ToString());
                }
            }

            return tableList;
        }

        public void UpdateSystemName(string tableName, string newSystemName) {
            string sqlStr = @"UPDATE " + tableName + " SET SystemName = @SystemName";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(sqlStr, connection) { CommandTimeout = 0 };
                command.Parameters.AddWithValue("@SystemName", newSystemName.Replace("\\", ""));
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void CreateForecastDiskTable() {
            
            string sqlStr = @"CREATE TABLE `ForecastDisks` (
                            `FromTimestamp` datetime NOT NULL,
                            `DeviceName` varchar(45) NOT NULL,
                            `QueueLength` double DEFAULT NULL,
                            `StdDevQueueLength` double DEFAULT NULL,
                            `DP2Busy` double DEFAULT NULL,
                            `StdDevDP2Busy` double DEFAULT NULL,
                            PRIMARY KEY (`FromTimestamp`,`DeviceName`)
                            ) ENGINE=InnoDB DEFAULT CHARSET=latin1;";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(sqlStr, connection) { CommandTimeout = 0 };
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void AlterForecastDisk() {
            string sqlStr = @"ALTER TABLE `ForecastDisks` 
                            ADD COLUMN `DP2Busy` DOUBLE NULL AFTER `StdDevQueueLength`,
                            ADD COLUMN `StdDevDP2Busy` DOUBLE NULL AFTER `DP2Busy`;";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(sqlStr, connection) { CommandTimeout = 0 };
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void CreateForecastStorageTable() {

            string sqlStr = @"CREATE TABLE `ForecastStorages` (
                            `FromTimestamp` datetime NOT NULL,
                            `DeviceName` varchar(45) NOT NULL,
                            `UsedPercent` double DEFAULT NULL,
                            `StdDevUsedPercent` double DEFAULT NULL,
                            PRIMARY KEY (`FromTimestamp`,`DeviceName`)
                            ) ENGINE=InnoDB DEFAULT CHARSET=latin1;";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(sqlStr, connection) { CommandTimeout = 0 };
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void CreateForecastProcess() {
            string sqlStr = @"CREATE TABLE `ForecastProcesses` (
                                              `FromTimestamp` datetime NOT NULL,
                                              `ProcessName` varchar(10) NOT NULL,
                                              `CpuNumber` varchar(2) NOT NULL,
                                              `Pin` int(11) NOT NULL,
                                              `Volume` varchar(8) NOT NULL,
                                              `SubVol` varchar(8) NOT NULL,
                                              `FileName` varchar(8) NOT NULL,
                                              `ProcessBusy` double DEFAULT NULL,
                                              `StdDevProcessBusy` double DEFAULT NULL,
                                              `RecvQueueLength` double DEFAULT NULL,
                                              `StdDevRecvQueueLength` double DEFAULT NULL,
                                              PRIMARY KEY (`FromTimestamp`,`ProcessName`,`CpuNumber`,`Pin`,`Volume`,`SubVol`,`FileName`)
                                            ) ENGINE=InnoDB DEFAULT CHARSET=latin1;";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(sqlStr, connection) { CommandTimeout = 0 };
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void CreateForecastTmf() {
            string sqlStr = @"CREATE TABLE `ForecastTmfs` (
                                          `FromTimestamp` datetime NOT NULL,
                                          `ProcessName` varchar(10) NOT NULL,
                                          `CpuNumber` varchar(2) NOT NULL,
                                          `Pin` int(11) NOT NULL,
                                          `Volume` varchar(8) NOT NULL,
                                          `SubVol` varchar(8) NOT NULL,
                                          `FileName` varchar(8) NOT NULL,
                                          `AbortPercent` double DEFAULT NULL,
                                           `StdDevAbortPercent` double DEFAULT NULL,
                                          PRIMARY KEY (`FromTimestamp`,`ProcessName`,`CpuNumber`,`Pin`,`Volume`,`SubVol`,`FileName`)
                                        ) ENGINE=InnoDB DEFAULT CHARSET=latin1;";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(sqlStr, connection) { CommandTimeout = 0 };
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void CreateExceptionsTable() {
            string sqlStr = @"CREATE TABLE `Exceptions` (
                          `FromTimestamp` datetime NOT NULL,
                          `Entity` varchar(15) NOT NULL,
                          `Counter` varchar(15) NOT NULL,
                          `Instance` varchar(15) NOT NULL,
                          `Actual` double DEFAULT NULL,
                          `Upper` double DEFAULT NULL,
                          `Lower` double DEFAULT NULL,
                          `DisplayRed` tinyint(4) DEFAULT NULL,
                          PRIMARY KEY (`FromTimestamp`,`Entity`,`Counter`,`Instance`)
                        ) ENGINE=InnoDB DEFAULT CHARSET=latin1;";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(sqlStr, connection) { CommandTimeout = 0 };
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public bool CheckColumn(string databaseName, string tableName, string columnName) {
            string cmdText = @"SELECT  COUNT(*) AS ColumnName FROM information_schema.COLUMNS 
                                WHERE TABLE_SCHEMA = @DatabaseName 
                                AND TABLE_NAME = @TableName 
                                AND COLUMN_NAME = @ColumnName";
            bool exists = true;
            try {
                using (var mySqlConnection = new MySqlConnection(_connectionString)) {
                    mySqlConnection.Open();
                    var cmd = new MySqlCommand(cmdText, mySqlConnection);
                    cmd.Parameters.AddWithValue("@DatabaseName", databaseName);
                    cmd.Parameters.AddWithValue("@TableName", tableName);
                    cmd.Parameters.AddWithValue("@ColumnName", columnName);
                    cmd.CommandTimeout = 0;
                    MySqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read()) {
                        if (Convert.ToInt16(reader["ColumnName"]).Equals(0)) {
                            exists = false;
                        }
                    }
                }
            }
            catch (Exception ex) {
                exists = false;
            }
            return exists;
        }

        public void BulkDeleteSingleParameter(String cmdText, String parameterName, List<String> singleParameters) {
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText + Infrastructure.Helper.CommandParameter, connection);
                connection.Open();
                foreach (String singleParameter in singleParameters) {
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue(parameterName, singleParameter);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}