using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class UploadStatus {
        private readonly string _connectionString = "";

        public UploadStatus(string connectionString) {
            _connectionString = connectionString;
        }

        public int GetStatusId(int orderId) {
            var cmdText = "SELECT StatusId FROM UploadStatus WHERE OrderId = @OrderId";
            int statusId = 0;

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@OrderId", orderId);
                connection.Open();
                var reader = command.ExecuteReader();
                if (reader.Read()) {
                    statusId = Convert.ToInt32(reader["StatusId"]);
                }
            }
            return statusId;
        }
        public void DeleteEntry(int orderId) {
            var cmdText = "DELETE FROM UploadStatus WHERE OrderId = @OrderId";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@OrderId", orderId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}
