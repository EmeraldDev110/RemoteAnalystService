using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class TransmonLogs {
        private readonly string _connectionString = "";

        public TransmonLogs(string connectionString) {
            _connectionString = connectionString;
        }

		public DataTable GetSystemsResidual(DateTime residualTime) {
			string cmdText = "SELECT * FROM TransmonLogs WHERE `LogTime` = @ResidualTime;";
			DataTable systemsResidual = new DataTable();
			try {
				using (var connection = new MySqlConnection(_connectionString)) {
					var command = new MySqlCommand(cmdText, connection) { CommandTimeout = 0 };
					command.Parameters.AddWithValue("@ResidualTime", residualTime);
					var adapter = new MySqlDataAdapter(command);
					adapter.Fill(systemsResidual);
				}
			}
			catch (Exception ex) {
				throw new Exception();
			}
			return systemsResidual;
		}

        public int Insert(string csvPath) {
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
                        TableName = "TransmonLogs",
                        FieldTerminator = "|",
                        LineTerminator = "\r\n",
                        FileName = csvPath,
                        NumberOfLinesToSkip = 0,
                        ConflictOption = MySqlBulkLoaderConflictOption.Replace
                    };
                    inserted = bl.Load();
                }
            }
            catch (Exception ex) {
                throw new Exception();
            }

            return inserted;
        }
    }
}
