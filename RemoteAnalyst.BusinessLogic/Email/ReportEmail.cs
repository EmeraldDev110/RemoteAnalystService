using System;
using log4net;

namespace RemoteAnalyst.BusinessLogic.Email {
    public class ReportEmail
    {
        private static readonly ILog Log = LogManager.GetLogger("EmailError");
        private readonly EmailManager _emailManager;

        private readonly string _supportEmail;
        private readonly string _website;
        private readonly bool _isLocalAnalyst;

        public ReportEmail(string advisorEmail, string supportEmail, string website, string emailServer,
            int emailPort, string emailUser, string emailPassword, bool emailAuth, string systemLocation,
            string serverPath, bool isSSL, bool isLocalAnalyst,
            string mailGunSendAPIKey, string mailGunSendDomain) {
            _emailManager = new EmailManager(emailServer
                                , serverPath
                                , emailPort
                                , emailUser
                                , emailPassword
                                , emailAuth
                                , systemLocation
                                , advisorEmail
                                , supportEmail
                                , website
                                , isSSL
                                , isLocalAnalyst
                                , mailGunSendAPIKey
                                , mailGunSendDomain);
            _supportEmail = supportEmail;
            _website = website;
            _isLocalAnalyst = isLocalAnalyst;
        }

        public void SendGlacierConfirmation(string emailAddress, string customerName, string systemSerial, string systemName, DateTime fromDateTime, DateTime toDateTime) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            var email = new EmailHeaderFooter();
            string email_subject = $"Archive Data Load Confirmation for {systemName}.";

            //Get Header.
            string email_body = email.EmailHeader(_isLocalAnalyst);

            email_body += "<br /><br />Dear " + customerName + ",<br />";
            email_body += "<br />This is to confirm that we have received your request to reload archive data for the following:<br />";
            email_body += "<br />System: <br />";
            email_body += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;System #:&nbsp;" + systemSerial + "<br />";
            email_body += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Name:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + systemName + "<br />";
            email_body += "<br />Reload Data:<br />";
            email_body += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;From:&nbsp;" + fromDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "<br />";
            email_body += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;To:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + toDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "<br />";
            email_body += "<br />You will receive another email, when the data is loaded.<br />";
            email_body += "<br />Write to " + _supportEmail + " if assistance is needed.<br />";


            //Sent Email.
            try {
                _emailManager.SendEmail(emailAddress, email_subject, email_body, _supportEmail);
            }
            catch (Exception ex) {
                Log.ErrorFormat("SendGlacierConfirmation: {0}", ex);
            }
        }
        
        public void SendReportConfirmation(string emailAddress, string customerName, string systemSerial, string systemName, DateTime fromDateTime, DateTime toDateTime, bool qtReport) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            var email = new EmailHeaderFooter();

            //Set the subject and body
            string email_subject = string.Empty;
            if (qtReport)
                email_subject = "QT Report Confirmation.";
            else
                email_subject = "DPA Report Confirmation.";

            //Get Header.
            string email_body = email.EmailHeader(_isLocalAnalyst);

            email_body += "<br />Dear " + customerName + ",<br />";
            if (qtReport)
               email_body += "<br />This is to confirm that we have received your order to generate QT report for the following collection:<br />";
            else
               email_body += "<br />This is to confirm that we have received your order to generate DPA report for the following collection:<br />";

            email_body += "<br />System: <br />";
            email_body += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;System #:&nbsp;" + systemSerial + "<br />";
            email_body += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Name:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + systemName + "<br />";
            email_body += "<br />Collection:<br />";
            email_body += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;From:&nbsp;" + fromDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "<br />";
            email_body += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;To:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + toDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "<br />";
            
            email_body += "<br />Write to " + _supportEmail + " if assistance is needed.<br />";

            //Get Footer.
            email_body += email.EmailFooter(_supportEmail, _website);

            //Sent Email.
            try
            {
                _emailManager.SendEmail(emailAddress, email_subject, email_body, _supportEmail);
            }
            catch (Exception ex) {
                Log.ErrorFormat("SendReportConfirmation: {0}", ex);
            }
        }

        public void SendLoadCompleteEmail(string emailAddress, string customerName, string systemName) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            var email = new EmailHeaderFooter();
            string email_subject = systemName + " – Your request has been processed";

            //Get Header.
            string email_body = email.EmailHeader(_isLocalAnalyst);
            email_body += "<br /><br />Dear " + customerName + ",<br />";
            email_body += "<br />This email is to notify you that your request has been successfully processed, and you should see the data loaded for " + systemName + ".:<br />";
            email_body += "<br />If you have any questions or concerns, please write to " + _supportEmail + "<br />";
            //Get Footer.
            email_body += email.EmailFooter(_supportEmail, _website);

            //Sent Email.
            try
            {
                _emailManager.SendEmail(emailAddress, email_subject, email_body, _supportEmail);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("SendLoadCompleteEmail: {0}", ex);
            }
        }

        public void SendApplicationLoadEmail(string emailAddress, string customerName, string applicationName) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            var email = new EmailHeaderFooter();

            //Get Header.
           string email_body = email.EmailHeader(_isLocalAnalyst);
           email_body += "<br /><br />Dear " + customerName + ",<br />";

            if (!_isLocalAnalyst) {
               email_body += "<br />Remote Analyst completed loading data for Application " + applicationName + "<br />";
               email_body += "<br />You may logon to Remote Analyst, and review this application's activities.<br />";
            }
            else {
               email_body += "<br />Local Analyst completed loading data for Application " + applicationName + "<br />";
               email_body += "<br />You may logon to Local Analyst, and review this application's activities.<br />";
            }
            //Get Footer.
           email_body += email.EmailFooter(_supportEmail, _website);

            string email_subject = string.Empty;
            if (!_isLocalAnalyst) {
                email_subject = "RA - load for Application " + applicationName;
                
            } else {
                email_subject = "Local Analyst - load for Application " + applicationName;
            }

            //Sent Email.
            try
            {
                _emailManager.SendEmail(emailAddress, email_subject, email_body, _supportEmail);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("SendApplicationLoadEmail: {0}", ex);
            }
        }
    }
}
