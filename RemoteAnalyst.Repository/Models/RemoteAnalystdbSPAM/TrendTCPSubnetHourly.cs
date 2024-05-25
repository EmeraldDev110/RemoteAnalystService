using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class TrendTCPSubnetHourly
    {
        public virtual DateTime Hour { get; set; }
        public virtual string SubnetName { get; set; }
        public virtual double PeakTotalPackets { get; set; }
        public virtual double AverageTotalPackets { get; set; }
        public override bool Equals(object obj)
        {
            TrendTCPSubnetHourly other = obj as TrendTCPSubnetHourly;
            if (other == null) return false;
            return SubnetName == other.SubnetName && Hour == other.Hour;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            int hash = 17;
            hash = hash * 23 + SubnetName.GetHashCode();
            hash = hash * 23 + Hour.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
