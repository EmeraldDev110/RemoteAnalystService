using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class FileEntity
    {
        public virtual int EntityCounterID { get; set;}
        public virtual string SystemName { get; set; }
        public virtual int OpenerCpu { get; set; }
        public virtual string OSLetter { get; set; }
        public virtual int OSNumber { get; set; }
        public virtual DateTime FromTimestamp { get; set; }
        public virtual DateTime ToTimestamp { get; set; }
        public virtual double DeltaTime { get; set; }
        public virtual double FileBusyTime { get; set; }
        public virtual double Reads { get; set; }
        public virtual double Writes { get; set; }
        public virtual double UpdatesOrReplies { get; set; }
        public virtual double DeletesOrWriteReads { get; set; }
        public virtual double InfoCalls { get; set; }
        public virtual double RecordsUsed { get; set; }
        public virtual double RecordsAccessed { get; set; }
        public virtual double DiscReads { get; set; }
        public virtual double Messages { get; set; }
        public virtual double MessageBytesF { get; set; }
        public virtual double LockWaits { get; set; }
        public virtual double TimeoutsOrCancels { get; set; }
        public virtual int OpenerPin { get; set; }
        public virtual int FileNumber { get; set; }
        public virtual int FileType { get; set; }
        public virtual int DeviceType { get; set; }
        public virtual string Volume { get; set; }
        public virtual string SubVol { get; set; }
        public virtual string FileName { get; set; }
        public virtual string FileSystemName { get; set; }
        public virtual string OpenerProcessName { get; set; }
        public virtual string OpenerVolume { get; set; }
        public virtual string OpenerSubVol { get; set; }
        public virtual string OpenerFileName { get; set; }
        public virtual string DeviceName { get; set; }
        public virtual string OpenerDeviceName { get; set; }
        public virtual int OpenerOsspid { get; set; }
        public virtual string OpenerPathID { get; set; }
        public virtual string OpenerCrvsn { get; set; }
        public virtual double FromJulian { get; set; }
        public virtual double ToJulian { get; set; }
        public virtual long UniqueID { get; set; }
    }
}
