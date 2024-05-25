using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class PvCpuOnce
    {
        public virtual DateTime FromTimestamp { get; set; }
        public virtual DateTime ToTimestamp { get; set; }
        public virtual string CpuNumber { get; set; }
        public virtual int ProcessorType { get; set; }
        public virtual string SoftwareVersion { get; set; }
        public virtual int PageSize { get; set; }
        public virtual int MemorySize { get; set; }
        public virtual float LocalTimeOffset { get; set; }
        public virtual float ElapsedTime { get; set; }
        public override bool Equals(object obj)
        {
            PvCpuOnce other = obj as PvCpuOnce;
            if (other == null) return false;
            return FromTimestamp == other.FromTimestamp && ToTimestamp == other.ToTimestamp && CpuNumber == other.CpuNumber;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            int hash = 17;
            hash = hash * 23 + FromTimestamp.GetHashCode();
            hash = hash * 23 + ToTimestamp.GetHashCode();
            hash = hash * 23 + CpuNumber.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
