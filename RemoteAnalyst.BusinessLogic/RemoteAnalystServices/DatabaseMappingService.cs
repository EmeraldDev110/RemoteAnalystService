using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using RemoteAnalyst.Repository.Models;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices
{
    public class DatabaseMappingService
    {
        private readonly string _connectionString = "";
        private readonly Decrypt stringDecrypt;

        public DatabaseMappingService(string connectionString)
        {
            _connectionString = connectionString;
            stringDecrypt = new Decrypt();
        }

        public string GetConnectionStringFor(string systemSerial)
        {
            var databaseMappings = new DatabaseMappingRepository();
            string connectionString = databaseMappings.GetConnectionString(systemSerial);
            return connectionString;
        }

        public string GetMySQLConnectionStringFor(string systemSerial) {
            var databaseMappings = new DatabaseMappingRepository();
            string connectionString = databaseMappings.GetConnectionString(systemSerial);

            return connectionString;
        }

        public IDictionary<string, string> GetAllConnectionStringFor()
        {
            var databaseMappings = new DatabaseMappingRepository();
            ICollection<DatabaseMapping> rows = databaseMappings.GetAllConnectionStrings();
            IDictionary<string, string> retVal = new Dictionary<string, string>();

            foreach (DatabaseMapping row in rows)
            {
                string systemSerial = row.SystemSerial;
                string connectionString = row.ConnectionString;
                if (!retVal.ContainsKey(systemSerial))
                    retVal.Add(systemSerial, connectionString);
            }
			return retVal;
        }

        public string GetAllConnectionStrings(string systemSerial)
        {
            var databaseMappings = new DatabaseMappingRepository();
            string connectionString = databaseMappings.GetConnectionString(systemSerial);

            return connectionString;
        }

        public bool CheckDatabaseFor(string connectionString)
        {
            var databaseMappings = new DatabaseMappingRepository();
            bool exists = databaseMappings.CheckDatabase(connectionString);

            return exists;
        }

        public string CheckConnectionFor(string connectionString) {
            var databaseMappings = new DatabaseMappingRepository();
            var isConnect = databaseMappings.CheckConnection(connectionString);

            return isConnect;
        }
        public void InsertNewEntryFor(string systemSerial, string newConnectionString) {
            var databaseMappings = new DatabaseMappingRepository();
            databaseMappings.InsertNewEntry(systemSerial, newConnectionString);
        }

        public void UpdateMySQLConnectionStringFor(string systemSerial, string mySQLConnectionString) {
            var databaseMappings = new DatabaseMappingRepository();
            databaseMappings.UpdateConnectionString(systemSerial, mySQLConnectionString);
        }
        public void UpdateConnectionString(string systemSerial, string systemConnection, string connectionString, string mainConnectionString, string isLocalAnalyst = "false")
        {
            var databaseMappings = new DatabaseMappingRepository();
            databaseMappings.UpdateConnectionString(systemSerial, systemConnection, isLocalAnalyst);

        }

        public string GetConnectionStringDynamicReportGenerator(string systemSerial, string newConnectionString, bool isLocalAnalyst, string databasePostfix = "") {
            //if(!isLocalAnalyst)
            //    newConnectionString = newConnectionString.Replace("SERIALNUMBER", systemSerial);
            //else
            //    newConnectionString = newConnectionString.Replace("SERIALNUMBER", systemSerial + databasePostfix);

            //return newConnectionString;

            var databaseMappings = new DatabaseMappingRepository();
            string connectionString = databaseMappings.GetConnectionString(systemSerial);
            if (isLocalAnalyst)
            {
                if (!connectionString.Contains("SERVER="))
                {
                    connectionString = stringDecrypt.strDESDecrypt(connectionString);
                }
            }
            return connectionString;
        }

        public List<DatabaseMappingInfo> GetAllDatabaseConnectionFor() {
            var databaseMapping = new DatabaseMappingRepository();
            var dataTable = databaseMapping.GetAllDatabaseConnection();

            var connectionsInfo = new List<DatabaseMappingInfo>();
            foreach (DataRow dataTableRow in dataTable.Rows) {
                connectionsInfo.Add(new DatabaseMappingInfo {
                    ConnectionString = Convert.ToString(dataTableRow["ConnectionString"]),
                    SystemName = Convert.ToString(dataTableRow["SystemName"]),
                    SystemSerial = Convert.ToString(dataTableRow["SystemSerial"]),
                    CompanyName = dataTableRow.IsNull("CompanyName") ? "" : Convert.ToString(dataTableRow["CompanyName"])
                });
            }

            return connectionsInfo;
        }

        public string GetRdsConnectionStringFor(string rdsName) {
            var databaseMapping = new DatabaseMappingRepository();
            var connectionString = databaseMapping.GetRdsConnectionString(rdsName);

			//Remove Database.
			var tempConnection = connectionString.Split(';');

            var newConnectionString = new StringBuilder();
            foreach (var s in tempConnection) {
                if (s.Split('=')[0].ToUpper() != "DATABASE") {
                    newConnectionString.Append(s + ";");
                }
            }


            return newConnectionString.ToString();
        }
    }
}