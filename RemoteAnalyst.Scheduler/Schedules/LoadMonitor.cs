using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using RemoteAnalyst.AWS.CloudWatch;
using RemoteAnalyst.AWS.EC2;
using RemoteAnalyst.AWS.RDS;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.Scheduler.Schedules {
    class LoadMonitor {
        private static readonly ILog Log = LogManager.GetLogger("LoadMonitor");

        public void Timer_Elapsed(object source, System.Timers.ElapsedEventArgs e) {
            GetMonitorData();
        }

        public void GetMonitorData() {
            var monitorSerivce = new MonitorService(ConnectionString.ConnectionStringDB, Log);
            monitorSerivce.LoadMonitorData();
        }
    }
}
