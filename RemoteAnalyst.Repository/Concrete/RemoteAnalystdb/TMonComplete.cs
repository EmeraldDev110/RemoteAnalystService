using System;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class TMonComplete {
        private readonly string _connectionString = "";

        public TMonComplete(string connectionString) {
            _connectionString = connectionString;
        }

        public void InsertCompleteLog(string expectedTime, string systemSerial, DateTime finishedTime, string fileName) {
            const string cmdText = "INSERT INTO TransMonComplete (ExpectedTime, SystemSerial, FinishedTime, FileName) " +
                                   "VALUES (@ExpectedTime, @SystemSerial, @FinishedTime, @FileName)";
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection) { CommandTimeout = 0 };
                command.Parameters.AddWithValue("@ExpectedTime", expectedTime);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@FinishedTime", finishedTime);
                command.Parameters.AddWithValue("@FileName", fileName);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}