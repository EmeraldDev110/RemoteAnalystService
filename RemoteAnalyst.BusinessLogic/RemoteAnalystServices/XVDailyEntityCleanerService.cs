using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class XVDailyEntityCleanerService {

        private readonly string _connectionString;
        public XVDailyEntityCleanerService(string connectionString) {
            _connectionString = connectionString;
        }

        public List<XVDailyEntityCleanerView> GetAllSystemInformation() {
            XVDailyEntityCleanerRepository xvDailyEntityCleaner = new XVDailyEntityCleanerRepository(_connectionString);
            DataTable table = xvDailyEntityCleaner.GetAllSystemInformation();

            var systemList = new List<XVDailyEntityCleanerView>();
            foreach(DataRow row in table.Rows) {
                systemList.Add(new XVDailyEntityCleanerView {
                    SystemName = row["SystemName"].ToString(),
                    SystemSerial = row["SystemSerial"].ToString(),
                    TimeZone = Convert.ToInt32(row["TimeZone"]),
                    ConnectionString = row["MySQLConnectionString"].ToString()
                });
            }

            return systemList;
        }

        public void RemoveXVDailyTablesOlderThanXDays(int retentionDays) {
            XVDailyEntityCleanerRepository xvDailyEntityCleaner = new XVDailyEntityCleanerRepository(_connectionString);
            DataTable currentXVTableList = xvDailyEntityCleaner.GetXVDailyTables(retentionDays);


            foreach(DataRow row in currentXVTableList.Rows) {
                var tableName = row["table_name"].ToString();
                if (PastRetentionDay(retentionDays, tableName)) {
                    xvDailyEntityCleaner.DeleteXVDailyTableByName(tableName);
                }

            }
        }

        private bool PastRetentionDay(int retentionDays, string tableName) {
            var tableData = tableName.Split('_');
            var year = Convert.ToInt32(tableData[5]);
            var month = Convert.ToInt32(tableData[6]);
            var day = Convert.ToInt32(tableData[7]);

            DateTime currDate = DateTime.Now;
            DateTime tableDate = new DateTime(year, month, day);
            DateTime compareDate = currDate.AddDays(-1 * retentionDays);
            
            if (tableDate < compareDate) {
                return true;
            }
            return false;
        }
    }
}
