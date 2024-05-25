using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using MySqlConnector;
using RemoteAnalyst.Repository.Infrastructure;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM
{
    public class DataTables
    {
        private readonly string _connectionString;

        public DataTables(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDictionary<string, DateTime> GetCreatedDate(string systemSerial, string systemConnectionString, string databaseName)
        {
            IDictionary<string, DateTime> tables = new Dictionary<string, DateTime>();

            string cmdText = @"SELECT table_name as name,  STR_TO_DATE(TABLE_COMMENT, '%Y-%m-%d %H:%i:%s') as crdate 
										FROM INFORMATION_SCHEMA.TABLES
										WHERE table_name LIKE '%" + systemSerial + @"%'  
										AND TABLE_SCHEMA = @DatabaseNames 
										AND NOT (table_name LIKE '%" + systemSerial + @"_DISKBROWSER%') 
										AND NOT (table_name LIKE '%" + systemSerial + @"_FILETREND%') 
										AND NOT (table_name LIKE '%" + systemSerial + @"_OSS_Names%') 
										ORDER BY STR_TO_DATE(TABLE_COMMENT, '%Y-%m-%d %H:%i:%s');";
            try {
                using (var connection = new MySqlConnection(systemConnectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@DatabaseNames", databaseName);
                    connection.Open();
                    var reader = command.ExecuteReader();

                    while (reader.Read()) {
                        if (!tables.ContainsKey(reader["name"].ToString())) {
                            tables.Add(reader["name"].ToString(), Convert.ToDateTime(reader["crdate"]));
                        }
                    }
                }
            }
            catch (Exception ex) {
                tables = FakeGetCreateDate(systemSerial, systemConnectionString, databaseName);
            }

            return tables;
        }

        private IDictionary<string, DateTime> FakeGetCreateDate(string systemSerial, string systemConnectionString, string databaseName)
        {
            IDictionary<string, DateTime> tables = new Dictionary<string, DateTime>();

            string cmdText = @"SELECT table_name as name FROM INFORMATION_SCHEMA.TABLES
                              WHERE table_name LIKE '%" + systemSerial + "%'  AND TABLE_SCHEMA = @DatabaseNames " +
							  "AND (table_name NOT LIKE '%" + systemSerial + "_CPU%') ORDER BY create_time";

            try
            {
                using (var connection = new MySqlConnection(systemConnectionString))
                {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@DatabaseNames", databaseName);
                    connection.Open();
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        if (!tables.ContainsKey(reader["name"].ToString()))
                        {
                            String tableName = reader["name"].ToString();
                            String[] elements = tableName.Split('_');
                            if(elements != null && elements.Length == 5)
                            {
                                DateTime creationDate = DateTime.ParseExact(
                                                                elements[2] + "-" + elements[3] + "-" + elements[4], 
                                                                "yyyy-M-d", 
                                                                CultureInfo.InvariantCulture
                                                            );
                                tables.Add(tableName, creationDate);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                
            }
            return tables;
        }

        public void DropTable(string tableName)
        {
            string cmdText = @"DROP TABLE " + tableName;
            //Drop table.
            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception) {
                //If table does not exist, skip it
            }
        }

        public void InsertEntityData(string tableName, DataTable dsData, string path)
        {
            try
            {
                string pathToCsv = path + @"\BulkInsert_" + DateTime.Now.Ticks + ".csv";
                var sb = new StringBuilder();
                if (dsData.Rows.Count > 0) {
                    foreach (DataRow dataRow in dsData.Rows) {
                        var row = new StringBuilder();
                        for (int x = 0; x < dsData.Columns.Count; x++) {
                            if (dsData.Columns[x].DataType.Name.Equals("DateTime")) {
                                DateTime tempDate = Convert.ToDateTime(dataRow[x]);
                                row.Append(tempDate.ToString("yyyy-MM-dd HH-mm-ss") + "|");
                            }
                            else {
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
                        var bl = new MySqlBulkLoader(mySqlConnection)
                        {
                            Local = true,
                            TableName = tableName,
                            FieldTerminator = "|",
                            LineTerminator = "\r\n",
                            FileName = pathToCsv,
                            NumberOfLinesToSkip = 0,
                            ConflictOption = MySqlBulkLoaderConflictOption.Replace
                        };
                        inserted = bl.Load();
                    }
                }
                catch (Exception ex) {
                    throw new Exception();
                }

                if (inserted >= 0) {
                    File.Delete(pathToCsv);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Table " + tableName + " :" + ex.Message);
            }
        }

        public void InsertForecastData(string tableName, string pathToCsv) {
            int inserted = -1;
            try
            {
                /*
                 * Explicitly added Local=true to allow Bulk loading
                 * https://dev.mysql.com/doc/connector-net/en/connector-net-programming-bulk-loader.html
                 */
                using (var mySqlConnection = new MySqlConnection(_connectionString)) {
                    mySqlConnection.Open();
                    var bl = new MySqlBulkLoader(mySqlConnection)
                    {
                        Local = true,
                        TableName = tableName,
                        FieldTerminator = "|",
                        LineTerminator = "\r\n",
                        FileName = pathToCsv,
                        NumberOfLinesToSkip = 0,
                        ConflictOption = MySqlBulkLoaderConflictOption.Replace
                    };
                    inserted = bl.Load();
                }
            }
            catch (Exception ex) {
                throw new Exception();
            }

            if (inserted >= 0) {
                File.Delete(pathToCsv);
            }
        }
        public void InsertSPAMEntityData(string tableName, DataTable dsData, DateTime selectedStartTime, DateTime selectedStopTime, string path)
        {
            try
            {
                /*//string connectionString = Config.ConnectionString;
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    var bulkCopy = new SqlBulkCopy(connection);
                    //no time out.
                    bulkCopy.BulkCopyTimeout = 0;
                    //Set batch size. 0 = bigest system can give.
                    bulkCopy.BatchSize = 0;
                    if (dsData.Rows.Count > 0) {
                        DataRow r = dsData.Rows[0];
                        //Add column mapping for every column in the dataset
                        foreach (DataColumn c in r.Table.Columns) {
                            if (c.ColumnName == "[Group]" || c.ColumnName == "[User]")
                                //Special case for columns with []
                                bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("[" + c.ColumnName + "]",
                                    c.ColumnName));
                            else
                                bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(c.ColumnName, c.ColumnName));
                        }

                        bulkCopy.DestinationTableName = "[" + tableName + "]";
                        bulkCopy.WriteToServer(dsData);
                    }
                    //bulkCopy.DestinationTableName = "[" + tableName + "]";
                    //bulkCopy.WriteToServer(dsData);
                }*/
                string pathToCsv = path + @"\BulkInsert_" + DateTime.Now.Ticks + ".csv";
                var sb = new StringBuilder();
                if (dsData.Rows.Count > 0) {
                    foreach (DataRow dataRow in dsData.Rows) {
                        var row = new StringBuilder();
                        for (int x = 0; x < dsData.Columns.Count; x++) {
                            if (dsData.Columns[x].DataType.Name.Equals("DateTime")) {
                                DateTime tempDate = Convert.ToDateTime(dataRow[x]);
                                row.Append(tempDate.ToString("yyyy-MM-dd HH-mm-ss") + "|");
                            }
                            else {
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
                    throw new Exception();
                }

                if (inserted >= 0) {
                    File.Delete(pathToCsv);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void DeleteTempTable(string tableName)
        {
            //string connectionString = Config.ConnectionString;
            string cmdText = "DROP TABLE " + tableName;

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    var command = new MySqlCommand(cmdText, connection);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void CreateFileIndex(string fileTableName)
        {
            /*string fileCmdText2 = @"IF NOT EXISTS (SELECT name FROM sysindexes WHERE name = 'IX_" + fileTableName +
                                  @"_2')
	                                CREATE  INDEX [IX_" + fileTableName + @"_2] ON
	                                [dbo].[" + fileTableName + @"]([OpenerCpu], [OpenerPin],
	                                [OpenerProcessName], [OpenerVolume], [OpenerSubVol],
	                                [OpenerFileName], [OpenerDeviceName], [OpenerOsspid],
	                                [OpenerPathID], [OpenerCrvsn])";*/
            string fileCmdText2 = @"ALTER TABLE `" + fileTableName + @"` 
                                    ADD INDEX `IX_" + fileTableName + @"_2` (`OpenerCpu` ASC, `OpenerPin` ASC, `OpenerProcessName` ASC,
                                    `OpenerVolume` ASC,`OpenerSubVol` ASC,`OpenerFileName` ASC,`OpenerDeviceName` ASC,
                                    `OpenerOsspid` ASC,`OpenerPathID` ASC,`OpenerCrvsn` ASC);";

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    var command = new MySqlCommand(fileCmdText2, connection);
                    connection.Open();
                    command.CommandTimeout = 0;

                    command.ExecuteNonQuery();
                }
            }
            catch
            {
            }
        }

        public void RunCommand(string cmdText) {
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                connection.Open();
                command.CommandTimeout = 0;

                command.ExecuteNonQuery();
            }
        }

    }
}