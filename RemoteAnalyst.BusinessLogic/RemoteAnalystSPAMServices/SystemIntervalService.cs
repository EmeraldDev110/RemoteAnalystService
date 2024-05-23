using System;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices
{
    public class SystemIntervalService
    {
        private readonly string _connectionString;

        public SystemIntervalService(string connectionStringTrend)
        {
            _connectionString = connectionStringTrend;
        }

        public void DeleteDataFor(DateTime oldDate)
        {
            var systemInterval = new SystemIntervalRepository(_connectionString);
            systemInterval.DeleteData(oldDate);
        }
    }
}