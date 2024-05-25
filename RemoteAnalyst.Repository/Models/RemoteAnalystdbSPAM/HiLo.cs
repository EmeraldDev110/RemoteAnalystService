using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class HiLo
    {
        public virtual string SystemSerialNum { get; set; }
        public virtual DateTime DataDate { get; set; }
        public virtual int AttributeID { get; set; }
        public virtual int DataHour { get; set; }
        public virtual float AvgVal { get; set; }
        public virtual float Hi { get; set; }
        public virtual string HiCPU { get; set; }
        public virtual DateTime HiIntv { get; set; }
        public virtual float Lo { get; set; }
        public virtual string LoCPU { get; set; }
        public virtual DateTime LoIntv { get; set; }
        public override bool Equals(object obj)
        {
            HiLo other = obj as HiLo;
            if (other == null) return false;
            return SystemSerialNum == other.SystemSerialNum && DataDate == other.DataDate && AttributeID == other.AttributeID && DataHour == other.DataHour;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            int hash = 17;
            hash = hash * 23 + SystemSerialNum.GetHashCode();
            hash = hash * 23 + DataDate.GetHashCode();
            hash = hash * 23 + AttributeID.GetHashCode();
            hash = hash * 23 + DataHour.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
