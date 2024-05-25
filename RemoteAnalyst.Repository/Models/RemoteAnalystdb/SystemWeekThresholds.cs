using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class SystemWeekThresholds
    {
        public virtual string SystemSerial { get; set; }
        public virtual int ThresholdTypeId { get; set; }
        public virtual double CPUBusyMinor { get; set; }
        public virtual double CPUBusyMajor { get; set; }
        public virtual double CPUQueueLengthMinor { get; set; }
        public virtual double CPUQueueLengthMajor { get; set; }
        public virtual double IPUBusyMinor { get; set; }
        public virtual double IPUBusyMajor { get; set; }
        public virtual double IPUQueueLengthMinor { get; set; }
        public virtual double IPUQueueLengthMajor { get; set; }
        public virtual double DiskFullMinor { get; set; }
        public virtual double DiskFullMajor { get; set; }
        public virtual double DiskQueueLengthMinor { get; set; }
        public virtual double DiskQueueLengthMajor { get; set; }
        public virtual double DiskDP2Minor { get; set; }
        public virtual double DiskDP2Major { get; set; }
        public virtual double StorageMinor { get; set; }
        public virtual double StorageMajor{ get; set; }

        public override bool Equals(object obj)
        {
            SystemWeekThresholds other = obj as SystemWeekThresholds;
            if (other == null) return false;
            return SystemSerial == other.SystemSerial && ThresholdTypeId == other.ThresholdTypeId;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;
            int hash = 17;
            hash = hash * 23 + SystemSerial.GetHashCode();
            hash = hash * 23 + ThresholdTypeId.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
