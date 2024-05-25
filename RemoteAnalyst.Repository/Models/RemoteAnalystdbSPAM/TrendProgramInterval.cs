using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class TrendProgramInterval
    {
        public virtual int ProgramProfileId { get; set; }
        public virtual DateTime Interval { get; set; }
        public virtual int CpuNum { get; set; }
        public virtual int Pin { get; set; }
        public virtual string ProcessName { get; set; }
        public virtual string Volume { get; set; }
        public virtual string SubVolume { get; set; }
        public virtual string FileName { get; set; }
        public virtual double CPUBusy { get; set; }
        public virtual double QueueLength { get; set; }
        public virtual double MsgRecdRate { get; set; }
        public virtual double MsgSentRate { get; set; }
        public override bool Equals(object obj)
        {
            TrendProgramInterval other = obj as TrendProgramInterval;
            if (other == null) return false;
            return ProgramProfileId == other.ProgramProfileId && Interval == other.Interval && CpuNum == other.CpuNum
                && Pin == other.Pin && ProcessName == other.ProcessName && Volume == other.Volume
                && SubVolume == other.SubVolume && FileName == other.FileName;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            int hash = 17;
            hash = hash * 23 + ProgramProfileId.GetHashCode();
            hash = hash * 23 + Interval.GetHashCode();
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
