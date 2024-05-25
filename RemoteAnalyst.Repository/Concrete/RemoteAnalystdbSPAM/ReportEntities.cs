using System;
using System.Collections.Generic;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM
{
    public class ReportEntities
    {
        private readonly string ConnectionString = "";

        public ReportEntities(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public IList<int> GetReportWithFileEntity()
        {
            string cmdText = "SELECT ReportID FROM ReportEntities WHERE Entities LIKE '%5%'";
            IList<int> reportWithFileEntity = new List<int>();

            using (var connection = new MySqlConnection(ConnectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                connection.Open();
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    reportWithFileEntity.Add(Convert.ToInt32(reader["ReportID"]));
                }
            }

            return reportWithFileEntity;
        }

    }
}