using System;

namespace RemoteAnalyst.BusinessLogic.ModelView {
    public class ReportDetail {
        public string SystemName { get; set; }
        public string SystemSerial { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string ReportType { get; set; }

        public string OrderBy { get; set; }
    }
}
