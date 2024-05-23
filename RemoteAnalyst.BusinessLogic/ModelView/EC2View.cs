using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.BusinessLogic.ModelView {
    public class EC2View {
        public string InstanceId { get; set; }
        public string EC2Name { get; set; }

        public string InstanceName { get; set; }

        public int Loads { get; set; }

        public double CpuBusy { get; set; }

        public int TodayLoadCount { get; set; }

        public double TodayLoadSize { get; set; }

        public double CpuBusyAverage { get; set; }

        public double CpuBusyPeak { get; set; }
    }
}
