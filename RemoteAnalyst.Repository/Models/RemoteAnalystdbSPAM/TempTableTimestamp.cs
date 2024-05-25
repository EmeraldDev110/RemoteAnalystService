using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class TempTableTimestamp
    {
        public virtual string TableName { get; set; }
        public virtual DateTime Start { get; set; }
        public virtual DateTime End { get; set; }
        public virtual string FileName { get; set; }
        public override bool Equals(object obj)
        {
            TempTableTimestamp other = obj as TempTableTimestamp;
            if (other == null) return false;
            return TableName == other.TableName && Start == other.Start && End == other.End;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;
            cachedHashCode = (TableName + "|" + Start + "|" + End).GetHashCode();
            return cachedHashCode.Value;
        }
    }
}
