using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteAnalyst.UWSLoader.Core.Repository;

namespace RemoteAnalyst.UWSLoader.Core.BusinessLogic {
    class TempCurrentTablesService {
        private readonly string _connectionString = "";

        public TempCurrentTablesService(string connectionString) {
            _connectionString = connectionString;
        }

        public void DeleteCurrentTableFor(string tableName) {
            var tempCurrentTables = new TempCurrentTables(_connectionString);
            tempCurrentTables.DeleteCurrentTable(tableName);
        }

        public void InsertCurrentTableFor(string tableName, int entityID, long interval, DateTime startTime,
            string UWSSerialNumber, string measVersion) {
            var tempCurrentTables = new TempCurrentTables(_connectionString);
            tempCurrentTables.InsertCurrentTable(tableName, entityID, interval, startTime, UWSSerialNumber, measVersion);
        }

        public long GetIntervalFor(string buildTableName) {
            var tempCurrentTables = new TempCurrentTables(_connectionString);
            long retVal = tempCurrentTables.GetInterval(buildTableName);
            return retVal;
        }
    }
}
