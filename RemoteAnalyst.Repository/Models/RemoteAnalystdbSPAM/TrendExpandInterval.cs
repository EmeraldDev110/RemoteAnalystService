using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class TrendExpandInterval
    {
        public virtual DateTime Interval { get; set; }
        public virtual string DeviceName { get; set; }
        public virtual double TransmitPackets { get; set; }
        public virtual double RetransmitPackets { get; set; }
        public override bool Equals(object obj)
        {
            TrendExpandInterval other = obj as TrendExpandInterval;
            if (other == null) return false;
            return Interval == other.Interval && DeviceName == other.DeviceName;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            int hash = 17;
            hash = hash * 23 + Interval.GetHashCode();
            hash = hash * 23 + DeviceName.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
