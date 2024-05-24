using System;
using System.Threading;
using RemoteAnalyst.AWS.EC2;
using RemoteAnalyst.AWS.Queue;
using RemoteAnalyst.AWS.Queue.View;
using RemoteAnalyst.AWS.SNS;
using RemoteAnalyst.BusinessLogic.BLL;
using RemoteAnalyst.BusinessLogic.Enums;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.ReportGenerator.BLL.Schedule;

namespace RemoteAnalyst.ReportGenerator {
    /// <summary>
    /// Amazon reads ReportQ and calls function to generate report.
    /// </summary>
    internal class Amazon {
        /// <summary>
        /// Calls Amazon SQS to read message for ReportQ. Once message is read, ReadQueue calls function to generate report.
        /// </summary>
        internal void ReadQueue() {
            var sqs = new AmazonSQS();
            string queueUrl = "";

            DateTime emailSendDateTimeURL = DateTime.MinValue;
            do {
                try {
                    //Read Queue.
                    if (!string.IsNullOrEmpty(ConnectionString.SQSReportOptimizer))
                        queueUrl = sqs.GetAmazonSQSUrl(ConnectionString.SQSReportOptimizer);
                }
                catch (Exception ex) {
                    TimeSpan emailSpan = DateTime.Now - emailSendDateTimeURL;
                    if (emailSpan.TotalMinutes > 30) {
                        AmazonError.WriteLog(ex, "Amazon.cs: GetAmazonSQSUrl",
                            ConnectionString.AdvisorEmail,
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
                            ConnectionString.MailGunSendAPIKey,
                            ConnectionString.MailGunSendDomain);
                        emailSendDateTimeURL = DateTime.Now;
                    }
                    Thread.Sleep(60000);
                }
            } while (queueUrl.Length == 0);

            if (queueUrl.Length > 0) {
                #region call AMAZON

                var reportMaxQueue = new RAInfoService(ConnectionString.ConnectionStringDB);

                DateTime emailSendDateTime = DateTime.MinValue;
                var reportQueues = new ReportQueueAWSService(ConnectionString.ConnectionStringDB);

                var ec2 = new AmazonEC2();
                string instanceID = "";
                int retry = 0;
                do {
                    try {
                        instanceID = ec2.GetEC2ID();
                        retry = 3;
                    }
                    catch (Exception e) {
                        Thread.Sleep(AmazonError.GetTimeoutDuration(retry));
                        retry++;
                        if (retry == 3) {
                            return;
                        }
                    }
                } while (retry < 3);

                if (string.IsNullOrEmpty(instanceID)) {
                    return;
                }

                int checkStatusCount = 0;

                //loop every one mins to SQS to check new message.
                while (true) {
                    try {
                        bool isReportGenerating = reportQueues.CheckOtherQueuesFor(instanceID);

                        if (!isReportGenerating) {
                            MessageView view = sqs.ReadMessage(queueUrl);

                            if (!string.IsNullOrEmpty(view.Body)) {
                                string[] contents = view.Body.Split('|');
                                string type = contents[0].Trim();

                                int maxQueue;

                                //We only have QT & DPA orders
                                if (type.ToUpper().Equals("QT")) {
                                    maxQueue = reportMaxQueue.GetMaxQueueFor(ScheduleQueueConstants.MaxQT);

                                    if (reportQueues.GetCurrentCountFor((int) Report.Types.QT, instanceID) < maxQueue) {
                                        reportQueues.InsertNewQueueFor(view.Body, (int) Report.Types.QT, instanceID);

                                        sqs.DeleteMessage(queueUrl, view.ReceiptHandle);
                                    }
                                }
                                else if (type.ToUpper().Equals("DPA")) {
                                    maxQueue = reportMaxQueue.GetMaxQueueFor(ScheduleQueueConstants.MaxDPA);

                                    if (reportQueues.GetCurrentCountFor((int) Report.Types.DPA, instanceID) < maxQueue) {
                                        reportQueues.InsertNewQueueFor(view.Body, (int) Report.Types.DPA, instanceID);

                                        sqs.DeleteMessage(queueUrl, view.ReceiptHandle);
                                    }
                                }
                                else if (type.ToUpper().Equals("GLACIER")) {
                                    var maxGlacierLoad = 1;
                                    if (reportQueues.GetCurrentCountFor((int)Report.Types.GlacierLoad, instanceID) < maxGlacierLoad) {
                                        reportQueues.InsertNewQueueFor(view.Body, (int) Report.Types.GlacierLoad, instanceID);
                                        sqs.DeleteMessage(queueUrl, view.ReceiptHandle);
                                    }
                                }
                                else {
                                    sqs.DeleteMessage(queueUrl, view.ReceiptHandle);
                                }
                            }
                        }

                        if (checkStatusCount == 6) {
                            bool inQueue = reportQueues.CheckOtherQueuesFor(instanceID);
                            if (!inQueue) {
                                //Smart EC2 Logic. Check how long EC2 has been running.

                                DateTime launchTime = ec2.GetLaunchTime(instanceID);
                                TimeSpan runningTime = DateTime.Now - launchTime;
                                if (ConnectionString.EC2TerminateAllowTime - (runningTime.TotalMinutes % 60) <= 0) {
                                    //Shut down the EC2 instance if no reports
                                    IAmazonSNS amazonSns = new AmazonSNS();
                                    string subject = ReportGeneratorStatus.StatusMessage.TerminateEC2.ToString();
                                    string messasge = ReportGeneratorInfo.GetReportGeneratorInfo(instanceID, launchTime, runningTime);
                                    amazonSns.SendToTopic(subject, messasge, ConnectionString.SNSProdTriggerReportARN);
                                    break;
                                }
                            }
                            checkStatusCount = 0;
                        }
                        else {
                            checkStatusCount++;
                        }
                    }
                    catch (Exception ex) {

                        TimeSpan emailSpan = DateTime.Now - emailSendDateTime;
                        if (emailSpan.TotalMinutes > 30) {
                            AmazonError.WriteLog(ex, "Amazon.cs: while loop",
                                ConnectionString.AdvisorEmail,
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
                                ConnectionString.MailGunSendAPIKey,
                                ConnectionString.MailGunSendDomain);
                            emailSendDateTime = DateTime.Now;
                        }
                    }
                    finally {
                        Thread.Sleep(60000); //Per Khody's request, change it to every 10 seconds. 
                        //need to increase the interval to 1 min, if user submits more than 4 reports at the same time
                        //all the reports will be assigned to that instance
                    }
                }

                #endregion
            }
        }
    }
}