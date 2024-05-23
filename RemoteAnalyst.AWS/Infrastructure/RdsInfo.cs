using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.AWS.Infrastructure {
    public class RdsInfo {
        public string RdsName { get; set; }
        public string RdsType { get; set; }
        public bool IsAurora { get; set; }
    }
}
