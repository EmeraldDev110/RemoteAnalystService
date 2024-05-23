using System;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices
{
    public class CpuAlertService
    {
        private readonly string _connectionString = "";

        public CpuAlertService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void DeleteDataFor(DateTime oldDate)
        {
            var cpuAlerts = new CpuAlertRepository(_connectionString);
            cpuAlerts.DeleteData(oldDate);
        }
    }
}