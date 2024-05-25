using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.UWSLoader.Core.ModelView {
    public class CurrentTables {
        public string TableName { get; set; }
        public int EntityID { get; set; }
        public string SystemSerial { get; set; }
        public long Interval { get; set; }
        public DateTime DataDate { get; set; }
        public string MeasureVersion { get; set; }
    }
}
