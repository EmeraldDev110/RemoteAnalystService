using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.UWSLoader.Core.Enums {
    public static class  ReturnStatus {
        public enum Types {
            OverLap = 1,
            IntervalMismatch = 2,
            SameStartAndStopTime = 3
        }
    }
}
