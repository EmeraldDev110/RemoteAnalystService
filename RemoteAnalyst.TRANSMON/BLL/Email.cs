﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using log4net;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.TransMon.TransMonFactoryPattern.Model;

namespace RemoteAnalyst.TransMon.BLL {
    public class Email {
        private static readonly ILog Log = LogManager.GetLogger("TransMonLog");
        private readonly string _advisorEmail = ConnectionString.AdvisorEmail;
        private readonly string _supportEmail = ConnectionString.SupportEmail;
        private readonly int _emailPort = ConnectionString.EmailPort;
        private readonly string _emailServer = ConnectionString.EmailServer;
        private readonly string _emailUser = ConnectionString.EmailUser;
        private readonly string _emailPassword = ConnectionString.EmailPassword;
        private readonly SmtpClient _emailClient;
        private MailMessage _emailMessage;
        private readonly NetworkCredential credential;

        public Email() {
            //Setup Email Server
            System.Net.ServicePointManager.SecurityProtocol = (SecurityProtocolType)(0xc0 | 0x300 | 0xc00);
            _emailClient = new SmtpClient();
            _emailClient.EnableSsl = ConnectionString.EmailIsSSL;
            _emailClient.Host = _emailServer;
            _emailClient.Port = _emailPort;
            _emailClient.UseDefaultCredentials = false;
            //Set up credentials
            credential = new NetworkCredential();
            credential.UserName = _emailUser;
            credential.Password = _emailPassword;
            _emailClient.Credentials = credential;
        }

        public void CreateSendErrorEmail(string emailText, string path) {
            //Force all datetime to be in US format.
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            string emailtext = string.Empty;
            MailAddress fromEmail = new MailAddress(_advisorEmail);
            MailAddress toEmail = new MailAddress(_supportEmail);
            _emailMessage = new MailMessage(fromEmail, toEmail);
            _emailMessage.IsBodyHtml = true;
            _emailMessage.Subject = "Remote Analyst UWS Delay/Failure";

            _emailMessage.To.Add(toEmail);
            _emailMessage.Body = "The following Message has been generated by RATransMon :<br><br>" + emailText;
            try {
                _emailClient.Send(_emailMessage);
            }
            catch (Exception ex) {
                Log.ErrorFormat("CreateSendErrorEmail1 error {0} ", ex);
            }
        }

        public void SendFileCountEmail(TransmonView transmonView, DateTime startTime, DateTime stopTime, DataTable inProgressDataTable) {
            //Force all datetime to be in US format.
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            var emailtext = new StringBuilder();
            MailAddress fromEmail = new MailAddress(_advisorEmail);
            MailAddress toEmail = new MailAddress(_supportEmail);
            _emailMessage = new MailMessage(fromEmail, toEmail);
            _emailMessage.IsBodyHtml = true;
            _emailMessage.Subject = "RA TransMon: Missing loads";

            emailtext.Append("<table>" +
                               "<tr>" +
                               "<td>Company Name:</td>" +
                               "<td>" + transmonView.CompanyName + "</td>" +
                               "</tr>" +
                               "<tr>" +
                               "<td>System:</td>" +
                               "<td>" + transmonView.SystemName + " (" + transmonView.SystemSerial + ")</td>" +
                               "</tr>" +
                               "<tr>" +
                               "<td>From:</td>" +
                               "<td>" + startTime + "</td>" +
                               "</tr>" +
                               "<tr>" +
                               "<td>To:</td>" +
                               "<td>" + stopTime + "</td>" +
                               "</tr>" +
                               "<tr>" +
                               "<td>Expected By:</td>" +
                               "<td>" + DateTime.Now + "</td>" +
                               "</tr>" +
                               "</table>");

            if (inProgressDataTable.Rows.Count > 0) {
                emailtext.Append("<br />");
                emailtext.Append("<table style='border: 1px solid black;' cellpadding=5 cellspacing=0>" +
                                 "<tr>" +
                                 "<td style='border: 1px solid black;text-align:center;background-color:#e6e6e6'><b>File Name</b></td>" +
                                 "<td style='border: 1px solid black;text-align:center;background-color:#e6e6e6'><b>FTP Received Time</b></td>" +
                                 "<td style='border: 1px solid black;text-align:center;background-color:#e6e6e6'><b>S3 Sent Time</b></td>" +
                                 "<td style='border: 1px solid black;text-align:center;background-color:#e6e6e6'><b>Start Load Time</b></td>" +
                                 "<td style='border: 1px solid black;text-align:center;background-color:#e6e6e6'><b>Loaded Time</b></td>" +
                                 "</tr>");
                foreach (DataRow row in inProgressDataTable.Rows) {
                    var ftpReceivedTime = row.IsNull("FTPReceivedTime") ? "" : Convert.ToDateTime(row["FTPReceivedTime"]).ToString();
                    var s3SentTime = row.IsNull("S3SentTime") ? "" : Convert.ToDateTime(row["S3SentTime"]).ToString();
                    var uploadedtime = row.IsNull("StartLoadTime") ? "" : Convert.ToDateTime(row["StartLoadTime"]).ToString();
                    var loadedTime = row.IsNull("loadedtime") ? "" : Convert.ToDateTime(row["loadedtime"]).ToString();

                    emailtext.Append("<tr>" +
                                     "<td style='border: 1px solid black;'>" + row["filename"] + "</td>" +
                                     "<td style='border: 1px solid black;text-align:right'>" + ftpReceivedTime + "</td>" +
                                     "<td style='border: 1px solid black;text-align:right'>" + s3SentTime + "</td>" +
                                     "<td style='border: 1px solid black;text-align:right'>" + uploadedtime + "</td>" +
                                     "<td style='border: 1px solid black;text-align:right'>" + loadedTime + "</td>" +
                                     "</tr>");
                }
                emailtext.Append("</table>");
            }

            _emailMessage.Priority = MailPriority.High;
            _emailMessage.Body = emailtext.ToString();
            try {
                _emailClient.Send(_emailMessage);
            }
            catch (Exception ex) {
                Log.ErrorFormat("SendFileCountEmail error {0}", ex);
            }
        }

        public void SendHourSummary(List<TransmonView> transmonView, List<DriveView> driveView, DataTable inProgressDataTable, DateTime summaryStartTime, DateTime summaryStopTime) {
            //Force all datetime to be in US format.
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            var emailtext = new StringBuilder();
            var isFlag = false;

            //if (driveView.Count > 0) {
            //    emailtext.Append("<b>Disk Space:</b><br />");
            //    emailtext.Append("<table style='border: 1px solid black;' cellpadding=5 cellspacing=0>" +
            //                     "<tr>" +
            //                     "<td style='border: 1px solid black;text-align:center;background-color:#e6e6e6'><b>Drive Name</b></td>" +
            //                     "<td style='border: 1px solid black;text-align:center;background-color:#e6e6e6'><b>Free Space (GB)</b></td>" +
            //                     "<td style='border: 1px solid black;text-align:center;background-color:#e6e6e6'><b>Total Size (GB)</b></td>" +
            //                     "</tr>");
            //    foreach (var view in driveView) {
            //        emailtext.Append("<tr>" +
            //                         "<td style='border: 1px solid black;'>" + view.VolumeLabel + "</td>" +
            //                         "<td style='border: 1px solid black;text-align:right'>" + view.TotalFreeSpace.ToString("#,##0.00") + "</td>" +
            //                         "<td style='border: 1px solid black;text-align:right'>" + view.TotalSize.ToString("#,##0.00") + "</td>" +
            //                         "</tr>");
            //    }
            //    emailtext.Append("</table><br>");
            //}


			// Summary table section
            var totalSize = 0d;
			var statusTitle = "[OK]";

			var summaryTable = new StringBuilder();

            summaryTable.Append("<table style='border: 1px solid black;' cellpadding=5 cellspacing=0>" +
                             "<tr>" +
                             "<td style='border: 1px solid black;text-align:center;background-color:#e6e6e6'><b>Company Name</b></td>" +
                             "<td style='border: 1px solid black;text-align:center;background-color:#e6e6e6'><b>System Name</b></td>" +
                             "<td style='border: 1px solid black;text-align:center;background-color:#e6e6e6'><b>Files Expected</b></td>" +
                             "<td style='border: 1px solid black;text-align:center;background-color:#e6e6e6'><b>Files Loaded</b></td>" +
                             "<td style='border: 1px solid black;text-align:center;background-color:#e6e6e6'><b>Files In Progress</b></td>" +
                             "<td style='border: 1px solid black;text-align:center;background-color:#e6e6e6'><b>File Size (MB)</b></td>" +
                             "</tr>");

			foreach (var view in transmonView) {
                var totalFileSize = view.TotalFileSize ?? 0;
				totalSize += totalFileSize;

				if (view.LoadedFileCount < view.ExpectedFileCount) {
					// change status title only if it's not error
					if (statusTitle != "[Error]")
						statusTitle = "[Warning]";
					isFlag = true;
                    if (view.InProgressFileCount > 0) {
                        summaryTable.Append("<tr style='background-color:yellow;'>" +
                                     "<td style='border: 1px solid black;'>" + view.CompanyName + "</td>" +
                                     "<td style='border: 1px solid black;'>" + view.SystemName + "</td>" +
                                     "<td style='border: 1px solid black;text-align:right;'>" + view.ExpectedFileCount + "</td>" +
                                     "<td style='border: 1px solid black;text-align:right;'>" + view.LoadedFileCount + "</td>" +
                                     "<td style='border: 1px solid black;text-align:right;'>" + view.InProgressFileCount + "</td>" +
                                     "<td style='border: 1px solid black;text-align:right;'>" + Math.Round(totalFileSize / Convert.ToDouble(1024 * 1024), 2).ToString("#,##0.00") + "</td>" +
                                     "</tr>");
                    } else {
                        if (statusTitle != "[Error]")
                            statusTitle = "[Error]";
                        summaryTable.Append("<tr style='background-color:red;'>" +
                                        "<td style='border: 1px solid black;'>" + view.CompanyName + "</td>" +
                                        "<td style='border: 1px solid black;'>" + view.SystemName + "</td>" +
                                        "<td style='border: 1px solid black;text-align:right;'>" + view.ExpectedFileCount + "</td>" +
                                        "<td style='border: 1px solid black;text-align:right;'>" + view.LoadedFileCount + "</td>" +
                                        "<td style='border: 1px solid black;text-align:right;'>" + view.InProgressFileCount + "</td>" +
                                        "<td style='border: 1px solid black;text-align:right;'>" + Math.Round(totalFileSize / Convert.ToDouble(1024 * 1024), 2).ToString("#,##0.00") + "</td>" +
                                        "</tr>");                        
                    }
				}

                //if (view.InProgressFileCount > 0) {
                //    if (view.LoadedFileCount + view.InProgressFileCount < view.ExpectedFileCount + view.ResidualFromLastInterval) {
                //        statusTitle = "[Error]";
                //        isFlag = true;
                //        summaryTable.Append("<tr style='background-color:red;'>" +
                //                         "<td style='border: 1px solid black;'>" + view.CompanyName + "</td>" +
                //                         "<td style='border: 1px solid black;'>" + view.SystemName + "</td>" +
                //                         "<td style='border: 1px solid black;text-align:right;'>" + view.ExpectedFileCount + "</td>" +
                //                         "<td style='border: 1px solid black;text-align:right;'>" + view.LoadedFileCount + "</td>" +
                //                         "<td style='border: 1px solid black;text-align:right;'>" + view.InProgressFileCount + "</td>" +
                //                         "<td style='border: 1px solid black;text-align:right;'>" + Math.Round(totalFileSize / Convert.ToDouble(1024 * 1024), 2).ToString("#,##0.00") + "</td>" +
                //                         "</tr>");
                //    } else {
                //        // change status title only when it's not error
                //        if (statusTitle != "[Error]")
                //            statusTitle = "[Warning]";
                //        isFlag = true;
                //        summaryTable.Append("<tr style='background-color:yellow;'>" +
                //                         "<td style='border: 1px solid black;'>" + view.CompanyName + "</td>" +
                //                         "<td style='border: 1px solid black;'>" + view.SystemName + "</td>" +
                //                         "<td style='border: 1px solid black;text-align:right;'>" + view.ExpectedFileCount + "</td>" +
                //                         "<td style='border: 1px solid black;text-align:right;'>" + view.LoadedFileCount + "</td>" +
                //                         "<td style='border: 1px solid black;text-align:right;'>" + view.InProgressFileCount + "</td>" +
                //                         "<td style='border: 1px solid black;text-align:right;'>" + Math.Round(totalFileSize / Convert.ToDouble(1024 * 1024), 2).ToString("#,##0.00") + "</td>" +
                //                         "</tr>");
                //    }
                //} else if (view.ExpectedFileCount != view.LoadedFileCount + view.ResidualFromLastInterval) {
                //    // change status title only if it's not error
                //    if (statusTitle != "[Error]")
                //        statusTitle = "[Warning]";
                //    isFlag = true;
                //    summaryTable.Append("<tr style='background-color:yellow;'>" +
                //                     "<td style='border: 1px solid black;'>" + view.CompanyName + "</td>" +
                //                     "<td style='border: 1px solid black;'>" + view.SystemName + "</td>" +
                //                     "<td style='border: 1px solid black;text-align:right;'>" + view.ExpectedFileCount + "</td>" +
                //                     "<td style='border: 1px solid black;text-align:right;'>" + view.LoadedFileCount + "</td>" +
                //                     "<td style='border: 1px solid black;text-align:right;'>" + view.InProgressFileCount + "</td>" +
                //                     "<td style='border: 1px solid black;text-align:right;'>" + Math.Round(totalFileSize / Convert.ToDouble(1024 * 1024), 2).ToString("#,##0.00") + "</td>" +
                //                     "</tr>");
                //}
            }

            //Summary
            summaryTable.Append("<tr>" +
                             "<td style='border: 1px solid black;text-align:right' colspan='2'>Total</td>" +
							 "<td style='border: 1px solid black;text-align:right'>" + transmonView.Sum(x => x.ExpectedFileCount) + "</td>" +
							 "<td style='border: 1px solid black;text-align:right'>" + transmonView.Sum(x => x.LoadedFileCount) + "</td>" +
							 "<td style='border: 1px solid black;text-align:right'>" + transmonView.Sum(x => x.InProgressFileCount) + "</td>" +
							 "<td style='border: 1px solid black;text-align:right'>" + Math.Round(totalSize / Convert.ToDouble(1024 * 1024), 2).ToString("#,##0.00") + "</td>" +
                             "</tr>");
            summaryTable.Append("</table>");

			// Append summary table if there's something to show
			// otherwise Show All OK.
			if(statusTitle != "[OK]") {
				emailtext.Append("<b>Needs Attention:</b><br />");
			} else {
				emailtext.Append("<b>All OK.</b><br /><b>Summary:</b><br />");
			}
			emailtext.Append(summaryTable);
            emailtext.Append("<b>For red colored systems, please investigate. If the expected-file-count has changed or the system is not expected to send files, please revise the system in Transmon's check list.</b><br />");

            if (inProgressDataTable.Rows.Count > 0) {
                emailtext.Append("<br />");
                emailtext.Append("<b>In Progress:</b><br />");
                emailtext.Append("<table style='border: 1px solid black;' cellpadding=5 cellspacing=0>" +
                                 "<tr>" +
                                 "<td style='border: 1px solid black;text-align:center;background-color:#e6e6e6'><b>Company Name</b></td>" +
                                 "<td style='border: 1px solid black;text-align:center;background-color:#e6e6e6'><b>System Name</b></td>" +
                                 "<td style='border: 1px solid black;text-align:center;background-color:#e6e6e6'><b>File Name</b></td>" +
                                 "<td style='border: 1px solid black;text-align:center;background-color:#e6e6e6'><b>FTP Received Time</b></td>" +
                                 //"<td style='border: 1px solid black;text-align:center;background-color:#e6e6e6'><b>S3 Sent Time</b></td>" +
                                 //"<td style='border: 1px solid black;text-align:center;background-color:#e6e6e6'><b>Start Load Time</b></td>" +
                                 //"<td style='border: 1px solid black;text-align:center;background-color:#e6e6e6'><b>Loaded Time</b></td>" +
                                 "</tr>");
                foreach (DataRow row in inProgressDataTable.Rows) {
                    var tempCompanyName = transmonView.Where(x => x.SystemSerial == row["SystemSerial"].ToString()).Select(x => x.CompanyName);
                    var tempSystemName = transmonView.Where(x => x.SystemSerial == row["SystemSerial"].ToString()).Select(x => x.SystemName);
                    var ftpReceivedTime = Convert.ToDateTime(row["FTPReceivedTime"]);
                    var s3SentTime = Convert.ToDateTime(row["S3SentTime"]);
                    var uploadedtime = row.IsNull("StartLoadTime") ? "" : Convert.ToDateTime(row["StartLoadTime"]).ToString();
                    var loadedTime = row.IsNull("loadedtime") ? "" : Convert.ToDateTime(row["loadedtime"]).ToString();

                    emailtext.Append("<tr>" +
                                     "<td style='border: 1px solid black;'>" + tempCompanyName.FirstOrDefault() + "</td>" +
                                     "<td style='border: 1px solid black;'>" + tempSystemName.FirstOrDefault() + "</td>" +
                                     "<td style='border: 1px solid black;'>" + row["filename"] + "</td>" +
                                     "<td style='border: 1px solid black;text-align:right'>" + ftpReceivedTime + "</td>" +
                                     //"<td style='border: 1px solid black;text-align:right'>" + s3SentTime + "</td>" +
                                     //"<td style='border: 1px solid black;text-align:right'>" + uploadedtime + "</td>" +
                                     //"<td style='border: 1px solid black;text-align:right'>" + loadedTime + "</td>" +
                                     "</tr>");
                }
                emailtext.Append("</table>");
            }

            //Email Server
            MailAddress fromEmail = new MailAddress(_advisorEmail);
            MailAddress toEmail = new MailAddress(_supportEmail);
            _emailMessage = new MailMessage(fromEmail, toEmail);
            _emailMessage.IsBodyHtml = true;
            _emailMessage.Subject = $@"RA TransMon: Hourly Summary for hour {summaryStartTime.ToString("HH:mm")} - {summaryStopTime.ToString("HH:mm")} {statusTitle} ";

            if (isFlag)
                _emailMessage.Priority = MailPriority.High;
            _emailMessage.To.Add(toEmail);
          
            _emailMessage.Body = emailtext.ToString();
            try {
                _emailClient.Send(_emailMessage);
            }
            catch (Exception ex) {
                Log.ErrorFormat("SendFileCountEmail error {0}", ex);
            }
        }

        public void SendFTPStorageOver70Percent(List<DriveView> driveView) {
            //Force all datetime to be in US format.
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            var emailtext = new StringBuilder();

            if (driveView.Count > 0) {
                emailtext.Append("<br />");
                emailtext.Append("<table style='border: 1px solid black;' cellpadding=5 cellspacing=0>" +
                                 "<tr>" +
                                 "<td style='border: 1px solid black;text-align:center;background-color:#e6e6e6'><b>Drive Name</b></td>" +
                                 "<td style='border: 1px solid black;text-align:center;background-color:#e6e6e6'><b>Free Space (GB)</b></td>" +
                                 "<td style='border: 1px solid black;text-align:center;background-color:#e6e6e6'><b>Total Size (GB)</b></td>" +
                                 "</tr>");
                foreach (var view in driveView) {
                    if (view.PercentUsed > 70) {
                        emailtext.Append("<tr>" +
                                         "<td style='border: 1px solid black;background-color:red''>" + view.VolumeLabel + "</td>" +
                                         "<td style='border: 1px solid black;text-align:right;background-color:red''>" + view.TotalFreeSpace.ToString("#,##0.00") + "</td>" +
                                         "<td style='border: 1px solid black;text-align:right;background-color:red''>" + view.TotalSize.ToString("#,##0.00") + "</td>" +
                                         "</tr>");
                    }
                    else {
                        emailtext.Append("<tr>" +
                                         "<td style='border: 1px solid black;'>" + view.VolumeLabel + "</td>" +
                                         "<td style='border: 1px solid black;text-align:right'>" + view.TotalFreeSpace.ToString("#,##0.00") + "</td>" +
                                         "<td style='border: 1px solid black;text-align:right'>" + view.TotalSize.ToString("#,##0.00") + "</td>" +
                                         "</tr>");
                    }
                }
                emailtext.Append("</table>");
            }

            //Email Server
            MailAddress fromEmail = new MailAddress(_advisorEmail);
            MailAddress toEmail = new MailAddress(_supportEmail);
            _emailMessage = new MailMessage(fromEmail, toEmail);
            _emailMessage.IsBodyHtml = true;
            _emailMessage.Subject = "RA TransMon: FTP Storage Over 70 %";
            _emailMessage.Priority = MailPriority.High;
            _emailMessage.To.Add(toEmail);
            _emailMessage.Body = emailtext.ToString();
            try {
                _emailClient.Send(_emailMessage);
            }
            catch (Exception ex) {
                Log.ErrorFormat("SendFTPStorageOver70Percent error {0}", ex);
            }

        }
        public void SendIncompletedLoadEmail(TransmonView transmonView, DateTime startTime, DateTime stopTime, DataTable loadingInfoDataTable) {
            //Force all datetime to be in US format.
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            
            var emailtext = new StringBuilder();

            emailtext.Append("<table>" +
                            "<tr>" +
                            "<td>Company Name:</td>" +
                            "<td>" + transmonView.CompanyName + "</td>" +
                            "</tr>" +
                            "<tr>" +
                            "<td>System:</td>" +
                            "<td>" + transmonView.SystemName + " (" + transmonView.SystemSerial + ")</td>" +
                            "</tr>" +
                            "<tr>" +
                            "<td>From:</td>" +
                            "<td>" + startTime + "</td>" +
                            "</tr>" +
                            "<tr>" +
                            "<td>To:</td>" +
                            "<td>" + stopTime + "</td>" +
                            "</tr>" +
                            "<tr>" +
                            "<td>Expected By:</td>" +
                            "<td>" + DateTime.Now + "</td>" +
                            "</tr>" +
                            "</table>");


            if (loadingInfoDataTable.Rows.Count > 0) {
                emailtext.Append("<br />");
                emailtext.Append("<table style='border: 1px solid black;' cellpadding=5 cellspacing=0>" +
                                 "<tr>" +
                                 "<td style='border: 1px solid black;text-align:center;background-color:#e6e6e6'><b>File Name</b></td>" +
                                 "<td style='border: 1px solid black;text-align:center;background-color:#e6e6e6'><b>Received at FTP</b></td>" +
                                 "<td style='border: 1px solid black;text-align:center;background-color:#e6e6e6'><b>Sent to S3</b></td>" +
                                 "<td style='border: 1px solid black;text-align:center;background-color:#e6e6e6'><b>Load Started</b></td>" +
                                 "</tr>");
                foreach (DataRow row in loadingInfoDataTable.Rows) {
                    var ftpReceivedTime = row.IsNull("FTPReceivedTime") ? "" : row["FTPReceivedTime"].ToString();
                    var s3SentTime = row.IsNull("S3SentTime") ? "" : row["S3SentTime"].ToString();
                    var loadedtime = row.IsNull("loadedtime") ? "" : row["loadedtime"].ToString();

                    emailtext.Append("<tr>" +
                                     "<td style='border: 1px solid black;'>" + row["filename"] + "</td>" +
                                     "<td style='border: 1px solid black;text-align:right'>" + ftpReceivedTime + "</td>" +
                                     "<td style='border: 1px solid black;text-align:right'>" + s3SentTime + "</td>" +
                                     "<td style='border: 1px solid black;text-align:right'>" + loadedtime + "</td>" +
                                     "</tr>");
                }
                emailtext.Append("</table>");
            }

            //Email Server
            MailAddress fromEmail = new MailAddress(_advisorEmail);
            MailAddress toEmail = new MailAddress(_supportEmail);
            _emailMessage = new MailMessage(fromEmail, toEmail);
            _emailMessage.IsBodyHtml = true;
            _emailMessage.Subject = "RA TransMon: Late loads";
            _emailMessage.Priority = MailPriority.High;
            _emailMessage.To.Add(_supportEmail);
            _emailMessage.Body = emailtext.ToString();
            try {
                _emailClient.Send(_emailMessage);
            }
            catch (Exception ex) {
                Log.ErrorFormat("SendIncompletedLoadEmail error {0}", ex);
            }
        }
    }
}