using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class DailyDisk
    {
        public virtual string DD_SystemSerialNum { get; set; }
        public virtual DateTime DD_Date { get; set; }
        public virtual string DD_DiskName { get; set; }
        public virtual double DD_UsedGB { get; set; }
        public virtual double DD_DeltaMB { get; set; }
        public virtual double DD_DeltaPercent { get; set; }
        public override bool Equals(object obj)
        {
            DailyDisk other = obj as DailyDisk;
            if (other == null) return false;
            return DD_SystemSerialNum == other.DD_SystemSerialNum && DD_Date == other.DD_Date && DD_DiskName == other.DD_DiskName;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            int hash = 17;
            hash = hash * 23 + DD_SystemSerialNum.GetHashCode();
            hash = hash * 23 + DD_Date.GetHashCode();
            hash = hash * 23 + DD_DiskName.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
