using System;
using System.Collections.Generic;
using log4net;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.BusinessLogic.Util;


namespace RemoteAnalyst.Scheduler.Schedules {
    public class LoadingInfoCleaner {
        private static readonly ILog Log = LogManager.GetLogger("Cleaner");

        public void Timer_ElapsedHourly(object source, System.Timers.ElapsedEventArgs e) {
            RunAnalyzeTables();
            int currHour = DateTime.Now.Hour;

            if (currHour.Equals(1)) {
                StartCleanUp();
            }
        }

        public void RunAnalyzeTables()
        {
            var systemService = new System_tblService(ConnectionString.ConnectionStringDB);
            var activeSystemList = systemService.GetActiveSystemFor();
            foreach (string systemSerial in activeSystemList) {
#if DEBUG
                if (systemSerial != "080984") continue;
#endif
                int timeZoneIndex = systemService.GetTimeZoneFor(systemSerial);
                var localTime = TimeZoneInformation.ToLocalTime(timeZoneIndex, DateTime.Now);

                Log.InfoFormat("System: {0}", systemSerial);
                Log.InfoFormat("System's Time: {0}", timeZoneIndex);
                Log.InfoFormat("localTime: {0}", localTime);
                
                /*
                 * Run Analyze Table at noon - customer system time since
                 * it is "half" way through the daily loads
                 */
#if DEBUG
                localTime = new DateTime(2023,3,7,12,0,0);
#endif
                if (localTime.Hour == 12) {
                    var databaseMappingService = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
                    var connectionStringSystem = databaseMappingService.GetConnectionStringFor(systemSerial);
                    var databaseCheck = new Database(connectionStringSystem);
                    var databaseName = Helper.FindKeyName(connectionStringSystem, "DATABASE");
                    var tablePattern = systemSerial + "_%_" + localTime.Year + "_" + localTime.Month + "_" + localTime.Day;
                    List<string> tables = databaseCheck.GetTablesFromSchema(databaseName, tablePattern);
                    var sysData = new DataTableService(connectionStringSystem);
                    foreach (string tableName in tables) { 
                        sysData.RunCommandFor("ANALYZE TABLE " + tableName);
                    }
                }
            }
        }
        public void StartCleanUp() {
            try {
                Log.Info("Started daily clean up of LoadingInfo table");
                var retentionDays = GetLoadingInfoRetentionDays();
                DeleteLoadingInfoOlderThanXDaysAgo(retentionDays);
                Log.Info("Completed daily clean up of LoadingInfo table");
            } catch (Exception ex) {
                Log.ErrorFormat("Error Cleaning up LoadingInfo table {0}", ex);
            }

        }

        private int GetLoadingInfoRetentionDays() {
            Log.Info("Retrieving LoadingInfoRetentionDays");
            var RAInfoService = new RAInfoService(ConnectionString.ConnectionStringDB);
            var loadingInfoRetentionDays = Convert.ToInt32(RAInfoService.GetQueryValueFor("LoadingInfoRetentionDays"));
            Log.InfoFormat("Returned LoadingInfoRetentionDays with value: {0}", loadingInfoRetentionDays);
            return loadingInfoRetentionDays;
        }

        private void DeleteLoadingInfoOlderThanXDaysAgo(int loadingInfoRetentionDays) {
            Log.Info("Deleting rows in LoadingInfo table");
            var loadingInfoService = new LoadingInfoService(ConnectionString.ConnectionStringDB);
            loadingInfoService.DeleteLoadingInfoOlderThanXDaysAgo(loadingInfoRetentionDays);
            Log.InfoFormat("Rows succesfully deleted for uploadedtime < {0}", DateTime.Now.AddDays(-1 * loadingInfoRetentionDays).ToString("yyyy-MM-dd 00:00:00"));
        }
    }
}
