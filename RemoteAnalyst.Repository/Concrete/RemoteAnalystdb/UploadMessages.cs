using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class UploadMessages {
        private readonly string _connectionString = "";

        public UploadMessages(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void InsertNewEntry(int uploadID, DateTime timeStamp, int source, string message) {
               string cmdText = "INSERT INTO UploadMessages (UploadID, TimeStamp, Source, Message) VALUES " +
                                "(@UploadID, @TimeStamp, @Source, @Message)";

            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@UploadID", uploadID);
                    command.Parameters.AddWithValue("@TimeStamp", timeStamp);
                    command.Parameters.AddWithValue("@Source", source);
                    command.Parameters.AddWithValue("@Message", message);

                    connection.Open();
                    command.ExecuteReader();
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public int CheckMessageCount(int uploadID, string message) {
            string cmdText = @"SELECT COUNT(UploadID) AS MessageCount FROM UploadMessages
                                WHERE Message LIKE '" + message + "%' AND UploadID = @UploadID";

            int messageCount = 0;
            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@UploadID", uploadID);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                        messageCount = Convert.ToInt32(reader["MessageCount"]);
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }

            return messageCount;
        }
    }
}
