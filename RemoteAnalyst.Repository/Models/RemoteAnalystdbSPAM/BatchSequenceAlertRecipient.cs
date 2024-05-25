using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class BatchSequenceAlertRecipient
    {
        public virtual int BatchSequenceProfileId { get; set; }
        public virtual string EmailAddress { get; set; }
        public override bool Equals(object obj)
        {
            BatchSequenceAlertRecipient other = obj as BatchSequenceAlertRecipient;
            if (other == null) return false;
            return BatchSequenceProfileId == other.BatchSequenceProfileId && EmailAddress == other.EmailAddress;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            int hash = 17;
            hash = hash * 23 + BatchSequenceProfileId.GetHashCode();
            hash = hash * 23 + EmailAddress.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
