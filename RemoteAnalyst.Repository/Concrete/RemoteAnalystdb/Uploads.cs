using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class Uploads {
        private readonly string _connectionString = "";

        public Uploads(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int GetCustomerID(int uploadID) {
            var cmdText = "SELECT CustomerID FROM Uploads WHERE UploadID = @UploadID";
            int customerID = 0;

            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.Parameters.AddWithValue("@UploadID", uploadID);
                connection.Open();
                var reader = command.ExecuteReader();
                if (reader.Read()) {
                    customerID = Convert.ToInt32(reader["CustomerID"]);
                }
            }
            return customerID;
        }

        public void UpdateLoadedDate(int uploadID) {
            string cmdText = @"UPDATE Uploads SET LoadedDate = @LoadedDate
                             WHERE UploadID = @UploadID";

            var connection = new MySqlConnection(_connectionString);
            var command = new MySqlCommand(cmdText, connection);
            command.Parameters.AddWithValue("@UploadID", uploadID);
            command.Parameters.AddWithValue("@LoadedDate", DateTime.Now);

            try {
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch {
            }
            finally {
                connection.Close();
            }
        }

        public void UpdateLoadedStatus(int uploadID, string status) {
            string cmdText = @"UPDATE Uploads SET Status = @Status
                             WHERE UploadID = @UploadID";

            var connection = new MySqlConnection(_connectionString);
            var command = new MySqlCommand(cmdText, connection);
            command.Parameters.AddWithValue("@UploadID", uploadID);
            command.Parameters.AddWithValue("@Status", status);

            try {
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch {
            }
            finally {
                connection.Close();
            }
        }

        public void UploadCollectionStartTime(int uploadID, DateTime collectionStartTime) {
            string cmdText = @"UPDATE Uploads SET CollectionStartTime = @CollectionStartTime
                             WHERE UploadID = @UploadID";

            var connection = new MySqlConnection(_connectionString);
            var command = new MySqlCommand(cmdText, connection);
            command.Parameters.AddWithValue("@UploadID", uploadID);
            command.Parameters.AddWithValue("@CollectionStartTime", collectionStartTime);

            try {
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch {
            }
            finally {
                connection.Close();
            }
        }

        public void UploadCollectionToTime(int uploadID, DateTime collectionToTime) {
            string cmdText = @"UPDATE Uploads SET CollectionToTime = @CollectionTotTime
                             WHERE UploadID = @UploadID";

            var connection = new MySqlConnection(_connectionString);
            var command = new MySqlCommand(cmdText, connection);
            command.Parameters.AddWithValue("@UploadID", uploadID);
            command.Parameters.AddWithValue("@CollectionTotTime", collectionToTime);

            try {
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch {
            }
            finally {
                connection.Close();
            }
        }
    }
}
