using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class TrendExpandHourly
    {
        public virtual DateTime Hour { get; set; }
        public virtual string DeviceName { get; set; }
        public virtual double PeakTransmitPackets { get; set; }
        public virtual double AverageTransmitPackets { get; set; }
        public virtual double PeakRetransmitPackets { get; set; }
        public virtual double AverageRetransmitPackets { get; set; }
        public override bool Equals(object obj)
        {
            TrendExpandHourly other = obj as TrendExpandHourly;
            if (other == null) return false;
            return Hour == other.Hour && DeviceName == other.DeviceName;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            int hash = 17;
            hash = hash * 23 + Hour.GetHashCode();
            hash = hash * 23 + DeviceName.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
