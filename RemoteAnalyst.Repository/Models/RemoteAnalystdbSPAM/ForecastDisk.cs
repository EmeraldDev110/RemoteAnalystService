using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class ForecastDisk
    {
        public virtual DateTime FromTimestamp { get; set; }
        public virtual string DeviceName { get; set; }
        public virtual double QueueLength { get; set; }
        public virtual double StdDevQueueLength { get; set; }
        public virtual double DP2Busy { get; set; }
        public virtual double StdDevDP2Busy { get; set; }

        public override bool Equals(object obj)
        {
            ForecastDisk other = obj as ForecastDisk;
            if (other == null) return false;
            return FromTimestamp == other.FromTimestamp && DeviceName == other.DeviceName;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;
            int hash = 17;
            hash = hash * 23 + FromTimestamp.GetHashCode();
            hash = hash * 23 + DeviceName.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
