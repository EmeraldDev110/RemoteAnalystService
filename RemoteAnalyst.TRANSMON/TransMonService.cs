using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.TransMon.BLL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RemoteAnalyst.TransMon {
    public partial class TransMonService : ServiceBase {

        private int processId;
        private string filePath;
        public TransMonService() {
            InitializeComponent();
        }


        public void Verify(string[] args)
        {
            this.OnStart(args);

            // do something here...  

            this.OnStop();
        }

        protected override void OnStart(string[] args) {
            ReadXML.ImportDataFromXML();

#if (!DEBUG)
            var transMon = new BLL.TransMon();
            var timerRunTransMon = new System.Timers.Timer(1000 * 60 * 15); //Every 15 mins.
            timerRunTransMon.Elapsed += transMon.CheckFiles_Elapsed;
            timerRunTransMon.AutoReset = true;
            timerRunTransMon.Enabled = true;

            var transMonReloader = new BLL.TransMon();
            var timerRunTransMonReload = new System.Timers.Timer(1000 * 60 * 60); //Every 1 hour.
            timerRunTransMonReload.Elapsed += transMonReloader.ReloadFailedFiles_Elapsed;
            timerRunTransMonReload.AutoReset = true;
            timerRunTransMonReload.Enabled = true;

            var storageRep = new BLL.StorageReport();
            var timerRunStorageRep = new System.Timers.Timer(60 * 60 * 1000);
            timerRunStorageRep.Elapsed += storageRep.TimerRunStorageRep_Elapsed;
            timerRunStorageRep.AutoReset = true;
            timerRunStorageRep.Enabled = true;
#endif
        }

        protected override void OnStop()
        {


            //Process process = null;
            //try
            //{
            //    process = Process.GetProcessById((int)processId);
            //}
            //finally
            //{
            //    if (process != null)
            //    {
            //        process.Kill();
            //        process.WaitForExit();
            //        process.Dispose();
            //    }

            //    File.Delete(filePath);
            //}
        }
    }
}
