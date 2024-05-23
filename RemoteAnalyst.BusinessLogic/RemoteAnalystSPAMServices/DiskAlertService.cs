using System;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices
{
    public class DiskAlertService
    {
        private readonly string _connectionString = "";

        public DiskAlertService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void DeleteDataFor(DateTime oldDate)
        {
            var diskAlerts = new DiskAlertRepository(_connectionString);
            diskAlerts.DeleteData(oldDate);
        }
    }
}