using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class BatchSequenceTrend
    {
        public virtual int BatchSequenceProfileId { get; set; }
        public virtual string ProgramFile { get; set; }
        public virtual DateTime DataDate { get; set; }
        public virtual DateTime StartTime { get; set; }
        public virtual DateTime EndTime { get; set; }
        public virtual int Duration { get; set; }
        public override bool Equals(object obj)
        {
            BatchSequenceTrend other = obj as BatchSequenceTrend;
            if (other == null) return false;
            return BatchSequenceProfileId == other.BatchSequenceProfileId && ProgramFile == other.ProgramFile && DataDate == other.DataDate;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            int hash = 17;
            hash = hash * 23 + BatchSequenceProfileId.GetHashCode();
            hash = hash * 23 + ProgramFile.GetHashCode();
            hash = hash * 23 + DataDate.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
