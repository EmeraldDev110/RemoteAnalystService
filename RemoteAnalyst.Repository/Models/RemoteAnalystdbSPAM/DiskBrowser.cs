using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class DiskBrowser
    {
        public virtual string DeviceName { get; set; }
        public virtual DateTime FromTimestamp { get; set; }
        public virtual DateTime ToTimestamp { get; set; }
        public virtual float PhysicalIORate { get; set; }
        public virtual float LogicalIORate { get; set; }
        public virtual float QueueLength { get; set; }
        public virtual float CacheHitRate { get; set; }
        public virtual float DP2Busy { get; set; }
        public virtual string BusiestFilePhysicalName { get; set; }
        public virtual float BusiestFilePhysicalIO { get; set; }
        public virtual string BusiestFileLogicalName { get; set; }
        public virtual float BusiestFileLogicalIO { get; set; }
        public override bool Equals(object obj)
        {
            DiskBrowser other = obj as DiskBrowser;
            if (other == null) return false;
            return DeviceName == other.DeviceName && FromTimestamp == other.FromTimestamp && ToTimestamp == other.ToTimestamp;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            int hash = 17;
            hash = hash * 23 + DeviceName.GetHashCode();
            hash = hash * 23 + FromTimestamp.GetHashCode();
            hash = hash * 23 + ToTimestamp.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
