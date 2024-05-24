using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using log4net;
using Pathway.Core.Repositories;
using RemoteAnalyst.AWS.Infrastructure;
using RemoteAnalyst.AWS.Queue;
using RemoteAnalyst.AWS.SNS;
using RemoteAnalyst.BusinessLogic.BLL;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.Enums;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.Trigger.JobPool;

namespace RemoteAnalyst.Scheduler.Schedules {
    class RANewReportDispatcher {

        private static readonly ILog Log = LogManager.GetLogger("RANewReportDispatcher");

        public void Timer_Elapsed(object source, ElapsedEventArgs e) {
			DoRANewReportDispatch();
		}

		public void DoRANewReportDispatch() {
			CheckDispatchDaily(DateTime.Now);
			if (DateTime.Now.Minute < 30) {
				//Since frequency is every 30 minutes, check to make sure it isn't triggered twice for the same hour
				int currHour = BusinessLogic.Util.Helper.RoundUp(DateTime.Now, TimeSpan.FromMinutes(15)).Hour;
				if (currHour.Equals(1)) {
					CheckAllReports();
				}
			}


        }

		internal void CheckAllReports() {
            var sysInfo = new System_tblService(ConnectionString.ConnectionStringDB);
            List<string> expiredSystem = sysInfo.GetExpiredSystemFor(ConnectionString.IsLocalAnalyst);

            try {
                Log.Info("***********************************************************************");
                Log.Info("Running Daily Trend Report Dispatcher");
                Log.Info("*****calling CheckDispatchWeekly");
                CheckDispatchWeekly(DateTime.Now, expiredSystem);
                Log.Info("*****calling CheckDispatchMonthly");
                CheckDispatchMonthly(DateTime.Now, expiredSystem);
                Log.Info("*****calling CheckDispatchStorageReports");
                CheckDispatchStorageReports(DateTime.Now, expiredSystem);
                Log.Info("*****calling CheckDispatchQTReports");
                CheckDispatchQTReports(DateTime.Now, expiredSystem);
                Log.Info("*****calling CheckDispatchDPAReports");
                CheckDispatchDPAReports(DateTime.Now, expiredSystem);
                Log.Info("*****calling CheckDispatchQNMReports");
                CheckDispatchQNMReports(DateTime.Now, expiredSystem);
                Log.Info("*****calling CheckDispatchApplicationReports");
                CheckDispatchApplicationReports(DateTime.Now, expiredSystem);
                Log.Info("*****calling CheckDispatchPathwayReports");
                CheckDispatchPathwayReports(DateTime.Now, expiredSystem);
            }
            catch (Exception ex) {
                Log.ErrorFormat("ReportDispatcher Error: {0}",ex.Message);
                
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
                        ConnectionString.EmailIsSSL, ConnectionString.IsLocalAnalyst, 
                        ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                    email.SendLocalAnalystErrorMessageEmail("Scheduler - ReportDispatcher.cs", ex.Message, LicenseService.GetProductName(ConnectionString.ConnectionStringDB));
                }
            }
            finally {
                Log.Info("ReportDispatcher Done");
            }
        }

        internal void CheckDispatchDaily(DateTime currDateTime) {
            try {
                var systemTable = new System_tblService(ConnectionString.ConnectionStringDB);
                List<string> expiredSystem = systemTable.GetExpiredSystemFor(ConnectionString.IsLocalAnalyst);
                #region Get Trend Reports.

                var scheduleService = new ScheduleService(ConnectionString.ConnectionStringDB);
                var scheduleView = scheduleService.GetSchdulesFor((int) Schedule.Types.Daily);

                Log.InfoFormat("scheduleView: {0}",scheduleView.Count);
                

                var emailLists = new List<EmailList>();
                var cusAnalyst = new CusAnalystService(ConnectionString.ConnectionStringDB);

                foreach (var view in scheduleView) {
                    //Don't get expire system.
                    if (!expiredSystem.Contains(view.SystemSerial)) {
                        /*if (view.SystemSerial.Equals("076863")) {
                            extendTime = 2;
                        }*/
#if (DEBUG)
                        //if (view.SystemSerial.CompareTo("079797") != 0
                        //    && view.SystemSerial.CompareTo("079798") != 0) continue;
                        if (view.SystemSerial.CompareTo("078578") != 0) continue;
#endif
                        Log.InfoFormat("System: {0}",view.SystemSerial);
                        

                        Log.InfoFormat("ScheduleId: {0}",view.ScheduleId);
                        

						//Get System's Time.
						int timeZoneIndex = systemTable.GetTimeZoneFor(view.SystemSerial);
						Log.InfoFormat("System's Time Index: {0}",timeZoneIndex);
						

                        var localTime = TimeZoneInformation.ToLocalTime(timeZoneIndex, currDateTime);
                        Log.InfoFormat("localTime: {0}",localTime);
                        

                        Log.InfoFormat("HourBoundaryTrigger: {0}",view.HourBoundaryTrigger);
                        Log.InfoFormat("currDateTime.Minute: {0}",currDateTime.Minute);
                        

                        //If it is second time in the hour then skip if the schedule is for the hour boundary
                        if (view.HourBoundaryTrigger && currDateTime.Minute > 30)
                        {
                            Log.Info("Skipping because of HourBoundaryTrigger condition");
                            
                            continue;
                        }
                        //If it is first time in the hour then skip if the schedule is for the 30 minute boundary
                        if (!view.HourBoundaryTrigger && currDateTime.Minute <= 30)
                        {
                            Log.Info("Skipping because of HourBoundaryTrigger condition");
                             
                            continue;
                        }

                        var extendTime = 2; 
                        if (!view.HourBoundaryTrigger) {   
                            //Changed from 2 to 0 since the timer triggers at 30 mins mark instead of the next hour
                            extendTime = 0;
                        }
                        var sendHour = view.DailyAt + extendTime;
                        if (sendHour >= 24)
                            sendHour -= 24;

                        if (localTime.Hour.Equals(sendHour)) {
                            //we are adding extra hour, due to the time gap that we receive customer's data.
                            DateTime reportStopTime;
                            DateTime reportStartTime;

                            if (view.DailyOn < view.DailyAt) {
                                DateTime previousDate = localTime;
                                if(view.HourBoundaryTrigger) { 
                                    previousDate = localTime.AddDays(-1);
                                }
                                //Default values.
                                reportStartTime = new DateTime(previousDate.Year, previousDate.Month, previousDate.Day, view.DailyOn, 0, 0);
                                if (view.DailyAt == 24)
                                    reportStopTime = new DateTime(previousDate.Year, previousDate.Month, previousDate.Day, 23, 59, 59);
                                else
                                    reportStopTime = new DateTime(previousDate.Year, previousDate.Month, previousDate.Day, view.DailyAt, 0, 0);

                                Log.InfoFormat("reportStartTime: {0}",reportStartTime);
                                Log.InfoFormat("reportStopTime: {0}",reportStopTime);
                                Log.InfoFormat("Email: {0}",view.Email);
                                

                                emailLists.Add(new EmailList {
                                    SystemSerial = view.SystemSerial,
                                    SystemName = view.SystemName,
                                    StartTime = reportStartTime,
                                    StopTime = reportStopTime,
                                    CustomerId = cusAnalyst.GetCustomerIDFor(view.Email),
                                    Email = view.Email,
                                    AlertException = view.AlertException,
                                    ScheduleId = view.ScheduleId
                                });
                            }
                            else if (view.DailyOn >= view.DailyAt) {
                                DateTime previousDate = localTime;
                                if (view.HourBoundaryTrigger)
                                {
                                    previousDate = localTime.AddDays(-1);
                                }
                                reportStartTime = new DateTime(previousDate.Year, previousDate.Month, previousDate.Day, view.DailyOn, 0, 0);
                                if (view.DailyAt == 24)
                                    reportStopTime = new DateTime(localTime.Year, localTime.Month, localTime.Day, 23, 59, 59);
                                else
                                    reportStopTime = new DateTime(localTime.Year, localTime.Month, localTime.Day, view.DailyAt, 0, 0);

                                Log.InfoFormat("reportStartTime: {0}",reportStartTime);
                                Log.InfoFormat("reportStopTime: {0}",reportStopTime);
                                Log.InfoFormat("Email: {0}",view.Email);
                                
                                emailLists.Add(new EmailList {
                                    SystemSerial = view.SystemSerial,
                                    SystemName = view.SystemName,
                                    StartTime = reportStartTime,
                                    StopTime = reportStopTime,
                                    CustomerId = cusAnalyst.GetCustomerIDFor(view.Email),
                                    Email = view.Email,
                                    AlertException = view.AlertException,
                                    ScheduleId = view.ScheduleId
                                });
                            }
                            else {
                                Log.Info("*******Does not match any cases*******");
                                Log.InfoFormat("view.DailyOn: {0}",view.DailyOn);
                                Log.InfoFormat("view.DailyAt: {0}",view.DailyAt);
                                Log.InfoFormat("view.Email: {0}",view.Email);
                                
                            }
                        }
                        else
                        {
                            Log.InfoFormat("localHour {0} doesn't match sendHour {1}", localTime.Hour, sendHour);
                        }
                    }
                    else
                    {
                        Log.InfoFormat("skipping expired system: {0}",view.SystemSerial);
                    }
                    Log.InfoFormat("emailLists: {0}",emailLists.Count);                              
                }
                if (emailLists.Count > 0) {
                    //Group the emailList by SystemSerial, SystemName, Start, Stop Time.
                    var groupedList = emailLists.GroupBy(x => new { x.SystemSerial, x.SystemName, x.StartTime, x.StopTime, x.AlertException, x.ScheduleId}).ToList();

                    Log.InfoFormat("***Unique Dailies setup groupedList: {0}",groupedList.Count);
                    
                    Log.Info("ScheduleId|StartTime|StopTime|EmailList|SystemSerial|SystemName|AlertException");
                    
                    foreach (var gList in groupedList) {
                        try {
#if (DEBUG)
                            //var emailList = "Bernard_Scherer@ajg.com,Ken_Kesler@ajg.com,vishalkudchadkar@idelji.com,rubenkazantsev@idelji.com".Split(',').ToList();
                            var emailList = "vishalkudchadkar@idelji.coms".Split(',').ToList();
                            //var emailList = gList.Select(x => x.Email).Distinct().ToList();
#else
                            var emailList = gList.Select(x => x.Email).Distinct().ToList();
#endif
                            //var newDicList = gList.ToDictionary(x => x.CustomerId, x => x.Email);
                            Log.InfoFormat("{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                                gList.Key.ScheduleId,  
                                gList.Key.StartTime, gList.Key.StopTime,
                                string.Join(",", emailList),
                                gList.Key.SystemSerial, gList.Key.SystemName,
                                gList.Key.AlertException);
                            

                            SendLoadEmail(gList.Key.StartTime, gList.Key.StopTime, emailList, gList.Key.SystemSerial, gList.Key.SystemName, gList.Key.AlertException, gList.Key.ScheduleId);
                        } catch (Exception ex) {
                            Log.Error("*******************************");
                            Log.ErrorFormat("CheckDispatchDaily: groupedList Error 2: {0}",ex);                            
                        }
                    }
                }

#endregion
            }
            catch (Exception ex) {
                Log.ErrorFormat("Daily Trend Error: {0}", ex);
            }
        }

        internal void CheckDispatchWeekly(DateTime currDateTime, List<string> expiredSystem) {

            var scheduleService = new ScheduleService(ConnectionString.ConnectionStringDB);
            var scheduleView = scheduleService.GetSchdulesFor((int)Schedule.Types.Weekly);

            var emailLists = GetStartStopDate(scheduleView, currDateTime, expiredSystem);
            Log.InfoFormat("emailLists: {0}",emailLists.Count);
            
            if (emailLists.Count > 0) {
                //Group the emailList by SystemSerial, SystemName, Start, Stop Time.
                var groupedList = emailLists.GroupBy(x => new { x.SystemSerial, x.StartTime, x.StopTime, x.ReportFromHour, x.ReportToHour, x.AlertException, x.IsLowPin, x.IsHighPin, x.IsAllSubVol, x.SubVols }).ToList();

                Log.InfoFormat("***groupedList: {0}",groupedList.Count);
                

                foreach (var gList in groupedList) {
                    try {
                        var newList = gList.Select(x => x.Email).Distinct().ToList();

                        Log.InfoFormat("StartTime: {0}",gList.Key.StartTime);
                        Log.InfoFormat("StopTime: {0}",gList.Key.StopTime);
                        Log.InfoFormat("ReportFromHour: {0}",gList.Key.ReportFromHour);
                        Log.InfoFormat("ReportToHour: {0}",gList.Key.ReportToHour);
                        Log.InfoFormat("Emails: {0}",string.Join(",", newList));
                        Log.InfoFormat("SystemSerial: {0}",gList.Key.SystemSerial);
                        Log.InfoFormat("AlertException: {0}",gList.Key.AlertException);
                        Log.InfoFormat("IsLowPin: {0}",gList.Key.IsLowPin);
                        Log.InfoFormat("IsHighPin: {0}",gList.Key.IsHighPin);
                        Log.InfoFormat("IsAllSubVol: {0}",gList.Key.IsAllSubVol);
                        Log.InfoFormat("SubVols: {0}",gList.Key.SubVols);
                        

                        var reportDownloads = new ReportDownloadService(ConnectionString.ConnectionStringDB);
                        int reportDownloadId = reportDownloads.InsertNewReportFor(gList.Key.SystemSerial, gList.Key.StartTime, gList.Key.StopTime, 2, 0);
                        reportDownloads.UpdateStatusFor(reportDownloadId, 0);
                        var reportDownloadLogService = new ReportDownloadLogService(ConnectionString.ConnectionStringDB);
                        reportDownloadLogService.InsertNewLogFor(reportDownloadId, DateTime.Now, "Start generating Weekly report");
                        
                        var fromHour = gList.Key.ReportFromHour;
                        var toHour = gList.Key.ReportToHour;

                        if (fromHour == "00:00" && toHour == "00:00") {
                            fromHour = "";
                            toHour = "";
                        }

                        var message = new StringBuilder();
                        message.Append("NEWTREND|");
                        message.Append(gList.Key.SystemSerial + "|");
                        message.Append(gList.Key.StartTime + "|");
                        message.Append(gList.Key.StopTime + "|");
                        message.Append(fromHour + "|");
                        message.Append(toHour + "|");
                        message.Append("Weekly|");
                        message.Append(string.Join(",", newList) + "|");
                        message.Append(gList.Key.AlertException + "|");
                        message.Append(gList.Key.IsLowPin + "|");
                        message.Append(gList.Key.IsHighPin + "|");
                        message.Append(gList.Key.IsAllSubVol + "|");
                        message.Append(gList.Key.SubVols + "|");
                        message.Append(reportDownloadId);

                        Log.InfoFormat("Message: {0}",message);



                        if (ConnectionString.IsLocalAnalyst)
                        {
                            var triggerInsert = new TriggerService(ConnectionString.ConnectionStringDB);
                            triggerInsert.InsertFor("", (int)TriggerType.Type.ReportGeneratorStatic, message.ToString());
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
                                        sqs.WriteMessage(urlQueue, message.ToString());
                                        retry = 5;
                                    }
                                    else {
                                        //Retry after duration.
                                        Thread.Sleep(AmazonError.GetTimeoutDuration(retry));
                                        retry++;
                                        if (retry == 5) {
                                            AmazonError.WriteLog("urlQueue is empty", "Amazon.cs: CheckDispathWeeklyReport",
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
                                        AmazonError.WriteLog(ex, "Amazon.cs: CheckDispathWeeklyReport",
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
                    catch (Exception ex) {
                        Log.Error("*******************************");
                        Log.ErrorFormat("CheckDispatchWeekly: groupedList Error 2: {0}",ex);                        
                    }
                }
            }
        }

		internal void CheckDispatchMonthly(DateTime currDateTime, List<string> expiredSystem) {
            var scheduleService = new ScheduleService(ConnectionString.ConnectionStringDB);
            var scheduleView = scheduleService.GetSchdulesFor((int)Schedule.Types.Monthly);
            var emailLists = GetStartStopDate(scheduleView, currDateTime, expiredSystem);
            Log.InfoFormat("emailLists: {0}",emailLists.Count);
            
            if (emailLists.Count > 0) {
                //Group the emailList by SystemSerial, SystemName, Start, Stop Time.
                var groupedList = emailLists.GroupBy(x => new { x.SystemSerial, x.StartTime, x.StopTime, x.ReportFromHour, x.ReportToHour, x.AlertException }).ToList();

                Log.InfoFormat("***groupedList: {0}",groupedList.Count);
                

                foreach (var gList in groupedList) {
                    try {
                        var newList = gList.Select(x => x.Email).ToList();

                        Log.InfoFormat("StartTime: {0}",gList.Key.StartTime);
                        Log.InfoFormat("StopTime: {0}",gList.Key.StopTime);
                        Log.InfoFormat("ReportFromHour: {0}",gList.Key.ReportFromHour);
                        Log.InfoFormat("ReportToHour: {0}",gList.Key.ReportToHour);
                        Log.InfoFormat("Emails: {0}",string.Join(",", newList));
                        Log.InfoFormat("SystemSerial: {0}",gList.Key.SystemSerial);
                        Log.InfoFormat("AlertException: {0}",gList.Key.AlertException);
                        

						var reportDownloads = new ReportDownloadService(ConnectionString.ConnectionStringDB);
						int reportDownloadId = reportDownloads.InsertNewReportFor(gList.Key.SystemSerial, gList.Key.StartTime, gList.Key.StopTime, 2, 0);
						reportDownloads.UpdateStatusFor(reportDownloadId, 0);
						var reportDownloadLogService = new ReportDownloadLogService(ConnectionString.ConnectionStringDB);
						reportDownloadLogService.InsertNewLogFor(reportDownloadId, DateTime.Now, "Start generating Monthly report");
						var fromHour = gList.Key.ReportFromHour;
                        var toHour = gList.Key.ReportToHour;

                        if (fromHour == "00:00" && toHour == "00:00") {
                            fromHour = "";
                            toHour = "";
                        }

                        var message = new StringBuilder();
                        message.Append("NEWTREND|");
                        message.Append(gList.Key.SystemSerial + "|");
                        message.Append(gList.Key.StartTime + "|");
                        message.Append(gList.Key.StopTime + "|");
                        message.Append(fromHour + "|");
                        message.Append(toHour + "|");
                        message.Append("Monthly|");
                        message.Append(string.Join(",", newList) + "|");
                        message.Append(gList.Key.AlertException + "|");
                        message.Append(reportDownloadId);

                        Log.InfoFormat("Message: {0}",message);

                        if (ConnectionString.IsLocalAnalyst)
                        {
                            var triggerInsert = new TriggerService(ConnectionString.ConnectionStringDB);
                            triggerInsert.InsertFor("", (int)TriggerType.Type.ReportGeneratorStatic, message.ToString());
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
                                        sqs.WriteMessage(urlQueue, message.ToString());
                                        retry = 5;
                                    }
                                    else {
                                        //Retry after duration.
                                        Thread.Sleep(AmazonError.GetTimeoutDuration(retry));
                                        retry++;
                                        if (retry == 5) {
                                            AmazonError.WriteLog("urlQueue is empty", "Amazon.cs: CheckDispathMonthlyReport",
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
                                        AmazonError.WriteLog(ex, "Amazon.cs: CheckDispathMonthlyReport",
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
                    catch (Exception ex) {
                        Log.Error("*******************************");
                        Log.ErrorFormat("CheckDispatchMonthly: groupedList Error 2: {0}",ex);                        
                    }
                }
            }
        }

        internal void CheckDispatchStorageReports(DateTime currDateTime, List<string> expiredSystem) {
            var scheduleService = new ScheduleService(ConnectionString.ConnectionStringDB);
            var scheduleView = scheduleService.GetSchdulesFor((int)Schedule.Types.Storage);
            var emailLists = GetStartStopDate(scheduleView, currDateTime, expiredSystem);
            Log.InfoFormat("emailLists: {0}",emailLists.Count);
            
            if (emailLists.Count > 0) {
                //Group the emailList by SystemSerial, SystemName, Start, Stop Time.
                var groupedList = emailLists.GroupBy(x => new { x.SystemSerial, x.StartTime, x.StopTime, x.DetailTypeId, x.ScheduleId }).ToList();

                Log.InfoFormat("***groupedList: {0}",groupedList.Count);
                

                foreach (var gList in groupedList) {
                    try {
                        var newList = gList.Select(x => x.Email).ToList();

                        Log.InfoFormat("StartTime: {0}",gList.Key.StartTime);
                        Log.InfoFormat("StopTime: {0}",gList.Key.StopTime);
                        Log.InfoFormat("Customers: {0}",string.Join(",", newList));
                        Log.InfoFormat("SystemSerial: {0}",gList.Key.SystemSerial);
                        

                        var reportDownloads = new ReportDownloadService(ConnectionString.ConnectionStringDB);
                        var reportDownloadId = reportDownloads.InsertNewReportFor(gList.Key.SystemSerial, gList.Key.StartTime, gList.Key.StopTime, (int)Schedule.Types.Storage, 0);
                        
                        //Write to Qeueue.
                        string buildMessage = "StorageScheduleNew|" + reportDownloadId + "|" + gList.Key.DetailTypeId + "|" + gList.Key.SystemSerial + "|" + gList.Key.ScheduleId + "|" + string.Join(",", newList);

                        if (ConnectionString.IsLocalAnalyst)
                        {
                            var triggerInsert = new TriggerService(ConnectionString.ConnectionStringDB);
                            triggerInsert.InsertFor(gList.Key.SystemSerial, (int)TriggerType.Type.ReportGeneratorStatic, buildMessage);
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
                                                ConnectionString.IsLocalAnalyst,
                                                ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
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
                        trigger.WriteReportMessage(ConnectionString.WatchFolder, (int)Report.Types.Storage, gList.Key.SystemSerial, buildMessage);
                        */
                    }
                    catch (Exception ex) {
                        Log.Error("*******************************");
                        Log.ErrorFormat("CheckDispatchDailyStorageReports: groupedList Error 2: {0}",ex);
                        
                    }
                }
            }
        }
        internal void CheckDispatchQTReports(DateTime currDateTime, List<string> expiredSystem) {

            var scheduleService = new ScheduleService(ConnectionString.ConnectionStringDB);
            var scheduleView = scheduleService.GetSchdulesFor((int)Schedule.Types.QuickTuner);
            var emailLists = GetStartStopDate(scheduleView, currDateTime, expiredSystem);
            Log.InfoFormat("emailLists: {0}",emailLists.Count);
            
            if (emailLists.Count > 0) {
                var systemTable = new System_tblService(ConnectionString.ConnectionStringDB);
                //Group the emailList by SystemSerial, SystemName, Start, Stop Time.
                var groupedList = emailLists.GroupBy(x => new {x.ScheduleId,  x.SystemSerial, x.StartTime, x.StopTime }).ToList();

                Log.InfoFormat("***groupedList: {0}",groupedList.Count);
                

                foreach (var gList in groupedList) {
                    try {
                        var newList = gList.Select(x => x.Email).ToList();

                        var scheduleId = gList.Key.ScheduleId;
                        Log.InfoFormat("ScheduleId: {0}",scheduleId);
                        Log.InfoFormat("StartTime: {0}",gList.Key.StartTime);
                        Log.InfoFormat("StopTime: {0}",gList.Key.StopTime);
                        Log.InfoFormat("Emails: {0}",string.Join(",", newList));
                        Log.InfoFormat("SystemSerial: {0}",gList.Key.SystemSerial);
                        

                        //Insert entry on ReportDownloads.
                        var reportDownloads = new ReportDownloadService(ConnectionString.ConnectionStringDB);
                        var reportDownloadId = reportDownloads.InsertNewReportFor(gList.Key.SystemSerial, gList.Key.StartTime, gList.Key.StopTime, (int)Schedule.Types.QuickTuner, 0);
                        var param = scheduleService.GetQTParamFor(scheduleId).Split('|');

						//Write to Qeueue.
						string systemName = systemTable.GetSystemNameFor(gList.Key.SystemSerial);
						System_tblService system_TblService = new System_tblService(ConnectionString.ConnectionStringDB);
						string companyName = system_TblService.GetCompanyNameFor(gList.Key.SystemSerial);

						string buildMessage = "QT|" + reportDownloadId + "|" + gList.Key.SystemSerial + "|" + systemName +
                                              "|" + gList.Key.StartTime + "|" + gList.Key.StopTime + "|" +
                                              param[0] + "|" + param[1] + "||||" + string.Join(",", newList) + @"|-1|false|true|S|" + companyName + @"|S QT \" + systemName + " " + companyName + "|250";

                        Log.InfoFormat("Message: {0}",buildMessage);

                        if (ConnectionString.IsLocalAnalyst)
                        {
                            var triggerInsert = new TriggerService(ConnectionString.ConnectionStringDB);
                            triggerInsert.InsertFor(gList.Key.SystemSerial, (int)TriggerType.Type.ReportGenerator, buildMessage);
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
                        /* Dead code, need to research
                        var trigger = new Triggers();
                        trigger.WriteReportMessage(ConnectionString.WatchFolder, (int)Report.Types.QT, gList.Key.SystemSerial, buildMessage);
                        */
                    }
                    catch (Exception ex) {
                        Log.Error("*******************************");
                        Log.ErrorFormat("CheckDispatchQTReports: groupedList Error 2: {0}",ex);
                        
                    }
                }
            }
        }
        internal void CheckDispatchDPAReports(DateTime currDateTime, List<string> expiredSystem) {
            var scheduleService = new ScheduleService(ConnectionString.ConnectionStringDB);
            var scheduleView = scheduleService.GetSchdulesFor((int)Schedule.Types.DeepDive);
            var emailLists = GetStartStopDate(scheduleView, currDateTime, expiredSystem);
            Log.InfoFormat("emailLists: {0}",emailLists.Count);
            
            if (emailLists.Count > 0) {
                //Group the emailList by SystemSerial, SystemName, Start, Stop Time.
                var groupedList = emailLists.GroupBy(x => new { x.ScheduleId, x.SystemSerial, x.StartTime, x.StopTime }).ToList();
                Log.InfoFormat("***groupedList: {0}",groupedList.Count);
                

                foreach (var gList in groupedList) {
                    try {
                        var newList = gList.Select(x => x.Email).ToList();

                        var scheduleId = gList.Key.ScheduleId;
                        Log.InfoFormat("ScheduleId: {0}",scheduleId);
                        Log.InfoFormat("StartTime: {0}",gList.Key.StartTime);
                        Log.InfoFormat("StopTime: {0}",gList.Key.StopTime);
                        Log.InfoFormat("Emails: {0}",string.Join(",", newList));
                        Log.InfoFormat("SystemSerial: {0}",gList.Key.SystemSerial);
                        

                        //Insert entry on ReportDownloads.
                        var reportDownloads = new ReportDownloadService(ConnectionString.ConnectionStringDB);
                        var reportDownloadId = reportDownloads.InsertNewReportFor(gList.Key.SystemSerial, gList.Key.StartTime, gList.Key.StopTime, (int)Schedule.Types.DeepDive, 0);
                        var param = scheduleService.GetDDParamFor(scheduleId).Split('|');

						System_tblService system_TblService = new System_tblService(ConnectionString.ConnectionStringDB);
						string systemName = system_TblService.GetSystemNameFor(gList.Key.SystemSerial);
						string companyName = system_TblService.GetCompanyNameFor(gList.Key.SystemSerial);

                        //NOTE iReport does not exist anymore.
                        //Write to Qeueue. if parameter is empty, it uses deaulft values.
                        string buildMessage = "DPA|" + reportDownloadId + "|" + gList.Key.SystemSerial + "|" + gList.Key.StartTime + "|" + gList.Key.StopTime + "|" + param[0] + "|" +
                                              param[1] + "|" + param[2] + "||" + string.Join(",", newList) + "|" + param[3] + "|" + param[4] + @"|false|true|S|" + companyName + @"|S DPA \" + systemName + " " + companyName + "|250";

                        Log.InfoFormat("Message: {0}",buildMessage);


                        if (ConnectionString.IsLocalAnalyst)
                        {
                            var triggerInsert = new TriggerService(ConnectionString.ConnectionStringDB);
                            triggerInsert.InsertFor(gList.Key.SystemSerial, (int)TriggerType.Type.ReportGenerator, buildMessage);
                        }
                        else {
#region New scheduler for report generation

                            IAmazonSNS amazonSns = new AmazonSNS();
                            string subjectMessage = ReportGeneratorStatus.GetEnumDescription(ReportGeneratorStatus.StatusMessage.DPAOrder);
                            amazonSns.SendToTopic(subjectMessage, buildMessage, ConnectionString.SNSProdTriggerReportARN);
                            //Make sure lambda won't timed out
                            Thread.Sleep(60000);

#endregion

                        }
                        /* Dead code, need to research
                        var trigger = new Triggers();
                        trigger.WriteReportMessage(ConnectionString.WatchFolder, (int)Report.Types.DPA, gList.Key.SystemSerial, buildMessage);
                        */                            
                    }
                    catch (Exception ex) {
                        Log.Error("*******************************");
                        Log.ErrorFormat("CheckDispatchDPAReports: groupedList Error 2: {0}",ex);
                        
                    }
                }
            }
        }
        internal void CheckDispatchQNMReports(DateTime currDateTime, List<string> expiredSystem) {
            var scheduleService = new ScheduleService(ConnectionString.ConnectionStringDB);
            var scheduleView = scheduleService.GetSchdulesFor((int)Schedule.Types.Network);
            var emailLists = GetStartStopDate(scheduleView, currDateTime, expiredSystem);
            Log.InfoFormat("emailLists: {0}",emailLists.Count);
            
            if (emailLists.Count > 0) {
                //Group the emailList by SystemSerial, SystemName, Start, Stop Time.
                var groupedList = emailLists.GroupBy(x => new { x.SystemSerial, x.StartTime, x.StopTime }).ToList();

                Log.InfoFormat("***groupedList: {0}",groupedList.Count);
                

                foreach (var gList in groupedList) {
                    try {
                        var newList = gList.Select(x => x.Email).ToList();

                        Log.InfoFormat("StartTime: {0}",gList.Key.StartTime);
                        Log.InfoFormat("StopTime: {0}",gList.Key.StopTime);
                        Log.InfoFormat("Customers: {0}",string.Join(",", newList));
                        Log.InfoFormat("SystemSerial: {0}",gList.Key.SystemSerial);
                        

                        //sbTrends.Append("QNM|" + view.DeliveryID + "|" + reportID + "|" + systemserial + "|" + startdate + "|" + enddate + "|" + title + "|true\n");
                        string message = "QNMNew|" + gList.Key.SystemSerial + "|" + gList.Key.StartTime + "|" + gList.Key.StopTime + "|" + string.Join(", ", newList) + "|true";
                        Log.InfoFormat("Message: {0}",message);
                        

                        var retry = 1;
                        if(ConnectionString.IsLocalAnalyst)
                        {
                            var triggerInsert = new TriggerService(ConnectionString.ConnectionStringDB);
                            triggerInsert.InsertFor(gList.Key.SystemSerial, (int)TriggerType.Type.ReportGeneratorStatic, message);
                        }
                        else  {
                            do {
                                var sqs = new AmazonSQS();
                                string urlQueue = "";
                                if (!string.IsNullOrEmpty(ConnectionString.SQSReport))
                                    urlQueue = sqs.GetAmazonSQSUrl(ConnectionString.SQSReport);

                                if (urlQueue.Length > 0) {
                                    sqs.WriteMessage(urlQueue, message);
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
                            while (retry < 5);
                        }
                    }
                    catch (Exception ex) {
                        Log.Error("*******************************");
                        Log.ErrorFormat("CheckDispatchQNMReports: groupedList Error 2: {0}",ex);
                        
                    }
                }
            }
        }

        internal void CheckDispatchBatchSequenceReports(DateTime currDateTime, List<string> expiredSystem)
        {
            var scheduleService = new ScheduleService(ConnectionString.ConnectionStringDB);
            var scheduleView = scheduleService.GetSchdulesFor((int)Schedule.Types.BatchSequence); //
            var emailLists = GetStartStopDate(scheduleView, currDateTime, expiredSystem);
            Log.InfoFormat("emailLists: {0}",emailLists.Count);
            
            if (emailLists.Count > 0)
            {
                //Group the emailList by SystemSerial, SystemName, Start, Stop Time.
                var groupedList = emailLists.GroupBy(x => new { x.SystemSerial, x.StartTime, x.StopTime, x.BatchId, x.ReportDownloadId}).ToList();

                Log.InfoFormat("***groupedList: {0}",groupedList.Count);
                

                foreach (var gList in groupedList)
                {
                    try
                    {
                        var newList = gList.Select(x => x.Email).ToList();
                        var batchProgramList = gList.Select(x => x.BatchProgram).ToList();
                        var batchIdList = gList.Select(x => x.BatchId).ToList();
                        Log.InfoFormat("StartTime: {0}",gList.Key.StartTime);
                        Log.InfoFormat("StopTime: {0}",gList.Key.StopTime);
                        
                        Log.InfoFormat("Customers: {0}",string.Join(",", newList));
                        Log.InfoFormat("SystemSerial: {0}",gList.Key.SystemSerial);
                        Log.InfoFormat("batchIdList: {0}",gList.Key.BatchId);
                        Log.InfoFormat("batchProgramList: {0}", string.Join(",", batchProgramList));
                        Log.InfoFormat("reportDownloadId: {0}", gList.Key.ReportDownloadId);
                        

                        //sbTrends.Append("QNM|" + view.DeliveryID + "|" + reportID + "|" + systemserial + "|" + startdate + "|" + enddate + "|" + title + "|true\n");
                        string message = "Batch|" + gList.Key.SystemSerial + "|" + gList.Key.StartTime + "|" + gList.Key.StopTime + "|" + string.Join(", ", batchProgramList) + "|" + gList.Key.BatchId + "|" + string.Join(", ", newList) + "|" + gList.Key.ReportDownloadId + "|true";
                        Log.InfoFormat("Message: {0}",message);
                        

                        var retry = 1;
                        if (ConnectionString.IsLocalAnalyst)
                        {
                            var triggerInsert = new TriggerService(ConnectionString.ConnectionStringDB);
                            triggerInsert.InsertFor(gList.Key.SystemSerial, (int)TriggerType.Type.ReportGeneratorStatic, message);
                        }
                        else
                        {
                            do
                            {
                                var sqs = new AmazonSQS();
                                string urlQueue = "";
                                if (!string.IsNullOrEmpty(ConnectionString.SQSReport))
                                    urlQueue = sqs.GetAmazonSQSUrl(ConnectionString.SQSReport);
                                
                                if (urlQueue.Length > 0)
                                {
                                    sqs.WriteMessage(urlQueue, message);
                                    retry = 5;
                                }
                                else
                                {
                                    //Retry after duration.
                                    Thread.Sleep(AmazonError.GetTimeoutDuration(retry));
                                    retry++;
                                    if (retry == 5)
                                    {
                                        
                                        AmazonError.WriteLog("urlQueue is empty", "Amazon.cs: CheckDispatchBatchSequenceReport",
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
                    catch (Exception ex)
                    {
                        Log.Error("*******************************");
                        Log.ErrorFormat("CheckDispatchBatchSequenceReports: groupedList Error 2: {0}",ex);
                        
                    }
                }
            }
        }

        internal void CheckDispatchApplicationReports(DateTime currDateTime, List<string> expiredSystem)
        {
            var scheduleService = new ScheduleService(ConnectionString.ConnectionStringDB);
            var scheduleView = scheduleService.GetSchdulesFor((int)Schedule.Types.Application);
            var emailLists = GetStartStopDate(scheduleView, currDateTime, expiredSystem);
            Log.InfoFormat("emailLists: {0}",emailLists.Count);
            
            if (emailLists.Count > 0)
            {
                //Group the emailList by SystemSerial, SystemName, Start, Stop Time.
                var groupedList = emailLists.GroupBy(x => new { x.SystemSerial, x.StartTime, x.StopTime, x.DetailTypeId}).ToList();
                Log.InfoFormat("***groupedList: {0}",groupedList.Count);
                

                foreach (var gList in groupedList)
                {
                    try
                    {
                        var newList = gList.Select(x => x.Email).ToList();
                        Log.InfoFormat("StartTime: {0}",gList.Key.StartTime);
                        Log.InfoFormat("StopTime: {0}",gList.Key.StopTime);
                        Log.InfoFormat("Customers: {0}",string.Join(",", newList));
                        Log.InfoFormat("SystemSerial: {0}",gList.Key.SystemSerial);
                        Log.InfoFormat("Application list: {0}",gList.Key.DetailTypeId);
                        

                        var reportDownloads = new ReportDownloadService(ConnectionString.ConnectionStringDB);
                        var reportDownloadId = reportDownloads.InsertNewReportFor(gList.Key.SystemSerial, gList.Key.StartTime, gList.Key.StopTime, (int)Schedule.Types.Application, 0);
                        string message = "NEWAPPLICATION|" + gList.Key.SystemSerial + "|" + gList.Key.StartTime + "|" + gList.Key.StopTime + "|" + gList.Key.DetailTypeId + "|" + string.Join(", ", newList) + "|" + reportDownloadId;
                        Log.InfoFormat("Message: {0}",message);
                        

                        var retry = 1;
                        if (ConnectionString.IsLocalAnalyst)
                        {
                            var triggerInsert = new TriggerService(ConnectionString.ConnectionStringDB);
                            triggerInsert.InsertFor(gList.Key.SystemSerial, (int)TriggerType.Type.ReportGeneratorStatic, message);
                        }
                        else
                        {
                            do
                            {
                                var sqs = new AmazonSQS();
                                string urlQueue = "";
                                if (!string.IsNullOrEmpty(ConnectionString.SQSReport))
                                    urlQueue = sqs.GetAmazonSQSUrl(ConnectionString.SQSReport);

                                if (urlQueue.Length > 0)
                                {
                                    sqs.WriteMessage(urlQueue, message);
                                    retry = 5;
                                }
                                else
                                {
                                    //Retry after duration.
                                    Thread.Sleep(AmazonError.GetTimeoutDuration(retry));
                                    retry++;
                                    if (retry == 5)
                                    {
                                        AmazonError.WriteLog("urlQueue is empty", "Amazon.cs: CheckDispathApplicationReport",
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
                    catch (Exception ex)
                    {
                        Log.Error("*******************************");
                        Log.ErrorFormat("CheckDispatchApplicationReports: groupedList Error 2: {0}",ex);
                        
                    }
                }
            }
        }

        internal void CheckDispatchPathwayReports(DateTime currDateTime, List<string> expiredSystem)
        {
            var scheduleService = new ScheduleService(ConnectionString.ConnectionStringDB);
            var scheduleView = scheduleService.GetSchdulesFor((int)Schedule.Types.Pathway);
            var emailLists = GetStartStopDate(scheduleView, currDateTime, expiredSystem);
            var databaseMappingService = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
            
            
            Log.InfoFormat("emailLists: {0}",emailLists.Count);
            
            if (emailLists.Count > 0)
            {
                var groupedList = emailLists.GroupBy(x => new { x.SystemSerial, x.StartTime, x.StopTime, x.DetailTypeId, x.SystemName }).ToList();
                Log.InfoFormat("***groupedList: {0}",groupedList.Count);
                

                foreach (var gList in groupedList)
                {
                    try {
                        var stopTime = gList.Key.StopTime;
                        //if gList.Key.StopTime ends with 59:59, add one second.
                        if (gList.Key.StopTime.Hour == 23 && gList.Key.StopTime.Minute == 59 && gList.Key.StopTime.Second == 59)
                            stopTime = gList.Key.StopTime.AddSeconds(1);

                        var newList = gList.Select(x => x.Email).ToList();
                        Log.InfoFormat("StartTime: {0}",gList.Key.StartTime);
                        Log.InfoFormat("StopTime: {0}",stopTime);
                        Log.InfoFormat("Customers: {0}",string.Join(",", newList));
                        Log.InfoFormat("SystemSerial: {0}",gList.Key.SystemSerial);
                        Log.InfoFormat("Pathway type: {0}",gList.Key.DetailTypeId);
                        

                        string newConnectionString = databaseMappingService.GetMySQLConnectionStringFor(gList.Key.SystemSerial);
                        PvPwyListRepository pwyList = new PvPwyListRepository();
						var nullChecker = new NullCheckService();
						if(!nullChecker.NullCheckForPathwayPramaterPvCollects(gList.Key.StartTime, stopTime, newConnectionString) || !nullChecker.NullCheckForPathwayPramaterPvPwyList(gList.Key.StartTime, stopTime, newConnectionString)) {
							var emailReport = new EmailToSupport(ConnectionString.AdvisorEmail, ConnectionString.SupportEmail,
							ConnectionString.WebSite,
							ConnectionString.EmailServer, ConnectionString.EmailPort, ConnectionString.EmailUser,
							ConnectionString.EmailPassword, ConnectionString.EmailAuthentication,
                            ConnectionString.SystemLocation, ConnectionString.ServerPath,
                            ConnectionString.EmailIsSSL, ConnectionString.IsLocalAnalyst,
                            ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                            
                            foreach(var email in newList) {
                                emailReport.SendPathwayParamaterErrorMassageEmail(gList.Key.SystemSerial, gList.Key.SystemName, gList.Key.StartTime, stopTime, email, gList.Key.DetailTypeId);
                            }							
							continue;

						}

                        var pwyLists = pwyList.GetPathwayNames(gList.Key.StartTime, stopTime);
                        var pvCollectService = new PvCollectsService(newConnectionString);
                        var intervalInfo = pvCollectService.GetIntervalFor(gList.Key.StartTime, stopTime);
                        long interval;
                        if (intervalInfo.IntervalType.Equals("H"))
                        {
                            interval = intervalInfo.IntervalNumber * 60 * 60;
                        }
                        else
                        {
                            interval = intervalInfo.IntervalNumber * 60;
                        }

                        var reportDownloads = new ReportDownloadService(ConnectionString.ConnectionStringDB);
                        var reportDownloadId = reportDownloads.InsertNewReportFor(gList.Key.SystemSerial, gList.Key.StartTime, stopTime, (int)Schedule.Types.Pathway, 0);


                        string message = "Pathway|" + gList.Key.SystemSerial + "|" + gList.Key.StartTime + "|" + stopTime + "|" + String.Join(",", pwyLists.ToArray()) + "|" + string.Join(",", newList) + "|" + interval + "|" + gList.Key.DetailTypeId + "|" + reportDownloadId;

                        Log.InfoFormat("Message: {0}",message);
                        

                        var retry = 1;
                        if (ConnectionString.IsLocalAnalyst)
                        {
                            var triggerInsert = new TriggerService(ConnectionString.ConnectionStringDB);
                            triggerInsert.InsertFor(gList.Key.SystemSerial, (int)TriggerType.Type.ReportGeneratorStatic, message);
                        }
                        else 
                        {
							do
                            {
                                var sqs = new AmazonSQS();
                                string urlQueue = "";
                                if (!string.IsNullOrEmpty(ConnectionString.SQSReport))
                                    urlQueue = sqs.GetAmazonSQSUrl(ConnectionString.SQSReport);

                                if (urlQueue.Length > 0)
                                {
                                    sqs.WriteMessage(urlQueue, message);
                                    retry = 5;
                                }
                                else
                                {
                                    //Retry after duration.
                                    Thread.Sleep(AmazonError.GetTimeoutDuration(retry));
                                    retry++;
                                    if (retry == 5)
                                    {
                                        AmazonError.WriteLog("urlQueue is empty", "Amazon.cs: CheckDispathPathwayReport",
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
                    catch (Exception ex)
                    {
                        Log.Error("*******************************");
                        Log.ErrorFormat("CheckDispatchPathwayReports: groupedList Error 2: {0}",ex);
                        
                    }
                }
            }
        }

        private List<EmailList> GetStartStopDate(List<ScheduleView> scheduleView, DateTime currDateTime, List<string> expiredSystem) {
            var systemTable = new System_tblService(ConnectionString.ConnectionStringDB);
            var emailLists = new List<EmailList>();
            var cusAnalyst = new CusAnalystService(ConnectionString.ConnectionStringDB);
            var scheduleService = new ScheduleService(ConnectionString.ConnectionStringDB);

            var isLowPin = false;
            var isHihgPin = false;
            var isAllSubVol = false;
            var subVols = "";

            foreach (var view in scheduleView) {
                var match = false;
                if (!expiredSystem.Contains(view.SystemSerial)) {
                    DateTime reportStopTime = DateTime.MinValue;
                    DateTime reportStartTime = DateTime.MinValue;

                    // if type is batch then fetch BatchProgram list from ScheduleBatchSequency
                    //view.ScheduleId

                    if (view.Frequency.Equals("Daily")) {
                        var extendTime = 2;
                        /*if (view.SystemSerial.Equals("076863") ||
                            view.SystemSerial.Equals("076862") ||
                            view.SystemSerial.Equals("077637")) {
                            extendTime = 2;
                        }*/

                        //Get System's Time.
                        int timeZoneIndex = systemTable.GetTimeZoneFor(view.SystemSerial);
                        var localTime = TimeZoneInformation.ToLocalTime(timeZoneIndex, DateTime.Now);

                        Log.InfoFormat("System: {0}",view.SystemSerial);
                        Log.InfoFormat("System's Time: {0}",timeZoneIndex);
                        Log.InfoFormat("localTime: {0}",localTime);
                        

                        /*var sendHour = view.DailyAt + extendTime;
                        if (sendHour >= 24)
                            sendHour -= 24;*/

                        //if (localTime.Hour.Equals(sendHour)) {
                            if (view.DailyOn < view.DailyAt) {
                                match = true;
                                DateTime previousDate = localTime.AddDays(-1);
                                //Default values.
                                reportStartTime = new DateTime(previousDate.Year, previousDate.Month, previousDate.Day, view.DailyOn, 0, 0);
                                if (view.DailyAt == 24)
                                    reportStopTime = new DateTime(previousDate.Year, previousDate.Month, previousDate.Day, 23, 59, 59);
                                else
                                    reportStopTime = new DateTime(previousDate.Year, previousDate.Month, previousDate.Day, view.DailyAt, 0, 0);
                            }
                            else if (view.DailyOn >= view.DailyAt) {
                                match = true;
                                DateTime previousDate = localTime.AddDays(-1);
                                reportStartTime = new DateTime(previousDate.Year, previousDate.Month, previousDate.Day, view.DailyOn, 0, 0);
                                if (view.DailyAt == 24)
                                    reportStopTime = new DateTime(localTime.Year, localTime.Month, localTime.Day, 23, 59, 59);
                                else
                                    reportStopTime = new DateTime(localTime.Year, localTime.Month, localTime.Day, view.DailyAt, 0, 0);
                            }
                            else {
                                Log.Info("*******Does not match any cases*******");
                                Log.InfoFormat("view.DailyOn: {0}",view.DailyOn);
                                Log.InfoFormat("view.DailyAt: {0}",view.DailyAt);
                                Log.InfoFormat("view.Email: {0}",view.Email);                                
                            }
                        //}
                    }
                    else if (view.Frequency.Equals("Weekly")) {
                        if (Convert.ToInt32(currDateTime.DayOfWeek) == view.WeeklyOn) {
                            match = true;
                            if (view.Type.Equals("Quick Tuner") || view.Type.Equals("Deep Dive") || view.Type.Equals("Network")||view.Type.Equals("Pathway")) {
                                var processDay = view.WeeklyFor;
                                int currentDay = view.WeeklyOn;

                                var currentDateTime = currDateTime;
                                if (currentDay > processDay)
                                    currentDateTime = currentDateTime.AddDays((currentDay - processDay) * -1);
                                else
                                    currentDateTime = currentDateTime.AddDays(processDay - currentDay - 7);

                                var startTime = view.WeeklyFrom;
                                var stopTime = view.WeeklyTo;
                                reportStartTime = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, startTime, 0, 0);

                                if (stopTime == 24)
                                    reportStopTime = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, 23, 59, 59);
                                else
                                    reportStopTime = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, stopTime, 0, 0);
                            }
                            else {
                                DateTime previousDate = currDateTime.AddDays(-7);
                                reportStartTime = new DateTime(previousDate.Year, previousDate.Month, previousDate.Day, 0, 0, 0);
                                //reportStopTime = new DateTime(currDateTime.Year, currDateTime.Month, (currDateTime.Day - 1), 23, 59, 59);
                                reportStopTime = new DateTime(currDateTime.Year, currDateTime.Month, currDateTime.Day, 23, 59, 59);
                                reportStopTime = reportStopTime.AddDays(-1);

                                var pinInfo = scheduleService.GetPinInfoFor(view.ScheduleId);

                                if (pinInfo.Rows.Count > 0) {
                                    if(!pinInfo.Rows[0].IsNull("IsLowPin"))
                                        isLowPin = Convert.ToBoolean(pinInfo.Rows[0]["IsLowPin"]);

                                    if (!pinInfo.Rows[0].IsNull("IsHighPin"))
                                        isHihgPin = Convert.ToBoolean(pinInfo.Rows[0]["IsHighPin"]);

                                    if (!pinInfo.Rows[0].IsNull("IsAllSubVol"))
                                        isAllSubVol = Convert.ToBoolean(pinInfo.Rows[0]["IsAllSubVol"]);

                                    if (!pinInfo.Rows[0].IsNull("SubVols"))
                                        subVols = pinInfo.Rows[0]["SubVols"].ToString();
                                }
                            }
                        }
                    }
                    else if (view.Frequency.Equals("Monthly")) {
                        if (view.Type.Equals("Quick Tuner") || view.Type.Equals("Deep Dive") || view.Type.Equals("Network") || view.Type.Equals("Pathway")) {
                            if (!view.IsMonthlyOn) {
                                var date = new DateTime(currDateTime.Year, currDateTime.Month, 1);
                                var dayOfWeek = MapDayOfWeek(view.MonthlyOnWeekDay);
                                //Check Frequency.
                                var newValue = date.AddDays((dayOfWeek < date.DayOfWeek ? 7 : 0) + dayOfWeek - date.DayOfWeek);
                                var finalDate = newValue.AddDays((view.MonthlyOn - 1) * 7);

                                if (currDateTime.Date == finalDate.Date) {
                                    match = true;
                                    //Get Report Date Time.
                                    if (!view.IsMonthlyFor) {
                                        var reportDayOfWeek = MapDayOfWeek(view.MonthlyFor);
                                        var reportDate = DateTime.Now.AddDays(-7);
                                        int delta = reportDayOfWeek - reportDate.DayOfWeek;
                                        var newReportDate = reportDate.AddDays(delta);
                                        var startTime = view.MonthlyFrom;
                                        var stopTime = view.MonthlyTo;

                                        reportStartTime = new DateTime(newReportDate.Year, newReportDate.Month, newReportDate.Day, startTime, 0, 0);
                                        if (stopTime == 24)
                                            reportStopTime = new DateTime(newReportDate.Year, newReportDate.Month, newReportDate.Day, 23, 59, 59);
                                        else
                                            reportStopTime = new DateTime(newReportDate.Year, newReportDate.Month, newReportDate.Day, stopTime, 0, 0);

                                    }
                                    else {
                                        var processDay = view.MonthlyFor;
                                        var startTime = view.MonthlyFrom;
                                        var stopTime = view.MonthlyTo;

                                        if (processDay == 0)
                                            processDay = DateTime.DaysInMonth(currDateTime.Year, currDateTime.Month);

                                        reportStartTime = new DateTime(currDateTime.Year, currDateTime.Month, processDay, startTime, 0, 0);
                                        if (stopTime == 24)
                                            reportStopTime = new DateTime(currDateTime.Year, currDateTime.Month, processDay, 23, 59, 59);
                                        else
                                            reportStopTime = new DateTime(currDateTime.Year, currDateTime.Month, processDay, stopTime, 0, 0);
                                    }
                                }
                            }
                            else { //Get Report Date Time.
                                if (!view.IsMonthlyFor) {
                                    var reportDayOfWeek = MapDayOfWeek(view.MonthlyFor);
                                    var reportDate = DateTime.Now.AddDays(-7);
                                    int delta = reportDayOfWeek - reportDate.DayOfWeek;
                                    var newReportDate = reportDate.AddDays(delta);
                                    var startTime = view.MonthlyFrom;
                                    var stopTime = view.MonthlyTo;

                                    reportStartTime = new DateTime(newReportDate.Year, newReportDate.Month, newReportDate.Day, startTime, 0, 0);
                                    if (stopTime == 24)
                                        reportStopTime = new DateTime(newReportDate.Year, newReportDate.Month, newReportDate.Day, 23, 59, 59);
                                    else
                                        reportStopTime = new DateTime(newReportDate.Year, newReportDate.Month, newReportDate.Day, stopTime, 0, 0);

                                }
                                else {
                                    var processDay = view.MonthlyFor;
                                    var startTime = view.MonthlyFrom;
                                    var stopTime = view.MonthlyTo;

                                    if (processDay == 0)
                                        processDay = DateTime.DaysInMonth(currDateTime.Year, currDateTime.Month);

                                    reportStartTime = new DateTime(currDateTime.Year, currDateTime.Month, processDay, startTime, 0, 0);
                                    if (stopTime == 24)
                                        reportStopTime = new DateTime(currDateTime.Year, currDateTime.Month, processDay, 23, 59, 59);
                                    else
                                        reportStopTime = new DateTime(currDateTime.Year, currDateTime.Month, processDay, stopTime, 0, 0);
                                }
                            }
                        }
                        else {
                            int iDate = view.MonthlyOn;
                            if (iDate == 0)
                                iDate = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month);

                            if (Convert.ToInt32(currDateTime.Day) == iDate) {
                                match = true;
                                if (!view.Overlap) {
                                    DateTime previousMonth = currDateTime.AddMonths(-1);
                                    reportStartTime = new DateTime(previousMonth.Year, previousMonth.Month, previousMonth.Day, 0, 0, 0);
                                    reportStopTime = new DateTime(previousMonth.Year, previousMonth.Month, DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month), 0, 0, 0);
                                } else {
                                    DateTime previousMonth = currDateTime.AddMonths(-1);
                                    reportStartTime = new DateTime(previousMonth.Year, previousMonth.Month, 1, 0, 0, 0);
                                    reportStopTime = new DateTime(previousMonth.Year, previousMonth.Month, DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month), 0, 0, 0).AddDays(1);
                                }
                            }
                        }

                    }
                    if (match) {
                        Log.InfoFormat("reportStartTime: {0}",reportStartTime);
                        Log.InfoFormat("reportStopTime: {0}",reportStopTime);
                        Log.InfoFormat("Email: {0}",view.Email);
                        
                        emailLists.Add(new EmailList {
                            ScheduleId = view.ScheduleId,
                            SystemSerial = view.SystemSerial,
                            SystemName = view.SystemName,
                            StartTime = reportStartTime,
                            StopTime = reportStopTime,
                            CustomerId = cusAnalyst.GetCustomerIDFor(view.Email),
                            DetailTypeId = view.DetailTypeId,
                            Email = view.Email,
                            ReportFromHour = view.ReportFromHour,
                            ReportToHour = view.ReportToHour,
                            IsLowPin = isLowPin,
                            IsHighPin = isHihgPin,
                            IsAllSubVol = isAllSubVol,
                            SubVols = subVols,
                            BatchProgram = view.BatchProgram,
                            BatchId = view.BatchId,
                            ReportDownloadId = view.ReportDownloadId,
                        });
                    }
                }
            }

            return emailLists;
        }

        private DayOfWeek MapDayOfWeek(int weekday) {
            var selectedWeekday = DayOfWeek.Friday;

            switch (weekday) {
                case 0:
                    selectedWeekday = DayOfWeek.Sunday;
                    break;
                case 1:
                    selectedWeekday = DayOfWeek.Monday;
                    break;
                case 2:
                    selectedWeekday = DayOfWeek.Tuesday;
                    break;
                case 3:
                    selectedWeekday = DayOfWeek.Wednesday;
                    break;
                case 4:
                    selectedWeekday = DayOfWeek.Thursday;
                    break;
                case 5:
                    selectedWeekday = DayOfWeek.Friday;
                    break;
                case 6:
                    selectedWeekday = DayOfWeek.Saturday;
                    break;
            }

            return selectedWeekday;
        }

        private void SendLoadEmail(DateTime starttime, DateTime stoptime, List<string> emailList, string systemSerial, string systemName, bool alertException, int scheduleId) {
            var dailyEmail = new DailyEmail(ConnectionString.EmailServer, ConnectionString.ServerPath,
                ConnectionString.EmailPort, ConnectionString.EmailUser, ConnectionString.EmailPassword, ConnectionString.EmailAuthentication,
                ConnectionString.SystemLocation, ConnectionString.AdvisorEmail, ConnectionString.ConnectionStringDB, ConnectionString.ConnectionStringSPAM,
                ConnectionString.ConnectionStringTrend, ConnectionString.SupportEmail, ConnectionString.WebSite,
                ConnectionString.EmailIsSSL, ConnectionString.IsLocalAnalyst,
                ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);

            dailyEmail.SendLoadEmail(starttime, stoptime, emailList, systemSerial, systemName, ConnectionString.SystemLocation, ConnectionString.DatabasePrefix, alertException, scheduleId);
        }
    }
}
