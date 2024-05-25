using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class VProcVersions
    {
        public virtual string VPROCVersion { get; set; }
        public virtual string ClassName { get; set; }
        public virtual string DataDictionary { get; set; }
        public override bool Equals(object obj)
        {
            VProcVersions other = obj as VProcVersions;
            if (other == null) return false;
            return VPROCVersion == other.VPROCVersion && ClassName == other.ClassName && DataDictionary == other.DataDictionary;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            int hash = 17;
            hash = hash * 23 + VPROCVersion.GetHashCode();
            hash = hash * 23 + ClassName.GetHashCode();
            hash = hash * 23 + DataDictionary.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
