using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class CurrentTables
    {
        public virtual string TableName { get; set; }
        public virtual int EntityID { get; set; }
        public virtual string SystemSerial { get; set; }
        public virtual int Interval { get; set; }
        public virtual DateTime DataDate { get; set; }
        public virtual string MeasureVersion { get; set; }
    }
}
