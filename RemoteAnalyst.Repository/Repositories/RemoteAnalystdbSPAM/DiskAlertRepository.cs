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
    public class DiskAlertRepository
    {
        private readonly string _connectionString;

        public DiskAlertRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void DeleteData(DateTime oldDate)
        {
            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "DiskAlert"))
            {
                session.Query<DiskAlert>()
                    .Where(a => a.DateTime < oldDate)
                    .Delete();
            }
        }
    }
}
