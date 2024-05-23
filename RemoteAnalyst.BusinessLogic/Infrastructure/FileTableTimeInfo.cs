using System;

namespace RemoteAnalyst.BusinessLogic.Infrastructure {
    public class FileTableTimeInfo {
        public DateTime StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }

        public long Interval { get; set; }
    }
}
