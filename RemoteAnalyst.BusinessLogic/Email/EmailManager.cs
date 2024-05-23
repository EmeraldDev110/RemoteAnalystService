using log4net;
using Mailgun.Core.Messages;
using Mailgun.Messages;
using Mailgun.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.BusinessLogic.Email
{
    public class EmailManager
    {

        private readonly string _advisorEmail = "";
        private readonly bool _emailAuthentication;
        private readonly string _emailPassword = "";
        private readonly int _emailPort;
        private readonly string _emailServer = "";
        private readonly string _emailUser = "";
        private readonly string _serverPath = "";
        private readonly string _supportEmail = "";
        private readonly string _systemLocation = "";
        private readonly string _webSite = "";
        private readonly bool _isSSL;
        private readonly bool _isLocalAnalyst;
        private readonly SmtpClient _emailClient;
        private MailMessage _emailMessage;
        private readonly NetworkCredential credential;
        private readonly MessageService _mailGunMessageService;
        private readonly string _mailGunSendDomain = "";
        private static readonly ILog Log = LogManager.GetLogger("EmailError");

        public EmailManager(string emailServer
            , string serverPath
            , int emailPort
            , string emailUser
            , string emailPassword
            , bool emailAuthentication
            , string systemLocation
            , string advisorEmail
            , string supportEmail
            , string webSite
            , bool isSSL
            , bool isLocalAnalyst
            , string mailGunSendAPIKey
            , string mailGunSendDomain)
        {
            _serverPath = serverPath;
            _systemLocation = systemLocation;
            _advisorEmail = advisorEmail;
            _supportEmail = supportEmail;
            if (webSite.EndsWith("/"))
                webSite = webSite.Remove(webSite.Length - 1, 1);

            _webSite = webSite;

            if (isLocalAnalyst)
            {
                _emailServer = emailServer;
                _emailPort = emailPort;
                _emailUser = emailUser;
                _emailPassword = emailPassword;
                _emailAuthentication = emailAuthentication;
                _isSSL = isSSL;
                _isLocalAnalyst = isLocalAnalyst;

                //Setup Email Server
                System.Net.ServicePointManager.SecurityProtocol = (SecurityProtocolType)(0xc0 | 0x300 | 0xc00);
                //Initialize email server
                _emailClient = new SmtpClient();
                _emailClient.EnableSsl = isSSL;
                _emailClient.Host = _emailServer;
                _emailClient.Port = _emailPort;
                _emailClient.UseDefaultCredentials = false;
                //Set up credentials
                credential = new NetworkCredential();
                credential.UserName = _emailUser;
                credential.Password = _emailPassword;
                _emailClient.Credentials = credential;
                _mailGunMessageService = null;
            }
            else
            {
                _mailGunMessageService = new MessageService(mailGunSendAPIKey, true, "api.mailgun.net/v3");
                _mailGunSendDomain = mailGunSendDomain;
                _emailClient = null;
            }
        }

        public async void SendEmail(string userEmail, string email_subject, string email_body, string bcc = null, string[] attachments = null)
        {
            string[] userEmails = new string[1];
            userEmails[0] = userEmail;
            SendEmail(userEmails, email_subject, email_body, bcc, attachments);
        }

        public async void SendEmail(string[] userEmails, string email_subject, string email_body, string bcc = null, string[] attachments = null)
        {
            var logoPath = @"" + _serverPath + "\\Images-Work";
            IMessageBuilder messageBuilder;
            if (_isLocalAnalyst)
            {
                MailAddress toEmail = new MailAddress(userEmails[0]);
                MailAddress fromEmail = new MailAddress(_advisorEmail);
                _emailMessage = new MailMessage(fromEmail, toEmail);
                _emailMessage.IsBodyHtml = true;
                _emailMessage.Subject = email_subject;
                _emailMessage.Body = email_body;
                // Starting from 1 since the 1st userEmail already specified
                for (int email_index = 1; email_index < userEmails.Length; email_index++)
                {
                    _emailMessage.To.Add(new MailAddress(userEmails[email_index]));
                }

                if (attachments != null)
                {
                    foreach (string attachment in attachments)
                    {
                        if (attachment.Length > 0)
                        {
                            _emailMessage.Attachments.Add(GetAttachment(attachment));
                        }
                    }
                }
                ImageInsert("ralogo_email.gif", logoPath + "\\ralogo_email.gif", "image/gif", MediaTypeNames.Image.Gif);
                _emailClient.Send(_emailMessage);
                DisposeAttachments(_emailMessage);
            }
            else
            {
                bool userEmailAdded = false;
                messageBuilder = new MessageBuilder()
                                .SetFromAddress(new Recipient { Email = _advisorEmail });

                foreach (var userEmail in userEmails)
                {
                    if (userEmail.Length > 0)
                    {
                        messageBuilder.AddToRecipient(new Recipient { Email = userEmail });
                        userEmailAdded = true;
                    }
                }
                if (!userEmailAdded)
                {
                    Log.Info("No ToReceipient specified " + email_subject + email_body);
                    return;
                }
                if (bcc != null && bcc.Length > 0)
                {
                    messageBuilder.AddBccRecipient(new Recipient { Email = bcc });
                }
                messageBuilder.SetSubject(email_subject);
                messageBuilder.SetHtmlBody(email_body);
                if (attachments != null)
                {
                    foreach (string attachment in attachments)
                    {
                        if (attachment != null && attachment.Length > 0)
                        {
                            try
                            {
                                messageBuilder.AddAttachment(new FileInfo(attachment));
                            }
                            catch (Exception ex)
                            {
                                Log.InfoFormat("Failed to add attachment: {0}", ex);
                            }
                        }
                    }
                }
                messageBuilder.AddInlineImage(new FileInfo(@"" + logoPath + "\\ralogo_email.gif"));
                messageBuilder.AddInlineImage(new FileInfo(@"" + logoPath + "\\cloud_blue.png"));
                var content = await _mailGunMessageService.SendMessageAsync(_mailGunSendDomain, messageBuilder.GetMessage());
                if (content.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception(content.ToString());
                }
            }
        }


        public async void SendEmailWithEmailDetail(string userEmail, string email_subject, string email_body, EmailContent emailDetail)
        {
            var logoPath = @"" + _serverPath + "\\Images-Work";
            IMessageBuilder messageBuilder;
            if (_isLocalAnalyst)
            {
                MailAddress toEmail = new MailAddress(userEmail);
                MailAddress fromEmail = new MailAddress(_advisorEmail);
                _emailMessage = new MailMessage(fromEmail, toEmail);
                _emailMessage.IsBodyHtml = true;
                _emailMessage.Subject = email_subject;
                _emailMessage.Body = email_body;

                if (emailDetail != null)
                {
                    if (!string.IsNullOrEmpty(emailDetail.PeakCPUBusy))
                        ImageInsert("PeakCPUBusy", @"" + emailDetail.PeakCPUBusy);
                    if (!string.IsNullOrEmpty(emailDetail.PeakCPUQueue))
                        ImageInsert("PeakCPUQueue", @"" + emailDetail.PeakCPUQueue);
                    if (!string.IsNullOrEmpty(emailDetail.CPUBusy))
                        ImageInsert("CPUBusy", @"" + emailDetail.CPUBusy);
                    if (!string.IsNullOrEmpty(emailDetail.ApplicationBusy))
                        ImageInsert("ApplicationBusy", @"" + emailDetail.ApplicationBusy);
                    if (!string.IsNullOrEmpty(emailDetail.CPUBusyForecast))
                        ImageInsert("CPUBusyForecast", @"" + emailDetail.CPUBusyForecast);
                    if (!string.IsNullOrEmpty(emailDetail.CPUQueue))
                        ImageInsert("CPUQueue", @"" + emailDetail.CPUQueue);
                    if (!string.IsNullOrEmpty(emailDetail.IPUBusy))
                        ImageInsert("IPUBusy", @"" + emailDetail.IPUBusy);
                    if (!string.IsNullOrEmpty(emailDetail.IPUQueue))
                        ImageInsert("IPUQueue", @"" + emailDetail.IPUQueue);
                    if (!string.IsNullOrEmpty(emailDetail.HighestDiskQueue))
                        ImageInsert("HighestDiskQueue", @"" + emailDetail.HighestDiskQueue);
                    if (!string.IsNullOrEmpty(emailDetail.HighestProcessBusy))
                        ImageInsert("HighestProcessBusy", @"" + emailDetail.HighestProcessBusy);
                    if (!string.IsNullOrEmpty(emailDetail.HighestProcessQueue))
                        ImageInsert("HighestProcessQueue", @"" + emailDetail.HighestProcessQueue);
                    if (!string.IsNullOrEmpty(emailDetail.Transaction))
                        ImageInsert("Transaction", @"" + emailDetail.Transaction);
                    if (!string.IsNullOrEmpty(emailDetail.Storage))
                        ImageInsert("Storage", @"" + emailDetail.Storage);
                }
                ImageInsert("ralogo_email.gif", logoPath + "\\ralogo_email.gif", "image/gif", MediaTypeNames.Image.Gif);
                _emailClient.Send(_emailMessage);
            }
            else
            {
                messageBuilder = new MessageBuilder()
                                .SetFromAddress(new Recipient { Email = _advisorEmail })
                                .AddToRecipient(new Recipient { Email = userEmail });

                messageBuilder.SetSubject(email_subject);
                messageBuilder.SetHtmlBody(email_body);

                messageBuilder.AddInlineImage(new FileInfo(@"" + logoPath + "\\ralogo_email.gif"));
                messageBuilder.AddInlineImage(new FileInfo(@"" + logoPath + "\\cloud_blue.png"));
                if (emailDetail != null)
                {
                    if (!string.IsNullOrEmpty(emailDetail.PeakCPUBusy))
                        messageBuilder.AddInlineImage(new FileInfo(emailDetail.PeakCPUBusy));
                    if (!string.IsNullOrEmpty(emailDetail.PeakCPUQueue))
                        messageBuilder.AddInlineImage(new FileInfo(emailDetail.PeakCPUQueue));
                    if (!string.IsNullOrEmpty(emailDetail.CPUBusy))
                        messageBuilder.AddInlineImage(new FileInfo(emailDetail.CPUBusy));
                    if (!string.IsNullOrEmpty(emailDetail.ApplicationBusy))
                        messageBuilder.AddInlineImage(new FileInfo(emailDetail.ApplicationBusy));
                    if (!string.IsNullOrEmpty(emailDetail.CPUBusyForecast))
                        messageBuilder.AddInlineImage(new FileInfo(emailDetail.CPUBusyForecast));
                    if (!string.IsNullOrEmpty(emailDetail.CPUQueue))
                        messageBuilder.AddInlineImage(new FileInfo(emailDetail.CPUQueue));
                    if (!string.IsNullOrEmpty(emailDetail.IPUBusy))
                        messageBuilder.AddInlineImage(new FileInfo(emailDetail.IPUBusy));
                    if (!string.IsNullOrEmpty(emailDetail.IPUQueue))
                        messageBuilder.AddInlineImage(new FileInfo(emailDetail.IPUQueue));
                    if (!string.IsNullOrEmpty(emailDetail.HighestDiskQueue))
                        messageBuilder.AddInlineImage(new FileInfo(emailDetail.HighestDiskQueue));
                    if (!string.IsNullOrEmpty(emailDetail.HighestProcessBusy))
                        messageBuilder.AddInlineImage(new FileInfo(emailDetail.HighestProcessBusy));
                    if (!string.IsNullOrEmpty(emailDetail.HighestProcessQueue))
                        messageBuilder.AddInlineImage(new FileInfo(emailDetail.HighestProcessQueue));
                    if (!string.IsNullOrEmpty(emailDetail.Transaction))
                        messageBuilder.AddInlineImage(new FileInfo(emailDetail.Transaction));
                    if (!string.IsNullOrEmpty(emailDetail.Storage))
                        messageBuilder.AddInlineImage(new FileInfo(emailDetail.Storage));
                }

                var content = await _mailGunMessageService.SendMessageAsync(_mailGunSendDomain, messageBuilder.GetMessage());
                if (content.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception(content.ToString());
                }
            }
        }


        private void ImageInsert(string imageName, string path, string contentType = "image/jpg", string mediaType = MediaTypeNames.Image.Jpeg)
        {
            byte[] reader = File.ReadAllBytes(path);
            MemoryStream image1 = new MemoryStream(reader);
            AlternateView av = AlternateView.CreateAlternateViewFromString(_emailMessage.Body, null, MediaTypeNames.Text.Html);

            LinkedResource headerImage = new LinkedResource(image1, mediaType);
            headerImage.ContentId = imageName;
            headerImage.ContentType = new ContentType(contentType);
            av.LinkedResources.Add(headerImage);
            _emailMessage.AlternateViews.Add(av);
        }
        Attachment GetAttachment(string reportSavePath)
        {
            // Create the file attachment for this email message.
            Attachment data = new Attachment(reportSavePath, MediaTypeNames.Application.Octet);
            // Add time stamp information for the file.
            ContentDisposition disposition = data.ContentDisposition;
            disposition.CreationDate = System.IO.File.GetCreationTime(reportSavePath);
            disposition.ModificationDate = System.IO.File.GetLastWriteTime(reportSavePath);
            disposition.ReadDate = System.IO.File.GetLastAccessTime(reportSavePath);
            return data;
        }

        private void DisposeAttachments(MailMessage message)
        {
            foreach (Attachment attachment in message.Attachments)
            {
                attachment.Dispose();
            }
            message.Attachments.Dispose();
            message = null;
        }
    }
}
