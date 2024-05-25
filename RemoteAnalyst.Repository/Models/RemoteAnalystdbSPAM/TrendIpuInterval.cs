using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class TrendIpuInterval
    {
        public virtual DateTime Interval { get; set; }
        public virtual string CpuNumber { get; set; }
        public virtual string IpuNumber { get; set; }
        public virtual double IpuBusy { get; set; }
        public virtual double QueueLength { get; set; }
        public override bool Equals(object obj)
        {
            TrendIpuInterval other = obj as TrendIpuInterval;
            if (other == null) return false;
            return Interval == other.Interval && CpuNumber == other.CpuNumber && IpuNumber == other.IpuNumber;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            int hash = 17;
            hash = hash * 23 + Interval.GetHashCode();
            hash = hash * 23 + CpuNumber.GetHashCode();
            hash = hash * 23 + IpuNumber.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
