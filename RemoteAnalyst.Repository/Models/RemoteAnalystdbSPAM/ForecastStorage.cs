using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class ForecastStorage
    {
        public virtual DateTime FromTimestamp { get; set; }
        public virtual string DeviceName { get; set; }
        public virtual double UsedPercent { get; set; }
        public virtual double StdDevUsedPercent { get; set; }

        public override bool Equals(object obj)
        {
            ForecastStorage other = obj as ForecastStorage;
            if (other == null) return false;
            return FromTimestamp == other.FromTimestamp && DeviceName == other.DeviceName;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;
            int hash = 17;
            hash = hash * 23 + FromTimestamp.GetHashCode();
            hash = hash * 23 + DeviceName.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
