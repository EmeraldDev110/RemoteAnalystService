using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class ReportQueuesAWS
    {
        public virtual int QueueID { get; set; }
        public virtual string Message { get; set; }
        public virtual int TypeID { get; set; }
        public virtual int Loading { get; set; }
        public virtual DateTime OrderDate { get; set; }
        public virtual string InstanceID { get; set; }
    }
}
