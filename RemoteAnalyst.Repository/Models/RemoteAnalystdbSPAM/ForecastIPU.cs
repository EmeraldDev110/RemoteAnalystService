using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class ForecastIPU
    {
        public virtual DateTime FromTimestamp { get; set; }
        public virtual int CpuNumber { get; set; }
        public virtual int IpuNumber { get; set; }
        public virtual double IpuBusy { get; set; }
        public virtual double IpuQueue { get; set; }
        public virtual double StdDevIpuBusy { get; set; }
        public virtual double StdDevIpuQueue { get; set; }

        public override bool Equals(object obj)
        {
            ForecastIPU other = obj as ForecastIPU;
            if (other == null) return false;
            return FromTimestamp == other.FromTimestamp && CpuNumber == other.CpuNumber && IpuNumber == other.IpuNumber;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;
            int hash = 17;
            hash = hash * 23 + FromTimestamp.GetHashCode();
            hash = hash * 23 + CpuNumber.GetHashCode();
            hash = hash * 23 + IpuNumber.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
