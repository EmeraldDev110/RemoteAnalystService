using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class TrendCpuHourly
    {
        public virtual DateTime Hour { get; set; }
        public virtual string CpuNumber { get; set; }
        public virtual double PeakCpuBusy { get; set; }
        public virtual double AverageCpuBusy { get; set; }
        public virtual double PeakQueueLength { get; set; }
        public virtual double AverageQueueLength { get; set; }
        public virtual double PeakMemoryUsed { get; set; }
        public virtual double AverageMemoryUsed { get; set; }
        public virtual double PeakDp2Busy { get; set; }
        public virtual double AverageDp2Busy { get; set; }
        public virtual double PeakSwapRate { get; set; }
        public virtual double AverageSwapRate { get; set; }
        public virtual double PeakDispatchRate { get; set; }
        public virtual double AverageDispatchRate { get; set; }
        public override bool Equals(object obj)
        {
            TrendCpuHourly other = obj as TrendCpuHourly;
            if (other == null) return false;
            return Hour == other.Hour && CpuNumber == other.CpuNumber;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            int hash = 17;
            hash = hash * 23 + Hour.GetHashCode();
            hash = hash * 23 + CpuNumber.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
