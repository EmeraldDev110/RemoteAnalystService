using System;

namespace RemoteAnalyst.BusinessLogic.ModelView {
    public class ArchiveStatusView{
        public string DataDate { get; set; }
        public string TableName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public string ArchiveID { get; set; }
        public ArchiveStatus.Status Status { get; set; }
    }
}