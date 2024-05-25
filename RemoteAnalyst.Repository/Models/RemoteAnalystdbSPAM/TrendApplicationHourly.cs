using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class TrendApplicationHourly
    {
        public virtual DateTime Hour { get; set; }
        public virtual string ApplicationName { get; set; }
        public virtual double PeakCpuBusy { get; set; }
        public virtual double AverageCpuBusy { get; set; }
        public virtual double PeakDiskIO { get; set; }
        public virtual double AverageDiskIO { get; set; }
        public override bool Equals(object obj)
        {
            TrendApplicationHourly other = obj as TrendApplicationHourly;
            if (other == null) return false;
            return Hour == other.Hour && ApplicationName == other.ApplicationName;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            int hash = 17;
            hash = hash * 23 + ApplicationName.GetHashCode();
            hash = hash * 23 + Hour.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
