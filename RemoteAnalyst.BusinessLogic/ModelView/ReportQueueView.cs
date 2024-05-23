using System;

namespace RemoteAnalyst.BusinessLogic.ModelView {
    public class ReportQueueView {
        public int QueueID { get; set; }
        public string FileName { get; set; }
        public int TypeID { get; set; }
        public int Loading { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
