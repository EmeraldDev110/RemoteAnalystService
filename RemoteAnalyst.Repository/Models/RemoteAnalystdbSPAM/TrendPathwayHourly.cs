using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class TrendPathwayHourly
    {
        public virtual DateTime Interval { get; set; }
        public virtual string PathwayName { get; set; }
        public virtual double PeakCPUBusy { get; set; }
        public virtual double CpuBusy { get; set; }
        public virtual double PeakLinkmonTransaction { get; set; }
        public virtual double AverageLinkmonTransaction { get; set; }
        public virtual double PeakTCPTransaction { get; set; }
        public virtual double AverageTCPTransaction { get; set; }
        public virtual double ServerTransaction { get; set; }
        public override bool Equals(object obj)
        {
            TrendPathwayHourly other = obj as TrendPathwayHourly;
            if (other == null) return false;
            return Interval == other.Interval && PathwayName == other.PathwayName;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            int hash = 17;
            hash = hash * 23 + PathwayName.GetHashCode();
            hash = hash * 23 + Interval.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
