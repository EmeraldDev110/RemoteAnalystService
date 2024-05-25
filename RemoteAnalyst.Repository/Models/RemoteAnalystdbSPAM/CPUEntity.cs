using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class CPUEntity
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
        public virtual double CpuQTime { get; set; }
        public virtual double Dispatches { get; set; }
        public virtual double Swaps { get; set; }
        public virtual double IntrBusyTime { get; set; }
        public virtual double ProcessOvhd { get; set; }
        public virtual double DiscIOsF { get; set; }
        public virtual double CacheHitsF { get; set; }
        public virtual double Transactions { get; set; }
        public virtual double ResponseTime { get; set; }
        public virtual double CompTraps { get; set; }
        public virtual double TnsrBusyTime { get; set; }
        public virtual double AccelBusyTime { get; set; }
        public virtual double TnsBusyTime { get; set; }
        public virtual double PageRequests { get; set; }
        public virtual double PageScans { get; set; }
        public virtual double MemPageScans { get; set; }
        public virtual long StartingFreeMem { get; set; }
        public virtual long EndingFreeMem { get; set; }
        public virtual long StartingUCME { get; set; }
        public virtual long EndingUCME { get; set; }
        public virtual long StartingUCL { get; set; }
        public virtual long EndingUCL { get; set; }
        public virtual long StartingSCL { get; set; }
        public virtual long EndingSCL { get; set; }
        public virtual long StartingFreeCIDs { get; set; }
        public virtual long EndingFreeCIDs { get; set; }
        public virtual double UnspPagesQTime { get; set; }
        public virtual long UnspPagesStart { get; set; }
        public virtual long UnspPagesEnd { get; set; }
        public virtual double LinkPrePushMsgs { get; set; }
        public virtual double LinkReadLinkMsgs { get; set; }
        public virtual double LinkLargeMsgs { get; set; }
        public virtual double ReadLinkCacheAll { get; set; }
        public virtual double ReadLinkCacheCtrl { get; set; }
        public virtual double ReadLinkCacheNone { get; set; }
        public virtual double ReplyCtrlCacheMsgs { get; set; }
        public virtual double ProcessHSamples { get; set; }
        public virtual long StartingTimerCells { get; set; }
        public virtual long EndingTimerCells { get; set; }
        public virtual double LockedPagesQtime { get; set; }
        public virtual double LockedPagesStart { get; set; }
        public virtual double LockedPagesEnd { get; set; }
        public virtual double IpuBusyTime1 { get; set; }
        public virtual double IpuQtime1 { get; set; }
        public virtual double IpuDispatches1 { get; set; }
        public virtual double IpuBusyTime2 { get; set; }
        public virtual double IpuQtime2 { get; set; }
        public virtual double IpuDispatches2 { get; set; }
        public virtual double IpuBusyTime3 { get; set; }
        public virtual double IpuQtime3 { get; set; }
        public virtual double IpuDispatches3 { get; set; }
        public virtual double IpuBusyTime4 { get; set; }
        public virtual double IpuQtime4 { get; set; }
        public virtual double IpuDispatches4 { get; set; }
        public virtual double IpuBusyTime5 { get; set; }
        public virtual double IpuQtime5 { get; set; }
        public virtual double IpuDispatches5 { get; set; }
        public virtual double IpuBusyTime6 { get; set; }
        public virtual double IpuQtime6 { get; set; }
        public virtual double IpuDispatches6 { get; set; }
        public virtual double IpuBusyTime7 { get; set; }
        public virtual double IpuQtime7 { get; set; }
        public virtual double IpuDispatches7 { get; set; }
        public virtual double IpuBusyTime8 { get; set; }
        public virtual double IpuQtime8 { get; set; }
        public virtual double IpuDispatches8 { get; set; }
        public virtual double IpuBusyTime9 { get; set; }
        public virtual double IpuQtime9 { get; set; }
        public virtual double IpuDispatches9 { get; set; }
        public virtual double IpuBusyTime10 { get; set; }
        public virtual double IpuQtime10 { get; set; }
        public virtual double IpuDispatches10 { get; set; }
        public virtual double IpuBusyTime11 { get; set; }
        public virtual double IpuQtime11 { get; set; }
        public virtual double IpuDispatches11 { get; set; }
        public virtual double IpuBusyTime12 { get; set; }
        public virtual double IpuQtime12 { get; set; }
        public virtual double IpuDispatches12 { get; set; }
        public virtual double IpuBusyTime13 { get; set; }
        public virtual double IpuQtime13 { get; set; }
        public virtual double IpuDispatches13 { get; set; }
        public virtual double IpuBusyTime14 { get; set; }
        public virtual double IpuQtime14 { get; set; }
        public virtual double IpuDispatches14 { get; set; }
        public virtual double IpuBusyTime15 { get; set; }
        public virtual double IpuQtime15 { get; set; }
        public virtual double IpuDispatches15 { get; set; }
        public virtual double IpuBusyTime16 { get; set; }
        public virtual double IpuQtime16 { get; set; }
        public virtual double IpuDispatches16 { get; set; }
        public virtual double ReadRequests1 { get; set; }
        public virtual double WriteRequests1 { get; set; }
        public virtual double ReadBytes1 { get; set; }
        public virtual double WriteBytes1 { get; set; }
        public virtual double ReadRequests2 { get; set; }
        public virtual double WriteRequests2 { get; set; }
        public virtual double ReadBytes2 { get; set; }
        public virtual double WriteBytes2 { get; set; }
        public virtual double ReadRequests3 { get; set; }
        public virtual double WriteRequests3 { get; set; }
        public virtual double ReadBytes3 { get; set; }
        public virtual double WriteBytes3 { get; set; }
        public virtual double ReadRequests4 { get; set; }
        public virtual double WriteRequests4 { get; set; }
        public virtual double ReadBytes4 { get; set; }
        public virtual double WriteBytes4 { get; set; }
        public virtual double ReadRequests5 { get; set; }
        public virtual double WriteRequests5 { get; set; }
        public virtual double ReadBytes5 { get; set; }
        public virtual double WriteBytes5 { get; set; }
        public virtual double ReadRequests6 { get; set; }
        public virtual double WriteRequests6 { get; set; }
        public virtual double ReadBytes6 { get; set; }
        public virtual double WriteBytes6 { get; set; }
        public virtual double ReadRequests7 { get; set; }
        public virtual double WriteRequests7 { get; set; }
        public virtual double ReadBytes7 { get; set; }
        public virtual double WriteBytes7 { get; set; }
        public virtual double ReadRequests8 { get; set; }
        public virtual double WriteRequests8 { get; set; }
        public virtual double ReadBytes8 { get; set; }
        public virtual double WriteBytes8 { get; set; }
        public virtual double ReadRequests9 { get; set; }
        public virtual double WriteRequests9 { get; set; }
        public virtual double ReadBytes9 { get; set; }
        public virtual double WriteBytes9 { get; set; }
        public virtual double ReadRequests10 { get; set; }
        public virtual double WriteRequests10 { get; set; }
        public virtual double ReadBytes10 { get; set; }
        public virtual double WriteBytes10 { get; set; }
        public virtual double ReadRequests11 { get; set; }
        public virtual double WriteRequests11 { get; set; }
        public virtual double ReadBytes11 { get; set; }
        public virtual double WriteBytes11 { get; set; }
        public virtual double ReadRequests12 { get; set; }
        public virtual double WriteRequests12 { get; set; }
        public virtual double ReadBytes12 { get; set; }
        public virtual double WriteBytes12 { get; set; }
        public virtual double ReadRequests13 { get; set; }
        public virtual double WriteRequests13 { get; set; }
        public virtual double ReadBytes13 { get; set; }
        public virtual double WriteBytes13 { get; set; }
        public virtual double ReadRequests14 { get; set; }
        public virtual double WriteRequests14 { get; set; }
        public virtual double ReadBytes14 { get; set; }
        public virtual double WriteBytes14 { get; set; }
        public virtual double ReadRequests15 { get; set; }
        public virtual double WriteRequests15 { get; set; }
        public virtual double ReadBytes15 { get; set; }
        public virtual double WriteBytes15 { get; set; }
        public virtual double ReadRequests16 { get; set; }
        public virtual double WriteRequests16 { get; set; }
        public virtual double ReadBytes16 { get; set; }
        public virtual double WriteBytes16 { get; set; }
        public virtual int CpuType { get; set; }
        public virtual int CpuSubtype { get; set; }
        public virtual int MemoryPages { get; set; }
        public virtual int Pcbs { get; set; }
        public virtual long PageSizeBytes { get; set; }
        public virtual long MemoryPages32 { get; set; }
        public virtual long MemInitialLock { get; set; }
        public virtual int Ipus { get; set; }
        public virtual string Flags { get; set; }
    }
}
