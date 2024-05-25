using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using RemoteAnalyst.UWSLoader.Core.Repository;

namespace RemoteAnalyst.UWSLoader.Core.BusinessLogic {
    class DataTableService {
        private readonly string _connectionString = "";

        public DataTableService(string connectionString) {
            _connectionString = connectionString;
        }

        public IDictionary<string, DateTime> GetCreatedDateFor(string systemSerial, string systemConnectionString) {
            var dataTables = new DataTables(_connectionString);

            IDictionary<string, DateTime> tables = new Dictionary<string, DateTime>();
            tables = dataTables.GetCreatedDate(systemSerial, systemConnectionString);
            return tables;
        }

        public void RebuildIndexFor(string tableName, string indexName) {
            var dataTables = new DataTables(_connectionString);
            dataTables.RebuildIndex(tableName, indexName);
        }

        public void DropTableFor(string tableName) {
            var dataTables = new DataTables(_connectionString);
            dataTables.DropTable(tableName);
        }

        public bool CheckDatabaseFor(string databaseName) {
            var dataTables = new DataTables(_connectionString);
            bool exists = false;
            exists = dataTables.CheckDatabase(databaseName);

            return exists;
        }

        public bool CheckTableFor(string tableName) {
            var dataTables = new DataTables(_connectionString);
            bool exists = false;
            exists = dataTables.CheckTable(tableName);

            return exists;
        }

        public void InsertEntityDataFor(string tableName, DataTable dsData, string path) {
            var dataTables = new DataTables(_connectionString);
            dataTables.InsertEntityData(tableName, dsData, path);
        }

        public void InsertSPAMEntityDataFor(string tableName, DataTable dsData, DateTime selectedStartTime, DateTime selectedStopTime, string path) {
            var dataTables = new DataTables(_connectionString);
            dataTables.InsertSPAMEntityData(tableName, dsData, selectedStartTime, selectedStartTime, path);
        }

        public void InsertSPAMEntityDataFor(string tableName, DataTable dsData, string path) {
            var dataTables = new DataTables(_connectionString);
            dataTables.InsertSPAMEntityData(tableName, dsData, path);
        }

        public void DeleteTempTableFor(string tableName) {
            var dataTables = new DataTables(_connectionString);
            dataTables.DeleteTempTable(tableName);
        }

        public void CreateFileIndexFor(string fileTableName) {
            var dataTables = new DataTables(_connectionString);
            dataTables.CreateFileIndex(fileTableName);
        }
    }
}
