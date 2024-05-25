using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class TrendProgramHourly
    {
        public virtual int ProgramProfileId { get; set; }
        public virtual DateTime Hour { get; set; }
        public virtual int CpuNum { get; set; }
        public virtual int Pin { get; set; }
        public virtual string ProcessName { get; set; }
        public virtual string Volume { get; set; }
        public virtual string SubVolume { get; set; }
        public virtual string FileName { get; set; }
        public virtual double PeakCPUBusy { get; set; }
        public virtual double AverageCPUBusy { get; set; }
        public virtual double PeakQueueLength { get; set; }
        public virtual double AverageQueueLength { get; set; }
        public virtual double PeakMsgRecdRate { get; set; }
        public virtual double AverageMsgRecdRate { get; set; }
        public virtual double PeakMsgSentRate { get; set; }
        public virtual double AverageMsgSentRate { get; set; }
        public override bool Equals(object obj)
        {
            TrendProgramHourly other = obj as TrendProgramHourly;
            if (other == null) return false;
            return ProgramProfileId == other.ProgramProfileId && Hour == other.Hour && CpuNum == other.CpuNum
                && Pin == other.Pin && ProcessName == other.ProcessName && Volume == other.Volume
                && SubVolume == other.SubVolume && FileName == other.FileName;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            int hash = 17;
            hash = hash * 23 + ProgramProfileId.GetHashCode();
            hash = hash * 23 + Hour.GetHashCode();
            hash = hash * 23 + CpuNum.GetHashCode();
            hash = hash * 23 + Pin.GetHashCode();
            hash = hash * 23 + ProcessName.GetHashCode();
            hash = hash * 23 + Volume.GetHashCode();
            hash = hash * 23 + SubVolume.GetHashCode();
            hash = hash * 23 + FileName.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
