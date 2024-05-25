using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteAnalyst.UWSLoader.Core.Repository;

namespace RemoteAnalyst.UWSLoader.Core.BusinessLogic {
    class CurrentTableService {
        private readonly string _connectionString = "";

        public CurrentTableService(string connectionString) {
            _connectionString = connectionString;
        }

        public void DeleteEntryFor(string tableName) {
            var currentTables = new CurrentTables(_connectionString);
            currentTables.DeleteEntry(tableName);
        }

        public void InsertEntryFor(string tableName, int entityID, long interval, DateTime startTime,
            string UWSSerialNumber, string measVersion) {
            var currentTables = new CurrentTables(_connectionString);
            currentTables.InsertEntry(tableName, entityID, interval, startTime, UWSSerialNumber, measVersion);
        }

        public long GetIntervalFor(string buildTableName) {
            var currentTables = new CurrentTables(_connectionString);
            long retVal = currentTables.GetInterval(buildTableName);
            return retVal;
        }
    }
}
