using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using log4net;
using Pathway.Core.Repositories;
using Pathway.Core.Services;
using RemoteAnalyst.AWS.Infrastructure;
using RemoteAnalyst.AWS.Queue;
using RemoteAnalyst.AWS.S3;
using RemoteAnalyst.AWS.SNS;
using RemoteAnalyst.BusinessLogic.BLL;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.Enums;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.Trigger.JobPool;

namespace RemoteAnalyst.Scheduler.Schedules {
    /// <summary>
    /// ReportDispatcher checks for the schedule report and write to ReportQ.
    /// </summary>
    internal class ReportDispatcher {
        private static readonly ILog Log = LogManager.GetLogger("ReportDispatcher");
        /// <summary>
        /// Timer_Elapsed is a event that gets call by Scheduler to start the schedule task.
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="e">Timer ElapsedEventArgs</param>
        public void Timer_Elapsed(object source, ElapsedEventArgs e) {
            int currHour = BusinessLogic.Util.Helper.RoundUp(DateTime.Now, TimeSpan.FromMinutes(15)).Hour;
            //int currHour = DateTime.Now.Hour;

            if (currHour.Equals(1)) {
                CheckAllReports();
            }
        }

        /// <summary>
        /// CheckAllReports calls CheckDispatchTrendReports, CheckDispatchStorageReports, CheckDispatchQTReports, and CheckDispatchDPAReports
        /// </summary>
        internal void CheckAllReports() {
            var sysInfo = new System_tblService(ConnectionString.ConnectionStringDB);
            List<string> expiredSystem = sysInfo.GetExpiredSystemFor(ConnectionString.IsLocalAnalyst);

            try {
                Log.Info("***********************************************************************");
                Log.Info("Running Daily Trend Report Dispatcher");              

                Log.Info("*****calling CheckDispatchTrendReports");
                CheckDispatchTrendReports(DateTime.Now, expiredSystem);

                Log.Info("*****calling CheckDispatchPathwayReports");
                CheckDispatchPathwayReports(DateTime.Now, expiredSystem);

                Log.Info("*****calling CheckDispatchStorageReports");
                CheckDispatchStorageReports(DateTime.Now, expiredSystem);

                Log.Info("*****calling CheckDispatchQTReports");
                
                CheckDispatchQTReports(DateTime.Now, expiredSystem);

                Log.Info("*****calling CheckDispatchDPAReports");
                
                CheckDispatchDPAReports(DateTime.Now, expiredSystem);

                Log.Info("*****calling CheckDispatchQNMReports");
                
                CheckDispatchQNMReports(DateTime.Now, expiredSystem);
                
                Log.Info("*****calling CheckDispatchTPSReports");
                
                CheckDispatchTPSReports(DateTime.Now, expiredSystem);
            }
            catch (Exception ex) {
                Log.ErrorFormat("ReportDispatcher Error: {0}", ex);
                
                if (!ConnectionString.IsLocalAnalyst) {
                    var amazon = new AmazonOperations();
                    amazon.WriteErrorQueue("ReportDispatcher Error: " + ex.Message);
                }
                else {
                    var email = new EmailToSupport(ConnectionString.AdvisorEmail, ConnectionString.SupportEmail,
                        ConnectionString.WebSite,
                        ConnectionString.EmailServer, ConnectionString.EmailPort, ConnectionString.EmailUser,
                        ConnectionString.EmailPassword, ConnectionString.EmailAuthentication,
                        ConnectionString.SystemLocation, ConnectionString.ServerPath,
                        ConnectionString.EmailIsSSL,
                        ConnectionString.IsLocalAnalyst, ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                    email.SendLocalAnalystErrorMessageEmail("Scheduler - ReportDispatcher.cs", ex.Message, LicenseService.GetProductName(ConnectionString.ConnectionStringDB));
                }
            }
            finally {
                ConnectionString.TaskCounter--;
                Log.Info("ReportDispatcher Done");
            }
        }

        /// <summary>
        /// Checks for the scheduled Trend reports. Write to ReportQ when there is trend reports to generate.
        /// </summary>
        /// <param name="currDateTime">Current Date Time</param>
        /// <param name="expiredSystem">List of Expired Systems</param>
        private void CheckDispatchTrendReports(DateTime currDateTime, List<string> expiredSystem) {
            //Get Trend Report. ReportTypeID 2 is Trend Report.
            try {
                #region Get Trend Reports.

                var deliveryScheduleService = new DeliveryScheduleService(ConnectionString.ConnectionStringTrend);
                IList<DeliveryScheduleView> scheduleView = deliveryScheduleService.GetSchdulesFor(2);

                var sbTrends = new StringBuilder();
                foreach (DeliveryScheduleView view in scheduleView) {

                    //Don't get expire system.
                    if (!expiredSystem.Contains(view.SystemSerial)) {
                        bool bCreateReport = false;

                        if (view.FrequencyName.Equals("Daily")) {
                            bCreateReport = true;
                        }

                        if (view.FrequencyName.Equals("Weekly")) {
                            if (Convert.ToInt32(currDateTime.DayOfWeek) == view.SendDay) {
                                bCreateReport = true;
                            }
                        }

                        if (view.FrequencyName.Equals("Monthly")) {
                            int iDate = view.SendDay;
                            if (iDate == 29) {
                                iDate = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month);
                            }
                            if (Convert.ToInt32(currDateTime.Day) == iDate) {
                                bCreateReport = true;
                            }
                        }

                        if (view.FrequencyName.Equals("Quarterly")) {
                            int iDay = view.SendDay;

                            if ((Convert.ToInt32(currDateTime.Day) == iDay) &&
                                (Convert.ToInt32(currDateTime.Month) == 1 || Convert.ToInt32(currDateTime.Month) == 4 ||
                                 Convert.ToInt32(currDateTime.Month) == 7 || Convert.ToInt32(currDateTime.Month) == 10)) {
                                bCreateReport = true;
                            }
                        }

                        if (view.FrequencyName.Equals("Annually")) {
                            int iDate = view.SendDay;
                            if (iDate == 29) {
                                iDate = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month);
                            }
                            if (Convert.ToInt32(currDateTime.Day) == iDate &&
                                Convert.ToInt32(currDateTime.Month) == view.SendMonth) {
                                bCreateReport = true;
                            }
                        }

                        if (bCreateReport) {
                            IList<DeliveryScheduleView> schduleData =
                                deliveryScheduleService.GetSchduleDataFor(view.DeliveryID);
                            if (schduleData.Count > 0) {
                                int reportID = schduleData.First().TrendReportID;
                                string systemserial = schduleData.First().SystemSerial;
                                char periodType = schduleData.First().PeriodType;
                                int periodCount = schduleData.First().PeriodCount;
                                string title = schduleData.First().Title;

                                //Calculating Start and End Dates for Reports
                                DateTime startdate = DateTime.Now.Date;
                                DateTime enddate = DateTime.Now.Date;

                                if (periodType == 'D') {
                                    startdate = enddate.AddDays(-periodCount);
                                }
                                if (periodType == 'M') {
                                    startdate = enddate.AddMonths(-periodCount);
                                    startdate = startdate.AddDays(-startdate.Day + 1);
                                    enddate = startdate.AddMonths(periodCount);
                                    enddate = enddate.AddDays(-1);
                                }
                                if (periodType == 'Q') {
                                    if (enddate.Month == 2 || enddate.Month == 5 || enddate.Month == 8 ||
                                        enddate.Month == 11) {
                                        enddate = enddate.AddMonths(-1);
                                    }
                                    if (enddate.Month == 3 || enddate.Month == 6 || enddate.Month == 9 ||
                                        enddate.Month == 12) {
                                        enddate = enddate.AddMonths(-2);
                                    }
                                    startdate = enddate.AddMonths(-(periodCount * 3));
                                    startdate = startdate.AddDays(-startdate.Day + 1);
                                    enddate = startdate.AddMonths(periodCount * 3);
                                    enddate = enddate.AddDays(-1);
                                }
                                if (periodType == 'Y') {
                                    startdate = enddate.AddYears(-periodCount);
                                }

                                //since we have 250k size limit on Queue, put this info to S3.
                                sbTrends.Append("Trend|" + view.DeliveryID + "|" + reportID + "|" + systemserial + "|" +
                                                startdate + "|" + enddate + "|" + title + "|true\n");
                            }
                        }
                    }
                }

                if (sbTrends.Length > 0) {
                    if (ConnectionString.IsLocalAnalyst)
                    {
                        var triggerInsert = new TriggerService(ConnectionString.ConnectionStringDB);
                        triggerInsert.InsertFor("", (int)TriggerType.Type.ReportGeneratorStatic, sbTrends.ToString());
                        /* Dead code based on previous flow. May need to research more
                            //New logic for trigger file.
                            var trigger = new Triggers();
                            trigger.WriteReportMessage(ConnectionString.WatchFolder, (int) Report.Types.Trend, "", sbTrends.ToString());
                        */
                    }
                    else
                    {
                        string fileName = "trend_" + DateTime.Now.Ticks + ".txt";
                        //Write to S3 and insert to Queue.
                        var s3 = new AmazonS3(ConnectionString.S3WorkSpace);

                        var sqs = new AmazonSQS();
                        int retry = 0;
                        do
                        {
                            try
                            {
                                s3.WriteToS3(fileName, sbTrends.ToString());

                                string urlQueue = "";
                                if (!string.IsNullOrEmpty(ConnectionString.SQSReport))
                                    urlQueue = sqs.GetAmazonSQSUrl(ConnectionString.SQSReport);

                                if (urlQueue.Length > 0)
                                {
                                    sqs.WriteMessage(urlQueue, fileName);
                                    retry = 5;
                                }
                                else
                                {
                                    //Retry after duration.
                                    Thread.Sleep(AmazonError.GetTimeoutDuration(retry));
                                    retry++;
                                    if (retry == 5)
                                    {
                                        AmazonError.WriteLog("urlQueue is empty", "Amazon.cs: CheckDispathTrendReport",
                                            ConnectionString.AdvisorEmail,
                                            ConnectionString.SupportEmail,
                                            ConnectionString.WebSite,
                                            ConnectionString.EmailServer,
                                            ConnectionString.EmailPort,
                                            ConnectionString.EmailUser,
                                            ConnectionString.EmailPassword,
                                            ConnectionString.EmailAuthentication,
                                            ConnectionString.SystemLocation, ConnectionString.ServerPath,
                                            ConnectionString.EmailIsSSL,
                                            ConnectionString.IsLocalAnalyst, ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                //Retry after duration.
                                Thread.Sleep(AmazonError.GetTimeoutDuration(retry));
                                retry++;
                                if (retry == 5)
                                {
                                    AmazonError.WriteLog(ex, "Amazon.cs: CheckDispathTrendReport",
                                        ConnectionString.AdvisorEmail,
                                        ConnectionString.SupportEmail,
                                        ConnectionString.WebSite,
                                        ConnectionString.EmailServer,
                                        ConnectionString.EmailPort,
                                        ConnectionString.EmailUser,
                                        ConnectionString.EmailPassword,
                                        ConnectionString.EmailAuthentication,
                                        ConnectionString.SystemLocation, ConnectionString.ServerPath,
                                        ConnectionString.EmailIsSSL,
                                        ConnectionString.IsLocalAnalyst, ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                                }
                            }
                        } while (retry < 5);
                    }
                }

                #endregion
            }
            catch (Exception ex) {
                Log.ErrorFormat("Trend Report Error: {0}", ex);
                
            }

            GC.Collect();
        }

        /// <summary>
        /// Checks for the scheduled Pathway reports. Write to ReportQ when there is Pathway reports to generate.
        /// </summary>
        /// <param name="currDateTime">Current Date Time</param>
        /// <param name="expiredSystem">List of Expired Systems</param>
        private void CheckDispatchPathwayReports(DateTime currDateTime, List<string> expiredSystem) {
            //Get Trend Report. ReportTypeID 8 is Trend Report.
            try {
                #region Get Trend Reports.

                var deliveryScheduleService = new DeliveryScheduleService(ConnectionString.ConnectionStringTrend);
                IList<DeliveryScheduleView> scheduleView = deliveryScheduleService.GetSchdulesFor(8);

                foreach (DeliveryScheduleView view in scheduleView) {
                    //Don't get expire system.
                    if (!expiredSystem.Contains(view.SystemSerial)) {
                        bool bCreateReport = false;

                        if (view.FrequencyName.Equals("Daily")) {
                            bCreateReport = true;
                        }

                        if (view.FrequencyName.Equals("Weekly")) {
                            if (Convert.ToInt32(currDateTime.DayOfWeek) == view.SendDay) {
                                bCreateReport = true;
                            }
                        }

                        if (view.FrequencyName.Equals("Monthly")) {
                            int iDate = view.SendDay;
                            if (iDate == 29) {
                                iDate = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month);
                            }
                            if (Convert.ToInt32(currDateTime.Day) == iDate) {
                                bCreateReport = true;
                            }
                        }

                        if (view.FrequencyName.Equals("Quarterly")) {
                            int iDay = view.SendDay;

                            if ((Convert.ToInt32(currDateTime.Day) == iDay) &&
                                (Convert.ToInt32(currDateTime.Month) == 1 || Convert.ToInt32(currDateTime.Month) == 4 ||
                                 Convert.ToInt32(currDateTime.Month) == 7 || Convert.ToInt32(currDateTime.Month) == 10)) {
                                bCreateReport = true;
                            }
                        }

                        if (view.FrequencyName.Equals("Annually")) {
                            int iDate = view.SendDay;
                            if (iDate == 29) {
                                iDate = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month);
                            }
                            if (Convert.ToInt32(currDateTime.Day) == iDate &&
                                Convert.ToInt32(currDateTime.Month) == view.SendMonth) {
                                bCreateReport = true;
                            }
                        }

                        if (bCreateReport) {
                            IList<DeliveryScheduleView> schduleData = deliveryScheduleService.GetSchduleDataFor(view.DeliveryID);
                            if (schduleData.Count > 0) {
                                int reportID = schduleData.First().TrendReportID;
                                string systemserial = schduleData.First().SystemSerial;
                                char periodType = schduleData.First().PeriodType;
                                int periodCount = schduleData.First().PeriodCount;
                                string title = schduleData.First().Title;

                                //Calculating Start and End Dates for Reports
                                DateTime startdate = DateTime.Now.Date;
                                DateTime enddate = DateTime.Now.Date;

                                if (periodType == 'D') {
                                    startdate = enddate.AddDays(-periodCount);
                                }
                                else if (periodType == 'M') {
                                    startdate = enddate.AddMonths(-periodCount);
                                    startdate = startdate.AddDays(-startdate.Day + 1);
                                    enddate = startdate.AddMonths(periodCount);
                                    enddate = enddate.AddDays(-1);
                                }
                                else if (periodType == 'Q') {
                                    if (enddate.Month == 2 || enddate.Month == 5 || enddate.Month == 8 ||
                                        enddate.Month == 11) {
                                        enddate = enddate.AddMonths(-1);
                                    }
                                    if (enddate.Month == 3 || enddate.Month == 6 || enddate.Month == 9 ||
                                        enddate.Month == 12) {
                                        enddate = enddate.AddMonths(-2);
                                    }
                                    startdate = enddate.AddMonths(-(periodCount * 3));
                                    startdate = startdate.AddDays(-startdate.Day + 1);
                                    enddate = startdate.AddMonths(periodCount * 3);
                                    enddate = enddate.AddDays(-1);
                                }
                                else if (periodType == 'Y') {
                                    startdate = enddate.AddYears(-periodCount);
                                }

                                //Get Pathway Names.
                                string connectionString = ConnectionString.ConnectionStringSPAM;

                                var databaseMap = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
                                string newConnectionString = databaseMap.GetConnectionStringFor(systemserial);
                                if (newConnectionString.Length == 0) {
                                    newConnectionString = connectionString;
                                }
                                var pwyLists = new List<string>();
                                try {
                                    PvPwyListRepository pwyList = new PvPwyListRepository();
                                    pwyLists = pwyList.GetPathwayNames(startdate, enddate);
                                    if (pwyLists.Count == 0) {
                                        Log.Info("pwyLists is empty. Exit the function.");
                                        
                                        continue;
                                    }
                                }
                                catch (Exception ex) {
                                    Log.ErrorFormat("Get Pathway List Error: {0}", ex);
                                    
                                    continue;
                                }
                                string pathwayList = string.Join(",", pwyLists);
                                long interval;
                                //Get Interval.
                                try {

                                    Pathway.Core.Services.PvCollectsService collectService = new Pathway.Core.Services.PvCollectsService();
                                    interval = collectService.GetIntervalFor(startdate, enddate);
                                }
                                catch (Exception ex) {
                                    Log.ErrorFormat("Get Interval Error: {0}", ex);
                                    
                                    continue;
                                }

                                string type = reportID == 1 ? "Pathway" : "Alerts";

                                //Get emails.
                                var recepientListService = new RecepientListService(ConnectionString.ConnectionStringTrend);
                                IList<string> emails = recepientListService.GetEmailListFor(view.DeliveryID);
                                string emailstoSend = string.Join(",", emails);
                                
                                var reportDownloads = new ReportDownloadService(ConnectionString.ConnectionStringDB);
                                var reportDownloadId = reportDownloads.InsertNewReportFor(systemserial, startdate, enddate, (int)Schedule.Types.Pathway, 0);

                                string pathwayMessage = "Pathway|" + systemserial + "|" + startdate + "|" + enddate + "|" + pathwayList + "|" + emailstoSend + "|" + interval + "|" + type + "|" + reportDownloadId;

                                if (ConnectionString.IsLocalAnalyst)
                                {
                                    var triggerInsert = new TriggerService(ConnectionString.ConnectionStringDB);
                                    triggerInsert.InsertFor(view.SystemSerial, (int)TriggerType.Type.ReportGeneratorStatic, pathwayMessage);
                                }
                                else {
                                    var sqs = new AmazonSQS();
                                    int retry = 0;
                                    do {
                                        try {
                                            string urlQueue = "";
                                            if (!string.IsNullOrEmpty(ConnectionString.SQSReport))
                                                urlQueue = sqs.GetAmazonSQSUrl(ConnectionString.SQSReport);

                                            if (urlQueue.Length > 0) {
                                                sqs.WriteMessage(urlQueue, pathwayMessage);
                                                retry = 5;
                                            }
                                            else {
                                                //Retry after duration.
                                                Thread.Sleep(AmazonError.GetTimeoutDuration(retry));
                                                retry++;
                                                if (retry == 5) {
                                                    AmazonError.WriteLog("urlQueue is empty", "Amazon.cs: CheckDispatchPathwayReports",
                                                        ConnectionString.AdvisorEmail,
                                                        ConnectionString.SupportEmail,
                                                        ConnectionString.WebSite,
                                                        ConnectionString.EmailServer,
                                                        ConnectionString.EmailPort,
                                                        ConnectionString.EmailUser,
                                                        ConnectionString.EmailPassword,
                                                        ConnectionString.EmailAuthentication,
                                                        ConnectionString.SystemLocation, ConnectionString.ServerPath,
                                                        ConnectionString.EmailIsSSL,
                                                        ConnectionString.IsLocalAnalyst, ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                                                }
                                            }
                                        }
                                        catch (Exception ex) {
                                            //Retry after duration.
                                            Thread.Sleep(AmazonError.GetTimeoutDuration(retry));
                                            retry++;
                                            if (retry == 5) {
                                                AmazonError.WriteLog(ex, "Amazon.cs: CheckDispatchPathwayReports",
                                                    ConnectionString.AdvisorEmail,
                                                    ConnectionString.SupportEmail,
                                                    ConnectionString.WebSite,
                                                    ConnectionString.EmailServer,
                                                    ConnectionString.EmailPort,
                                                    ConnectionString.EmailUser,
                                                    ConnectionString.EmailPassword,
                                                    ConnectionString.EmailAuthentication,
                                                    ConnectionString.SystemLocation, ConnectionString.ServerPath,
                                                    ConnectionString.EmailIsSSL,
                                                    ConnectionString.IsLocalAnalyst, ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                                            }
                                        }
                                    } while (retry < 5);
                                }
                                /* Dead code, need to research 
                                        //string pathwayMessage = "Pathway\n" + systemserial + "\n" + startdate + "\n" + enddate + "\n" + pathwayList + "\n" + emailstoSend + "\n" + interval + "\n" + type;
                                        var trigger = new Triggers();
                                        trigger.WriteReportMessage(ConnectionString.WatchFolder, (int) Report.Types.Pathway, systemserial, pathwayMessage);
                                */
                            }
                        }
                    }
                }

                #endregion
            }
            catch (Exception ex) {
                Log.ErrorFormat("Pathway Report Error: {0}", ex);
                
            }

            GC.Collect();
        }

        /// <summary>
        /// Checks for the scheduled Storage reports. Write to ReportQ when there is trend reports to generate.
        /// </summary>
        /// <param name="currDateTime">Current Date Time</param>
        /// <param name="expiredSystem">List of Expired Systems</param>
        private void CheckDispatchStorageReports(DateTime currDateTime, List<string> expiredSystem) {
            //Get Storage Report. ReportTypeID 4 is Storage Report.
            try {
                #region Storage Report

                //*************************************************************************
                //Get Storage Reports.

                var deliveryScheduleService = new DeliveryScheduleService(ConnectionString.ConnectionStringTrend);
                IList<DeliveryScheduleView> scheduleView = deliveryScheduleService.GetSchdulesFor(4);

                foreach (DeliveryScheduleView view in scheduleView) {
                    if (!expiredSystem.Contains(view.SystemSerial)) {
                        bool bCreateReport = false;
                        //Check dates.
                        if (view.FrequencyName.Equals("Daily")) {
                            bCreateReport = true;
                        }
                        else if (view.FrequencyName.Equals("Weekly")) {
                            if (Convert.ToInt32(currDateTime.DayOfWeek) == view.SendDay) {
                                bCreateReport = true;
                            }
                        }
                        else if (view.FrequencyName.Equals("Monthly")) {
                            int iDate = view.SendDay;
                            if (iDate == 29) {
                                iDate = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month);
                            }
                            if (Convert.ToInt32(currDateTime.Day) == iDate) {
                                bCreateReport = true;
                            }
                        }
                        else if (view.FrequencyName.Equals("Quarterly")) {
                            int iDay = view.SendDay;

                            if ((Convert.ToInt32(currDateTime.Day) == iDay) &&
                                (Convert.ToInt32(currDateTime.Month) == 1 || Convert.ToInt32(currDateTime.Month) == 4 ||
                                 Convert.ToInt32(currDateTime.Month) == 7 || Convert.ToInt32(currDateTime.Month) == 10)) {
                                bCreateReport = true;
                            }
                        }
                        else if (view.FrequencyName.Equals("Annually")) {
                            int iDate = view.SendDay;
                            if (iDate == 29) {
                                iDate = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month);
                            }
                            if (Convert.ToInt32(currDateTime.Day) == iDate &&
                                Convert.ToInt32(currDateTime.Month) == view.SendMonth) {
                                bCreateReport = true;
                            }
                        }

                        if (bCreateReport) {
                            int deliveryID = view.DeliveryID;
                            string systemserial = view.SystemSerial;

                            //Check to see if capacity and capacity summary report exists on same systems.
                            var storageReportService = new StorageReportService(ConnectionString.ConnectionStringTrend);
                            //If capacity and capcaitySummary is true, don't send CapacitySummanry.
                            bool capacity = storageReportService.CheckCapacitiesFor(deliveryID);

                            IList<StorageReportView> storageReportView =
                                storageReportService.GetSchduleDataFor(deliveryID);

                            try {
                                foreach (StorageReportView storageView in storageReportView) {
                                    int storageID = storageView.StorageID;
                                    int customerID = storageView.CustomerID;

                                    if (storageID == 1) {
                                        int groupDisk = storageView.GroupDisk;
                                        int groupDiskID = storageView.GroupDiskID;
                                        //Write to Qeueue.
                                        string buildMessage = "StorageSchedule|" + deliveryID + "|" + storageID + "|" +
                                                              systemserial + "|" + customerID + "|" + groupDisk + "|" +
                                                              groupDiskID;
                                        if (ConnectionString.IsLocalAnalyst)
                                        {
                                            var triggerInsert = new TriggerService(ConnectionString.ConnectionStringDB);
                                            triggerInsert.InsertFor(view.SystemSerial, (int)TriggerType.Type.ReportGeneratorStatic, buildMessage);
                                        }
                                        else  {
                                            var sqs = new AmazonSQS();
                                            int retry = 0;
                                            do {
                                                try {
                                                    string urlQueue = "";
                                                    if (!string.IsNullOrEmpty(ConnectionString.SQSReport))
                                                        urlQueue = sqs.GetAmazonSQSUrl(ConnectionString.SQSReport);

                                                    if (urlQueue.Length > 0) {
                                                        sqs.WriteMessage(urlQueue, buildMessage);
                                                        retry = 5;
                                                    }
                                                    else {
                                                        //Retry after duration.
                                                        Thread.Sleep(AmazonError.GetTimeoutDuration(retry));
                                                        retry++;
                                                        if (retry == 5) {
                                                            AmazonError.WriteLog("urlQueue is empty", "Amazon.cs: CheckDispathStorageReport",
                                                                ConnectionString.AdvisorEmail,
                                                                ConnectionString.SupportEmail,
                                                                ConnectionString.WebSite,
                                                                ConnectionString.EmailServer,
                                                                ConnectionString.EmailPort,
                                                                ConnectionString.EmailUser,
                                                                ConnectionString.EmailPassword,
                                                                ConnectionString.EmailAuthentication,
                                                                ConnectionString.SystemLocation, ConnectionString.ServerPath,
                                                                ConnectionString.EmailIsSSL,
                                                                ConnectionString.IsLocalAnalyst, ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                                                        }
                                                    }
                                                }
                                                catch (Exception ex) {
                                                    //Retry after duration.
                                                    Thread.Sleep(AmazonError.GetTimeoutDuration(retry));
                                                    retry++;
                                                    if (retry == 5) {
                                                        AmazonError.WriteLog(ex, "Amazon.cs: CheckDispathStorageReport",
                                                            ConnectionString.AdvisorEmail,
                                                            ConnectionString.SupportEmail,
                                                            ConnectionString.WebSite,
                                                            ConnectionString.EmailServer,
                                                            ConnectionString.EmailPort,
                                                            ConnectionString.EmailUser,
                                                            ConnectionString.EmailPassword,
                                                            ConnectionString.EmailAuthentication,
                                                            ConnectionString.SystemLocation, ConnectionString.ServerPath,
                                                            ConnectionString.EmailIsSSL,
                                                            ConnectionString.IsLocalAnalyst, ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                                                    }
                                                }
                                            } while (retry < 5);
                                        }
                                        /* Dead code, need to research
                                        var trigger = new Triggers();
                                        trigger.WriteReportMessage(ConnectionString.WatchFolder, (int) Report.Types.Storage, systemserial, buildMessage);
                                        */
                                    }
                                    else if (storageID == 2) {
                                        //Write to Qeueue.
                                        string buildMessage = "StorageSchedule|" + deliveryID + "|" + storageID + "|" +
                                                              systemserial + "|" + customerID;
                                        if (ConnectionString.IsLocalAnalyst)
                                        {
                                            var triggerInsert = new TriggerService(ConnectionString.ConnectionStringDB);
                                            triggerInsert.InsertFor(view.SystemSerial, (int)TriggerType.Type.ReportGeneratorStatic, buildMessage);
                                        }
                                        else  {
                                            var sqs = new AmazonSQS();
                                            int retry = 0;
                                            do {
                                                try {
                                                    string urlQueue = "";
                                                    if (!string.IsNullOrEmpty(ConnectionString.SQSReport))
                                                        urlQueue = sqs.GetAmazonSQSUrl(ConnectionString.SQSReport);

                                                    if (urlQueue.Length > 0) {
                                                        sqs.WriteMessage(urlQueue, buildMessage);
                                                        retry = 5;
                                                    }
                                                    else {
                                                        //Retry after duration.
                                                        Thread.Sleep(AmazonError.GetTimeoutDuration(retry));
                                                        retry++;
                                                        if (retry == 5) {
                                                            AmazonError.WriteLog("urlQueue is empty", "Amazon.cs: CheckDispathStorageReport1",
                                                                ConnectionString.AdvisorEmail,
                                                                ConnectionString.SupportEmail,
                                                                ConnectionString.WebSite,
                                                                ConnectionString.EmailServer,
                                                                ConnectionString.EmailPort,
                                                                ConnectionString.EmailUser,
                                                                ConnectionString.EmailPassword,
                                                                ConnectionString.EmailAuthentication,
                                                                ConnectionString.SystemLocation, ConnectionString.ServerPath,
                                                                ConnectionString.EmailIsSSL,
                                                                ConnectionString.IsLocalAnalyst, ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                                                        }
                                                    }
                                                }
                                                catch (Exception ex) {
                                                    //Retry after duration.
                                                    Thread.Sleep(AmazonError.GetTimeoutDuration(retry));
                                                    retry++;
                                                    if (retry == 5) {
                                                        AmazonError.WriteLog(ex, "Amazon.cs: CheckDispathStorageReport1",
                                                            ConnectionString.AdvisorEmail,
                                                            ConnectionString.SupportEmail,
                                                            ConnectionString.WebSite,
                                                            ConnectionString.EmailServer,
                                                            ConnectionString.EmailPort,
                                                            ConnectionString.EmailUser,
                                                            ConnectionString.EmailPassword,
                                                            ConnectionString.EmailAuthentication,
                                                            ConnectionString.SystemLocation, ConnectionString.ServerPath,
                                                            ConnectionString.EmailIsSSL,
                                                            ConnectionString.IsLocalAnalyst, ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                                                    }
                                                }
                                            } while (retry < 5);
                                        }
                                        /* Dead code, need to research
                                                var trigger = new Triggers();
                                                trigger.WriteReportMessage(ConnectionString.WatchFolder, (int) Report.Types.Storage, systemserial, buildMessage);
                                        */
                                    }
                                    else if (storageID == 3) {
                                        if (!capacity) {
                                            //Write to Qeueue.
                                            string buildMessage = "StorageSchedule|" + deliveryID + "|" + storageID + "|" +
                                                                  systemserial;
                                            if (ConnectionString.IsLocalAnalyst)
                                            {
                                                var triggerInsert = new TriggerService(ConnectionString.ConnectionStringDB);
                                                triggerInsert.InsertFor(view.SystemSerial, (int)TriggerType.Type.ReportGeneratorStatic, buildMessage);
                                            }
                                            else  {
                                                var sqs = new AmazonSQS();
                                                int retry = 0;
                                                do {
                                                    try {
                                                        string urlQueue = "";
                                                        if (!string.IsNullOrEmpty(ConnectionString.SQSReport))
                                                            urlQueue = sqs.GetAmazonSQSUrl(ConnectionString.SQSReport);

                                                        if (urlQueue.Length > 0) {
                                                            sqs.WriteMessage(urlQueue, buildMessage);
                                                            retry = 5;
                                                        }
                                                        else {
                                                            //Retry after duration.
                                                            Thread.Sleep(AmazonError.GetTimeoutDuration(retry));
                                                            retry++;
                                                            if (retry == 5) {
                                                                AmazonError.WriteLog("urlQueue is empty", "Amazon.cs: CheckDispathStorageReport2",
                                                                    ConnectionString.AdvisorEmail,
                                                                    ConnectionString.SupportEmail,
                                                                    ConnectionString.WebSite,
                                                                    ConnectionString.EmailServer,
                                                                    ConnectionString.EmailPort,
                                                                    ConnectionString.EmailUser,
                                                                    ConnectionString.EmailPassword,
                                                                    ConnectionString.EmailAuthentication,
                                                                    ConnectionString.SystemLocation, ConnectionString.ServerPath,
                                                                    ConnectionString.EmailIsSSL,
                                                                    ConnectionString.IsLocalAnalyst, ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                                                            }
                                                        }
                                                    }
                                                    catch (Exception ex) {
                                                        //Retry after duration.
                                                        Thread.Sleep(AmazonError.GetTimeoutDuration(retry));
                                                        retry++;
                                                        if (retry == 5) {
                                                            AmazonError.WriteLog(ex, "Amazon.cs: CheckDispathStorageReport2",
                                                                ConnectionString.AdvisorEmail,
                                                                ConnectionString.SupportEmail,
                                                                ConnectionString.WebSite,
                                                                ConnectionString.EmailServer,
                                                                ConnectionString.EmailPort,
                                                                ConnectionString.EmailUser,
                                                                ConnectionString.EmailPassword,
                                                                ConnectionString.EmailAuthentication,
                                                                ConnectionString.SystemLocation, ConnectionString.ServerPath,
                                                                ConnectionString.EmailIsSSL,
                                                                ConnectionString.IsLocalAnalyst, ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                                                        }
                                                    }
                                                } while (retry < 5);
                                            }
                                            /* Dead code, need to research
                                            var trigger = new Triggers();
                                            trigger.WriteReportMessage(ConnectionString.WatchFolder, (int) Report.Types.Storage, systemserial, buildMessage);
                                            */
                                        }
                                    }
                                    else {
                                        Log.InfoFormat("StorageID Error ID: {0}", storageID);
                                        
                                    }
                                }
                            }
                            catch (Exception ex) {
                                Log.ErrorFormat("Error: {0}", ex);
                                
                            }
                        }
                    }
                }

                #endregion
            }
            catch (Exception ex) {
                Log.ErrorFormat("Storage Report Error: {0}", ex);
                
            }
        }

        private DayOfWeek MapDayOfWeek(int weekday) {
            var selectedWeekday = DayOfWeek.Friday;

            switch (weekday) {
                case 1:
                    selectedWeekday = DayOfWeek.Sunday;
                    break;
                case 2:
                    selectedWeekday = DayOfWeek.Monday;
                    break;
                case 3:
                    selectedWeekday = DayOfWeek.Tuesday;
                    break;
                case 4:
                    selectedWeekday = DayOfWeek.Wednesday;
                    break;
                case 5:
                    selectedWeekday = DayOfWeek.Thursday;
                    break;
                case 6:
                    selectedWeekday = DayOfWeek.Friday;
                    break;
                case 7:
                    selectedWeekday = DayOfWeek.Saturday;
                    break;
            }

            return selectedWeekday;
        }

        /// <summary>
        /// Checks for the scheduled QT reports. Write to ReportQ when there is trend reports to generate.
        /// </summary>
        /// <param name="currDateTime">Current Date Time</param>
        /// <param name="expiredSystem">List of Expired Systems</param>
        private void CheckDispatchQTReports(DateTime currDateTime, List<string> expiredSystem) {
            //Get QT Report. ReportTypeID 7 is QT Report.
            try {
                #region QT Report

                var startdate = new DateTime();
                var enddate = new DateTime();

                DateTime currentDateTime = DateTime.Today;

                var deliveryScheduleService = new DeliveryScheduleService(ConnectionString.ConnectionStringTrend);
                IList<DeliveryScheduleView> deliveryScheduleView = deliveryScheduleService.GetQTSchduleFor();

                foreach (DeliveryScheduleView view in deliveryScheduleView) {
                    currDateTime = DateTime.Now;
                    if (!expiredSystem.Contains(view.SystemSerial)) {
                        bool bCreateReport = false;
                        int processDay;
                        int startTime;
                        int stopTime;
                        if (view.FrequencyName.Equals("Weekly")) {
                            if (Convert.ToInt32(currentDateTime.DayOfWeek) == view.SendDay) {
                                bCreateReport = true;

                                if (view.StartTime != -1 && view.StopTime != -1) {
                                    processDay = view.ProcessDate;
                                    int currentDay = view.SendDay;

                                    if (currentDay > processDay) {
                                        currDateTime = currentDateTime.AddDays((currentDay - processDay) * -1);
                                    }
                                    else {
                                        //currDateTime = currentDateTime.AddDays((7 - processDay) * -1);
                                        currDateTime = currentDateTime.AddDays(processDay - currentDay - 7);
                                    }
                                    startTime = view.StartTime;
                                    stopTime = view.StopTime;
                                    startdate = new DateTime(currDateTime.Year, currDateTime.Month, currDateTime.Day,
                                        startTime, 0, 0);

                                    if (stopTime == 24) {
                                        enddate = new DateTime(currDateTime.Year, currDateTime.Month, currDateTime.Day,
                                            23, 59, 59);
                                    }
                                    else {
                                        enddate = new DateTime(currDateTime.Year, currDateTime.Month, currDateTime.Day,
                                            stopTime, 0, 0);
                                    }
                                }
                            }
                        }
                        else if (view.FrequencyName.Equals("Monthly")) {
                            //Check for new data.
                            if (view.IsWeekdays) {
                                var date = new DateTime(currentDateTime.Year, currentDateTime.Month, 1);
                                var dayOfWeek = MapDayOfWeek(view.FrequencyWeekday);
                                //Check Frequency.
                                var newValue = date.AddDays((dayOfWeek < date.DayOfWeek ? 7 : 0) + dayOfWeek - date.DayOfWeek);
                                var finalDate = newValue.AddDays((view.FrequencyMonthCount - 1) * 7);

                                if (currentDateTime.Date == finalDate.Date) {
                                    bCreateReport = true;
                                    //Get Report Date Time.
                                    if (view.IsReportDataLast) {
                                        var reportDayOfWeek = MapDayOfWeek(view.ReportDataWeekday);
                                        var reportDate = DateTime.Now.AddDays(-7);
                                        int delta = reportDayOfWeek - reportDate.DayOfWeek;
                                        var newReportDate = reportDate.AddDays(delta);
                                        startTime = view.StartTime;
                                        stopTime = view.StopTime;

                                        startdate = new DateTime(newReportDate.Year, newReportDate.Month, newReportDate.Day, startTime, 0, 0);
                                        if (stopTime == 24) {
                                            enddate = new DateTime(newReportDate.Year, newReportDate.Month, newReportDate.Day, 23, 59, 59);
                                        }
                                        else {
                                            enddate = new DateTime(newReportDate.Year, newReportDate.Month, newReportDate.Day, stopTime, 0, 0);
                                        }

                                    }
                                    else {
                                        if (view.StartTime != -1 && view.StopTime != -1) {
                                            processDay = view.ProcessDate;
                                            startTime = view.StartTime;
                                            stopTime = view.StopTime;

                                            startdate = new DateTime(currDateTime.Year, currDateTime.Month, processDay, startTime, 0, 0);
                                            if (stopTime == 24) {
                                                enddate = new DateTime(currDateTime.Year, currDateTime.Month, processDay, 23, 59, 59);
                                            }
                                            else {
                                                enddate = new DateTime(currDateTime.Year, currDateTime.Month, processDay, stopTime, 0, 0);
                                            }
                                        }
                                    }
                                }
                            }
                            else {
                                int iDate = view.SendDay;
                                if (iDate == 29) {
                                    iDate = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month);
                                }

                                if (Convert.ToInt32(currentDateTime.Day) == iDate) {
                                    bCreateReport = true;
                                    if (view.StartTime != -1 && view.StopTime != -1) {
                                        processDay = view.ProcessDate;
                                        startTime = view.StartTime;
                                        stopTime = view.StopTime;

                                        startdate = new DateTime(currDateTime.Year, currDateTime.Month, processDay, startTime, 0, 0);
                                        if (stopTime == 24) {
                                            enddate = new DateTime(currDateTime.Year, currDateTime.Month, processDay, 23, 59, 59);
                                        }
                                        else {
                                            enddate = new DateTime(currDateTime.Year, currDateTime.Month, processDay, stopTime, 0, 0);
                                        }
                                    }
                                }
                            }
                        }

                        if (bCreateReport) {
                            var systemTblService = new System_tblService(ConnectionString.ConnectionStringDB);
                            string systemName = systemTblService.GetSystemNameFor(view.SystemSerial);

                            //Get emails.
                            var recepientListService =
                                new RecepientListService(
                                    ConnectionString.ConnectionStringTrend);
                            IList<string> emails = recepientListService.GetEmailListFor(view.DeliveryID);
                            var emailstoSend = new StringBuilder();
                            foreach (string s in emails) {
                                emailstoSend.Append(s + ",");
                            }
                            if (emailstoSend.Length > 0) {
                                emailstoSend = emailstoSend.Remove(emailstoSend.Length - 1, 1);
                            }

                            //Insert entry on ReportDownloads.
                            var reportDownloads = new ReportDownloadService(ConnectionString.ConnectionStringDB);
                            var reportDownloadId = reportDownloads.InsertNewReportFor(view.SystemSerial, startdate, enddate, 3, 0);

							System_tblService system_TblService = new System_tblService(ConnectionString.ConnectionStringDB);
							string companyName = system_TblService.GetCompanyNameFor(view.SystemSerial);

							//Write to Qeueue.
							string buildMessage = "QT|" + reportDownloadId + "|" + view.SystemSerial + "|" + systemName + "|" + startdate + "|" +
                                                  enddate + "|" +
                                                  view.QLenAlert + "," + view.FOpenAlert + "," + view.FLockWaitAlert +
                                                  "|" +
                                                  view.MinProcBusy + "|" + view.ExSourceCPU + "|" + view.ExDestCPU + "|" +
                                                  view.ExProgName + "|" + emailstoSend + "|-1|false|true|S|" + companyName + @"|S QT \" + systemName + " " + companyName + "|250";

							Log.InfoFormat("Message: {0}",buildMessage);

                            if (ConnectionString.IsLocalAnalyst)
                            {
                                var triggerInsert = new TriggerService(ConnectionString.ConnectionStringDB);
                                triggerInsert.InsertFor(view.SystemSerial, (int)TriggerType.Type.ReportGenerator, buildMessage);
                            }
                            else {
                                #region New scheduler for report generation

                                    IAmazonSNS amazonSns = new AmazonSNS();
                                    string subjectMessage = ReportGeneratorStatus.GetEnumDescription(ReportGeneratorStatus.StatusMessage.QTOrder);
                                    amazonSns.SendToTopic(subjectMessage, buildMessage, ConnectionString.SNSProdTriggerReportARN);
                                    //Make sure lambda won't timed out
                                    Thread.Sleep(60000);

                                    #endregion
                            }
                            /*  Dead code, need to research
                                var trigger = new Triggers();
                                trigger.WriteReportMessage(ConnectionString.WatchFolder, (int) Report.Types.QT, view.SystemSerial, buildMessage);
                            */
                        }
                    }
                }
            }
                #endregion

            catch (Exception ex) {
                Log.ErrorFormat("QT Report Error: {0}", ex);
                
            }
        }

        /// <summary>
        /// Checks for the scheduled DPA reports. Write to ReportQ when there is trend reports to generate.
        /// </summary>
        /// <param name="currDateTime">Current Date Time</param>
        /// <param name="expiredSystem">List of Expired Systems</param>
        private void CheckDispatchDPAReports(DateTime currDateTime, List<string> expiredSystem) {
            //Get iReport Report. ReportTypeID 6 is iReport Report.
            try {
                #region DPA Report

                var startdate = new DateTime();
                var enddate = new DateTime();
                DateTime currentDateTime = DateTime.Today;

                var deliveryScheduleService = new DeliveryScheduleService(ConnectionString.ConnectionStringTrend);
                IList<DeliveryScheduleView> deliveryScheduleView = deliveryScheduleService.GetDPASchduleFor();

                foreach (DeliveryScheduleView view in deliveryScheduleView) {
                    currDateTime = DateTime.Now;
                    if (!expiredSystem.Contains(view.SystemSerial)) {
                        bool bCreateReport = false;
                        int processDay;
                        int startTime;
                        int stopTime;
                        if (view.FrequencyName.Equals("Weekly")) {
                            if (Convert.ToInt32(currentDateTime.DayOfWeek) == view.SendDay) {
                                bCreateReport = true;

                                if (view.StartTime != -1 && view.StopTime != -1) {
                                    processDay = view.ProcessDate;
                                    int currentDay = view.SendDay;

                                    currDateTime = currentDay > processDay
                                        ? currentDateTime.AddDays((currentDay - processDay) * -1)
                                        : currentDateTime.AddDays(processDay - currentDay - 7);
                                    startTime = view.StartTime;
                                    stopTime = view.StopTime;
                                    startdate = new DateTime(currDateTime.Year, currDateTime.Month, currDateTime.Day,
                                        startTime, 0, 0);

                                    if (stopTime == 24) {
                                        enddate = new DateTime(currDateTime.Year, currDateTime.Month, currDateTime.Day,
                                            23, 59, 59);
                                    }
                                    else {
                                        enddate = new DateTime(currDateTime.Year, currDateTime.Month, currDateTime.Day,
                                            stopTime, 0, 0);
                                    }
                                    if (enddate < startdate) {
                                        enddate = enddate.AddDays(1);
                                    }
                                }
                            }
                        }
                        else if (view.FrequencyName.Equals("Monthly")) {
                            if (view.IsWeekdays) {
                                var date = new DateTime(currentDateTime.Year, currentDateTime.Month, 1);
                                var dayOfWeek = MapDayOfWeek(view.FrequencyWeekday);
                                //Check Frequency.
                                var newValue = date.AddDays((dayOfWeek < date.DayOfWeek ? 7 : 0) + dayOfWeek - date.DayOfWeek);
                                var finalDate = newValue.AddDays((view.FrequencyMonthCount - 1)*7);

                                if (currentDateTime.Date == finalDate.Date) {
                                    bCreateReport = true;
                                    //Get Report Date Time.
                                    if (view.IsReportDataLast) {
                                        var reportDayOfWeek = MapDayOfWeek(view.ReportDataWeekday);
                                        var reportDate = DateTime.Now.AddDays(-7);
                                        int delta = reportDayOfWeek - reportDate.DayOfWeek;
                                        var newReportDate = reportDate.AddDays(delta);
                                        startTime = view.StartTime;
                                        stopTime = view.StopTime;

                                        startdate = new DateTime(newReportDate.Year, newReportDate.Month, newReportDate.Day, startTime, 0, 0);
                                        if (stopTime == 24) {
                                            enddate = new DateTime(newReportDate.Year, newReportDate.Month, newReportDate.Day, 23, 59, 59);
                                        }
                                        else {
                                            enddate = new DateTime(newReportDate.Year, newReportDate.Month, newReportDate.Day, stopTime, 0, 0);
                                        }

                                    }
                                    else {
                                        if (view.StartTime != -1 && view.StopTime != -1) {
                                            processDay = view.ProcessDate;
                                            startTime = view.StartTime;
                                            stopTime = view.StopTime;

                                            startdate = new DateTime(currDateTime.Year, currDateTime.Month, processDay, startTime, 0, 0);
                                            if (stopTime == 24) {
                                                enddate = new DateTime(currDateTime.Year, currDateTime.Month, processDay, 23, 59, 59);
                                            }
                                            else {
                                                enddate = new DateTime(currDateTime.Year, currDateTime.Month, processDay, stopTime, 0, 0);
                                            }
                                        }
                                    }
                                }
                            }
                            else {
                                int iDate = view.SendDay;
                                if (iDate == 29) {
                                    iDate = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month);
                                }
                                if (Convert.ToInt32(currentDateTime.Day) == iDate) {
                                    bCreateReport = true;
                                    if (view.StartTime != -1 && view.StopTime != -1) {
                                        processDay = view.ProcessDate;
                                        startTime = view.StartTime;
                                        stopTime = view.StopTime;

                                        startdate = new DateTime(currDateTime.Year, currDateTime.Month, processDay, startTime, 0, 0);
                                        if (stopTime == 24) {
                                            enddate = new DateTime(currDateTime.Year, currDateTime.Month, processDay, 23, 59, 59);
                                        }
                                        else {
                                            enddate = new DateTime(currDateTime.Year, currDateTime.Month, processDay, stopTime, 0, 0);
                                        }
                                    }
                                }
                            }
                        }

                        if (bCreateReport) {
                            string systemserial = view.SystemSerial;
                            int groupID = view.TrendReportID;

                            //Get emails.
                            var recepientListService =
                                new RecepientListService(
                                    ConnectionString.ConnectionStringTrend);
                            IList<string> emails = recepientListService.GetEmailListFor(view.DeliveryID);
                            var emailstoSend = new StringBuilder();
                            foreach (string s in emails) {
                                emailstoSend.Append(s + ",");
                            }
                            if (emailstoSend.Length > 0) {
                                emailstoSend = emailstoSend.Remove(emailstoSend.Length - 1, 1);
                            }

                            //Get Reports.
                            var reportGroupDetailService =
                                new ReportGroupDetailService(ConnectionString.ConnectionStringTrend);
                            IList<int> reports = reportGroupDetailService.GetReportIDsFor(groupID);

                            var report = new StringBuilder();
                            foreach (int i in reports) {
                                report.Append(i + ",");
                            }
                            if (report.Length > 0) {
                                report = report.Remove(report.Length - 1, 1);
                            }

                            //Get Charts.
                            var chartGroupDetailService =
                                new ChartGroupDetailService(ConnectionString.ConnectionStringTrend);
                            IList<int> charts = chartGroupDetailService.GetChartIDsFor(groupID);

                            var chart = new StringBuilder();
                            foreach (int i in charts) {
                                chart.Append(i + ",");
                            }
                            if (chart.Length > 0) {
                                chart = chart.Remove(chart.Length - 1, 1);
                            }


                            //Insert entry on ReportDownloads.
                            var reportDownloads = new ReportDownloadService(ConnectionString.ConnectionStringDB);
                            var reportDownloadId = reportDownloads.InsertNewReportFor(view.SystemSerial, startdate, enddate, 4, 0);

							System_tblService system_TblService = new System_tblService(ConnectionString.ConnectionStringDB);
							string systemName = system_TblService.GetSystemNameFor(view.SystemSerial);
							string companyName = system_TblService.GetCompanyNameFor(view.SystemSerial);

							//NOTE iReport does not exist anymore.
							//Write to Qeueue. if parameter is empty, it uses deaulft values.
							string buildMessage = "DPA|" + reportDownloadId + "|" + view.SystemSerial + "|" + startdate + "|" + enddate + "||" +
                                                  report + "|" + chart + "||" + emailstoSend + "|3000|2003|false|true|S|" + companyName + @"|S DPA \" + systemName + " " + companyName + "|250";
							Log.InfoFormat("Message: {0}",buildMessage);

                            if (ConnectionString.IsLocalAnalyst)
                            {
                                var triggerInsert = new TriggerService(ConnectionString.ConnectionStringDB);
                                triggerInsert.InsertFor(view.SystemSerial, (int)TriggerType.Type.ReportGenerator, buildMessage);
                            }
                            else
                            {
                                #region New scheduler for report generation
                                IAmazonSNS amazonSns = new AmazonSNS();
                                string subjectMessage = ReportGeneratorStatus.GetEnumDescription(ReportGeneratorStatus.StatusMessage.DPAOrder);
                                amazonSns.SendToTopic(subjectMessage, buildMessage, ConnectionString.SNSProdTriggerReportARN);
                                //Make sure lambda won't timed out
                                Thread.Sleep(60000);
                                #endregion
                            }
                            /*  Dead code, need to research
                                var trigger = new Triggers();
                                trigger.WriteReportMessage(ConnectionString.WatchFolder, (int) Report.Types.DPA, view.SystemSerial, buildMessage);
                            */
                        }
                    }
                }

                #endregion
            }
            catch (Exception ex) {
                Log.ErrorFormat("DPA Report Error: {0}", ex);
                
            }
        }

        private void CheckDispatchQNMReports(DateTime currDateTime, List<string> expiredSystem) {
            try {
                #region Get Trend Reports.

                var deliveryScheduleService = new DeliveryScheduleService(ConnectionString.ConnectionStringTrend);
                IList<DeliveryScheduleView> scheduleView = deliveryScheduleService.GetSchdulesFor(9);

                Log.InfoFormat("Schedule count: {0}",scheduleView.Count);
                

                var sbTrends = new StringBuilder();
                foreach (DeliveryScheduleView view in scheduleView) {
                    //Don't get expire system.
                    if (!expiredSystem.Contains(view.SystemSerial)) {
                        bool bCreateReport = false;

                        if (view.FrequencyName.Equals("Daily")) {
                            bCreateReport = true;
                        }

                        if (view.FrequencyName.Equals("Weekly")) {
                            if (Convert.ToInt32(currDateTime.DayOfWeek) == view.SendDay) {
                                bCreateReport = true;
                            }
                        }

                        if (view.FrequencyName.Equals("Monthly")) {
                            int iDate = view.SendDay;
                            if (iDate == 29) {
                                iDate = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month);
                            }
                            if (Convert.ToInt32(currDateTime.Day) == iDate) {
                                bCreateReport = true;
                            }
                        }

                        if (view.FrequencyName.Equals("Quarterly")) {
                            int iDay = view.SendDay;

                            if ((Convert.ToInt32(currDateTime.Day) == iDay) &&
                                (Convert.ToInt32(currDateTime.Month) == 1 || Convert.ToInt32(currDateTime.Month) == 4 ||
                                 Convert.ToInt32(currDateTime.Month) == 7 || Convert.ToInt32(currDateTime.Month) == 10)) {
                                bCreateReport = true;
                            }
                        }

                        if (view.FrequencyName.Equals("Annually")) {
                            int iDate = view.SendDay;
                            if (iDate == 29) {
                                iDate = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month);
                            }
                            if (Convert.ToInt32(currDateTime.Day) == iDate &&
                                Convert.ToInt32(currDateTime.Month) == view.SendMonth) {
                                bCreateReport = true;
                            }
                        }

                        if (bCreateReport) {
                            IList<DeliveryScheduleView> schduleData = deliveryScheduleService.GetSchduleDataFor(view.DeliveryID);
                            Log.InfoFormat("ScheduleData count: {0}",schduleData.Count);
                            
                            if (schduleData.Count > 0) {
                                int reportID = schduleData.First().TrendReportID;
                                string systemserial = schduleData.First().SystemSerial;
                                char periodType = schduleData.First().PeriodType;
                                int periodCount = schduleData.First().PeriodCount;
                                string title = schduleData.First().Title;

                                //Calculating Start and End Dates for Reports
                                DateTime startdate = DateTime.Now.Date;
                                DateTime enddate = DateTime.Now.Date;

                                if (periodType == 'D') {
                                    startdate = enddate.AddDays(-periodCount);
                                }
                                if (periodType == 'M') {
                                    startdate = enddate.AddMonths(-periodCount);
                                    startdate = startdate.AddDays(-startdate.Day + 1);
                                    enddate = startdate.AddMonths(periodCount);
                                    enddate = enddate.AddDays(-1);
                                }
                                if (periodType == 'Q') {
                                    if (enddate.Month == 2 || enddate.Month == 5 || enddate.Month == 8 ||
                                        enddate.Month == 11) {
                                        enddate = enddate.AddMonths(-1);
                                    }
                                    if (enddate.Month == 3 || enddate.Month == 6 || enddate.Month == 9 ||
                                        enddate.Month == 12) {
                                        enddate = enddate.AddMonths(-2);
                                    }
                                    startdate = enddate.AddMonths(-(periodCount * 3));
                                    startdate = startdate.AddDays(-startdate.Day + 1);
                                    enddate = startdate.AddMonths(periodCount * 3);
                                    enddate = enddate.AddDays(-1);
                                }
                                if (periodType == 'Y') {
                                    startdate = enddate.AddYears(-periodCount);
                                }

                                sbTrends.Append("QNM|" + view.DeliveryID + "|" + reportID + "|" + systemserial + "|" +
                                                startdate + "|" + enddate + "|" + title + "|true\n");
                            }
                        }
                    }
                }

                Log.InfoFormat("Message: {0}",sbTrends);
                

                if (sbTrends.Length > 0) {
                    if (ConnectionString.IsLocalAnalyst) {
                        var triggerInsert = new TriggerService(ConnectionString.ConnectionStringDB);
                        triggerInsert.InsertFor("", (int) TriggerType.Type.ReportGeneratorStatic, sbTrends.ToString());
                    }
                    else
                    {
                        string fileName = "qnm_" + DateTime.Now.Ticks + ".txt";
                        //Write to S3 and insert to Queue.
                        var s3 = new AmazonS3(ConnectionString.S3WorkSpace);
                        var sqs = new AmazonSQS();
                        int retry = 0;
                        do
                        {
                            try
                            {
                                s3.WriteToS3(fileName, sbTrends.ToString());

                                string urlQueue = "";
                                if (!string.IsNullOrEmpty(ConnectionString.SQSReport))
                                    urlQueue = sqs.GetAmazonSQSUrl(ConnectionString.SQSReport);

                                if (urlQueue.Length > 0)
                                {
                                    sqs.WriteMessage(urlQueue, fileName);
                                    retry = 5;
                                }
                                else
                                {
                                    //Retry after duration.
                                    Thread.Sleep(AmazonError.GetTimeoutDuration(retry));
                                    retry++;
                                    if (retry == 5)
                                    {
                                        AmazonError.WriteLog("urlQueue is empty", "Amazon.cs: CheckDispathQNMReport",
                                            ConnectionString.AdvisorEmail,
                                            ConnectionString.SupportEmail,
                                            ConnectionString.WebSite,
                                            ConnectionString.EmailServer,
                                            ConnectionString.EmailPort,
                                            ConnectionString.EmailUser,
                                            ConnectionString.EmailPassword,
                                            ConnectionString.EmailAuthentication,
                                            ConnectionString.SystemLocation, ConnectionString.ServerPath,
                                            ConnectionString.EmailIsSSL,
                                            ConnectionString.IsLocalAnalyst, ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                //Retry after duration.
                                Thread.Sleep(AmazonError.GetTimeoutDuration(retry));
                                retry++;
                                if (retry == 5)
                                {
                                    AmazonError.WriteLog(ex, "Amazon.cs: CheckDispathQNMReport",
                                        ConnectionString.AdvisorEmail,
                                        ConnectionString.SupportEmail,
                                        ConnectionString.WebSite,
                                        ConnectionString.EmailServer,
                                        ConnectionString.EmailPort,
                                        ConnectionString.EmailUser,
                                        ConnectionString.EmailPassword,
                                        ConnectionString.EmailAuthentication,
                                        ConnectionString.SystemLocation, ConnectionString.ServerPath,
                                        ConnectionString.EmailIsSSL,
                                        ConnectionString.IsLocalAnalyst, ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                                }
                            }
                        }
                        while (retry < 5);
                    }

                }

                #endregion
            }
            catch (Exception ex) {
                Log.ErrorFormat("QNM Report Error: {0}", ex);
                
            }

            GC.Collect();
        }


        private void CheckDispatchTPSReports(DateTime currDateTime, List<string> expiredSystem) {
            //Get iReport Report. ReportTypeID 6 is iReport Report.
            try {
                #region TPS Report

                var startdate = new DateTime();
                var enddate = new DateTime();
                DateTime currentDateTime = DateTime.Today;

                var deliveryScheduleService = new DeliveryScheduleService(ConnectionString.ConnectionStringTrend);
                IList<DeliveryScheduleView> deliveryScheduleView = deliveryScheduleService.GetTPSSchduleFor();

                var sbTrends = new StringBuilder();
                foreach (DeliveryScheduleView view in deliveryScheduleView) {
                    currDateTime = DateTime.Now;
                    if (!expiredSystem.Contains(view.SystemSerial)) {
                        bool bCreateReport = false;
                        int processDay;
                        int startTime;
                        int stopTime;
                        if (view.FrequencyName.Equals("Weekly")) {
                            if (Convert.ToInt32(currentDateTime.DayOfWeek) == view.SendDay) {
                                bCreateReport = true;

                                if (view.StartTime != -1 && view.StopTime != -1) {
                                    processDay = view.ProcessDate;
                                    int currentDay = view.SendDay;

                                    currDateTime = currentDay > processDay
                                        ? currentDateTime.AddDays((currentDay - processDay) * -1)
                                        : currentDateTime.AddDays(processDay - currentDay - 7);
                                    startTime = view.StartTime;
                                    stopTime = view.StopTime;
                                    startdate = new DateTime(currDateTime.Year, currDateTime.Month, currDateTime.Day,
                                        startTime, 0, 0);

                                    if (stopTime == 24) {
                                        enddate = new DateTime(currDateTime.Year, currDateTime.Month, currDateTime.Day,
                                            23, 59, 59);
                                    }
                                    else {
                                        enddate = new DateTime(currDateTime.Year, currDateTime.Month, currDateTime.Day,
                                            stopTime, 0, 0);
                                    }
                                    if (enddate < startdate) {
                                        enddate = enddate.AddDays(1);
                                    }
                                }
                            }
                        }
                        else if (view.FrequencyName.Equals("Monthly")) {
                            if (view.IsWeekdays) {
                                var date = new DateTime(currentDateTime.Year, currentDateTime.Month, 1);
                                var dayOfWeek = MapDayOfWeek(view.FrequencyWeekday);
                                //Check Frequency.
                                var newValue = date.AddDays((dayOfWeek < date.DayOfWeek ? 7 : 0) + dayOfWeek - date.DayOfWeek);
                                var finalDate = newValue.AddDays((view.FrequencyMonthCount - 1) * 7);

                                if (currentDateTime.Date == finalDate.Date) {
                                    bCreateReport = true;
                                    //Get Report Date Time.
                                    if (view.IsReportDataLast) {
                                        var reportDayOfWeek = MapDayOfWeek(view.ReportDataWeekday);
                                        var reportDate = DateTime.Now.AddDays(-7);
                                        int delta = reportDayOfWeek - reportDate.DayOfWeek;
                                        var newReportDate = reportDate.AddDays(delta);
                                        startTime = view.StartTime;
                                        stopTime = view.StopTime;

                                        startdate = new DateTime(newReportDate.Year, newReportDate.Month, newReportDate.Day, startTime, 0, 0);
                                        if (stopTime == 24) {
                                            enddate = new DateTime(newReportDate.Year, newReportDate.Month, newReportDate.Day, 23, 59, 59);
                                        }
                                        else {
                                            enddate = new DateTime(newReportDate.Year, newReportDate.Month, newReportDate.Day, stopTime, 0, 0);
                                        }

                                    }
                                    else {
                                        if (view.StartTime != -1 && view.StopTime != -1) {
                                            processDay = view.ProcessDate;
                                            startTime = view.StartTime;
                                            stopTime = view.StopTime;

                                            startdate = new DateTime(currDateTime.Year, currDateTime.Month, processDay, startTime, 0, 0);
                                            if (stopTime == 24) {
                                                enddate = new DateTime(currDateTime.Year, currDateTime.Month, processDay, 23, 59, 59);
                                            }
                                            else {
                                                enddate = new DateTime(currDateTime.Year, currDateTime.Month, processDay, stopTime, 0, 0);
                                            }
                                        }
                                    }
                                }
                            }
                            else {
                                int iDate = view.SendDay;
                                if (iDate == 29) {
                                    iDate = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month);
                                }
                                if (Convert.ToInt32(currentDateTime.Day) == iDate) {
                                    bCreateReport = true;
                                    if (view.StartTime != -1 && view.StopTime != -1) {
                                        processDay = view.ProcessDate;
                                        startTime = view.StartTime;
                                        stopTime = view.StopTime;

                                        startdate = new DateTime(currDateTime.Year, currDateTime.Month, processDay, startTime, 0, 0);
                                        if (stopTime == 24) {
                                            enddate = new DateTime(currDateTime.Year, currDateTime.Month, processDay, 23, 59, 59);
                                        }
                                        else {
                                            enddate = new DateTime(currDateTime.Year, currDateTime.Month, processDay, stopTime, 0, 0);
                                        }
                                    }
                                }
                            }
                        }

                        if (bCreateReport) {
                            sbTrends.Append("TPS|" + view.DeliveryID + "|" + view.TrendReportID + "|" + view.SystemSerial + "|" +
                                            startdate + "|" + enddate + "|true\n");
                        }
                    }
                }
                Log.InfoFormat("Message: {0}", sbTrends);
                
                if (sbTrends.Length > 0) {
                    if (ConnectionString.IsLocalAnalyst)
                    {
                        var triggerInsert = new TriggerService(ConnectionString.ConnectionStringDB);
                        triggerInsert.InsertFor("", (int)TriggerType.Type.ReportGeneratorStatic, sbTrends.ToString());
                    }
                    else  {
                        string fileName = "tps_" + DateTime.Now.Ticks + ".txt";
                        //Write to S3 and insert to Queue.
                        var s3 = new AmazonS3(ConnectionString.S3WorkSpace);
                        var sqs = new AmazonSQS();
                        int retry = 0;
                        do {
                            try {
                                s3.WriteToS3(fileName, sbTrends.ToString());

                                string urlQueue = "";
                                if (!string.IsNullOrEmpty(ConnectionString.SQSReport))
                                    urlQueue = sqs.GetAmazonSQSUrl(ConnectionString.SQSReport);

                                if (urlQueue.Length > 0) {
                                    sqs.WriteMessage(urlQueue, fileName);
                                    retry = 5;
                                }
                                else {
                                    //Retry after duration.
                                    Thread.Sleep(AmazonError.GetTimeoutDuration(retry));
                                    retry++;
                                    if (retry == 5) {
                                        AmazonError.WriteLog("urlQueue is empty", "Amazon.cs: CheckDispathQNMReport",
                                            ConnectionString.AdvisorEmail,
                                            ConnectionString.SupportEmail,
                                            ConnectionString.WebSite,
                                            ConnectionString.EmailServer,
                                            ConnectionString.EmailPort,
                                            ConnectionString.EmailUser,
                                            ConnectionString.EmailPassword,
                                            ConnectionString.EmailAuthentication,
                                            ConnectionString.SystemLocation, ConnectionString.ServerPath,
                                            ConnectionString.EmailIsSSL,
                                            ConnectionString.IsLocalAnalyst, ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                                    }
                                }
                            }
                            catch (Exception ex) {
                                //Retry after duration.
                                Thread.Sleep(AmazonError.GetTimeoutDuration(retry));
                                retry++;
                                if (retry == 5) {
                                    AmazonError.WriteLog(ex, "Amazon.cs: CheckDispathQNMReport",
                                        ConnectionString.AdvisorEmail,
                                        ConnectionString.SupportEmail,
                                        ConnectionString.WebSite,
                                        ConnectionString.EmailServer,
                                        ConnectionString.EmailPort,
                                        ConnectionString.EmailUser,
                                        ConnectionString.EmailPassword,
                                        ConnectionString.EmailAuthentication,
                                        ConnectionString.SystemLocation, ConnectionString.ServerPath,
                                        ConnectionString.EmailIsSSL,        
                                        ConnectionString.IsLocalAnalyst, ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                                }
                            }
                        }
                        while (retry < 5);
                    }
                }

                #endregion
            }
            catch (Exception ex) {
                Log.ErrorFormat("TPS Report Error: {0}", ex);
            }
        }
    }
}