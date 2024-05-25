using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class TrendTmfInterval
    {
        public virtual DateTime Interval { get; set; }
        public virtual double Complete { get; set; }
        public virtual double Abort { get; set; }
        public virtual long BeginTrans { get; set; }
        public virtual long AbortTrans { get; set; }
        public virtual double HomeTransRate { get; set; }
        public virtual double HomeTransART { get; set; }
    }
}
