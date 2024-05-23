using System;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices
{
    public class DiskFileAlertService
    {
        private readonly string _connectionString = "";

        public DiskFileAlertService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void DeleteDataFor(DateTime oldDate)
        {
            var diskFileAlerts = new DiskFileAlertRepository(_connectionString);
            diskFileAlerts.DeleteData(oldDate);
        }
    }
}