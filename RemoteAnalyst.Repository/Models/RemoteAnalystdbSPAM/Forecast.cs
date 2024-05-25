using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class Forecast
    {
        public virtual DateTime FromTimestamp { get; set; }
        public virtual int CpuNumber { get; set; }
        public virtual double CPUBusy { get; set; }
        public virtual long MemoryUsed { get; set; }
        public virtual double CPUQueue { get; set; }
        public virtual double StdDevCPUBusy { get; set; }
        public virtual double StdDevMemoryUsed { get; set; }
        public virtual double StdDevCPUQueue { get; set; }

        public override bool Equals(object obj)
        {
            Forecast other = obj as Forecast;
            if (other == null) return false;
            return FromTimestamp == other.FromTimestamp && CpuNumber == other.CpuNumber;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;
            int hash = 17;
            hash = hash * 23 + FromTimestamp.GetHashCode();
            hash = hash * 23 + CpuNumber.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
