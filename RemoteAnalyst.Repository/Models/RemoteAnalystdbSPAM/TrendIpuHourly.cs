using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class TrendIpuHourly
    {
        public virtual DateTime Hour { get; set; }
        public virtual string CpuNumber { get; set; }
        public virtual string IpuNumber { get; set; }
        public virtual double PeakIpuBusy { get; set; }
        public virtual double AverageIpuBusy { get; set; }
        public virtual double PeakQueueLength { get; set; }
        public virtual double AverageQueueLength { get; set; }
        public override bool Equals(object obj)
        {
            TrendIpuHourly other = obj as TrendIpuHourly;
            if (other == null) return false;
            return Hour == other.Hour && CpuNumber == other.CpuNumber && IpuNumber == other.IpuNumber;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            int hash = 17;
            hash = hash * 23 + Hour.GetHashCode();
            hash = hash * 23 + CpuNumber.GetHashCode();
            hash = hash * 23 + IpuNumber.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
