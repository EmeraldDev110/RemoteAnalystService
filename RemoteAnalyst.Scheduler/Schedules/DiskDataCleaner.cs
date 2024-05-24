using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using log4net;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.Scheduler.Schedules {
    internal class DiskDataCleaner {
        private static readonly ILog Log = LogManager.GetLogger("Cleaner");

        public void Timer_Elapsed(object source, ElapsedEventArgs e) {
            int currHour = BusinessLogic.Util.Helper.RoundUp(DateTime.Now, TimeSpan.FromMinutes(15)).Hour;
            //int currHour = DateTime.Now.Hour;

            if (currHour.Equals(3)) {
                CleanDiskData();
            }
        }

        public void CleanDiskData() {
            DateTime currentDateTime = DateTime.Today;
            var service = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
            IDictionary<string, string> connections = service.GetAllConnectionStringFor();
            var systemTblService = new System_tblService(ConnectionString.ConnectionStringDB);
                List<string> expiredSystem = systemTblService.GetExpiredSystemFor(ConnectionString.IsLocalAnalyst);
            foreach (KeyValuePair<string, string> kv in connections) {
                try {
                        if (expiredSystem.Contains(kv.Key))
                        {
                            continue;   // Skip cleanup logic for expired systems. Should drop the database manually
                        }
                    var dailyDiskService = new DailyDiskService(kv.Value);
                    var monthlyDiskService = new MonthlyDiskService(kv.Value);

                    int trendMonths = systemTblService.GetTrendMonthsFor(kv.Key);
                    DateTime oldDate = currentDateTime.AddMonths(-trendMonths);

                    dailyDiskService.DeleteDataFor(oldDate);
                    monthlyDiskService.DeleteDataFor(oldDate);
                }
                catch (Exception ex) {
                    Log.Error("*******************************************************");
                    Log.ErrorFormat("CleanDiskData System: {0}, {1}", kv.Key, ex);
                }
            }
        }
    }
}