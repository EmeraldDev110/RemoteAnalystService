using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class ProcessAlert
    {
        public virtual int AlertID { get; set; }
        public virtual string System { get; set; }
        public virtual DateTime DateTime { get; set; }
        public virtual int Interval { get; set; }
        public virtual string ProcessName { get; set; }
        public virtual string ProgramName { get; set; }
        public virtual double PercentBusy { get; set; }
        public virtual string AncestorProcessName { get; set; }
        public virtual string AncestorProgramName { get; set; }
        public virtual float Duration { get; set; }
        public override bool Equals(object obj)
        {
            ProcessAlert other = obj as ProcessAlert;
            if (other == null) return false;
            return AlertID == other.AlertID && System == other.System
                && DateTime == other.DateTime && Interval == other.Interval
                && ProcessName == other.ProcessName && PercentBusy == other.PercentBusy;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;
            cachedHashCode = (AlertID + "|" + System + "|" + DateTime
                + "|" + Interval + "|" + ProcessName + "|" + PercentBusy).GetHashCode();
            return cachedHashCode.Value;
        }
    }
}
