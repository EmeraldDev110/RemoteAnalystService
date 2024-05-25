using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class CpuAlert
    {
        public virtual int AlertID { get; set; }
        public virtual string System { get; set; }
        public virtual DateTime DateTime { get; set; }
        public virtual int Interval { get; set; }
        public virtual int Cpu { get; set; }
        public virtual double PercentBusy { get; set; }
        public virtual double PercentFreeMem { get; set; }
        public virtual double QLen { get; set; }
        public override bool Equals(object obj)
        {
            CpuAlert other = obj as CpuAlert;
            if (other == null) return false;
            return AlertID == other.AlertID && System == other.System
                && DateTime == other.DateTime && Interval == other.Interval
                && Cpu == other.Cpu;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;
            cachedHashCode = (AlertID + "|" + System + "|" + DateTime
                + "|" + Interval + "|" + Cpu).GetHashCode();
            return cachedHashCode.Value;
        }
    }
}
