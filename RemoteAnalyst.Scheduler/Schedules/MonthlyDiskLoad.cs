using System;
using System.Collections.Generic;
using log4net;
using RemoteAnalyst.AWS.Infrastructure;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.Scheduler.Schedules
{
    /// <summary>
    /// Once a month, MonthlyDiskLoad gets daily disk data, avg it out, and load it to monthly table.
    /// </summary>
    internal class MonthlyDiskLoad {
        private static readonly ILog Log = LogManager.GetLogger("MonthlyTrendLoad");

        /// <summary>
        /// Timer_Elapsed is a event that gets call by Scheduler to start the schedule task.
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="e">Timer ElapsedEventArgs</param>
        public void Timer_Elapsed(object source, System.Timers.ElapsedEventArgs e) {
            int currDay = DateTime.Now.Day;
            int currHour = DateTime.Now.Hour;

            if (currDay.Equals(5) && currHour.Equals(3)) {
                LoadMonthlyDiskData();
            }
        }

        /// <summary>
        /// LoadMonthlyDiskData gets daily disk data, avg it out, and load it to monthly table.
        /// </summary>
        internal void LoadMonthlyDiskData()
        {
            try
            {
                var sysInfo = new System_tblService(ConnectionString.ConnectionStringDB);
                List<string> expiredSystem = sysInfo.GetExpiredSystemFor(ConnectionString.IsLocalAnalyst);
                    
                var dbService = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
                IDictionary<string, string> connections = dbService.GetAllConnectionStringFor();
                    
                foreach (KeyValuePair<string, string> kv in connections) {
                    if (!kv.Value.Contains("RemoteAnalystdbSPAM")) {
                        var service = new DailyDiskService(kv.Value);
                        IList<DailyDiskView> disks = service.GetDiskNamesFor();

                        const int startDate = -1;
                        const int endDate = 0;
                        var dailyDiskService = new DailyDiskService(kv.Value);
                        var monthlyDiskService = new MonthlyDiskService(kv.Value);
                        var diskInfoService = new DiskInfoService(kv.Value);

                        foreach (DailyDiskView view in disks) {
                            try {
                                if (!expiredSystem.Contains(view.SystemSerial)) {
                                    for (int i = startDate; i < endDate; i++) {
                                        double deltaMB = 0;
                                        double deltaPercent = 0;

                                        //Checking if rows exist for the month, else inserting new row
                                        //Start						
                                        DateTime tempDate = DateTime.Now.AddMonths(i);
                                        tempDate = tempDate.AddDays(-tempDate.Day + 1).Date;

                                        bool exists = monthlyDiskService.CheckDataFor(view.SystemSerial, tempDate,
                                            view.DiskName);
                                        if (!exists) {
                                            //Checks if there is a data in DailyDisk Table.
                                            bool dailyExists = dailyDiskService.CheckDataFor(view.SystemSerial,
                                                view.DiskName, tempDate.Month, tempDate.Year);
                                            if (dailyExists) {
                                                //Insert New Row for the given date	(month)	
                                                monthlyDiskService.InsertNewDataFor(view.SystemSerial, tempDate,
                                                    view.DiskName);
                                            }
                                        }

                                        //Getting UsedGB for First Date within that month															
                                        double firstDayGB = dailyDiskService.GetUsedGBFor(view.SystemSerial, view.DiskName,
                                            tempDate.Month, tempDate.Year, 1);

                                        //Getting UsedGB for Last Date within that month	
                                        double lastDayGB = dailyDiskService.GetUsedGBFor(view.SystemSerial, view.DiskName,
                                            tempDate.Month, tempDate.Year, 0);

                                        //Getting Averaged Used GB for Month
                                        double avgUsedGB = dailyDiskService.GetAveragedUsedGBFor(view.SystemSerial,
                                            view.DiskName,
                                            tempDate.Month, tempDate.Year);

                                        //Getting Previous Months AvgUsedGB Value
                                        double prevAvgUsedGB = dailyDiskService.GetAveragedUsedGBFor(view.SystemSerial,
                                            view.DiskName, tempDate.AddMonths(-1).Month, tempDate.AddMonths(-1).Year);

                                        //Getting Disk Capacity from DISKINFO table
                                        double capacityGB = diskInfoService.GetAveragedUsedGBFor(view.SystemSerial,
                                            view.DiskName);

                                        if (prevAvgUsedGB > 0) {
                                            deltaMB = (avgUsedGB - prevAvgUsedGB) * 1024;
                                            deltaPercent = (deltaMB / (capacityGB * 1024)) * 100;
                                        }

                                        //Updating the values here
                                        monthlyDiskService.UpdateDataFor(view.SystemSerial, view.DiskName, tempDate,
                                            firstDayGB, lastDayGB, avgUsedGB, deltaMB, deltaPercent);
                                    }
                                }
                            }
                            catch (Exception ex) {
                                Log.ErrorFormat("System: {0}, MonthlyDiskLoad Error: {1}", kv.Key, ex);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("MonthlyDiskLoad Error: {0}", ex);
                    
                if (!ConnectionString.IsLocalAnalyst) {
                    var amazon = new AmazonOperations();
                    amazon.WriteErrorQueue("MonthlyDiskLoad Error: " + ex.Message);
                }
            }
            finally
            {
                ConnectionString.TaskCounter--;
            }
           
        }
    }
}