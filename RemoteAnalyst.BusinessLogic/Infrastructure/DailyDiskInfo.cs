using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.BusinessLogic.Infrastructure {
    public class DailyDiskInfo {
        public string DiskName { get; set; }
        public double FreeGB { get; set; }
        public double UsedGB { get; set; }
        public double UsedPercent { get; set; }
        public double Delta1Day { get; set; }
        public double Delta1Week { get; set; }
        public double Delta1Month { get; set; }
        public double FreePercent { get; set; }
    }
}
