using System;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices
{
    public class AlertSummaryService
    {
        private readonly string _connectionString = "";

        public AlertSummaryService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void DeleteDataFor(DateTime oldDate)
        {
            var alertSummary = new AlertSummaryRepository(_connectionString);
            alertSummary.DeleteData(oldDate);
        }
    }
}