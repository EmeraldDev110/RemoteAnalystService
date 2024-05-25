using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class DISCEntity
    {
        public virtual int EntityCounterID { get; set;}
        public virtual string SystemName { get; set;}
        public virtual int CpuNum { get; set;}
        public virtual string OSLetter { get; set;}
        public virtual int OSNumber { get; set;}
        public virtual DateTime FromTimestamp { get; set;}
        public virtual DateTime ToTimestamp { get; set;}
        public virtual double DeltaTime { get; set;}
        public virtual double RequestQTime { get; set; }
        public virtual double Requests { get; set; }
        public virtual double Reads { get; set; }
        public virtual double Writes { get; set; }
        public virtual double InputBytesF { get; set; }
        public virtual double OutputBytesF { get; set; }
        public virtual double Swaps { get; set; }
        public virtual double ControlPoints { get; set; }
        public virtual double ControlPointWrites { get; set; }
        public virtual double FreeSpaceIos { get; set; }
        public virtual double RequestsBlocked { get; set; }
        public virtual double StartingFreeSpace { get; set; }
        public virtual double EndingFreeSpace { get; set; }
        public virtual long StartingFreeBlocks { get; set; }
        public virtual long EndingFreeBlocks { get; set; }
        public virtual double VolsemQTime { get; set; }
        public virtual double ReadQBusyTime { get; set; }
        public virtual double ReadQTime { get; set; }
        public virtual double WriteQBusyTime { get; set; }
        public virtual double WriteQTime { get; set; }
        public virtual double DeviceQBusyTime { get; set; }
        public virtual double DbioReads { get; set; }
        public virtual double DbioWrites { get; set; }
        public virtual double Defreqs { get; set; }
        public virtual double DefreqQTime { get; set; }
        public virtual double DeferredQTime { get; set; }
        public virtual long CNBlks1 { get; set; }
        public virtual long BlockSize1 { get; set; }
        public virtual long BlockInUseStart1 { get; set; }
        public virtual long BlockInUseEnd1 { get; set; }
        public virtual double BlockSplits1 { get; set; }
        public virtual double Hits1 { get; set; }
        public virtual double Misses1 { get; set; }
        public virtual double Faults1 { get; set; }
        public virtual double AuditBufForces1 { get; set; }
        public virtual double BlksDirtyQTime1 { get; set; }
        public virtual double WriteCleans1 { get; set; }
        public virtual double WriteDirtys1 { get; set; }
        public virtual double WriteMisses1 { get; set; }
        public virtual long CNBlks2 { get; set; }
        public virtual long BlockSize2 { get; set; }
        public virtual long BlockInUseStart2 { get; set; }
        public virtual long BlockInUseEnd2 { get; set; }
        public virtual double BlockSplits2 { get; set; }
        public virtual double Hits2 { get; set; }
        public virtual double Misses2 { get; set; }
        public virtual double Faults2 { get; set; }
        public virtual double AuditBufForces2 { get; set; }
        public virtual double BlksDirtyQTime2 { get; set; }
        public virtual double WriteCleans2 { get; set; }
        public virtual double WriteDirtys2 { get; set; }
        public virtual double WriteMisses2 { get; set; }
        public virtual long CNBlks3 { get; set; }
        public virtual long BlockSize3 { get; set; }
        public virtual long BlockInUseStart3 { get; set; }
        public virtual long BlockInUseEnd3 { get; set; }
        public virtual double BlockSplits3 { get; set; }
        public virtual double Hits3 { get; set; }
        public virtual double Misses3 { get; set; }
        public virtual double Faults3 { get; set; }
        public virtual double AuditBufForces3 { get; set; }
        public virtual double BlksDirtyQTime3 { get; set; }
        public virtual double WriteCleans3 { get; set; }
        public virtual double WriteDirtys3 { get; set; }
        public virtual double WriteMisses3 { get; set; }
        public virtual long CNBlks4 { get; set; }
        public virtual long BlockSize4 { get; set; }
        public virtual long BlockInUseStart4 { get; set; }
        public virtual long BlockInUseEnd4 { get; set; }
        public virtual double BlockSplits4 { get; set; }
        public virtual double Hits4 { get; set; }
        public virtual double Misses4 { get; set; }
        public virtual double Faults4 { get; set; }
        public virtual double AuditBufForces4 { get; set; }
        public virtual double BlksDirtyQTime4 { get; set; }
        public virtual double WriteCleans4 { get; set; }
        public virtual double WriteDirtys4 { get; set; }
        public virtual double WriteMisses4 { get; set; }
        public virtual long CNBlks5 { get; set; }
        public virtual long BlockSize5 { get; set; }
        public virtual long BlockInUseStart5 { get; set; }
        public virtual long BlockInUseEnd5 { get; set; }
        public virtual double BlockSplits5 { get; set; }
        public virtual double Hits5 { get; set; }
        public virtual double Misses5 { get; set; }
        public virtual double Faults5 { get; set; }
        public virtual double AuditBufForces5 { get; set; }
        public virtual double BlksDirtyQTime5 { get; set; }
        public virtual double WriteCleans5 { get; set; }
        public virtual double WriteDirtys5 { get; set; }
        public virtual double WriteMisses5 { get; set; }
        public virtual long CNBlks6 { get; set; }
        public virtual long BlockSize6 { get; set; }
        public virtual long BlockInUseStart6 { get; set; }
        public virtual long BlockInUseEnd6 { get; set; }
        public virtual double BlockSplits6 { get; set; }
        public virtual double Hits6 { get; set; }
        public virtual double Misses6 { get; set; }
        public virtual double Faults6 { get; set; }
        public virtual double AuditBufForces6 { get; set; }
        public virtual double BlksDirtyQTime6 { get; set; }
        public virtual double WriteCleans6 { get; set; }
        public virtual double WriteDirtys6 { get; set; }
        public virtual double WriteMisses6 { get; set; }
        public virtual long CNBlks7 { get; set; }
        public virtual long BlockSize7 { get; set; }
        public virtual long BlockInUseStart7 { get; set; }
        public virtual long BlockInUseEnd7 { get; set; }
        public virtual double BlockSplits7 { get; set; }
        public virtual double Hits7 { get; set; }
        public virtual double Misses7 { get; set; }
        public virtual double Faults7 { get; set; }
        public virtual double AuditBufForces7 { get; set; }
        public virtual double BlksDirtyQTime7 { get; set; }
        public virtual double WriteCleans7 { get; set; }
        public virtual double WriteDirtys7 { get; set; }
        public virtual double WriteMisses7 { get; set; }
        public virtual long CNBlks8 { get; set; }
        public virtual long BlockSize8 { get; set; }
        public virtual long BlockInUseStart8 { get; set; }
        public virtual long BlockInUseEnd8 { get; set; }
        public virtual double BlockSplits8 { get; set; }
        public virtual double Hits8 { get; set; }
        public virtual double Misses8 { get; set; }
        public virtual double Faults8 { get; set; }
        public virtual double AuditBufForces8 { get; set; }
        public virtual double BlksDirtyQTime8 { get; set; }
        public virtual double WriteCleans8 { get; set; }
        public virtual double WriteDirtys8 { get; set; }
        public virtual double WriteMisses8 { get; set; }
        public virtual int Pin { get; set; }
        public virtual int DeviceType { get; set; }
        public virtual int DeviceSubType { get; set; }
        public virtual int Servernet { get; set; }
        public virtual string DeviceName { get; set; }
        public virtual long LogicalDevice { get; set; }
        public virtual long Group { get; set; }
        public virtual long Module { get; set; }
        public virtual long Slot { get; set; }
        public virtual int Meas_Clim_Rel { get; set; }
        public virtual int Meas_Path_Sel { get; set; }
        public virtual int Meas_Clim_Device { get; set; }
        public virtual int Filler1 { get; set; }
        public virtual int Filler2 { get; set; }
        public virtual int Filler3 { get; set; }
        public virtual int Filler4 { get; set; }
        public virtual int Filler5 { get; set; }
        public virtual int Path { get; set; }
        public virtual int Lun { get; set; }
        public virtual int Partition { get; set; }
        public virtual int Target_id { get; set; }
        public virtual string ConfigName { get; set; }
        public virtual string AdapterName { get; set; }
        public virtual string SacName { get; set; }
        public virtual double Capacity { get; set; }
        public virtual string StoragePool { get; set; }
        public virtual int DiscProcessType { get; set; }
        public virtual string Reserved_1 { get; set; }
        public virtual long UniqueID { get; set; }
    }
}
