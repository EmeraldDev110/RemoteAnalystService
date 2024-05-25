using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class TrendApplicationInterval
    {
        public virtual DateTime Interval { get; set; }
        public virtual string ApplicationName { get; set; }
        public virtual double CpuBusy { get; set; }
        public virtual double DiskIO { get; set; }
        public override bool Equals(object obj)
        {
            TrendApplicationInterval other = obj as TrendApplicationInterval;
            if (other == null) return false;
            return Interval == other.Interval && ApplicationName == other.ApplicationName;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            int hash = 17;
            hash = hash * 23 + ApplicationName.GetHashCode();
            hash = hash * 23 + Interval.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
