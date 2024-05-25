using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteAnalyst.UWSLoader.Core.Repository;

namespace RemoteAnalyst.UWSLoader.Core.BusinessLogic {
    class TableTimeStampService {
        private readonly string _connectionString = "";

        public TableTimeStampService(string connectionString) {
            _connectionString = connectionString;
        }

        public void DeleteEntryFor(string tableName) {
            var timestamp = new TableTimeStamp(_connectionString);
            timestamp.DeleteEntry(tableName);
        }

        public void DeleteEntryFor(string tableName, DateTime startTime, DateTime stopTime) {
            var timestamp = new TableTimeStamp(_connectionString);
            timestamp.DeleteEntry(tableName, startTime, stopTime);
        }

        public void InsertEntryFor(string tableName, DateTime startTime, DateTime stopTime, int status) {
            var timestamp = new TableTimeStamp(_connectionString);
            timestamp.InsetEntryFor(tableName, startTime, stopTime, status);
        }

        public bool CheckTimeOverLapFor(string tableName, DateTime startTime, DateTime stopTime) {
            var timestamp = new TableTimeStamp(_connectionString);
            bool retVal = timestamp.CheckTimeOverLap(tableName, startTime, stopTime);
            return retVal;
        }

        public bool CheckTempTimeOverLapFor(string tableName, DateTime startTime, DateTime stopTime) {
            var timestamp = new TableTimeStamp(_connectionString);
            bool retVal = timestamp.CheckTempTimeOverLap(tableName, startTime, stopTime);
            return retVal;
        }
        //Used in SPAM cleaner
        public void UpdateStatusUsingTableNameFor(string tableName, DateTime startTime, DateTime stopTime, int status) {
            var timestamp = new TableTimeStamp(_connectionString);
            timestamp.UpdateStatusUsingTableName(tableName, startTime, stopTime, status);
        }
        //Used in UWSLoader
        public void UpdateStatusUsingArchiveIDFor(string archiveID, int status) {
            var timestamp = new TableTimeStamp(_connectionString);
            timestamp.UpdateStatusUsingArchiveID(archiveID, status);
        }

        public void UpdateArchiveIDFor(string tableName, DateTime startTime, DateTime stopTime, string archiveID, DateTime creationDate) {
            var timestamp = new TableTimeStamp(_connectionString);
            timestamp.UpdateArchiveID(tableName, startTime, stopTime, archiveID, creationDate);
        }

        public string GetArchiveIDFor(string tableName, DateTime startTime, DateTime stopTime) {
            var timestamp = new TableTimeStamp(_connectionString);
            string archiveID = timestamp.GetArchiveID(tableName, startTime, stopTime);
            return archiveID;
        }
        public string GetArchiveIDFor(string tableName) {
            var timestamp = new TableTimeStamp(_connectionString);
            string archiveID = timestamp.GetArchiveID(tableName);
            return archiveID;
        }


    }
}
