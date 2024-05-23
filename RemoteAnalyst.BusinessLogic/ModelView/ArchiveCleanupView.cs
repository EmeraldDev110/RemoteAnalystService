using System;

namespace RemoteAnalyst.BusinessLogic.ModelView {
    public class ArchiveCleanupView {
        public string TableName { get; set; }
        public string ArchiveID { get; set; }
        public DateTime CreationDate { get; set; }
    }
}