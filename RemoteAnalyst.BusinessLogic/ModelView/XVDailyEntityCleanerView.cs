using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.BusinessLogic.ModelView {
    public class XVDailyEntityCleanerView {
        public string SystemSerial { get; set; }
        public string SystemName { get; set; }
        public string ConnectionString { get; set; }
        public int TimeZone { get; set; }
    }
}
