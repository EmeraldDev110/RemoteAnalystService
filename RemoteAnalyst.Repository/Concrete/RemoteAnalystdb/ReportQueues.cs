using System;
using System.Data;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class ReportQueues {
        private readonly string _connectionString;

        public ReportQueues(string connectionString) {
            _connectionString = connectionString;
        }

        public void InsertNewQueue(string fileName, int typeID) {
            string ntsConnectionString = _connectionString;
            string cmdText = @"INSERT INTO ReportQueues (FileName ,TypeID ,Loading, OrderDate)
                           VALUES (@FileName ,@TypeID ,@Loading, @OrderDate)";

            using (var connection = new MySqlConnection(ntsConnectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@FileName", fileName);
                command.Parameters.AddWithValue("@TypeID", typeID);
                command.Parameters.AddWithValue("@Loading", 0);
                command.Parameters.AddWithValue("@OrderDate", DateTime.Now);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void RemoveQueue(int queueID) {
            string connectionString = _connectionString;
            string cmdText = "DELETE FROM ReportQueues WHERE QueueID = @QueueID";

            using (var connection = new MySqlConnection(connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@QueueID", queueID);
                connection.Open();

                command.ExecuteNonQuery();
            }
        }

    }
}