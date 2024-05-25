using System;
using System.Collections.Generic;
using RemoteAnalyst.Repository.Models;
using RemoteAnalyst.Repository.Helpers;
using NHibernate;
using NHibernate.Criterion;
using System.Data;
using System.Linq;
using NHibernate.Linq;
using RemoteAnalyst.Repository.Concrete.Model;

namespace RemoteAnalyst.Repository.Repositories
{
    public class TempCurrentTableRepository
    {
        private readonly string _connectionString;

        public TempCurrentTableRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        public void InsertCurrentTable(string tableName, int entityID, long interval, DateTime startTime,
            string UWSSerialNumber, string measVersion)
        {
            try
            {
                using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "TempCurrentTable"))
                using (ITransaction transaction = session.BeginTransaction())
                {
                    TempCurrentTable tempCurrentTable = new TempCurrentTable()
                    {
                        TableName = tableName,
                        EntityID = entityID,
                        Interval = (int)interval,
                        DataDate = startTime,
                        SystemSerial = UWSSerialNumber,
                        MeasureVersion = measVersion
                    };
                    session.Save(tempCurrentTable);
                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public long GetInterval(string buildTableName)
        {
            //string connectionString = Config.ConnectionString;
            string cmdText = "SELECT `Interval` FROM TempCurrentTables WHERE TableName = @TableName";
            long currentInterval = 0;

            try
            {
                using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "TempCurrentTable"))
                {
                    currentInterval = session.CreateCriteria<TempCurrentTable>()
                        .Add(Restrictions.Eq("TableName", buildTableName))
                        .SetProjection(Projections.Property("Interval"))
                        .UniqueResult<int>();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return currentInterval;
        }

        public void DeleteCurrentTable(string tableName)
        {
            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "TempCurrentTable"))
            {
                session.Query<TempCurrentTable>()
                    .Where(a => a.TableName == tableName)
                    .Delete();
            }
        }
    }
}
