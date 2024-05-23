using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.BusinessLogic.Email {
    public class EmailContent {
        public string Content { get; set; }
        public string CPUBusy { get; set; }
        public string ApplicationBusy { get; set; }
        public string CPUQueue { get; set; }
        public string IPUBusy { get; set; }
        public string IPUQueue { get; set; }

        public string PeakCPUBusy { get; set; }
        public string PeakCPUQueue { get; set; }
        public string ExcelLocation { get; set; }

        public string HighestDiskQueue { get; set; }
        public string HighestProcessBusy { get; set; }
        public string HighestProcessQueue { get; set; }
        public string Transaction { get; set; }
        public string Storage { get; set; }

        public bool HourDrop { get; set; }
        public List<System.DateTime[]> HourDropPeriods { get; set; }
        public string CPUBusyForecast { get; set; }
    }
}
