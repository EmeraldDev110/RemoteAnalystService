using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class TrendTmfHourly
    {
        public virtual DateTime Hour { get; set; }
        public virtual long BeginTrans { get; set; }
        public virtual long AbortTrans { get; set; }
        public virtual double HomeTransRate { get; set; }
        public virtual double HomeTransART { get; set; }
    }
}
