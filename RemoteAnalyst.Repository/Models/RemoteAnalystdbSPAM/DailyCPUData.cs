using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class DailyCPUData
    {
        public virtual DateTime DateTime { get; set; }
        public virtual int CpuNumber { get; set; }
        public virtual double CPUBusy { get; set; }
        public virtual double CPUQueue { get; set; }
        public override bool Equals(object obj)
        {
            DailyCPUData other = obj as DailyCPUData;
            if (other == null) return false;
            return DateTime == other.DateTime && CpuNumber == other.CpuNumber;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            int hash = 17;
            hash = hash * 23 + DateTime.GetHashCode();
            hash = hash * 23 + CpuNumber.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
