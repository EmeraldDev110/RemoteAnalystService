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
    public class TempTableTimestampRepository
    {
        private readonly string _connectionString;

        public TempTableTimestampRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void InsertTempTimeStamp(string tableName, DateTime startTime, DateTime stopTime, string fileName)
        {
            bool duplicate = CheckDuplicate(tableName, startTime, stopTime);
            if (!duplicate)
            {
                try
                {
                    using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "TempTableTimestamp"))
                    using (ITransaction transaction = session.BeginTransaction())
                    {
                        TempTableTimestamp tempTableTimestamp = new TempTableTimestamp()
                        {
                            TableName = tableName,
                            Start = startTime,
                            End = stopTime,
                            FileName = fileName
                        };
                        session.Save(tempTableTimestamp);
                        transaction.Commit();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }

        public bool CheckDuplicate(string tableName, DateTime startTime, DateTime stopTime)
        {
            bool duplicate = false;
            try
            {
                using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "TempTableTimestamp"))
                {
                    var res = session.Query<TempTableTimestamp>()
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
        public void DeleteTempTimeStamp(string tableName)
        {
            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "TempTableTimestamp"))
            {
                session.Query<TempTableTimestamp>()
                    .Where(a => a.TableName == tableName)
                    .Delete();
            }
        }

        public void AddFileNameColumnToTempTableTimestampTable()
        {
            // TODO
            string sqlStr = "ALTER TABLE `TempTableTimestamp` ADD COLUMN `FileName` VARCHAR(1000) NULL AFTER `End`";
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
                using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "TempTableTimestamp"))
                {
                    var res = session.CreateCriteria<TempTableTimestamp>()
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
