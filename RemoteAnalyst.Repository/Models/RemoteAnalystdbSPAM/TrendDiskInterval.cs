using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class TrendDiskInterval
    {
        public virtual DateTime Interval { get; set; }
        public virtual double TotalIO { get; set; }
        public virtual string BusiestDisk { get; set; }
        public virtual double IORate { get; set; }
        public virtual double AverageQueueLength { get; set; }
        public virtual string QueuedDisk { get; set; }
        public virtual double QueueLength { get; set; }
        public virtual double AverageCacheHitRate { get; set; }
        public virtual string TrashingDisk { get; set; }
        public virtual double CacheHitRate { get; set; }
        public virtual double AverageDp2Busy { get; set; }
        public virtual string BusiestDp2 { get; set; }
        public virtual double Dp2Busy { get; set; }
        public virtual double AverageRequestART { get; set; }
        public virtual string HighestARTDisk { get; set; }
        public virtual double ART { get; set; }
        public virtual double AverageDeviceQBusy { get; set; }
        public virtual string BusiestQDevice { get; set; }
        public virtual double DeviceQBusy { get; set; }
    }
}
