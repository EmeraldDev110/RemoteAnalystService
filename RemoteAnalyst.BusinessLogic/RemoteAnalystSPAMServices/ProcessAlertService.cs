using System;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices
{
    public class ProcessAlertService
    {
        private readonly string _connectionString = "";

        public ProcessAlertService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void DeleteDataFor(DateTime oldDate)
        {
            var processAlerts = new ProcessAlertRepository(_connectionString);
            processAlerts.DeleteData(oldDate);
        }
    }
}