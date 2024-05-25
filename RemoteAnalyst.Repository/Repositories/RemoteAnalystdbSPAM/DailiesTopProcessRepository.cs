using System;
using System.Collections.Generic;
using RemoteAnalyst.Repository.Models;
using RemoteAnalyst.Repository.Helpers;
using NHibernate;
using NHibernate.Criterion;
using System.Data;

namespace RemoteAnalyst.Repository.Repositories
{
    public class DailiesTopProcessRepository
    {
        private readonly string _connectionString;
        public DailiesTopProcessRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DataTable GetProcessBusyData(DateTime startTime, DateTime stopTime)
        {
            var processBusy = new DataTable();
            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
            {
                ICollection<DailiesTopProcess> res = session.CreateCriteria<DailiesTopProcess>()
                    .SetMaxResults(20)
                    .Add(Restrictions.Eq("DataType", 1))
                    .Add(Restrictions.Ge("FromTimestamp", startTime))
                    .Add(Restrictions.Lt("FromTimestamp", stopTime))
                    .AddOrder(Order.Desc("Busy"))
                    .List<DailiesTopProcess>();
                processBusy = CollectionHelper.ToDataTable(res);
                processBusy.Columns["Busy"].ColumnName = "Busy %";
            }

            return processBusy;
        }

        public DataTable GetProcessQueueData(DateTime startTime, DateTime stopTime)
        {
            var processQueue = new DataTable();

            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
            {
                ICollection<DailiesTopProcess> res = session.CreateCriteria<DailiesTopProcess>()
                    .SetMaxResults(20)
                    .Add(Restrictions.Eq("DataType", 2))
                    .Add(Restrictions.Ge("FromTimestamp", startTime))
                    .Add(Restrictions.Lt("FromTimestamp", stopTime))
                    .AddOrder(Order.Desc("ReceiveQueue"))
                    .List<DailiesTopProcess>();
                processQueue = CollectionHelper.ToDataTable(res);
                processQueue.Columns["Busy"].ColumnName = "Busy %";
            }

            return processQueue;
        }
    }
}
