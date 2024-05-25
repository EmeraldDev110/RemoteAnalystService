using System;
using System.Collections.Generic;
using RemoteAnalyst.Repository.Models;
using RemoteAnalyst.Repository.Helpers;
using NHibernate;
using NHibernate.Criterion;
using System.Data;
using System.Linq;
using NHibernate.Linq;

namespace RemoteAnalyst.Repository.Repositories
{
    public class RAInfoRepository
    {
        public string GetQueryValue(string key)
        {
            using (ISession session = NHibernateHelper.OpenSession("RAInfo"))
            {
                RAInfo res = session
                    .CreateCriteria(typeof(RAInfo))
                    .Add(Restrictions.Eq("QueryKey", key))
                    .UniqueResult<RAInfo>();
                return res.QueryValue ?? string.Empty;
            }
        }

        public int GetMaxQueue(string key)
        {
            using (ISession session = NHibernateHelper.OpenSession("RAInfo"))
            {
                RAInfo res = session
                    .CreateCriteria(typeof(RAInfo))
                    .Add(Restrictions.Eq("QueryKey", key))
                    .UniqueResult<RAInfo>();
                return res != null ? Convert.ToInt32(res.QueryValue) : 1;
            }
        }
        public string GetValue(string key)
        {
            using (ISession session = NHibernateHelper.OpenSession("RAInfo"))
            {
                RAInfo res = session
                    .CreateCriteria(typeof(RAInfo))
                    .Add(Restrictions.Eq("QueryKey", key))
                    .UniqueResult<RAInfo>();
                return res.QueryValue ?? string.Empty;
            }
        }
    }
}
