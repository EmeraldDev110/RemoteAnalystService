using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices
{
    public class DataTableService
    {
        private readonly string _connectionString = "";

        public DataTableService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDictionary<string, DateTime> GetCreatedDateFor(string systemSerial, string systemConnectionString, string databaseName)
        {
            var dataTables = new DataTables(_connectionString);

            IDictionary<string, DateTime> tables = new Dictionary<string, DateTime>();
            tables = dataTables.GetCreatedDate(systemSerial, systemConnectionString, databaseName);
            return tables;
        }

        public void DropTableFor(string tableName)
        {
            var dataTables = new DataTables(_connectionString);
            dataTables.DropTable(tableName);
        }

        public void InsertEntityDataFor(string tableName, DataTable dsData, string path)
        {
            var dataTables = new DataTables(_connectionString);
            dataTables.InsertEntityData(tableName, dsData, path);
        }

        public void InsertSPAMEntityDataFor(string tableName, DataTable dsData, DateTime selectedStartTime, DateTime selectedStopTime, string path)
        {
            var dataTables = new DataTables(_connectionString);
            dataTables.InsertSPAMEntityData(tableName, dsData, selectedStartTime, selectedStartTime, path);
        }

        public void DeleteTempTableFor(string tableName)
        {
            var dataTables = new DataTables(_connectionString);
            dataTables.DeleteTempTable(tableName);
        }

        public void CreateFileIndexFor(string fileTableName)
        {
            var dataTables = new DataTables(_connectionString);
            dataTables.CreateFileIndex(fileTableName);
        }

        public void RunCommandFor(string cmdText) {
            var dataTables = new DataTables(_connectionString);
            dataTables.RunCommand(cmdText);
        }
    }
}