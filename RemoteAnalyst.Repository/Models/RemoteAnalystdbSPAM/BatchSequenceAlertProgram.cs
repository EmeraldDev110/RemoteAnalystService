using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class BatchSequenceAlertProgram
    {
        public virtual int BatchSequenceProfileId { get; set; }
        public virtual string ProgramFile { get; set; }
        public virtual int Order { get; set; }
        public virtual ICollection<BatchSequenceTrend> TrendList { get; set; }
        public override bool Equals(object obj)
        {
            BatchSequenceAlertProgram other = obj as BatchSequenceAlertProgram;
            if (other == null) return false;
            return BatchSequenceProfileId == other.BatchSequenceProfileId && ProgramFile == other.ProgramFile;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;
            cachedHashCode = (BatchSequenceProfileId + "|" + ProgramFile).GetHashCode();
            return cachedHashCode.Value;
        }
    }
}
