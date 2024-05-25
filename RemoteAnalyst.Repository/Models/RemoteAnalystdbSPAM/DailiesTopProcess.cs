using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class DailiesTopProcess
    {
        public virtual int DataType { get; set; }
        public virtual DateTime FromTimestamp { get; set; }
        public virtual int CpuNum { get; set; }
        public virtual int PIN { get; set; }
        public virtual int IpuNum { get; set; }
        public virtual string ProcessName { get; set; }
        public virtual int Priority { get; set; }
        public virtual double Busy { get; set; }
        public virtual string Program { get; set; }
        public virtual double ReceiveQueue { get; set; }
        public virtual double MemUsed { get; set; }
        public virtual string AncestorProcessName { get; set; }
        public virtual int User { get; set; }
        public virtual int Group { get; set; }
        public override bool Equals(object obj)
        {
            DailiesTopProcess other = obj as DailiesTopProcess;
            if (other == null) return false;
            return DataType == other.DataType && FromTimestamp == other.FromTimestamp
                && CpuNum == other.CpuNum && PIN == other.PIN
                && IpuNum == other.IpuNum && ProcessName == other.ProcessName
                && Priority == other.Priority;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;
            cachedHashCode = (DataType + "|" + FromTimestamp + "|" + CpuNum
                + "|" + PIN + "|" + IpuNum + "|" + ProcessName).GetHashCode();
            return cachedHashCode.Value;
        }
    }
}
