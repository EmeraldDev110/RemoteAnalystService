using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using log4net;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.ReportGenerator.BLL
{
    /// <summary>
    /// Send Error emails to support.
    /// </summary>
    internal class ErrorEmails
    {
        private readonly EmailManager _emailManager;
        private static readonly ILog Log = LogManager.GetLogger("EmailError");
        public ErrorEmails()
        {
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
        /// Send Report generate error.
        /// </summary>
        /// <param name="systemName">System Name</param>
        /// <param name="startTime">Report Start Time</param>
        /// <param name="endTime">Report Stop Time</param>
        /// <param name="errorMessage">Error Message</param>
        /// <param name="reportType">Report Type</param>
        internal void SendReportErrorEmail(string systemName, DateTime startTime, DateTime endTime, string errorMessage,
            string reportType, string instanceID)
        {
            try
            {
                //Force all datetime to be in US format.
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                string email_subject = LicenseService.GetProductName(ConnectionString.ConnectionStringDB) + " " + reportType + " Generation Error";
                var header = new EmailHeaderFooter();

                //Get Header.
                string email_body = header.EmailHeader(ConnectionString.IsLocalAnalyst);
                //contents.
                email_body += "System Name  : " + systemName + "<br />";
                email_body += "Report Type  : " + reportType + "<br />";
                email_body += "Start Time   : " + startTime + "<br />";
                email_body += "End Time     : " + endTime + "<br />";
                email_body += "Error Message: " + errorMessage + "<br />";
                email_body += "EC2 Instance ID: " + instanceID + "<br />";
                //Get Footer.
                email_body += header.EmailFooter(ConnectionString.SupportEmail, ConnectionString.WebSite);

                _emailManager.SendEmail(ConnectionString.SupportEmail, email_subject, email_body, ConnectionString.SupportEmail);
            }
            catch (Exception ex)
            {
                Log.InfoFormat("CreateSendErrorEmail {0}, {1} ", ex, errorMessage);
            }
        }
    }
}