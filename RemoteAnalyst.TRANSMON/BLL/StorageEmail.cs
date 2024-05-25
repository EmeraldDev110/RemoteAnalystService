using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading;
using RemoteAnalyst.BusinessLogic.ModelView;
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Net.Mime;
using log4net;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.TransMon.BLL {
	class StorageEmail {
        private static readonly ILog Log = LogManager.GetLogger("TransMonLog");
        private readonly string _advisorEmail = ConnectionString.AdvisorEmail;
		private readonly int _emailPort = ConnectionString.EmailPort;
		private readonly string _emailServer = ConnectionString.EmailServer;
		private readonly string _emailUser = ConnectionString.EmailUser;
		private readonly string _emailPassword = ConnectionString.EmailPassword;
		private readonly string _serverPath = ConnectionString.ServerPath;
		private readonly string _supportEmail = ConnectionString.SupportEmail;
		private readonly string _webSite = ConnectionString.WebSite;
		private readonly SmtpClient _emailClient;
		private MailMessage _emailMessage;
		private readonly NetworkCredential credential;

		public StorageEmail() {
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

		public void SendDailySummary(List<StorageAnalysisView> storageAnalysisView, string summaryStartTime, string chartDir) {
			//Force all datetime to be in US format.
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			var emailtext = new StringBuilder();
			var isHighPriority = false;
			MailAddress fromEmail = new MailAddress(_advisorEmail);
			MailAddress toEmail = new MailAddress(_supportEmail);
			_emailMessage = new MailMessage(fromEmail, toEmail);
			_emailMessage.IsBodyHtml = true;
			_emailMessage.Subject = $"RA Storage Report: Daily Summary for {summaryStartTime}";
			_emailMessage.To.Add(toEmail);

			emailtext.Append("<b>Storage Report</b><br />");
			emailtext.Append("<table style='border: 1px solid black;' cellpadding=5 cellspacing=0>");
			// headers
			emailtext.Append(@"<tr style='text-align:center;background-color:#e6e6e6;'>
								<th style='border: 1px solid black;'>System Serial</th>
								<th style='border: 1px solid black;'>System Name</th>
								<th style='border: 1px solid black;'>Company Name</th>
								<th style='border: 1px solid black;'>Active Size (GB)</th>
								<th style='border: 1px solid black;'>Trend Size (GB)</th>
								<th style='border: 1px solid black;'>S3 Size (GB)</th>
							 </tr>");
			// data
			foreach(var view in storageAnalysisView) {
				emailtext.Append($@"<tr>
									<td style='border: 1px solid black;'>{view.SystemSerial}</td>
									<td style='border: 1px solid black;'>{view.SystemName}</td>
									<td style='border: 1px solid black;'>{view.CompanyName}</td>
									<td style='border: 1px solid black;text-align: right;'>{Math.Round((float)view.ActiveSizeInMB / 1024, 2).ToString("#,##0.00")}</td>
									<td style='border: 1px solid black;text-align: right;'>{Math.Round((float)view.TrendSizeInMB / 1024, 2).ToString("#,##0.00")}</td>
									<td style='border: 1px solid black;text-align: right;'>{Math.Round((float)view.S3SizeInMB / 1024, 2).ToString("#,##0.00")}</td>
								 </tr>");
			}
			emailtext.Append("</table><br /><br />");

			emailtext.Append("<b>Weekly Trend:</b><br />");
			emailtext.Append(@"<img src='cid:WeeklyTrendChart' />");

			
			if (isHighPriority)
				_emailMessage.Priority = MailPriority.High;

			_emailMessage.Body = emailtext.ToString();

			//Insert image
			byte[] reader = File.ReadAllBytes(chartDir);
			MemoryStream image1 = new MemoryStream(reader);
			AlternateView av = AlternateView.CreateAlternateViewFromString(_emailMessage.Body, null, MediaTypeNames.Text.Html);

			LinkedResource headerImage = new LinkedResource(image1, MediaTypeNames.Image.Jpeg);
			headerImage.ContentId = "WeeklyTrendChart";
			headerImage.ContentType = new ContentType("image/jpg");
			av.LinkedResources.Add(headerImage);
			_emailMessage.AlternateViews.Add(av);

			try {
				_emailClient.Send(_emailMessage);
			}
			catch (Exception ex) {
                Log.ErrorFormat("SendFileCountEmail error {0}", ex);
			}
		}
	}
}
