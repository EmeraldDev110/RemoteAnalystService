using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.BusinessLogic.ModelView {
    public class RdsView {
        public string RdsName { get; set; }
        public string RdsRealName { get; set; }
        public int Loads { get; set; }
        public double CpuBusy { get; set; }
        public double GbSize { get; set; }
        public double FreeSpace { get; set; }

        //public List<DatabaseMappingInfo> AssignedSystem = new List<DatabaseMappingInfo>();
        public string TodayLoadCount { get; set; }
        public string TodayLoadSize { get; set; }
        public double CpuBusyAverage { get; set; }
        public double CpuBusyPeak { get; set; }
        public string DisplaySpace { get; set; }
    }
}
