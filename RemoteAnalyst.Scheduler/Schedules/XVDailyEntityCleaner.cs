using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using log4net;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.Scheduler.Schedules {
    public class XVDailyEntityCleaner {
        private static readonly ILog Log = LogManager.GetLogger("Cleaner");

        public void Timer_ElapsedHourly(object source, System.Timers.ElapsedEventArgs e) {
            StartXVEntityCleaner();
        }

        public void StartXVEntityCleaner() {

            Log.Info("Starting XVEntityCleaner");

            System_tblService systemTableService = new System_tblService(ConnectionString.ConnectionStringDB);
            XVDailyEntityCleanerService xvDailyEntityCleanerService = new XVDailyEntityCleanerService(ConnectionString.ConnectionStringDB);
            var systemInformationList = xvDailyEntityCleanerService.GetAllSystemInformation();
            var currTime = DateTime.Now;
            foreach (var system in systemInformationList) {

#if (!DEBUG)
                if (LocalHourConvertedToSystemHourEqualsSpecifiedTime(currTime, system.TimeZone, 1)) {
#endif
#if (DEBUG)
                if (system.SystemSerial == "123456") { 
#endif
                    try {
                        Log.InfoFormat("Cleaning System with SystemSerial: {0}", system.SystemSerial);
                        var retentionDays = systemTableService.GetRetentionDayFor(system.SystemSerial);
                        RemoveXVDailyTablesOlderThanXDays(retentionDays, system.ConnectionString);
                        Log.InfoFormat("Finished Cleaning system with SystemSerial: {0}", system.SystemSerial);
                        Log.InfoFormat("Retention Days: {0}", retentionDays);
                    } catch (Exception ex) {
                        Log.ErrorFormat("An Error Occurred Cleaning System with SystemSerial: {0}, error {1}",
                            system.SystemSerial, ex);
                    }

                }
            }
            Log.Info("Finished Hourly Clean Up");
        }

        public void RemoveXVDailyTablesOlderThanXDays(int retentionDays, string systemConnectionString) {
            XVDailyEntityCleanerService xvDailyEntityCleanerService = new XVDailyEntityCleanerService(systemConnectionString);
            xvDailyEntityCleanerService.RemoveXVDailyTablesOlderThanXDays(retentionDays);
        }

        public bool LocalHourConvertedToSystemHourEqualsSpecifiedTime(DateTime currTime, int timeZoneIndex, int desiredHourToRun) {
            string timeZoneName = TimeZoneIndexConverter.ConvertIndexToName(timeZoneIndex);

            TimeZoneInfo est = TimeZoneInfo.FindSystemTimeZoneById(timeZoneName);
            DateTime targetTime = TimeZoneInfo.ConvertTime(currTime, est);

            if (targetTime.Hour == desiredHourToRun) return true;
            else return false;
        }
    }
}
