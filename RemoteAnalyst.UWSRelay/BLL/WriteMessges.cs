using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Data;
using RemoteAnalyst.AWS.Queue;
using RemoteAnalyst.AWS.SNS;
using RemoteAnalyst.BusinessLogic.BLL;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.BusinessLogic.Enums;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using System.Configuration;
using log4net;
using System.Windows.Forms;

namespace RemoteAnalyst.UWSRelay.BLL {
    internal class WriteMessges {
        ILog Log = LogManager.GetLogger("JobPool");
        private readonly string sqsqueue = string.Empty;

        public WriteMessges(string SQSqueue) {
            sqsqueue = SQSqueue;
        }

        public void write(string fileType, string systemSerial, string fileName, int uwsId, int uploadID = 0) {
            if (systemSerial == "078998")
                systemSerial = "078831";

            string tempSystemSerial = systemSerial;
            //add '0' before 5 digit system serial
            while (tempSystemSerial.Length < 6) {
                tempSystemSerial = "0" + tempSystemSerial;
            }
            if (ConnectionString.IsLocalAnalyst) {
                Log.Info("Insert into Trigger Table");
                
                var saveLocation = ConnectionString.NetworkStorageLocation + "Systems\\" + tempSystemSerial + "\\" + fileName;
                Log.Info("saveLocation: " + saveLocation);
                

                var triggerService = new TriggerService(ConnectionString.ConnectionStringDB);
                triggerService.InsertFor(tempSystemSerial, (int)TriggerType.Type.Loader, fileType, saveLocation, uploadID, uwsId);
            } else {
                System_tblService system_TblService = new System_tblService(ConnectionString.ConnectionStringDB);
                var systemAndExpiredDate = system_TblService.GetEndDateFor(tempSystemSerial);
                if (systemAndExpiredDate.Count == 0) {
                    Log.InfoFormat("{0} not found in System_tbl", tempSystemSerial);
                    
                    try {
                        var email = new EmailHelper();
                        email.SendLicenseExpireEmail(tempSystemSerial, EmailHelper.LicenseExpireEmailReason.NotFound);
                    } catch (Exception ex) {
                        Log.ErrorFormat("Error occurs when sending unregistered system email: {0}", ex);
                    }
                    return;
                }
                Decrypt decrypt = new Decrypt();
                var expireDate = Convert.ToDateTime(decrypt.strDESDecrypt(systemAndExpiredDate.Values.First()).Split(' ')[1]);
                int timeZoneIndex = system_TblService.GetTimeZoneFor(tempSystemSerial);
                DateTime systemLocalTime = TimeZoneInformation.ToLocalTime(timeZoneIndex, DateTime.Now);

                if (systemLocalTime.Date > expireDate.AddDays(7).Date) {
                    Log.InfoFormat("{0} expired and out of 7 days of grace period", tempSystemSerial);
                    
                    if (!tempSystemSerial.Equals("077105")) { //This is for Walgreen, Don't send message to support.
                        try {
                            var email = new EmailHelper();
                            email.SendLicenseExpireEmail(tempSystemSerial, EmailHelper.LicenseExpireEmailReason.Expired);
                        } catch (Exception ex) {
                            Log.ErrorFormat("Error occurs when sending expire email: {0}", ex);
                        }
                    }
                    var loadingInfoService = new LoadingInfoService(ConnectionString.ConnectionStringDB);
                    try {
                        loadingInfoService.DeleteLoadingInfoByUWSID(uwsId);
                    } catch (Exception ex) {
                        Log.InfoFormat("Failed clear LoadingInfo table: {0}", ex);
                    }
                    return;
                } else if (systemLocalTime.Date > expireDate.Date) {
                    Log.InfoFormat("{0} expired but within 7 days of grace period", tempSystemSerial);
                    
                    if (!tempSystemSerial.Equals("077105")) { //This is for Walgreen, Don't send message to support.
                        try {
                            var email = new EmailHelper();
                            email.SendLicenseExpireEmail(tempSystemSerial, EmailHelper.LicenseExpireEmailReason.ExpiredWithinGrace);
                        } catch (Exception ex) {
                            Log.ErrorFormat("Error occurs when sending expire email: {0}", ex);
                        }
                    }
                }

                string message = fileType.ToUpper() + "\n" +
                                    tempSystemSerial + "\n" +
                                    "Systems/" + tempSystemSerial + "/" + fileName + "\n" +
                                    uwsId;
                if (uploadID > 0)
                    message += "\n" + uploadID;
                WriteMessageToSQS(sqsqueue, message, systemSerial);

            }
            Log.InfoFormat("Done at {0}", DateTime.Now);
        }

        private void WriteMessageToSQS(string queueName, string message, string systemSerial) {
            int retry = 0;

            var amazonSqs = new AmazonSQS();
            string urlQueue = string.Empty;
            do {
                Log.InfoFormat("sqsqueue: {0}", queueName);
                
                urlQueue = amazonSqs.GetAmazonSQSUrl(queueName);
                Log.InfoFormat("urlQueue: {0}", urlQueue);
                
                if (urlQueue.Length > 0) {
                    var messageId = amazonSqs.WriteMessageWithGroupName(urlQueue, message, systemSerial);
                    retry = 5;
                    Log.InfoFormat("messageId: {0}, message: {1}, retry {2}", messageId, message, retry);
                    
                } else {
                    //Retry after duration.
                    Thread.Sleep(AmazonError.GetTimeoutDuration(retry));
                    retry++;
                    Log.InfoFormat("retry {0}", retry);
                    
                    if (retry == 5) {
                        AmazonError.WriteLog("urlQueue is empty", "Amazon.cs: CheckDispathTrendReport " + queueName,
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
            while (retry < 5);
        }

        public void WriteToReportQueue(string fileType, int orderId, string systemSerial, string fileName) {
            int retry = 0;
            string tempSystemSerial = systemSerial;
            //add '0' before 5 digit system serial
            while (tempSystemSerial.Length < 6) {
                tempSystemSerial = "0" + tempSystemSerial;
            }
            
            if (ConnectionString.IsLocalAnalyst) {
                Log.Info("Insert into Trigger Table");
                

                var saveLocation = ConnectionString.NetworkStorageLocation + "Systems\\" + tempSystemSerial + "\\" + fileName;
                var triggerService = new TriggerService(ConnectionString.ConnectionStringDB);
                triggerService.InsertFor(tempSystemSerial, (int)TriggerType.Type.Loader, fileType, saveLocation, orderId, 0);
            } else {
                var amazonSqs = new AmazonSQS();
                string message = fileType.ToUpper() + "|" +
                                 orderId + "|" +
                                 tempSystemSerial + "|" +
                                 "Systems/" + tempSystemSerial + "/" + fileName;

                do {
                    var urlQueue = amazonSqs.GetAmazonSQSUrl("sqs-prod-reportQ");
                    Log.InfoFormat("urlQueue: {0}", urlQueue);
                    

                    if (urlQueue.Length > 0) {
                        amazonSqs.WriteMessage(urlQueue, message);
                        retry = 5;
                        Log.InfoFormat("message: {0}, retry: {1}", message, retry);
                    } else {
                        //Retry after duration.
                        Thread.Sleep(AmazonError.GetTimeoutDuration(retry));
                        retry++;
                        Log.InfoFormat("retry: {0}", retry);
                        
                        if (retry == 5) {
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
                                ConnectionString.IsLocalAnalyst,
                                ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                        }
                    }
                }
                while (retry < 5);
            }
            Log.InfoFormat("Done at {0}", DateTime.Now);
        }

        internal void SubmitSnsCall(string fileType, string systemSerial, string fileName, int uwsId) {
            if (systemSerial == "078998")
                systemSerial = "078831";

            System_tblService system_TblService = new System_tblService(ConnectionString.ConnectionStringDB);
            var systemAndExpiredDate = system_TblService.GetEndDateFor(systemSerial);
            if (systemAndExpiredDate.Count == 0) {
                Log.InfoFormat("{0} not found in System_Tbl.", systemSerial);
                return;
            }
            Decrypt decrypt = new Decrypt();
            var expireDate = Convert.ToDateTime(decrypt.strDESDecrypt(systemAndExpiredDate.Values.First()).Split(' ')[1]);
            int timeZoneIndex = system_TblService.GetTimeZoneFor(systemSerial);
            DateTime systemLocalTime = TimeZoneInformation.ToLocalTime(timeZoneIndex, DateTime.Now);
            if (systemLocalTime.Date > expireDate.AddDays(7).Date) {
                Log.InfoFormat("{0} expired and out of 7 days of grace period.", systemSerial);
                
                var loadingInfoService = new LoadingInfoService(ConnectionString.ConnectionStringDB);
                try {
                    loadingInfoService.DeleteLoadingInfoByUWSID(uwsId);
                } catch (Exception ex) {
                    Log.ErrorFormat("Failed clear LoadingInfo table: {0}", ex);
                }
                return;
            } else if (systemLocalTime.Date > expireDate.Date) {
                Log.InfoFormat("{0} expired but within 7 days of grace period.", systemSerial);
            }

            string message = fileType.ToUpper() + "|" + systemSerial + "|" + "Systems/" + systemSerial + "/" + fileName + "|" + uwsId;
            Log.InfoFormat("Message: {0}", message);

            try {
                Log.Info("Calling Topic.");
                IAmazonSNS amazonSns = new AmazonSNS();
                string subjectMessage = ReportGeneratorStatus.GetEnumDescription(ReportGeneratorStatus.StatusMessage.Loader);
                amazonSns.SendToTopic(subjectMessage, message, ConnectionString.SNSLambdaLoader);
            } catch (Exception ex) {
                Log.ErrorFormat("Error: {0}", ex);
                SnsRetry(message);
            }
            //Make sure lambda won't timed out
            //Thread.Sleep(60000);
        }

        private void SnsRetry(string message) {
            //Retry logic.
            var retry = 1;
            do {
                try {
                    Log.InfoFormat("Retry: {0}", retry);
                    Thread.Sleep(AmazonError.GetTimeoutDuration(retry));
                    IAmazonSNS amazonSns = new AmazonSNS();
                    string subjectMessage = ReportGeneratorStatus.GetEnumDescription(ReportGeneratorStatus.StatusMessage.Loader);
                    amazonSns.SendToTopic(subjectMessage, message, ConnectionString.SNSLambdaLoader);
                    retry = 5;
                } catch (Exception ex) {
                    Log.ErrorFormat("Error: {0}", ex);
                    retry++;
                }
            }
            while (retry < 5);
        }
    }
}