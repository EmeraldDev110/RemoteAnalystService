using System;
using System.Configuration;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.ReportGenerator.BLL;

namespace RemoteAnalyst.ReportGenerator {
    sealed partial class ReportGeneratorService : ServiceBase {
        public ReportGeneratorService() {
            InitializeComponent();

            ServiceName = "ReportGenerator - Dynamic";
            EventLog.Source = "ReportGenerator - Dynamic";
            EventLog.Log = "Application";

            // These Flags set whether or not to handle that specific
            //  type of event. Set to true if you need it, false otherwise.
            CanHandlePowerEvent = true;
            CanHandleSessionChangeEvent = true;
            CanPauseAndContinue = false;
            CanShutdown = true;
            CanStop = true;

            if (!EventLog.SourceExists("ReportGenerator - Dynamic")) {
                EventLog.CreateEventSource("ReportGenerator - Dynamic", "Application");
            }
        }

        protected override void OnStart(string[] args) {
            try {
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["S3XML"]))
                {
                    EventLog.WriteEntry("ReportGenerator - Dynamic", "Reading from local xml");
                    ReadXML.ImportDataFromXML();
                }
                else {
                    EventLog.WriteEntry("ReportGenerator - Dynamic", "Reading from S3");
                    ReadXML.ImportDataFromXMLS3();
                    EventLog.WriteEntry("ReportGenerator - Dynamic", "MailGunSendAPIKey " + ConnectionString.MailGunSendAPIKey);
                    EventLog.WriteEntry("ReportGenerator - Dynamic", "MailGunSendDomain " + ConnectionString.MailGunSendDomain);
                }

                if (ConnectionString.IsLocalAnalyst) {
                    var jobScheduleLocalAnalyst = new JobScheduleLocalAnalyst();
                    jobScheduleLocalAnalyst.StartTriggerTimers();
                }
                else
                {
                    var sqs = new Amazon();
                    var threadTrend = new Thread(sqs.ReadQueue);
                    threadTrend.IsBackground = true;
                    threadTrend.Start();

                    //With dynamic report generator, do not need this
                    /*var scheduleKill = new JobScheduleAWS();
                    scheduleKill.StartCheckQueue();*/
                }
                /*
                    var watcher = new JobWatcher();
                    string folderPath = ConnectionString.WatchFolder;
                    watcher.StartJobWatch(folderPath);
                 */

                var schedule = new JobSchedules();
                schedule.StartScheduleTimers();
            }
            catch (Exception ex) {
                EventLog.WriteEntry("ReportGenerator - Dynamic", ex.Message);
            }
        }

        protected override void OnStop() {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
        }
    }
}