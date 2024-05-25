using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class TrendWalkthrough
    {
        public virtual int TrendID { get; set; }
        public virtual string SystemSerial { get; set; }
        public virtual DateTime FromTimeStamp { get; set; }
        public virtual int EntityID { get; set; }
        public virtual int CpuNum { get; set; }
        public virtual float High { get; set; }
        public virtual float Low { get; set; }
        public virtual float Average { get; set; }
        public override bool Equals(object obj)
        {
            TrendWalkthrough other = obj as TrendWalkthrough;
            if (other == null) return false;
            return TrendID == other.TrendID && SystemSerial == other.SystemSerial
                && FromTimeStamp == other.FromTimeStamp && EntityID == other.EntityID;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            int hash = 17;
            hash = hash * 23 + TrendID.GetHashCode();
            hash = hash * 23 + SystemSerial.GetHashCode();
            hash = hash * 23 + FromTimeStamp.GetHashCode();
            hash = hash * 23 + EntityID.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
