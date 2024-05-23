using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.BusinessLogic.ModelView {
    public class TransmonView {
        public string SystemSerial { get; set; }
        public int IntervalInMinutes { get; set; }
        public int ExpectedFileCount { get; set; }
        public int LoadedFileCount { get; set; }
        public int InProgressFileCount { get; set; }
        public int AllowanceTimeInMinutes { get; set; }
        public long? TotalFileSize { get; set; }
		public int ResidualFromLastInterval { get; set; }

        public string CompanyName { get; set; }
        public string SystemName { get; set; }

        public bool LoadStarted { get; set; }

    }

    public class DriveView {
        public string VolumeLabel { get; set; }
        public double TotalSize { get; set; }
        public double TotalFreeSpace { get; set; }
        public double PercentUsed { get; set; }
    }
}
