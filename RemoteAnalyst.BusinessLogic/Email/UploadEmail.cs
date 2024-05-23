using System;
using System.Collections.Generic;
using log4net;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.BusinessLogic.Email {
    public class UploadEmail
    {
        private static readonly ILog Log = LogManager.GetLogger("EmailError");
        private readonly EmailManager _emailManager;

        private readonly string _advisorEmail;
        private readonly string _serverPath;
        private readonly string _supportEmail;
        private readonly string _website;
        private readonly bool _isLocalAnalyst;

        public UploadEmail(string advisorEmail, string supportEmail, string website, string emailServer,
            int emailPort, string emailUser, string emailPassword, bool emailAuth, string systemLocation,
            string serverPath, bool isSSL, bool isLocalAnalyst,
            string mailGunSendAPIKey, string mailGunSendDomain)
        { 
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


            _advisorEmail = advisorEmail;
            _supportEmail = supportEmail;
            _website = website;
            _isLocalAnalyst = isLocalAnalyst;
            _serverPath = serverPath;
        }

        public void SendLoadLoadEmail(string emailAddress, string customerName, string systemSerial, string systemName, DateTime fromDateTime, DateTime toDateTime) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            var email = new EmailHeaderFooter();

            //Get Header.
            string email_body = email.EmailHeader(_isLocalAnalyst);

            email_body += "<br /><br />Dear " + customerName + ",<br />";
            email_body += $"<br />We have successfully loaded  the measure data for {systemName} ({systemSerial}) from {fromDateTime.ToString("yyyy-MM-dd HH:mm")} to {toDateTime.ToString("yyyy-MM-dd HH:mm")}<br />";
            if (!_isLocalAnalyst) {
                email_body += "<br />Data is available for analysis at www.RemoteAnalyst.com<br /><br />";
                email_body += "Should you have any questions please contact us at support@remoteanlayst.com";
            }
            else {
                email_body += $"<br />Data is available for analysis at {_website}<br /><br />";
                email_body += $"Should you have any questions please contact us at {_supportEmail}";
            }

            //Get Footer.
            email_body += email.EmailFooter(_supportEmail, _website);
            string email_subject = string.Empty;
            if (fromDateTime.Date != toDateTime.Date) {
                email_subject = $"MEASURE DATA LOADED  {systemName} FOR {fromDateTime.ToString("yyyy-MM-dd HH:mm")} TO {toDateTime.ToString("yyyy-MM-dd HH:mm")}";
            }
            else {
                email_subject = $"MEASURE DATA LOADED  {systemName} FOR {fromDateTime.ToString("yyyy-MM-dd")}";
            }
            
            //Sent Email.
            try
            {
                _emailManager.SendEmail(emailAddress, email_subject, email_body, _supportEmail);
            }
            catch (Exception ex) {
                Log.ErrorFormat("SendLoadLoadEmail: {0}", ex);
            }
        }

        public void SendLoadFailEmail(string emailAddress, string customerName, string systemSerial, string systemName, DateTime fromDateTime, DateTime toDateTime) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            var email = new EmailHeaderFooter();

            //Get Header.
            string email_body = email.EmailHeader(_isLocalAnalyst);
            email_body += "<br /><br />Dear " + customerName + ",<br />";
            email_body += $"<br />We are unable to load the measure data for {systemName} ({systemSerial}) from {fromDateTime.ToString("yyyy-MM-dd HH:mm")} to {toDateTime.ToString("yyyy-MM-dd HH:mm")}<br />";
            email_body += "Interval for the measure data must be at least 1 minute.<br />";

            //Get Footer.
            email_body += email.EmailFooter(_supportEmail, _website);

            string email_subject = string.Empty;
            if (fromDateTime.Date != toDateTime.Date) {
                email_subject = $"MEASURE DATA LOADED  {systemName} FOR {fromDateTime.ToString("yyyy-MM-dd HH:mm")} TO {toDateTime.ToString("yyyy-MM-dd HH:mm")}";
            }
            else {
                email_subject = $"MEASURE DATA LOADED  {systemName} FOR {fromDateTime.ToString("yyyy-MM-dd")}";
            }
            
            //Sent Email.
            try {
                _emailManager.SendEmail(emailAddress, email_subject, email_body, _supportEmail);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("SendLoadFailEmail: {0}", ex);
            }
        }


        public void SendGlacierLoadEmail(string emailAddress, string customerName, string systemSerial, string systemName, DateTime fromDateTime, DateTime toDateTime) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            string email_subject = $"Archive Data Loaded for {systemName}";
            var email = new EmailHeaderFooter();

            //Get Header.
            string email_body = email.EmailHeader(_isLocalAnalyst);

            email_body += "<br /><br />Dear " + customerName + ",<br />";
            email_body += "<br />This is to confirm that we have loaded the archive data for the following:<br />";
            email_body += "<br />System: <br />";
            email_body += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;System #:&nbsp;" + systemSerial + "<br />";
            email_body += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Name:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + systemName + "<br />";
            email_body += "<br />Reload Data:<br />";
            email_body += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;From:&nbsp;" + fromDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "<br />";
            email_body += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;To:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + toDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "<br />";
            email_body += "<br />You will receive another email, when the data is loaded.<br />";
            email_body += "<br />Write to " + _supportEmail + " if assistance is needed.<br />";

            //Get Footer.
            email_body += email.EmailFooter(_supportEmail, _website);

            //Sent Email.
            try {
                _emailManager.SendEmail(emailAddress, email_subject, email_body, _supportEmail);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("SendGlacierLoadEmail: {0}", ex);
            }
        }

        public void SendGlacierFailEmailToSupport(string emailAddress, string customerName, string systemSerial, string systemName, DateTime fromDateTime, DateTime toDateTime, Dictionary<string, string> failedArchiveIds) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");            
            string email_subject = $"Archive Data Loaded Failed for {systemName}";

            var email = new EmailHeaderFooter();
            string email_body = email.EmailHeader(_isLocalAnalyst);
            email_body += "<br /><br />Dear Support,<br />";
            email_body += "<br />We failed to load following archive data:<br />";
            email_body += "<br />Request By: " + customerName + "<br />";
            email_body += "<br />System Name: " + systemName + "<br />";
            email_body += "<br />Archive Data:";
            email_body += "<br />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;From:" + fromDateTime.ToString("yyyy-MM-dd HH:mm");
            email_body += "<br />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;To:" + toDateTime.ToString("yyyy-MM-dd HH:mm") + "<br /><br />";
            email_body += "<br />Archive Ids:<br />";

            foreach (var failedArchiveId in failedArchiveIds) {
                email_body += "<br />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;ID:" + failedArchiveId.Key;
                email_body += "<br />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Time:" + failedArchiveId.Value + "<br />";
            }
            email_body += email.EmailFooter(_supportEmail, _website);

            //Sent Email.
            try {
                _emailManager.SendEmail(emailAddress, email_subject, email_body, _supportEmail);
            }
            catch (Exception ex) {
                Log.ErrorFormat("SendGlacierFailEmailToSupport: {0}", ex);
            }
        }

        public void SendGlacierFailEmailToCustomer(string emailAddress, string customerName, string systemSerial, string systemName, DateTime fromDateTime, DateTime toDateTime, Dictionary<string, string> failedArchiveIds) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            var email = new EmailHeaderFooter();
            string email_subject = $"Unable to load archive data for {systemName}";

            //Get Header.
            string email_body = email.EmailHeader(_isLocalAnalyst);

            email_body += $"<br /><br />Dear {customerName},<br />";
            email_body += "<br />We were unable to process the request to reload the following archive data. Our support team will review the request and contact you via email.<br />";
            email_body += "<br />";
            email_body += $"<br />System Number: {systemSerial}";
            email_body += "<br />System Name: " + systemName + "<br />";
            email_body += "<br />Collection:";
            email_body += "<br />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;From:" + fromDateTime.ToString("yyyy-MM-dd HH:mm");
            email_body += "<br />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;To:" + toDateTime.ToString("yyyy-MM-dd HH:mm") + "<br />";
            email_body += "<br />Write support@remoteanalyst.com if immediate assistance is needed.<br />";
            
            //Get Footer.
            email_body += email.EmailFooter(_supportEmail, _website);

            //Sent Email.
            try {
                _emailManager.SendEmail(emailAddress, email_subject, email_body, _supportEmail);
            }
            catch (Exception ex) {
                Log.ErrorFormat("SendGlacierFailEmailToCustomer: {0}", ex);
            }
        }

        public void CreateSendErrorEmail(string emailText, string path, string connStr) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            //email subject
            var productName = LicenseService.GetProductName(connStr);
            if (_isLocalAnalyst) {
                productName = "Local Analyst";
            }
            string email_subject = productName + " Error";

            //email body
            var email = new EmailHeaderFooter();
            string email_body = email.EmailHeader(_isLocalAnalyst);
            email_body += "The following Error Message has been generated on " + productName + ":<br><br>";
            email_body += emailText;
            email_body += email.EmailFooter(_supportEmail, _website);

            //send the email
            try {
                string[] attachments = { path };
                _emailManager.SendEmail(_supportEmail, email_subject, email_body, null, attachments);
            }
            catch (Exception ex) {
                Log.ErrorFormat("CreateSendErrorEmail1 {0}", ex);
            }
        }
    }
}
