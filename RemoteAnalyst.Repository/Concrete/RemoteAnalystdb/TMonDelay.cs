using System;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class TMonDelay {
        private readonly string _connectionString = "";

        public TMonDelay(string connectionString) {
            _connectionString = connectionString;
        }

        public void InsertDelayLog(string expectedTime, string systemSerial, DateTime delayTime, string fileName) {
            const string cmdText = "INSERT INTO TransMonDelay (ExpectedTime, SystemSerial, DelayTime, FileName) " +
                                   "VALUES (@ExpectedTime, @SystemSerial, @DelayTime, @FileName)";
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection) { CommandTimeout = 0 };
                command.Parameters.AddWithValue("@ExpectedTime", expectedTime);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@DelayTime", delayTime);
                command.Parameters.AddWithValue("@FileName", fileName);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}