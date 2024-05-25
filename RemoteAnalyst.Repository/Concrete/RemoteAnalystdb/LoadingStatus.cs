using System;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb {
    public class LoadingStatus {
        private readonly string _connectionString = "";

        public LoadingStatus(string connectionString) {
            _connectionString = connectionString;
        }

        public void UpdateLoadingStatus(string instanceID, int value) {
            string cmdText = "UPDATE LoadingStatus SET CurrentLoad = @CurrentLoad WHERE InstanceID = @InstanceID";
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@InstanceID", instanceID);
                command.Parameters.AddWithValue("@CurrentLoad", value);
                try {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (Exception ex) {
                    throw new Exception(ex.Message);
                }
            }
        }

        public int GetCurrentLoad(string instanceID) {
            string cmdText = "SELECT CurrentLoad FROM LoadingStatus WHERE InstanceID = @InstanceID";
            int currentLoad = 0;
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@InstanceID", instanceID);
                connection.Open();
                var reader = command.ExecuteReader();

                if (reader.Read()) {
                    currentLoad = Convert.ToInt32(reader["CurrentLoad"].ToString());
                }
                reader.Close();
            }
            return currentLoad;
        }

        public bool CheckLoading(string instanceID) {
            string cmdText = "SELECT CurrentLoad, MaxLoad FROM LoadingStatus WHERE InstanceID = @InstanceID";
            bool retVal = false;
            int currentLoad = 0;
            int maxLoad = 0;
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@InstanceID", instanceID);
                connection.Open();
                try {
                    var reader = command.ExecuteReader();
                    if (reader.Read()) {
                        currentLoad = Convert.ToInt32(reader["CurrentLoad"].ToString());
                        maxLoad = Convert.ToInt32(reader["MaxLoad"].ToString());
                    }
                    if (currentLoad < maxLoad)
                        retVal = true;
                }
                catch { }
            }
            return retVal;
        }

        public int CheckCurrentLoads(string instanceID) {
            string cmdText = "SELECT MaxLoad - CurrentLoad AS Available FROM LoadingStatus WHERE InstanceID = @InstanceID";
            int available = 0;
            using (var connection = new MySqlConnection(_connectionString)) {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@InstanceID", instanceID);
                connection.Open();
                var reader = command.ExecuteReader();

                if (reader.Read()) {
                    available = Convert.ToInt32(reader["Available"]);
                }
                reader.Close();
            }
            return available;
        }

    }
}