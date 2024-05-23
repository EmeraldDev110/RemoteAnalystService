using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.AWS.Infrastructure {
    public class EC2Information {
        public string InstanceName { get; set; }
        public string InstanceId { get; set; }
        public string InstanceType { get; set; }
    }
}
