using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading;
using AdvancedIntellect.Ssl;
using log4net;
using Mailgun.Core.Messages;
using Mailgun.Messages;
using Mailgun.Service;
using RemoteAnalyst.BusinessLogic.BLL;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.BusinessLogic.ModelService;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.BusinessLogic.Email {
    public class DailyEmail {
        private readonly string _connectionStringDB = "";
        private readonly string _connectionStringSPAM = "";
        private readonly string _connectionStringTrend = "";
        private readonly string _serverPath = "";
        private readonly string _supportEmail = "";
        private readonly string _systemLocation = "";
        private readonly string _webSite = "";
        private readonly bool _isLocalAnalyst;
        private readonly EmailManager _emailManager;
        private static readonly ILog Log = LogManager.GetLogger("EmailError");

        public DailyEmail(string emailServer
            , string serverPath
            , int emailPort
            , string emailUser
            , string emailPassword
            , bool emailAuthentication
            , string systemLocation
            , string advisorEmail
            , string connectionStringDB
            , string connectionStringSPAM
            , string connectionStringTrend
            , string supportEmail
            , string webSite
            , bool isSSL
            , bool isLocalAnalyst
            , string mailGunSendAPIKey
            , string mailGunSendDomain)
        {
            _serverPath = serverPath;
            _systemLocation = systemLocation;
            _connectionStringDB = connectionStringDB;
            _connectionStringSPAM = connectionStringSPAM;
            _connectionStringTrend = connectionStringTrend;
            _supportEmail = supportEmail;
            _isLocalAnalyst = isLocalAnalyst;
            if (webSite.EndsWith("/"))
                webSite = webSite.Remove(webSite.Length - 1, 1);

            _webSite = webSite;
            _emailManager = new EmailManager(emailServer
                                                , serverPath
                                                , emailPort
                                                , emailUser
                                                , emailPassword
                                                , emailAuthentication
                                                , systemLocation
                                                , advisorEmail
                                                , supportEmail
                                                , webSite
                                                , isSSL
                                                , isLocalAnalyst
                                                , mailGunSendAPIKey
                                                , mailGunSendDomain);
        }

        /// <summary>
        ///     SendLoadEmail generate email with CPU Walk-Through graph and user defind alerts.
        /// </summary>
        /// <param name="starttime">Email content Start Time</param>
        /// <param name="stoptime">Email content Stop Time</param>
        /// <param name="emailList">Email List</param>
        /// <param name="systemSerial">System Serial Number</param>
        /// <param name="systemName">System Name</param>
        /// <param name="tempSaveLocation">CSV File Location</param>
        public void SendLoadEmail(DateTime starttime, DateTime stoptime, List<string> emailList, string systemSerial, string systemName, string tempSaveLocation, string databasePrefix, bool alertException = false, int scheduleId = -1) {
            var totalAttach = 0;
            try {
                var userEmail = "";
                var firstname = "";
                var lastname = "";

                var systemTableService = new System_tblService(_connectionStringDB);
                var longDatePattern = systemTableService.GetLongDatePatternFor(systemSerial);

                var databaseMappingService = new DatabaseMappingService(_connectionStringDB);
                var connectionStringSystem = databaseMappingService.GetConnectionStringFor(systemSerial);
                if (connectionStringSystem.Length == 0) {
                    connectionStringSystem = _connectionStringSPAM;
                }
                var cpuTableName = new List<string>();
                var processTableName = new List<string>();
                for (var start = starttime; start.Date < stoptime; start = start.AddDays(1)) {
                    cpuTableName.Add(systemSerial + "_CPU_" + start.Year + "_" + start.Month + "_" + start.Day);
                    processTableName.Add(systemSerial + "_PROCESS_" + start.Year + "_" + start.Month + "_" + start.Day);
                }

                //Check if we have CPU table.
                var chart = new JobProcessorChart(_connectionStringDB, _connectionStringTrend, connectionStringSystem, _serverPath);
                var cpudataExists = chart.CheckTableFor(systemSerial, cpuTableName);
                var processdataExists = chart.CheckTableFor(systemSerial, processTableName);
                var emailHeader = new EmailHeaderFooter();
                //string strPeriod;
                var timediff = Convert.ToDateTime(stoptime.ToShortDateString()).Subtract(Convert.ToDateTime(starttime.ToShortDateString()));

                if (!cpudataExists || !processdataExists) {
                    if (!_isLocalAnalyst) {
                        #region Send No Data Email

                        var email_subject = systemName + " Dailies for ";
                        if (timediff.TotalDays > 0) {
                            email_subject += Convert.ToDateTime(starttime).ToString(longDatePattern) + " thru " + Convert.ToDateTime(stoptime).ToString(longDatePattern);
                        }
                        else {
                            email_subject += Convert.ToDateTime(starttime).ToString(longDatePattern);
                        }
                        
                        //No need to send the notification.
                        var email_body = emailHeader.EmailHeaderNewEmail(firstname, lastname, _isLocalAnalyst);

                        //Send no data email.
                        //email body
                        email_body += "<p>No Data Available for \"" + systemName +
                                            "\" for the following Collection: <br />";
                        // during the <br>" + strPeriod;
                        email_body += "<ul>";
                        var strStopHour = stoptime.ToString("HH:mm");
                        if (timediff.TotalDays > 0) {
                            email_body += "<li>Period: " + starttime.ToString("HH:mm") + ", "
                                                + starttime.ToString(longDatePattern)
                                                + " - " + strStopHour + ", " + stoptime.ToString(longDatePattern) +
                                                " </li>";
                        }
                        else
                            email_body += "<li>Period: " + starttime.ToString("HH:mm") + " - " +
                                                strStopHour + ", " +
                                                starttime.ToString(longDatePattern) + " </li>";
                        email_body += "</ul>";
                        email_body += "</p>";
                        email_body += "<p><b>Support team has been notified.<b></p>";
                        email_body += emailHeader.EmailFooterWithOutBlockquote(_supportEmail, _webSite);
                        try {
                            _emailManager.SendEmail(_supportEmail, email_subject, email_body);
                            Log.Info("Sending Email to customer");
                            
                        }
                        catch (Exception ex) {
                            Log.ErrorFormat("Email Error: {0}", ex);
                        }
                        return;

                        #endregion
                    }
                }

                var currentTableService = new CurrentTableService(connectionStringSystem);
                var interval = currentTableService.GetIntervalFor(cpuTableName);
                var emailDetail = DetailEmailContents(connectionStringSystem, systemSerial, starttime, stoptime, cpuTableName, interval, alertException, tempSaveLocation, databasePrefix, scheduleId);

                if (emailDetail.HourDrop) {
                    if (!_isLocalAnalyst) {
                        #region Send Data Drop Email

                        //No need to send the notification.
                        var email_body = emailHeader.EmailHeaderNewEmail(firstname, lastname, _isLocalAnalyst);

                        //Send data drop email.
                        //email body
                        email_body += "<p>Data Drop for \"" + systemName +
                                            "\" for the following Collection: <br />";
                        // during the <br>" + strPeriod;
                        email_body += "<ul>";
                        var strStopHour = stoptime.ToString("HH:mm");
                        if (timediff.TotalDays > 0)
                        {
                            foreach (var period in emailDetail.HourDropPeriods)
                            {
                                email_body += "<li>Period: " + period[0].ToString("HH:mm") + ", "
                                                    + period[0].ToString(longDatePattern)
                                                    + " - " + period[1].ToString("HH:mm") + ", " + period[1].ToString(longDatePattern) +
                                                    " </li>";
                            }
                        }
                        else
                        {
                            foreach (var period in emailDetail.HourDropPeriods)
                            {
                                email_body += "<li>Period: " + period[0].ToString("HH:mm") + " - " +
                                                period[1].ToString("HH:mm") + ", " +
                                                period[0].ToString(longDatePattern) + " </li>";
                            }
                        }
                        email_body += "</ul>";
                        email_body += "</p>";
                        email_body += "<p><b>Support team has been notified.<b></p>";
                        email_body += emailHeader.EmailFooterWithOutBlockquote(_supportEmail, _webSite);
                        var email_subject = "Data Drop for \\" + systemName;

                        try {
                            _emailManager.SendEmail(_supportEmail, email_subject, email_body);
                            Log.Info("Sending Email to customer");                            
                        }
                        catch (Exception ex) {
                            Log.ErrorFormat("Email Error: {0}", ex);          
                        }
                        #endregion
                    }
                }

                foreach (var email in emailList) {
                    try {
                        userEmail = email;
                        Log.InfoFormat("userEmail: {0}", userEmail);
                        Log.Info("Start email creation");                      

                        if (userEmail.Length == 0)
                            continue;

                        //set the subject and body
                        Log.Info("Build email header");                        

                        var email_subject = systemName + " Dailies for ";
                        if (timediff.TotalDays > 0)
                            email_subject += Convert.ToDateTime(starttime).ToString(longDatePattern) + " thru " + Convert.ToDateTime(stoptime).ToString(longDatePattern);
                        else
                            email_subject += Convert.ToDateTime(starttime).ToString(longDatePattern);

                        #region Send Daily Email.

                        var totFileAttach = 0;
                        if (totFileAttach + totalAttach > 0) {
                            var total = totFileAttach + totalAttach;
                            if (total > 1)
                                email_subject += " - " + total + " Reports attached";
                            if (total == 1)
                                email_subject += " - " + total + " Report attached";
                        }

                        //email body
                        var emailTitle = new StringBuilder();
                        emailTitle.Append(
                            "<a name='top'></a><h3 style='margin-top: 0;margin-bottom: 0;font-size: 15px;font-family: Calibri;color: inherit;'>" +
                            systemName + " Daily Report for ");
                        if (timediff.TotalDays > 0) {
                            emailTitle.Append(starttime.ToString(longDatePattern) + " " +
                                              starttime.ToString("HH:mm") + " - " +
                                              stoptime.ToString(longDatePattern) + " " +
                                              stoptime.ToString("HH:mm"));
                        }
                        else {
                            emailTitle.Append(starttime.ToString(longDatePattern) + " " +
                                              starttime.ToString("HH:mm") + " - " + stoptime.ToString("HH:mm"));
                        }
                        if (alertException) {
                            emailTitle.Append(" - Exceptions Noted ");
                        }
                        emailTitle.Append("</h3>");
						emailTitle.Append("<a></a>");

                        var email_body = emailHeader.EmailHeaderDailyEmail(emailTitle.ToString(), _isLocalAnalyst);

                        //Insert Detail Body.
                        var encrypt = new Decrypt();
                        email_body += emailDetail.Content.Replace("@User", encrypt.strDESEncrypt(userEmail));
                        email_body += emailHeader.EmailFooterWithOutBlockquote(_supportEmail, _webSite);
                        try {
                            _emailManager.SendEmailWithEmailDetail(userEmail, email_subject, email_body, emailDetail);
                            Log.Info("Sending Email to customer");
                        }
                        catch (Exception ex) {
                            Log.ErrorFormat("ConsolidatedEmail at {0}", ex);
                        }
                        #endregion
                    }
                    catch (Exception ex) {
                        Log.ErrorFormat("Email Error for : {0}", email);
                        Log.Error(ex.Message);                        
                    }
                }
                try { 
                    Thread.Sleep(60*1000); // Wait for 1 minute to th emails to be sent out before deleting the charts
                }
                catch (Exception e)
                {

                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("Email Error: {0}", ex);              
            }
        }

        public EmailContent DetailEmailContents(string connectionStringSystem, string systemSerial, DateTime starttime,
            DateTime stoptime, List<string> cpuTableNames, long interval, bool alertException, string tempSaveLocation, string databasePrefix, int scheduleId) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            string dateFormat = "yyyy-MM-dd HH:mm:ss";

            var emailContent = new EmailContent();


            var systemTableService = new System_tblService(_connectionStringDB);
            var monthDayPattern = systemTableService.GetMonthDayPatternFor(systemSerial);
            var longDatePattern = systemTableService.GetLongDatePatternFor(systemSerial);

            var encrypt = new Decrypt();
            var encryptSystemSerial = encrypt.strDESEncrypt(systemSerial);

            var chart = new JobProcessorChart(_connectionStringDB, _connectionStringTrend, connectionStringSystem, _serverPath);

            //Insert Top 20 DISC
            var diskBrowserService = new DiskBrowserService(connectionStringSystem);
            var top20Disk = diskBrowserService.GetTop20DisksFor(systemSerial, starttime, stoptime, _isLocalAnalyst, monthDayPattern, _webSite);

            var cpuEntityTableService = new CPUEntityTableService(connectionStringSystem);
            var pageSizeBytes = cpuEntityTableService.GetPageSizeBytesFor(cpuTableNames);
            //Insert Top 20 by Process Busy
            var processEntityTableService = new ProcessEntityTableService(connectionStringSystem);
            var discNames = processEntityTableService.GetDP2ProcessesFor(systemSerial, starttime, stoptime);
            var isIPU = processEntityTableService.CheckIPUFor(systemSerial, starttime, stoptime, databasePrefix);

            var processBusyStatic = processEntityTableService.GetTop20ProcessByBusyStaticFor(systemSerial, starttime,
                stoptime, pageSizeBytes, interval, discNames, isIPU, _isLocalAnalyst, monthDayPattern, _webSite);
            var processBusyDynamic = processEntityTableService.GetTop20ProcessByBusyDynamicFor(systemSerial, starttime,
                stoptime, pageSizeBytes, interval, discNames, isIPU, _isLocalAnalyst, monthDayPattern, _webSite);
            //Insert Top 20 by Receive Queue
            var processQueueStatic = processEntityTableService.GetTop20ProcessByQueueStaticFor(systemSerial, starttime,
                stoptime, pageSizeBytes, interval, discNames, isIPU, _isLocalAnalyst, monthDayPattern, _webSite);
            var processQueueDynamic = processEntityTableService.GetTop20ProcessByQueueDynamicFor(systemSerial, starttime,
                stoptime, pageSizeBytes, interval, discNames, isIPU, _isLocalAnalyst, monthDayPattern, _webSite);
            //Insert Top 20 TMF Abort
            var processAbort = processEntityTableService.GetTop20ProcessByAbortFor(systemSerial, starttime, stoptime,
                pageSizeBytes, isIPU, _isLocalAnalyst, monthDayPattern, _webSite);

            var dailyDiskService = new DailyDiskService(connectionStringSystem);
            ScheduleService scheduleService = new ScheduleService(_connectionStringDB);
            var ignoreVolumeArray = scheduleService.GetIgnoreVolumesFor(scheduleId);
            DataTable scheduleStorageThresholds = scheduleService.GetScheduleStorageThresholdFor(scheduleId);
            var storage = dailyDiskService.GetDailyDiskInfoFor(systemSerial, starttime, ignoreVolumeArray, scheduleStorageThresholds);
            var storageGraphData = dailyDiskService.GetDiskGraphData(starttime);


            var dailyPorcessDatas = new DailyProcessDatas(connectionStringSystem);
            //Get last Week data.
            var lastWeekStartTime = starttime.AddDays(-7);
            var lastWeekStopTime = stoptime.AddDays(-7);
            var lastWeek = dailyPorcessDatas.GetPastData(lastWeekStartTime, lastWeekStopTime);

            //Get last month data.
            var lastMonthStartTime = Helper.GetLastMonthDate(starttime);
            var lastMonthStopTime = Helper.GetLastMonthDate(stoptime);
            var lastMonth = dailyPorcessDatas.GetPastData(lastMonthStartTime, lastMonthStopTime);

            var emailMessage = new StringBuilder();
            var emailMessageToc = new StringBuilder();
            var emailMessageGrid = new StringBuilder();
            var hourDrop = false;
            var hourDropPeriods = new List<System.DateTime[]>();

            #region Grid
            var dailyEmailUtil = new DailyEmailUtil();
            //var cpuBusyGridStructure = dailyEmailUtil.GenerateGridDataTable(24);

            var cpuBusyGridStructure = dailyEmailUtil.GenerateGridDataTable(starttime, stoptime);
            var cpuBusyGrid = cpuBusyGridStructure.Clone();
            var cpuQueueGrid = cpuBusyGridStructure.Clone();
            var ipuBusyGrid = cpuBusyGridStructure.Clone();
            var ipuQueueGrid = cpuBusyGridStructure.Clone();
            var diskGrid = cpuBusyGridStructure.Clone();
            var storageGrid = cpuBusyGridStructure.Clone();
            #endregion

            var cpuBusyColor = new Color();
            var cpuQueueColor = new Color();
            var ipuBusyColor = new Color();
            var ipuQueueColor = new Color();

            bool diskAlertError = false;
            bool storageAlertError = false;
            #region Forecast Data
            //Get Tolernace value;
            var toleranceInfo = systemTableService.GetToleranceFor(systemSerial);

            var businessTolerance = toleranceInfo.Rows[0].IsNull("BusinessTolerance") ? 5.0 : Convert.ToDouble(toleranceInfo.Rows[0]["BusinessTolerance"]);
            var batchTolerance = toleranceInfo.Rows[0].IsNull("BatchTolerance") ? 10.0 : Convert.ToDouble(toleranceInfo.Rows[0]["BatchTolerance"]);
            var otherTolerance = toleranceInfo.Rows[0].IsNull("OtherTolerance") ? 12.0 : Convert.ToDouble(toleranceInfo.Rows[0]["OtherTolerance"]);

            var forecast = new ForecastService(systemSerial, _connectionStringDB, connectionStringSystem);
            var forecastCpu = forecast.GetForecastCpuDataFor(starttime, stoptime);
            var forecastIpu = forecast.GetForecastIpuDataFor(starttime, stoptime);
            var forecastDisk = forecast.GetForecastDiskDataFor(starttime, stoptime);
            var forecastStorage = forecast.GetForecastStorageDataFor(starttime.Date, stoptime.Date);
            var forecastProcess = forecast.GetForecastProcessDataFor(starttime, stoptime);
            var forecastTmf = forecast.GetForecastTmfDataFor(starttime, stoptime);

            Log.InfoFormat("forecastCpu: {0}, forecastIpu: {1}, forecastDisk: {2}, ",
                forecastCpu.Count, forecastIpu.Count, forecastDisk.Count);
            Log.InfoFormat("forecastStorage: {0}, forecastProcess: {1}, forecastTmf: {2}, ",
                forecastStorage.Count, forecastProcess.Count, forecastTmf.Count);            

            var systemWeek = new SystemWeekService(systemSerial, _connectionStringDB);
            var systemWeekInfo = systemWeek.GetSystemWeek();
            #endregion

#if DEBUG
            alertException = false;
#endif
            if (alertException) {
                var exceptionServices = new ExceptionService(connectionStringSystem);

                Log.Info("***********Calling GetCPUBusyAlertColor****************");                
                var cpuBusyGridReturned = exceptionServices.GetExceptionFor(starttime, stoptime, "CPU", "Busy");
                var cpuBusyDataAndColor = dailyEmailUtil.CleanupGridDataTable(cpuBusyGridReturned);

                Log.Info("***********Calling GetCPUQueueAlertColor****************");                
                var cpuQueueColorReturned = exceptionServices.GetExceptionFor(starttime, stoptime, "CPU", "Queue");
                var cpuQueueDataAndColor = dailyEmailUtil.CleanupGridDataTable(cpuQueueColorReturned);
                
                Log.Info("***********Calling GetIPUBusyAlertColor****************");                
                var ipuBusyGridReturned = exceptionServices.GetExceptionFor(starttime, stoptime, "IPU", "Busy");
                var ipuBusyDataAndColor = dailyEmailUtil.CleanupGridDataTable(ipuBusyGridReturned);

                Log.Info("***********Calling GetIpuQueueAlertColor****************");                
                var ipuQueueGridReturned = exceptionServices.GetExceptionFor(starttime, stoptime, "IPU", "Queue");
                var ipuQueueDataAndColor = dailyEmailUtil.CleanupGridDataTable(ipuQueueGridReturned);
                
                var diskQueueDataAndColor = new DataSet();
                var diskDp2DataAndColor = new DataSet();
                var storageDataAndColor = new DataSet();

                try {
                    Log.Info("***********Calling GetDiskDP2AlertColor****************");                    
                    var diskDp2GridReturned = exceptionServices.GetExceptionFor(starttime, stoptime, "Disk", "DP2");
                    diskDp2DataAndColor = dailyEmailUtil.CleanupGridDataTable(diskDp2GridReturned);
                }
                catch (Exception ex) {
                    Log.ErrorFormat("GetDiskQueueAlertColor Error: {0}", ex);
                    diskAlertError = true;
                }

                try {
                    Log.Info("***********Calling GetDiskQueueAlertColor****************");                    
                    var diskQueueGridReturned = exceptionServices.GetExceptionFor(starttime, stoptime, "Disk", "Queue");
                    diskQueueDataAndColor = dailyEmailUtil.CleanupGridDataTable(diskQueueGridReturned);
                }
                catch (Exception ex) {
                    Log.ErrorFormat("GetDiskQueueAlertColor Error: {0}", ex);                    
                    diskAlertError = true;
                }

                try {
                    Log.Info("***********Calling GetStorageAlertColor****************");
                    
                    var storageGridReturned = new DataTable();
                    if (starttime.Date == stoptime.Date)
                        storageGridReturned = exceptionServices.GetExceptionFor(starttime, stoptime, "Storage", "Used%");
                    else
                        storageGridReturned = exceptionServices.GetExceptionFor(starttime.Date, stoptime.Date, "Storage", "Used%");

                    storageDataAndColor = dailyEmailUtil.CleanupGridDataTable(storageGridReturned);
                }
                catch (Exception ex) {
                    Log.ErrorFormat("GetStorageAlertColor Error: {0}", ex);
                    
                    storageAlertError = true;
                }

                var dataAndColor = dailyEmailUtil.MergeDataAndColor(cpuBusyDataAndColor, cpuQueueDataAndColor, ipuBusyDataAndColor, ipuQueueDataAndColor, diskQueueDataAndColor, diskDp2DataAndColor, storageDataAndColor);
                var htmlGrid = chart.CreateObjectsGrid(dataAndColor, encryptSystemSerial, starttime, stoptime, _isLocalAnalyst, _webSite);
				Log.Info("***********HTML text****************");
				Log.Info(htmlGrid);
				
                emailMessageGrid.Append(htmlGrid);

                //Save the alert value to the database.

            }
            else {
                Log.Info("***********Calling GetCPUBusyAlertColor****************");
                cpuBusyColor = chart.GetCPUBusyAlertColor(systemSerial, starttime, stoptime, interval, systemWeekInfo, businessTolerance, batchTolerance, otherTolerance, forecastCpu, alertException, Log);
                Log.Info("***********Calling GetCPUQueueAlertColor****************");
                cpuQueueColor = chart.GetCPUQueueAlertColor(systemSerial, starttime, stoptime, interval, systemWeekInfo, businessTolerance, batchTolerance, otherTolerance, forecastCpu, alertException, Log);
                Log.Info("***********Calling GetIPUBusyAlertColor****************");
                ipuBusyColor = chart.GetIPUBusyAlertColor(systemSerial, starttime, stoptime, interval, systemWeekInfo, businessTolerance, batchTolerance, otherTolerance, forecastIpu, alertException, Log);
                Log.Info("***********Calling GetIpuQueueAlertColor****************");
                ipuQueueColor = chart.GetIpuQueueAlertColor(systemSerial, starttime, stoptime, interval, systemWeekInfo, businessTolerance, batchTolerance, otherTolerance, forecastIpu, alertException, Log);
            }

            #region CPU Charts

            try {
                var cpubusyCharts = chart.CreateChartPerInterval(systemSerial, starttime, stoptime, _serverPath,
                    Log, interval, ref hourDrop, ref hourDropPeriods);
                if (cpubusyCharts.Length > 0) {
                    var encryptStartTime = encrypt.strDESEncrypt(starttime.ToShortDateString());
                    var chartPath = cpubusyCharts.Split(',');

                    if (chartPath[0].Length > 0 && chartPath[1].Length > 0) {
                        emailMessage.Append("<a name=CpuBusyGraph></a>");
                        emailMessage.Append(
                            @"<table style='background-color:#ffffff;border-radius: 3px;border: #cccccc 1px solid; width:100%;' cellpadding = 0 cellspacing = 0>
	                                          <tr ><td style='padding:5px 15px;color:#212121;background-color:#B7B7B7;border-color:#dddddd;'><h3 style='margin-top: 0;margin-bottom: 0;font-size: 15px;font-family: Calibri;color: inherit;'>CPU Busy</h3>
	                                          </td><td style='padding:5px 15px;color:#212121;background-color:#B7B7B7;border-color:#dddddd;text-align: right;'>");
                        if (!_isLocalAnalyst)
                            emailMessage.Append(@"<a href='" + _webSite + "/EmailLogin.aspx?User=@User&System=" + encryptSystemSerial + "&StartDate=" + encryptStartTime + @"'><img src='cid:cloud_blue.png' width='30'></a>");
                        emailMessage.Append(@"</td></tr><tr><td colspan=2>");
                        emailContent.CPUBusy = chartPath[0];
                        emailMessage.Append("<IMG src='cid:CPUBusy'>");
                        //var cpuBusyTable = chart.GenerateCPUBusyTable(systemSerial, starttime, stoptime, interval);
                        //emailMessage.Append(cpuBusyTable);
                        emailMessage.Append("</td></tr></table>");
                        emailMessage.Append(
                            "<div style=\"text-align:right;font-size: 7pt; font-family: Calibri;width:850px;\"><a href='#top' style='color:black'><b><u>Back to top</u></b></a></div>\n");

                        emailMessage.Append("<br>");
                        emailMessage.Append("<a name=CPUQueueLengthGraph></a>");
                        emailMessage.Append(
                            @"<table style='background-color:#ffffff;border-radius: 3px;border: #cccccc 1px solid; width:100%;' cellpadding = 0 cellspacing = 0>
	                                        <tr ><td style='padding:5px 15px;color:#212121;background-color:#B7B7B7;border-color:#dddddd;'><h3 style='margin-top: 0;margin-bottom: 0;font-size: 15px;font-family: Calibri;color: inherit;'>CPU Queue</h3>
	                                        </td><td style='padding:5px 15px;color:#212121;background-color:#B7B7B7;border-color:#dddddd;text-align: right;'>");
                        if (!_isLocalAnalyst)
                            emailMessage.Append("<a href='" + _webSite + "/EmailLogin.aspx?User=@User&System=" + encryptSystemSerial + "&StartDate=" + encryptStartTime + @"'><img src='cid:cloud_blue.png' width='30'></a>");
                        emailMessage.Append("</td></tr><tr><td colspan=2>");
                        emailContent.CPUQueue = chartPath[1];
                        emailMessage.Append("<IMG src='cid:CPUQueue'>");
                        //var cpuQueueTable = chart.GenerateCPUQueueTable(systemSerial, starttime, stoptime, interval);
                        //emailMessage.Append(cpuQueueTable);
                        emailMessage.Append("</td></tr></table>");
                        emailMessage.Append(
                            "<div style=\"text-align:right;font-size: 7pt; font-family: Calibri;width:850px;\"><a href='#top' style='color:black'><b><u>Back to top</u></b></a></div>\n");
                        emailMessage.Append("<br>");
                    }
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("CPU Error: {0}", ex);
            }

            #endregion

            #region IPU Charts

            try {
                var ipubusyCharts = chart.CreateIPUChartPerInterval(systemSerial, starttime, stoptime, _serverPath,
                    Log, interval);
                if (ipubusyCharts.Length > 0) {
                    var encryptStartTime = encrypt.strDESEncrypt(starttime.ToShortDateString());
                    var chartPath = ipubusyCharts.Split(',');

                    if (chartPath[0].Length > 0 && chartPath[1].Length > 0) {
                        emailMessage.Append("<a name=IpuBusyGraph></a>");
                        emailMessage.Append(
                            @"<table style='background-color:#ffffff;border-radius: 3px;border: #cccccc 1px solid; width:100%;' cellpadding = 0 cellspacing = 0>
	                                          <tr ><td style='padding:5px 15px;color:#212121;background-color:#B7B7B7;border-color:#dddddd;'><h3 style='margin-top: 0;margin-bottom: 0;font-size: 15px;font-family: Calibri;color: inherit;'>IPU Busy</h3>
	                                          </tr><tr><td colspan=2>");
                        emailContent.IPUBusy = chartPath[0];
                        emailMessage.Append("<IMG src='cid:IPUBusy'>");
                        emailMessage.Append("</td></tr></table>");
                        emailMessage.Append(
                            "<div style=\"text-align:right;font-size: 7pt; font-family: Calibri;width:850px;\"><a href='#top' style='color:black'><b><u>Back to top</u></b></a></div>\n");

                        emailMessage.Append("<br>");
                        emailMessage.Append("<a name=IPUQueueLengthGraph></a>");
                        emailMessage.Append(
                            @"<table style='background-color:#ffffff;border-radius: 3px;border: #cccccc 1px solid; width:100%;' cellpadding = 0 cellspacing = 0>
	                                        <tr ><td style='padding:5px 15px;color:#212121;background-color:#B7B7B7;border-color:#dddddd;'><h3 style='margin-top: 0;margin-bottom: 0;font-size: 15px;font-family: Calibri;color: inherit;'>IPU Queue</h3>
	                                        </td></tr><tr><td colspan=2>");
                        emailContent.IPUQueue = chartPath[1];
                        emailMessage.Append("<IMG src='cid:IPUQueue'>");
                        emailMessage.Append("</td></tr></table>");
                        emailMessage.Append(
                            "<div style=\"text-align:right;font-size: 7pt; font-family: Calibri;width:850px;\"><a href='#top' style='color:black'><b><u>Back to top</u></b></a></div>\n");
                        emailMessage.Append("<br>");
                    }
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("IPU Error: {0}", ex);
            }

            #endregion

            #region Application Charts

            var applicationBusyChart = "";
            try {
                applicationBusyChart = chart.CreateApplicationChartPerInterval(systemSerial, starttime, stoptime, _serverPath, Log, interval, ref hourDrop);
                if (applicationBusyChart.Length > 0) {
                    var encryptStartTime = encrypt.strDESEncrypt(starttime.ToShortDateString());
                    
                    emailMessage.Append("<a name=ApplicationBusyGraph></a>");
                    emailMessage.Append(
                        @"<table style='background-color:#ffffff;border-radius: 3px;border: #cccccc 1px solid; width:100%;' cellpadding = 0 cellspacing = 0>
	                                        <tr ><td style='padding:5px 15px;color:#212121;background-color:#B7B7B7;border-color:#dddddd;'><h3 style='margin-top: 0;margin-bottom: 0;font-size: 15px;font-family: Calibri;color: inherit;'>Application Busy</h3>
	                                        </td><td style='padding:5px 15px;color:#212121;background-color:#B7B7B7;border-color:#dddddd;text-align: right;'>");
                    emailMessage.Append(@"</td></tr><tr><td colspan=2>");
                    emailContent.ApplicationBusy = applicationBusyChart;
                    emailMessage.Append("<IMG src='cid:ApplicationBusy'>");
                    emailMessage.Append("</td></tr></table>");
                    emailMessage.Append(
                        "<div style=\"text-align:right;font-size: 7pt; font-family: Calibri;width:850px;\"><a href='#top' style='color:black'><b><u>Back to top</u></b></a></div>\n");
                    emailMessage.Append("<br>");
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("CPU Error: {0}", ex);
                
            }
            #endregion

            #region Highest Disk Queues

            //Get Disk Queue.
            // var diskQueueGraph = diskBrowserService.GetPeakDiskQueues(systemSerial, starttime, stoptime);
            var diskTrendTable = new DiskTrendTableRepository(connectionStringSystem);
            var diskQueueGraph = diskTrendTable.GetDiskTrendPerInterval(starttime.ToString(dateFormat), stoptime.ToString(dateFormat));
            try {
                if (diskQueueGraph.Rows.Count > 0) {
                    var diskQueueLocation = chart.CreateDiskQueuePerInterval(diskQueueGraph, lastWeek, lastMonth,
                        _serverPath, Log, interval, lastWeekStartTime, lastMonthStartTime);
                    if (diskQueueLocation.Length > 0) {
                        emailMessage.Append("<a name=Top20Disks></a>");
                        emailMessage.Append(
                            @"<table style='background-color:#ffffff;border-radius: 3px;border: #cccccc 1px solid; width:100%;' cellpadding = 0 cellspacing = 0>
	                <tr ><td style='padding:5px 15px;color:#212121;background-color:#B7B7B7;border-color:#dddddd;'><h3 style='margin-top: 0;margin-bottom: 0;font-size: 15px;font-family: Calibri;color: inherit;'>Highest Disk Queue, per interval</h3>
	                </td></tr><tr><td>");
                        emailContent.HighestDiskQueue = diskQueueLocation;
                        emailMessage.Append("<IMG src='cid:HighestDiskQueue'>");
                        emailMessage.Append("</td></tr></table>");
                        emailMessage.Append(
                            "<div style=\"text-align:right;font-size: 7pt; font-family: Calibri;width:850px;\"><a href='#top' style='color:black'><b><u>Back to top</u></b></a></div><br>\n");
                    }
                }

                if (top20Disk.Length > 0) {
                    emailMessage.Append("<a name=Top20Disks></a>");
                    emailMessage.Append(
                        @"<table style='margin-bottom: 23px;background-color:#ffffff;border-radius: 3px;border: #cccccc 1px solid; width:100%;' cellpadding = 0 cellspacing = 0>
	            <tr ><td style='padding:5px 15px;color:#212121;background-color:#B7B7B7;border-color:#dddddd;'><h3 style='margin-top: 0;margin-bottom: 0;font-size: 15px;font-family: Calibri;color: inherit;'>Highest Disk Queues</h3>
	            </td></tr><tr><td style='padding: 15px; font-size: 12px;padding-left:50px;'>");
                    emailMessage.Append(top20Disk);
                    emailMessage.Append(@"</td><tr></table>");
                    emailMessage.Append(
                        "<div style=\"text-align:right;FONT-SIZE: 7pt; FONT-FAMILY: Calibri;width:850px;\"><a href='#top' style='color:black'><b><u>Back to top</u></b></a></div><br>\n");
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("Highest Disk Queues Error: {0}", ex);
            }

            #endregion

            #region Highest Process Busy

            //Get main process data.
            var processEntityData = processEntityTableService.GetAllProcessByBusy(systemSerial, starttime, stoptime, interval);


            //Get Process Busy.
            //var processBusyGraph = processEntityTableService.GetPeakProcessByBusy(systemSerial, starttime, stoptime, interval);
            var processBusyGraph = processEntityTableService.GetPeakProcessByBusy(processEntityData, starttime, stoptime, interval);
            try {
                if (processBusyGraph.Count > 0) {
                    var lastWeekDic = lastWeek.AsEnumerable()
                        .ToDictionary(row => Convert.ToDateTime(row["DateTime"]),
                            row => Convert.ToDouble(row["ProcessBusy"]));
                    var lastMonthDic = lastMonth.AsEnumerable()
                        .ToDictionary(row => Convert.ToDateTime(row["DateTime"]),
                            row => Convert.ToDouble(row["ProcessBusy"]));

                    var processBusyLocation = chart.CreatePeakProcessBusyPerInterval(processBusyGraph, lastWeekDic,
                        lastMonthDic, _serverPath, Log, interval, lastWeekStartTime, lastMonthStartTime);
                    if (processBusyLocation.Length > 0) {
                        emailMessage.Append("<a name=Top20BusiestProcesses></a>");
                        emailMessage.Append(
                            @"<table style='background-color:#ffffff;border-radius: 3px;border: #cccccc 1px solid; width:100%;' cellpadding = 0 cellspacing = 0>
	                <tr ><td style='padding:5px 15px;color:#212121;background-color:#B7B7B7;border-color:#dddddd;'><h3 style='margin-top: 0;margin-bottom: 0;font-size: 15px;font-family: Calibri;color: inherit;'>Highest Process Busy, per interval</h3>
	                </td></tr><tr><td>");
                        emailContent.HighestProcessBusy = processBusyLocation;
                        emailMessage.Append("<IMG src='cid:HighestProcessBusy'>");
                        emailMessage.Append("</td></tr></table>");
                        emailMessage.Append(
                            "<div style=\"text-align:right;font-size: 7pt; font-family: Calibri;width:850px;\"><a href='#top' style='color:black'><b><u>Back to top</u></b></a></div><br>\n");
                    }
                }
                if (processBusyStatic.Length > 0) {
                    emailMessage.Append("<a name=Top20BusiestProcesses></a>");
                    emailMessage.Append(
                        @"<table style='margin-bottom: 23px;background-color:#ffffff;border-radius: 3px;border: #cccccc 1px solid; width:100%;' cellpadding = 0 cellspacing = 0>
	            <tr ><td style='padding:5px 15px;color:#212121;background-color:#B7B7B7;border-color:#dddddd;'><h3 style='margin-top: 0;margin-bottom: 0;font-size: 15px;font-family: Calibri;color: inherit;'>Highest Process Busy - Static</h3>
	            </td></tr><tr><td style='padding: 15px; font-size: 12px;padding-left:50px;'>");
                    emailMessage.Append(processBusyStatic);
                    emailMessage.Append(@"</td><tr></table>");
                    emailMessage.Append(
                        "<div style=\"text-align:right;FONT-SIZE: 7pt; FONT-FAMILY: Calibri;width:850px;\"><a href='#top' style='color:black'><b><u>Back to top</u></b></a></div><br>\n");
                }
                if (processBusyDynamic.Length > 0) {
                    emailMessage.Append("<a name=Top20BusiestProcesses></a>");
                    emailMessage.Append(
                        @"<table style='margin-bottom: 23px;background-color:#ffffff;border-radius: 3px;border: #cccccc 1px solid; width:100%;' cellpadding = 0 cellspacing = 0>
	            <tr ><td style='padding:5px 15px;color:#212121;background-color:#B7B7B7;border-color:#dddddd;'><h3 style='margin-top: 0;margin-bottom: 0;font-size: 15px;font-family: Calibri;color: inherit;'>Highest Process Busy - Dynamic</h3>
	            </td></tr><tr><td style='padding: 15px; font-size: 12px;padding-left:50px;'>");
                    emailMessage.Append(processBusyDynamic);
                    emailMessage.Append(@"</td><tr></table>");
                    emailMessage.Append(
                        "<div style=\"text-align:right;FONT-SIZE: 7pt; FONT-FAMILY: Calibri;width:850px;\"><a href='#top' style='color:black'><b><u>Back to top</u></b></a></div><br>\n");
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("Highest Process Busy Error: {0}", ex);
            }

            #endregion

            #region Highest Process Receive Queue

            //Get Process Queue.
            //var processQueueGraph = processEntityTableService.GetPeakProcessQueueByBusy(systemSerial, starttime, stoptime, interval);
            var processQueueGraph = processEntityTableService.GetPeakProcessQueueByBusy(processEntityData, starttime, stoptime, interval);
            try {
                if (processQueueGraph.Count > 0) {
                    var lastWeekDic = lastWeek.AsEnumerable()
                        .ToDictionary(row => Convert.ToDateTime(row["DateTime"]),
                            row => Convert.ToDouble(row["ProcessQueue"]));
                    var lastMonthDic = lastMonth.AsEnumerable()
                        .ToDictionary(row => Convert.ToDateTime(row["DateTime"]),
                            row => Convert.ToDouble(row["ProcessQueue"]));
                    var processQueueLocation = chart.CreatePeakProcessQueuePerInterval(processQueueGraph, lastWeekDic,
                        lastMonthDic, _serverPath, Log, interval, lastWeekStartTime, lastMonthStartTime);
                    if (processQueueLocation.Length > 0) {
                        emailMessage.Append("<a name=Top20BusiestReceiveQueue></a>");
                        emailMessage.Append(
                            @"<table style='background-color:#ffffff;border-radius: 3px;border: #cccccc 1px solid; width:100%;' cellpadding = 0 cellspacing = 0>
	                <tr ><td style='padding:5px 15px;color:#212121;background-color:#B7B7B7;border-color:#dddddd;'><h3 style='margin-top: 0;margin-bottom: 0;font-size: 15px;font-family: Calibri;color: inherit;'>Highest Process Receive Queue, per interval</h3>
	                </td></tr><tr><td>");
                        emailContent.HighestProcessQueue = processQueueLocation;
                        emailMessage.Append("<IMG src='cid:HighestProcessQueue'>");
                        emailMessage.Append("</td></tr></table>");
                        emailMessage.Append(
                            "<div style=\"text-align:right;font-size: 7pt; font-family: Calibri;width:850px;\"><a href='#top' style='color:black'><b><u>Back to top</u></b></a></div><br>\n");
                    }
                }
                if (processQueueStatic.Length > 0) {
                    emailMessage.Append("<a name=Top20BusiestReceiveQueue></a>");
                    emailMessage.Append(
                        @"<table style='margin-bottom: 23px;background-color:#ffffff;border-radius: 3px;border: #cccccc 1px solid; width:100%;' cellpadding = 0 cellspacing = 0>
	            <tr ><td style='padding:5px 15px;color:#212121;background-color:#B7B7B7;border-color:#dddddd;'><h3 style='margin-top: 0;margin-bottom: 0;font-size: 15px;font-family: Calibri;color: inherit;'>Highest Process Receive Queue - Static</h3>
	            </td></tr><tr><td style='padding: 15px; font-size: 12px;padding-left:50px;'>");
                    emailMessage.Append(processQueueStatic);
                    emailMessage.Append(@"</td><tr></table>");
                    emailMessage.Append(
                        "<div style=\"text-align:right;FONT-SIZE: 7pt; FONT-FAMILY: Calibri;width:850px;\"><a href='#top' style='color:black'><b><u>Back to top</u></b></a></div><br>\n");
                }
                if (processQueueDynamic.Length > 0) {
                    emailMessage.Append("<a name=Top20BusiestReceiveQueue></a>");
                    emailMessage.Append(
                        @"<table style='margin-bottom: 23px;background-color:#ffffff;border-radius: 3px;border: #cccccc 1px solid; width:100%;' cellpadding = 0 cellspacing = 0>
	            <tr ><td style='padding:5px 15px;color:#212121;background-color:#B7B7B7;border-color:#dddddd;'><h3 style='margin-top: 0;margin-bottom: 0;font-size: 15px;font-family: Calibri;color: inherit;'>Highest Process Receive Queue - Dynamic</h3>
	            </td></tr><tr><td style='padding: 15px; font-size: 12px;padding-left:50px;'>");
                    emailMessage.Append(processQueueDynamic);
                    emailMessage.Append(@"</td><tr></table>");
                    emailMessage.Append(
                        "<div style=\"text-align:right;FONT-SIZE: 7pt; FONT-FAMILY: Calibri;width:850px;\"><a href='#top' style='color:black'><b><u>Back to top</u></b></a></div><br>\n");
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("Highest Process Receive Queue Error: {0}", ex);
            }

            #endregion

            #region TMF Abort Rates

            //Get Process Abort.
            //var processAbortGraph = processEntityTableService.GetProcessAbort(systemSerial, starttime, stoptime, interval);
            var processAbortGraph = processEntityTableService.GetProcessAbort(processEntityData, starttime, stoptime, interval);
            try {
                if (processAbortGraph.Count > 0) {
                    var lastWeekDic = lastWeek.AsEnumerable()
                        .ToDictionary(row => Convert.ToDateTime(row["DateTime"]),
                            row =>
                                (Convert.ToDouble(row["TransactionAbort"]) /
                                 (Convert.ToDouble(row["TransactionCompleted"]) +
                                  Convert.ToDouble(row["TransactionAbort"]))) * 100);
                    var lastMonthDic = lastMonth.AsEnumerable()
                        .ToDictionary(row => Convert.ToDateTime(row["DateTime"]),
                            row =>
                                (Convert.ToDouble(row["TransactionAbort"]) /
                                 (Convert.ToDouble(row["TransactionCompleted"]) +
                                  Convert.ToDouble(row["TransactionAbort"]))) * 100);

                    var processAbortLocation = chart.CreateProcessAbortPerInterval(processAbortGraph, lastWeekDic,
                        lastMonthDic, _serverPath, Log, interval, lastWeekStartTime, lastMonthStartTime);
                    if (processAbortLocation.Length > 0) {
                        emailMessage.Append("<a name=Top20TMFAbort></a>");
                        emailMessage.Append(
                            @"<table style='background-color:#ffffff;border-radius: 3px;border: #cccccc 1px solid; width:100%;' cellpadding = 0 cellspacing = 0>
	                <tr ><td style='padding:5px 15px;color:#212121;background-color:#B7B7B7;border-color:#dddddd;'><h3 style='margin-top: 0;margin-bottom: 0;font-size: 15px;font-family: Calibri;color: inherit;'>Completed & Aborted Transactions, per interval</h3>
	                </td></tr><tr><td>");
                        emailContent.Transaction = processAbortLocation;
                        emailMessage.Append("<IMG src='cid:Transaction'>");
                        emailMessage.Append("</td></tr></table>");
                        emailMessage.Append(
                            "<div style=\"text-align:right;font-size: 7pt; font-family: Calibri;width:850px;\"><a href='#top' style='color:black'><b><u>Back to top</u></b></a></div><br>\n");
                    }
                }
                if (processAbort.Length > 0) {
                    emailMessage.Append("<a name=Top20TMFAbort></a>");
                    emailMessage.Append(
                        @"<table style='margin-bottom: 23px;background-color:#ffffff;border-radius: 3px;border: #cccccc 1px solid; width:100%;' cellpadding = 0 cellspacing = 0>
	            <tr ><td style='padding:5px 15px;color:#212121;background-color:#B7B7B7;border-color:#dddddd;'><h3 style='margin-top: 0;margin-bottom: 0;font-size: 15px;font-family: Calibri;color: inherit;'>Highest TMF Abort Rates</h3>
	            </td></tr><tr><td style='padding: 15px; font-size: 12px;padding-left:50px;'>");
                    emailMessage.Append(processAbort);
                    emailMessage.Append(@"</td><tr></table>");
                    emailMessage.Append(
                        "<div style=\"text-align:right;FONT-SIZE: 7pt; FONT-FAMILY: Calibri;width:850px;\"><a href='#top' style='color:black'><b><u>Back to top</u></b></a></div><br>\n");
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("TMF Abort Rates Error: {0}", ex);
            }

            #endregion

            #region Storage

            if (storageGraphData.Count > 0) {
                try {
                    var storageLocation = chart.CreateStorageToday(storageGraphData, _serverPath, longDatePattern);
                    if (storageLocation.Length > 0) {
                        emailMessage.Append("<a name=Top20Storage></a>");
                        emailMessage.Append(
                            @"<table style='background-color:#ffffff;border-radius: 3px;border: #cccccc 1px solid; width:100%;' cellpadding = 0 cellspacing = 0>
	                                        <tr ><td style='padding:5px 15px;color:#212121;background-color:#B7B7B7;border-color:#dddddd;'><h3 style='margin-top: 0;margin-bottom: 0;font-size: 15px;font-family: Calibri;color: inherit;'>Storage</h3>
	                                        </td></tr><tr><td>");
                        emailContent.Storage = storageLocation;
                        emailMessage.Append("<IMG src='cid:Storage'>");

                        emailMessage.Append("</td></tr></table>");
                        emailMessage.Append(
                            "<div style=\"text-align:right;font-size: 7pt; font-family: Calibri;width:850px;\"><a href='#top' style='color:black'><b><u>Back to top</u></b></a></div><br>\n");
                    }
                    if (storage.Length > 0) {
                        emailMessage.Append("<a name=Top20Storage></a>");
                        emailMessage.Append(
                            @"<table style='margin-bottom: 23px;background-color:#ffffff;border-radius: 3px;border: #cccccc 1px solid; width:100%;' cellpadding = 0 cellspacing = 0>
	            <tr ><td style='padding:5px 15px;color:#212121;background-color:#B7B7B7;border-color:#dddddd;'><h3 style='margin-top: 0;margin-bottom: 0;font-size: 15px;font-family: Calibri;color: inherit;'>Highest Volumes, based on % used</h3>
	            </td></tr><tr><td style='padding: 15px; font-size: 12px;padding-left:50px;'>");
                        emailMessage.Append(storage);
                        emailMessage.Append(@"</td><tr></table>");
                        emailMessage.Append(
                            "<div style=\"text-align:right;FONT-SIZE: 7pt; FONT-FAMILY: Calibri;width:850px;\"><a href='#top' style='color:black'><b><u>Back to top</u></b></a></div><br>\n");
                    }
                }
                catch (Exception ex) {
                }
            }

			#endregion

			#region TOC

			//Build Table of Content.
			//should go back to email header
			//emailMessageToc.Append("<a name=top></a>");

            emailMessageToc.Append(
                @"<table style='border-radius: 3px; border: black 1px solid; padding:5px; width:100%; font-size:12px;font-family: Calibri;'>");
            emailMessageToc.Append(@"<tr >");
            if (!alertException) {
                if (cpuBusyColor.Equals(Color.Red))
                    emailMessageToc.Append(
                        @"<td style='background-color: Red;border-radius: 3px;height:25px;text-align:center'>");
                else if (cpuBusyColor.Equals(Color.Yellow))
                    emailMessageToc.Append(
                        @"<td style='background-color: #f3c200; border-radius: 3px;height:25px;text-align:center'>");
                else
                    emailMessageToc.Append(
                        @"<td style='background-color: #585858;border-radius: 3px;height:25px;text-align:center'>");
                emailMessageToc.Append(
                    @"<a style='color: #ffffff;position: relative;display: block;padding: 5px 10px;' href='#CpuBusyGraph'>CPU Busy</a>");
                emailMessageToc.Append(@"</td>");
                if (cpuQueueColor.Equals(Color.Red))
                    emailMessageToc.Append(
                        @"<td style='background-color: Red;border-radius: 3px;height:25px;text-align:center'>");
                else if (cpuQueueColor.Equals(Color.Yellow))
                    emailMessageToc.Append(
                        @"<td style='background-color: #f3c200;border-radius: 3px;height:25px;text-align:center'>");
                else
                    emailMessageToc.Append(
                        @"<td style='background-color: #585858;border-radius: 3px;height:25px;text-align:center'>");

                emailMessageToc.Append(
                    @"<a style='color: #ffffff;position: relative;display: block;padding: 5px 10px;' href='#CPUQueueLengthGraph'>CPU Queue Length</a>");
                emailMessageToc.Append(@"</td>");

                if (ipuBusyColor.Equals(Color.Red))
                    emailMessageToc.Append(
                        @"<td style='background-color: Red;border-radius: 3px;height:25px;text-align:center'>");
                else if (ipuBusyColor.Equals(Color.Yellow))
                    emailMessageToc.Append(
                        @"<td style='background-color: #f3c200; border-radius: 3px;height:25px;text-align:center'>");
                else
                    emailMessageToc.Append(
                        @"<td style='background-color: #585858;border-radius: 3px;height:25px;text-align:center'>");

                emailMessageToc.Append(
                    @"<a style='color: #ffffff;position: relative;display: block;padding: 5px 10px;' href='#IpuBusyGraph'>IPU Busy</a>");
                emailMessageToc.Append(@"</td>");
                if (ipuQueueColor.Equals(Color.Red))
                    emailMessageToc.Append(
                        @"<td style='background-color: Red;border-radius: 3px;height:25px;text-align:center'>");
                else if (ipuQueueColor.Equals(Color.Yellow))
                    emailMessageToc.Append(
                        @"<td style='background-color: #f3c200;border-radius: 3px;height:25px;text-align:center'>");
                else
                    emailMessageToc.Append(
                        @"<td style='background-color: #585858;border-radius: 3px;height:25px;text-align:center'>");

                emailMessageToc.Append(
                    @"<a style='color: #ffffff;position: relative;display: block;padding: 5px 10px;' href='#IPUQueueLengthGraph'>IPU Queue Length</a>");
                emailMessageToc.Append(@"</td>");
            }
            //Add Application tab.
            if (applicationBusyChart.Length > 0) {
                emailMessageToc.Append(
                    @"<td style='background-color: #585858;border-radius: 3px;height:25px;text-align:center'>");
                emailMessageToc.Append(
                    @"<a style='color: #ffffff;position: relative;display: block;padding: 5px 10px;' href='#ApplicationBusyGraph'>Application Busy</a>");
                emailMessageToc.Append(@"</td>");
            }

            if (top20Disk.Length > 0 && (!alertException || diskAlertError)) {
                emailMessageToc.Append(
                    @"<td style='background-color: #585858;border-radius: 3px;height:25px;text-align:center'>");
                emailMessageToc.Append(
                    @"<a style='color: #ffffff;position: relative;display: block;padding: 5px 10px;' href='#Top20Disks'>Highest Disk Queues</a>");
                emailMessageToc.Append(@"</td>");
            }
            if (processBusyStatic.Length > 0 || processBusyDynamic.Length > 0) {
                if (processBusyStatic.Contains("red") || processBusyDynamic.Contains("red")) {
                    emailMessageToc.Append(
                        @"<td style='background-color: Red;border-radius: 3px;height:25px;text-align:center'>");
                }
                else if (processBusyStatic.Contains("yellow") || processBusyDynamic.Contains("yellow")) {
                    emailMessageToc.Append(
                        @"<td style='background-color: #f3c200;border-radius: 3px;height:25px;text-align:center'>");
                }
                else {
                    emailMessageToc.Append(
                        @"<td style='background-color: #585858;border-radius: 3px;height:25px;text-align:center'>");
                }
                emailMessageToc.Append(
                    @"<a style='color: #ffffff;position: relative;display: block;padding: 5px 10px;' href='#Top20BusiestProcesses'>Highest Process Busy</a>");
                emailMessageToc.Append(@"</td>");
            }

            if (processQueueStatic.Length > 0 || processQueueDynamic.Length > 0) {
                if (processQueueStatic.Contains("yellow") || processQueueDynamic.Contains("yellow")) {
                    emailMessageToc.Append(
                        @"<td style='background-color: #f3c200;border-radius: 3px;height:25px;text-align:center'>");
                }
                else {
                    emailMessageToc.Append(
                        @"<td style='background-color: #585858;border-radius: 3px;height:25px;text-align:center'>");
                }
                emailMessageToc.Append(
                    @"<a style='color: #ffffff;position: relative;display: block;padding: 5px 10px;' href='#Top20BusiestReceiveQueue'>Highest Process Receive Queue</a>");

                emailMessageToc.Append(@"</td>");
            }
            if (processAbort.Length > 0) {
                emailMessageToc.Append(
                    @"<td style='background-color: #585858;border-radius: 3px;height:25px;text-align:center'>");
                emailMessageToc.Append(
                    @"<a style='color: #ffffff;position: relative;display: block;padding: 5px 10px;' href='#Top20TMFAbort'>TMF Abort Rates</a>");
                emailMessageToc.Append(@"</td>");
            }
            if ((storage.Length > 0 || storageGraphData.Count > 0) && (!alertException || storageAlertError)) {
                if (storage.Contains("red") || storage.Contains("red")) {
                    emailMessageToc.Append(
                        @"<td style='background-color: red;border-radius: 3px;height:25px;text-align:center'>");
                }
                else if (storage.Contains("yellow") || storage.Contains("yellow")) {
                    emailMessageToc.Append(
                        @"<td style='background-color: #f3c200;border-radius: 3px;height:25px;text-align:center'>");
                }
                else {
                    emailMessageToc.Append(
                        @"<td style='background-color: #585858;border-radius: 3px;height:25px;text-align:center'>");
                }
                emailMessageToc.Append(
                    @"<a style='color: #ffffff;position: relative;display: block;padding: 5px 10px;' href='#Top20Storage'>Storage</a>");
                emailMessageToc.Append(@"</td>");
            }
            emailMessageToc.Append(@"</tr> </table> <br>");

            #endregion

            if (alertException)
                emailContent.Content = emailMessageGrid.ToString() + emailMessageToc + emailMessage;
            else
                emailContent.Content = emailMessageToc.ToString() + emailMessage.ToString();

            emailContent.HourDrop = hourDrop;
            emailContent.HourDropPeriods = hourDropPeriods;

            #region Store the data

            try {
                var processData = new DataTable();
                DataColumn myDataColumn;

                // Create DateTime column.
                myDataColumn = new DataColumn { DataType = Type.GetType("System.DateTime"), ColumnName = "DateTime" };
                processData.Columns.Add(myDataColumn);
                myDataColumn = new DataColumn { DataType = Type.GetType("System.Double"), ColumnName = "ProcessBusy" };
                processData.Columns.Add(myDataColumn);
                myDataColumn = new DataColumn { DataType = Type.GetType("System.Double"), ColumnName = "ProcessQueue" };
                processData.Columns.Add(myDataColumn);
                myDataColumn = new DataColumn {
                    DataType = Type.GetType("System.Double"),
                    ColumnName = "TransactionCompleted"
                };
                processData.Columns.Add(myDataColumn);
                myDataColumn = new DataColumn {
                    DataType = Type.GetType("System.Double"),
                    ColumnName = "TransactionAbort"
                };
                processData.Columns.Add(myDataColumn);
                myDataColumn = new DataColumn { DataType = Type.GetType("System.Double"), ColumnName = "DiskQueueLength" };
                processData.Columns.Add(myDataColumn);

                for (var start = starttime; start < stoptime; start = start.AddSeconds(interval)) {
                    var newRow = processData.NewRow();
                    newRow["DateTime"] = start;
                    if (processBusyGraph.ContainsKey(start)) {
                        newRow["ProcessBusy"] = processBusyGraph[start];
                    }
                    else {
                        newRow["ProcessBusy"] = 0;
                    }

                    if (processQueueGraph.ContainsKey(start))
                        newRow["ProcessQueue"] = processQueueGraph[start];
                    else
                        newRow["ProcessQueue"] = 0;

                    if (processAbortGraph.ContainsKey(start)) {
                        newRow["TransactionCompleted"] = processAbortGraph[start].BeginTrans -
                                                         processAbortGraph[start].AbortTrans;
                        newRow["TransactionAbort"] = processAbortGraph[start].AbortTrans;
                    }
                    else {
                        newRow["TransactionCompleted"] = 0;
                        newRow["TransactionAbort"] = 0;
                    }

                    if (diskQueueGraph.AsEnumerable().Any(x => x.Field<DateTime>("FromTimestamp").Equals(start))) {
                        var diskQueueLength =
                            diskQueueGraph.AsEnumerable()
                                .Where(x => x.Field<DateTime>("FromTimestamp").Equals(start))
                                .Select(x => x.Field<double>("QueueLength"))
                                .FirstOrDefault();
                        newRow["DiskQueueLength"] = diskQueueLength;
                    }
                    else
                        newRow["DiskQueueLength"] = 0;
                    processData.Rows.Add(newRow);
                }
                Log.Info("Calling StoreDailyEmailData");
                
                StoreDailyEmailData(connectionStringSystem, systemSerial,
                    starttime, stoptime, interval, processData, tempSaveLocation);
                Log.Info("Finished StoreDailyEmailData");
            }
            catch (Exception ex) {
                Log.ErrorFormat("systemSerial: {0}, starttime: {1}, starttime: {2}",
                    systemSerial, starttime, stoptime);
                Log.ErrorFormat("ConsolidatedEmail {0}", ex);
            }

            #endregion

            return emailContent;
        }

        private void StoreDailyEmailData(string mySqlConnectionString, string systemSerial, DateTime startTime,
            DateTime endTime, long interval, DataTable processDataTable, string tempSaveLocation) {
            var dataDate = new List<DateTime>();
            var sysUnrated = new DailySysUnratedService(mySqlConnectionString);
            if ((Convert.ToDateTime(endTime).Subtract(startTime)).Days >= 1) {
                var dset = sysUnrated.GetDataDateFor(1, startTime, endTime, systemSerial);
                for (var i = 0; i < dset.Tables["Interval"].Rows.Count; i++) {
                    dataDate.Add(Convert.ToDateTime(dset.Tables["Interval"].Rows[i]["DataDate"].ToString()));
                }
            }
            else {
                dataDate.Add(Convert.ToDateTime(startTime));
            }

            //Check if Table exists.
            var databaseName = Helper.FindKeyName(mySqlConnectionString, "DATABASE");
            var dailyCpuDatas = new DailyCPUDataRepository(mySqlConnectionString);
            var cpuTableExists = dailyCpuDatas.CheckTableName(databaseName);
            if (!cpuTableExists)
                dailyCpuDatas.CreateDailyCPUDatas();

            var dailyPorcessDatas = new DailyProcessDatas(mySqlConnectionString);
            var processTableExists = dailyPorcessDatas.CheckTableName(databaseName);
            if (!processTableExists)
                dailyPorcessDatas.CreateDailyCPUDatas();

            //Build CPU Data and insert.
            var databaseCheck = new Database(mySqlConnectionString);
            var cpuTableNames = new List<string>();

            foreach (var dateTime in dataDate) {
                var cpuTableName = systemSerial + "_CPU_" + dateTime.Year + "_" + dateTime.Month + "_" + dateTime.Day;
                var exists = databaseCheck.CheckTableExists(cpuTableName, databaseName);

                if (exists)
                    cpuTableNames.Add(cpuTableName);
            }

            var cpuEntityTable = new CPUEntityTable(mySqlConnectionString);
            var ipus = cpuEntityTable.CheckIPU(cpuTableNames.First());
            var cpuData = cpuEntityTable.GetAllCPUBusyAndQueue(cpuTableNames, ipus, interval, startTime, endTime);

            var dataTables = new DataTables(mySqlConnectionString);
            dataTables.InsertEntityData("DailyCPUDataRepository", cpuData, tempSaveLocation);
            //Build Process Data and Insert.
            dataTables.InsertEntityData("DailyProcessDatas", processDataTable, tempSaveLocation);
        }

    }
}