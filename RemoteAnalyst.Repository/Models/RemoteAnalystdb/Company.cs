using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Models
{
    public class Company
    {
        public virtual int CompanyID { get; set; }
        public virtual string CompanyName { get; set; }
        public virtual string Addr1 { get; set; }
        public virtual string Addr2 { get; set; }
        public virtual string City { get; set; }
        public virtual string State { get; set; }
        public virtual string ZipCode { get; set; }
        public virtual string Country { get; set; }
        public virtual string Phone { get; set; }
        public virtual string Contact { get; set; }
        public virtual string Status { get; set; }
        public virtual string Email { get; set; }
        public virtual string PrimarySysNum { get; set; }
        public virtual string SupportLink { get; set; }
        public virtual string LogoPath { get; set; }
        public virtual string ImagePath { get; set; }
        public virtual bool IsVendor { get; set; }
        public virtual ICollection<CusAnalyst> CusAnalysts { get; set; }
        public virtual ICollection<System> Systems { get; set; }
        public virtual DatabaseMapping DatabaseMapping { get; set; }
    }
}
