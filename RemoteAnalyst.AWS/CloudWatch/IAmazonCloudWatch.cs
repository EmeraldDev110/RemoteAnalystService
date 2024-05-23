using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.AWS.CloudWatch {
    internal interface IAmazonCloudWatch {
        double GetRDSCpuBusy(string databaseName);
    }
}
