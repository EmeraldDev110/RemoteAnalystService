using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class ReportDownloads
    {
        public virtual int ReportDownloadId { get; set; }
        public virtual string SystemSerial { get; set; }
        public virtual DateTime StartTime { get; set; }
        public virtual DateTime EndTime { get; set; }
        public virtual int TypeID { get; set; }
        public virtual DateTime GenerateDate { get; set; }
        public virtual string FileLocation { get; set; }
        public virtual int Status { get; set; }
        public virtual int OrderBy { get; set; }
        public virtual DateTime RequestDate { get; set; }
    }
}
