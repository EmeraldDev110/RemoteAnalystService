using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.AWS.RDS {
    internal interface IAmazonRDS {
        int GetRDSAllocatedStorage(string databaseName);
    }
}
