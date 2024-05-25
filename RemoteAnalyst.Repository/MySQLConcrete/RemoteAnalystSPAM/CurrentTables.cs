using System;
using System.Collections.Generic;
using MySqlConnector;
using NHibernate;
using RemoteAnalyst.Repository.Models;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.Repository.MySQLConcrete.RemoteAnalystSPAM {
    public class CurrentTables {private readonly string _connectionString;

    public CurrentTables(string connectionString) {
            _connectionString = connectionString;
        }

        public Dictionary<string, long> GetFileTableList() {
            string cmdText = @"SELECT C.TableName, `Interval` FROM CurrentTables AS C 
                                INNER JOIN TableTimestamp AS T ON C.TableName = T.TableName
                                WHERE EntityID = 5 AND Status = 0 GROUP BY TableName, `Interval`";

            var fileTableNames = new Dictionary<string, long>();
            //using (var mySqlConnection = new MySqlConnection(_connectionString)) {
            //    mySqlConnection.Open();
            //    var cmd = new MySqlCommand(cmdText, mySqlConnection);
            //    // cmd.prepare();
            //    var reader = cmd.ExecuteReader();

            //    while (reader.Read()) {
            //        if (!fileTableNames.ContainsKey(reader["TableName"].ToString()))
            //            fileTableNames.Add(reader["TableName"].ToString(), Convert.ToInt64(reader["Interval"]));
            //    }
            //}
            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
            {
                CurrentTable currentTable = null;
                TableTimestamp tableTimestamp = null;
                var res = session.QueryOver(() => currentTable)
                    .Inner.JoinQueryOver(() => currentTable.TableTimestamps, () => tableTimestamp)
                    .Where(() => currentTable.EntityID == 5 && tableTimestamp.Status == 0)
                    .SelectList(list => list
                        .SelectGroup(() => currentTable.TableName)
                        .SelectGroup(() => currentTable.Interval)
                    )
                    .List<object[]>();
                foreach (var row in res)
                {
                    string tableName = row[0] as string;
                    long interval = Convert.ToInt64(row[1]);
                    fileTableNames[tableName] = interval;
                }
            }

            return fileTableNames;
        }

        public Dictionary<string, long> GetProcessTableList() {
            string cmdText = @"SELECT C.TableName, `Interval` FROM CurrentTables AS C 
                                INNER JOIN TableTimestamp AS T ON C.TableName = T.TableName
                                WHERE EntityID = 7 AND Status = 0 GROUP BY TableName, `Interval`";

            var fileTableNames = new Dictionary<string, long>();
            //using (var mySqlConnection = new MySqlConnection(_connectionString)) {
            //    mySqlConnection.Open();
            //    var cmd = new MySqlCommand(cmdText, mySqlConnection);
            //    // cmd.prepare();
            //    var reader = cmd.ExecuteReader();

            //    while (reader.Read()) {
            //        if (!fileTableNames.ContainsKey(reader["TableName"].ToString()))
            //            fileTableNames.Add(reader["TableName"].ToString(), Convert.ToInt64(reader["Interval"]));
            //    }
            //}
            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
            {
                CurrentTable currentTable = null;
                TableTimestamp tableTimestamp = null;
                var res = session.QueryOver(() => currentTable)
                    .Inner.JoinQueryOver(() => currentTable.TableTimestamps, () => tableTimestamp)
                    .Where(() => currentTable.EntityID == 7 && tableTimestamp.Status == 0)
                    .SelectList(list => list
                        .SelectGroup(() => currentTable.TableName)
                        .SelectGroup(() => currentTable.Interval)
                    )
                    .List<object[]>();
                foreach (var row in res)
                {
                    string tableName = row[0] as string;
                    long interval = Convert.ToInt64(row[1]);
                    fileTableNames[tableName] = interval;
                }
            }

            return fileTableNames;
        }

    }
}
