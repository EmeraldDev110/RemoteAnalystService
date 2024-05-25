using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class AlertSummary
    {
        public virtual string SystemSerial { get; set; }
        public virtual DateTime AlertDate { get; set; }
        public virtual string DayofWeek { get; set; }
        public virtual int Critical { get; set; }
        public virtual int Major { get; set; }
        public virtual int Minor { get; set; }
        public virtual int Warning { get; set; }
        public virtual int Informational { get; set; }
        public override bool Equals(object obj)
        {
            AlertSummary other = obj as AlertSummary;
            if (other == null) return false;
            return SystemSerial == other.SystemSerial && AlertDate == other.AlertDate;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;
            int hash = 17;
            hash = hash * 23 + SystemSerial.GetHashCode();
            hash = hash * 23 + AlertDate.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
