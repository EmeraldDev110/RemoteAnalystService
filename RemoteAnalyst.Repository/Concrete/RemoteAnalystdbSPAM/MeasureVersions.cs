using System;
using MySqlConnector;
using RemoteAnalyst.Repository.Infrastructure;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM
{
    public class MeasureVersions
    {
        private readonly string ConnectionString = "";

        public MeasureVersions(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string GetMeasureDBTableName(string version)
        {
            string tableName = string.Empty;
            string versionName = string.Empty;
            //string connectionString = Config.ConnectionString;
            string cmdText = "SELECT DBTableName " +
                             "FROM MeasureVersions " +
                             "WHERE @Version >= FromVersion AND @Version <= ToVersion ";

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    var command = new MySqlCommand(cmdText + Helper.CommandParameter, connection) {CommandTimeout = 0};
                    command.Parameters.AddWithValue("@Version", version);
                    connection.Open();
                    var reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        tableName = Convert.ToString(reader["DBTableName"]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            if (tableName == "")
            {
                tableName = "LegacyDataDictionary";
            }
            return tableName;
        }
    }
}