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
    public class DiskTrendTableRepository
    {
        private readonly string _connectionString;

        public DiskTrendTableRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DataTable GetDiskTrendPerInterval(string fromTimestamp, string toTimestamp)
        {
            string cmdText = "SELECT `Interval` AS `FromTimestamp`, " +
                "QueueLength FROM TrendDiskInterval WHERE `Interval` >= '" + fromTimestamp + "' AND  `Interval` < '" + toTimestamp + "' " +
                "ORDER BY `Interval`";
            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
            {
                var res = session.QueryOver<TrendDiskInterval>()
                    .Where(x => x.Interval >= Convert.ToDateTime(fromTimestamp) && x.Interval < Convert.ToDateTime(toTimestamp))
                    .SelectList(list => list
                        .Select(x => x.Interval)
                        .Select(x => x.QueueLength)
                    ).List<object[]>();
            return CollectionHelper.ListToDataTable(res, new List<string>() { "FromTimestamp", "QueueLength" });
            }
        }
    }
}
