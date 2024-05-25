using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using System.Linq;
using System.Text;
using System.IO;

namespace RemoteAnalyst.UWSLoader.Core.Repository {
    class DataTables {
        private readonly string _connectionString;

        public DataTables(string connectionString) {
            _connectionString = connectionString;
        }

        public IDictionary<string, DateTime> GetCreatedDate(string systemSerial, string systemConnectionString) {
            IDictionary<string, DateTime> tables = new Dictionary<string, DateTime>();

            string cmdText = "SELECT name, crdate  FROM sysobjects " +
                             "WHERE name LIKE '%" + systemSerial + "%' " +
                             "AND xtype = 'U' ORDER BY crdate";

            try {
                using (var connection = new MySqlConnection(systemConnectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    connection.Open();
                    var reader = command.ExecuteReader();

                    while (reader.Read()) {
                        if (!tables.ContainsKey(reader["name"].ToString())) {
                            tables.Add(reader["name"].ToString(), Convert.ToDateTime(reader["crdate"]));
                        }
                    }
                }
            }
            catch (Exception) {
            }

            return tables;
        }

        public DataTable GetAllIndexes(string systemSerial, DateTime startDateTime, DateTime toDateTime) {
            string cmdText = @"SELECT 
                                t.name AS TableName, 
                                ind.name AS IndexName, 
                                create_date AS CreateDate
                                FROM sys.indexes ind 
                                INNER JOIN sys.tables t ON ind.object_id = t.object_id 
                                WHERE 
                                     ind.is_primary_key = 0 
                                     AND ind.is_unique = 0 
                                     AND ind.is_unique_constraint = 0 
                                     AND t.is_ms_shipped = 0 
	                                 AND ind.name IS NOT NULL
	                                 AND t.name LIKE '" + systemSerial + @"_%'
  	                                 AND create_date >= @StartTime
	                                 AND create_date <= @StopTime
                                GROUP BY t.name, ind.name, create_date
                                ORDER BY t.name, ind.name, create_date";
            var systemData = new DataTable();

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@StartTime", startDateTime);
                command.Parameters.AddWithValue("@StopTime", toDateTime);

                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(systemData);
            }

            return systemData;
        }

        public void RebuildIndex(string tableName, string indexName) {
            string cmdText = @"ALTER INDEX [" + indexName + @"]
                                ON [" + tableName + @"]
                                REBUILD WITH (FILLFACTOR = 100)";

            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection) { CommandTimeout = 0 };
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception) {
                //If table does not exist, skip it
            }
        }

        public void DropTable(string tableName) {
            string cmdText = @"DROP TABLE [" + tableName + "]";
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

        public bool CheckDatabase(string databaseName) {
            //string connectionString = Config.ConnectionString;
            //string cmdText = "SELECT name FROM SYS.DATABASES WHERE name = @DatabaseName";
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

        public bool CheckTable(string tableName) {
            //string connectionString = Config.ConnectionString;
            string cmdText = "SELECT * FROM " + tableName + " LIMIT 1";
            bool exists = false;

            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);

                    connection.Open();
                    command.ExecuteNonQuery();

                    //executed meaning table exist.
                    exists = true;
                }
            }
            catch {
                exists = false;
            }

            return exists;
        }

        public void InsertEntityData(string tableName, DataTable dsData, string path) {
            /*try {
                //string connectionString = Config.RAConnectionString;
                using (var connection = new MySqlConnection(_connectionString)) {
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
                }
            }
            catch (Exception ex) {
                throw new Exception("Table " + tableName + " :" + ex.Message);
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

        public void InsertSPAMEntityData(string tableName, DataTable dsData, DateTime selectedStartTime, DateTime selectedStopTime, string path) {
            /*try {
                //string connectionString = Config.ConnectionString;
                using (var connection = new MySqlConnection(_connectionString)) {
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
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
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

        public void DeleteTempTable(string tableName) {
            //string connectionString = Config.ConnectionString;
            string cmdText = "DROP TABLE " + tableName;

            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void CreateFileIndex(string fileTableName) {
            string fileCmdText2 = @"IF NOT EXISTS (SELECT name FROM sysindexes WHERE name = 'IX_" + fileTableName +
                                  @"_2')
	                                CREATE  INDEX [IX_" + fileTableName + @"_2] ON
	                                [dbo].[" + fileTableName + @"]([OpenerCpu], [OpenerPin],
	                                [OpenerProcessName], [OpenerVolume], [OpenerSubVol],
	                                [OpenerFileName], [OpenerDeviceName], [OpenerOsspid],
	                                [OpenerPathID], [OpenerCrvsn])";
            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(fileCmdText2, connection);
                    connection.Open();
                    command.CommandTimeout = 0;

                    command.ExecuteNonQuery();
                }
            }
            catch {
            }
        }

        public void InsertSPAMEntityData(string tableName, DataTable dsData, string path) {
            /*try {
                //string connectionString = Config.ConnectionString;
                using (var connection = new MySqlConnection(_connectionString)) {
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
                            if (c.ColumnName == "[Group]" || c.ColumnName == "[User]" || c.ColumnName == "[Index]")
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
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
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
    }
}
