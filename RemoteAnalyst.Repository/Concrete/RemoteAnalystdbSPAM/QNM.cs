using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using log4net;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM {
    public class QNM {
        //Queries
        public static readonly string QNM_About = "CREATE TABLE `QNM_About` (`NODE` varchar(50) NOT NULL, `FROM` datetime(6) NOT NULL, `TO` datetime(6) NOT NULL, `INTERVAL` varchar(50) DEFAULT NULL, `VERSION` varchar(50) DEFAULT NULL, `DataVersion` varchar(45) DEFAULT NULL, PRIMARY KEY(`NODE`, `FROM` , `TO`)) ENGINE=InnoDB DEFAULT CHARSET=utf8;";

        public static readonly string QNM_CLIMDetail = "CREATE TABLE `QNM_CLIMDetail` (`Date Time` datetime(6) NOT NULL,`CLIM Name` varchar(50) NOT NULL,`Sent Bytes` double DEFAULT NULL,`Received Bytes` double DEFAULT NULL,`Errors` double DEFAULT NULL,`Reset` char(1) DEFAULT NULL, PRIMARY KEY (`Date Time`,`CLIM Name`)) ENGINE=InnoDB DEFAULT CHARSET=utf8;";

        public static readonly string QNM_ExpandPathDetail = "CREATE TABLE `QNM_ExpandPathDetail` ( `Date Time` datetime(6) NOT NULL, `Device Name` varchar(50) NOT NULL, `Sent Packets` double DEFAULT NULL, `Received Packets` double DEFAULT NULL, `Sent Forwards` double DEFAULT NULL, `Received Forwards` double DEFAULT NULL, `L4 Packets Discarded` double DEFAULT NULL, `Avg.Packets/Frame Sent` double DEFAULT NULL, `Avg.Bytes/Frame Sent` double DEFAULT NULL, `Avg.Packets/Frame Received` double DEFAULT NULL, `Avg.Bytes/Frame Received` double DEFAULT NULL, `Transmit Timeouts` double DEFAULT NULL, `Retransmit Timeouts` double DEFAULT NULL, `Retransmit Packets` double DEFAULT NULL, `OOS Usage` double DEFAULT NULL, `OOS Timeouts` double DEFAULT NULL, `L4 Sent Ack` double DEFAULT NULL, `L4 Sent NOT Ack` double DEFAULT NULL, `L4 Received Ack` double DEFAULT NULL, `L4 Received NOT Ack` double DEFAULT NULL, `L3 Misc Bad Packets` double DEFAULT NULL, `Reset` char(1) DEFAULT NULL, PRIMARY KEY (`Date Time`, `Device Name`)) ENGINE=InnoDB DEFAULT CHARSET=utf8;";

        public static readonly string QNM_ProbeRoundTripDetail = "CREATE TABLE `QNM_ProbeRoundTripDetail` ( `Date Time` datetime(6) NOT NULL, `Selected System` varchar(50) NOT NULL, `Number of Systems` double DEFAULT NULL, `List of Systems` varchar(500) DEFAULT NULL, `Trip Time (ms)` double DEFAULT NULL, PRIMARY KEY (`Date Time`, `Selected System`) ) ENGINE=InnoDB DEFAULT CHARSET=utf8;";

        public static readonly string QNM_SLSADetail = "CREATE TABLE `QNM_SLSADetail` ( `Date Time` datetime(6) NOT NULL, `PIF Name` varchar(50) NOT NULL, `Type` varchar(50) DEFAULT NULL, `Sent Octets` double DEFAULT NULL, `Received Octets` double DEFAULT NULL, `Sent Errors` double DEFAULT NULL, `Received Errors` double DEFAULT NULL, `Collision Frames` double DEFAULT NULL, `Reset` char(1) DEFAULT NULL, PRIMARY KEY (`Date Time`, `PIF Name`) ) ENGINE=InnoDB DEFAULT CHARSET=utf8;";

        public static readonly string QNM_SLSASummary = "CREATE TABLE `QNM_SLSASummary` ( `PIF Name` varchar(50) NOT NULL, `Line Speed Mbits/sec` double DEFAULT NULL, `Auto Negotiation` double DEFAULT NULL, `Duplex Mode` double DEFAULT NULL, PRIMARY KEY (`PIF Name`) ) ENGINE=InnoDB DEFAULT CHARSET=utf8;";

        public static readonly string QNM_SysDiagrams = "CREATE TABLE `QNM_SysDiagrams` ( `name` varchar(160) NOT NULL, `principal_id` int(11) NOT NULL, `diagram_id` int(11) NOT NULL AUTO_INCREMENT, `version` int(11) DEFAULT NULL, `definition` longblob, PRIMARY KEY (`diagram_id`), UNIQUE KEY `UK_principal_name` (`principal_id`,`name`) ) ENGINE=InnoDB DEFAULT CHARSET=utf8;";

        public static readonly string QNM_TCPPacketsDetail = "CREATE TABLE `QNM_TCPPacketsDetail` ( `Date Time` datetime(6) NOT NULL, `Process Name` varchar(50) NOT NULL, `TCP Sent Packets` double DEFAULT NULL, `TCP Received Packets` double DEFAULT NULL, `TCP Received Out of Order Packets` double DEFAULT NULL, `UDP Total Input Packets` double DEFAULT NULL, `UDP Total Output Packets` double DEFAULT NULL, `Reset` char(1) DEFAULT NULL, PRIMARY KEY (`Date Time`, `Process Name`) ) ENGINE=InnoDB DEFAULT CHARSET=utf8;";

        public static readonly string QNM_TCPProcessDetail = "CREATE TABLE `QNM_TCPProcessDetail` ( `Date Time` datetime(6) NOT NULL, `Process Name` varchar(50) NOT NULL, `Sent Bytes` double DEFAULT NULL, `Received Bytes` double DEFAULT NULL, `Received Out of Order bytes` double DEFAULT NULL, `Reset` char(1) DEFAULT NULL, PRIMARY KEY (`Date Time`, `Process Name`) ) ENGINE=InnoDB DEFAULT CHARSET=utf8;";

        public static readonly string QNM_TCPSubnetDetail = "CREATE TABLE `QNM_TCPSubnetDetail` ( `Date Time` datetime(6) NOT NULL, `Subnet Process Name (IP Address)` varchar(100) NOT NULL, `Out Packets` double DEFAULT NULL, `In Packets` double DEFAULT NULL, `Reset` char(1) DEFAULT NULL, PRIMARY KEY ( `Date Time`, `Subnet Process Name (IP Address)`) ) ENGINE=InnoDB DEFAULT CHARSET=utf8;";

        public static readonly string QNM_TCPv6Detail = "CREATE TABLE `QNM_TCPv6Detail` ( `Date Time` datetime(6) NOT NULL, `Monitor Name` varchar(50) NOT NULL, `Sent Bytes` double DEFAULT NULL, `Received Bytes` double DEFAULT NULL, `Received Out of Order bytes` double DEFAULT NULL, `Reset` char(1) DEFAULT NULL, PRIMARY KEY (`Date Time`, `Monitor Name`) ) ENGINE=InnoDB DEFAULT CHARSET=utf8;";

        public static readonly string QNM_TCPv6SubnetDetail = "CREATE TABLE `QNM_TCPv6SubnetDetail` ( `Date Time` datetime(6) NOT NULL, `SUBNET Monitor Name (IP Address)` varchar(50) NOT NULL, `Out Packets` double DEFAULT NULL, `In Packets` double DEFAULT NULL, `Reset` char(1) DEFAULT NULL, PRIMARY KEY ( `Date Time`, `SUBNET Monitor Name (IP Address)`) ) ENGINE=InnoDB DEFAULT CHARSET=utf8;";
        
        public static readonly string QNM_CLIMCPUDetail = "CREATE TABLE `QNM_CLIMCPUDetail` (  `Date Time` DATETIME NOT NULL,  `CLIM Name` VARCHAR(50) NOT NULL,  `CPU Busy` DOUBLE NULL,  `Memory Free` DOUBLE NULL,  PRIMARY KEY (`Date Time`, `CLIM Name`));";
        public static readonly string QNM_CLIMDiskDetail = "CREATE TABLE `QNM_CLIMDiskDetail` (  `Date Time` DATETIME NOT NULL,  `CLIM Name` VARCHAR(50) NOT NULL,  `Size` DOUBLE NULL,  `Used` DOUBLE NULL,  PRIMARY KEY (`Date Time`, `CLIM Name`));";


        private readonly string _connectionString;
        private readonly string _path;

        public QNM(string path, string connectionString) {
            _path = path;
            _connectionString = connectionString;
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

        public List<string> GetAllUniquePIFNames() {
            string cmdText = @"SELECT DISTINCT `PIF Name` FROM QNM_SLSASummary";
            var names = new List<string>();
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read()) {
                    names.Add(reader["PIF Name"].ToString());
                }
            }
            return names;
        }

        public bool InsertData(string tableName, DataTable table, ILog log) {
            string pathToCsv = _path + @"BulkInsert_" + DateTime.Now.Ticks + ".csv";
            var sb = new StringBuilder();
            if (table.Rows.Count > 0) {
                foreach (DataRow dataRow in table.Rows) {
                    var row = new StringBuilder();
                    for (int x = 0; x < table.Columns.Count; x++) {
                        if (table.Columns[x].DataType.Name.Equals("DateTime")) {
                            DateTime tempDate = Convert.ToDateTime(dataRow[x]);
                            row.Append(tempDate.ToString("yyyy-MM-dd HH-mm-ss") + "|");
                        }
                        else {
                            var longValue = 0D;
                            if (double.TryParse(dataRow[x].ToString(), out longValue)) {
                                if (longValue > 4294967296)
                                    row.Append("0|");
                                else
                                    row.Append(dataRow[x] + "|");
                            }
                            else
                                row.Append(dataRow[x] + "|");
                        }
                    }
                    row = row.Remove(row.Length - 1, 1);
                    sb.Append(row + Environment.NewLine);
                }
            }
            File.AppendAllText(pathToCsv, sb.ToString());

            int inserted = -1;
            try
            {
                /*
                 * Explicitly added Local=true to allow Bulk loading
                 * https://dev.mysql.com/doc/connector-net/en/connector-net-programming-bulk-loader.html
                 */
                using (var mySqlConnection = new MySqlConnection(_connectionString)) {
                    mySqlConnection.Open();
                    var bl = new MySqlBulkLoader(mySqlConnection) {
                        Local = true,
                        TableName = tableName,
                        FieldTerminator = "|",
                        LineTerminator = "\r\n",
                        FileName = pathToCsv,
                        NumberOfLinesToSkip = 0
                    };
                    inserted = bl.Load();
                }
            }
            catch (Exception ex) {
                log.ErrorFormat("Exception insert {0}: {1}", tableName, ex.Message);
                throw new Exception();
            }

            if (inserted >= 0) {
                File.Delete(pathToCsv);
            }
            return inserted >= 0;
        }

        public void InsertAbout(DataTable dsData, string dataVersion) {
            string cmdText = "INSERT INTO `QNM_About` (`NODE`, `FROM`, `TO`, `INTERVAL`, `VERSION`, `DataVersion`) VALUES (@NODE, @FROM, @TO, @INTERVAL, @VERSION, @DataVersion)";
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@NODE", dsData.Rows[0][1].ToString());
                command.Parameters.AddWithValue("@FROM", dsData.Rows[1][1].ToString());
                command.Parameters.AddWithValue("@TO", dsData.Rows[2][1].ToString());
                command.Parameters.AddWithValue("@INTERVAL", dsData.Rows[3][1].ToString());
                command.Parameters.AddWithValue("@VERSION", dsData.Rows[4][1].ToString());
                command.Parameters.AddWithValue("@DataVersion", dataVersion);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public bool CheckAboutExists(DataTable dsData) {
            string cmdText = "SELECT NODE FROM `QNM_About` WHERE `NODE` = @NODE AND `FROM` = @FROM AND `TO` = @TO";
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@NODE", dsData.Rows[0][1].ToString());
                command.Parameters.AddWithValue("@FROM", dsData.Rows[1][1].ToString());
                command.Parameters.AddWithValue("@TO", dsData.Rows[2][1].ToString());
                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read()) {
                    return true;
                }
            }
            return false;
        }

        public bool CheckDetailsExists(string tableName, DateTime from, DateTime to) {
            string cmdText = "SELECT EXISTS (SELECT 1 FROM `" + tableName + "`" + " HAVING MIN(`Date Time`) <= @From AND MAX(`Date Time`) >= @To) AS EXIST";
            int exist = 0;
            using (var mySqlConnection = new MySqlConnection(_connectionString)) {
                mySqlConnection.Open();
                var cmd = new MySqlCommand(cmdText, mySqlConnection);
                // cmd.prepare();
                cmd.Parameters.AddWithValue("@From", from);
                cmd.Parameters.AddWithValue("@To", to);
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read()) {
                    exist = int.Parse(Convert.ToInt16(reader["Exist"]).ToString());
                }
            }
            return exist == 1;
        }

        public bool CheckIndexExists(string tableName) {
            string cmdText = "SHOW INDEX FROM " + tableName;
            using (var mySqlConnection = new MySqlConnection(_connectionString)) {
                mySqlConnection.Open();
                var cmd = new MySqlCommand(cmdText, mySqlConnection);
                // cmd.prepare();
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read()) {
                    return true;
                }
            }
            return false;
        }

        public void CreateTable(string cmdText) {
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                connection.Open();
                command.CommandTimeout = 0;
                command.ExecuteNonQuery();
            }
        }

        public bool CreateIndex(string tableName, string indexName, List<string> indexCols, ILog log) {
            try {
                string cmdText = @"ALTER TABLE `" + tableName + @"`
                                ADD INDEX `";
                cmdText += indexName + "` (";

                foreach (string indexCol in indexCols) {
                    cmdText += "`" + indexCol + "` ASC,";
                }

                cmdText = cmdText.Remove(cmdText.Length - 1);
                cmdText += ");";
                CreateTable(cmdText);
            }
            catch (Exception ex){
                log.ErrorFormat("Exception add index to {0}: {1}", tableName, ex.Message);
                return false;
            }
            return true;
        }

        public List<DateTime> GetDeleteDates(DateTime retentionDate) {
            string cmdText = @"SELECT `FROM` FROM QNM_About WHERE `FROM` < @RetentionDate";
            var deleteList = new List<DateTime>();
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@RetentionDate", retentionDate);

                connection.Open();
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read()) {
                    deleteList.Add(Convert.ToDateTime(reader["FROM"]));
                }
            }
            return deleteList;
        }
    }
}