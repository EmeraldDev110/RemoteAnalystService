using System;
using System.Data;
using System.IO;
using System.Text;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class TMonTomorrow {
        private readonly string _connectionString = "";

        public TMonTomorrow(string connectionString) {
            _connectionString = connectionString;
        }

        public void PopulateTMonTomorrow(DataTable table, string path) {
            /*using (var bulkCopy = new SqlBulkCopy(_connectionString)) {
                bulkCopy.BulkCopyTimeout = 600;
                bulkCopy.DestinationTableName = "TMonTomorrow";
                bulkCopy.WriteToServer(table);
            }*/
            string pathToCsv = path + @"\BulkInsert_" + DateTime.Now.Ticks + ".csv";
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
                        TableName = "TMonTomorrow",
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

        public DataTable GetExpectedTime() {
            var expectedTime = new DataTable();
            try {
                const string cmdText = "SELECT ExpectedTime, SystemSerial FROM TMonTomorrow LIMIT 1";
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    var adapter = new MySqlDataAdapter(command);
                    adapter.Fill(expectedTime);
                }
            }
            catch (Exception ex) {
            }
            return expectedTime;
        }

        public void DeleteExpectedTime(DateTime expectedTime, string systemSerial) {
            const string cmdText = "DELETE FROM TMonTomorrow WHERE ExpectedTime = @ExpectedTime AND SystemSerial = @SystemSerial";
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@ExpectedTime", expectedTime);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void DeleteJobsTomorrow() {
            const string cmdText = "DELETE FROM TMonTomorrow";
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}