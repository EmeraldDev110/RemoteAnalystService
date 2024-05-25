using System;
using System.IO;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using System.Net.Mail;
using System.Net.Mime;
using log4net;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.Infrastructure;

namespace RemoteAnalyst.UWSRelay.BLL {
	public class EmailHelper {
        private static readonly ILog Log = LogManager.GetLogger("EmailError"); 
		private readonly EmailManager _emailManager;

		public EmailHelper() {
			var decrypt = new Decrypt();
			_emailManager = new EmailManager(decrypt.strDESDecrypt(ConnectionString.EmailServer)
								, ConnectionString.ServerPath
								, ConnectionString.EmailPort
								, decrypt.strDESDecrypt(ConnectionString.EmailUser)
								, decrypt.strDESDecrypt(ConnectionString.EmailPassword)
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

		public enum LicenseExpireEmailReason { NotFound, ExpiredWithinGrace, Expired };
		/// <summary>
		/// Send out email if the system's license for RemoteAnalyst expired.
		/// </summary>
		/// <param name="systemSerial"> System serial number.</param>
		public void SendLicenseExpireEmail(string systemSerial, LicenseExpireEmailReason sendEmailReason) {
			//Sending Email with the report as attachment
			string connStr = ConnectionString.ConnectionStringDB;
			var systems = new System_tblService(connStr);
			string systemName = systems.GetSystemNameFor(systemSerial);
			string localPath = ConnectionString.ServerPath;

			string email_subject = LicenseService.GetProductName(ConnectionString.ConnectionStringDB) + " License";
			//email body
			var email = new EmailHeaderFooter();
			string email_body = email.EmailHeader(ConnectionString.IsLocalAnalyst);
			var reason = "expired";
            if(sendEmailReason == LicenseExpireEmailReason.NotFound) {
                reason = "not found";
            }
			email_body  += "Dear Support, <br><br>";
			email_body  += "The License for System: " + systemName + " " + reason + ".<br>";
			if (!ConnectionString.IsLocalAnalyst && sendEmailReason != LicenseExpireEmailReason.NotFound) {
				if (sendEmailReason == LicenseExpireEmailReason.ExpiredWithinGrace) {
					email_body  += systemName + " is currently within 7 days of grace period.<br>";
				}
				else {
					email_body  += systemName + " is currently out of 7 days of grace period.<br>";
				}
			}
			if (ConnectionString.IsLocalAnalyst)
				email_body  += "Please contact license.manager@hpe.com<br><br>";
			email_body  += email.EmailFooter(ConnectionString.SupportEmail, ConnectionString.WebSite);

			//send the email
			try {
				_emailManager.SendEmail(ConnectionString.SupportEmail, email_subject, email_body);
			}
			catch (Exception ex) {
				Log.ErrorFormat("SendLicenseExpireEmail: {0}", ex);
			}
		}

	}
}
