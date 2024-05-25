using System;
using System.Collections.Generic;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM
{
    public class ReportGroupDetail
    {
        private readonly string _connectionString;

        public ReportGroupDetail(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IList<int> GetReportIDs(int groupID)
        {
            IList<int> reports = new List<int>();
            string cmdText = @"SELECT ReportID FROM ReportGroupDetail
                                        WHERE GroupID = @GroupID";

            using (var connectioniReport = new MySqlConnection(_connectionString))
            {
                var commandiReport = new MySqlCommand(cmdText, connectioniReport);
                commandiReport.Parameters.AddWithValue("@GroupID", groupID);
                connectioniReport.Open();
                var readeriReport = commandiReport.ExecuteReader();
                while (readeriReport.Read())
                {
                    if (!reports.Contains(Convert.ToInt32(readeriReport["ReportID"])))
                        reports.Add(Convert.ToInt32(readeriReport["ReportID"]));
                }
            }

            return reports;
        }
    }
}