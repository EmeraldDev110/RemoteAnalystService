using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class DiskAlert
    {
        public virtual int AlertID { get; set; }
        public virtual string System { get; set; }
        public virtual DateTime DateTime { get; set; }
        public virtual int Interval { get; set; }
        public virtual string Disc { get; set; }
        public virtual double IO { get; set; }
        public virtual double IOQLen { get; set; }
        public virtual double PercentDP2Busy { get; set; }
        public virtual double PercentCacheMisses { get; set; }
        public override bool Equals(object obj)
        {
            DiskAlert other = obj as DiskAlert;
            if (other == null) return false;
            return AlertID == other.AlertID && System == other.System
                && DateTime == other.DateTime && Interval == other.Interval
                && Disc == other.Disc;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;
            cachedHashCode = (AlertID + "|" + System + "|" + DateTime
                + "|" + Interval + "|" + Disc).GetHashCode();
            return cachedHashCode.Value;
        }
    }
}
