using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.FTPRelay.BLL;

namespace RemoteAnalyst.FTPRelay {
    partial class FTPRelayService : ServiceBase {
        private readonly JobRelay _jobWatcher = new JobRelay();
        public FTPRelayService() {
            InitializeComponent();

            ServiceName = "FTPRelay";
            EventLog.Source = "FTPRelay";
            EventLog.Log = "Application";

            // These Flags set whether or not to handle that specific
            //  type of event. Set to true if you need it, false otherwise.
            CanHandlePowerEvent = true;
            CanHandleSessionChangeEvent = true;
            CanPauseAndContinue = false;
            CanShutdown = true;
            CanStop = true;

            if (!EventLog.SourceExists("FTPRelay")) {
                EventLog.CreateEventSource("FTPRelay", "Application");
            }
        }

        protected override void OnStart(string[] args) {
            try {
                ReadXML.ImportDataFromXML();
                StartJobWatch();
            }
            catch (Exception ex) {
                EventLog.WriteEntry("FTPRelay", ex.Message);
            }
        }

        protected override void OnStop() {
            StopJobWatch();
        }

        private void StartJobWatch() {
            //var nonStopInfo = new NonStopInfo();
            //nonStopInfo.CheckSystemFolder();

            //Call jobwatcher class.
            _jobWatcher.StartJobWatch();

            //Start the System Folder Watch.
            var systemFolderWatch = new SystemFolderWatch();
            systemFolderWatch.StartSystemFolderWatch();
        }

        private void StopJobWatch() {
            _jobWatcher.StopJobWatch();
        }
    }
}
