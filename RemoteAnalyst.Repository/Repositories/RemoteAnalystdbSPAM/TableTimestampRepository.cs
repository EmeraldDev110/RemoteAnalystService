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
    public class TableTimestampRepository
    {
        private readonly string _connectionString;

        public TableTimestampRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void DeleteEntry(string tableName)
        {
            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "TableTimestamp"))
            {
                session.Query<TableTimestamp>()
                    .Where(a => a.TableName == tableName)
                    .Delete();
            }
        }
        public void DeleteEntry(List<TableTimestampQueryParameter> parameters)
        {
            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "TableTimestamp"))
            {
                foreach (TableTimestampQueryParameter ttqParameter in parameters)
                {
                    session.Query<TableTimestamp>()
                    .Where(a => a.TableName == ttqParameter.TableName
                        && a.Start == ttqParameter.StartTime
                        && a.End == ttqParameter.StopTime
                        && string.IsNullOrEmpty(a.ArchiveID)
                    )
                    .Delete();
                }
            }
        }

        public void InsetEntryFor(string tableName, DateTime startTime, DateTime stopTime, int status, string fileName)
        {
            bool duplicate = CheckDuplicate(tableName, startTime, stopTime);
            if (!duplicate)
            {
                try
                {
                    using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "TableTimestamp"))
                    using (ITransaction transaction = session.BeginTransaction())
                    {
                        TableTimestamp tableTimestamp = new TableTimestamp()
                        {
                            TableName = tableName,
                            Start = startTime,
                            End = stopTime,
                            Status = status,
                            FileName = fileName
                        };
                        session.Save(tableTimestamp);
                        transaction.Commit();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }
        public bool CheckTimeOverLap(string tableName, DateTime startTime, DateTime stopTime)
        {
            DateTime currentStartTime = startTime.AddSeconds(startTime.Second * -1);
            DateTime currentEndTime = stopTime.AddSeconds(stopTime.Second * -1);
            var oldStartTime = new DateTime();
            var oldEndTime = new DateTime();
            bool timeStampokay = false;

            string cmdText = "SELECT Start, End FROM TableTimestamp WHERE TableName = @TableName";

            try
            {
                using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "TableTimestamp"))
                {
                    var res = session.QueryOver<TableTimestamp>()
                        .Where(x => x.TableName == tableName)
                        .List();

                    foreach (var row in res)
                    {
                        oldStartTime = row.Start;
                        oldEndTime = row.End;

                        //Compare Time. Got this code from David's collector code.
                        if (currentEndTime <= oldStartTime.AddSeconds(oldStartTime.Second * -1) || currentStartTime >= oldEndTime.AddSeconds(oldEndTime.Second * -1))
                        {
                            //Okay to load.
                            timeStampokay = true;
                            //break;
                        }
                        else
                        {
                            //over laps.
                            timeStampokay = false;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return timeStampokay;
        }

        public bool CheckTempTimeOverLap(string tableName, DateTime startTime, DateTime stopTime)
        {
            DateTime currentStartTime = startTime.AddSeconds(startTime.Second * -1);
            DateTime currentEndTime = stopTime.AddSeconds(stopTime.Second * -1);
            var oldStartTime = new DateTime();
            var oldEndTime = new DateTime();
            bool timeStampokay = false;

            try
            {
                using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "TempTableTimestamp"))
                {
                    var res = session.QueryOver<TempTableTimestamp>()
                        .Where(x => x.TableName == tableName)
                        .List();

                    foreach (var row in res)
                    {
                        oldStartTime = row.Start;
                        oldEndTime = row.End;

                        //Compare Time. Got this code from David's collector code.
                        if (currentEndTime <= oldStartTime.AddSeconds(oldStartTime.Second * -1) || currentStartTime >= oldEndTime.AddSeconds(oldEndTime.Second * -1))
                        {
                            //Okay to load.
                            timeStampokay = true;
                            //break;
                        }
                        else
                        {
                            //over laps.
                            timeStampokay = false;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return timeStampokay;
        }

        public bool CheckDuplicate(string tableName, DateTime startTime, DateTime stopTime)
        {
            bool duplicate = false;
            try
            {
                using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "TableTimestamp"))
                {
                    var res = session.Query<TableTimestamp>()
                        .Where(a => a.TableName == tableName
                            && a.Start == startTime
                            && a.End == stopTime
                        ).FirstOrDefault();
                    if (res != null) duplicate = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return duplicate;
        }
        public DataTable GetTimestampsFor(string tableName, DateTime startTime, DateTime stopTime)
        {
            var timestamps = new DataTable();
            try
            {
                using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "TableTimestamp"))
                {
                    ICollection<TableTimestamp> res = session.CreateCriteria<TableTimestamp>()
                        .Add(Restrictions.InsensitiveLike("TableName", '%' + tableName + '%'))
                        .Add(Restrictions.Ge("Start", startTime))
                        .Add(Restrictions.Le("End", stopTime))
                        .List<TableTimestamp>();
                    timestamps = CollectionHelper.ToDataTable(res);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return timestamps;
        }

        public void UpdateStatusUsingTableName(string tableName, DateTime startTime, DateTime stopTime, int status)
        {
            string cmdText = "UPDATE TableTimestamp SET Status = @Status WHERE TableName = @TableName AND Start = @Start AND End = @End ";
            try
            {
                using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "TableTimestamp"))
                using (ITransaction transaction = session.BeginTransaction())
                {
                    TableTimestamp tableTimestamp = session.Get<TableTimestamp>(new TableTimestamp() { TableName = tableName, Start = startTime, End = stopTime});
                    tableTimestamp.Status = status;
                    session.SaveOrUpdate(tableTimestamp);
                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void UpdateStatusUsingTableName(List<TableTimestampQueryParameter> parameters)
        {
            foreach (TableTimestampQueryParameter ttqParameter in parameters)
            {
                UpdateStatusUsingTableName(ttqParameter.TableName, ttqParameter.StartTime, ttqParameter.StopTime, ttqParameter.Status);
            }
        }

        public DataTable GetArchiveDetailsPerTable(string tableName)
        {
            var archiveDetails = new DataTable();
            const string cmdText = "SELECT TableName, Start, End, ArchiveID FROM TableTimestamp " +
                                   "WHERE TableName = @TableName";

            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "TableTimestamp"))
            {
                ICollection<TableTimestamp> res = session.CreateCriteria<TableTimestamp>()
                    .Add(Restrictions.Eq("TableName", tableName))
                    .List<TableTimestamp>();
                archiveDetails = CollectionHelper.ToDataTable(res);
            }
            return archiveDetails;

        }
        public DataTable GetGetLoadedData(DateTime dataStartDate, DateTime dataStopDate)
        {
            var archives = new DataTable();
            string cmdText = @"SELECT TableName, Start, End FROM TableTimestamp
                                WHERE (`Start` >= @StartDate AND `End` < @StopDate)
                                AND (TableName LIKE '%CPU%' OR TableName LIKE '%DISC%' OR TableName LIKE '%PROCESS%')";
            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "TableTimestamp"))
            {
                ICollection<TableTimestamp> res = session.CreateCriteria<TableTimestamp>()
                    .Add(Restrictions.Or(Restrictions.InsensitiveLike("TableName", "%CPU%"),
                        Restrictions.Or(Restrictions.InsensitiveLike("TableName", "%DISC%"),
                            Restrictions.InsensitiveLike("TableName", "%PROCESS%"))
                    ))
                    .Add(Restrictions.Ge("Start", dataStartDate))
                    .Add(Restrictions.Lt("End", dataStopDate))
                    .List<TableTimestamp>();
                archives = CollectionHelper.ToDataTable(res);
            }
            return archives;
        }

        public DataTable GetLoadedFileData(DateTime dataStartDate, DateTime dataStopDate)
        {
            var archives = new DataTable();
            string cmdText = @"SELECT TableName, Start, End FROM TableTimestamp
                                WHERE (`Start` >= @StartDate AND `End` < @StopDate)
                                AND (TableName LIKE '%FILE%')";
            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "TableTimestamp"))
            {
                ICollection<TableTimestamp> res = session.CreateCriteria<TableTimestamp>()
                    .Add(Restrictions.InsensitiveLike("TableName", "%FILE%"))
                    .Add(Restrictions.Ge("Start", dataStartDate))
                    .Add(Restrictions.Lt("End", dataStopDate))
                    .List<TableTimestamp>();
                archives = CollectionHelper.ToDataTable(res);
            }
            return archives;

        }

        public void AddFileNameColumnToTableTimestampTable()
        {
            // TODO
            string sqlStr = "ALTER TABLE `TableTimestamp` ADD COLUMN `FileName` VARCHAR(1000) NULL AFTER `CreationDate`";
        }

        public bool CheckMySqlColumn(string databaseName, string tableName, string columnName)
        {
            string cmdText = @"SELECT  COUNT(*) AS ColumnName FROM information_schema.COLUMNS 
                                WHERE TABLE_SCHEMA = @DatabaseName 
                                AND TABLE_NAME = @TableName 
                                AND COLUMN_NAME = @ColumnName";
            bool exists = true;
            try
            {
                using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "TableTimestamp"))
                {
                    var res = session.CreateCriteria<TableTimestamp>()
                        .SetProjection(Projections.Property(columnName))
                        .SetFirstResult(1)
                        .List();
                }
            }
            catch (Exception ex)
            {
                exists = false;
            }
            return exists;
        }

    }
}
