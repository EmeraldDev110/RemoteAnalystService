using System;
using System.Data;
using RemoteAnalyst.Repository.Resources;
using log4net;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class Batch {

        private readonly string _connectionString;
        private readonly bool _isLocalAnalyst = false;
        private readonly ILog _log;

        public Batch(string connectionString, ILog log, bool isLocalAnalyst) {
            _connectionString = connectionString;
            _log = log;
            _isLocalAnalyst = isLocalAnalyst;
            
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

        public DataTable GetAllSystemInformationForBatch() {
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
                        if (row["MySQLConnectionString"] != null && row["MySQLConnectionString"].ToString() != "")
                        {
                            row["MySQLConnectionString"] = decryptPassword(row["MySQLConnectionString"].ToString()); 
                        }

                    }
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return dataTable;
        }

        public DataTable GetBatchInformationBySystem() {
            string cmdText = @"SELECT P.BatchSequenceProfileId, P.Name, P.StartWindowStart, P.StartWindowEnd, P.StartWindowDoW,  
                                P.ExpectedFinishBy, P.AlertIFDoesNotStartOnTime, P.AlertIfDoesNotFinishOnTime, P.AlertIfOrderNotFollowed, 
                                GROUP_CONCAT(DISTINCT AR.EmailAddress SEPARATOR ', ') AS EmailList, GROUP_CONCAT(DISTINCT AP.ProgramFile ORDER BY AP.`Order` ASC SEPARATOR ',') AS ProgramFiles
                                FROM BatchSequenceProfile P
                                INNER JOIN BatchSequenceAlertPrograms AP ON P.BatchSequenceProfileId = AP.BatchSequenceProfileId
                                INNER JOIN BatchSequenceAlertRecipients AR ON AR.BatchSequenceProfileId = P.BatchSequenceProfileId
                                GROUP BY P.BatchSequenceProfileId";
            DataTable dataTable = new DataTable();
            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 0;
                    var adapter = new MySqlDataAdapter(command);
                    adapter.Fill(dataTable);
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return dataTable;
        }

        public DataTable GetBatchInformationByName(string batchSequenceName) {
            string cmdText = @"SELECT P.BatchSequenceProfileId, P.Name, P.StartWindowStart, P.StartWindowEnd, P.StartWindowDoW,  
                                P.ExpectedFinishBy, P.AlertIFDoesNotStartOnTime, P.AlertIfDoesNotFinishOnTime, P.AlertIfOrderNotFollowed, 
                                GROUP_CONCAT(DISTINCT AR.EmailAddress SEPARATOR ', ') AS EmailList, GROUP_CONCAT(DISTINCT AP.ProgramFile ORDER BY AP.`Order` ASC SEPARATOR ',') AS ProgramFiles
                                FROM BatchSequenceProfile P
                                INNER JOIN BatchSequenceAlertPrograms AP ON P.BatchSequenceProfileId = AP.BatchSequenceProfileId
                                INNER JOIN BatchSequenceAlertRecipients AR ON AR.BatchSequenceProfileId = P.BatchSequenceProfileId
                                WHERE `Name` = '" + batchSequenceName + "'";
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

        public int GetRDSRetentionDays(string systemSerial) {
            string cmdText = "SELECT `ExpertReportRetentionDay` FROM System_Tbl WHERE SystemSerial = " + systemSerial;
            int RDSRetentionDays = 0;

            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    connection.Open();
                    var reader = command.ExecuteReader();

                    if (reader.Read()) {
                        RDSRetentionDays = Convert.ToInt32(reader["ExpertReportRetentionDay"]);
                    }
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }

            return RDSRetentionDays;
        }

        public DataTable GetProcessesTrendInformationByBatchId(string query) {

            DataTable dataTable = new DataTable();
            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(query, connection);
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
        
        public void InsertBatchTrendData(string query) {
            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(query, connection);
                    command.CommandTimeout = 0;
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }

        }

        public string FindDatabaseName(string mysqlConnectionString) {
            string databaseName = "";
            string[] tempNames = mysqlConnectionString.Split(';');
            foreach (string s in tempNames) {
                if (s.ToUpper().Contains("DATABASE")) {
                    databaseName = s.Split('=')[1];
                }
            }
            return databaseName;
        }

        public bool MySqlTableExists(string databaseName, string tableName) {
            string cmdText = "SELECT COUNT(*) AS TableName FROM INFORMATION_SCHEMA.tables WHERE TABLE_SCHEMA = @DatabaseName AND table_name = @TableName";
            bool exists = true;
            try {
                using (var mySqlConnection = new MySqlConnection(_connectionString)) {
                    mySqlConnection.Open();
                    var cmd = new MySqlCommand(cmdText, mySqlConnection);
                    cmd.Parameters.AddWithValue("@DatabaseName", databaseName);
                    cmd.Parameters.AddWithValue("@TableName", tableName);
                    cmd.CommandTimeout = 0;
                    MySqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read()) {
                        if (Convert.ToInt16(reader["TableName"]).Equals(0)) {
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

        public void CreateBatchTables() {
            try {
                CreateBatchSequenceProfileTable();
                CreateBatchSequenceAlertProgramsTable();
                CreateBatchSequenceAlertRecipientsTable();
                CreateBatchSequenceTrendTable();
            } catch (Exception ex) {
                _log.Error("Error Creating batch tables");
                _log.Error("Error Message: " + ex.Message);
                _log.Error("Error: " + ex.StackTrace);
            }
        }

        internal void CreateBatchSequenceProfileTable() {
            string sqlStr = @"CREATE TABLE `BatchSequenceProfile` (
                              `BatchSequenceProfileId` int(11) NOT NULL AUTO_INCREMENT,
                              `Name` varchar(100) DEFAULT NULL,
                              `StartWindowStart` time DEFAULT NULL,
                              `StartWindowEnd` time DEFAULT NULL,
                              `StartWindowDoW` char(7) DEFAULT NULL,
                              `ExpectedFinishBy` time DEFAULT NULL,
                              `AlertIfDoesNotStartOnTime` tinyint(4) DEFAULT NULL,
                              `AlertIfOrderNotFollowed` tinyint(4) DEFAULT NULL,
                              `AlertIfDoesNotFinishOnTime` tinyint(4) DEFAULT NULL,
                              PRIMARY KEY (`BatchSequenceProfileId`),
                              UNIQUE KEY `Name_UNIQUE` (`Name`)
                            ) ENGINE=InnoDB AUTO_INCREMENT=17 DEFAULT CHARSET=latin1;";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(sqlStr, connection) { CommandTimeout = 0 };
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        internal void CreateBatchSequenceAlertProgramsTable() {
            string sqlStr = @"CREATE TABLE `BatchSequenceAlertPrograms` (
                              `BatchSequenceProfileId` int(11) NOT NULL,
                              `ProgramFile` varchar(40) NOT NULL,
                              `Order` int(11) DEFAULT NULL,
                              PRIMARY KEY (`BatchSequenceProfileId`,`ProgramFile`)
                            ) ENGINE=InnoDB DEFAULT CHARSET=latin1;";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(sqlStr, connection) { CommandTimeout = 0 };
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        internal void CreateBatchSequenceAlertRecipientsTable() {
            string sqlStr = @"CREATE TABLE `BatchSequenceAlertRecipients` (
                              `BatchSequenceProfileId` int(11) NOT NULL,
                              `EmailAddress` varchar(255) NOT NULL,
                              PRIMARY KEY (`BatchSequenceProfileId`,`EmailAddress`),
                              CONSTRAINT `BatchSequenceProfileId` FOREIGN KEY (`BatchSequenceProfileId`) REFERENCES `BatchSequenceProfile` (`BatchSequenceProfileId`) ON DELETE CASCADE ON UPDATE CASCADE
                            ) ENGINE=InnoDB DEFAULT CHARSET=latin1;";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(sqlStr, connection) { CommandTimeout = 0 };
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        internal void CreateBatchSequenceTrendTable() {
            string sqlStr = @"CREATE TABLE `BatchSequenceTrend` (
                              `BatchSequenceProfileId` int(11) NOT NULL,
                              `ProgramFile` varchar(40) NOT NULL,
                              `DataDate` datetime NOT NULL,
                              `StartTime` datetime DEFAULT NULL,
                              `EndTime` datetime DEFAULT NULL,
                              `Duration` int(11) DEFAULT NULL,
                              PRIMARY KEY (`BatchSequenceProfileId`,`ProgramFile`,`DataDate`),
                              CONSTRAINT `BatchSequenceTrend_BatchSequenceProfileId_ProgramFile` FOREIGN KEY (`BatchSequenceProfileId`, `ProgramFile`) REFERENCES `BatchSequenceAlertPrograms` (`BatchSequenceProfileId`, `ProgramFile`) ON DELETE CASCADE ON UPDATE CASCADE
                            ) ENGINE=InnoDB DEFAULT CHARSET=latin1;
";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(sqlStr, connection) { CommandTimeout = 0 };
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

    }
}
