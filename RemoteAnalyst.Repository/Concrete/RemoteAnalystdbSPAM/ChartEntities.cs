using System;
using System.Collections.Generic;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM
{
    public class ChartEntities
    {
        private readonly string ConnectionString = "";

        public ChartEntities(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public IList<int> GetChartWithFileEntity()
        {
            string cmdText = "SELECT ChartID FROM ChartEntities WHERE Entities LIKE '%5%'";
            IList<int> chartWithFileEntity = new List<int>();

            using (var connection = new MySqlConnection(ConnectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                connection.Open();
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    chartWithFileEntity.Add(Convert.ToInt32(reader["ChartID"]));
                }
            }

            return chartWithFileEntity;
        }

    }
}