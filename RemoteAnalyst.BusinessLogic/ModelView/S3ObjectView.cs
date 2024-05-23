using System;

namespace RemoteAnalyst.BusinessLogic.ModelView {
    public class S3ObjectView {
        public string key { get; set; }
        public long size { get; set; }
        public DateTime lastModifiedDate { get; set; }
    }
}