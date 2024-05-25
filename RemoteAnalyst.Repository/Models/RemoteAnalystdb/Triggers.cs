using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class Triggers
    {
        public virtual int TriggerId { get; set; }
        public virtual int TriggerType { get; set; }
        public virtual string SystemSerial { get; set; }
        public virtual string FileType { get; set; }
        public virtual string FileLocation { get; set; }
        public virtual int UploadId { get; set; }
        public virtual string Message { get; set; }
        public virtual int CustomerId { get; set; }
        public virtual DateTime InsertDate { get; set; }
        public virtual int UWSID { get; set; }
    }
}
