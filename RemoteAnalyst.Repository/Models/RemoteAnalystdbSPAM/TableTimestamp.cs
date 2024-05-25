using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class TableTimestamp
    {
        public virtual string TableName { get; set; }
        public virtual DateTime Start { get; set; }
        public virtual DateTime End { get; set; }
        public virtual int Status { get; set; }
        public virtual string ArchiveID { get; set; }
        public virtual DateTime? CreationDate { get; set; }
        public virtual string FileName { get; set; }
        public override bool Equals(object obj)
        {
            TableTimestamp other = obj as TableTimestamp;
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
