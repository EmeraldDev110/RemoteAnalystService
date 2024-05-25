using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class UploadFileNames {
        private readonly string _connectionString = "";

        public UploadFileNames(string connectionString) {
            _connectionString = connectionString;
        }

        public int GetOrderId(string fileName) {
            var cmdText = "SELECT OrderId FROM UploadFileNames WHERE FileName = @FileName";
            int orderId = 0;

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@FileName", fileName);
                connection.Open();
                var reader = command.ExecuteReader();
                if (reader.Read()) {
                    orderId = Convert.ToInt32(reader["OrderId"]);
                }
            }
            return orderId;
        }

        public Dictionary<string, bool> CheckLoaded(int orderId) {
            var cmdText = "SELECT FileName, Loaded FROM UploadFileNames WHERE OrderId = @OrderId";
            var loadedDictionary = new Dictionary<string, bool>();

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@OrderId", orderId);
                connection.Open();
                var reader = command.ExecuteReader();
                while (reader.Read()) {
                    loadedDictionary.Add(reader["FileName"].ToString(), Convert.ToBoolean(reader["Loaded"]));
                }
            }

            return loadedDictionary;
        }
        public void UpdateLoadStatus(string fileName) {
            var cmdText = "UPDATE UploadFileNames SET Loaded = 1 WHERE FileName = @FileName";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@FileName", fileName);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void DeleteEntries(int orderId) {
            var cmdText = "DELETE FROM UploadFileNames WHERE OrderId = @OrderId";

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@OrderId", orderId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}
