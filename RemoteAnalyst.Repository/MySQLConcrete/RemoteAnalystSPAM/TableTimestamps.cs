using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using NHibernate;
using NHibernate.Criterion;
using RemoteAnalyst.Repository.Helpers;
using RemoteAnalyst.Repository.Models;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.Repository.MySQLConcrete.RemoteAnalystSPAM {
    public class TableTimestamps {
        private readonly string _connectionString;

        public TableTimestamps(string connectionString) {
            _connectionString = connectionString;
        }

        public DataTable GetStartEndTime(string fileTableName) {
            var transactionProfile = new DataTable();
            string cmdText = @"SELECT Start, End FROM TableTimestamp 
                                WHERE Status = 0 AND TableName = @TableName
                                ORDER BY Start;";
            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
            {
                var res = session.CreateCriteria<TableTimestamp>()
                    .Add(Restrictions.Eq("Status", 0))
                    .Add(Restrictions.Eq("TableName", fileTableName))
                    .SetProjection(Projections.ProjectionList()
                        .Add(Projections.Property("Start"))
                        .Add(Projections.Property("End"))
                    ).AddOrder(Order.Asc(Projections.Property("Start")))
                    .List<object[]>();
                transactionProfile = CollectionHelper.ListToDataTable(res, new List<string>() { "Start", "End" });
            }

            return transactionProfile;
        }
    }
}