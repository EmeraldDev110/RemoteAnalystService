using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using RemoteAnalyst.AWS.Infrastructure;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using System.IO;
using RemoteAnalyst.BusinessLogic.Util;
using log4net;
using System.Web.UI.WebControls;

namespace RemoteAnalyst.Scheduler.Schedules
{
    /// <summary>
    /// Once a month, MonthlyTrendLoad gets daily system and application trend data, avg it out, and load it to monthly table.
    /// </summary>
    internal class MonthlyTrendLoad
    {
        private static readonly ILog Log = LogManager.GetLogger("MonthlyTrendLoad");
        /// <summary>
        /// Timer_Elapsed is a event that gets call by Scheduler to start the schedule task.
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="e">Timer ElapsedEventArgs</param>
        public void Timer_Elapsed(object source, System.Timers.ElapsedEventArgs e) {
            int currDay = DateTime.Now.Day;
            int currHour = DateTime.Now.Hour;

            if (currDay.Equals(5) && currHour.Equals(2)) {
                LoadMonthlyTrendData();
            }
        }

        /// <summary>
        /// LoadMonthlyTrendData gets system trend data, avg it out, and load it to monthly table.
        /// </summary>
        public void LoadMonthlyTrendData() {
            try
            {
                var arrSums = new double[24];
                var arrCounts = new int[24];

                Log.InfoFormat("ConnectionString.ConnectionStringDB: {0}", CleanupSPAM.RemovePassword(ConnectionString.ConnectionStringDB));

                var sysInfo = new System_tblService(ConnectionString.ConnectionStringDB);
                var expiredSystem = sysInfo.GetExpiredSystemFor(ConnectionString.IsLocalAnalyst);

                Log.Info("Get Per System Connection String");
                    
                var dbService = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
                IDictionary<string, string> connections = dbService.GetAllConnectionStringFor();

                foreach (KeyValuePair<string, string> kv in connections) {
                    Log.InfoFormat("kv.key: {0}, value {1}", kv.Key, kv.Value);
                        
                    if (!kv.Value.Contains("RemoteAnalystdbSPAM")) {
                        //DailySysUnratedService dailySysUnratedService = new DailySysUnratedService(ConnectionString.ConnectionStringTrend);
                        var dailySysUnratedService = new DailySysUnratedService(kv.Value);
                        IList<DailySysUnratedView> sysData = dailySysUnratedService.GetAllSystemDataFor();

                        var monthlySysUnratedService = new MonthlySysUnratedService(kv.Value);
                            
                        foreach (DailySysUnratedView view in sysData) {
                            //Don't get expire system.
                            try {
                                if (!expiredSystem.Contains(view.SystemSerial)) {
                                    for (int i = -2; i < 0; i++) {
                                        //Checking if rows exist for the month, else inserting new row
                                        double sumval = 0;
                                        DateTime tempDate = DateTime.Now.AddMonths(i);
                                        tempDate = tempDate.AddDays(-tempDate.Day + 1).Date;
                                        bool exists = monthlySysUnratedService.CheckDataFor(view.SystemSerial, tempDate,
                                            view.AttributeId, view.Object);
                                        if (!exists) {
                                            monthlySysUnratedService.InsertNewDataFor(view.SystemSerial, tempDate,
                                                view.AttributeId, view.Object);
                                        }
                                        int k;
                                        for (k = 0; k < 24; k++) {
                                            arrSums[k] = 0;
                                            arrCounts[k] = 0;
                                        }

                                        int numhours = 0;

                                        DataTable hourlyData = dailySysUnratedService.GetHourlyDataFor(view.SystemSerial,
                                            view.AttributeId, view.Object, tempDate.Month, tempDate.Year);

                                        foreach (DataRow dr in hourlyData.Rows) {
                                            for (k = 0; k < 24; k++) {
                                                if (dr["Hour" + Convert.ToString(k)] != DBNull.Value) {
                                                    sumval += Math.Round(Convert.ToDouble(dr["Hour" + Convert.ToString(k)]), 2);
                                                    arrSums[k] += Math.Round(
                                                        Convert.ToDouble(dr["Hour" + Convert.ToString(k)]), 2);
                                                    arrCounts[k]++;
                                                    numhours++;
                                                }
                                            }
                                        }

                                        DataTable hourlyDataMonth = monthlySysUnratedService.GetHourlyDataFor(
                                            view.SystemSerial, view.AttributeId, view.Object, tempDate);

                                        if (hourlyDataMonth.Rows.Count != 0) {
                                            var hourValues = new StringBuilder();
                                            double avg = 0;
                                            int avgcount = 0;
                                            int peakhour = -1;

                                            for (k = 0; k < 24; k++) {
                                                if (arrCounts[k] != 0) {
                                                    //hourlyDataMonth.Rows[0]["Hour" + Convert.ToString(k)] = arrSums[k] / arrCounts[k];
                                                    hourValues.Append("Hour" + Convert.ToString(k) + "=" + arrSums[k] / arrCounts[k] + ",");
                                                    avg += arrSums[k] / arrCounts[k];
                                                    avgcount++;
                                                    if (peakhour != -1) {
                                                        if (!hourlyDataMonth.Rows[0].IsNull("Hour" + Convert.ToString(peakhour)) &&
                                                            !hourlyDataMonth.Rows[0].IsNull("Hour" + Convert.ToString(k))) {
                                                            if (
                                                                Math.Round(
                                                                    Convert.ToDouble(hourlyDataMonth.Rows[0]["Hour" + Convert.ToString(peakhour)]), 2) <
                                                                Math.Round(Convert.ToDouble(hourlyDataMonth.Rows[0]["Hour" + Convert.ToString(k)]), 2))
                                                                peakhour = k;
                                                        }
                                                        else
                                                            peakhour = k;
                                                    }
                                                    else {
                                                        peakhour = k;
                                                    }
                                                }
                                            }
                                            if (avg != 0) {
                                                /*hourlyDataMonth.Rows[0]["SumVal"] = sumval;
                                            hourlyDataMonth.Rows[0]["AvgVal"] = avg / avgcount;
                                            hourlyDataMonth.Rows[0]["PeakHour"] = peakhour;
                                            hourlyDataMonth.Rows[0]["NumHours"] = numhours;*/

                                                //Remove last ',' from hourValues.
                                                hourValues = hourValues.Remove(hourValues.Length - 1, 1);

                                                monthlySysUnratedService.UpdateDataFor(sumval, Convert.ToDouble((avg / avgcount)),
                                                    peakhour, numhours, view.SystemSerial, view.AttributeId, view.Object,
                                                    tempDate, hourValues.ToString());
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex) {
                                Log.ErrorFormat("System {0}, MonthlyTrendLoad Error: {1}", view.SystemSerial, ex);
                            }
                        }
                    }
                }
                LoadMonthlyApplicationTrendData();
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("MonthlyTrendLoad Error: {0}", ex);

                if (!ConnectionString.IsLocalAnalyst) {
                    var amazon = new AmazonOperations();
                    amazon.WriteErrorQueue("MonthlyTrendLoad Error: " + ex.Message);
                }
            }
            finally
            {
                ConnectionString.TaskCounter--;
            }
            
        }

        /// <summary>
        /// LoadMonthlyApplicationTrendData gets application trend data, avg it out, and load it to monthly table.
        /// </summary>
        private void LoadMonthlyApplicationTrendData()
        {
            var arrSums = new double[24];
            var arrCounts = new int[24];

            //Application Unrated
            Log.Info("Application Unrated Data");
            

            var sysInfo = new System_tblService(ConnectionString.ConnectionStringDB);
            var expiredSystem = sysInfo.GetExpiredSystemFor(ConnectionString.IsLocalAnalyst);

            //Ryan Ji
            //IR 6466
            //Move tables to System DB

            //DailyAppUnratedService dailyAppUnratedService = new DailyAppUnratedService(ConnectionString.ConnectionStringTrend);
            var databaseMappingService = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
            IDictionary<string, string> systemList = databaseMappingService.GetAllConnectionStringFor();

            foreach (KeyValuePair<string, string> kvp in systemList)
            {
                string systemSerial = kvp.Key;
                string connectionSystem = kvp.Value;
                var dailyAppUnratedService = new DailyAppUnratedService(connectionSystem);
                IList<DailySysUnratedView> appData = dailyAppUnratedService.GetAllApplicationDataFor();

                var monthlyAppUnratedService = new MonthlyAppUnratedService(connectionSystem);


                //DailyAppUnratedService dailyAppUnratedService = new DailyAppUnratedService(ConnectionString.ConnectionStringSPAM);
                //IList<DailySysUnratedView> appData = dailyAppUnratedService.GetAllApplicationDataFor();

                //MonthlyAppUnratedService monthlyAppUnratedService = new MonthlyAppUnratedService(ConnectionString.ConnectionStringTrend);

                foreach (DailySysUnratedView view in appData)
                {
                    //Don't get expire system.
                    if (!expiredSystem.Contains(systemSerial))
                    {
                        for (int i = -2; i < 0; i++)
                        {
                            //Checking if rows exist for the month, else inserting new row
                            double sumval = 0;
                            DateTime tempDate = DateTime.Now.AddMonths(i);
                            tempDate = tempDate.AddDays(-tempDate.Day + 1).Date;

                            bool exists = monthlyAppUnratedService.CheckDataFor(view.SystemSerial, tempDate,
                                view.AttributeId, view.Object);
                            if (!exists)
                            {
                                monthlyAppUnratedService.InsertNewDataFor(view.SystemSerial, tempDate, view.AttributeId,
                                    view.Object);
                            }

                            int k;
                            for (k = 0; k < 24; k++)
                            {
                                arrSums[k] = 0;
                                arrCounts[k] = 0;
                            }

                            int numhours = 0;
                            DataTable hourlyData = dailyAppUnratedService.GetHourlyDataFor(view.SystemSerial,
                                view.AttributeId, view.Object, tempDate.Month, tempDate.Year);

                            foreach (DataRow dr in hourlyData.Rows)
                            {
                                for (k = 0; k < 24; k++)
                                {
                                    if (dr["Hour" + Convert.ToString(k)] != DBNull.Value)
                                    {
                                        sumval += Math.Round(Convert.ToDouble(dr["Hour" + Convert.ToString(k)]), 2);
                                        arrSums[k] += Math.Round(Convert.ToDouble(dr["Hour" + Convert.ToString(k)]), 2);
                                        arrCounts[k]++;
                                        numhours++;
                                    }
                                }
                            }

                            DataTable monthlyAppData = monthlyAppUnratedService.GetHourlyDataFor(view.SystemSerial,
                                view.AttributeId, view.Object, tempDate);


                            if (monthlyAppData.Rows.Count != 0) {
                                var hourValues = new StringBuilder();
                                double avg = 0;
                                int avgcount = 0;
                                int peakhour = -1;

                                for (k = 0; k < 24; k++)
                                {
                                    if (arrCounts[k] != 0)
                                    {
                                        //monthlyAppData.Rows[0]["Hour" + Convert.ToString(k)] = arrSums[k]/arrCounts[k];
                                        hourValues.Append("Hour" + Convert.ToString(k) + "=" + arrSums[k]/arrCounts[k] + ",");
                                        avg += arrSums[k]/arrCounts[k];
                                        avgcount++;
                                        /*if (peakhour == -1 ||
                                            Math.Round(
                                                Convert.ToDouble(
                                                    monthlyAppData.Rows[0]["Hour" + Convert.ToString(peakhour)]), 2) <
                                            Math.Round(
                                                Convert.ToDouble(monthlyAppData.Rows[0]["Hour" + Convert.ToString(k)]),
                                                2))
                                            peakhour = k;*/
                                        if (peakhour != -1) {
                                            if (!monthlyAppData.Rows[0].IsNull("Hour" + Convert.ToString(peakhour)) &&
                                                !monthlyAppData.Rows[0].IsNull("Hour" + Convert.ToString(k))) {
                                                if (
                                                    Math.Round(
                                                        Convert.ToDouble(monthlyAppData.Rows[0]["Hour" + Convert.ToString(peakhour)]), 2) <
                                                    Math.Round(Convert.ToDouble(monthlyAppData.Rows[0]["Hour" + Convert.ToString(k)]), 2))
                                                    peakhour = k;
                                            }
                                            else
                                                peakhour = k;
                                        }
                                        else {
                                            peakhour = k;
                                        }
                                    }
                                }
                                if (avg != 0)
                                {
                                    /*monthlyAppData.Rows[0]["SumVal"] = sumval;
                                    monthlyAppData.Rows[0]["AvgVal"] = avg / avgcount;
                                    monthlyAppData.Rows[0]["PeakHour"] = peakhour;
                                    monthlyAppData.Rows[0]["NumHours"] = numhours;*/
                                    //Remove last ',' from hourValues.
                                    hourValues = hourValues.Remove(hourValues.Length - 1, 1);

                                    monthlyAppUnratedService.UpdateDataFor(sumval, Convert.ToDouble(avg/avgcount),
                                        peakhour, numhours, view.SystemSerial, view.AttributeId, view.Object, tempDate, hourValues.ToString());
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}