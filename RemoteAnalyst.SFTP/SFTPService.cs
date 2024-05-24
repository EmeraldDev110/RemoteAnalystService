using System;
using System.Diagnostics;
using System.ServiceProcess;

namespace RemoteAnalyst.SFTP {
    public partial class SFTPService : ServiceBase {
        private BLL.SFTP sftp = new BLL.SFTP();
        public SFTPService() {
            InitializeComponent();

            this.ServiceName = "RemoteAnalyst SFTP Server";
            this.EventLog.Source = "RemoteAnalyst";
            this.EventLog.Log = "Application";

            // These Flags set whether or not to handle that specific
            //  type of event. Set to true if you need it, false otherwise.
            this.CanHandlePowerEvent = true;
            this.CanHandleSessionChangeEvent = true;
            this.CanPauseAndContinue = false;
            this.CanShutdown = true;
            this.CanStop = true;

            if (!EventLog.SourceExists("RemoteAnalyst"))
                EventLog.CreateEventSource("RemoteAnalyst", "Application");
        }

        protected override void OnStart(string[] args) {
            try {
                StartJob();
            }
            catch (Exception ex) {
                EventLog.WriteEntry("RemoteAnalyst", ex.Message);
            }
        }

        protected override void OnStop() {
            StopJob();
        }

        private void StartJob() {
            sftp.StartSFTP();
        }

        private void StopJob() {
            sftp.StartSFTP();
        }
    }
}