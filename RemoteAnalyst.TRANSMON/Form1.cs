using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Amazon;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Amazon.RDS;
using Amazon.RDS.Model;
using Amazon.Util;
using RemoteAnalyst.AWS.Queue;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.TransMon.BLL;

namespace RemoteAnalyst.TransMon {
    public partial class Form1 : Form {
        private BLL.TransMon transMon;

        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {

            /*//Read XML input
            var read = new ReadXML();
            read.ImportDataFromXML();
            //Create services
            var tMonSchedule = new TMonSchedule(ConnectionString.ConnectionStringDB);
            var tMonTomorrow = new TMonTomorrow(ConnectionString.ConnectionStringDB);
            var tMonScheduleService = new TMonScheduleService(tMonSchedule);
            var tMonTomorrowService = new TMonTomorrowService(tMonTomorrow);
            var systemTblService = new System_tblService(ConnectionString.ConnectionStringDB);
            //Run TranMon
            var transMon = new BLL.TransMon(tMonScheduleService, tMonTomorrowService, systemTblService);
            transMon.RunTransMon();*/
        }

        /*        private void button1_Click(object sender, EventArgs e) {
            var read = new ReadXML();
            read.ImportDataFromXML();
            //Create services
            /*var tMonSchedule = new TMonSchedule(ConnectionString.ConnectionStringDB);
            var tMonTomorrow = new TMonTomorrow(ConnectionString.ConnectionStringDB);
            var tMonScheduleService = new TMonScheduleService(tMonSchedule);
            var tMonTomorrowService = new TMonTomorrowService(tMonTomorrow);#1#
            //Run TranMon
            transMon = new BLL.TransMon();
            transMon.DeleteJobsTomorrow();
            transMon.CreateJobsTomorrow();
        }

        private void button2_Click(object sender, EventArgs e) {
            Thread t = new Thread(transMon.ProcessJobsTomorrow);
            t.IsBackground = true;
            t.Start();
            
        }*/

        private void button3_Click(object sender, EventArgs e) {
            button3.Enabled = false;

            //1. Run transMon - Delete everything in TMonTomorrow then populate with time
            //2. Run scheduler - if time is 23:59, then populate TMonTomorrow
            //3. If TMonTomorrow table is empty, wait.
            ReadXML.ImportDataFromXML();

#if !DEBUG
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
#else
            //var report = new StorageReport();
            //report.SendStorageEmail(DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"));

            var transMon = new BLL.TransMon();
            transMon.CheckFiles_Elapsed();

            //var transMonReload = new BLL.TransMon();
            //transMonReload.ReloadFailedFiles_Elapsed();

            //var totalCpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            //var totalCpuBusy = totalCpu.NextValue();
            //totalCpuBusy = totalCpu.NextValue();
            //transMon = new BLL.TransMon();
            //var cpuBusy = transMon.GetRDSCpuBusy("prod-mysql");
            /*var t = new Thread(transMon.CheckFiles_Elapsed);
            t.IsBackground = true;
            t.Start();*/

#endif
            /*var schedules = new JobSchedules();
            schedules.StartScheduleTimers();
            transMon = new BLL.TransMon();
            var t = new Thread(transMon.RunTransMon);
            t.IsBackground = true;
            t.Start();
            var button = sender as Button;
            if (button != null) {
                button.Enabled = false;
            }*/
        }
    }
}