using System;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices
{
    public class TempCurrentTablesService
    {
        private readonly string ConnectionString = "";

        public TempCurrentTablesService(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public void DeleteCurrentTableFor(string tableName)
        {
            var tempCurrentTables = new TempCurrentTableRepository(ConnectionString);
            tempCurrentTables.DeleteCurrentTable(tableName);
        }

        public void InsertCurrentTableFor(string tableName, int entityID, long interval, DateTime startTime,
            string UWSSerialNumber, string measVersion)
        {
            var tempCurrentTables = new TempCurrentTableRepository(ConnectionString);
            tempCurrentTables.InsertCurrentTable(tableName, entityID, interval, startTime, UWSSerialNumber, measVersion);
        }

        public long GetIntervalFor(string buildTableName)
        {
            var tempCurrentTables = new TempCurrentTableRepository(ConnectionString);
            long retVal = tempCurrentTables.GetInterval(buildTableName);
            return retVal;
        }
    }
}