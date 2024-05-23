using System;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices
{
    public class ProcessIntervalService
    {
        private readonly string _connectionString;

        public ProcessIntervalService(string connectionStringTrend)
        {
            _connectionString = connectionStringTrend;
        }

        public void DeleteDataFor(DateTime oldDate)
        {
            var processInterval = new ProcessIntervalRepository(_connectionString);
            processInterval.DeleteData(oldDate);
        }
    }
}