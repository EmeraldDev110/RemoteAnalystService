using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using RemoteAnalyst.AWS.SNS;
using RemoteAnalyst.AWS.CloudWatch;
using RemoteAnalyst.AWS.EC2;
using RemoteAnalyst.AWS.Infrastructure;
using RemoteAnalyst.AWS.Queue;
using RemoteAnalyst.AWS.Queue.View;
using RemoteAnalyst.BusinessLogic.BLL;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.Enums;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.UWSLoader.BLL.Process;
using RemoteAnalyst.UWSLoader.JobProcessor;
using RemoteAnalyst.UWSLoader.SPAM.BLL;
using System.Threading.Tasks;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.UWSLoader.BLL;
using RemoteAnalyst.UWSLoader.TableUpdater;
using RemoteAnalyst.BusinessLogic;
using log4net;
using aspNetEmail.vCardWriter;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Policy;

namespace RemoteAnalyst.UWSLoader.BLL.SQS {
    public class CheckQue
    {
        private readonly ILog _log;
        private static DateTime _emailsendDateTime = DateTime.MinValue;
        private readonly AmazonSQS _queue;
        private bool   _tableUpDateCheckDone = false;
        private string _loadType = "";
        private string _s3Location = "";
        private string _systemSerial = "";
        private int _ntsID;
        private int _uwsId;
        private string _instanceId;
        private TrafficManager _trafficManager;

        /// <summary>
        /// Constructor
        /// </summary>
        public CheckQue(ILog log) {
            _log = log;
            if (!ConnectionString.IsLocalAnalyst) { 
                _queue = new AmazonSQS();
                var ec2 = new AmazonEC2();
                _instanceId = ec2.GetEC2ID();
                _trafficManager = new TrafficManager(_log, _instanceId);
                if (!_trafficManager.IsLoaderActive()) { 
                    _log.InfoFormat("EC2 {0} is inactive. Check MonitorEC2 settings.", _instanceId);
                }
            }
            _tableUpDateCheckDone = false;
        }

        /// <summary>
        /// Check the queue to see if there is any messages.
        /// If there is, process the message and call different job processor according to the load type.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void CheckUWS(object source, ElapsedEventArgs e) {
            //Force all datetime to be in US format.
            
            string queueURL = "";
            var message = new List<MessageView>();
            _ntsID = 0;
            _uwsId = 0;
            var triggerId = 0;
            var lastMessageReadTime = DateTime.Now;

            try {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

                var loadingStatus = new LoadingStatusService(ConnectionString.ConnectionStringDB);
                var primaryEc2 = "";
                if (!string.IsNullOrEmpty(ConnectionString.PrimaryEC2))
                    primaryEc2 = ConnectionString.PrimaryEC2;

                bool isOkayToLoad = true;

                if (ConnectionString.IsLocalAnalyst)
                    isOkayToLoad = loadingStatus.IsLoadAvailableFor(primaryEc2);

                if (!isOkayToLoad) {
                    //Load DiscOpen, CPUINFO, JOURNAL, QNM, QNMCLIM and SCM even queue is full.
                    if (primaryEc2.Equals(_instanceId)) {

                        try {
                            if (ConnectionString.IsLocalAnalyst)
                            {
                                var trigger = new TriggerService(ConnectionString.ConnectionStringDB);
                                var triggers = trigger.GetTriggerFor((int)TriggerType.Type.Loader);
                                triggerId = triggers.TriggerId;
                                if (triggerId > 0)
                                {
                                    message.Add(new MessageView
                                    {
                                        //Body = triggers.FileType + "\n" + triggers.SystemSerial + "\n" + triggers.FileLocation
                                        Body = triggers.FileType + "\n" + triggers.SystemSerial + "\n" + triggers.FileLocation + "\n" + triggers.UwsId + "\n" + triggers.UploadId
                                    });
                                }
                                else
                                {
                                    return;
                                }
                            }
                            else {
                                //Check CPU Info and OSS
#if RDSMove
                                if (!string.IsNullOrEmpty(ConnectionString.SQSMultiLoad)) {
                                    var sqsLoads = ConnectionString.SQSMultiLoad.Split('|');
                                    _log.InfoFormat("Load/MultiLoad Queue name is: " + ConnectionString.SQSMultiLoad);
                                    
#else
                                if (!string.IsNullOrEmpty(ConnectionString.SQSLoad)) {
                                    var sqsLoads = ConnectionString.SQSLoad.Split('|');
                                    _log.InfoFormat("Load/MultiLoad Queue name is: " + ConnectionString.SQSLoad);
                                    

#endif
                                    lastMessageReadTime = DateTime.Now;
                                    foreach (var sqsLoad in sqsLoads) {
                                        queueURL = _queue.GetAmazonSQSUrl(sqsLoad);
                                        var tempMessage = _queue.ReadMessage(queueURL);
                                        //if (!string.IsNullOrEmpty(tempMessage.Body)) {
                                        if (tempMessage != null && !string.IsNullOrEmpty(tempMessage.Body)) {
                                            tempMessage.QueueURL = queueURL;
                                            message.Add(tempMessage);
                                        }
                                    }
                                } else {
                                    return;
                                }
                            }

                            foreach (var msg in message) {
                                if (!ConnectionString.IsLocalAnalyst) {
                                    //Delete Queue
                                    try {
                                        _queue.DeleteMessage(msg.QueueURL, msg.ReceiptHandle);
                                    } catch (Exception se) {
                                        var amazonOperations = new AmazonOperations();
                                        StringBuilder errorMessage = new StringBuilder();
                                        errorMessage.Append("Time: " + DateTime.Now + " \r\n");
                                        errorMessage.Append("LastMessageReadTime: " + lastMessageReadTime + " \r\n");
                                        errorMessage.Append("Source: CheckQue.cs::CheckUWS - NonSystemData \r\n");
                                        errorMessage.Append("instanceId: " + _instanceId + "\r\n");
                                        errorMessage.Append("Message: " + se.Message + "\r\n");
                                        errorMessage.Append("StackTrace: " + se.StackTrace + "\r\n");
                                        errorMessage.Append("QueueURL: " + msg.QueueURL + "\r\n");
                                        errorMessage.Append("Message Count: " + message.Count + " \r\n");
                                        errorMessage.Append("ReceiptHandle: " + msg.ReceiptHandle + "\r\n");
                                        if (msg.Body == null) {
                                            errorMessage.Append("Body: is NULL \r\n");
                                        } else {
                                            errorMessage.Append("Body: " + msg.Body + "\r\n");
                                        }
                                        amazonOperations.WriteErrorQueue(errorMessage.ToString());
                                    }
                                }
                                string[] tempSplit = msg.Body.Split('\n');
                                if (tempSplit.Length >= 3) {
                                    string tempLoadType = tempSplit[0];
                                    string tempSystemSerial = tempSplit[1];
                                    string tempS3Location = tempSplit[2];

                                    if (tempSplit.Length.Equals(4))
                                        _uwsId = Convert.ToInt32(tempSplit[3]);

                                    if (tempSplit.Length.Equals(5))
                                        _ntsID = Convert.ToInt32(tempSplit[4]);

                                    _log.InfoFormat("tempLoadType: " + tempLoadType);
                                    _log.InfoFormat("tempSystemSerial: " + tempSystemSerial);
                                    _log.InfoFormat("tempS3Location: " + tempS3Location);
                                    

                                    if (tempLoadType == "SYSTEM" && tempS3Location.Contains("DO")) {
                                        _log.Info("Calling DISCOPEN");
                                        
                                        var processData = new ProcessData(tempLoadType, tempSystemSerial, tempS3Location, _uwsId, 0);
                                        var thread = new Thread(processData.StartProcess) {
                                            IsBackground = true
                                        };
                                        thread.Start();
                                    } else if (tempLoadType == "JOURNAL") {
                                        _log.Info("Calling JOURNAL");
                                        
                                        string ossFileName = "";

                                        if (ConnectionString.IsLocalAnalyst)
                                        {
                                            if (File.Exists(tempS3Location))
                                            {
                                                var fileInfo = new FileInfo(tempS3Location);
                                                ossFileName = fileInfo.Name;
                                            }
                                        }
                                        else {
                                            ossFileName = tempSplit[2].Split('|')[0].Split('/')[2];
                                        }

                                        var oss = new ProcessOSSJRNL(tempSystemSerial, ossFileName, tempS3Location, _uwsId);
                                        var thread = new Thread(oss.StartProcess) {
                                            IsBackground = true
                                        }; //change the functio name.
                                        thread.Start();
                                    } else if (tempLoadType == "QNM") {
                                        var qnm = new OpenUWSQNM(tempSystemSerial, tempS3Location, _ntsID, ConnectionString.DatabasePrefix, _uwsId);
                                        var thread = new Thread(qnm.CreateNewData) {
                                            IsBackground = true
                                        };
                                        thread.Start();
                                    } else if (tempLoadType == "QNMCLIM") {
                                        var qnm = new OpenUWSQNMCLIM(tempSystemSerial, tempS3Location, _ntsID, ConnectionString.DatabasePrefix, _uwsId);
                                        var thread = new Thread(qnm.CreateNewData) {
                                            IsBackground = true
                                        };
                                        thread.Start();
                                    } else if (tempLoadType == "SCM") {
                                        string systemSerial = tempSplit[1];
                                        int profileID = int.Parse(tempSplit[2]);
                                        var jobSCM = new JobSCM(systemSerial, profileID);
                                        var thread = new Thread(jobSCM.GenerateSCMData) {
                                            IsBackground = true
                                        };
                                        thread.Start();
                                    } else if (tempLoadType == "APPLICATION") {
                                        string systemSerial = tempSplit[1];
                                        int profileId = int.Parse(tempSplit[2]);
                                        int customerId = int.Parse(tempSplit[3]);
                                        var jobApplication = new JobApplication(systemSerial, profileId, customerId);
                                        var thread = new Thread(jobApplication.GenerateApplicationData) {
                                            IsBackground = true
                                        };
                                        thread.Start();
                                    } else {
                                        return;
                                    }
                                }

                                _log.Info("Delete NonSystemData Message");
                                if (ConnectionString.IsLocalAnalyst) {
                                    if (triggerId > 0) {
                                        var trigger = new TriggerService(ConnectionString.ConnectionStringDB);
                                        trigger.DeleteTriggerFor(triggerId);
                                    }
                                }

                                //Clear the message.
                                msg.Body = "";
                            }

                            //Clear the message.
                            message.Clear();

                        } catch (Exception ex) {
                            _log.ErrorFormat("Error: {0}", ex);
                        }
                    } else {

                        bool isCheckSQS = loadingStatus.IsLoadAvailableFor(_instanceId);
                        if (isCheckSQS) {

 
                            // adding condition for double load
#if RDSMove
                                if (!string.IsNullOrEmpty(ConnectionString.SQSMultiLoad)) {
                                    var sqsLoads = ConnectionString.SQSMultiLoad.Split('|');

#else
                            if (!string.IsNullOrEmpty(ConnectionString.SQSLoad)) {
                                var sqsLoads = ConnectionString.SQSLoad.Split('|');

#endif

                                foreach (var sqsLoad in sqsLoads) {

                                    queueURL = _queue.GetAmazonSQSUrl(sqsLoad);
                                    var tempMessage = _queue.ReadMessage(queueURL);
                                    // changing the condition - Getting the object reference error - message is null
                                    //if (!string.IsNullOrEmpty(tempMessage.Body)) {
                                    if (tempMessage != null && !string.IsNullOrEmpty(tempMessage.Body)) {
                                        tempMessage.QueueURL = queueURL;
                                        message.Add(tempMessage);
                                    }
                                }
                            } else return;
                        } else {
                            return;
                        }
                    }
                } 
                else {
                    if (ConnectionString.IsLocalAnalyst)
                    {
                        var trigger = new TriggerService(ConnectionString.ConnectionStringDB);
                        var triggers = trigger.GetTriggerFor((int)TriggerType.Type.Loader);
                        triggerId = triggers.TriggerId;
                        if (triggerId > 0)
                        {
                            message.Add(new MessageView
                            {
                                Body = triggers.FileType + "\n" + triggers.SystemSerial + "\n" + triggers.FileLocation + "\n" + triggers.UwsId + "\n" + triggers.UploadId
                            });
                        }
                        else
                        {
                            return;
                        }

                    }
                    else
                    {
                        isOkayToLoad = _trafficManager.IsLoaderOKToLoad();
                        if (!isOkayToLoad)
                        {
                            EventLog.WriteEntry("UWSLoader", "_trafficManager said " + isOkayToLoad);
                            return;
                        }
#if RDSMove
                        if (!string.IsNullOrEmpty(ConnectionString.SQSMultiLoad)) {
                            var sqsLoads = ConnectionString.SQSMultiLoad.Split('|');
                            _log.InfoFormat("Queue name is: {0}", ConnectionString.SQSMultiLoad);
#else
                        if (!string.IsNullOrEmpty(ConnectionString.SQSLoad)) {
                            var sqsLoads = ConnectionString.SQSLoad.Split('|');
                            _log.InfoFormat("Queue name _log is: {0}", ConnectionString.SQSLoad);
#endif
                            foreach (var sqsLoad in sqsLoads) {
                                var rdsName = "";
                                var systemSerial = "";
#if DEBUG
                                var isOverCpuLimit = false;
#else
                                var isOverCpuLimit = _trafficManager.CheckLoaderCpuBusy();
#endif

                                if (!isOverCpuLimit) {
                                        
                                    queueURL = _queue.GetAmazonSQSUrl(sqsLoad);

                                    _log.InfoFormat("queueURL is not null: {0}", queueURL);
                                    
                                    var tempMessage = _queue.ReadMessage(queueURL);
                                    //Tracking the message
                                    _log.InfoFormat("Reading tempMessage from queueURL: {0}", tempMessage);
                                    
                                    // changing the condition - Getting the object reference error - message is null
                                    //if (!string.IsNullOrEmpty(tempMessage.Body)) {
                                    if (tempMessage != null && !string.IsNullOrEmpty(tempMessage.Body)) {

                                        //Get SystemSerial and get database name.
                                        string[] split = tempMessage.Body.Split('\n');

                                        if (split.Length > 3) {
                                            systemSerial = split[1];
#if DEBUG
                                            if (systemSerial != "077730")
                                            {
                                                continue;
                                            }
#endif 
                                            //Get database name.
                                            var databaseMapping = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
                                            var connectionString = databaseMapping.GetConnectionStringFor(systemSerial);

                                            var tempString = connectionString.Split(';');
                                            foreach (var s in tempString) {
                                                if (s.ToUpper().Contains("SERVER=")) {
                                                    rdsName = s.Split('=')[1].Split('.')[0];
                                                }
                                            }
                                        }

#if DEBUG
                                        tempMessage.QueueURL = queueURL;
                                        message.Add(tempMessage);
                                        lastMessageReadTime = DateTime.Now;
#else
                                        // Per Khody: Limit load per RDS to no more than 2 simultaneous per Server. 
                                        // So not looking at load per EC2 Load instance but overall systemSerial
                                        // For now the block has been put at the query level
                                        var isSystemLoadCountOverLimit = _trafficManager.CheckSystemLoadOverLimit(systemSerial);

                                        if (!isSystemLoadCountOverLimit) {
                                            var isOverRdsLimit = _trafficManager.CheckRdsCpuBusy(rdsName);

                                            if (!isOverRdsLimit) {
                                                tempMessage.QueueURL = queueURL;
                                                message.Add(tempMessage);
                                                lastMessageReadTime = DateTime.Now;
                                            } else {
                                                message.Clear();
                                            }
                                        } else {
                                            message.Clear();
                                        }
#endif
                                        _log.InfoFormat("messages so far {0}", message.Count);
                                        
                                            
                                    }
                                    else
                                    {
                                        _log.Info("No message to read");
                                        
                                    }
                                }
                            }
                            _log.Info("Loop is done");
                        } else return;
                    }

                }

            } catch (Exception ex) {
#region Send Error Message

                TimeSpan emailSpan = DateTime.Now - _emailsendDateTime;

                if (!ConnectionString.IsLocalAnalyst) {
                    _log.ErrorFormat("QueueName is: {0}, Error {1}", ConnectionString.SQSLoad, ex);
                }

                //Send Failed message.
                if (emailSpan.TotalMinutes > 30) {
                    AmazonError.WriteLog(ex, "CheckQue.cs: CheckUWS",
                        ConnectionString.AdvisorEmail,
                        ConnectionString.SupportEmail,
                        ConnectionString.WebSite,
                        ConnectionString.EmailServer,
                        ConnectionString.EmailPort,
                        ConnectionString.EmailUser,
                        ConnectionString.EmailPassword,
                        ConnectionString.EmailAuthentication, ConnectionString.SystemLocation,
                        ConnectionString.ServerPath,
                        ConnectionString.EmailIsSSL,
                        ConnectionString.IsLocalAnalyst,
                        ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain
                    );

                    //Reset the time.
                    _emailsendDateTime = DateTime.Now;
                }
                return;

#endregion
            }

            EventLog.WriteEntry("UWSLoader", "Messages to process " + message.Count);
            foreach (var msg in message) {
                if (!ConnectionString.IsLocalAnalyst) {
                    //Delete Queue
                    try {
                        _queue.DeleteMessage(msg.QueueURL, msg.ReceiptHandle);
                    }
                    catch(Exception se)
                    {
                        var amazonOperations = new AmazonOperations();
                        StringBuilder errorMessage = new StringBuilder();
                        errorMessage.Append("Time: " + DateTime.Now + " \r\n");
                        errorMessage.Append("LastMessageReadTime: " + lastMessageReadTime + " \r\n");                                        
                        errorMessage.Append("Source: CheckQue.cs::CheckUWS - System \r\n");
                        errorMessage.Append("instanceId: " + _instanceId + "\r\n");
                        errorMessage.Append("Message: " + se.Message + "\r\n");
                        errorMessage.Append("StackTrace: " + se.StackTrace + "\r\n");
                        errorMessage.Append("QueueURL: " + msg.QueueURL + "\r\n");
                        errorMessage.Append("ReceiptHandle: " + msg.ReceiptHandle + "\r\n");
                        errorMessage.Append("Message Count: " + message.Count + " \r\n");
                        if (msg.Body == null) {
                            errorMessage.Append("Body: is NULL \r\n");
                        }
                        else {
                            errorMessage.Append("Body: " + msg.Body + "\r\n");
                        }
                        amazonOperations.WriteErrorQueue(errorMessage.ToString());
                    }
                }
                EventLog.WriteEntry("UWSLoader", "Region LoadData " + msg.Body);
                if (msg.Body.Length > 0) {
                    string[] split = msg.Body.Split('\n');

                    if (split.Length < 3) {
#region Error Message
                        _log.ErrorFormat("message.Body: {0}, invalid message format", msg.Body);
                        if (!ConnectionString.IsLocalAnalyst) {
                            var amazonOperations = new AmazonOperations();
                            StringBuilder errorMessage = new StringBuilder();
                            errorMessage.Append("Source: CheckQue.cs::CheckUWS::Processing System data \r\n");
                            errorMessage.Append("instanceId: " + _instanceId + "\r\n");
                            errorMessage.Append("QueueURL: " + msg.QueueURL + "\r\n");
                            errorMessage.Append("message: " + msg.Body + "\r\n");
                            errorMessage.Append("Message: Invalid message read from queue\r\n");
                            amazonOperations.WriteErrorQueue(errorMessage.ToString());
                        }
#endregion
                    }
                    else {
                        #region Load Data
                        EventLog.WriteEntry("UWSLoader", "Region LoadData " + msg.Body);
                        _loadType = split[0];
                        _systemSerial = split[1];
                        _s3Location = split[2];

                        if (split.Length.Equals(4))
                            _uwsId = Convert.ToInt32(split[3]);

                        if (split.Length.Equals(5)) {
                            _uwsId = Convert.ToInt32(split[3]);
                            _ntsID = Convert.ToInt32(split[4]);
                        }
                        string invalidLicenseReason = "";

                        // Add License check. If License fails print information and continue
                        if (!LicenseChecker.ValidLicenseToLoad(_systemSerial, ref invalidLicenseReason)) {
                            EventLog.WriteEntry("UWSLoader", "CheckQueue: Line 667: " + _systemSerial + " " + invalidLicenseReason);
                            //ToDO: Delete file since the system does not have a valid license.
                            var deleteFile = new FileInfo(_s3Location);
                            try {
                                if (File.Exists(deleteFile.FullName)) {
                                    File.Delete(deleteFile.FullName);
                                }
                                EventLog.WriteEntry("UWSLoader", "CheckQueue: Line 681: _uwsId: " + _uwsId);
                                var loadingInfoService = new LoadingInfoService(ConnectionString.ConnectionStringDB);
                                loadingInfoService.UpdateLoadingStatusFor(_uwsId, "Fail");
                                loadingInfoService.UpdateFor(_uwsId.ToString());
                                EventLog.WriteEntry("UWSLoader", "CheckQueue: Line 684: " + _systemSerial);
                                _log.InfoFormat("License for {0} has expired or not found. Hence not loading {1}. Reason: {2}",
                                    _systemSerial, _s3Location, invalidLicenseReason);
                            }
                            catch (Exception ex) {
                                EventLog.WriteEntry("UWSLoader", "CheckQueue: Line 693: " + _systemSerial + " " + ex);
                            }
                            continue;
                        }
                        
                        // CHECK First if Tables have up to date primary Keys
                        // Tables will only update if the primary key is missing
                        if (!_tableUpDateCheckDone) { 
                            DatabaseMappingService databaseMappingService = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
                            string systemConnectionString = databaseMappingService.GetConnectionStringFor(_systemSerial);
                            TableUpdater.TableUpdater tableUpdater = new RemoteAnalyst.UWSLoader.TableUpdater.TableUpdater(systemConnectionString);
                            tableUpdater.UpdateTablePrimaryKeys();
                            _tableUpDateCheckDone = true;
                        }

                        EventLog.WriteEntry("UWSLoader", "_loadType: " +_loadType);
                        if (_loadType == "SYSTEM" || _loadType == "DISK" || _loadType == "PATHWAY" || _loadType == "UWS") {
                            DateTime selectedStartTime;
                            DateTime selectedStopTime;
                            if (split.Length >= 5 && DateTime.TryParse(split[3], out selectedStartTime) && DateTime.TryParse(split[4], out selectedStopTime)) {

                                //put it on LoadingStatusDetailDISCOPEN
                                var loadingStatusDISCOPEN = new LoadingStatusDetailDISCOPENService(ConnectionString.ConnectionStringDB);
                                loadingStatusDISCOPEN.InsertLoadingStatusFor(_s3Location, DateTime.Now, _systemSerial, "", selectedStartTime, selectedStopTime, _instanceId);
                            }
                            else {
                                //Insert into LoadingStatusDetail.
                                var dataFileName = "";
                                if (ConnectionString.IsLocalAnalyst)
                                {
                                    if (File.Exists(_s3Location))
                                    {
                                        var fileInfo = new FileInfo(_s3Location);
                                        dataFileName = fileInfo.Name;
                                    }
                                }
                                else
                                {
                                    var temp = _s3Location.Split('|');
                                    if (temp.Length == 1)
                                        dataFileName = _s3Location.Split('/')[2];
                                    else
                                    {
                                        dataFileName = temp[0].Split('/')[2];
                                    }
                                }
                                                                
                                var loadingStatusDetailService = new LoadingStatusDetailService(ConnectionString.ConnectionStringDB);
                                loadingStatusDetailService.InsertLoadingStatusFor(dataFileName, "", DateTime.Now, _systemSerial, "", _uwsId, 0, "System", _instanceId);
                                
                                var process = new JobProcess();
                                process.UpdateLoadingStatusDetail(dataFileName, _systemSerial);
                                
                                var loadingInfo = new LoadingInfoService(ConnectionString.ConnectionStringDB);
                                loadingInfo.UpdateInstanceIDFor(_uwsId, _instanceId);

                                var processData = new ProcessData(_loadType, _systemSerial, _s3Location, _uwsId, _ntsID);
                                var thread = new Thread(processData.StartProcess) {
                                    IsBackground = true
                                };
                                thread.Start();
                            }
                        }
                        else if (_loadType == "JOURNAL") {
                            var ossFileName = "";
                            if (ConnectionString.IsLocalAnalyst)
                            {
                                if (File.Exists(_s3Location))
                                {
                                    var fileInfo = new FileInfo(_s3Location);
                                    ossFileName = fileInfo.Name;
                                }
                            }                                
                            else {
                                ossFileName = split[2].Split('/')[2];
                            }

                            var oss = new ProcessOSSJRNL(_systemSerial, ossFileName, _s3Location, _uwsId);
                            var thread = new Thread(oss.StartProcess) {
                                IsBackground = true
                            }; //change the functio name.
                            thread.Start();
                        }
                        else if (_loadType == "QNM") {
                            var qnm = new OpenUWSQNM(_systemSerial, _s3Location, _ntsID, ConnectionString.DatabasePrefix, _uwsId);
                            var thread = new Thread(qnm.CreateNewData) {
                                IsBackground = true
                            };
                            thread.Start();
                        }
                        else if (_loadType == "QNMCLIM") {
                            var qnm = new OpenUWSQNMCLIM(_systemSerial, _s3Location, _ntsID, ConnectionString.DatabasePrefix, _uwsId);
                            var thread = new Thread(qnm.CreateNewData) {
                                IsBackground = true
                            };
                            thread.Start();
                        }
                        else if (_loadType == "SCM") {
                            //_s3Location will be scm profile id.
                            string systemSerial = split[1];
                            int profileID = int.Parse(split[2]);
                            var jobSCM = new JobSCM(systemSerial, profileID);
                            var thread = new Thread(jobSCM.GenerateSCMData) {
                                IsBackground = true
                            };
                            thread.Start();
                        }

                        //Delete Message from Queue or Trigger
                        if (ConnectionString.IsLocalAnalyst) {
                            if (triggerId > 0) {
                                var trigger = new TriggerService(ConnectionString.ConnectionStringDB);
                                trigger.DeleteTriggerFor(triggerId);
                            }
                        }

#endregion
                    }
                }
            }
        }

	}
}