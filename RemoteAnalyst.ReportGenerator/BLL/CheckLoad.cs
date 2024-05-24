using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.ReportGenerator.BLL {
    internal class CheckLoad {
        public bool CheckEntry(string systemSerial, string fileName) {
            bool duplicate = false;

            var databaseMapService = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
            string newConnectionString = databaseMapService.GetConnectionStringDynamicReportGenerator(systemSerial, ConnectionString.TempDatabaseConnectionString, ConnectionString.IsLocalAnalyst);

            var loadingStatus = new UWSLoadingStatusService(newConnectionString);
            duplicate = loadingStatus.CheckUWSLoadingStatusFor(systemSerial, fileName);

            return duplicate;
        }

        public void InsertLoadingStatus(string systemSerial, string fileName) {
            var databaseMapService = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
            string newConnectionString = databaseMapService.GetConnectionStringDynamicReportGenerator(systemSerial, ConnectionString.TempDatabaseConnectionString, ConnectionString.IsLocalAnalyst);

            var loadingStatus = new UWSLoadingStatusService(newConnectionString);
            loadingStatus.InsertUWSLoadingStatusFor(systemSerial, fileName);
        }

        public void RemoveLoadingStatus(string systemSerial, string fileName) {
            var databaseMapService = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
            string newConnectionString = databaseMapService.GetConnectionStringDynamicReportGenerator(systemSerial, ConnectionString.TempDatabaseConnectionString, ConnectionString.IsLocalAnalyst);

            var loadingStatus = new UWSLoadingStatusService(newConnectionString);
            loadingStatus.DeleteUWSLoadingStatusFor(systemSerial, fileName);
        }

        public bool CheckDatabaseExistence(string systemSerial, string databasePostfix, bool mainDBLoad = false) {
            var databaseMapService = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
			string newConnectionString = "";
			//QT and DPA using this, since data only goes to temporary EC2
			if (!mainDBLoad) {
				newConnectionString = databaseMapService.GetConnectionStringDynamicReportGenerator(systemSerial, ConnectionString.TempDatabaseConnectionString, ConnectionString.IsLocalAnalyst, databasePostfix);
			}
			//Glacier using this, since the data will be loaded into AWS RDS
			else {
				newConnectionString = databaseMapService.GetConnectionStringFor(systemSerial);
				if (ConnectionString.IsLocalAnalyst) {
					newConnectionString = ConnectionString.TempDatabaseConnectionString.Replace("SERIALNUMBER", systemSerial + databasePostfix);
				}
			}

            var exists = databaseMapService.CheckDatabaseFor(newConnectionString);
            return exists;
        }
    }
}