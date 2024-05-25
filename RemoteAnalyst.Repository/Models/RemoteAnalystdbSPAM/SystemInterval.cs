using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class SystemInterval
    {
        public virtual int ID { get; set; }
        public virtual string SystemSerialNum { get; set; }
        public virtual DateTime DataDate { get; set; }
        public virtual int Interval { get; set; }
    }
}
