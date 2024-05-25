using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class AlertRecipient
    {
        public virtual int Id { get; set; }
        public virtual int IdProcessWatchAlerts { get; set; }
        public virtual string SystemSerial { get; set; }
        public virtual string Email { get; set; }
    }
}
