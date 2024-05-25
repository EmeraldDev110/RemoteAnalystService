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
    public class DISCEntityTable
    {
        private readonly string _connectionString;

        public DISCEntityTable(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<string> GetDeviceNames(string entityTableName)
        {
            var deviceNames = new List<string>();
            try
            {
                string cmdText = "SELECT DISTINCT DeviceName FROM " + entityTableName;
                using (ISession session = NHibernateHelper.OpenSessionForPartioned("DISCEntity", entityTableName, _connectionString))
                {
                    deviceNames = (List<string>)session.CreateCriteria(typeof(DISCEntity))
                        .SetProjection(Projections.Distinct(Projections.Property("DeviceName")))
                        .List<string>();
                }
            }
            catch (Exception ex)
            {
            }
            return deviceNames;
        }

        public DataTable GetDISCEntityTableIntervalList(string entityTableName)
        {
            var intervalList = new DataTable();
            try
            {
                using (ISession session = NHibernateHelper.OpenSessionForPartioned("DISCEntity", entityTableName, _connectionString))
                {
                    IList<object[]> res = session.CreateCriteria(typeof(DISCEntity))
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
                throw new Exception(ex.Message);
            }
            return intervalList;
        }

        public bool CheckTableName(string entityTableName)
        {
            try
            {
                using (ISession session = NHibernateHelper.OpenSessionForPartioned("DISCEntity", entityTableName, _connectionString))
                {
                    session.Query<DISCEntity>().FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("doesn't exist")) return false;
                throw ex;
            }
            return true;
        }
    }
}
