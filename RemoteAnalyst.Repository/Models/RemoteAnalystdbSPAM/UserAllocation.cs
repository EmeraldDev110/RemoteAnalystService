using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class UserAllocation
    {
        public virtual string UA_SystemSerialNum { get; set; }
        public virtual string UA_DiskName { get; set; }
        public virtual int UA_Group { get; set; }
        public virtual int UA_User { get; set; }
        public virtual string UA_UserName { get; set; }
        public virtual double UA_UsedMB { get; set; }
        public virtual int UA_FileCount { get; set; }
        public override bool Equals(object obj)
        {
            UserAllocation other = obj as UserAllocation;
            if (other == null) return false;
            return UA_SystemSerialNum == other.UA_SystemSerialNum && UA_DiskName == other.UA_DiskName && UA_Group == other.UA_Group && UA_User == other.UA_User;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            int hash = 17;
            hash = hash * 23 + UA_SystemSerialNum.GetHashCode();
            hash = hash * 23 + UA_DiskName.GetHashCode();
            hash = hash * 23 + UA_Group.GetHashCode();
            hash = hash * 23 + UA_User.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
