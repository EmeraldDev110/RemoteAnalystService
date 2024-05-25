using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM {
    public class DailyProcessDatas {
        private readonly string _connectionString = "";

        public DailyProcessDatas(string connectionString) {
            _connectionString = connectionString;
        }
        public bool CheckTableName(string databaseName) {
            string cmdText = @"SELECT COUNT(*) AS TableName
                            FROM information_schema.tables 
                            WHERE table_name = 'DailyProcessDatas'
                            AND Table_Schema = @DatabaseName";
            bool tableExists = true;
            using (var mySqlConnection = new MySqlConnection(_connectionString)) {
                mySqlConnection.Open();
                var cmd = new MySqlCommand(cmdText, mySqlConnection);
                // cmd.prepare();
                cmd.Parameters.AddWithValue("@DatabaseName", databaseName);
                var reader = cmd.ExecuteReader();

                if (reader.Read()) {
                    if (Convert.ToInt16(reader["TableName"]).Equals(0)) {
                        tableExists = false;
                    }
                }
            }

            return tableExists;
        }
        public void CreateDailyCPUDatas() {
            var cmdText = @"CREATE TABLE `DailyProcessDatas` (
                          `DateTime` DATETIME NOT NULL,
                          `ProcessBusy` DOUBLE NULL,
                          `ProcessQueue` DOUBLE NULL,
                          `TransactionCompleted` DOUBLE NULL,
                          `TransactionAbort` DOUBLE NULL,
                          `DiskQueueLength` DOUBLE NULL,
                          PRIMARY KEY (`DateTime`));";

            using (var mySqlConnection = new MySqlConnection(_connectionString)) {
                mySqlConnection.Open();
                var cmd = new MySqlCommand(cmdText, mySqlConnection);
                // cmd.prepare();
                cmd.ExecuteNonQuery();
            }
        }

        public DataTable GetPastData(DateTime startDateTime, DateTime stopDateTime) {
            string cmdText = @"SELECT DateTime, DiskQueueLength, ProcessBusy, ProcessQueue, TransactionCompleted, TransactionAbort FROM DailyProcessDatas
                                WHERE DateTime >= @StartDateTime AND DateTime < @StopDateTime
                                ORDER BY  DateTime";

            var processData = new DataTable();
            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@StartDateTime", startDateTime);
                    command.Parameters.AddWithValue("@StopDateTime", stopDateTime);
                    var adapter = new MySqlDataAdapter(command);
                    adapter.Fill(processData);
                }
            }
            catch {
            }
            return processData;
        }
    }
}
