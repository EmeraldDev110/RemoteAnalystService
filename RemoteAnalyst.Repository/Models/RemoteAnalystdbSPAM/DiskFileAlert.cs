using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class DiskFileAlert
    {
        public virtual int AlertID { get; set; }
        public virtual string System { get; set; }
        public virtual DateTime DateTime { get; set; }
        public virtual int Interval { get; set; }
        public virtual string DiskFile { get; set; }
        public virtual double IO { get; set; }
        public override bool Equals(object obj)
        {
            DiskFileAlert other = obj as DiskFileAlert;
            if (other == null) return false;
            return AlertID == other.AlertID && System == other.System
                && DateTime == other.DateTime && Interval == other.Interval
                && DiskFile == other.DiskFile;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;
            cachedHashCode = (AlertID + "|" + System + "|" + DateTime
                + "|" + Interval + "|" + DiskFile).GetHashCode();
            return cachedHashCode.Value;
        }
    }
}
