using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.Repository.Concrete.Model {
    public class TableTimestampQueryParameter {
        public string DataDate { get; set; }
        public string TableName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public string ArchiveID { get; set; }
        public int Status { get; set; }
    }
}
