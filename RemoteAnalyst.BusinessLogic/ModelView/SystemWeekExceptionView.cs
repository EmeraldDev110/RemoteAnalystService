using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.BusinessLogic.ModelView {
    public class SystemWeekExceptionView {
        public string SystemSerial { get; set; }
        public int EntityId { get; set; }
        public int CounterId { get; set; }
        public int IsRegular { get; set; }
        public int DayOfWeek { get; set; }
        public string Hour { get; set; }
        public double Value { get; set; }
        public int IsChanged { get; set; }
    }
}
