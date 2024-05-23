using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.BusinessLogic.ModelView {
    public class ExceptionView {
        public DateTime FromTimestamp { get; set; }
        public string EntityId { get; set; }
        public string CounterId { get; set; }
        public string Instance { get; set; }
        public double Actual { get; set; }
        public double Upper { get; set; }
        public double Lower { get; set; }
        public bool DisplayRed { get; set; }
        public bool IsException { get; set; }
    }
}
