using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class TrendTCPSubnetInterval
    {
        public virtual DateTime Interval { get; set; }
        public virtual string SubnetName { get; set; }
        public virtual double TotalBytes { get; set; }
        public override bool Equals(object obj)
        {
            TrendTCPSubnetInterval other = obj as TrendTCPSubnetInterval;
            if (other == null) return false;
            return SubnetName == other.SubnetName && Interval == other.Interval;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            int hash = 17;
            hash = hash * 23 + SubnetName.GetHashCode();
            hash = hash * 23 + Interval.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
