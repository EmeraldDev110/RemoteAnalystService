using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class TrendDiskHourly
    {
        public virtual DateTime Hour { get; set; }
        public virtual double AverageTotalIO { get; set; }
        public virtual double PeakTotalIO { get; set; }
        public virtual string DiskAtHiPeakIORate { get; set; }
        public virtual double IORateAtHiPeak { get; set; }
        public virtual string DiskAtHiAvgIORate { get; set; }
        public virtual double IORateAtHiAvg { get; set; }
        public virtual string DiskAtHiAvgQueueLength { get; set; }
        public virtual double QueueLengthAtHiAvg { get; set; }
        public virtual string DiskAtHiPeakQueueLength { get; set; }
        public virtual double QueueLengthAtHiPeak { get; set; }
        public virtual string DiskAtHiAvgCacheHitRate { get; set; }
        public virtual double CacheHitRateAtHiAvg { get; set; }
        public virtual string DiskAtHiPeakCacheHitRate { get; set; }
        public virtual double CacheHitRateAtHiPeak { get; set; }
        public virtual string DiskAtHiAvgDP2Busy { get; set; }
        public virtual double DP2BusyAtHiAvg { get; set; }
        public virtual string DiskAtHiPeakDP2Busy { get; set; }
        public virtual double DP2BusyAtHiPeak { get; set; }
        public virtual string DiskAtHiAvgART { get; set; }
        public virtual string DiskAtHiPeakART { get; set; }
        public virtual double ARTAtHiAvg { get; set; }
        public virtual double ARTAtHiPeak { get; set; }
        public virtual string DiskAtHiAvgDeviceQBusy { get; set; }
        public virtual string DiskAtHiPeakDeviceQBusy { get; set; }
        public virtual double DeviceQBusyAtHiAvg { get; set; }
        public virtual double DeviceQBusyAtHiPeak { get; set; }
    }
}
