using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class ProfileDetail
    {
        public virtual int RecordID { get; set; }
        public virtual int ProfileID { get; set; }
        public virtual string ProfileEntity { get; set; }
    }
}
