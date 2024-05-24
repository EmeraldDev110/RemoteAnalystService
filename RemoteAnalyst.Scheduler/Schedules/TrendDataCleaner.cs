using System;
using System.Collections.Generic;
using System.IO;
using log4net;
using RemoteAnalyst.AWS.Infrastructure;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.Scheduler.Schedules
{
    /// <summary>
    /// TrendDataCleaner deletes all the trend data that's over two years.
    /// </summary>
    internal class TrendDataCleaner
    {
        private static readonly ILog Log = LogManager.GetLogger("Cleaner");
        /// <summary>
        /// Timer_Elapsed is a event that gets call by Scheduler to start the schedule task.
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="e">Timer ElapsedEventArgs</param>
        public void Timer_Elapsed(object source, System.Timers.ElapsedEventArgs e) {
            int currHour = DateTime.Now.Hour;

            if (currHour.Equals(3)) {
                CleanTrendData();
            }
        }

        /// <summary>
        /// CleanTrendData cleans data from DailySysUnrated, DailyAppUnrated, SystemInterval, ProcessInterval, AlertSummary, CpuAlert, DiskAlert, DiskFileAlert, ProcessAlert, and TMFAlert
        /// </summary>
        public void CleanTrendData() {
            try {
                DateTime currentDateTime = DateTime.Today;
                var service = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
                IDictionary<string, string> connections = service.GetAllConnectionStringFor();
                var systemTblService = new System_tblService(ConnectionString.ConnectionStringDB);
                    List<string> expiredSystem = systemTblService.GetExpiredSystemFor(ConnectionString.IsLocalAnalyst);
                //kv is systemSerial - connectionString
                foreach (KeyValuePair<string, string> kv in connections) {
                    try {
#if (DEBUG)
                        if (kv.Key != "078578") continue;

#endif
                            if (expiredSystem.Contains(kv.Key))
                            {
                                continue;   // Skip cleanup logic for expired systems. Should drop the database manually
                            }
                        if (kv.Value.Length > 0) {
                            try {
                                var dailySysUnratedService = new DailySysUnratedService(kv.Value);
                                var dailyAppUnratedService = new DailyAppUnratedService(kv.Value);
                                var systemIntervalService = new SystemIntervalService(kv.Value);
                                var processIntervalService = new ProcessIntervalService(kv.Value);
                                var alertSummaryService = new AlertSummaryService(kv.Value);
                                var cpuAlertService = new CpuAlertService(kv.Value);
                                var diskAlertService = new DiskAlertService(kv.Value);
                                var diskFileAlertService = new DiskFileAlertService(kv.Value);
                                var processAlertService = new ProcessAlertService(kv.Value);
                                var trendCleanerService = new TrendCleanerService(kv.Value);
                                int trendMonths = systemTblService.GetTrendMonthsFor(kv.Key);
                                DateTime oldDate = currentDateTime.AddMonths(-trendMonths);

                                dailySysUnratedService.DeleteDataFor(oldDate);
                                dailyAppUnratedService.DeleteDataFor(oldDate);
                                systemIntervalService.DeleteDataFor(oldDate);
                                processIntervalService.DeleteDataFor(oldDate);

                                alertSummaryService.DeleteDataFor(oldDate);
                                cpuAlertService.DeleteDataFor(oldDate);
                                diskAlertService.DeleteDataFor(oldDate);
                                diskFileAlertService.DeleteDataFor(oldDate);
                                processAlertService.DeleteDataFor(oldDate);

                                trendCleanerService.DeleteDataFor("TrendApplicationInterval", "Interval", oldDate);
                                trendCleanerService.DeleteDataFor("TrendApplicationHourly", "Hour", oldDate);
                                trendCleanerService.DeleteDataFor("TrendCpuInterval", "Interval", oldDate);
                                trendCleanerService.DeleteDataFor("TrendCpuHourly", "Hour", oldDate);
                                trendCleanerService.DeleteDataFor("TrendDiskInterval", "Interval", oldDate);
                                trendCleanerService.DeleteDataFor("TrendDiskHourly", "Hour", oldDate);
                                trendCleanerService.DeleteDataFor("TrendExpandInterval", "Interval", oldDate);
                                trendCleanerService.DeleteDataFor("TrendExpandHourly", "Hour", oldDate);
                                trendCleanerService.DeleteDataFor("TrendHiLo", "DataDate", oldDate);
                                trendCleanerService.DeleteDataFor("TrendIpuInterval", "Interval", oldDate);
                                trendCleanerService.DeleteDataFor("TrendIpuHourly", "Hour", oldDate);
                                trendCleanerService.DeleteDataFor("TrendPathwayHourly", "Interval", oldDate);
                                trendCleanerService.DeleteDataFor("TrendProcessInterval", "Interval", oldDate);
                                trendCleanerService.DeleteDataFor("TrendProcessHourly", "Hour", oldDate);
                                trendCleanerService.DeleteDataFor("TrendProgramInterval", "Interval", oldDate);
                                trendCleanerService.DeleteDataFor("TrendProgramHourly", "Hour", oldDate);
                                trendCleanerService.DeleteDataFor("TrendTCPProcessInterval", "Interval", oldDate);
                                trendCleanerService.DeleteDataFor("TrendTCPProcessHourly", "Hour", oldDate);
                                trendCleanerService.DeleteDataFor("TrendTCPSubnetInterval", "Interval", oldDate);
                                trendCleanerService.DeleteDataFor("TrendTCPSubnetHourly", "Hour", oldDate);
                                trendCleanerService.DeleteDataFor("TrendTmfInterval", "Interval", oldDate);
                                trendCleanerService.DeleteDataFor("TrendTmfHourly", "Hour", oldDate);
                                trendCleanerService.DeleteDataFor("TrendWalkthrough", "FromTimeStamp", oldDate);

                            }
                            catch (Exception ex) {
                                Log.ErrorFormat("TrendDataCleaner Error: System: {0}, {1}", 
                                                    kv.Key, ex);
                            }
                        }
                    }
                    catch (Exception ex) {
                        Log.ErrorFormat("TrendDataCleaner Error: System: {0}, {1}",
                                            kv.Key, ex);
                    }
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("TrendDataCleaner Error (2): Error: {0}", ex);
                if (!ConnectionString.IsLocalAnalyst) {
                    var amazon = new AmazonOperations();
                    amazon.WriteErrorQueue("TrendDataCleaner Error: " + ex.Message);
                }
            }
            finally {
                ConnectionString.TaskCounter--;
            }
        }
    }
}
