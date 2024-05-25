using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class ReportActivity
    {
        public virtual int ReportActivityID { get; set; }
        public virtual DateTime Date { get; set; }
        public virtual string Email { get; set; }
        public virtual string SystemSerial { get; set; }
        public virtual string ReportType { get; set; }
        public virtual string ReportName { get; set; }
        public virtual DateTime PeriodFrom { get; set; }
        public virtual DateTime PeriodTo { get; set; }
    }
}
