using System;
using System.Collections.Generic;
using RemoteAnalyst.Repository.Models;
using RemoteAnalyst.Repository.Helpers;
using NHibernate;
using NHibernate.Criterion;
using System.Data;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.Repository.MySQLConcrete.RemoteAnalystSPAM
{
    public class Entities
    {
        private readonly string _connectionString;

        public Entities(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DataTable GetTimeIntervalCountPerEntity(List<string> entityTables)
        {
            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
            {
                ICollection<object[]> res = session.CreateCriteria(typeof(TableTimestamp))
                    .Add(Restrictions.In("TableName", entityTables))
                    .SetProjection(Projections.ProjectionList()
                        .Add(Projections.GroupProperty("TableName"))
                        .Add(Projections.Alias(Projections.Count("TableName"), "IntervalCount"))
                    ).List<object[]>();
                return CollectionHelper.ListToDataTable(res, new List<string>() { "TableName", "IntervalCount" });
            }
        }

        public int GetCPUCount(string className, string tableName, DateTime fromDateTime, DateTime toDateTime)
        {
            int couCount = 0;
            try
            {
                using (ISession session = NHibernateHelper.OpenSessionForPartioned(className, tableName, _connectionString))
                {
                    int res = session.CreateCriteria(NHibernateHelper.GetType(className))
                        .Add(Restrictions.Ge("FromTimestamp", fromDateTime))
                        .Add(Restrictions.Lt("FromTimestamp", toDateTime))
                        .SetProjection(Projections.ProjectionList()
                            .Add(Projections.Alias(Projections.CountDistinct("CpuNum"), "IntervalCount"))
                        ).UniqueResult<int>();
                    couCount = res;
                }
            }
            catch
            {
            }
            return couCount;
        }

        public bool CheckTime(string className, string tableName, DateTime fromDateTime, DateTime toDateTime)
        {
            string cmdText = @"SELECT FromTimestamp FROM " + tableName +
                              @" WHERE FromTimestamp >= @FromTimestamp AND FromTimestamp < @ToTimestamp AND 
                                 ToTimestamp > @FromTimestamp AND ToTimestamp <= @ToTimestamp AND DeltaTime > 0 LIMIT 1";
            //" WHERE FromTimestamp >= @FromTimestamp AND FromTimestamp <= @ToTimestamp LIMIT 1";

            bool exist = false;
            try
            {
                using (ISession session = NHibernateHelper.OpenSessionForPartioned(className, tableName, _connectionString))
                {
                    var res = session.CreateCriteria(NHibernateHelper.GetType(className))
                        .SetMaxResults(1)
                        .Add(Restrictions.Ge("FromTimestamp", fromDateTime))
                        .Add(Restrictions.Lt("FromTimestamp", toDateTime))
                        .Add(Restrictions.Gt("DeltaTime", 0.0))
                        .SetProjection(Projections.Property("FromTimestamp"))
                        .UniqueResult<DateTime>();
                    if (res != null) exist = true;
                }
            }
            catch
            {
            }
            return exist;
        }

        public int GetOpenerCPUCount(string tableName, DateTime fromDateTime, DateTime toDateTime)
        {
            int couCount = 0;
            try
            {
                using (ISession session = NHibernateHelper.OpenSessionForPartioned("DailyFile", tableName, _connectionString))
                {
                    int res = session.CreateCriteria(NHibernateHelper.GetType("DailyFile"))
                        .Add(Restrictions.Ge("FromTimestamp", fromDateTime))
                        .Add(Restrictions.Lt("FromTimestamp", toDateTime))
                        .SetProjection(Projections.ProjectionList()
                            .Add(Projections.Alias(Projections.CountDistinct("OpenerCpu"), "CPUCount"))
                        ).UniqueResult<int>();
                    couCount = res;
                }
            }
            catch
            {
            }
            return couCount;
        }
    }
}
