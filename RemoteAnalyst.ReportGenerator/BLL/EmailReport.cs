using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using DPA_QT_Shared_Classes.Shared_Clasess;
using RemoteAnalyst.AWS.Queue;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.Util;
using System.Net.Mail;
using System.Net;
using System.Net.Mime;
using RemoteAnalyst.BusinessLogic.Email;

namespace RemoteAnalyst.ReportGenerator.BLL
{
    /// <summary>
    /// Email reports
    /// </summary>
    internal class EmailReport
    {
        private readonly EmailManager _emailManager;

        public EmailReport() {

            _emailManager = new EmailManager(ConnectionString.EmailServer
                                , ConnectionString.ServerPath
                                , ConnectionString.EmailPort
                                , ConnectionString.EmailUser
                                , ConnectionString.EmailPassword
                                , ConnectionString.EmailAuthentication
                                , ConnectionString.SystemLocation
                                , ConnectionString.AdvisorEmail
                                , ConnectionString.SupportEmail
                                , ConnectionString.WebSite
                                , ConnectionString.EmailIsSSL
                                , ConnectionString.IsLocalAnalyst
                                , ConnectionString.MailGunSendAPIKey
                                , ConnectionString.MailGunSendDomain);
        }

        /// <summary>
        /// Build QT email body without Customer Name
        /// </summary>
        /// <param name="systemSerial">System Serial Number</param>
        /// <param name="systemName">System Name</param>
        /// <param name="startTime">Report Start Time</param>
        /// <param name="endTime">Report Stop Time</param>
        /// <returns>email content</returns>
        private string BuildQTReportBody(string systemSerial, string systemName, DateTime startTime, DateTime endTime)
        {
            var sb = new StringBuilder();

            sb.Append("<br>Dear Customer,<br><br>");
            sb.Append(
                "Your QT Analyses for the following collection are ready, and attached as a zip file to this email. <br><br>");
            sb.Append("System:<br>");
            sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;System #: " + systemSerial + "<br>");
            sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;System Name: " + systemName + "<br><br>");
            sb.Append("Collection: <br>");
            sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;From: " + startTime.ToString("yyyy-MM-dd HH:mm:ss") + "<br>");
            sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;To: " + endTime.ToString("yyyy-MM-dd HH:mm:ss") + "<br><br>");

            sb.Append("Write to <a href='" + ConnectionString.SupportEmail + "'>" + ConnectionString.SupportEmail + "</a> if assistance is needed.");

            return sb.ToString();
        }

        /// <summary>
        /// Build QT email body with Customer Name
        /// </summary>
        /// <param name="systemSerial">System Serial Number</param>
        /// <param name="systemName">System Name</param>
        /// <param name="startTime">Report Start Time</param>
        /// <param name="endTime">Report Stop Time</param>
        /// <param name="custName">Customer Name</param>
        /// <returns>email content</returns>
        private string BuildQTReportBody(string systemSerial, string systemName, DateTime startTime, DateTime endTime,
            string custName)
        {
            var sb = new StringBuilder();

            sb.Append("<br>Dear " + custName + ",<br><br>");
            sb.Append(
                "Your QT Analyses for the following collection are ready, and attached as a zip file to this email. <br><br>");
            sb.Append("System:<br>");
            sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;System #: " + systemSerial + "<br>");
            sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;System Name: " + systemName + "<br><br>");
            sb.Append("Collection: <br>");
            sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;From: " + startTime.ToString("yyyy-MM-dd HH:mm:ss") + "<br>");
            sb.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;To: " + endTime.ToString("yyyy-MM-dd HH:mm:ss") + "<br><br>");

            sb.Append("Write to <a href='" + ConnectionString.SupportEmail + "'>" + ConnectionString.SupportEmail + "</a> if assistance is needed.");

            return sb.ToString();
        }

		internal void SendQTReportError(List<string> emailList, string systemSerial, string systemName,
			DateTime startTime, DateTime endTime) {
			var custInfo = new CusAnalystService(ConnectionString.ConnectionStringDB);
            foreach (string toEmail in emailList) {
                string email_subject = "Error generating QT report on: " + systemName;

                string custName = custInfo.GetUserNameFor(toEmail);
				var sbEmail = new StringBuilder();
				var header = new EmailHeaderFooter();
                //contents.
                sbEmail.Append(header.EmailHeader(ConnectionString.IsLocalAnalyst));
				sbEmail.Append("Dear Customer,<br><br>Your QT report for the following collection was not generated since the data is unavailable.<br><br>" + "System #: " + systemSerial +
										"<br>System Name: " + systemName + "<br><br>Collection: No data available for the below time period<br>From: " + startTime.ToString("yyyy-MM-dd HH:mm:ss") + "<br>To: " + endTime.ToString("yyyy-MM-dd HH:mm:ss") +
										"<br>Write to " + ConnectionString.SupportEmail + " if assistance is needed.");
				sbEmail.Append(header.EmailFooter(ConnectionString.SupportEmail, ConnectionString.WebSite));
				string email_body = sbEmail.ToString();

				try {
                    _emailManager.SendEmail(toEmail, email_subject, email_body, ConnectionString.SupportEmail);
                }
				catch (Exception ex) {
					if (!string.IsNullOrEmpty(ConnectionString.SQSError)) {
						var sqs = new AmazonSQS();
						string urlQueue = sqs.GetAmazonSQSUrl(ConnectionString.SQSError);
						sqs.WriteMessage(urlQueue, "SendQTReportError: " + ex.Message);
					}
				}
			}

		}

		/// <summary>
		/// Send QT report to customer
		/// </summary>
		/// <param name="location">Excel save location</param>
		/// <param name="emailList">List of Emails</param>
		/// <param name="systemSerial">System Serial Number</param>
		/// <param name="systemName">System Name</param>
		/// <param name="startTime">Report Start Time</param>
		/// <param name="endTime">Report Stop Time</param>
		internal void SendQTReportEmail(string location, List<string> emailList, string systemSerial, string systemName,
            DateTime startTime, DateTime endTime)
        {
            var custInfo = new CusAnalystService(ConnectionString.ConnectionStringDB);
            foreach (string toEmail in emailList)
            {
                var productName = LicenseService.GetProductName(ConnectionString.ConnectionStringDB);
                if (ConnectionString.IsLocalAnalyst) {
                    productName = "Local Analyst";
                }
                string email_subject = productName + " QT Reports";
                string custName = custInfo.GetUserNameFor(toEmail);
                var sbEmail = new StringBuilder();
                var header = new EmailHeaderFooter();

                //contents.
                sbEmail.Append(header.EmailHeader(ConnectionString.IsLocalAnalyst));
                
                if (custName.Length > 0)
                    sbEmail.Append(BuildQTReportBody(systemSerial, systemName, startTime, endTime, custName));
                else
                    sbEmail.Append(BuildQTReportBody(systemSerial, systemName, startTime, endTime));

                sbEmail.Append(header.EmailFooter(ConnectionString.SupportEmail, ConnectionString.WebSite));
                string email_body = sbEmail.ToString();

                string[] attachments = { location };
                try
                {
                    _emailManager.SendEmail(toEmail, email_subject, email_body, ConnectionString.SupportEmail, attachments);
                }
                catch (Exception ex)
                {
                    if (!string.IsNullOrEmpty(ConnectionString.SQSError)) {
                        var sqs = new AmazonSQS();
                        string urlQueue = sqs.GetAmazonSQSUrl(ConnectionString.SQSError);
                        sqs.WriteMessage(urlQueue, "SendQTReportEmail: " + ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Send QT notification email to customer
        /// </summary>
        /// <param name="emailList">List of Emails</param>
        /// <param name="systemSerial">System Serial Number</param>
        /// <param name="systemName">System Name</param>
        /// <param name="startTime">Report Start Time</param>
        /// <param name="endTime">Report Stop Time</param>
        /// <param name="downloadKey">Excel Download Key</param>
        internal void SendQTReportNotification(List<string> emailList, string systemSerial, string systemName,
            DateTime startTime, DateTime endTime, int downloadKey, bool attachmentInEmail)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            var header = new EmailHeaderFooter();
            var custInfo = new CusAnalystService(ConnectionString.ConnectionStringDB);

            foreach (string toEmail in emailList)
            {
                string custName = custInfo.GetUserNameFor(toEmail);
                string email_body = header.EmailHeader(ConnectionString.IsLocalAnalyst);

                //Build Email body.
                if (custName.Length > 0)
                {
                    email_body += "Dear " + custName + ",<br><br>";
                    email_body +=
                        "Your QT Analyses for the following collection are ready.";
                    email_body += attachmentInEmail ? " However it is too large to email you as an attachment." : "";
                    email_body += 
                        "<br><br>You can download the QT Analyses from <a href='" + ConnectionString.WebSite + "'>" + ConnectionString.WebSite + "</a><br><br>";
                    email_body += "System:<br>";
                    email_body += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Serial #: " + systemSerial + "<br>";
                    email_body += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Name:&nbsp;&nbsp;&nbsp; " + systemName + "<br><br>";
                    email_body += "Collection:<br>";
                    email_body += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;From: " + startTime + "<br>";
                    email_body += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;To:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + endTime +
                                   "<br><br>";

                    email_body += "Here are the instructions:<br><br>";
                    email_body += 
                        "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; a. Login into <a href='" + ConnectionString.WebSite + "'>" + ConnectionString.WebSite + "</a><br>";
                    email_body += 
                        "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; b. Click on the \"Generate -> Archives\" from the menu.<br>";
                    email_body += 
                        "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; c. Click on the Download Analysis icon corresponding to the Analyses. This will download the zipped report to a folder on your PC.<br><br>";
                    email_body += "Please let us know if you have any problems downloading the report.";
                }
                else
                {
                    var decrypt = new Decrypt();
                    string encyptKey = decrypt.strDESEncrypt(downloadKey.ToString());

                    email_body += "Dear Customer,<br><br>";
                    email_body +=
                        "Your QT Analyses for the following collection are ready.";
                    email_body += attachmentInEmail ? " However it is too large to email you as an attachment." : "";
                    email_body += 
                        "<br><br>You can download the QT Analyses from <a href='www.remoteanalyst.com/DataCenter/Download.aspx?Key=" +
                        encyptKey + "'>www.remoteanalyst.com/DataCenter/Download.aspx?Key=" + encyptKey + "</a><br><br>";
                    email_body += "System:<br>";
                    email_body += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Serial #: " + systemSerial + "<br>";
                    email_body += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Name:&nbsp;&nbsp;&nbsp; " + systemName + "<br><br>";
                    email_body += "Collection:<br>";
                    email_body += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;From: " + startTime + "<br>";
                    email_body += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;To:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + endTime +
                                   "<br><br>";

                    email_body += "Please let us know if you have any problems downloading the report.";
                }

                email_body += header.EmailFooter(ConnectionString.SupportEmail, ConnectionString.WebSite);

                var productName = LicenseService.GetProductName(ConnectionString.ConnectionStringDB);
                if (ConnectionString.IsLocalAnalyst) {
                    productName = "Local Analyst";
                }
                string email_subject = productName + " QT Reports";

                try {
                    _emailManager.SendEmail(toEmail, email_subject, email_body, ConnectionString.SupportEmail);
                }
                catch (Exception ex)
                {
                    if (!string.IsNullOrEmpty(ConnectionString.SQSError)) {
                        var sqs = new AmazonSQS();
                        string urlQueue = sqs.GetAmazonSQSUrl(ConnectionString.SQSError);
                        sqs.WriteMessage(urlQueue, "SendQTReportNotification: " + ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Build DPA email body without Customer Name
        /// </summary>
        /// <param name="systemName">System Name</param>
        /// <param name="startTime">Report Start Time</param>
        /// <param name="endTime">Report Stop Time</param>
        /// <param name="intervals">Intervals</param>
        /// <returns>email content</returns>
        private string BuildDPAReportBody(string systemName, DateTime startTime, DateTime endTime, int intervals)
        {
            var sb = new StringBuilder();

            //sb.Append("The attached zip file contains the following expert reports generated on " + systemName);
            //sb.Append(" for the period " + startTime + " through " + endTime + " (Interval " + intervals + " seconds): <br /> <br />");
            sb.Append("<br>Dear Customer,<br><br>");
            sb.Append("Attached here is your DPA Excel report for: <br>");
            sb.Append("System Name: " + systemName + "<br>");
            sb.Append("From: " + startTime.ToString("yyyy-MM-dd HH:mm:ss") + "<br>");
            sb.Append("To: " + endTime.ToString("yyyy-MM-dd HH:mm:ss") + "<br>");
            sb.Append("Interval: " + intervals + "");

            sb.Append("<br /><br />Please access the analysis by unzipping the attached file.");

            return sb.ToString();
        }

        /// <summary>
        /// Build DPA email body with Customer Name
        /// </summary>
        /// <param name="systemName">System Name</param>
        /// <param name="startTime">Report Start Time</param>
        /// <param name="endTime">Report Stop Time</param>
        /// <param name="intervals">Intervals</param>
        /// <param name="custName">Customer Name</param>
        /// <returns>email content</returns>
        private string BuildDPAReportBody(string systemName, DateTime startTime, DateTime endTime, int intervals,
            string custName)
        {
            var sb = new StringBuilder();

            //sb.Append("The attached zip file contains the following expert reports generated on " + systemName);
            //sb.Append(" for the period " + startTime + " through " + endTime + " (Interval " + intervals + " seconds): <br /> <br />");
            sb.Append("<br>Dear " + custName + ",<br><br>");
            sb.Append("Attached here is your DPA Excel report for: <br>");
            sb.Append("System Name: " + systemName + "<br>");
            sb.Append("From: " + startTime.ToString("yyyy-MM-dd HH:mm:ss") + "<br>");
            sb.Append("To: " + endTime.ToString("yyyy-MM-dd HH:mm:ss") + "<br>");
            sb.Append("Interval: " + intervals + "");

            sb.Append("<br /><br />Please access the analysis by unzipping the attached file.");

            return sb.ToString();
        }

		internal void SendDPAReportError(List<string> emailList, string systemSerial, string systemName,
			DateTime startTime, DateTime endTime) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            var header = new EmailHeaderFooter();
            var custInfo = new CusAnalystService(ConnectionString.ConnectionStringDB);

			foreach (string toEmail in emailList) {
                string email_body = header.EmailHeader(ConnectionString.IsLocalAnalyst);

                //contents.
                email_body += "Dear Customer,<br><br>Your DPA report for the following collection was not generated since the data is unavailable.<br><br>" + "System #: " + systemSerial +
										"<br>System Name: \\" + systemName + "<br><br>Collection: No data available for the below time period<br>From: " + startTime.ToString("yyyy-MM-dd HH:mm:ss") + "<br>To: " + endTime.ToString("yyyy-MM-dd HH:mm:ss") +
										"<br>Write to " + ConnectionString.SupportEmail + " if assistance is needed.";

                email_body += header.EmailFooter(ConnectionString.SupportEmail, ConnectionString.WebSite);

                string email_subject = "Error generating DPA report on: " + systemName;

                try {
                    _emailManager.SendEmail(toEmail, email_subject, email_body, ConnectionString.SupportEmail);
                }
				catch (Exception ex) {
					if (!string.IsNullOrEmpty(ConnectionString.SQSError)) {
						var sqs = new AmazonSQS();
						string urlQueue = sqs.GetAmazonSQSUrl(ConnectionString.SQSError);
						sqs.WriteMessage(urlQueue, "SendDPAReportError: " + ex.Message);
					}
				}
			}

		}

		/// <summary>
		/// Send DPA report to customer
		/// </summary>
		/// <param name="location">Excel save location</param>
		/// <param name="emailList">List of Emails</param>
		/// <param name="systemName">System Name</param>
		/// <param name="startTime">Report Start Time</param>
		/// <param name="endTime">Report Stop Time</param>
		/// <param name="intervals">Intervals</param>
		public void SendDPAReportEmail(string location, List<string> emailList, string systemName, DateTime startTime,
            DateTime endTime, int intervals)
        {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            var header = new EmailHeaderFooter();
            var custInfo = new CusAnalystService(ConnectionString.ConnectionStringDB);

            foreach (string toEmail in emailList)
            {
                string custName = custInfo.GetUserNameFor(toEmail);
            
                string email_body = header.EmailHeader(ConnectionString.IsLocalAnalyst);

                //contents.
                if (custName.Length > 0)
                {
                    email_body += BuildDPAReportBody(systemName, startTime, endTime, intervals, custName);
                }
                else
                {
                    email_body += BuildDPAReportBody(systemName, startTime, endTime, intervals);
                }
                email_body += header.EmailFooter(ConnectionString.SupportEmail, ConnectionString.WebSite);

                var productName = LicenseService.GetProductName(ConnectionString.ConnectionStringDB);
                if (ConnectionString.IsLocalAnalyst) {
                    productName = "Local Analyst";
                }
                string email_subject = productName + " DPA Reports";
                string[] attachments = { location };
                try
                {
                    _emailManager.SendEmail(toEmail, email_subject, email_body, ConnectionString.SupportEmail, attachments);
                }
                catch (Exception ex)
                {
                    if (!string.IsNullOrEmpty(ConnectionString.SQSError)) {
                        var sqs = new AmazonSQS();
                        string urlQueue = sqs.GetAmazonSQSUrl(ConnectionString.SQSError);
                        sqs.WriteMessage(urlQueue, "SendDPAReportEmail: " + ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Send DPA Notification to customer
        /// </summary>
        /// <param name="emailList">List of Emails</param>
        /// <param name="systemSerial">System Serial Number</param>
        /// <param name="systemName">System Name</param>
        /// <param name="startTime">Report Start Time</param>
        /// <param name="endTime">Report Stop Time</param>
        /// <param name="downloadKey">Excel Download Key</param>
        public void SendDPAReportNotification(List<string> emailList, string systemSerial, string systemName,
            DateTime startTime, DateTime endTime, int downloadKey, bool attachmentInEmail)
        {
            var custInfo = new CusAnalystService(ConnectionString.ConnectionStringDB);
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            var header = new EmailHeaderFooter();

            foreach (string toEmail in emailList)
            {
                string custName = custInfo.GetUserNameFor(toEmail);
                
                string email_body = header.EmailHeader(ConnectionString.IsLocalAnalyst);
                
                //Build Email body.
                if (custName.Length > 0)
                {
                    email_body += "Dear " + custName + ",<br><br>";
                    email_body +=
                        "Your DPA Analyses for the following collection are ready.";
                    email_body += attachmentInEmail ? " However it is too large to email you as an attachment." : "";
                    email_body +=
                        "<br><br>You can download the DPA Analyses from <a href='" + ConnectionString.WebSite + "'>" + ConnectionString.WebSite + "</a><br><br>";
                    email_body += "System:<br>";
                    email_body += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Serial #: " + systemSerial + "<br>";
                    email_body += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Name:&nbsp;&nbsp;&nbsp; " + systemName + "<br><br>";
                    email_body += "Collection:<br>";
                    email_body += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;From: " + startTime.ToString("yyyy-MM-dd HH:mm:ss") + "<br>";
                    email_body += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;To:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + endTime.ToString("yyyy-MM-dd HH:mm:ss") +
                                   "<br><br>";

                    email_body += "Here are the instructions:<br><br>";
                    email_body += 
                        "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; a. Login into <a href='" + ConnectionString.WebSite + "'>" + ConnectionString.WebSite + "</a><br>";
                    email_body += 
                        "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; b. Click on the \"Generate -> Archives\" from the menu.<br>";
                    email_body += 
                        "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; c. Click on the Download Analysis icon corresponding to the Analyses. This will download the zipped report to a folder on your PC.<br><br>";
                    email_body += "Please let us know if you have any problems downloading the report.";
                }
                else
                {
                    var decrypt = new Decrypt();
                    string encyptKey = decrypt.strDESEncrypt(downloadKey.ToString());

                    email_body += "Dear Customer,<br><br>";
                    email_body +=
                        "Your DPA Analyses for the following collection are ready.";
                    email_body += attachmentInEmail ? " However it is too large to email you as an attachment." : "";
                    email_body += 
                        "<br><br>You can download the DPA Analyses from <a href='www.remoteanalyst.com/DataCenter/Download.aspx?Key=" +
                        encyptKey + "'>www.remoteanalyst.com/DataCenter/Download.aspx?Key=" + encyptKey + "</a><br><br>";
                    email_body += "System:<br>";
                    email_body += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Serial #: " + systemSerial + "<br>";
                    email_body += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Name:&nbsp;&nbsp;&nbsp; " + systemName + "<br><br>";
                    email_body += "Collection:<br>";
                    email_body += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;From: " + startTime + "<br>";
                    email_body += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;To:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + endTime +
                                   "<br><br>";

                    email_body += "Please let us know if you have any problems downloading the report.";
                }

                email_body += header.EmailFooter(ConnectionString.SupportEmail, ConnectionString.WebSite);

                var productName = LicenseService.GetProductName(ConnectionString.ConnectionStringDB);
                if (ConnectionString.IsLocalAnalyst) {
                    productName = "Local Analyst";
                }

                string email_subject = productName + " DPA Reports";

                try
                {
                    _emailManager.SendEmail(toEmail, email_subject, email_body, ConnectionString.SupportEmail);
                }
                catch (Exception ex)
                {
                    if (!string.IsNullOrEmpty(ConnectionString.SQSError)) {
                        var sqs = new AmazonSQS();
                        string urlQueue = sqs.GetAmazonSQSUrl(ConnectionString.SQSError);
                        sqs.WriteMessage(urlQueue, "SendDPAReportNotification: " + ex.Message);
                    }
                }
            }
        }
    }
}