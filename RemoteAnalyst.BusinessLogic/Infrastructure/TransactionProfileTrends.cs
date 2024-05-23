using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.BusinessLogic.Infrastructure {
    class TransactionProfileTrends {
        public DateTime FromTimestamp { get; set; }
        public DateTime ToTimestamp { get; set; }
        public double TPS { get; set; }

    }
}
