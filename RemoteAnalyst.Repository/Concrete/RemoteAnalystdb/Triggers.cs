using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class Triggers {
        private readonly string _connectionString = "";

        public Triggers(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Insert(string systemSerial, int triggerType, string message) {
            string cmdText = @"INSERT INTO Triggers (TriggerType, SystemSerial, Message) VALUES
                                (@TriggerType, @SystemSerial, @Message)";
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@TriggerType", triggerType);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@Message", message);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void Insert(string systemSerial, int triggerType, string fileType, string fileLocation, int uploadId, int uwsId) {
            string cmdText = @"INSERT INTO Triggers (TriggerType, SystemSerial, FileType, FileLocation, UploadId, InsertDate, UWSID) VALUES
                                (@TriggerType, @SystemSerial, @FileType, @FileLocation, @UploadId, @InsertDate, @UWSID)";
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@TriggerType", triggerType);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@FileType", fileType);
                command.Parameters.AddWithValue("@FileLocation", fileLocation);
                command.Parameters.AddWithValue("@UploadId", uploadId);
                command.Parameters.AddWithValue("@InsertDate", DateTime.Now);
                command.Parameters.AddWithValue("@UWSID", uwsId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void DeleteTriiger(int triggerId) {
            string cmdText = @"DELETE FROM Triggers WHERE TriggerId = @TriggerId";
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@TriggerId", triggerId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public DataTable GetTrigger(int triggerType) {
            string cmdText = @"SELECT TriggerId, SystemSerial, FileType, FileLocation, UploadId, Message, CustomerId, InsertDate, UWSID
                            FROM Triggers WHERE TriggerType = @TriggerType ORDER BY InsertDate LIMIT 1";
            var triggerView = new DataTable();
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@TriggerType", triggerType);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(triggerView);
            }

            return triggerView;
        }
    }
}
