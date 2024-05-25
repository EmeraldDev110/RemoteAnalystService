using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.UWSLoader.Core.ModelView {
    public class UWSInfo {
        public DateTime StartDateTime { get; set; }
        public DateTime StopDateTime { get; set; }
        public long Interval { get; set; }
        public List<int> EntityIds { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<int, int> DuplicatedEntityIds { get; set; }

        public List<CurrentTables> CurrentTables { get; set; }

        public List<TableTimestamp> TableTimestamps { get; set; }
    }
}
