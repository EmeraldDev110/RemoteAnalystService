using System;
using System.Collections.Generic;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices
{
    public class CurrentTableService
    {
        private readonly string _connectionString = "";

        public CurrentTableService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void DeleteEntryFor(string tableName)
        {
            var currentTables = new CurrentTableRepository(_connectionString);
            currentTables.DeleteEntry(tableName);
        }

        public void InsertEntryFor(string tableName, int entityID, long interval, DateTime startTime,
            string UWSSerialNumber, string measVersion)
        {
            var currentTables = new CurrentTableRepository(_connectionString);
            currentTables.InsertEntry(tableName, entityID, interval, startTime, UWSSerialNumber, measVersion);
        }

        public long GetIntervalFor(string buildTableName)
        {
            var currentTables = new CurrentTableRepository(_connectionString);
            long retVal = currentTables.GetInterval(buildTableName);
            return retVal;
        }

        public long GetLatestIntervalFor() {
            var currentTables = new CurrentTableRepository(_connectionString);
            long retVal = currentTables.GetLatestIntervl();
            return retVal;

        }

        public long GetIntervalFor(List<string> buildTableNames) {
            var currentTables = new CurrentTableRepository(_connectionString);
            var retVal = 0L;

            foreach (var buildTableName in buildTableNames) {
                try {
                    retVal = currentTables.GetInterval(buildTableName);
                    if(retVal > 0)
                        break;
                }
                catch {}
            }
            return retVal;
        }

        public List<int> GetEntitiesFor(DateTime startDateTime, DateTime stopDateTime, long interval) {
            var currentTables = new CurrentTableRepository(_connectionString);
            var entities = currentTables.GetEntities(startDateTime, stopDateTime, interval);
            return entities;
        }

    }
}