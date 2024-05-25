using System;
using System.Collections.Generic;
using RemoteAnalyst.Repository.Models;
using RemoteAnalyst.Repository.Helpers;
using NHibernate;
using NHibernate.Criterion;
using System.Data;
using System.Linq;

namespace RemoteAnalyst.Repository.Repositories
{
    public class SystemRepository
    {
        public Dictionary<string, string> GetLicenseDate()
        {
            var dicLicense = new Dictionary<string, string>();
            using (ISession session = NHibernateHelper.OpenSession("System"))
            {
                var systems = session
                    .CreateCriteria(typeof(Models.System))
                    .Add(Restrictions.IsNotNull("PlanEndDate"))
                    .List<Models.System>();
                foreach (var system in systems)
                {
                    dicLicense.Add(system.SystemSerial, system.PlanEndDate);
                }
            }
            return dicLicense;
        }

        public string GetCompanyName(string systemSerial)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                var res = session.QueryOver<Company>().JoinQueryOver<Models.System>(c => c.Systems)
                    .Where(s => s.SystemSerial == systemSerial).Select(c => c.CompanyName).SingleOrDefault<string>();
                return res;
            }
        }

        public int GetCompanyID(string systemSerial)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                var res = session.QueryOver<Company>().JoinQueryOver<Models.System>(c => c.Systems)
                    .Where(s => s.SystemSerial == systemSerial).Select(c => c.CompanyID).SingleOrDefault<int>();
                return res;
            }
        }

        public int GetRetentionDay(string systemSerial)
        {
            using (ISession session = NHibernateHelper.OpenSession("System"))
            {
                Models.System res = session
                    .CreateCriteria(typeof(Models.System))
                    .Add(Restrictions.Eq("SystemSerial", systemSerial))
                    .UniqueResult<Models.System>();
                return res.RetentionDay;
            }
        }

        public IDictionary<string, string> GetEndDate(string systemSerial)
        {
            IDictionary<string, string> systemTblServices = new Dictionary<string, string>();
            using (ISession session = NHibernateHelper.OpenSession("System"))
            {
                var res = session
                    .CreateCriteria(typeof(Models.System))
                    .Add(Restrictions.Eq("SystemSerial", systemSerial))
                    .List<Models.System>();
                foreach (Models.System system in res)
                {
                    systemTblServices.Add(system.SystemName, system.PlanEndDate);
                }
            }
            return systemTblServices;
        }

        public string GetMeasFH(string systemSerial)
        {
            using (ISession session = NHibernateHelper.OpenSession("System"))
            {
                Models.System res = session
                    .CreateCriteria(typeof(Models.System))
                    .Add(Restrictions.Eq("SystemSerial", systemSerial))
                    .UniqueResult<Models.System>();
                return res.MEASFH;
            }
        }

        public string GetSystemName(string systemSerial)
        {
            using (ISession session = NHibernateHelper.OpenSession("System"))
            {
                Models.System res = session
                    .CreateCriteria(typeof(Models.System))
                    .Add(Restrictions.Eq("SystemSerial", systemSerial))
                    .UniqueResult<Models.System>();
                return res.SystemName;
            }
        }

        public bool GetAttachmentInEmail(string systemSerial)
        {
            using (ISession session = NHibernateHelper.OpenSession("System"))
            {
                Models.System res = session
                    .CreateCriteria(typeof(Models.System))
                    .Add(Restrictions.Eq("SystemSerial", systemSerial))
                    .UniqueResult<Models.System>();
                return res.AttachmentInEmail;
            }
        }

        public Dictionary<string, string> GetExpiredSystem(bool isLocalAnalyst = false)
        {
            Dictionary<string, string> expiredSystems = new Dictionary<string, string>();
            using (ISession session = NHibernateHelper.OpenSession("System"))
            {
                ICriteria criteria = session
                    .CreateCriteria(typeof(Models.System))
                    .Add(Restrictions.IsNotNull("PlanEndDate"));
                if (!isLocalAnalyst)
                {
                    criteria.Add(Restrictions.Eq("IsNTS", false));
                }
                var res = criteria.List<Models.System>();
                foreach (Models.System system in res)
                {
                    expiredSystems.Add(system.SystemName, system.PlanEndDate);
                }
            }
            return expiredSystems;
        }

        public DataTable GetAllSystems()
        {
            using (ISession session = NHibernateHelper.OpenSession("System"))
            {
                ICollection<Models.System> res = session
                    .CreateCriteria(typeof(Models.System))
                    .List<Models.System>();
                return CollectionHelper.ToDataTable(res);
            }
        }

        public bool AllowOverlappingData(string systemSerial)
        {
            //VISA's CompanyID is 215.
            bool overLapping = false;

            using (ISession session = NHibernateHelper.OpenSession("System"))
            {
                Models.System res = session
                    .CreateCriteria(typeof(Models.System))
                    .Add(Restrictions.Eq("SystemSerial", systemSerial))
                    .Add(Restrictions.Eq("CompanyID", 215))
                    .UniqueResult<Models.System>();
                if (res != null) overLapping = true;
            }

            return overLapping;
        }

        public bool isProcessDirectlySystem(string systemSerial)
        {
            //VISA's CompanyID is 215.
            bool isProcessDirectlySystem = false;

            using (ISession session = NHibernateHelper.OpenSession("System"))
            {
                Models.System res = session
                    .CreateCriteria(typeof(Models.System))
                    .Add(Restrictions.Eq("SystemSerial", systemSerial))
                    .Add(Restrictions.Eq("CompanyID", 215))
                    .UniqueResult<Models.System>();
                if (res != null) isProcessDirectlySystem = true;
            }

            return isProcessDirectlySystem;
        }

        public int GetTimeZone(string systemSerial)
        {
            using (ISession session = NHibernateHelper.OpenSession("System"))
            {
                Models.System res = session
                    .CreateCriteria(typeof(Models.System))
                    .Add(Restrictions.Eq("SystemSerial", systemSerial))
                    .UniqueResult<Models.System>();
                return res.TimeZone;
            }
        }

        public string GetCountryCode(string systemSerial)
        {
            var countryCode = "en-US";
            using (ISession session = NHibernateHelper.OpenSession("System"))
            {
                Models.System res = session
                    .CreateCriteria(typeof(Models.System))
                    .Add(Restrictions.Eq("SystemSerial", systemSerial))
                    .UniqueResult<Models.System>();
                if (res?.CountryCode != null) countryCode = res.CountryCode;
            }
            return countryCode;
        }

        public int GetArchiveRetensionValue(string systemSerial)
        {
            int archiveRetension = 0;
            using (ISession session = NHibernateHelper.OpenSession("System"))
            {
                Models.System res = session
                    .CreateCriteria(typeof(Models.System))
                    .Add(Restrictions.Eq("SystemSerial", systemSerial))
                    .UniqueResult<Models.System>();
                if (res?.ArchiveRetention != null) archiveRetension = res.ArchiveRetention;
            }
            return archiveRetension;
        }

        public int GetTrendMonth(string systemSerial)
        {
            int trendMonths = 0;
            using (ISession session = NHibernateHelper.OpenSession("System"))
            {
                Models.System res = session
                    .CreateCriteria(typeof(Models.System))
                    .Add(Restrictions.Eq("SystemSerial", systemSerial))
                    .UniqueResult<Models.System>();
                if (res?.TrendMonths != null) trendMonths = res.TrendMonths;
            }
            return trendMonths;
        }

        public bool IsNTSSystem(string systemSerial)
        {
            using (ISession session = NHibernateHelper.OpenSession("System"))
            {
                Models.System res = session
                    .CreateCriteria(typeof(Models.System))
                    .Add(Restrictions.Eq("SystemSerial", systemSerial))
                    .UniqueResult<Models.System>();
                return res.IsNTS;
            }
        }

        public DataTable GetTolerance(string systemSerial)
        {
            using (ISession session = NHibernateHelper.OpenSession("System"))
            {
                Models.System res = session
                    .CreateCriteria(typeof(Models.System))
                    .Add(Restrictions.Eq("SystemSerial", systemSerial))
                    .UniqueResult<Models.System>();
                return CollectionHelper.ToDataTable(res, new List<string>() { "BusinessTolerance", "BatchTolerance", "OtherTolerance" });
            }
        }

        public DataTable GetAllCompanySystemSerialAndName()
        {
            Models.System system = null;
            Company company = null;
            using (ISession session = NHibernateHelper.OpenSession())
            {
                IList<object[]> res = session.QueryOver(() => system)
                     .JoinQueryOver(s => s.Company, () => company).Where(c => c.CompanyName != null)
                     .Select(_ => system.SystemSerial, _ => company.CompanyName).List<object[]>();
                List<string> propNames = new List<string>() { "SystemSerial", "CompanyName" };
                return CollectionHelper.ListToDataTable(res, propNames);
            }
        }

        public int GetLoadLimit(string systemSerial)
        {
            using (ISession session = NHibernateHelper.OpenSession("System"))
            {
                Models.System res = session
                    .CreateCriteria(typeof(Models.System))
                    .Add(Restrictions.Eq("SystemSerial", systemSerial))
                    .UniqueResult<Models.System>();
                return res.TimeZone;
            }
        }
    }
}
