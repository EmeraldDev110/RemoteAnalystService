using System;
using System.Collections.Generic;
using RemoteAnalyst.Repository.Models;
using RemoteAnalyst.Repository.Helpers;
using NHibernate;
using NHibernate.Criterion;
using System.Data;
using System.Linq;
using NHibernate.Linq;
using NHibernate.Transform;

namespace RemoteAnalyst.Repository.Repositories
{
    public class DiskBrowserRepository
    {
        private readonly string _connectionString;

        public DiskBrowserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }


        public DataTable GetTop20Disks(List<string> diskBrowserables, DateTime startTime, DateTime stopTime)
        {
            var diskBusyTable = new DataTable();
            IEnumerable<DiskBrowser> union = null;
            foreach (var diskBrowserable in diskBrowserables)
            {
                using (ISession session = NHibernateHelper.OpenSessionForPartioned("DiskBrowser", diskBrowserable, _connectionString))
                {
                    var res = session.CreateCriteria<DiskBrowser>()
                        .SetMaxResults(20)
                        .Add(Restrictions.Ge("FromTimestamp", startTime))
                        .Add(Restrictions.Lt("FromTimestamp", stopTime))
                        .Add(Restrictions.Gt("ToTimestamp", startTime))
                        .Add(Restrictions.Le("ToTimestamp", stopTime))
                        .AddOrder(Order.Desc(Projections.Property("QueueLength")))
                        .Future<DiskBrowser>();
                    if (union == null) union = res;
                    else union = union.Union(res);
                    ICollection<DiskBrowser> list = union.OrderByDescending(x => x.QueueLength).Take(20).ToList();
                    diskBusyTable = CollectionHelper.ToDataTable(list);
                }
            }

            return diskBusyTable;
        }

        public DataTable GetQueueLength(List<string> diskBrowserables, DateTime startTime, DateTime stopTime)
        {
            var diskBusyTable = new DataTable();
            IEnumerable<DiskBrowser> union = null;
            foreach (var diskBrowserable in diskBrowserables)
            {
                using (ISession session = NHibernateHelper.OpenSessionForPartioned("DiskBrowser", diskBrowserable, _connectionString))
                {
                    var res = session.CreateCriteria<DiskBrowser>()
                        .Add(Restrictions.Ge("FromTimestamp", startTime))
                        .Add(Restrictions.Lt("FromTimestamp", stopTime))
                        .Add(Restrictions.Gt("ToTimestamp", startTime))
                        .Add(Restrictions.Le("ToTimestamp", stopTime))
                        .AddOrder(Order.Asc(Projections.Property("FromTimestamp")))
                        .AddOrder(Order.Asc(Projections.Property("DeviceName")))
                        .Future<DiskBrowser>();
                    if (union == null) union = res;
                    else union = union.Union(res);
                    ICollection<DiskBrowser> list = union.OrderBy(x => x.FromTimestamp).ThenBy(x => x.DeviceName).ToList();
                    diskBusyTable = CollectionHelper.ToDataTable(list);
                }
            }
            return diskBusyTable;
        }

        public DataTable GetDP2Busy(List<string> diskBrowserables, DateTime startTime, DateTime stopTime)
        {

            var diskBusyTable = new DataTable();
            IEnumerable<DiskBrowser> union = null;
            foreach (var diskBrowserable in diskBrowserables)
            {
                using (ISession session = NHibernateHelper.OpenSessionForPartioned("DiskBrowser", diskBrowserable, _connectionString))
                {
                    var res = session.CreateCriteria<DiskBrowser>()
                        .Add(Restrictions.Ge("FromTimestamp", startTime))
                        .Add(Restrictions.Lt("FromTimestamp", stopTime))
                        .Add(Restrictions.Gt("ToTimestamp", startTime))
                        .Add(Restrictions.Le("ToTimestamp", stopTime))
                        .AddOrder(Order.Asc(Projections.Property("FromTimestamp")))
                        .AddOrder(Order.Asc(Projections.Property("DeviceName")))
                        .Future<DiskBrowser>();
                    if (union == null) union = res;
                    else union = union.Union(res);
                    ICollection<DiskBrowser> list = union.OrderBy(x => x.FromTimestamp).ThenBy(x => x.DeviceName).ToList();
                    diskBusyTable = CollectionHelper.ToDataTable(list);
                }
            }
            return diskBusyTable;
        }
    }
}
