using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.UWSRelay.BLL {
    class UWSUploadInfo {
        public string FileName { get; set; }
        public string SystemSerial { get; set; }
        public string LocalPath { get; set; }
        public string Type { get; set; }
        public int UploadId { get; set; }
    }
}
