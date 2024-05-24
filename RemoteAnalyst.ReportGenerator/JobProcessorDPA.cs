using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NonStopSPAM.BLL;
using NonStopSPAM.DLL;
using RemoteAnalyst.AWS.EC2;
using RemoteAnalyst.AWS.Infrastructure;
using RemoteAnalyst.AWS.SNS;
using RemoteAnalyst.BusinessLogic.BLL;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.Enums;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.ReportGenerator.BLL;
using RemoteAnalyst.BusinessLogic.Util;
using log4net;

namespace RemoteAnalyst.ReportGenerator {
    /// <summary>
    /// JobProcessorDPA collects information to generate DPA report.
    /// </summary>
    internal class JobProcessorDPA {
        private static readonly ILog Log = LogManager.GetLogger("DPAReportLog");
        private const string errorMessage = "We are currently experiencing a problem generating DPA report. Please be advised that our support team is actively working on resolving this issue and will provide you an update shortly. We apologize for any inconvenience caused.";
        private const string localAnalystErrorMessage = "We are currently experiencing a problem generating DPA report. Please contact support for assistance.";

        private readonly string _charts = "";
        private readonly string _emails = "";
        private readonly string _excelMaxRow = "";
        private readonly string _excelVersion = "";
        private readonly string _iReports = "";
        private readonly bool _macroReport;
        private readonly bool _isSchedule;
        private readonly int _ntsOrderID;
        private readonly string _parameters = "";
        private readonly int _queueID;
        private readonly int _reportDownloadId;
        private readonly string _reports = "";
        private readonly string _systemSerial = "";
        private DateTime _startTime;
        private DateTime _stopTime;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="reportDownloadId">Report Download Id</param>
        /// <param name="startTime">Report Start Time</param>
        /// <param name="stopTime">Report Stop Time</param>
        /// <param name="parameters">DPA Parameters</param>
        /// <param name="reports">Selected Reports</param>
        /// <param name="charts">Selected Charts</param>
        /// <param name="iIReports">Selected iReport</param>
        /// <param name="systemSerial">System Serial Number</param>
        /// <param name="emails">Selected Emails</param>
        /// <param name="excelMaxRow">Number of max row on each excel worksheet</param>
        /// <param name="excelVersion">Excel save format</param>
        /// <param name="ntsOrderID">ntsOrderID</param>
        /// <param name="queueID">Optional parameter for queueID, default is 0</param>
        public JobProcessorDPA(int reportDownloadId, DateTime startTime, DateTime stopTime, string parameters, string reports, string charts,
            string iIReports, string systemSerial, string emails, string excelMaxRow, string excelVersion, int ntsOrderID, int queueID, bool macroReport, bool isSchedule) {
            _reportDownloadId = reportDownloadId;
            _startTime = startTime;
            _stopTime = stopTime;
            _parameters = parameters;
            _reports = reports;
            _charts = charts;
            _iReports = iIReports;
            _systemSerial = systemSerial;
            _emails = emails;
            _excelMaxRow = excelMaxRow;
            _excelVersion = excelVersion;
            _ntsOrderID = ntsOrderID;
            _queueID = queueID;
            _macroReport = macroReport;
            _isSchedule = isSchedule;
        }

        public static string RemovePassword(string connectionString)
        {
            try
            {
                if (String.IsNullOrEmpty(connectionString))
                {
                    return connectionString;
                }
                if ((connectionString.Contains("PASSWORD") && connectionString.Contains(";")) || (connectionString.Contains("password") && connectionString.Contains(";")))
                {
                    List<string> strlist = connectionString.Split(';').ToList();
                    for (int i = 0; i < strlist.Count; i++)
                    {
                        if (strlist[i].Contains("PASSWORD") || connectionString.Contains("password"))
                        {
                            strlist.Remove(strlist[i]);
                            break;
                        }
                    }
                    string concat = String.Join(";", strlist.ToArray());
                    return concat;
                }
                else
                {
                    return connectionString;
                }
            }
            catch (Exception e)
            {
                return connectionString;
            }
        }

        /// <summary>
        /// ProcessReport collect information to generate DPA report and calls GenerateDPA class.
        /// </summary>
        internal void ProcessReport() {
            //RA 2051.17.0
            var reportDownloadLogService = new ReportDownloadLogService(ConnectionString.ConnectionStringDB);
            var reportDownload = new ReportDownloadService(ConnectionString.ConnectionStringDB);
            //Get save location.
            string systemLocation = ConnectionString.SystemLocation;
            string saveLocation = systemLocation + _systemSerial + "\\DPA_" + DateTime.Now.Ticks;
            //Create directory.
            var dirInfo = new DirectoryInfo(saveLocation);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
            Log.Info("Sending email to Customer letting them know we start the Report.");
            

            #region Send email to Customer letting them know we start the Report.

            var sysInfo = new System_tblService(ConnectionString.ConnectionStringDB);
            var emailList = new List<string>();
            //Get Emails
            string[] email = _emails.Split(',');
            foreach (string s in email.Where(s => !emailList.Contains(s))) {
                emailList.Add(s);
            }

            var reportParam = new ReportParameters();
            reportParam.SystemName = sysInfo.GetSystemNameFor(_systemSerial);
            reportParam.IsLocalAnalyst = ConnectionString.IsLocalAnalyst;

            if (!_isSchedule) {
                var emailReportConfirmation = new ReportEmail(ConnectionString.AdvisorEmail,
                    ConnectionString.SupportEmail,
                    ConnectionString.WebSite,
                    ConnectionString.EmailServer,
                    ConnectionString.EmailPort,
                    ConnectionString.EmailUser,
                    ConnectionString.EmailPassword,
                    ConnectionString.EmailAuthentication,
                    ConnectionString.SystemLocation,
                    ConnectionString.ServerPath,
                    ConnectionString.EmailIsSSL,
                    ConnectionString.IsLocalAnalyst, 
                    ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                var custInfo = new CusAnalystService(ConnectionString.ConnectionStringDB);
                try {
                    foreach (string custEmail in emailList) {
                        string custName = custInfo.GetUserNameFor(custEmail);
                        if (custName.Length.Equals(0)) {
                            custName = "Customer";
                        }
                        emailReportConfirmation.SendReportConfirmation(custEmail, custName, _systemSerial, reportParam.SystemName, _startTime, _stopTime, false);
                    }
                }
                catch (Exception) {
                }
            }

            #endregion

            Log.Info("Sent email to Customer letting them know we start the Report.");
            

            reportDownloadLogService.InsertNewLogFor(_reportDownloadId, DateTime.Now, "Start Loading required data.");
            var databasePostfix = "_RG_" + DateTime.Now.Ticks;
            Log.InfoFormat("NTS Order ID: {0}", _ntsOrderID);
            

#if (!DEBUG)
            if (_ntsOrderID == 0) {
                Log.Info(" Check archived data before processing reports");
                
                #region - Check archived data before processing reports

                //1. First check if the file is being downloaded or not, if downloading return
                //2. Then check if the file is archived or not, if archived, send message to loader and return, otherwise perform regular load
                var jobProcessorGlacier = new JobProcessorGlacier(_systemSerial, _startTime, _stopTime, false, 0, 0);  //pass 0 as customer id and 0 for sampletypeid. customerId if for Glacier Data Load.
                var glacierLoadCount = jobProcessorGlacier.CheckFile("DPA", _queueID);
                
                //Load data.
                var loadData = new LoadDataS3();
                bool success = loadData.LoadUWSFiles(_systemSerial, _startTime, _stopTime, false, databasePostfix, _reportDownloadId, reportParam.SystemName, emailList);
                Log.InfoFormat("glacierLoadCount {0} success: {1}", glacierLoadCount, success);
                
                if (!success && glacierLoadCount == 0) {
                    reportDownload.UpdateStatusFor(_reportDownloadId, 1);
					var _systemName = sysInfo.GetSystemNameFor(_systemSerial);
					var emailReport = new EmailReport();
					emailReport.SendDPAReportError(emailList, _systemSerial, _systemName, _startTime, _stopTime);
					if (ConnectionString.IsLocalAnalyst)
                        reportDownloadLogService.InsertNewLogFor(_reportDownloadId, DateTime.Now, localAnalystErrorMessage);
                    else
                        reportDownloadLogService.InsertNewLogFor(_reportDownloadId, DateTime.Now, @"No data available for the requested time period. Report will not be generated. You can manually order this report when the data is reloaded successfully. Contact support to reload the missing data.");
                    Log.Info("No data available for the requested time period from either Glacier or S3. Report will not be generated. You can manually order this report when the data is reloaded successfully. Contact support to reload the missing data.");
                    
					ClearQueue.Clear(Report.Types.DPA, _queueID);
					TerminateEC2(ReportGeneratorStatus.GetEnumDescription(ReportGeneratorStatus.StatusMessage.DPANoDataAvailable));
                    return;
                }
                
                #endregion
            }
            else {
				var databaseMapService = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
				string newConnectionString = databaseMapService.GetConnectionStringFor(_systemSerial);
                Log.InfoFormat("Checking databaseMapService for {0} {1}",
                    _systemSerial, JobProcessorDPA.RemovePassword(newConnectionString));
                
                if (newConnectionString.Length == 0) {
                    Log.Info("setting to default");
                    
                    newConnectionString = ConnectionString.ConnectionStringSPAM;
				}
                Log.InfoFormat("Connecting directly for data: {0}", JobProcessorDPA.RemovePassword(newConnectionString));
                
                var nullChecker = new NullCheckService();
                var isDataAvailable = nullChecker.NullCheckForDPAandQT(_startTime, _stopTime, newConnectionString);
                Log.InfoFormat("DataAvailable check: startTime: {0} stopTime: {1}",
                    _startTime, _stopTime);
                
                if (!isDataAvailable) {
					var emailReport = new EmailReport();
					var _systemName = sysInfo.GetSystemNameFor(_systemSerial);
					emailReport.SendDPAReportError(emailList, _systemSerial, _systemName, _startTime, _stopTime);
					string errorMessage = @"No data available for the requested time period. Report will not be generated. You can manually order this report when the data is reloaded successfully. Contact support to reload the missing data.";
					ClearQueue.Clear(Report.Types.DPA, _queueID);
					reportDownloadLogService.InsertNewLogFor(_reportDownloadId, DateTime.Now, errorMessage);
					reportDownload.UpdateStatusFor(_reportDownloadId, 1);
                    Log.Info("No data available for the requested time period in online database. Report will not be generated. You can manually order this report when the data is reloaded successfully. Contact support to reload the missing data.");
                    
                    TerminateEC2(ReportGeneratorStatus.GetEnumDescription(ReportGeneratorStatus.StatusMessage.DPANoDataAvailable));
                    return;
                }
			}

            Log.Info("Data check complete ");
            
            if (_ntsOrderID == 0) {
                #region Load OSS Names

                try {
                    //Get ConnectionString
                    var databaseMapService = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
                    string newConnectionString = databaseMapService.GetConnectionStringFor(_systemSerial);
                    if (newConnectionString.Length == 0) {
                        newConnectionString = ConnectionString.ConnectionStringSPAM;
                    }

                    var ossJrnl = new OSSJRNLService(newConnectionString);
                    string localConnectionString = databaseMapService.GetConnectionStringDynamicReportGenerator(_systemSerial, ConnectionString.TempDatabaseConnectionString, ConnectionString.IsLocalAnalyst, databasePostfix);
                    ossJrnl.CopyOSSNames(_systemSerial, localConnectionString, Log, saveLocation);
                }
                catch {
                }

                #endregion
            }
            Log.Info("Data loaded Starting analyses ");
            
            reportDownloadLogService.InsertNewLogFor(_reportDownloadId, DateTime.Now, "Data is loaded. Starting analyses.");
#endif // Skip data validation check for DEBUG
            #region Report Generation

            Log.Info("Report generation started");
            Exception DPAException = null;
            reportParam.SystemName = sysInfo.GetSystemNameFor(_systemSerial);

            try {
                reportParam.ReportDownloadId = _reportDownloadId;
                reportParam.RemoteAnalystConnectionString = ConnectionString.ConnectionStringDB;
                reportParam.MacroReport = _macroReport;

                Log.InfoFormat("MacroReport: {0}", _macroReport);
                

                Log.Info("Getting Selected Reports");
                

                #region Get selected reports

                var reports = new List<int>();
                var charts = new List<int>();
                var iReports = new List<int>();

                if (_reports.Length > 0) {
                    string[] report = _reports.Split(',');
                    foreach (string s in report) {
                        if (s.Length > 0) {
                            if (!reports.Contains(Convert.ToInt32(s))) {
                                reports.Add(Convert.ToInt32(s));
                            }
                        }
                    }
                }
                if (_charts.Length > 0) {
                    string[] chart = _charts.Split(',');
                    foreach (string s in chart) {
                        if (s.Length > 0) {
                            if (!charts.Contains(Convert.ToInt32(s))) {
                                charts.Add(Convert.ToInt32(s));
                            }
                        }
                    }
                }
                if (_iReports.Length > 0) {
                    string[] iReport = _iReports.Split(',');
                    foreach (string s in iReport) {
                        if (s.Length > 0) {
                            if (!iReports.Contains(Convert.ToInt32(s))) {
                                iReports.Add(Convert.ToInt32(s));
                            }
                        }
                    }
                }

                #endregion

                Log.Info("Getting Parameters");
                

                #region Get Parameter information.

                //Get number of days.
                int dayCount = 0;
                for (DateTime start = _startTime; start.Date <= _stopTime.Date; start = start.AddDays(1)) {
                    dayCount++;
                }
                reportParam.DayCount = dayCount;

                //Get new database Name.
                var databaseMapService = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
                string newConnectionString = databaseMapService.GetConnectionStringDynamicReportGenerator(_systemSerial, ConnectionString.TempDatabaseConnectionString, ConnectionString.IsLocalAnalyst, databasePostfix);
                if (_ntsOrderID != 0) {
                    newConnectionString = databaseMapService.GetConnectionStringFor(_systemSerial);
                }
#if (DEBUG)
                newConnectionString = "Server=10.200.1.81;Port=3306;Database=RemoteAnalystdb078578;Uid=raadmin;Pwd=prod@RA32;";
#endif
                #region Get default parameters.

                reportParam.ConnectionString = newConnectionString;

                string[] parameter = _parameters.Split('^');

                if (parameter.Length == 12) {
                    reportParam.Allow = Convert.ToDouble(parameter[0]);
                    reportParam.Audit = parameter[1].ToUpper();
                    reportParam.Begin = parameter[2];
                    reportParam.Exclude = Convert.ToDouble(parameter[3]);
                    reportParam.File = parameter[4];
                    reportParam.Flag = Convert.ToInt32(parameter[5]);
                    reportParam.ParameterInterval = Convert.ToInt32(parameter[6]);
                    reportParam.IOFactor = Convert.ToDouble(parameter[7]);
                    reportParam.MinMsgs = Convert.ToInt32(parameter[8]);
                    reportParam.Needle = Convert.ToDouble(parameter[9]);
                    reportParam.Program = parameter[10];
                    reportParam.ShowRCV = Convert.ToInt32(parameter[11]) == 1;
                }
                else {
                    //just default.
                    Config.ConnectionString = newConnectionString;
                    var param = new Params();
                    reportParam.Allow = Convert.ToDouble(param.getDefaultAllow());
                    reportParam.Audit = param.getDefaultAudit();
                    reportParam.Begin = param.getDefaultBegin();
                    reportParam.Exclude = Convert.ToDouble(param.getDefaultExclude());
                    reportParam.File = param.getDefaultFile();
                    reportParam.Flag = Convert.ToInt32(param.getDefaultFlag());
                    reportParam.ParameterInterval = Convert.ToInt32(param.getDefaultIntv());
                    reportParam.IOFactor = Convert.ToDouble(param.getDefaultIOFactor());
                    reportParam.MinMsgs = Convert.ToInt32(param.getDefaultMinmsgs());
                    reportParam.Needle = Convert.ToDouble(param.getDefaultNeedle());
                    reportParam.Program = param.getDefaultProg();
                    reportParam.ShowRCV = Convert.ToInt32(param.getDefaultShowrcv()) == 1;
                }

                #endregion

                reportParam.FolderName = saveLocation;
                reportParam.ViewRept = false;
                reportParam.OuputFormat = 2;

                //NonStopSPAM.DLL.TimeSlot time = new NonStopSPAM.DLL.TimeSlot(ConnectionString.connectionStringSPAM);
                var time = new TimeSlot(newConnectionString);
                reportParam.Intervals = time.GetInterval(_startTime.ToString("MM/dd/yy"), _systemSerial);
                reportParam.StartTime = _startTime;
                reportParam.EndTime = _stopTime;
                reportParam.SystemSerial = _systemSerial;

                reportParam.Continuous = true;

                //NonStopSPAM.DLL.GetMeasure meas = new NonStopSPAM.DLL.GetMeasure(ConnectionString.connectionStringSPAM);
                var meas = new GetMeasure(newConnectionString);
                reportParam.MeasureVersion = meas.GetMeasureVersion(_systemSerial, Convert.ToDateTime(_startTime));
                reportParam.MeasureFormat = meas.GetMeasureCode(reportParam.MeasureVersion);

                reportParam.ReportIDs = reports;
                reportParam.ChartIDs = charts;
                reportParam.CsvIDs = iReports;

                //Delta Time is Calculated in Seconds
                reportParam.DeltaTime = time.ObtainDeltaTime(reportParam.StartTime, reportParam.EndTime,
                    reportParam.DayCount, reportParam.SystemSerial, reportParam.Continuous);

                //Build report name.
                string fileName = reportParam.SystemName.Replace("\\", "") + "(" + reportParam.SystemSerial + ")" +
                                  " - * for " + reportParam.StartTime.ToString("yyyy-MM-dd HHmm") + " to " +
                                  reportParam.EndTime.ToString("yyyy-MM-dd HHmm") + ".xlsm";
                
                reportParam.ExcelName = fileName;

                reportParam.ExcelVersion = _excelVersion;
                reportParam.MaxRowCount = Convert.ToInt32(_excelMaxRow);

                #endregion

                Log.Info("Getting Emails");
                Log.InfoFormat("{0}", string.Join(",", emailList));
                Log.Info("Calling Report Call.");
                
                bool attachmentInEmail = sysInfo.GetAttachmentInEmailFor(_systemSerial);
                var dpa = new GenerateDPA(_systemSerial);
                dpa.ReportCall(reportParam, Log, emailList, _ntsOrderID, ConnectionString.IsLocalAnalyst, attachmentInEmail);
            }
            catch (Exception ex) {
                reportDownload.UpdateStatusFor(_reportDownloadId, 2);
                if (ConnectionString.IsLocalAnalyst)
                    reportDownloadLogService.InsertNewLogFor(_reportDownloadId, DateTime.Now, localAnalystErrorMessage);
                else
                    reportDownloadLogService.InsertNewLogFor(_reportDownloadId, DateTime.Now, errorMessage);

                DPAException = ex;
                Log.ErrorFormat("DPA Error: {0}", ex);
            }
            finally {
                //No matter exception occurred or not, send the SNS notification to terminate instance
                //If exception occurred, send the log file
                string subject = ReportGeneratorStatus.GetEnumDescription(ReportGeneratorStatus.StatusMessage.DPACompleted);
                if (DPAException != null) {
                    subject = ReportGeneratorStatus.GetEnumDescription(ReportGeneratorStatus.StatusMessage.DPAException);
                }
                ClearQueue.Clear(Report.Types.DPA, _queueID);
                TerminateEC2(subject);
            }
            #endregion
        }

        internal void TerminateEC2(string subject)
        {
            if (!ConnectionString.IsLocalAnalyst)
            {
                var ec2 = new AmazonEC2();
                string instanceID = ec2.GetEC2ID();

                //Check if there is any other report that running on this EC2.
                var reportQueues = new ReportQueueAWSService(ConnectionString.ConnectionStringDB);
                bool inQueue = reportQueues.CheckOtherQueuesFor(instanceID);

                if (!inQueue)
                {
                    //Smart EC2 Logic. Check how long EC2 has been running.

                    DateTime launchTime = ec2.GetLaunchTime(instanceID);
                    TimeSpan runningTime = DateTime.Now - launchTime;
                    if (_ntsOrderID != 0 || ConnectionString.EC2TerminateAllowTime - (runningTime.TotalMinutes % 60) <= 0)
                    {
                        //Once the report is done, send message trigger the Lambda to shut down the instance
                        Log.InfoFormat("Calling SNS to terminate EC2 {0}", instanceID);
                        IAmazonSNS amazonSns = new AmazonSNS();
                        string messasge = ReportGeneratorInfo.GetReportGeneratorInfo(instanceID, launchTime, runningTime);
                        amazonSns.SendToTopic(subject, messasge, ConnectionString.SNSProdTriggerReportARN);
                    }
                }
            }
        }
    }
}