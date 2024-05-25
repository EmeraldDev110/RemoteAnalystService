using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class ProcessEntity
    {
        public virtual int EntityCounterID { get; set;}
        public virtual string SystemName { get; set;}
        public virtual int CpuNum { get; set;}
        public virtual string OSLetter { get; set;}
        public virtual int OSNumber { get; set;}
        public virtual DateTime FromTimestamp { get; set;}
        public virtual DateTime ToTimestamp { get; set;}
        public virtual double DeltaTime { get; set;}
        public virtual double CpuBusyTime { get; set;}
        public virtual double ReadyTime { get; set;}
        public virtual double MemQtime { get; set;}
        public virtual double Dispatches { get; set;}
        public virtual double PageFaults { get; set;}
        public virtual double PresPagesQTime { get; set;}
        public virtual double RecvQTime { get; set;}
        public virtual double MessagesSent { get; set;}
        public virtual double SentBytesF { get; set;}
        public virtual double ReturnedBytesF { get; set;}
        public virtual double MessagesReceived { get; set;}
        public virtual double ReceivedBytesF { get; set;}
        public virtual double ReplyBytesF { get; set;}
        public virtual double LcbsInUseQTime { get; set;}
        public virtual double CheckPoints { get; set;}
        public virtual double CompTraps { get; set;}
        public virtual double TnsrBusyTime { get; set;}
        public virtual double AccelBusyTime { get; set;}
        public virtual double TnsBusyTime { get; set;}
        public virtual double FileOpenCalls { get; set;}
        public virtual double BeginTrans { get; set;}
        public virtual double AbortTrans { get; set;}
        public virtual int PresPagesStart { get; set;}
        public virtual int PresPagesEnd { get; set;}
        public virtual double OssnsRequests { get; set;}
        public virtual double OssnsWaitTime { get; set;}
        public virtual double OssnsRedirects { get; set;}
        public virtual double Launches { get; set;}
        public virtual double LaunchWaitTime { get; set;}
        public virtual double OpenCloseWaitTime { get; set;}
        public virtual double IpuSwitches { get; set;}
        public virtual int IpuNum { get; set;}
        public virtual int IpuNumPrev { get; set;}
        public virtual double LockedPagesQtime { get; set;}
        public virtual double LockedPagesStart { get; set;}
        public virtual double LockedPagesEnd { get; set;}
        public virtual int Pin { get; set;}
        public virtual int Priority { get; set;}
        public virtual int Group { get; set;}
        public virtual int User { get; set;}
        public virtual string ProcessName { get; set;}
        public virtual string Volume { get; set;}
        public virtual string SubVol { get; set;}
        public virtual string FileName { get; set;}
        public virtual int OssPid { get; set;}
        public virtual int AncestorCpu { get; set;}
        public virtual int AncestorPin { get; set;}
        public virtual string AncestorSysName { get; set;}
        public virtual string AncestorProcessName { get; set;}
        public virtual string DeviceName { get; set;}
        public virtual string PathID { get; set;}
        public virtual string Crvsn { get; set;}
        public virtual int ProgramAccelerated { get; set;}
        public virtual int Ipus { get; set;}
        public virtual string HomeTermSysName { get; set;}
        public virtual string Device { get; set;}
        public virtual string SubDevice { get; set;}
        public virtual string Qualifier { get; set;}
        public virtual long UniqueID { get; set; }
    }
}
