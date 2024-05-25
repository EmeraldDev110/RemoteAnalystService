using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using RemoteAnalyst.UWSLoader.Core.Repository;

namespace RemoteAnalyst.UWSLoader.Core.BusinessLogic {
    class DatabaseMappingService {
        private readonly string _connectionString = "";

        public DatabaseMappingService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public string GetConnectionStringFor(string systemSerial)
        {
            var databaseMappings = new DatabaseMappings(_connectionString);
            string connectionString = databaseMappings.GetConnectionString(systemSerial);

            return connectionString;
        }

        public string GetMySQLConnectionStringFor(string systemSerial) {
            var databaseMappings = new DatabaseMappings(_connectionString);
            string connectionString = databaseMappings.GetMySQLConnectionString(systemSerial);

            return connectionString;
        }

        public string GetMySQLConnectionStringFor() {
            var databaseMappings = new DatabaseMappings(_connectionString);
            string connectionString = databaseMappings.GetMySQLConnectionString();

            return connectionString;
        }

        public IDictionary<string, string> GetAllConnectionStringFor()
        {
            var databaseMappings = new DatabaseMappings(_connectionString);
            DataTable dt = databaseMappings.GetAllConnectionString();
            IDictionary<string, string> retVal = new Dictionary<string, string>();

            foreach (DataRow dr in dt.Rows)
            {
                string systemSerial = Convert.ToString(dr["SystemSerial"]);
                string connectionStr = Convert.ToString(dr["ConnectionString"]);

                if (!retVal.ContainsKey(systemSerial))
                    retVal.Add(systemSerial, connectionStr);
            }
            return retVal;
        }

        public bool CheckDatabaseFor(string connectionString)
        {
            var databaseMappings = new DatabaseMappings(_connectionString);
            bool exists = databaseMappings.CheckDatabase(connectionString);

            return exists;
        }

        public void InsertNewEntryFor(string systemSerial, string newConnectionString) {
            var databaseMappings = new DatabaseMappings(_connectionString);
            databaseMappings.InsertNewEntry(systemSerial, newConnectionString);
        }

        public void UpdateMySQLConnectionStringFor(string systemSerial, string mySQLConnectionString) {
            var databaseMappings = new DatabaseMappings(_connectionString);
            databaseMappings.UpdateMySQLConnectionString(systemSerial, mySQLConnectionString);
        }

        public string GetConnectionStringDynamicReportGenerator(string systemSerial, string newConnectionString) {
            newConnectionString = newConnectionString.Replace("SERIALNUMBER", systemSerial);
            return newConnectionString;
        }
    }
}
