using System;
using System.Collections.Generic;
using System.IO;
using GPAPrototype.DAO;
using GPAPrototype.Datasets.Common;
using GPAPrototype.ExcelGen;
using log4net;
using RemoteAnalyst.AWS.EC2;
using RemoteAnalyst.AWS.Infrastructure;
using RemoteAnalyst.AWS.S3;
using RemoteAnalyst.AWS.SNS;
using RemoteAnalyst.BusinessLogic.BLL;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.Enums;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.ReportGenerator.BLL;
using RemoteAnalyst.BusinessLogic.Util;
using NonStopSPAM.DLL;
using GPAPrototype.Database;
using Microsoft.VisualBasic.ApplicationServices;

namespace RemoteAnalyst.ReportGenerator
{
    /// <summary>
    /// JobProcessorQT collects information to generate QT report.
    /// </summary>
    internal class JobProcessorQT
    {
        private static readonly ILog Log = LogManager.GetLogger("QTReportLog");
        private const string errorMessage = "We are currently experiencing a problem generating QT report. Please be advised that our support team is actively working on resolving this issue and will provide you an update shortly. We apologize for any inconvenience caused.";
        private const string localAnalystErrorMessage = "We are currently experiencing a problem generating QT report. Please contact support for assistance.";

        private readonly string _alerts = "";
        private readonly string _dest = "";
        private readonly string _emails = "";
        private readonly bool _isSchedule;
        private readonly bool _isBeforeJ06;
        private readonly int _ntsOrderID;
        private readonly string _optional = "";
        private readonly string _program = "";
        private readonly int _queueID;
        private readonly int _reportDownloadId;
        private readonly string _source = "";
        private readonly DateTime _startTime;
        private readonly DateTime _stopTime;
        private readonly string _systemName = "";
        private readonly string _systemSerial = "";
        private int _autoID;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="reportDownloadId">Report Download Id</param>
        /// <param name="startTime">Report Start Time</param>
        /// <param name="stopTime">Report Stop Time</param>
        /// <param name="alerts">Disk QLen, DiskFile Transient Open, and DiskFile Requests Blocked value</param>
        /// <param name="optional">Optional Parameters</param>
        /// <param name="soruce">Exclude Source CPU</param>
        /// <param name="dest">Exclude Dest. CPU</param>
        /// <param name="program">Exclude Programs</param>
        /// <param name="systemSerial">System Serial Number</param>
        /// <param name="systemName">System Name</param>
        /// <param name="emails">Selected Email</param>
        /// <param name="autoID">Compare QT report</param>
        /// <param name="ntsOrderID">NTS Order ID, if requested from NTS</param>
        /// <param name="queueID">Optional parameter for queueID, default is 0</param>
        public JobProcessorQT(int reportDownloadId, DateTime startTime, DateTime stopTime, string alerts, string optional, string soruce,
            string dest, string program, string systemSerial, string systemName, string emails, int autoID, int ntsOrderID, bool isSchedule,
            bool isBeforeJ06, int queueID)
        {
            _reportDownloadId = reportDownloadId;
            _startTime = startTime;
            _stopTime = stopTime;
            _alerts = alerts;
            _optional = optional;
            _source = soruce;
            _dest = dest;
            _program = program;
            _systemSerial = systemSerial;
            _systemName = systemName;
            _emails = emails;
            _autoID = autoID;
            _ntsOrderID = ntsOrderID;
            _queueID = queueID;
            _isSchedule = isSchedule;
            _isBeforeJ06 = isBeforeJ06;

            if (_systemName.Contains("\\"))
            {
                _systemName = _systemName.Replace("\\", "");
            }
        }

        /// <summary>
        /// ProcessReport collect information to generate QT report and calls TunerLib.dll to generate QT report. Once report generation is complete, it zips the files and emails report.
        /// </summary>
        public void ProcessReport()
        {
            //RA 2051.17.0
            var reportDownloadLogService = new ReportDownloadLogService(ConnectionString.ConnectionStringDB);
            var reportDownload = new ReportDownloadService(ConnectionString.ConnectionStringDB);

            //Check Excel
            //Type officeType = Type.GetTypeFromProgID("Excel.Application");
            //if (officeType == null)
            //{
            //    reportDownloadLogService.InsertNewLogFor(_reportDownloadId, DateTime.Now, "Excel was not found on the server, please install excel 2013 or higher to generate this report.");
            //    reportDownload.UpdateStatusFor(_reportDownloadId, 2);
            //    ClearQueue.Clear(Report.Types.QT, _queueID);
            //    return;
            //}

            #region Send email to Customer letting them know we start the Report.

            var emailList = new List<string>();
            string[] email = _emails.Split(',');
            foreach (string s in email)
            {
                if (!emailList.Contains(s))
                {
                    emailList.Add(s);
                }
            }

            if (!_isSchedule)
            {
                var emailReportConfirmation = new ReportEmail(ConnectionString.AdvisorEmail,
                    ConnectionString.SupportEmail,
                    ConnectionString.WebSite,
                    ConnectionString.EmailServer,
                    ConnectionString.EmailPort,
                    ConnectionString.EmailUser,
                    ConnectionString.EmailPassword,
                    ConnectionString.EmailAuthentication, ConnectionString.SystemLocation,
                    ConnectionString.ServerPath,
                    ConnectionString.EmailIsSSL,
                    ConnectionString.IsLocalAnalyst, ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                var custInfo = new CusAnalystService(ConnectionString.ConnectionStringDB);

                try
                {
                    foreach (string custEmail in emailList)
                    {
                        string custName = custInfo.GetUserNameFor(custEmail);
                        if (custName.Length.Equals(0))
                        {
                            custName = "Customer";
                        }

                        emailReportConfirmation.SendReportConfirmation(custEmail, custName, _systemSerial, _systemName, _startTime, _stopTime, true);
                    }
                }
                catch (Exception)
                {
                }
            }

            #endregion

            reportDownloadLogService.InsertNewLogFor(_reportDownloadId, DateTime.Now, "Start Loading required data.");
            var databasePostfix = "_RG_" + DateTime.Now.Ticks;

            var databaseMapService = new DatabaseMappingService(ConnectionString.ConnectionStringDB);

#if (!DEBUG)
            if (_ntsOrderID == 0)
            {
            #region - Load data before processing reports

                var jobProcessorGlacier = new JobProcessorGlacier(_systemSerial, _startTime, _stopTime, false, 0, 0);  //pass 0 as customer id and sampleTypeId. customerId if for Glacier Data Load.
                var glacierLoadCount = jobProcessorGlacier.CheckFile("QT", _queueID);

                //Load UWS data.
                var loadData = new LoadDataS3();
                bool success = loadData.LoadUWSFiles(_systemSerial, _startTime, _stopTime, true, databasePostfix, _reportDownloadId, _systemName, emailList);
                //If any load failed, should not start report generation
                if (!success && glacierLoadCount == 0)
                {
                    reportDownload.UpdateStatusFor(_reportDownloadId, 1);
                    var emailReport = new EmailReport();
                    emailReport.SendQTReportError(emailList, _systemSerial, _systemName, _startTime, _stopTime);
                    if (ConnectionString.IsLocalAnalyst)
                        reportDownloadLogService.InsertNewLogFor(_reportDownloadId, DateTime.Now, localAnalystErrorMessage);
                    else
                        reportDownloadLogService.InsertNewLogFor(_reportDownloadId, DateTime.Now, @"No data available for the requested time period. Report will not be generated. You can manually order this report when the data is reloaded successfully. Contact support to reload the missing data.");
                    ClearQueue.Clear(Report.Types.QT, _queueID);
                    TerminateEC2(ReportGeneratorStatus.GetEnumDescription(ReportGeneratorStatus.StatusMessage.QTNoDataAvailable));
                    return;
                }

            #endregion
            }
            else
            {
                string newConnectionString = databaseMapService.GetConnectionStringFor(_systemSerial);
                if (newConnectionString.Length == 0)
                {
                    newConnectionString = ConnectionString.ConnectionStringSPAM;
                }
                var nullChecker = new NullCheckService();
                if (!nullChecker.NullCheckForDPAandQT(_startTime, _stopTime, newConnectionString))
                {
                    var emailReport = new EmailReport();
                    emailReport.SendQTReportError(emailList, _systemSerial, _systemName, _startTime, _stopTime);
                    string errorMessage = @"No data available for the requested time period. Report will not be generated. You can manually order this report when the data is reloaded successfully. Contact support to reload the missing data.";
                    reportDownloadLogService.InsertNewLogFor(_reportDownloadId, DateTime.Now, errorMessage);
                    reportDownload.UpdateStatusFor(_reportDownloadId, 1);
                    ClearQueue.Clear(Report.Types.QT, _queueID);
                    TerminateEC2(ReportGeneratorStatus.GetEnumDescription(ReportGeneratorStatus.StatusMessage.QTNoDataAvailable));
                    return;
                }
            }

            if (_ntsOrderID == 0)
            {
            #region Load OSS Names

                try
                {
                    //Get ConnectionString

                    string newConnectionString = databaseMapService.GetConnectionStringFor(_systemSerial);
                    if (newConnectionString.Length == 0)
                    {
                        newConnectionString = ConnectionString.ConnectionStringSPAM;
                    }

                    Log.InfoFormat("newConnectionString: {0}", JobProcessorDPA.RemovePassword(newConnectionString));
                    
                    var ossJrnl = new OSSJRNLService(newConnectionString);
                    string localConnectionString = databaseMapService.GetConnectionStringDynamicReportGenerator(_systemSerial, ConnectionString.TempDatabaseConnectionString, ConnectionString.IsLocalAnalyst, databasePostfix);
                    ossJrnl.CopyOSSNames(_systemSerial, localConnectionString, Log, ConnectionString.ServerPath);
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Error: {0}", ex);
                }

            #endregion
            }

            reportDownloadLogService.InsertNewLogFor(_reportDownloadId, DateTime.Now, "Data is loaded. Starting analyses.");
#endif // Skip data validation check for DEBUG

            #region Report Generation

            string logSaveLocation = "";
            string logFileToSend = "";
            Exception QTException = null;

            try
            {
                //Get save location.
                string path = "";
                string saveLocation = "";
                path = ConnectionString.ServerPath;
                string systemLocation = ConnectionString.SystemLocation;
                saveLocation = systemLocation + _systemSerial + "\\QT_" + DateTime.Now.Ticks;
                Log.InfoFormat("Save location: {0}", saveLocation);
                var dirInfo = new DirectoryInfo(saveLocation);
                if (!dirInfo.Exists)
                {
                    dirInfo.Create();
                }
                Log.Info("Create directory");
                Log.Info("Report generation started");
                Log.InfoFormat("ConnectionString.ConnectionStringDB: {0}", JobProcessorDPA.RemovePassword(ConnectionString.ConnectionStringDB));
                
                Log.InfoFormat("ConnectionString.ConnectionStringComparative: {0}", JobProcessorDPA.RemovePassword(ConnectionString.ConnectionStringComparative));
                
                logSaveLocation = saveLocation;
                try
                {
                    try
                    {
                        //if autoID = -1, Check to see there is comparative Analysis.
                        if (_autoID == -1)
                        {
                            if (_ntsOrderID == 0)
                            {

                                int companyID = ComparativeAnalysisDao.GetCompanyId(new GetDataConnection(), _systemSerial, "System_Tbl");
                                ComparativeAnalysisDao.SetLogFileLocationForRa(saveLocation);
                                _autoID =
                                    ComparativeAnalysisDao.GetLastComparisonRecord(new RemoteAnalystConnection(),
                                        "Comparative_Analysis_Summary", _systemSerial, companyID.ToString());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.ErrorFormat("AutoID Error: {0}", ex);
                    }

                    Log.InfoFormat("AutoID: {0}", _autoID);

                    string[] alert = _alerts.Split(',');
                    string discQLen = "";
                    string diskFileTransientOpen = "";
                    string diskFileRequestsBlocked = "";

                    if (alert.Length == 1)
                    {
                        discQLen = alert[0];
                    }
                    else if (alert.Length == 2)
                    {
                        discQLen = alert[0];
                        diskFileTransientOpen = alert[1];
                    }
                    else if (alert.Length == 3)
                    {
                        discQLen = alert[0];
                        diskFileTransientOpen = alert[1];
                        diskFileRequestsBlocked = alert[2];
                    }

                    try
                    {
                        string qtReport;
                        Log.InfoFormat("SystemSerial: {0}", _systemSerial);
                        //Get new database Name.
                        string newConnectionString = databaseMapService.GetConnectionStringDynamicReportGenerator(_systemSerial, ConnectionString.TempDatabaseConnectionString, ConnectionString.IsLocalAnalyst, databasePostfix);

                        if (_ntsOrderID != 0)
                        {
                            newConnectionString = databaseMapService.GetConnectionStringFor(_systemSerial);
                        }
                        Log.InfoFormat("newConnectionString: {0}", JobProcessorDPA.RemovePassword(newConnectionString));

                        if (newConnectionString.Length == 0)
                        {
                            newConnectionString = ConnectionString.ConnectionStringSPAM;
                        }

                        //System Database connectionString.
                        var systemDatabaseConnectionString = databaseMapService.GetConnectionStringFor(_systemSerial);
#if (DEBUG)
                        newConnectionString = "Server=10.200.1.81;Port=3306;Database=RemoteAnalystdb078578;Uid=raadmin;Pwd=prod@RA32;";
                        systemDatabaseConnectionString = "Server=10.200.1.81;Port=3306;Database=RemoteAnalystdb078578;Uid=raadmin;Pwd=prod@RA32;";
#endif
                        Log.Info("Setting required parameters");
                        var param = new Param(new DataContext());
                        param.SetSystemSerial(_systemSerial);
                        param.SetSystemName(_systemName);
                        param.SetOrderId(-50);
                        param.SetConnectionString(new RemoteAnalystConnection());
                        param.SetDataConnectionString(new GetDataConnection());
                        param.RemoteAnalystConnectionString = new RemoteAnalystConnection();
                        param.RemoteAnalystConnectionStringTrend = ConnectionString.ConnectionStringTrend;
                        param.RemoteAnalystConnectionStringComparative = null; //ToDO
                        param.WebDataConnectionString = new WebDataConnection();
                        param.SetLocation(saveLocation);
                        param.SetStartTime(_startTime);
                        param.SetEndTime(_stopTime);
                        param.SetDbName("");
                        param.Analysistype = 1;
                        param.SetCpuFrom(_source);
                        param.SetCpuTo(_dest);
                        param.SetProgram(_program);
                        param.SetQLenAlert(discQLen);
                        param.SetFOpenAlert(diskFileTransientOpen);
                        param.SetFLockWaitAlert(diskFileRequestsBlocked);
                        param.SetParamFrom(_startTime);
                        param.SetParamTo(_stopTime);
                        param.FromRa = true;
                        param.IsBeforeJ0619 = _isBeforeJ06;
                        Log.Info("Setting optional parameters");
                        if (_optional.Length > 0)
                        {
                            string[] options = _optional.Split(',');
                            if (options.Length == 1)
                            {
                                param.SetOverBusyBy(options[0]);
                            }
                            else if (options.Length == 2)
                            {
                                param.SetOverBusyBy(options[0]);
                                param.MinimumTotalTransientTransactions = Convert.ToInt32(options[1]);
                            }
                            else if (options.Length == 3)
                            {
                                param.SetOverBusyBy(options[0]);
                                param.MinimumTotalTransientTransactions = Convert.ToInt32(options[1]);
                                param.LowPinSubVols = options[2].Replace(' ', ',');
                            }
                            else if (options.Length > 3)
                            {
                                param.SetOverBusyBy(options[0]);
                                param.MinimumTotalTransientTransactions = Convert.ToInt32(options[1]);

                                for (var x = 2; x < options.Length; x++)
                                    param.LowPinSubVols += options[x] + ",";

                                param.LowPinSubVols = param.LowPinSubVols.Remove(param.LowPinSubVols.Length - 1, 1);
                            }
                        }

                        param.ReportDownloadId = _reportDownloadId;
                        param.IsLocalAnalyst = ConnectionString.IsLocalAnalyst;

                        //param.SetOverBusyBy(_optional);
                        param.ImageLocation = path;
                        param.SetOrderAutoId(_autoID);

                        Log.InfoFormat("setConnectionString: {0}", JobProcessorDPA.RemovePassword(param.GetConnectionString().ConnectionString));
                        Log.InfoFormat("setDataConnectionString: {0}", JobProcessorDPA.RemovePassword(param.GetDataConnectionString().ConnectionString));
                        Log.InfoFormat("RemoteAnalystConnectionString: {0}", JobProcessorDPA.RemovePassword(param.RemoteAnalystConnectionString.ConnectionString));
                        Log.InfoFormat("RemoteAnalystConnectionStringTrend: {0}", JobProcessorDPA.RemovePassword(param.RemoteAnalystConnectionStringTrend));
                        Log.InfoFormat("RemoteAnalystConnectionStringComparative: {0}", JobProcessorDPA.RemovePassword(param.RemoteAnalystConnectionStringComparative.ConnectionString));
                        Log.InfoFormat("saveLocation: {0}", saveLocation);
                        Log.InfoFormat("StartTime: {0}", _startTime);
                        Log.InfoFormat("StopTime: {0}", _stopTime);
                        Log.InfoFormat("Source: {0}", _source);
                        Log.InfoFormat("Dest: {0}", _dest);
                        Log.InfoFormat("Program: {0}", _program);
                        Log.InfoFormat("discQLen: {0}", discQLen);
                        Log.InfoFormat("diskFileTransientOpen: {0}", diskFileTransientOpen);
                        Log.InfoFormat("diskFileRequestsBlocked: {0}", diskFileRequestsBlocked);
                        Log.InfoFormat("Optional: {0}", _optional);
                        Log.InfoFormat("AutoID: {0}", _autoID);
                        Log.InfoFormat("ImageLocation: {0}", path);
                        Log.InfoFormat("ReportDownloadId: {0}", param.ReportDownloadId);
                        Log.Info("Calling CreateReport");

                        var createQT = new CreateExcelReport(param);
                        qtReport = createQT.CreateReports();

                        //Close the log file.
                        createQT.CloseLogFile();
                        Log.InfoFormat("Report Path: {0}", qtReport);
                        

                        var files = new List<string>();
                        if (File.Exists(qtReport))
                        {
                            files.Add(qtReport);
                        }

                        //Add help file.
                        /*if (File.Exists(saveLocation + "\\QT.txt")) {
                            files.Add(saveLocation + "\\QT.txt");
                        }*/

                        var reportStartTime = reportDownloadLogService.GetFirstLogDateFor(_reportDownloadId);
                        var totalReportTime = DateTime.Now - reportStartTime;

                        var hours = (totalReportTime.Days * 24) + totalReportTime.Hours;
                        var minutes = totalReportTime.Minutes;
                        var newMessage = "Analyses is completed. ET (hh:mm): " + hours.ToString("D2") + ":" + minutes.ToString("D2");
                        reportDownloadLogService.InsertNewLogFor(_reportDownloadId, DateTime.Now, newMessage);

                        Log.InfoFormat("Zip the Excel files at : {0}", DateTime.Now);
                        
                        //Zip the Excel files.
                        var zipReports = new ZIPReports();
                        string zipLocation = zipReports.CreateQTZipFile(_systemSerial, _systemName, files, _startTime,
                            _stopTime, saveLocation);

                        if (zipLocation.Equals(""))
                        {
                            throw new Exception();
                        }

                        var fileInfo = new FileInfo(zipLocation);
                        string zipSaveLocation = "";
                        if (ConnectionString.IsLocalAnalyst)
                        {
                            //Save to network location.
                            var networkSaveLocation = ConnectionString.NetworkStorageLocation;
                            if (!Directory.Exists(networkSaveLocation + "Systems/" + _systemSerial + "/" + _reportDownloadId + "/"))
                                Directory.CreateDirectory(networkSaveLocation + "Systems/" + _systemSerial + "/" + _reportDownloadId + "/");

                            zipSaveLocation = networkSaveLocation + "Systems/" + _systemSerial + "/" + _reportDownloadId + "/" + fileInfo.Name;

                            fileInfo.CopyTo(zipSaveLocation, true);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(ConnectionString.S3Reports))
                            {
                                //Save the file to S3.
                                var s3 = new AmazonS3(ConnectionString.S3Reports);
                                s3.WriteToS3WithLocaFile("Systems/" + _systemSerial + "/" + _reportDownloadId + "/" + fileInfo.Name, zipLocation);
                                //Build S3 full URL
                                zipSaveLocation = "Systems/" + _systemSerial + "/" + _reportDownloadId + "/" + fileInfo.Name;
                            }
                        }

                        Log.InfoFormat("zipSaveLocation : {0}", zipSaveLocation);
                        

                        var reportDownloads = new ReportDownloadService(ConnectionString.ConnectionStringDB);
                        reportDownloads.UpdateFileLocationFor(_reportDownloadId, zipSaveLocation);

                        Log.Info("Send Email");
                        

                        var sysInfo = new System_tblService(ConnectionString.ConnectionStringDB);
                        bool attachmentInEmail = sysInfo.GetAttachmentInEmailFor(_systemSerial);
                        //Check file size, greater than 5MB send notice email.
                        var emailReport = new EmailReport();
                        if (fileInfo.Length < 5242880 && attachmentInEmail)
                        {
                                emailReport.SendQTReportEmail(zipLocation, emailList, _systemSerial, _systemName, _startTime, _stopTime);
                        }
                        else
                        {
                            emailReport.SendQTReportNotification(emailList, _systemSerial, _systemName, _startTime, _stopTime, _reportDownloadId, attachmentInEmail);
                        }

                        foreach (var s in emailList)
                        {
                            reportDownloadLogService.InsertNewLogFor(_reportDownloadId, DateTime.Now, "Analyses is emailed to " + s + ".");
                        }
                        GC.Collect();
                        reportDownload.UpdateStatusFor(_reportDownloadId, 1);
                    }
                    catch (Exception ex)
                    {
                        reportDownload.UpdateStatusFor(_reportDownloadId, 2);
                        QTException = ex;

                        var errorEmails = new ErrorEmails();

                        if (ConnectionString.IsLocalAnalyst)
                        {
                            reportDownloadLogService.InsertNewLogFor(_reportDownloadId, DateTime.Now, localAnalystErrorMessage);
                            errorEmails.SendReportErrorEmail(_systemName, _startTime, _stopTime, ex.Message, "QT", "");
                        }
                        else
                        {
                            reportDownloadLogService.InsertNewLogFor(_reportDownloadId, DateTime.Now, errorMessage);
                            var ec2 = new AmazonEC2();
                            string instanceID = ec2.GetEC2ID();
                            errorEmails.SendReportErrorEmail(_systemName, _startTime, _stopTime, ex.Message, "QT", instanceID);
                        }
                        Log.ErrorFormat("QT Error: {0}", ex.Message);
                        Log.ErrorFormat("QT StackTrace: {0}", ex.StackTrace);
                        Log.ErrorFormat("QT Error InnerException: {0}", ex.InnerException);
                        
                        
                    }
                }
                catch
                {
                }
            }
            catch (Exception ex)
            {
                var amazon = new AmazonOperations();
                amazon.WriteErrorQueue("QT Error: " + ex.Message);

                reportDownload.UpdateStatusFor(_reportDownloadId, 2);
            }
            finally
            {
                var dirInfo = new DirectoryInfo(logSaveLocation);
                if (!dirInfo.Exists)
                {
                    dirInfo.Create();
                }

                string subject = ReportGeneratorStatus.GetEnumDescription(ReportGeneratorStatus.StatusMessage.QTCompleted);
                if (QTException != null)
                {
                    subject = ReportGeneratorStatus.GetEnumDescription(ReportGeneratorStatus.StatusMessage.QTException);
                    if (!ConnectionString.IsLocalAnalyst)
                    {
                        //since QT and DPA create new folder, there is no need to create unique log name.
                        try
                        {
                            var amazon = new AmazonOperations();
                            foreach (FileInfo file in dirInfo.GetFiles())
                            {
                                if (file.Name.Contains(".txt"))
                                {
                                    //log file name and location
                                    if (!file.Name.Contains("QT.txt"))
                                    {
                                        amazon.WriteToS3(file.Name, file.FullName);
                                        amazon.WriteErrorQueue(file.Name);
                                    }
                                }
                            }
                        }
                        catch (Exception exS3)
                        {
                            AmazonError.WriteLog(exS3, "JobProcessorQT.cs",
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
                //Delete the queue
                ClearQueue.Clear(Report.Types.QT, _queueID);
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
                        Log.InfoFormat("Calling SNS to terminate EC2 {0}", instanceID);
 
                        //Once the report is done, send message trigger the Lambda to shut down the instance
                        IAmazonSNS amazonSns = new AmazonSNS();
                        string messasge = ReportGeneratorInfo.GetReportGeneratorInfo(instanceID, launchTime, runningTime);
                        amazonSns.SendToTopic(subject, messasge, ConnectionString.SNSProdTriggerReportARN);
                    }
                }
            }
        }
    }
}