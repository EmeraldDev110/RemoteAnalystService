using Google.Protobuf.WellKnownTypes;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class TransactionProfiles
    {
        public virtual int TransactionProfileId { get; set; }
        public virtual string SystemSerial { get; set; }
        public virtual string TransactionProfileName { get; set; }
        public virtual string TransactionFile { get; set; }
        public virtual int OpenerType { get; set; }
        public virtual string OpenerName { get; set; }
        public virtual int TransactionCounter { get; set; }
        public virtual float IOTransactionRatio { get; set; }
        public virtual int Retention { get; set; }
        public virtual DateTime UpdatedOn { get; set; }
        public virtual int IsCpuToFile { get; set; }
        public override bool Equals(object obj)
        {
            TransactionProfiles other = obj as TransactionProfiles;
            if (other == null) return false;
            return TransactionProfileId == other.TransactionProfileId && SystemSerial == other.SystemSerial;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            int hash = 17;
            hash = hash * 23 + TransactionProfileId.GetHashCode();
            hash = hash * 23 + SystemSerial.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
