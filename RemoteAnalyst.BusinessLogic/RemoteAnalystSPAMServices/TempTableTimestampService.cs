using System;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices
{
    public class TempTableTimestampService
    {
        private readonly string ConnectionString = "";

        public TempTableTimestampService(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public void DeleteTempTimeStampFor(string tableName)
        {
            var tempTableTimestamp = new TempTableTimestampRepository(ConnectionString);
            tempTableTimestamp.DeleteTempTimeStamp(tableName);
        }

        public void InsertTempTimeStampFor(string tableName, DateTime startTime, DateTime stopTime, string fileName)
        {
            CreateNewColumnsIfNotPresent();

            var tempTableTimestamp = new TempTableTimestampRepository(ConnectionString);
            tempTableTimestamp.InsertTempTimeStamp(tableName, startTime, stopTime, fileName);
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

        internal void CreateNewColumnsIfNotPresent() {
            var tempTimestamp = new TempTableTimestampRepository(ConnectionString);
            var databaseName = FindDatabaseName(ConnectionString);
            var table = "TempTableTimestamp";
            if (!tempTimestamp.CheckMySqlColumn(databaseName, table, "FileName")) {
                tempTimestamp.AddFileNameColumnToTempTableTimestampTable();
            }
        }
    }
}