using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.BusinessLogic.ModelView {
    public class TriggerView {
        public int TriggerId { get; set; }
        public int TriggerType { get; set; }
        public string SystemSerial { get; set; }
        public string FileType { get; set; }
        public string FileLocation { get; set; }
        public int UploadId { get; set; }
        public string Message { get; set; }
        public int CustomerId { get; set; }
        public DateTime InsertDate { get; set; }
        public int UwsId { get; set; }
    }
}
