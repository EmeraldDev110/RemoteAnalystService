using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class DailySysUnrated
    {
        public virtual string SystemSerialNum { get; set; }
        public virtual DateTime DataDate { get; set; }
        public virtual int AttributeID { get; set; }
        public virtual string Object { get; set; }
        public virtual float AvgVal { get; set; }
        public virtual float Hour0 { get; set; }
        public virtual float Hour1 { get; set; }
        public virtual float Hour2 { get; set; }
        public virtual float Hour3 { get; set; }
        public virtual float Hour4 { get; set; }
        public virtual float Hour5 { get; set; }
        public virtual float Hour6 { get; set; }
        public virtual float Hour7 { get; set; }
        public virtual float Hour8 { get; set; }
        public virtual float Hour9 { get; set; }
        public virtual float Hour10 { get; set; }
        public virtual float Hour11 { get; set; }
        public virtual float Hour12 { get; set; }
        public virtual float Hour13 { get; set; }
        public virtual float Hour14 { get; set; }
        public virtual float Hour15 { get; set; }
        public virtual float Hour16 { get; set; }
        public virtual float Hour17 { get; set; }
        public virtual float Hour18 { get; set; }
        public virtual float Hour19 { get; set; }
        public virtual float Hour20 { get; set; }
        public virtual float Hour21 { get; set; }
        public virtual float Hour22 { get; set; }
        public virtual float Hour23 { get; set; }
        public virtual int PeakHour { get; set; }
        public virtual int NumHours { get; set; }
        public override bool Equals(object obj)
        {
            DailySysUnrated other = obj as DailySysUnrated;
            if (other == null) return false;
            return SystemSerialNum == other.SystemSerialNum && DataDate == other.DataDate && AttributeID == other.AttributeID && Object == other.Object;
        }
        private int? cachedHashCode;

        public override int GetHashCode()
        {
            if (cachedHashCode.HasValue) return cachedHashCode.Value;

            int hash = 17;
            hash = hash * 23 + SystemSerialNum.GetHashCode();
            hash = hash * 23 + DataDate.GetHashCode();
            hash = hash * 23 + AttributeID.GetHashCode();
            hash = hash * 23 + Object.GetHashCode();
            cachedHashCode = hash;
            return cachedHashCode.Value;
        }
    }
}
