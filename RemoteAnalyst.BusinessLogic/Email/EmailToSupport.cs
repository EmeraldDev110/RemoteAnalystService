using System;
using System.Collections.Generic;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.BusinessLogic.ModelView;
using System.Linq;
using log4net;

namespace RemoteAnalyst.BusinessLogic.Email
{
    public class EmailToSupport
    {
        private static readonly ILog Log = LogManager.GetLogger("EmailError");
        private readonly EmailManager _emailManager;

        private readonly string _advisorEmail;
        private readonly string _serverPath;
        private readonly string _supportEmail;
        private readonly string _website;
        private readonly bool _isLocalAnalyst;

        public EmailToSupport(string advisorEmail, string supportEmail, 
            string website, string emailServer,
            int emailPort, string emailUser, string emailPassword, 
            bool emailAuth, string systemLocation,
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

        public void SendLicenseNotice(Dictionary<string, int> dicLicense, Dictionary<string, string> systemSerialAndCompanyName) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            var email = new EmailHeaderFooter();
            //Set the subject and body
            string email_subject = "License notice.";

            //Get Header.
            string email_body = email.EmailHeader(_isLocalAnalyst);
			email_body += "<br /><br />";

			//Get Body.
			if (dicLicense.Values.Any(value => (value < 0))) {
				email_body += "License(s) for the following server(s) expired:";
				email_body += "<br /><br />";
				email_body += "<ul>";
				// key: system name, val: licence expiration date
				foreach (KeyValuePair<string, int> i in dicLicense) {
					if (i.Value < 0) {
						string companyName = systemSerialAndCompanyName.ContainsKey(i.Key) ? systemSerialAndCompanyName[i.Key] : "Unknow Company Name";
						email_body += $"<li>System Serial: {i.Key}, Company Name: {companyName}, Date of Expiry: {DateTime.Now.AddDays(i.Value).ToString("MM-dd-yyyy")}</li>";
					}
				}
				email_body += "</ul>";
			}

			if (dicLicense.Values.Any(value => (value >= 0))) {
				email_body += "License(s) for the following server(s) will expire:";
				email_body += "<br /><br />";
				email_body += "<ul>";
				// key: system name, val: licence expiration date
				foreach (KeyValuePair<string, int> i in dicLicense) {
					if (i.Value >= 0) {
						string companyName = systemSerialAndCompanyName.ContainsKey(i.Key) ? systemSerialAndCompanyName[i.Key] : "Unknow Company Name";
						email_body += $"<li>System Serial: {i.Key}, Company Name: {companyName}, Date of Expiry: {DateTime.Now.AddDays(i.Value).ToString("MM-dd-yyyy")}</li>";
					}
				}
				email_body += "</ul>";
			}
			
			if (_isLocalAnalyst) {
				email_body += "<br /><br />";
				email_body += "Please contact License Manager at <a href=\"mailto:license.manager@hpe.com\">license.manager@hpe.com</a> to renew the license keys.";
			}
			//Get Footer.
			email_body += email.EmailFooter(_supportEmail, _website);


            //Sent Email.
            try
            {
                _emailManager.SendEmail(_supportEmail, email_subject, email_body);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("SendLicenseNotice: {0}", ex);
            }
        }

		public void SendAWSErrorEmail(string message, string source) {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            var email = new EmailHeaderFooter();
            //Set the subject and body
            string email_subject = "AWS Error Message.";

            //Get Header.
            string email_body = email.EmailHeader(_isLocalAnalyst);

            email_body += "<br /><br />Following Error Occurred on :" + source + "<br>";
            email_body += "Error Message: " + message;

            //Get Footer.
            email_body += email.EmailFooter(_supportEmail, _website);

            //Sent Email.
            try
            {
                _emailManager.SendEmail(_supportEmail, email_subject, email_body);
            }
            catch (Exception ex) {
                Log.ErrorFormat("SendAWSErrorEmail: {0}", ex);
            }
        }

        public void SendUnknownCpuType(string sytemSerial, string osVersion, string cupType, string cupSubType) {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            var email = new EmailHeaderFooter();
            //Set the subject and body
            string email_subject = "Unknown CPU Info From " + sytemSerial;

            //Get Header.
            string email_body = email.EmailHeader(_isLocalAnalyst);

            email_body += "<br /><br />We received unknown CPU Info from " + sytemSerial + "<br />";
            email_body += "<br />OS Version: " + osVersion + "<br />";
            email_body += "<br />CPU Type: " + cupType + "<br />";
            email_body += "<br />CPU Sub Type: " + cupSubType + "<br />";

            //Get Footer.
            email_body += email.EmailFooter(_supportEmail, _website);
           
            //Sent Email.
            try {
                _emailManager.SendEmail(_supportEmail, email_subject, email_body);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("SendUnknownCpuType: {0}", ex);
            }
        }

        public void SendReportWatcher(ReportDetail reportDetail, int reportDownloadId, string connectionStringDB) {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            var email = new EmailHeaderFooter();
            //Set the subject and body
            string email_subject = "Report Delay";

            //Get Header.
            string email_body = email.EmailHeader(_isLocalAnalyst);

            email_body += "<br /><br />The following Message has been generated on Report Watcher:<br />";
            email_body += "<br /><br />Below Report has been running over 4 hours:<br />";
			email_body += "<br />ReportDownloadId: " + reportDownloadId;
			email_body += "<br />System Name: " + reportDetail.SystemName;
            email_body += "<br />System Serial: " + reportDetail.SystemSerial;
            email_body += "<br />From: " + reportDetail.StartTime;
            email_body += "<br />To: " + reportDetail.EndTime;
            email_body += "<br />Report Type: " + reportDetail.ReportType;
            email_body += "<br />Order By: " + reportDetail.OrderBy + "<br />";

            //Get Footer.
            email_body += email.EmailFooter(_supportEmail, _website);

            //Sent Email.
            try {
                _emailManager.SendEmail(_supportEmail, email_subject, email_body);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("SendReportWatcher: {0}", ex);
            }
        }

        public void SendLocalAnalystErrorEmail(string source, string logFileLocation, string productName) {
			if (_isLocalAnalyst) {
				productName = "Local Analyst";
			}

            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            var email = new EmailHeaderFooter();
            //Set the subject and body
            string email_subject = productName + " Error";

            string email_body = email.EmailHeader(_isLocalAnalyst);
            email_body += "<br /><br />" + productName + " Error From " + source + "<br />";
            email_body += email.EmailFooter(_supportEmail, _website);

            //Sent Email.
            try {
                string[] attachments = { logFileLocation };
                _emailManager.SendEmail(_supportEmail, email_subject, email_body, null, attachments);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("SendLocalAnalystErrorEmail: {0}", ex);
            }
        }
		
        public void SendPathwayParamaterErrorMassageEmail(string systemSerial, string systemName,
			DateTime startTime, DateTime endTime, string email, string type) {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            var header = new EmailHeaderFooter();

            //Set the subject and body
            string email_subject = "Error generating Pathway report on: " + systemName;

            string email_body = header.EmailHeader(_isLocalAnalyst);
            email_body += "Dear Customer,<br> Your Pathway " + type + " report was not generated since no pathways specified.<br><br>" + "System #: " + systemSerial +
									"<br>For " + startTime.ToString("MMM dd, yyyy") + " through " + endTime.ToString("MMM dd, yyyy") +
									"<br>System Name: " + systemName + "<br><br>No pathways specified for the below time period<br>From: " + startTime.ToString("yyyy-MM-dd HH:mm:ss") + "<br>To: " + endTime.ToString("yyyy-MM-dd HH:mm:ss") +
									"<br>Write to " + _supportEmail + " if assistance is needed.";
            email_body += header.EmailFooter(_supportEmail, _website);

            //Sent Email.
            try {
                _emailManager.SendEmail(_supportEmail, email_subject, email_body);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("SendPathwayParamaterErrorMassageEmail: {0}", ex);
            }
        }
		
        public void SendLocalAnalystErrorMessageEmail(string source, string errorMessage, string productName) {
            if (_isLocalAnalyst) {
                productName = "Local Analyst";
            }
            //Format the error message.
            var newMessage = "";

            if (errorMessage.Contains("Timeout in IO operation"))
                return;

            if (errorMessage.Contains("the server is not responding"))
                newMessage = "Unable to connect to the database";
            else if (errorMessage.Contains("Visual Basic Project is not trusted"))
                newMessage = "Please enable the macros within Excel Settings";
            else
                newMessage = errorMessage;


            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            var email = new EmailHeaderFooter();

            //Set the subject and body
            string email_subject = productName + " Error";

            //Get Header.
            string email_body = email.EmailHeader(_isLocalAnalyst);

            email_body += "<br /><br />" + productName + " Error From " + source + "<br />";
            email_body += "Error: " + newMessage + "<br />";

            //Get Footer.
            email_body += email.EmailFooter(_supportEmail, _website);

            //Sent Email.
            try {
                _emailManager.SendEmail(_supportEmail, email_subject, email_body);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("SendLocalAnalystErrorMessageEmail: {0}", ex);
            }
        }

        //RA-705
        public void SendFileLoadErrorEmail(string subject, string desc, string recipient, string customerName, string supportEmail, string systemSerial, Dictionary<string, string> filesFailed, string reportName, DateTime orderedDate, DateTime startTime, DateTime stopTime) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            var email = new EmailHeaderFooter();

            //Set the subject and body
            string email_subject = subject;

            //Get Header.
            string email_body = email.EmailHeader(_isLocalAnalyst);
            email_body += "<br/><br />Dear " + customerName + ",<br/>";
            //email_body += "<br /><br /> " + source + "<br />");//source could be RA or LA
            email_body += "<br/><br/>An issue was identified when generating the following report you had requested. Support team is notified of this issue. You will receive an update as soon as resolution in place or updates are available." + "<br />";
            email_body += "<br/> <b>Possible reasons are: </b>";
            email_body += "<br/>&nbsp;&nbsp;&nbsp;&nbsp; Files were not retrieved successfully from " + desc;
            email_body += "<br/>&nbsp;&nbsp;&nbsp;&nbsp; Files were not loaded to database successfully.";

            if (recipient.Equals(supportEmail)) {
                email_body += "<br/><br/><b>List of UWS files affected: </b>";
                foreach (KeyValuePair<string, string> kv in filesFailed) {
                    email_body += "<br/>&nbsp;&nbsp;&nbsp;&nbsp; " + kv.Key.Substring(kv.Key.IndexOf(systemSerial) + systemSerial.Length);
                }
            }

            email_body += "<br/><br/><b>Report requested: </b>";
            email_body += "<br/>&nbsp;&nbsp;&nbsp;&nbsp; Report name: " + reportName;
            email_body += "<br/>&nbsp;&nbsp;&nbsp;&nbsp; Ordered at: " + orderedDate;
            email_body += "<br/>&nbsp;&nbsp;&nbsp;&nbsp; Covering data period for : " + startTime.ToString("yyyy-MM-dd HH:mm") + " through " + stopTime.ToString("yyyy-MM-dd HH:mm");

            //Get Footer.
            email_body += email.EmailFooter(_supportEmail, _website);

            //Sent Email.
            try {
                _emailManager.SendEmail(recipient, email_subject, email_body, _supportEmail);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("SendFileLoadErrorEmail: {0}", ex);
            }
        }

        public bool SendPmcTestEmail(string emailTo) {
            var isEmailOkay = false;

            try {
                //Force all datetime to be in US format.
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                var email = new EmailHeaderFooter();

                //Set the subject and body
                string email_subject = "Local Analyst Test Email.";

                //Get Header.
                string email_body = email.EmailHeader(_isLocalAnalyst);
                DateTime expireDate = DateTime.Today.AddDays(29);

                email_body += "<br /><br /> This is test email from Local Analyst <br /><br />";
                //Get Footer.
                email_body += email.EmailFooter(_supportEmail, _website);

                //Sent Email.
                _emailManager.SendEmail(emailTo, email_subject, email_body);
                isEmailOkay = true;
            }
            catch (Exception ex) {
                throw new Exception(ex.ToString());
            }

            return isEmailOkay;
        }

        public void SendDataDropEmail(string systemName, DateTime dropDate, List<DateTime> missingSystemInterval, List<DateTime> loadFailSystemInterval, bool isStorageData, bool isPathway, long interval) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            var email = new EmailHeaderFooter();

            //Set the subject and body
            string email_subject = "DATA DROP " + systemName + " ON " + dropDate.ToString("yyyy-MM-dd");

            //Get Header.
            string email_body = email.EmailHeader(_isLocalAnalyst);

            if (missingSystemInterval.Count > 0) {
                email_body += "<br /><br />We did not receive following measure data for " + dropDate.ToString("yyyy-MM-dd") + ":";
                email_body += "<ul>";
                foreach (var missingDate in missingSystemInterval) {
                    email_body += "<li>" + missingDate.ToString("HH:mm") + " to " + missingDate.AddSeconds(interval).ToString("HH:mm") + " </li>";
                }
                email_body += "</ul>";
            }

            if (loadFailSystemInterval.Count > 0) {
                email_body += "<br /><br />We falied to load following UWS file for " + dropDate.ToString("yyyy-MM-dd") + ":";
                email_body += "<ul>";
                foreach (var missingDate in missingSystemInterval) {
                    email_body += "<li>" + missingDate.ToString("HH:mm") + " to " + missingDate.AddSeconds(interval).ToString("HH:mm") + " </li>";
                }
                email_body += "</ul>";
            }

            if (!isStorageData) {
                email_body += "<br /><br />We did not receive Storage data for " + dropDate.ToString("yyyy-MM-dd") + " <br />";
            }

            if (!isPathway) {
                email_body += "<br /><br />We did not receive Pathway data for " + dropDate.ToString("yyyy-MM-dd") + " <br />";
            }

            //Get Footer.
            email_body += email.EmailFooter(_supportEmail, _website);

            //Sent Email.
            try {
                _emailManager.SendEmail(_supportEmail, email_subject, email_body);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("SendDataDropEmail: {0}", ex);
            }
        }
    }
}