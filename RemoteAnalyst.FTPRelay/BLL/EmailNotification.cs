using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using log4net;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.FTPRelay.BLL {
    public class EmailNotification {
        private static readonly ILog Log = LogManager.GetLogger("EmailError");
        private readonly string _advisorEmail = ConnectionString.AdvisorEmail;
        private readonly string _supportEmail = ConnectionString.SupportEmail;
        private readonly string _emailServer;
        private readonly string _password;
        private readonly int _emailPort = ConnectionString.EmailPort;
        private readonly string _userName;
        private readonly SmtpClient _emailClient;
        private MailMessage _emailMessage;
        private readonly NetworkCredential credential;

        public EmailNotification() {
            var decrypt = new Decrypt();
            _emailServer = decrypt.strDESDecrypt(ConnectionString.EmailServer);
            _userName = decrypt.strDESDecrypt(ConnectionString.EmailUser);
            _password = decrypt.strDESDecrypt(ConnectionString.EmailPassword);

            System.Net.ServicePointManager.SecurityProtocol = (SecurityProtocolType)(0xc0 | 0x300 | 0xc00);
            //Initialize email server
            _emailClient = new SmtpClient();
            _emailClient.EnableSsl = ConnectionString.EmailIsSSL;
            _emailClient.Host = _emailServer;
            _emailClient.Port = _emailPort;
            _emailClient.UseDefaultCredentials = false;
            //Set up credentials
            credential = new NetworkCredential();
            credential.UserName = _userName;
            credential.Password = _password;
            _emailClient.Credentials = credential;
        }

        public void SendUploadFailedEmail(string systemSerial, string fileName, string type, string errorMessage = "") {
            try {
                MailAddress toEmail = new MailAddress(_supportEmail);
                MailAddress fromEmail = new MailAddress(_advisorEmail);
                _emailMessage = new MailMessage(fromEmail, toEmail);
                _emailMessage.Subject = "FTP Failed";
                _emailMessage.IsBodyHtml = true;

                _emailMessage.Body = "Following FTP Transfer Failed at " + DateTime.Now + ": <br><br>";
                _emailMessage.Body += "System Serial: " + systemSerial + "<br>";
                _emailMessage.Body += "File Name: " + fileName + "<br>";
                if (errorMessage.Length > 0)
                    _emailMessage.Body +=  "Message: " + errorMessage + "<br><br>";

                _emailClient.Send(_emailMessage);

            }
            catch (Exception ex) {
                Log.ErrorFormat("FTP Log File: {0}", ex);
            }
        }
    }
}
