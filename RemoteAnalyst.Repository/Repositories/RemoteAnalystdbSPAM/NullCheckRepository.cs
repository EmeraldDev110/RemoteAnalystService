using Google.Protobuf.WellKnownTypes;
using log4net;
using MySqlConnector;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using RemoteAnalyst.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;

namespace RemoteAnalyst.Repository.Repositories
{
    public class NullCheckRepository
    {
        private readonly string ConnectionString;

        public NullCheckRepository(string connectionString)
        {
            ConnectionString = connectionString;
        }
        public bool NullCheckForPathway(DateTime fromTimestamp, DateTime toTimestamp, string connectionStringSystem)
        {
            using (ISession session = NHibernateHelper.OpenSessionCustom(ConnectionString, "PvCpuOnce"))
            {
                var res = session
                    .CreateCriteria(typeof(Models.PvCpuOnce))
                    .Add(Restrictions.Ge("FromTimestamp", fromTimestamp))
                    .Add(Restrictions.Le("FromTimestamp", fromTimestamp))
                    .List<Models.PvCpuOnce>();
                return res.Count > 0;
            }
            /*string cmdText = @"SELECT COUNT(*) AS TableCount FROM PvCpuonce WHERE FromTimestamp >= @FromTimeStamp AND ToTimeStamp <= @ToTimeStamp;";
            int rowCount = 0;
            try
            {
                using (var connection = new MySqlConnection(connectionStringSystem))
                {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@FromTimeStamp", fromTimestamp.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@ToTimeStamp", toTimestamp.ToString("yyyy-MM-dd HH:mm:ss"));
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        rowCount = Convert.ToInt32(reader["TableCount"].ToString());
                    }
                }
                if (rowCount > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }*/
        }

        public bool NullCheckForTrendReport(string systemSerial, DateTime currDateTime, string strOp, int entityID, string connectionString)
        {
            using (ISession session = NHibernateHelper.OpenSessionCustom(ConnectionString, "TableTimestamp"))
            {
                Models.CurrentTables c = null;
                Models.TableTimestamp t = null;
                var res = session.QueryOver<Models.CurrentTables>(() => c)
                                        .JoinEntityAlias(
                                            () => t,
                                            () => c.TableName == t.TableName,
                                            JoinType.InnerJoin)
                                        .Select(ct => ct.TableName)
                                        .Where(() => c.EntityID == entityID && t.Status != 0 && c.SystemSerial == systemSerial); /*&& (strOp == ">" ? c.DataDate > currDateTime :
                                                                                                                               strOp == ">=" ? c.DataDate >= currDateTime :
                                                                                                                               strOp == "<" ? c.DataDate < currDateTime :
                                                                                                                               c.DataDate <= currDateTime))*/
                var res2 = new List<string>();
                if (strOp == ">")
                {
                    res2 = (List<string>)res.Where((ct) => ct.DataDate > currDateTime).OrderBy(ct => ct.DataDate).Asc.List<string>();
                } else if (strOp == ">=") {
                    res2 = (List<string>)res.Where((ct) => ct.DataDate >= currDateTime).OrderBy(ct => ct.DataDate).Asc.List<string>();
                } else if (strOp == "<") {
                    res2 = (List<string>)res.Where((ct) => ct.DataDate < currDateTime).OrderBy(ct => ct.DataDate).Desc.List<string>();
                } else {
                    res2 = (List<string>)res.Where((ct) => ct.DataDate <= currDateTime).OrderBy(ct => ct.DataDate).Desc.List<string>();
                }
                if (res2.Count > 0 && res2[0] != null && res2[0] != "")
                {
                    return true;
                }
            }
            return false;
            /*var tableName = "";
            string cmdText = @"SELECT C.TableName FROM 
                             CurrentTables  AS C
                             INNER JOIN TableTimestamp AS T ON C.TableName = T.TableName
                             WHERE EntityID = @EntityID
                             AND T.Status = 0
                             AND SystemSerial = @SystemSerial
                             AND DataDate " + strOp + " @DataDate";

            if (strOp == ">" || strOp == ">=")
            {
                cmdText += " ORDER BY DataDate";
            }
            else
            {
                cmdText += " ORDER BY DataDate DESC";
            }

            cmdText += " LIMIT 1";


            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 180;
                    command.Parameters.AddWithValue("@EntityID", entityID);
                    command.Parameters.AddWithValue("@SystemSerial", systemSerial);
                    command.Parameters.AddWithValue("@DataDate", currDateTime.Date);
                    connection.Open();

                    MySqlDataReader reader = command.ExecuteReader();

                    if (reader != null)
                    {
                        if (reader.Read())
                        {
                            tableName = Convert.ToString(reader["TableName"]);

                        }
                    }
                }

            }
            catch (Exception ex)
            {
                return false;
            }

            if (tableName == "") return false;
            return true;*/
        }

        public bool NullCheckForProgramReport(DateTime startDate, DateTime stopDate, int programProfileId, string connectionString)
        {
            using (ISession session = NHibernateHelper.OpenSessionCustom(ConnectionString, "TrendProgramInterval"))
            {
                var res = session
                    .CreateCriteria(typeof(Models.TrendProgramInterval))
                    .Add(Restrictions.Ge("Interval", startDate))
                    .Add(Restrictions.Lt("Interval", stopDate))
                    .Add(Restrictions.Eq("ProgramProfileId", programProfileId))
                    .List<Models.TrendProgramInterval>();
                return res.Count > 0;
            }
            /*var rowCount = 0;
            string cmdText = @"SELECT ProgramProfileId FROM TrendProgramInterval 
                                WHERE `Interval` >= @StartDate 
                                AND `Interval` < @StopDate AND ProgramProfileId = @ProgramProfileId LIMIT 1";

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    var command = new MySqlCommand(cmdText, connection);
                    command.CommandTimeout = 180;
                    command.Parameters.AddWithValue("@StartDate", startDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@StopDate", stopDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@ProgramProfileId", programProfileId);
                    connection.Open();

                    MySqlDataReader reader = command.ExecuteReader();

                    if (reader != null)
                    {
                        if (reader.Read())
                        {
                            rowCount = 1;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                return false;
            }

            if (rowCount == 0) return false;
            return true;*/
        }

        public bool NullCheckForQNM(DateTime fromTimestamp, DateTime toTimestamp, string connectionStringSystem, ILog log)
        {
            return false;
            /*var cmdText = new StringBuilder();
            int rowCount = 0;
            int dayCount = 0;
            cmdText.Append("SELECT COUNT(`Date Time`) as rowCount ");
            cmdText.Append("FROM ( ");
            for (DateTime start = fromTimestamp; start.Date <= toTimestamp.Date; start = start.AddDays(1))
            {
                dayCount++;
            }
            var dayTick = fromTimestamp;
            for (int i = 0; i < dayCount; i++)
            {
                var strTableName = "QNM_TCPProcessDetail_" + dayTick.Year + "_" + dayTick.Month + "_" + dayTick.Day;
                if (i > 0) cmdText.Append(" UNION ALL ");
                cmdText.Append("SELECT `Date Time` ");
                cmdText.Append("FROM `" + strTableName + "` ");
                cmdText.Append("WHERE (`Date Time` >= @FromTimestamp AND `Date Time` <  @ToTimestamp)    ");
                //ADDING ONE DAY TO DATE.
                dayTick = dayTick.AddDays(1);
            }
            cmdText.Append(") AS Table1 ");
            log.InfoFormat("NullCheckForQNM: cmdText: {0}", cmdText.ToString());
            log.InfoFormat("NullCheckForQNM: fromTimestamp: {0}", fromTimestamp.ToString("yyyy-MM-dd HH:mm:ss"));
            log.InfoFormat("NullCheckForQNM: toTimestamp: {0}", toTimestamp.ToString("yyyy-MM-dd HH:mm:ss"));
            try
            {
                using (var connection = new MySqlConnection(connectionStringSystem))
                {
                    var command = new MySqlCommand(cmdText.ToString(), connection);
                    command.Parameters.AddWithValue("@FromTimeStamp", fromTimestamp.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@ToTimeStamp", toTimestamp.ToString("yyyy-MM-dd HH:mm:ss"));
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        log.Info("NullCheckForQNM: rowCount from resultset");
                        rowCount = Convert.ToInt32(reader["rowCount"].ToString());
                    }
                    log.InfoFormat("NullCheckForQNM: rowCount: {0}", rowCount);
                    if (rowCount > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("NullCheckForQNM: Exception: {0}", ex);
                return false;
            }*/
        }
        public bool NullCheckForStorage(DateTime fromTimestamp, DateTime toTimestamp, string connectionStringSystem)
        {
            using (ISession session = NHibernateHelper.OpenSessionCustom(ConnectionString, "DailyDisk"))
            {
                var res = session
                    .CreateCriteria(typeof(Models.DailyDisk))
                .Add(Restrictions.Ge("DD_Date", fromTimestamp))
                    .Add(Restrictions.Le("DD_Date", toTimestamp))
                    .List<Models.DailyDisk>();
                return res.Count > 0;
            }
            /*string cmdText = @"SELECT COUNT(*) AS TableCount FROM DailyDisk WHERE DD_Date >= @FromTimeStamp AND DD_Date <= @ToTimeStamp;";
            int rowCount = 0;
            try
            {
                using (var connection = new MySqlConnection(connectionStringSystem))
                {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@FromTimeStamp", fromTimestamp.ToString("yyyy-MM-dd 00:00:00"));
                    command.Parameters.AddWithValue("@ToTimeStamp", toTimestamp.ToString("yyyy-MM-dd 00:00:00"));
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        rowCount = Convert.ToInt32(reader["TableCount"].ToString());
                    }
                    if (rowCount > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }*/
        }

        public bool NullCheckForUserAllocation(string connectionStringSystem)
        {
            using (ISession session = NHibernateHelper.OpenSessionCustom(ConnectionString, "DailyDisk"))
            {
                var res = session
                    .CreateCriteria(typeof(Models.UserAllocation))
                    .List<Models.UserAllocation>();
                return res.Count > 0;
            }
            /*string cmdText = @"SELECT COUNT(*) AS TableCount FROM UserAllocation";
            int rowCount = 0;
            try
            {
                using (var connection = new MySqlConnection(connectionStringSystem))
                {
                    var command = new MySqlCommand(cmdText, connection);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        rowCount = Convert.ToInt32(reader["TableCount"].ToString());
                    }
                    if (rowCount > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }*/
        }
        public bool NullCheckForTPS(DateTime fromTimestamp, DateTime toTimestamp, string connectionStringSystem)
        {
            return false;
            /*string cmdText = @"SELECT COUNT(*) AS TableCount FROM TransactionProfileTrends WHERE FromDateTime >= @FromTimeStamp AND ToDateTime <= @ToTimeStamp;";
            int rowCount = 0;
            try
            {
                using (var connection = new MySqlConnection(connectionStringSystem))
                {
                    var command = new MySqlCommand(cmdText, connection);
                    command.Parameters.AddWithValue("@FromTimeStamp", fromTimestamp.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@ToTimeStamp", toTimestamp.ToString("yyyy-MM-dd HH:mm:ss"));
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        rowCount = Convert.ToInt32(reader["TableCount"].ToString());
                    }
                    if (rowCount > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }*/
        }
    }

}
