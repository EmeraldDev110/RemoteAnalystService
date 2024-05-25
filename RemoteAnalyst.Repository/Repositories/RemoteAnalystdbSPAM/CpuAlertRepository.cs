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
    public class CpuAlertRepository
    {
        private readonly string _connectionString;

        public CpuAlertRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void DeleteData(DateTime oldDate)
        {
            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "CpuAlert"))
            {
                session.Query<CpuAlert>()
                    .Where(a => a.DateTime < oldDate)
                    .Delete();
            }
        }
    }
}
