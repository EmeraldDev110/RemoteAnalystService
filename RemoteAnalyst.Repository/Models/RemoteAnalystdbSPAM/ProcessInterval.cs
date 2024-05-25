using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class ProcessInterval
    {
        public virtual string SystemSerialNum { get; set; }
        public virtual DateTime FromTimeStamp { get; set; }
        public virtual DateTime DataDate { get; set; }
        public virtual int Cpu { get; set; }
        public virtual int Pin { get; set; }
        public virtual string ProcessName { get; set; }
        public virtual string ProgramName { get; set; }
        public virtual float CpuBusyTime { get; set; }
        public virtual float DeltaTime { get; set; }
        public virtual string AncestorProcessName { get; set; }
        public virtual string AncestorProgramName { get; set; }
        public virtual int Priority { get; set; }
        public virtual float MemPages { get; set; }
        public virtual int CpuType { get; set; }
        public virtual int CpuSubtype { get; set; }
        public virtual int BCPU { get; set; }
        public override bool Equals(object obj)
        {
            ProcessInterval other = obj as ProcessInterval;
            if (other == null) return false;
            return SystemSerialNum == other.SystemSerialNum && FromTimeStamp == other.FromTimeStamp
                && Cpu == other.Cpu && Pin == other.Pin
                && ProcessName == other.ProcessName && ProgramName == other.ProgramName
                && CpuBusyTime == other.CpuBusyTime && DeltaTime == other.DeltaTime;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;
            cachedHashCode = (SystemSerialNum + "|" + FromTimeStamp + "|" + Cpu
                + "|" + Pin + "|" + ProcessName + "|" + ProgramName
                + "|" + CpuBusyTime + "|" + DeltaTime).GetHashCode();
            return cachedHashCode.Value;
        }
    }
}
