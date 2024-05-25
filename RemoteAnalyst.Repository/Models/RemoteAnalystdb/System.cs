using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class System
    {
        public virtual string SystemSerial { get; set; }
        public virtual string SystemName { get; set; }
        public virtual int CompanyID { get; set; }
        public virtual DateTime PlanStartDate { get; set; }
        public virtual string PlanEndDate { get; set; }
        public virtual bool PlanRequest { get; set; }
        public virtual int RetentionDay { get; set; }
        public virtual int UWSRetentionDay { get; set; }
        public virtual int TimeZone { get; set; }
        public virtual string CollectorVersion { get; set; }
        public virtual DateTime DateCollectorDownload { get; set; }
        public virtual int CollectorDownloadUser { get; set; }
        public virtual bool ExpertReport { get; set; }
        public virtual int ExpertReportRetentionDay { get; set; }
        public virtual int Evaluation { get; set; }
        public virtual int ArchiveRetention { get; set; }
        public virtual string MEASFH { get; set; }
        public virtual int TrendMonths { get; set; }
        public virtual int Storage { get; set; }
        public virtual int ArchiveRetentionPathway { get; set; }
        public virtual int TrendMonthsPathway { get; set; }
        public virtual int TrendMonthsStorage { get; set; }
        public virtual bool IsNTS { get; set; }
        public virtual float BusinessTolerance { get; set; }
        public virtual float BatchTolerance { get; set; }
        public virtual float OtherTolerance { get; set; }
        public virtual string City { get; set; }
        public virtual int QNMRetentionDay { get; set; }
        public virtual string CountryCode { get; set; }
        public virtual int LoadLimit { get; set; }
        public virtual bool AttachmentInEmail { get; set; }
        public virtual string Notes { get; set; }
        public virtual Company Company { get; set; }
    }
}
