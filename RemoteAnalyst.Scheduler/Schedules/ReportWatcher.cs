using System;
using System.Timers;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;

namespace RemoteAnalyst.Scheduler.Schedules {
    class ReportWatcher {
        public void Timer_Elapsed(object source, ElapsedEventArgs e) {
            CheckReportDownloads();
        }

        public void CheckReportDownloads() {
            if (!ConnectionString.IsLocalAnalyst) {
                var reportDownloadService = new ReportDownloadService(ConnectionString.ConnectionStringDB);
                var reportDownloadIds = reportDownloadService.GetProcessingIdsFor();

                var reportDownloadLogService = new ReportDownloadLogService(ConnectionString.ConnectionStringDB);
				foreach (var reportDownloadId in reportDownloadIds) {
                    //Get datatime.
                    var logDate = reportDownloadLogService.GetFirstLogDateFor(reportDownloadId);

                    if (!logDate.Equals(DateTime.MinValue)) {
                        //Check if the reports has been running over 4 hours.
                        if (logDate < DateTime.Now.AddHours(-4)) {
                            //Send Delay Email to Support.
                            //Get more information.
                            var reportInfo = reportDownloadService.GetReportDetailFor(reportDownloadId);

                            var emailToSupport = new EmailToSupport(ConnectionString.AdvisorEmail, ConnectionString.SupportEmail,
                                ConnectionString.WebSite,
                                ConnectionString.EmailServer, ConnectionString.EmailPort, ConnectionString.EmailUser,
                                ConnectionString.EmailPassword, ConnectionString.EmailAuthentication,
                                ConnectionString.SystemLocation, ConnectionString.ServerPath,
                                ConnectionString.EmailIsSSL,
                                ConnectionString.IsLocalAnalyst, ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                            emailToSupport.SendReportWatcher(reportInfo, reportDownloadId, ConnectionString.ConnectionStringDB);

                        }
                    }
                }
            }
        }
    }
}
