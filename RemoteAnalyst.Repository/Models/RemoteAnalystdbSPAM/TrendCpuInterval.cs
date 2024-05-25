using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class TrendCpuInterval
    {
        public virtual DateTime Interval { get; set; }
        public virtual string CpuNumber { get; set; }
        public virtual double CpuBusy { get; set; }
        public virtual double QueueLength { get; set; }
        public virtual double MemoryUsed { get; set; }
        public virtual double Dp2Busy { get; set; }
        public virtual double SwapRate { get; set; }
        public virtual double DispatchRate { get; set; }
        public virtual double PageSizeBytes { get; set; }
        public virtual double MemoryPages32 { get; set; }
        public virtual double Ipus { get; set; }
        public override bool Equals(object obj)
        {
            TrendCpuInterval other = obj as TrendCpuInterval;
            if (other == null) return false;
            return Interval == other.Interval && CpuNumber == other.CpuNumber;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            int hash = 17;
            hash = hash * 23 + Interval.GetHashCode();
            hash = hash * 23 + CpuNumber.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
