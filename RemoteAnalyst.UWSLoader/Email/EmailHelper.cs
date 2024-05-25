using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading;
using log4net;
using RemoteAnalyst.BusinessLogic.Enums;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.ModelService;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.Repository.Concrete;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.UWSLoader.Email {
    /// <summary>
    /// EmailHelper class should contains all email send out function except error email and loads complete notification
    /// For now it only have function the send license expire email.
    /// </summary>
    public class EmailHelper {
        private static readonly ILog Log = LogManager.GetLogger("EmailError");
        private readonly EmailManager _emailManager;

		public EmailHelper() {
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

		public void SendRDSMoveErrorEmail(string systemSerial, string tableName) {
			var systems = new SystemRepository();
			string systemName = systems.GetSystemName(systemSerial);
			string email_subject = "RDSMove for " + systemName + "error";
			//email body
			var email = new EmailHeaderFooter();
			string email_body = email.EmailHeader(ConnectionString.IsLocalAnalyst);
			email_body += "Dear Support, <br><br>";
			email_body += "The RDSMove for System: " + systemName + " has error when copy table " + tableName + " at " + DateTime.Now + ".<br>";
			email_body += email.EmailFooter(ConnectionString.SupportEmail, ConnectionString.WebSite);

			//send the email
			try {
				_emailManager.SendEmail(ConnectionString.SupportEmail, email_subject, email_body);
			}
			catch (Exception ex) {
				Log.ErrorFormat("SendRDSMoveStartEmail: {0}, {1}, {2}", 
									systemSerial, tableName, ex);
			}
		}

		public void SendRDSMoveFinishEmail(string systemSerial) {
			var systems = new SystemRepository();
			string systemName = systems.GetSystemName(systemSerial);
			string email_subject  = "RDSMove for " + systemName + "has finished";

			//email body
			var email = new EmailHeaderFooter();
			string email_body = email.EmailHeader(ConnectionString.IsLocalAnalyst);
			email_body += "Dear Support, <br><br>";
			email_body += "The RDSMove for System: " + systemName + " has finished at " + DateTime.Now + ".<br>";
			email_body += email.EmailFooter(ConnectionString.SupportEmail, ConnectionString.WebSite);

			//send the email
			try {
				_emailManager.SendEmail(ConnectionString.SupportEmail, email_subject, email_body);
			}
			catch (Exception ex) {
                Log.ErrorFormat("SendRDSMoveFinishEmail: {0} {1}", systemSerial, ex);
			}
		}

		public void SendRDSMoveStartEmail(string systemSerial) {
			var systems = new SystemRepository();
			string systemName = systems.GetSystemName(systemSerial);
			string email_subject = "RDSMove for " + systemName + "has started";

			//email body
			var email = new EmailHeaderFooter();
			string email_body = email.EmailHeader(ConnectionString.IsLocalAnalyst);
			email_body += "Dear Support, <br><br>";
			email_body += "The RDSMove for System: " + systemName + " has started at " + DateTime.Now + ".<br>";
			email_body += email.EmailFooter(ConnectionString.SupportEmail, ConnectionString.WebSite);

			//send the email
			try {
				_emailManager.SendEmail(ConnectionString.SupportEmail, email_subject, email_body);
			}
			catch (Exception ex) {
                Log.ErrorFormat("SendRDSMoveStartEmail: {0} {1}", systemSerial, ex);
            }
		}

		/// <summary>
		/// Send out the email notification for errors. Attach the log file in the email content.
		/// </summary>
		/// <param name="emailText"> Part of the email content that describe the errors.</param>
		public void SendErrorEmail(string emailText) {
			//Force all datetime to be in US format.
			System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

			string localPath = ConnectionString.ServerPath;
			string emailtext = string.Empty;
			string connStr = ConnectionString.ConnectionStringDB;

			//For Email
			string supportEmail = ConnectionString.SupportEmail;
			string WebSite = ConnectionString.WebSite;
			var productName = LicenseService.GetProductName(ConnectionString.ConnectionStringDB);
			var isLocalAnalyst = LicenseService.IsLocalAnalystOrPMC(ConnectionString.ConnectionStringDB);
			if (isLocalAnalyst) {
				productName = "Local Analyst";
			}

			string email_subject  = productName + " Error";

			//email body
			emailtext = "The following Error Message has been generated on " + productName + ":<br><br>";
			emailtext += emailText;
			var email = new EmailHeaderFooter();
			string email_body = email.EmailHeader(ConnectionString.IsLocalAnalyst);
			email_body += emailtext;
			email_body += email.EmailFooter(supportEmail, WebSite);


			//send the email
			try {
				_emailManager.SendEmail(ConnectionString.SupportEmail, email_subject, email_body);
			}
			catch (Exception ex) {
                Log.ErrorFormat("CreateSendErrorEmail1: {0}", ex);
			}
		}

		public void SendErrorEmail(string emailAddress, string customerName, UploadError.Types errorType) {
			//Force all datetime to be in US format.
			System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
			
			string emailText = "";
			string email_subject = "";
			if (errorType == UploadError.Types.Fail) {
				//set the subject and body
				email_subject  = "Invalid Metric File";
				emailText = "Dear " + customerName + ",<br><br>" +
							"The metric file you uploaded is invalid file.";
			}
			else if (errorType == UploadError.Types.NoMatch) {
				//set the subject and body
				email_subject  = "Non Registered System";
				emailText = "Dear " + customerName + ",<br><br>" +
							"The metric file you uploaded belongs to unknown or non-registered system.";
			}
			else if (errorType == UploadError.Types.Same) {
				//set the subject and body
				email_subject  = "Invalid Metric File";
				emailText = "Dear " + customerName + ",<br><br>" +
							"The metric file you uploaded already exists.";
			}

			//email body
			var email = new EmailHeaderFooter();
			string email_body = email.EmailHeader(ConnectionString.IsLocalAnalyst);
			email_body += emailText;
			email_body += email.EmailFooter(ConnectionString.SupportEmail, ConnectionString.WebSite);

			//send the email
			try {
				_emailManager.SendEmail(emailAddress, email_subject, email_body, ConnectionString.SupportEmail);
			}
			catch (Exception ex) {
                Log.ErrorFormat("SendErrorEmail: {0}", ex);
                Log.ErrorFormat("ConnectionString.AdvisorEmail: {0}", ConnectionString.AdvisorEmail);
			}
		}

		public void SendSCMLoadCompleteEmail(string emailAddress, string customerName, string profileName, string date) {
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			MailAddress toEmail = new MailAddress(emailAddress);
			MailAddress fromEmail = new MailAddress(ConnectionString.AdvisorEmail);
			string email_subject  = LicenseService.GetProductName(ConnectionString.ConnectionStringDB) + " SCM Profile Load Complete";

			var sbEmail = new StringBuilder();
			var header = new EmailHeaderFooter();
			sbEmail.Append(header.EmailHeader(ConnectionString.IsLocalAnalyst));
			sbEmail.Append(BuildSCMLoadBody(customerName, profileName, date));
			sbEmail.Append(header.EmailFooter(ConnectionString.SupportEmail, ConnectionString.WebSite));
			string email_body = sbEmail.ToString();

			try
			{
				_emailManager.SendEmail(emailAddress, email_subject, email_body, ConnectionString.SupportEmail);
			}
			catch (Exception ex) {
				throw new Exception(ex.Message);
			}
		}

		private string BuildSCMLoadBody(string customerName, string profileName, string date) {
			var sb = new StringBuilder();
			if (customerName.Equals(""))
				customerName = "Customer";
			sb.Append("Dear " + customerName + ", <br><br>");
			sb.Append("Historical review for Profile " + profileName + " is completed. <br>");
			sb.Append("This profile now has TPS data going back to " + date + ".<br><br>");
			sb.Append("Please write to <a href='" + ConnectionString.SupportEmail + "'>" + ConnectionString.SupportEmail + "</a> if assistance is needed.<br><br>");
			return sb.ToString();
		}
	}
}