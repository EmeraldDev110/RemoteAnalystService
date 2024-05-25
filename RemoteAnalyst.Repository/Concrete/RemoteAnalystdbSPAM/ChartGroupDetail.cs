using System;
using System.Collections.Generic;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM
{
    public class ChartGroupDetail
    {
        private readonly string _connectionString;

        public ChartGroupDetail(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IList<int> GetChartIDs(int groupID)
        {
            IList<int> charts = new List<int>();
            string cmdText = @"SELECT ChartID FROM ChartGroupDetail
                                        WHERE GroupID = @GroupID";

            using (var connectioniReport = new MySqlConnection(_connectionString))
            {
                var commandiReport = new MySqlCommand(cmdText, connectioniReport);
                commandiReport.Parameters.AddWithValue("@GroupID", groupID);
                connectioniReport.Open();
                var readeriReport = commandiReport.ExecuteReader();
                while (readeriReport.Read())
                {
                    if (!charts.Contains(Convert.ToInt32(readeriReport["ChartID"])))
                        charts.Add(Convert.ToInt32(readeriReport["ChartID"]));
                }
            }

            return charts;
        }
    }
}