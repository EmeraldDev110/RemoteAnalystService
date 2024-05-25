using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class ZmsBladeDataDictionary
    {
        public virtual int EntityID { get; set; }
        public virtual string ColumnName { get; set; }
        public virtual string ColumnType { get; set; }
        public virtual int ColumnSize { get; set; }
        public virtual int ColumnOrder { get; set; }
        public virtual bool Website { get; set; }
        public override bool Equals(object obj)
        {
            ZmsBladeDataDictionary other = obj as ZmsBladeDataDictionary;
            if (other == null) return false;
            return EntityID == other.EntityID && ColumnName == other.ColumnName && ColumnOrder == other.ColumnOrder;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            int hash = 17;
            hash = hash * 23 + EntityID.GetHashCode();
            hash = hash * 23 + ColumnName.GetHashCode();
            hash = hash * 23 + ColumnOrder.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
