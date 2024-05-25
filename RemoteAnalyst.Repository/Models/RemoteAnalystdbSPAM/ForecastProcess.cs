using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class ForecastProcess
    {
        public virtual DateTime FromTimestamp { get; set; }
        public virtual string ProcessName { get; set; }
        public virtual string CpuNumber { get; set; }
        public virtual int Pin { get; set; }
        public virtual string Volume { get; set; }
        public virtual string SubVol { get; set; }
        public virtual string FileName { get; set; }
        public virtual double ProcessBusy { get; set; }
        public virtual double StdDevProcessBusy { get; set; }
        public virtual double RecvQueueLength { get; set; }
        public virtual double StdDevRecvQueueLength { get; set; }

        public override bool Equals(object obj)
        {
            ForecastProcess other = obj as ForecastProcess;
            if (other == null) return false;
            return FromTimestamp == other.FromTimestamp && ProcessName == other.ProcessName && CpuNumber == other.CpuNumber;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;
            int hash = 17;
            hash = hash * 23 + FromTimestamp.GetHashCode();
            hash = hash * 23 + ProcessName.GetHashCode();
            hash = hash * 23 + CpuNumber.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
