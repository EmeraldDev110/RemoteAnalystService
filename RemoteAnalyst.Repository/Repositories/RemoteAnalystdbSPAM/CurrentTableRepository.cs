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
    public class CurrentTableRepository
    {
        private readonly string _connectionString;

        public CurrentTableRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void DeleteEntry(string tableName)
        {
            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "CurrentTable"))
            {
                session.Query<CurrentTable>()
                    .Where(a => a.TableName == tableName)
                    .Delete();
            }
        }

        public void InsertEntry(string tableName, int entityID, long interval, DateTime startTime,
            string UWSSerialNumber, string measVersion)
        {
            try
            {
                using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "CurrentTable"))
                using (ITransaction transaction = session.BeginTransaction())
                {
                    CurrentTable currentTable = new CurrentTable()
                    {
                        TableName = tableName,
                        EntityID = entityID,
                        Interval = (int)interval,
                        DataDate = startTime,
                        SystemSerial = UWSSerialNumber,
                        MeasureVersion = measVersion
                    };
                    session.Save(currentTable);
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
            string cmdText = "SELECT `Interval` FROM CurrentTables WHERE TableName = @TableName";
            long currentInterval = 0;

            try
            {
                using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "CurrentTable"))
                {
                    currentInterval = session.CreateCriteria<CurrentTable>()
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

        public long GetLatestIntervl()
        {
            //string connectionString = Config.ConnectionString;
            string cmdText = "SELECT `Interval` FROM CurrentTables ORDER BY DataDate DESC LIMIT 1";
            long currentInterval = 0;

            try
            {
                using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "CurrentTable"))
                {
                    currentInterval = session.CreateCriteria<CurrentTable>()
                        .SetMaxResults(1)
                        .SetProjection(Projections.Property("Interval"))
                        .AddOrder(Order.Desc(Projections.Property("Interval")))
                        .UniqueResult<int>();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return currentInterval;
        }

        public List<int> GetEntities(DateTime startDateTime, DateTime stopDateTime, long interval)
        {
            //string connectionString = Config.ConnectionString;
            /*var cmdText = @"SELECT DISTINCT(EntityID) FROM CurrentTables AS C
                            INNER JOIN TableTimestamp AS T ON C.TableName = T.TableName
                            WHERE Start >= @StartTime AND 
                            End <= @StopTime";*/

            var cmdText = @"SELECT DISTINCT(EntityID) FROM CurrentTables AS C
                            INNER JOIN TableTimestamp AS T ON C.TableName = T.TableName
                            WHERE (@StartTime BETWEEN DATE_ADD(`Start`, INTERVAL -" + interval + " SECOND) AND DATE_ADD(`End`, INTERVAL " + interval + @" SECOND)) 
                            AND (@StopTime BETWEEN DATE_ADD(`Start`, INTERVAL -" + interval + " SECOND) AND DATE_ADD(`End`, INTERVAL " + interval + @" SECOND)) OR
                            (Start >= @StartTimeAddSec AND End <= @StopTimeAddSec)";
            var entities = new List<int>();

            try
            {
                using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "CurrentTable"))
                {
                    CurrentTable currentTable = null;
                    TableTimestamp tableTimestamp = null;
                    entities = (List<int>)session.QueryOver(() => currentTable)
                        .Inner.JoinAlias(() => currentTable.TableTimestamps, () => tableTimestamp)
                        .Where(() => (
                            tableTimestamp.Start <= startDateTime.AddSeconds(interval)
                            && tableTimestamp.Start <= stopDateTime.AddSeconds(interval)
                            && tableTimestamp.End >= startDateTime.AddSeconds(-interval)
                            && tableTimestamp.End >= stopDateTime.AddSeconds(-interval)
                            ) || (tableTimestamp.Start >= startDateTime.AddSeconds(interval * -0.1)
                                && tableTimestamp.End <= stopDateTime.AddSeconds(interval * 0.1)
                            )
                        )
                        .SelectList(_ => _.SelectGroup(() => currentTable.EntityID))
                        .List<int>();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return entities;
        }
    }
}
