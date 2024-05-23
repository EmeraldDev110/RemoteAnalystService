using System;
using System.Collections.Generic;
using System.IO;
using log4net;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices
{
    public class DatabaseService
    {
        private readonly string _connectionString = "";

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public string CreateDatabaseFor(string databaseName, ILog log)
        {
            var db = new Database(_connectionString);
            string retVal = db.CreateDatabase(databaseName, log);
            return retVal;
        }

        public void CreateTablesFor(string databaseName, string connectionString)
        {
            var db = new Database(connectionString);
            db.CreateTables(databaseName);
        }

        public List<String> GetQNMTableNamesInDatabaseFor(string tableSchema) {
            var db = new Database(_connectionString);
            return db.GetQNMTableNamesInDatabase(tableSchema);
        }

        public bool CheckTableExistsFor(string tableName, string databaseName) {
            var db = new Database(_connectionString);
            return db.CheckTableExists(tableName, databaseName);
        }

        public List<string> GetDetailTableList(string systemSerial, string databaseName) {
            var db = new Database(_connectionString);
            var detailTableList = db.GetDetailTableList(systemSerial, databaseName);

            return detailTableList;
        }

        public void UpdateSystemName(string tableName, string newSystemName) {
            var db = new Database(_connectionString);
            db.UpdateSystemName(tableName, newSystemName);
        }

        public void BulkDeleteSingleParameterFor(string cmdText, string parameterName, List<String> singleParameters) {
            var db = new Database(_connectionString);
            db.BulkDeleteSingleParameter(cmdText, parameterName, singleParameters);
        }
    }
}