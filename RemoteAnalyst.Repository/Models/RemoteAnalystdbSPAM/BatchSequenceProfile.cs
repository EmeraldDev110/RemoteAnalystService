using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class BatchSequenceProfile
    {
        public virtual int BatchSequenceProfileId { get; set; }
        public virtual string Name { get; set; }
        public virtual DateTime StartWindowStart { get; set; }
        public virtual DateTime StartWindowEnd { get; set; }
        public virtual string StartWindowDoW { get; set; }
        public virtual DateTime ExpectedFinishBy { get; set; }
        public virtual bool AlertIfDoesNotStartOnTime { get; set; }
        public virtual bool AlertIfOrderNotFollowed { get; set; }
        public virtual bool AlertIfDoesNotFinishOnTime { get; set; }
        public virtual string EmailList { get; set; }
        public virtual string ProgramFiles { get; set; }
        public virtual ICollection<BatchSequenceAlertRecipient> AlertRecipients { get; set; }
        public virtual ICollection<BatchSequenceAlertProgram> AlertPrograms { get; set; }
    }
}
