using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.UWSLoader.Core.Enums {
    static class ArchiveStatus {
        public enum Status {
            Active = 0,
            Archived = 1,
            Download = 2
        }
    }
}
