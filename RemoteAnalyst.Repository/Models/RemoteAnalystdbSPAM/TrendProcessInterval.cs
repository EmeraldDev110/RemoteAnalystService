using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class TrendProcessInterval
    {
        public virtual DateTime Interval { get; set; }
        public virtual string CpuNumber { get; set; }
        public virtual string BusiestProcess { get; set; }
        public virtual double ProcessBusy { get; set; }
        public virtual string MemoryHogProcess { get; set; }
        public virtual double MemoryUsedByHog { get; set; }
        public virtual int TransientCount { get; set; }
        public virtual string BusiestProgramFileName { get; set; }
        public virtual string MemoryHogProgramFileName { get; set; }
        public virtual int ProcessCount { get; set; }
        public override bool Equals(object obj)
        {
            TrendProcessInterval other = obj as TrendProcessInterval;
            if (other == null) return false;
            return CpuNumber == other.CpuNumber && Interval == other.Interval;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            int hash = 17;
            hash = hash * 23 + CpuNumber.GetHashCode();
            hash = hash * 23 + Interval.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
