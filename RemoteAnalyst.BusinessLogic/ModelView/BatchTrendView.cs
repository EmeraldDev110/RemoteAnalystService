using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.BusinessLogic.ModelView {
    public class BatchTrendView {
        public string FullFileName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime DataDate { get; set; }
        public int Duration { get; set; }
    }
}
