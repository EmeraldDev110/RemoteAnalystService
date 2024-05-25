using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class RAInfo
    {
        public virtual int ID { get; set; }
        public virtual string QueryKey { get; set; }
        public virtual string QueryValue { get; set; }
        public virtual string Remark { get; set; }
    }
}
