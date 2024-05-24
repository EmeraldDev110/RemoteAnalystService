using System;
using System.Configuration;
using System.Diagnostics;
using System.ServiceProcess;
using RemoteAnalyst.BusinessLogic.Util;
using System.Collections.Generic;
using RemoteAnalyst.AWS.EC2;
using RemoteAnalyst.BusinessLogic.Email;
using System.Linq;
using System.Threading;
using RemoteAnalyst.Scheduler.Data;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using Amazon.EC2.Model;

namespace RemoteAnalyst.Scheduler
{
    partial class SchedulerService : ServiceBase
    {
        static Dictionary<string, EmailAlert> timers = new Dictionary<string, EmailAlert>();
        public SchedulerService()
        {
            InitializeComponent();

            this.ServiceName = "Scheduler";
            this.EventLog.Source = "Scheduler";
            this.EventLog.Log = "Application";

            // These Flags set whether or not to handle that specific
            //  type of event. Set to true if you need it, false otherwise.
            this.CanHandlePowerEvent = true;
            this.CanHandleSessionChangeEvent = true;
            this.CanPauseAndContinue = false;
            this.CanShutdown = true;
            this.CanStop = true;

            if (!EventLog.SourceExists("Scheduler"))
                EventLog.CreateEventSource("Scheduler", "Application");
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["S3XML"]))
                    ReadXML.ImportDataFromXML();
                else {
                    ReadXML.ImportDataFromXMLS3();
                }

                if (!LicenseService.IsValidProductIndentifierKey(ConnectionString.ConnectionStringDB)) {
                    EventLog.WriteEntry("Scheduler", "Invalid Product Indentifier Key. Please contact " + ConnectionString.SupportEmail);
                }
                else {
                    ConnectionString.IsLocalAnalyst = LicenseService.IsLocalAnalystOrPMC(ConnectionString.ConnectionStringDB);
                    var scheduler = new Scheduler();
                    scheduler.StartScheduleTimers();
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Scheduler", ex.Message);
            }
        }

        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
        }

        private static void TriggerHourlyEC2Alert(Object stateInfo)
        {
            if (DateTime.Now.Hour > 22 && DateTime.Now.Minute < 5)
            {
                AutoResetEvent autoEvent = (AutoResetEvent)stateInfo;
                TriggerEmail(timers["instance"].message, timers["instance"].source);
            }
        }

        private static void TriggerEmail(string message, string source)
        {
            //stops triggering email notifications using autoEvent
            var decrypt = new Decrypt();
            EmailToSupport email = new EmailToSupport(ConnectionString.AdvisorEmail, ConnectionString.SupportEmail,
                ConnectionString.WebSite, decrypt.strDESDecrypt(ConnectionString.EmailServer), ConnectionString.EmailPort,
                decrypt.strDESDecrypt(ConnectionString.EmailUser), decrypt.strDESDecrypt(ConnectionString.EmailPassword), true,
                ConnectionString.SystemLocation, ConnectionString.ServerPath, true, ConnectionString.IsLocalAnalyst,
                ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
            email.SendAWSErrorEmail(message, source);
        }

        private static void EC2Alert()
        {
            AmazonEC2 ec2 = new AmazonEC2();
            string[] prefixList = { "S QT", "S DPA", "O QT", "O DPA" };
            List<Instance> instances = AmazonEC2.GetEC2Instances();
            foreach (Instance i in instances)
            {
                foreach (Tag tag in i.Tags)
                {
                    if (tag.Key.Equals("Name", StringComparison.InvariantCultureIgnoreCase) == true)
                    {
                        DateTime launchTime = ec2.GetLaunchTime(i.InstanceId);
                        
                        if ((prefixList.Any(p => tag.Value.StartsWith(p) == true)) && (launchTime < (DateTime.Now - new TimeSpan(24, 0, 0))))
                        {
                            //if there is an issue and a timer has not been created
                            if (!timers.ContainsKey(i.InstanceId))
                            {
                                string message = String.Format("The following EC2 has been running for over 24 hours:\n" + "EC2 Name: {0}\n"
                                    + "EC2 InstanceID: {1}\n" + "EC2 LaunchTime: {2}\n" + "Regional Endpoint: {3}\n", tag.Value, i.InstanceId, launchTime, RemoteAnalyst.AWS.Helper.GetRegionEndpoint());
                                string source = tag.Value;
                                AutoResetEvent autoEvent = new AutoResetEvent(false);
                                Timer emailTimer = new Timer(TriggerHourlyEC2Alert,
                                                       autoEvent, 0, 360000);
                                timers.Add(i.InstanceId, new EmailAlert(message, source, emailTimer, autoEvent));
                                //AutoResetEvent autoEvent = (AutoResetEvent)stateInfo;
                                //TriggerHourlyEC2Alert(message, tag.Value);
                            }
                            //if there is an issue and there is a timer already created, then do not need to do anything

                        } else if (timers.ContainsKey(i.InstanceId)) //no issue anymore, so need to stop timer
                        {
                            timers[i.InstanceId].emailTimer.Dispose();
                            timers.Remove(i.InstanceId);
                        }
                        break;
                    }

                }
            }

        }

        
    }
}