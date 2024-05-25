using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.UWSLoader.Core.ModelView {
    public class TableTimestamp {
        public string TableName { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }
    }
}
