using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.UWSRelay.BLL;

namespace RemoteAnalyst.UWSRelay
{
    public partial class UWSWatcherServices : ServiceBase {
        //private readonly JobRelay jobWatcher = new JobRelay();
        public UWSWatcherServices()
        {
            InitializeComponent();

            this.ServiceName = "UWSRelay";
            this.EventLog.Source = "UWSRelay";
            this.EventLog.Log = "Application";

            // These Flags set whether or not to handle that specific
            //  type of event. Set to true if you need it, false otherwise.
            this.CanHandlePowerEvent = true;
            this.CanHandleSessionChangeEvent = true;
            this.CanPauseAndContinue = false;
            this.CanShutdown = true;
            this.CanStop = true;

            if (!EventLog.SourceExists("UWSRelay")) {
                EventLog.CreateEventSource("UWSRelay", "Application");
            }
        }

        protected override void OnStart(string[] args) {
            try
            {

                EventLog.WriteEntry("UWSRelay", "On Start");
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["S3XML"])) {
                    EventLog.WriteEntry("UWSRelay", "Read from XML File");
                    ReadXML.ImportDataFromXML();
                }
                else {
                    EventLog.WriteEntry("UWSRelay", "Read from S3 XML File");
                    ReadXML.ImportDataFromXMLS3();
                }
                EventLog.WriteEntry("UWSRelay", 
                        "Started up. XML File Read");
                EventLog.WriteEntry("UWSRelay",
                        "ConnectionString.ConnectionStringDB: " + FTPFile.RemovePassword(ConnectionString.ConnectionStringDB));
                InHouseConfigService inHouseConfigServicee = new InHouseConfigService(ConnectionString.ConnectionStringDB);
                DataTable volumnIpPair = inHouseConfigServicee.GetNonstopVolumnAndIpPair();
                foreach (DataRow dataRow in volumnIpPair.Rows) {
                    ConnectionString.Vols.Add(dataRow["Volumn"].ToString(), dataRow["IP"].ToString());
                }
                ConnectionString.VolumeOrder = 0;

                if (!LicenseService.IsValidProductIndentifierKey(ConnectionString.ConnectionStringDB)) {
                    EventLog.WriteEntry("UWSRelay", "Invalid Product Indentifier Key. Please contact " + ConnectionString.SupportEmail);
                }
                else {
                    ConnectionString.IsLocalAnalyst = LicenseService.IsLocalAnalystOrPMC(ConnectionString.ConnectionStringDB);
                    EventLog.WriteEntry("UWSRelay", "Starting Job Watch ...");
                    StartJobWatch();
                    EventLog.WriteEntry("UWSRelay", "Job Watch started.");
                }
            }
            catch (Exception ex) {
                EventLog.WriteEntry("UWSRelay", ex.Message);
            }
        }

        protected override void OnStop()
        {
            StopJobWatch();
        }

        private void StartJobWatch() {
            //Call jobwatcher class.
            //string folderPath = ConnectionString.WatchFolder;
            //watcher.StartJobWatch(folderPath);
            //jobWatcher.StartJobWatch();

            //Start the FTP Log.
            var scheduler = new Scheduler();
            scheduler.StartScheduleTimers();
        }

        private void StopJobWatch() {
            //jobWatcher.StopJobWatch();
        }
    }
}
