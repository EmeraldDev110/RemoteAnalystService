using System;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Concrete.RemoteAnalystdb
{
    public class ReportActivity
    {
        private readonly string ConnectionString = "";
        private readonly string ConnectionStringTrend = "";

        public ReportActivity(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public ReportActivity(string connectionString, string connectionStringTrend)
        {
            ConnectionString = connectionString;
            ConnectionStringTrend = connectionStringTrend;
        }

        public void InsertNewEntry(string email, string systemSerial, string reportType, string reportName,
            DateTime from, DateTime to)
        {
            string cmdText = "";

            if (reportType != "Storage")
            {
                cmdText = "INSERT INTO ReportActivity (Date, Email, SystemSerial, " +
                          "ReportType, ReportName, PeriodFrom, PeriodTo) VALUES " +
                          "(@Date, @Email, @SystemSerial, @ReportType, " +
                          "@ReportName, @PeriodFrom, @PeriodTo)";
            }
            else
            {
                cmdText = "INSERT INTO ReportActivity (Date, Email, SystemSerial, " +
                          "ReportType, ReportName) VALUES " +
                          "(@Date, @Email, @SystemSerial, @ReportType, " +
                          "@ReportName)";
            }

            using (var connection = new MySqlConnection(ConnectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                command.CommandTimeout = 0;
                command.Parameters.AddWithValue("@Date", DateTime.Now);
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                command.Parameters.AddWithValue("@ReportType", reportType);
                command.Parameters.AddWithValue("@ReportName", reportName);
                if (reportType != "Storage")
                {
                    command.Parameters.AddWithValue("@PeriodFrom", from);
                    command.Parameters.AddWithValue("@PeriodTo", to);
                }
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}