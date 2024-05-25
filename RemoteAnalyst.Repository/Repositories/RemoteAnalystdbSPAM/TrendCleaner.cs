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
    public class TrendCleaner
    {
        private readonly string _connectionString;

        public TrendCleaner(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void DeleteDataFor(string trendTableName, string dateColumnName, DateTime oldDate)
        {
            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
            {
                var type = NHibernateHelper.GetType(trendTableName);
                var res = session.CreateCriteria(type)
                    .Add(Restrictions.Lt(dateColumnName, oldDate))
                    .List();
                using (ITransaction transaction = session.BeginTransaction())
                {
                    foreach (var row in res)
                    {
                        session.Delete(row);
                    }
                    transaction.Commit();
                }
                
            }
        }
    }
}
