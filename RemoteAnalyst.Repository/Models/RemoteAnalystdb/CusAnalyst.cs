using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class CusAnalyst
    {
        public virtual int Id { get; set; }
        public virtual int CompanyID { get; set; }
        public virtual string Fname { get; set; }
        public virtual string Lname { get; set; }
        public virtual string Login { get; set; }
        public virtual string Password { get; set; }
        public virtual string Phone { get; set; }
        public virtual string CellPhone { get; set; }
        public virtual string Email { get; set; }
        public virtual DateTime DateRegistered { get; set; }
        public virtual DateTime DateExpire { get; set; }
        public virtual string Type { get; set; }
        public virtual string CollectorVersion { get; set; }
        public virtual string DateCollectorDownload { get; set; }
        public virtual string Addr1 { get; set; }
        public virtual string Addr2 { get; set; }
        public virtual string City { get; set; }
        public virtual string State { get; set; }
        public virtual string Country { get; set; }
        public virtual string Zipcode { get; set; }
        public virtual string CmpName { get; set; }
        public virtual string CmpPhone { get; set; }
        public virtual string CmpAddr1 { get; set; }
        public virtual string CmpAddr2 { get; set; }
        public virtual string CmpCity { get; set; }
        public virtual string CmpState { get; set; }
        public virtual string CmpZipcode { get; set; }
        public virtual string CmpCountry { get; set; }
        public virtual string CmpSystem { get; set; }
        public virtual string CmpBilling { get; set; }
        public virtual bool Visited { get; set; }
        public virtual int DaysUWS { get; set; }
        public virtual int DaysSample { get; set; }
        public virtual int NumLogins { get; set; }
        public virtual DateTime LastLogin { get; set; }
        public virtual bool Status { get; set; }
        public virtual DateTime Expiredate { get; set; }
        public virtual DateTime LastNewRALogin { get; set; }
        public virtual int ChartFrom { get; set; }
        public virtual int ChartTo { get; set; }
        public virtual bool IsMultiServer { get; set; }
        public virtual int MultiServerDisplayFormat { get; set; }
        public virtual int LogonAttemptCount { get; set; }
        public virtual DateTime LogonLockTime { get; set; }
    }
}
