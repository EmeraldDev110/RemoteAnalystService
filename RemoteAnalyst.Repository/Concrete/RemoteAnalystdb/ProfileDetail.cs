using System;
using System.Collections.Generic;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb
{
    public class ProfileDetail
    {
        private readonly string ConnectionString = "";

        public ProfileDetail(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string GetApplicationName(int recordId) {
            var sqlStr = @"SELECT ProfileEntity FROM ProfileDetail WHERE RecordId = @RecordId";

            var applicationName = "";
            using (var connection = new MySqlConnection(ConnectionString)) {
                var command = new MySqlCommand(sqlStr, connection) {
                    CommandTimeout = 0
                };
                command.Parameters.AddWithValue("@RecordId", recordId);
                connection.Open();

                var reader = command.ExecuteReader();

                if (reader.Read()) {
                    applicationName = reader["ProfileEntity"].ToString();
                }
            }

            return applicationName;
        }
    }
}