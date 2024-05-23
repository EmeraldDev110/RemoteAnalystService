using System;
using System.Text;
using RemoteAnalyst.Repository.Concrete.Model;

namespace RemoteAnalyst.BusinessLogic.ModelService {
    public class ConsolidatedAlertService {
        private readonly string _connectionString;
        private readonly string _connectionStringSystem;

        public ConsolidatedAlertService(string connectionString, string connectionStringSystem) {
            _connectionString = connectionString;
            _connectionStringSystem = connectionStringSystem;
        }

        public bool CheckAlertFor(DateTime fromTime, DateTime toTime, string systemSerial, int customerID, Enums.Alert.Alerts severity) {
            var alerts = new ConsolidatedAlerts(_connectionString, _connectionStringSystem);
            bool isExists = alerts.CheckAlert(fromTime, toTime, systemSerial, customerID, (int) severity);

            return isExists;
        }
    }
}