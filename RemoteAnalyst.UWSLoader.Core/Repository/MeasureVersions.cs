using System;
using System.Collections.Generic;
using MySqlConnector;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.UWSLoader.Core.Repository {
    class MeasureVersions {
        private readonly string ConnectionString = "";

        public MeasureVersions(string connectionString) {
            ConnectionString = connectionString;
        }

        public string GetMeasureDBTableName(string version) {
            string tableName = string.Empty;
            string versionName = string.Empty;
            //string connectionString = Config.ConnectionString;
            string cmdText = "SELECT DBTableName " +
                             "FROM MeasureVersions " +
                             "WHERE @Version >= FromVersion AND @Version <= ToVersion ";

            try {
                using (var connection = new MySqlConnection(ConnectionString)) {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@Version", version);
                    connection.Open();
                    var reader = command.ExecuteReader();

                    if (reader.Read()) {
                        tableName = Convert.ToString(reader["DBTableName"]);
                    }
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }

            if (tableName == "") {
                tableName = "LegacyDataDictionary";
            }
            return tableName;
        }
    }
}
