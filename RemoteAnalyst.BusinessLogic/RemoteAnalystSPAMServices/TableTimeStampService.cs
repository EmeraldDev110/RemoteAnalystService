using System;
using System.Collections.Generic;
using System.Data;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.Repository.Concrete.Model;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices
{
    public class TableTimeStampService
    {
        private readonly string _connectionString = "";

        public TableTimeStampService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void DeleteEntryFor(string tableName)
        {
            var timestamp = new TableTimestampRepository(_connectionString);
            timestamp.DeleteEntry(tableName);
        }

        public void DeleteEntryFor(List<TableTimestampQueryParameter> parameters) {
            var timestamp = new TableTimestampRepository(_connectionString);
            timestamp.DeleteEntry(parameters);
        }

        public void InsertEntryFor(string tableName, DateTime startTime, DateTime stopTime, int status, string fileName)
        {
            var timestamp = new TableTimestampRepository(_connectionString);
            CreateNewColumnsIfNotPresent();
            timestamp.InsetEntryFor(tableName, startTime, stopTime, status, fileName);
        }

        public bool CheckTimeOverLapFor(string tableName, DateTime startTime, DateTime stopTime)
        {
            var timestamp = new TableTimestampRepository(_connectionString);
            bool retVal = timestamp.CheckTimeOverLap(tableName, startTime, stopTime);
            return retVal;
        }

        public bool CheckDuplicateFor(string tableName, DateTime startTime, DateTime stopTime) {
            var timestamp = new TableTimestampRepository(_connectionString);
            bool retVal = timestamp.CheckDuplicate(tableName, startTime, stopTime);
            return retVal;
        }

        public bool CheckTempTimeOverLapFor(string tableName, DateTime startTime, DateTime stopTime)
        {
            var timestamp = new TableTimestampRepository(_connectionString);
            bool retVal = timestamp.CheckTempTimeOverLap(tableName, startTime, stopTime);
            return retVal;
        }
        //Used in SPAM cleaner
        public void UpdateStatusUsingTableNameFor(string tableName, DateTime startTime, DateTime stopTime, int status) {
            var timestamp = new TableTimestampRepository(_connectionString);
            timestamp.UpdateStatusUsingTableName(tableName, startTime, stopTime, status);
        }

        //Used in SPAM cleaner
        public void UpdateStatusUsingTableNameFor(List<TableTimestampQueryParameter> parameters) {
            var timestamp = new TableTimestampRepository(_connectionString);
            timestamp.UpdateStatusUsingTableName(parameters);
        }

        public List<ArchiveStatusView> GetArchiveDetailsPerTableFor(string tableName) {
            var timestamp = new TableTimestampRepository(_connectionString);
            var archiveDetails = new List<ArchiveStatusView>();
            DataTable archiveStatusDataTable = timestamp.GetArchiveDetailsPerTable(tableName);

            foreach (DataRow dr in archiveStatusDataTable.Rows) {
                var archiveStatusView = new ArchiveStatusView {
                    TableName = Convert.ToString(dr["TableName"]),
                    StartTime = Convert.ToDateTime(dr["Start"]),
                    StopTime = Convert.ToDateTime(dr["End"]),
                    ArchiveID = Convert.ToString(dr["ArchiveID"])
                };
                archiveDetails.Add(archiveStatusView);
            }
            return archiveDetails;
        } 

        public DataTable GetLoadedDataFor(DateTime dataStartDate, DateTime dataStopDate) {
            var timestamp = new TableTimestampRepository(_connectionString);
            var loadedData = timestamp.GetGetLoadedData(dataStartDate, dataStopDate);
            return loadedData;
        }

        public DataTable GetLoadedFileDataFor(DateTime dataStartDate, DateTime dataStopDate) {
            var timestamp = new TableTimestampRepository(_connectionString);
            var loadedData = timestamp.GetLoadedFileData(dataStartDate, dataStopDate);
            return loadedData;
        }

        public string FindDatabaseName(string mysqlConnectionString) {
            string databaseName = "";
            string[] tempNames = mysqlConnectionString.Split(';');
            foreach (string s in tempNames) {
                if (s.ToUpper().Contains("DATABASE")) {
                    databaseName = s.Split('=')[1];
                }
            }
            return databaseName;
        }
        
        public void CreateNewColumnsIfNotPresent() {
            var timestamp = new TableTimestampRepository(_connectionString);
            var databaseName = FindDatabaseName(_connectionString);
            var table = "TableTimestamp";
            if (!timestamp.CheckMySqlColumn(databaseName, table, "FileName")) {
                timestamp.AddFileNameColumnToTableTimestampTable();
            }
        }

        public DataTable GetTimestampsFor(string tableName, DateTime startTime, DateTime stopTime)
        {
            var timestamp = new TableTimestampRepository(_connectionString);
            return timestamp.GetTimestampsFor(tableName, startTime, stopTime);
        }
    }
}