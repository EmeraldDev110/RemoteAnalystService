using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.BusinessLogic.Enums {
    public static class FileType {
        public enum Type {
            OSS = 0,
            CPUInfo = 1,
            Storage = 2,
            Pathway = 3,
            System = 4,
            QNM = 5,
            CLIM = 6
        }
    }
}
