using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.ReportGenerator.ModelView {
    class TrendView {
        public IList<int> DeliveryID { get; set; }
        public IList<string> Report { get; set; }
        public IList<DateTime> StartPeriod { get; set; }
        public IList<DateTime> EndPeriod { get; set; }
        public IList<bool> IsSchedule { get; set; } 
    }
}
