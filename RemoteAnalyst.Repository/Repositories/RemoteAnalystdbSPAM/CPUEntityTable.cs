using System;
using System.Collections.Generic;
using RemoteAnalyst.Repository.Models;
using RemoteAnalyst.Repository.Helpers;
using NHibernate;
using NHibernate.Criterion;
using System.Data;
using System.Linq;
using NHibernate.Linq;
using NHibernate.Criterion.Lambda;

namespace RemoteAnalyst.Repository.Repositories
{
    public class CPUEntityTable
    {
        private readonly string _connectionString;

        public CPUEntityTable(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DataTable GetCPUEntityTableIntervalList(string entityTableName)
        {
            var intervalList = new DataTable();
            try
            {
                string cmdText = "SELECT DISTINCT DeviceName FROM " + entityTableName;
                using (ISession session = NHibernateHelper.OpenSessionForPartioned("CPUEntity", entityTableName, _connectionString))
                {
                    var res = session.CreateCriteria<CPUEntity>()
                        .SetProjection(Projections.Distinct(Projections.ProjectionList()
                            .Add(Projections.Property("FromTimestamp"))
                            .Add(Projections.Property("ToTimestamp"))
                        ))
                        .List<object[]>();
                    intervalList = CollectionHelper.ListToDataTable(res, new List<string>() { "FromTimestamp", "ToTimestamp" });
                }
            }
            catch (Exception ex)
            {
            }
            return intervalList;
        }

        public bool CheckIPU(string tableName)
        {
            string cmdText = "SELECT Ipus FROM " + tableName + " LIMIT 1;";
            bool exists = false;

            using (ISession session = NHibernateHelper.OpenSessionForPartioned("CPUEntity", tableName, _connectionString))
            {
                var res = session.CreateCriteria<CPUEntity>()
                    .SetMaxResults(1)
                    .UniqueResult<CPUEntity>();
                if (res?.Ipus != null) exists = true;
            }
            return exists;
        }

        public int GetNumOfIPUs(string tableName)
        {
            string cmdText = "SELECT Ipus FROM " + tableName + " LIMIT 1;";
            int IPUs = 0;
            using (ISession session = NHibernateHelper.OpenSessionForPartioned("CPUEntity", tableName, _connectionString))
            {
                IPUs = session.CreateCriteria<CPUEntity>()
                    .SetMaxResults(1)
                    .SetProjection(Projections.Property("Ipus"))
                    .UniqueResult<int>();
            }
            return IPUs;
        }

        public DataTable GetAllCPUBusyAndQueue(List<string> cpuTables, bool ipu, long interval, DateTime startTime, DateTime stopTime)
        {
            var cpuBusyTable = new DataTable();
            IEnumerable<CPUEntity> union = null;
            IList<object[]> list = new List<object[]>();
            foreach (string cpuTable in cpuTables)
            {
                using (ISession session = NHibernateHelper.OpenSessionForPartioned("CPUEntity", cpuTable, _connectionString))
                {
                    var res = session.CreateCriteria<CPUEntity>()
                        .Add(Restrictions.Gt("DeltaTime", (double)interval * 1000000 * 0.8))
                        .Add(Restrictions.NotEqProperty("FromTimestamp", "ToTimestamp"))
                        .Add(Restrictions.Ge("FromTimestamp", startTime))
                        .Add(Restrictions.Lt("FromTimestamp", stopTime))
                        .Add(Restrictions.Gt("ToTimestamp", startTime))
                        .Add(Restrictions.Le("ToTimestamp", stopTime))
                        //.SetProjection(Projections.ProjectionList()
                        //    .Add(Projections.Alias(Projections.Property("FromTimestamp"), "DateTime"))
                        //    .Add(Projections.Alias(Projections.Property("CPUNum"), "CPUNumber"))
                        //)
                        .Future<CPUEntity>();
                    if (union == null) union = res;
                    else union = union.Union(res);
                    list = union.AsEnumerable().Select(x => new object[] { x.FromTimestamp, x.CpuNum, x.CpuBusyTime / (x.DeltaTime * (ipu ? x.Ipus : 1)) * 100, x.CpuQTime / x.DeltaTime }
                    //new {
                    //    DateTime = x.FromTimestamp,
                    //    CPUNumber = x.CpuNum,
                    //    Busy = x.CpuBusyTime / (x.DeltaTime * (ipu ? x.Ipus : 1)) * 100,
                    //    Queue = x.CpuQTime / x.DeltaTime
                    //}
                    ).ToList();
                }
            }
            cpuBusyTable = CollectionHelper.ListToDataTable(list, new List<string>() { "DateTime", "CPUNumber", "Busy", "Queue" });
            return cpuBusyTable;
        }

        public DataTable GetAllCPUBusyAndMemory(List<DateTime> dateList, DateTime startTime, DateTime stopTime)
        {
            var cpuBusyTable = new DataTable();
            IEnumerable<TrendCpuInterval> union = null;
            IList<object[]> list = new List<object[]>();
            foreach (DateTime dateTime in dateList)
            {
                using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
                {
                    var res = session.CreateCriteria<TrendCpuInterval>()
                        .Add(Restrictions.Ge("Interval", dateTime))
                        .Add(Restrictions.Lt("Interval", dateTime.AddDays(1)))
                        .Future<TrendCpuInterval>();
                    if (union == null) union = res;
                    else union = union.Union(res);
                    list = union.AsEnumerable().Select(x => new object[] { x.Interval, x.CpuBusy, x.QueueLength, x.CpuNumber, x.MemoryUsed }).ToList();
                }
            }
            cpuBusyTable = CollectionHelper.ListToDataTable(list, new List<string>() { "Date & Time", "Busy", "Queue", "CPUNumber", "MemoryUsed" });
            return cpuBusyTable;
        }

        public DataTable GetAllIPUBusyAndQueue(List<string> cpuTables, int numOfIPUs, long interval, DateTime startTime, DateTime stopTime)
        {
            var ipuBusyTable = new DataTable();
            IEnumerable<CPUEntity> union = null;
            IList<CPUEntity> list = new List<CPUEntity>();
            foreach (string cpuTable in cpuTables)
            {
                using (ISession session = NHibernateHelper.OpenSessionForPartioned("CPUEntity", cpuTable, _connectionString))
                {
                    var res = session.CreateCriteria<CPUEntity>()
                        .Add(Restrictions.Gt("DeltaTime", (double)interval * 1000000 * 0.8))
                        .Add(Restrictions.NotEqProperty("FromTimestamp", "ToTimestamp"))
                        .Add(Restrictions.Ge("FromTimestamp", startTime))
                        .Add(Restrictions.Lt("FromTimestamp", stopTime))
                        .Add(Restrictions.Gt("ToTimestamp", startTime))
                        .Add(Restrictions.Le("ToTimestamp", stopTime))
                        .AddOrder(Order.Asc("FromTimestamp"))
                        .Future<CPUEntity>();
                    if (union == null) union = res;
                    else union = union.Union(res);
                    list = union.ToList();
                }
            }
            var propNames = new List<string>() { "CPUNumber", "Date & Time" };
            for (int i = 1; i <= numOfIPUs; i++)
            {
                propNames.Add("IPUBusy" + i);
                propNames.Add("IPUQLength" + i);
            }
            var computedList = list.AsEnumerable().Select(x => new object[] { x.CpuNum, x.FromTimestamp }).ToList();
            for (int i = 0; i < list.Count; i++)
            {
                var objectList = computedList[i].ToList();
                for (int ipuNum = 1; ipuNum <= numOfIPUs; ipuNum++)
                {
                    var dataObj = list[i];
                    double ipuBusyTime = (double)dataObj.GetType().GetProperty("IpuBusyTime" + ipuNum).GetValue(dataObj);
                    double ipuQLength = (double)dataObj.GetType().GetProperty("IpuQtime" + ipuNum).GetValue(dataObj);
                    objectList.Add(ipuBusyTime / dataObj.DeltaTime * 100);
                    objectList.Add(ipuQLength / dataObj.DeltaTime);
                }
                computedList[i] = objectList.ToArray<object>();
            }
            ipuBusyTable = CollectionHelper.ListToDataTable(computedList, propNames);
            return ipuBusyTable;
        }

        public DataTable GetApplicationBusy(DateTime startTime, DateTime stopTime)
        {
            //    cmdText.Append(@"SELECT ApplicationName, `Interval` AS `Date & Time`, CpuBusy, DiskIO FROM TrendApplicationInterval
            //                    WHERE (`Interval` >= @FromTimestamp AND `Interval` <= @ToTimestamp) AND ApplicationName != 'Others'
            //                    ORDER BY `Date & Time`");
            var applicationBusyTable = new DataTable();
            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
            {
                var res = session.CreateCriteria<TrendApplicationInterval>()
                    .Add(Restrictions.Ge("Interval", startTime))
                    .Add(Restrictions.Le("Interval", stopTime))
                    .Add(Restrictions.Not(Restrictions.Eq("ApplicationName", "Others")))
                    .SetProjection(Projections.ProjectionList()
                        .Add(Projections.Property("ApplicationName"))
                        .Add(Projections.Property("Interval"))
                        .Add(Projections.Property("CpuBusy"))
                        .Add(Projections.Property("DiskIO"))
                    )
                    .AddOrder(Order.Asc(Projections.Property("Interval")))
                    .List<object[]>();
                applicationBusyTable = CollectionHelper.ListToDataTable(res, new List<string>() { "ApplicationName", "Interval", "CpuBusy", "DiskIO" });
            }
            return applicationBusyTable;
        }

        public int GetPageSizeBytes(string cpuTables)
        {
            string cmdText = @"SELECT PageSizeBytes FROM " + cpuTables + " LIMIT 1";
            int pageSizeBytes = 0;
            using (ISession session = NHibernateHelper.OpenSessionForPartioned("CPUEntity", cpuTables, _connectionString))
            {
                pageSizeBytes = (int)session.Query<CPUEntity>()
                    .Select(x => x.PageSizeBytes)
                    .FirstOrDefault();
            }

            return pageSizeBytes;
        }
    }
}
