using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.Scheduler.Schedules
{
    public class EmailNotification
    {
        private static readonly ILog Log = LogManager.GetLogger("EmailNotification");
        public void Timer_ElapsedHourly(object source, System.Timers.ElapsedEventArgs e)
        {
            //Note: If more functions are called from here, they need to be called from 
            //      Scheduler.StartDailyEmailNotification
            CheckHourlyEmails();
        }

        public void Timer_ElapsedDaily(object source, System.Timers.ElapsedEventArgs e)
        {
            //Note: If more functions are called from here, they need to be called from 
            //      Scheduler.StartDailyEmailNotification
            CheckWeeklyEmails();
        }


        public void CheckHourlyEmails()
        {
            var notificationPreferenceService = new NotificationPreferenceService(ConnectionString.ConnectionStringDB);
            var hourlyList = notificationPreferenceService.GetEveryHourSystemsFor();

            Log.InfoFormat("hourlyList: {0}",hourlyList.Count);
            

            var systemTable = new System_tblService(ConnectionString.ConnectionStringDB);
            var dec = new Decrypt();

            foreach (var view in hourlyList)
            {
                //Get exprie date and time zone.
                var systemInfo = systemTable.GetEndDateFor(view.SystemSerial);
                var expireDate = Convert.ToDateTime(dec.strDESDecrypt(systemInfo.Values.First()).Split(' ')[1]);
                if (expireDate >= DateTime.Today)
                {
                    //Get System's Time.
                    int timeZoneIndex = systemTable.GetTimeZoneFor(view.SystemSerial);

                    //Get send hour.
                    var sendHour = new List<int>();
                    for (var x = view.StartHour; x < 24; x += view.EveryHour)
                    {
                        if (view.SystemSerial.Equals("076863") ||
                            view.SystemSerial.Equals("076862") ||
                            view.SystemSerial.Equals("077637"))
                        {
                            //By request from RJA, add three hours.
                            if (x < 21)
                                sendHour.Add(x + 3);
                            else
                            {
                                var newTime = (x + 3) - 24;
                                sendHour.Add(newTime);
                            }
                        }
                        else
                        {
                            //we are adding extra hour, due to the time gap that we receive customer's data.
                            if (!x.Equals(23))
                                sendHour.Add(x + 1);
                            else
                                sendHour.Add(0);
                        }
                    }

                    var localTime = TimeZoneInformation.ToLocalTime(timeZoneIndex, DateTime.Now);

                    Log.InfoFormat("System: {0}",view.SystemSerial);
                    Log.InfoFormat("System's Time: {0}",timeZoneIndex);
                    Log.InfoFormat("localTime: {0}",localTime);
                    

                    if (sendHour.Contains((localTime.Hour)))
                    {
                        //Since we added an extra hour, we need to subtract 1 hour to make the from and to time correct.
                        if (view.SystemSerial.Equals("076863") ||
                            view.SystemSerial.Equals("076862") ||
                            view.SystemSerial.Equals("077637"))
                        {
                            localTime = localTime.AddHours(-3);
                        }
                        else
                            localTime = localTime.AddHours(-1);

                        var reportStopTime = new DateTime(localTime.Year, localTime.Month, localTime.Day, localTime.Hour, 0, 0);
                        var reportStartTime = reportStopTime.AddHours(-view.EveryHour);

                        string systemName = systemTable.GetSystemNameFor(view.SystemSerial);

                        Log.InfoFormat("reportStartTime: {0}",reportStartTime);
                        Log.InfoFormat("reportStopTime: {0}",reportStopTime);
                        Log.InfoFormat("CustomerID: {0}",view.CustomerID);
                        

                        //var cust = new CusAnalystService(ConnectionString.ConnectionStringDB);
                        //var email = cust.GetEmailAddressFor(view.CustomerID);
                        //var custInfo = new Dictionary<int, string>();
                        //custInfo.Add(view.CustomerID, view.Email);

                        var emailList = new List<string> {view.Email};
                        SendLoadEmail(reportStartTime, reportStopTime, emailList, view.SystemSerial, systemName);
                    }

                }
                else
                {
                    //License Expired.
                }
            }
            
        }

        public void CheckDailyEmails()
        {
            var notificationPreferenceService = new NotificationPreferenceService(ConnectionString.ConnectionStringDB);
            var dailyList = notificationPreferenceService.GetEveryDailySystemsFor();

            var systemTable = new System_tblService(ConnectionString.ConnectionStringDB);
            var dec = new Decrypt();

            var emailLists = new List<EmailList>();

            Log.Info("*****************************************");
            Log.InfoFormat("dailyList: {0}",dailyList.Count);
            

            foreach (var view in dailyList)
            {
                try
                {
                    var extendTime = 1;
                    if (view.SystemSerial.Equals("076863") ||
                        view.SystemSerial.Equals("076862") ||
                        view.SystemSerial.Equals("077637"))
                    {
                        extendTime = 2;
                    }
                    //Get exprie date and time zone.
                    var systemInfo = systemTable.GetEndDateFor(view.SystemSerial);
                    var expireDate = Convert.ToDateTime(dec.strDESDecrypt(systemInfo.Values.First()).Split(' ')[1]);
                    if (expireDate >= DateTime.Today)
                    {
                        #region Build Start and Stop time and generate email.

                        //Get System's Time.
                        int timeZoneIndex = systemTable.GetTimeZoneFor(view.SystemSerial);

                        var localTime = TimeZoneInformation.ToLocalTime(timeZoneIndex, DateTime.Now);

                        Log.InfoFormat("System: {0}",view.SystemSerial);
                        Log.InfoFormat("System's Time: {0}",timeZoneIndex);
                        Log.InfoFormat("localTime: {0}",localTime);
                        

                        if (localTime.Hour.Equals(view.SendHour + extendTime))
                        {
                            //we are adding extra hour, due to the time gap that we receive customer's data.
                            DateTime reportStopTime;
                            DateTime reportStartTime;
                            string systemName = systemTable.GetSystemNameFor(view.SystemSerial);

                            if (view.IsPreviousDay)
                            {
                                DateTime previousDate = localTime.AddDays(-1);
                                //Default values.
                                reportStartTime = new DateTime(previousDate.Year, previousDate.Month, previousDate.Day, 0, 0, 0);
                                reportStopTime = new DateTime(previousDate.Year, previousDate.Month, previousDate.Day, 23, 59, 59);

                                Log.InfoFormat("reportStartTime: {0}",reportStartTime);
                                Log.InfoFormat("reportStopTime: {0}",reportStopTime);
                                Log.InfoFormat("CustomerID: {0}",view.CustomerID);
                                

                                emailLists.Add(new EmailList
                                {
                                    SystemSerial = view.SystemSerial,
                                    SystemName = systemName,
                                    StartTime = reportStartTime,
                                    StopTime = reportStopTime,
                                    CustomerId = view.CustomerID,
                                    Email = view.Email
                                });
                                //SendLoadEmail(reportStartTime, reportStopTime, view.CustomerID, view.SystemSerial, systemName);
                            }
                            else
                            {
                                reportStopTime = new DateTime(localTime.Year, localTime.Month, localTime.Day, view.SendHour, 0, 0);
                                reportStartTime = reportStopTime.AddHours(-view.LastHour);

                                Log.InfoFormat("reportStartTime: {0}",reportStartTime);
                                Log.InfoFormat("reportStopTime: {0}",reportStopTime);
                                Log.InfoFormat("CustomerID: {0}",view.CustomerID);
                                
                                emailLists.Add(new EmailList
                                {
                                    SystemSerial = view.SystemSerial,
                                    SystemName = systemName,
                                    StartTime = reportStartTime,
                                    StopTime = reportStopTime,
                                    CustomerId = view.CustomerID,
                                    Email = view.Email
                                });
                                //SendLoadEmail(reportStartTime, reportStopTime, view.CustomerID, view.SystemSerial, systemName);
                            }
                        }

                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("*******************************");
                    Log.ErrorFormat("EmailNotification Error 1: {0}",ex.Message);
                    
                }
            }


            //Since Daily is default, need to go through all the system.
            var allSystems = systemTable.GetLicenseDateFor();
            var cusAnalyst = new CusAnalystService(ConnectionString.ConnectionStringDB);

            foreach (var system in allSystems)
            {
                var expireDate = Convert.ToDateTime(dec.strDESDecrypt(system.Value).Split(' ')[1]);
                if (expireDate >= DateTime.Today)
                {
                    //Get all the users within System.
                    int companyID = systemTable.GetCompanyIDFor(system.Key);
                    if (companyID != 215)
                    {
                        var customerIDs = cusAnalyst.GetCustomersFor(companyID);
                        //Get System's Time.
                        int timeZoneIndex = systemTable.GetTimeZoneFor(system.Key);
                        var localTime = TimeZoneInformation.ToLocalTime(timeZoneIndex, DateTime.Now);

                        foreach (var customerID in customerIDs)
                        {
                            //Check if user and system has an entry on NotificationPreferences
                            bool isExits = notificationPreferenceService.CheckIsDailyFor(system.Key, customerID);

                            if (!isExits)
                            {
                                //Send out the email.
                                //Dafult value is at 5 AM for Previous Day.
                                try
                                {
                                    #region Build Start and Stop time and generate email.

                                    if (localTime.Hour.Equals(5))
                                    {
                                        string systemName = systemTable.GetSystemNameFor(system.Key);

                                        DateTime previousDate = localTime.AddDays(-1);
                                        //Default values.
                                        var reportStartTime = new DateTime(previousDate.Year, previousDate.Month, previousDate.Day, 0, 0, 0);
                                        var reportStopTime = new DateTime(previousDate.Year, previousDate.Month, previousDate.Day, 23, 59, 59);

                                        Log.InfoFormat("reportStartTime: {0}",reportStartTime);
                                        Log.InfoFormat("reportStopTime: {0}",reportStopTime);
                                        Log.InfoFormat("CustomerID: {0}",customerID);
                                        
                                        emailLists.Add(new EmailList
                                        {
                                            SystemSerial = system.Key,
                                            SystemName = systemName,
                                            StartTime = reportStartTime,
                                            StopTime = reportStopTime,
                                            CustomerId = customerID,
                                            Email = cusAnalyst.GetEmailAddressFor(customerID)
                                        });
                                        //SendLoadEmail(reportStartTime, reportStopTime, customerID, system.Key, systemName);
                                    }

                                    #endregion
                                }
                                catch (Exception ex)
                                {
                                    Log.Error("*******************************");
                                    Log.ErrorFormat("EmailNotification Error 2: {0}",ex.Message);
                                }
                            }
                        }
                    }
                }
            }


            Log.InfoFormat("emailLists: {0}",emailLists.Count);
            
            //Group the emailList by SystemSerial, SystemName, Start, Stop Time.
            var groupedList = emailLists.GroupBy(x => new { x.SystemSerial, x.SystemName, x.StartTime, x.StopTime }).ToList();

            Log.InfoFormat("***groupedList: {0}",groupedList.Count);
            

            foreach (var gList in groupedList)
            {
                try
                {
                    var newList = gList.Select(x => x.Email).Distinct().ToList();
                    //var newDicList = gList.ToDictionary(x => x.CustomerId, x => x.Email);

                    Log.InfoFormat("StartTime: {0}",gList.Key.StartTime);
                    Log.InfoFormat("StopTime: {0}",gList.Key.StopTime);
                    Log.InfoFormat("Emails: {0}",string.Join(",", newList));
                    Log.InfoFormat("SystemSerial: {0}",gList.Key.SystemSerial);
                    Log.InfoFormat("SystemName: {0}",gList.Key.SystemName);
                    

                    SendLoadEmail(gList.Key.StartTime, gList.Key.StopTime, newList, gList.Key.SystemSerial, gList.Key.SystemName);
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("*******************************");
                    Log.ErrorFormat("CheckDailyEmails: groupedList Error 2: {0}",ex);
                }
            }
            
        }

        public void CheckWeeklyEmails()
        {
            var notificationPreferenceService = new NotificationPreferenceService(ConnectionString.ConnectionStringDB);
            var dailyList = notificationPreferenceService.GetEveryWeeklySystemsFor();

            var systemTable = new System_tblService(ConnectionString.ConnectionStringDB);
            var dec = new Decrypt();

            var emailLists = new List<EmailList>();

            Log.Info("*****************************************");
            Log.InfoFormat("dailyList: {0}",dailyList.Count);
            

            foreach (var view in dailyList)
            {
                try
                {
                    //Get exprie date and time zone.
                    var systemInfo = systemTable.GetEndDateFor(view.SystemSerial);
                    var expireDate = Convert.ToDateTime(dec.strDESDecrypt(systemInfo.Values.First()).Split(' ')[1]);
                    if (expireDate >= DateTime.Today)
                    {
                        #region Build Start and Stop time and generate email.

                        //Get System's Time.
                        int timeZoneIndex = systemTable.GetTimeZoneFor(view.SystemSerial);

                        var localTime = TimeZoneInformation.ToLocalTime(timeZoneIndex, DateTime.Now);

                        Log.InfoFormat("System: {0}",view.SystemSerial);
                        Log.InfoFormat("System's Time: {0}",timeZoneIndex);
                        Log.InfoFormat("localTime: {0}",localTime);
                        
                        var currentDateTime = DateTime.Now;

                        if ((int)currentDateTime.DayOfWeek == view.SendWeek)
                        {
                            if (currentDateTime.Hour.Equals(view.WeekSendHour))
                            {
                                string systemName = systemTable.GetSystemNameFor(view.SystemSerial);

                                //Get date from LastWeek.
                                var currDateTime = currentDateTime.AddDays(view.LastWeek - (int)currentDateTime.DayOfWeek - 7);
                                var reportStartTime = new DateTime(currDateTime.Year, currDateTime.Month, currDateTime.Day, 0, 0, 0);
                                var reportStopTime = new DateTime(currDateTime.Year, currDateTime.Month, currDateTime.Day, 23, 59, 59);

                                //adjust start and stop time for VISA.
                                var systemTblService = new System_tblService(ConnectionString.ConnectionStringDB);
                                var isProcessDirectlySystem = systemTblService.isProcessDirectlySystemFor(view.SystemSerial);
                                if (isProcessDirectlySystem)
                                {
                                    reportStartTime = new DateTime(currDateTime.Year, currDateTime.Month, currDateTime.Day, 8, 0, 0);
                                    reportStopTime = new DateTime(currDateTime.Year, currDateTime.Month, currDateTime.Day, 16, 0, 0);
                                }

                                Log.InfoFormat("reportStartTime: {0}",reportStartTime);
                                Log.InfoFormat("reportStopTime: {0}",reportStopTime);
                                Log.InfoFormat("CustomerID: {0}",view.CustomerID);
                                
                                emailLists.Add(new EmailList
                                {
                                    SystemSerial = view.SystemSerial,
                                    SystemName = systemName,
                                    StartTime = reportStartTime,
                                    StopTime = reportStopTime,
                                    CustomerId = view.CustomerID,
                                    Email = view.Email
                                });
                            }
                        }
                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("*******************************");
                    Log.ErrorFormat("EmailNotification Error 1: {0}",ex.Message);
                    
                }
            }

            Log.InfoFormat("emailLists: {0}",emailLists.Count);
            
            //Group the emailList by SystemSerial, SystemName, Start, Stop Time.
            var groupedList = emailLists.GroupBy(x => new { x.SystemSerial, x.SystemName, x.StartTime, x.StopTime }).ToList();

            Log.InfoFormat("***groupedList: {0}",groupedList.Count);
            

            foreach (var gList in groupedList)
            {
                try
                {
                    var newList = gList.Select(x => x.Email).Distinct().ToList();
                    //var newDicList = gList.ToDictionary(x => x.CustomerId, x => x.Email);

                    Log.InfoFormat("StartTime: {0}",gList.Key.StartTime);
                    Log.InfoFormat("StopTime: {0}",gList.Key.StopTime);
                    Log.InfoFormat("Emails: {0}",string.Join(",", newList));
                    Log.InfoFormat("SystemSerial: {0}",gList.Key.SystemSerial);
                    Log.InfoFormat("SystemName: {0}",gList.Key.SystemName);
                    

                    SendLoadEmail(gList.Key.StartTime, gList.Key.StopTime, newList, gList.Key.SystemSerial, gList.Key.SystemName);
                }
                catch (Exception ex)
                {
                    Log.Error("*******************************");
                    Log.ErrorFormat("CheckWeeklyEmails: groupedList Error 2: {0}",ex.Message);
                    Log.ErrorFormat("CheckWeeklyEmails: groupedList Error 2: {0}",ex.StackTrace);
                    
                }
            }
        }

        /// <summary>
        /// SendLoadEmail generate email with CPU Walk-Through graph and user defind alerts.
        /// </summary>
        /// <param name="starttime">Email content Start Time</param>
        /// <param name="stoptime">Email content Stop Time</param>
        /// <param name="emailList">Email List</param>
        /// <param name="systemSerial">System Serial Number</param>
        /// <param name="systemName">System Name</param>
        public void SendLoadEmail(DateTime starttime, DateTime stoptime, List<string> emailList, string systemSerial, string systemName)
        {
            var dailyEmail = new DailyEmail(ConnectionString.EmailServer, ConnectionString.ServerPath, 
                ConnectionString.EmailPort, ConnectionString.EmailUser, ConnectionString.EmailPassword, ConnectionString.EmailAuthentication,
                ConnectionString.SystemLocation, ConnectionString.AdvisorEmail, ConnectionString.ConnectionStringDB, ConnectionString.ConnectionStringSPAM,
                ConnectionString.ConnectionStringTrend, ConnectionString.SupportEmail, ConnectionString.WebSite,
                ConnectionString.EmailIsSSL, ConnectionString.IsLocalAnalyst, 
                ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);

            dailyEmail.SendLoadEmail(starttime, stoptime, emailList, systemSerial, systemName, ConnectionString.SystemLocation, ConnectionString.DatabasePrefix);
        }

        /*public void SendLoadEmailWeekly(DateTime starttime, DateTime stoptime, List<int> customerIds, string systemSerial, string systemName, bool monthly)
        {
            var weeklyEmail = new WeeklyEmail(ConnectionString.EmailServer, ConnectionString.ServerPath, 
                ConnectionString.EmailPort, ConnectionString.EmailUser, ConnectionString.EmailPassword, ConnectionString.EmailAuthentication,
                ConnectionString.SystemLocation, ConnectionString.AdvisorEmail, ConnectionString.ConnectionStringDB, ConnectionString.ConnectionStringSPAM,
                ConnectionString.ConnectionStringTrend, ConnectionString.SupportEmail, ConnectionString.WebSite);

            weeklyEmail.SendLoadEmailWeekly(starttime, stoptime, customerIds, systemSerial, systemName, monthly);
        }*/
    }
}
