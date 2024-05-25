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
    public class CPUTrendTableRepository
    {
        private readonly string _connectionString;

        public CPUTrendTableRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DataTable GetCPUBusyInterval(string fromTimestamp, string toTimestamp)
        {
            string cmdText = "SELECT `Interval` AS `Date & Time`, " +
                "CpuBusy as Busy, CAST(CpuNumber as UNSIGNED) as CPUNumber FROM TrendCpuInterval WHERE `Interval` >= '" + fromTimestamp + "' AND  `Interval` < '" + toTimestamp + "'";
            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
            {
                var res = session.QueryOver<TrendCpuInterval>()
                    .Where(x => x.Interval >= Convert.ToDateTime(fromTimestamp) && x.Interval < Convert.ToDateTime(toTimestamp))
                    .SelectList(list => list
                        .Select(x => x.Interval)
                        .Select(x => x.CpuBusy)
                        .Select(Projections.Cast(NHibernateUtil.UInt32, Projections.Property<TrendCpuInterval>(x => x.CpuNumber)))
                    ).List<object[]>();
            return CollectionHelper.ListToDataTable(res, new List<string>() { "Date & Time", "Busy", "CPUNumber" });
            }
        }

        public DataTable GetCPUQueueInterval(string fromTimestamp, string toTimestamp)
        {
            string cmdText = "SELECT `Interval` AS `Date & Time`, " +
                "QueueLength as Queue, CAST(CpuNumber as UNSIGNED) as CPUNumber FROM TrendCpuInterval WHERE `Interval` >= '" + fromTimestamp + "' AND  `Interval` < '" + toTimestamp + "'";
            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
            {
                var res = session.QueryOver<TrendCpuInterval>()
                    .Where(x => x.Interval >= Convert.ToDateTime(fromTimestamp) && x.Interval < Convert.ToDateTime(toTimestamp))
                    .SelectList(list => list
                        .Select(x => x.Interval)
                        .Select(x => x.QueueLength)
                        .Select(Projections.Cast(NHibernateUtil.UInt32, Projections.Property<TrendCpuInterval>(x => x.CpuNumber)))
                    ).List<object[]>();
            return CollectionHelper.ListToDataTable(res, new List<string>() { "Date & Time", "Queue", "CPUNumber" });
            }
        }

        public DataTable GetIPUBusyInterval(string fromTimestamp, string toTimestamp)
        {
            string cmdText = "SELECT `Interval` AS `Date & Time`, " +
                "IPUBusy as Busy, CpuNumber as CPUNumber, IpuNumber as IPUNumber " +
                " FROM TrendIpuInterval WHERE `Interval` >= '" + fromTimestamp + "' AND  `Interval` < '" + toTimestamp + "' " +
                " ORDER BY `Interval`, CpuNumber, IpuNumber ";
            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
            {
                var res = session.QueryOver<TrendIpuInterval>()
                    .Where(x => x.Interval >= Convert.ToDateTime(fromTimestamp) && x.Interval < Convert.ToDateTime(toTimestamp))
                    .SelectList(list => list
                        .Select(x => x.Interval)
                        .Select(x => x.IpuBusy)
                        .Select(x => x.CpuNumber)
                        .Select(x => x.IpuNumber)
                    ).OrderBy(x => x.Interval).Asc
                    .ThenBy(x => x.CpuNumber).Asc
                    .ThenBy(x => x.IpuBusy).Asc
                    .List<object[]>();
                return CollectionHelper.ListToDataTable(res, new List<string>() { "Date & Time", "Busy", "CPUNumber", "IPUNumber" });
            }
        }

        public DataTable GetIPUQueueInterval(string fromTimestamp, string toTimestamp)
        {
            string cmdText = "SELECT `Interval` AS `Date & Time`, " +
                "QueueLength as Queue, CpuNumber as CPUNumber, IpuNumber as IPUNumber " +
                " FROM TrendIpuInterval WHERE `Interval` >= '" + fromTimestamp + "' AND  `Interval` < '" + toTimestamp + "' " +
                " ORDER BY `Interval`, CpuNumber, IpuNumber ";
            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
            {
                var res = session.QueryOver<TrendIpuInterval>()
                    .Where(x => x.Interval >= Convert.ToDateTime(fromTimestamp) && x.Interval < Convert.ToDateTime(toTimestamp))
                    .SelectList(list => list
                        .Select(x => x.Interval)
                        .Select(x => x.QueueLength)
                        .Select(x => x.CpuNumber)
                        .Select(x => x.IpuNumber)
                    ).OrderBy(x => x.Interval).Asc
                    .ThenBy(x => x.CpuNumber).Asc
                    .ThenBy(x => x.IpuBusy).Asc
                    .List<object[]>();
                return CollectionHelper.ListToDataTable(res, new List<string>() { "Date & Time", "Queue", "CPUNumber", "IPUNumber" });
            }
        }
    }
}
