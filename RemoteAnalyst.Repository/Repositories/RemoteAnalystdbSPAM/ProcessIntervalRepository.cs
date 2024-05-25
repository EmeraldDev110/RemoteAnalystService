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
    public class ProcessIntervalRepository
    {
        private readonly string _connectionString;

        public ProcessIntervalRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void DeleteData(DateTime oldDate)
        {
            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "ProcessInterval"))
            {
                session.Query<ProcessInterval>()
                    .Where(a => a.DataDate < oldDate)
                    .Delete();
            }
        }
    }
}
