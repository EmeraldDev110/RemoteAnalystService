using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class TrendTCPProcessInterval
    {
        public virtual DateTime Interval { get; set; }
        public virtual string ProcessName { get; set; }
        public virtual double TotalBytes { get; set; }
        public override bool Equals(object obj)
        {
            TrendTCPProcessInterval other = obj as TrendTCPProcessInterval;
            if (other == null) return false;
            return ProcessName == other.ProcessName && Interval == other.Interval;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            int hash = 17;
            hash = hash * 23 + ProcessName.GetHashCode();
            hash = hash * 23 + Interval.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
