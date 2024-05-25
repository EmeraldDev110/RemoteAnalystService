using System;
using System.Collections.Generic;
using RemoteAnalyst.Repository.Models;
using RemoteAnalyst.Repository.Helpers;
using NHibernate;
using NHibernate.Criterion;
using System.Data;

namespace RemoteAnalyst.Repository.Repositories
{
    public class CusAnalystRepository
    {
        public IList<int> GetCustomers(int companyID)
        {
            IList<int> Ids = new List<int>();

            using (ISession session = NHibernateHelper.OpenSession("CusAnalyst"))
            {
                var customers = session
                    .CreateCriteria(typeof(CusAnalyst))
                    //.Add(Restrictions.Eq("CompanyID", companyID))
                    .List<CusAnalyst>();
                foreach (var customer in customers)
                {
                    Ids.Add(customer.Id);
                }
            }
            if (Ids.Count == 0)
            {
                Ids.Add(0);
            }
            return Ids;
        }

        public DataTable GetCustomerEmail(int customerID)
        {
            using (ISession session = NHibernateHelper.OpenSession("CusAnalyst"))
            {
                CusAnalyst cusAnalyst = session
                    .CreateCriteria(typeof(CusAnalyst))
                    .Add(Restrictions.Eq("Id", customerID))
                    .UniqueResult<CusAnalyst>();
                return CollectionHelper.ToDataTable(cusAnalyst);
            }
        }

        public string GetEmailAddress(int customerID)
        {
            using (ISession session = NHibernateHelper.OpenSession("CusAnalyst"))
            {
                CusAnalyst cusAnalyst = session
                    .CreateCriteria(typeof(CusAnalyst))
                    .Add(Restrictions.Eq("Id", customerID))
                    .UniqueResult<CusAnalyst>();
                return cusAnalyst != null ? cusAnalyst.Email : "";
            }
        }

        public int GetCustomerID(string customerEmail)
        {
            using (ISession session = NHibernateHelper.OpenSession("CusAnalyst"))
            {
                CusAnalyst cusAnalyst = session
                    .CreateCriteria(typeof(CusAnalyst))
                    .Add(Restrictions.Eq("Email", customerEmail))
                    .UniqueResult<CusAnalyst>();
                return cusAnalyst != null ? cusAnalyst.Id : 0;
            }
        }

        public int GetCompanyID(int customerID)
        {
            using (ISession session = NHibernateHelper.OpenSession("CusAnalyst"))
            {
                CusAnalyst cusAnalyst = session
                    .CreateCriteria(typeof(CusAnalyst))
                    .Add(Restrictions.Eq("Id", customerID))
                    .UniqueResult<CusAnalyst>();
                return cusAnalyst != null ? cusAnalyst.CompanyID : 0;
            }
        }

        public string GetLoginName(int customerID)
        {
            using (ISession session = NHibernateHelper.OpenSession("CusAnalyst"))
            {
                CusAnalyst cusAnalyst = session
                    .CreateCriteria(typeof(CusAnalyst))
                    .Add(Restrictions.Eq("Id", customerID))
                    .UniqueResult<CusAnalyst>();
                return cusAnalyst != null ? cusAnalyst.Login : "";
            }
        }

        public string GetAdminEmail(string systemSerial)
        {
            using (ISession cusAnalystSession = NHibernateHelper.OpenSession("CusAnalyst"))
            {
                using (ISession systemTblSession = NHibernateHelper.OpenSession("System"))
                {
                    Models.System system = systemTblSession
                    .CreateCriteria(typeof(Models.System))
                    .Add(Restrictions.Eq("SystemSerial", systemSerial))
                    .UniqueResult<Models.System>();
                    CusAnalyst cusAnalyst = cusAnalystSession
                        .CreateCriteria(typeof(CusAnalyst))
                        .Add(Restrictions.Eq("CompanyID", system.CompanyID))
                        .Add(Restrictions.Eq("Type", "admn"))
                        .UniqueResult<CusAnalyst>();
                    return cusAnalyst != null ? cusAnalyst.Email : "";
                }
            }
        }

        public string GetUserName(string email)
        {
            using (ISession session = NHibernateHelper.OpenSession("CusAnalyst"))
            {
                CusAnalyst cusAnalyst = session
                    .CreateCriteria(typeof(CusAnalyst))
                    .Add(Restrictions.Eq("Email", email))
                    .UniqueResult<CusAnalyst>();
                return cusAnalyst?.Fname + " " + cusAnalyst?.Lname;
            }
        }
    }
}
