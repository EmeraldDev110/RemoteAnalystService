using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class TrendTCPProcessHourly
    {
        public virtual DateTime Hour { get; set; }
        public virtual string ProcessName { get; set; }
        public virtual double PeakTotalBytes { get; set; }
        public virtual double AverageTotalBytes { get; set; }
        public override bool Equals(object obj)
        {
            TrendTCPProcessHourly other = obj as TrendTCPProcessHourly;
            if (other == null) return false;
            return ProcessName == other.ProcessName && Hour == other.Hour;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            int hash = 17;
            hash = hash * 23 + ProcessName.GetHashCode();
            hash = hash * 23 + Hour.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
