using System;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices
{
    public class ReportActivityService
    {
        private readonly string ConnectionString = "";
        private readonly string ConnectionStringTrend = "";

        public ReportActivityService(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public void InsertNewEntryFor(string email, string systemSerial, string reportType, string reportName)
        {
            var reportActivity = new ReportActivity(ConnectionString);
            reportActivity.InsertNewEntry(email, systemSerial, reportType, reportName, DateTime.Now, DateTime.Now);
        }

        public void InsertNewEntryFor(string email, string systemSerial, string reportType, string reportName,
            DateTime startTime, DateTime endTime)
        {
            var reportActivity = new ReportActivity(ConnectionString);
            reportActivity.InsertNewEntry(email, systemSerial, reportType, reportName, startTime, startTime);
        }

    }
}