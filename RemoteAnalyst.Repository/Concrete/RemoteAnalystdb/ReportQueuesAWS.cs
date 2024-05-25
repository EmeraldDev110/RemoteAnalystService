using System;
using System.Data;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class ReportQueuesAWS {
        private readonly string _connectionString;

        public ReportQueuesAWS(string connectionString) {
            _connectionString = connectionString;
        }

        public DataTable GetCurrentQueues(int typeID, string instanceID) {
            string connectionString = _connectionString;
            string cmdText = "SELECT * FROM ReportQueuesAWS WHERE Loading = 0 AND TypeID = @TypeID AND InstanceID = @InstanceID ORDER BY QueueID LIMIT 1";
            var entityOrders = new DataTable();

            using (var connection = new MySqlConnection(connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@TypeID", typeID);
                command.Parameters.AddWithValue("@InstanceID", instanceID);

                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(entityOrders);
            }

            return entityOrders;
        }

        public void UpdateOrders(int queueID) {
            string connectionString = _connectionString;
            string cmdText = "UPDATE ReportQueuesAWS SET Loading = 1 WHERE QueueID = @QueueID";

            using (var connection = new MySqlConnection(connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@QueueID", queueID);
                connection.Open();

                command.ExecuteNonQuery();
            }
        }

        public int GetProcessingOrder(int typeID, string instanceID) {
            string connectionString = _connectionString;
            string cmdText = "SELECT COUNT(QueueID) AS ProcessCount FROM ReportQueuesAWS WHERE Loading = 1 AND TypeID = @TypeID AND InstanceID = @InstanceID";
            int processingEntity = 0;

            using (var connection = new MySqlConnection(connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@TypeID", typeID);
                command.Parameters.AddWithValue("@InstanceID", instanceID);
                connection.Open();

                var reader = command.ExecuteReader();

                if (reader.Read()) {
                    processingEntity = Convert.ToInt32(reader["ProcessCount"]);
                }
            }

            return processingEntity;
        }

        public void InsertNewQueue(string message, int typeID, string instanceID) {
            string ntsConnectionString = _connectionString;
            string cmdText = @"INSERT INTO ReportQueuesAWS (Message ,TypeID ,Loading, OrderDate, InstanceID)
                           VALUES (@Message ,@TypeID ,@Loading, @OrderDate, @InstanceID)";

            using (var connection = new MySqlConnection(ntsConnectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@Message", message);
                command.Parameters.AddWithValue("@TypeID", typeID);
                command.Parameters.AddWithValue("@Loading", 0);
                command.Parameters.AddWithValue("@OrderDate", DateTime.Now);
                command.Parameters.AddWithValue("@InstanceID", instanceID);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public bool CheckOtherQueues(string instanceID) {
            string connectionString = _connectionString;
            string cmdText = "SELECT QueueID FROM ReportQueuesAWS WHERE InstanceID = @InstanceID LIMIT 1";
            var inQueue = false;

            using (var connection = new MySqlConnection(connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@InstanceID", instanceID);
                connection.Open();

                var reader = command.ExecuteReader();

                if (reader.Read()) {
                    inQueue = true;
                }
            }

            return inQueue;
        }

        public int GetCurrentCount(int typeID, string instanceID) {
            string connectionString = _connectionString;
            string cmdText = "SELECT COUNT(QueueID) AS ProcessCount FROM ReportQueuesAWS WHERE TypeID = @TypeID AND InstanceID = @InstanceID";
            int processingEntity = 0;

            using (var connection = new MySqlConnection(connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@TypeID", typeID);
                command.Parameters.AddWithValue("@InstanceID", instanceID);
                connection.Open();

                var reader = command.ExecuteReader();

                if (reader.Read()) {
                    processingEntity = Convert.ToInt32(reader["ProcessCount"]);
                }
            }

            return processingEntity;
        }

        public void RemoveQueue(int queueID) {
            string connectionString = _connectionString;
            string cmdText = "DELETE FROM ReportQueuesAWS WHERE QueueID = @QueueID";

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
