using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static log4net.Appender.RollingFileAppender;
using System.Web.Util;
using NHibernate.SqlCommand;
using RemoteAnalyst.Repository.Helpers;
using Google.Protobuf.WellKnownTypes;

namespace RemoteAnalyst.Repository.Repositories
{
    public class CPUEntityRepository
    {
        private readonly string _connectionString;

        public CPUEntityRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        public bool CheckIpus(string tableName)
        {
            using (ISession session = NHibernateHelper.OpenSessionForPartioned("CPUEntity", tableName, _connectionString))
            {
                Models.CPUEntity t = null;
                var res = session.QueryOver<Models.CPUEntity>(() => t)
                                        .Select(ct => ct.Ipus).List<int>();
                return res?[0] != null;
            }
             /*   bool ipus = false;
            string cmdText = "SELECT Ipus FROM " + tableName + " LIMIT 1";

            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);
                connection.Open();
                var reader = command.ExecuteReader();

                if (reader != null)
                {
                    if (reader.Read())
                    {
                        ipus = true;
                    }
                }
            }
            return ipus;*/
        }

        public long GetTotalMemory(string tableName)
        {
            using (ISession session = NHibernateHelper.OpenSessionForPartioned("CPUEntity", tableName, _connectionString))
            {
                Models.CPUEntity t = null;
                var res = session.QueryOver<Models.CPUEntity>(() => t)
                    .SelectList(l => l.Select(ct => ct.MemoryPages32).Select(ct => ct.PageSizeBytes)).List<object[]>();
                return (long)Convert.ToDouble(res[0][0]) * (long)Convert.ToDouble(res[0][1]); //!= null ? res[0].MemoryPages32 * res[0].PageSizeBytes : 0;
            }
            /*string cmdText = "SELECT (MemoryPages32 * PageSizeBytes) AS TotalMemory FROM " + tableName + " LIMIT 1;";
            var totalMemory = 0L;
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText, connection);

                connection.Open();
                var reader = command.ExecuteReader();
                if (reader != null)
                {
                    if (reader.Read())
                    {
                        totalMemory = Convert.ToInt64(reader["TotalMemory"]);
                    }
                }
            }
            return totalMemory;*/
        }

        /*public DataTable GetAllCPUBusy(List<string> cpuTables, bool ipu, DateTime fromTimestamp, DateTime toTimestamp, long interval)
        {
            var cmdText = new StringBuilder();
            for (int i = 0; i < cpuTables.Count; i++)
            {
                if (ipu)
                {
                    cmdText.Append(" SELECT FromTimeStamp, AVG(((CpuBusyTime/(DeltaTime * Ipus))*100)) AS CPUBusy ");
                }
                else
                {
                    cmdText.Append(" SELECT FromTimeStamp, AVG(((CpuBusyTime/DeltaTime)*100)) AS CPUBusy ");
                }
                cmdText.Append(" FROM " + cpuTables[i] + " ");
                cmdText.Append(" WHERE FromTimestamp >= @FromTimestamp AND FromTimestamp < @ToTimestamp AND ToTimestamp > @FromTimestamp AND ToTimestamp <= @ToTimestamp ");
                if (i != cpuTables.Count - 1)
                {
                    cmdText.Append(" UNION ALL ");
                }
            }
            cmdText.Append(" GROUP BY FromTimeStamp ORDER BY FromTimeStamp ASC");


            var cpuData = new DataTable();
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText.ToString(), connection);
                command.Parameters.AddWithValue("@FromTimestamp", fromTimestamp.AddSeconds(interval * -0.2));
                command.Parameters.AddWithValue("@ToTimestamp", toTimestamp.AddSeconds(interval * 0.2));
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(cpuData);
            }

            return cpuData;
        }*/

        public DataTable GetAllCPUDataFromTrendTable(DateTime fromTimestamp, DateTime toTimestamp)
        {
            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "TrendCpuHourly"))
            {
                Models.TrendCpuHourly t = null;
                var res = session.QueryOver<Models.TrendCpuHourly>(() => t)
                    .SelectList(l => l
                        .Select(ct => ct.Hour)
                        .Select(ct => ct.CpuNumber)
                        .Select(ct => ct.AverageCpuBusy)
                        .Select(ct => ct.AverageQueueLength)
                        .Select(ct => ct.AverageSwapRate)
                        .Select(ct => ct.AverageMemoryUsed)
                        )
                    .Where(c => c.Hour >= fromTimestamp && c.Hour <= toTimestamp)
                    .List<object[]>();
                List<string> propNames = new List<string>() { "Hour", "CpuNumber", "AverageCpuBusy", "AverageQueueLength", "AverageSwapRate",
                                        "AverageMemoryUsed" };
                return CollectionHelper.ListToDataTable(res, propNames);
            }
            /*var cmdText = new StringBuilder();

            cmdText.Append(@"SELECT I.`Hour`, I.CpuNumber, I.AverageCpuBusy, I.AverageQueueLength, IFNULL(I.AverageSwapRate, 0) AS AverageSwapRate,
                                I.AverageMemoryUsed FROM TrendCpuHourly AS I
                                WHERE I.`Hour` >= @FromTimestamp AND I.`Hour` <= @ToTimestamp");

            var cpuData = new DataTable();
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText.ToString(), connection);
                command.Parameters.AddWithValue("@FromTimestamp", fromTimestamp);
                command.Parameters.AddWithValue("@ToTimestamp", toTimestamp);

                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(cpuData);
            }

            return cpuData;*/
        }

        public DataTable GetAllIPUDataFromTrendTable(DateTime fromTimestamp, DateTime toTimestamp)
        {
            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "TrendIpuHourly"))
            {
                Models.TrendIpuHourly t = null;
                var res = session.QueryOver<Models.TrendIpuHourly>(() => t)
                    .SelectList(l => l
                        .Select(ct => ct.Hour)
                        .Select(ct => ct.CpuNumber)
                        .Select(ct => ct.IpuNumber)
                        .Select(ct => ct.AverageIpuBusy)
                        .Select(ct => ct.AverageQueueLength)
                        )
                    .Where(c => c.Hour >= fromTimestamp && c.Hour <= toTimestamp)
                    .List<object[]>();
                List<string> propNames = new List<string>() { "Hour", "CpuNumber", "IpuNumber", "AverageIpuBusy", "AverageQueueLength" };
                return CollectionHelper.ListToDataTable(res, propNames);
            }
            /*var cmdText = new StringBuilder();

            cmdText.Append(@"SELECT H.Hour, H.CpuNumber, H.IpuNumber, H.AverageIpuBusy, H.AverageQueueLength
                            FROM TrendIpuHourly AS H
                            WHERE H.`Hour` >= @FromTimestamp AND H.`Hour` <= @ToTimestamp");

            var cpuData = new DataTable();
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText.ToString(), connection);
                command.Parameters.AddWithValue("@FromTimestamp", fromTimestamp);
                command.Parameters.AddWithValue("@ToTimestamp", toTimestamp);

                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(cpuData);
            }

            return cpuData;*/
        }

        public double GetTotalCpuBusy(DateTime fromTimestamp)
        {
            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "TrendCpuHourly"))
            {
                var res = session
                    .CreateCriteria(typeof(Models.TrendCpuHourly))
                    .Add(Restrictions.Eq("Hour", fromTimestamp))
                    .SetProjection(Projections.ProjectionList()
                        .Add(Projections.Alias(Projections.Sum("AverageCpuBusy"), "AverageCpuBusy"))
                    )
                    .List<double>();
                return res[0];
            }
            /*var cmdText = new StringBuilder();

            cmdText.Append(@"SELECT SUM(AverageCpuBusy) AS AverageCpuBusy
                            FROM TrendCpuHourly
                            WHERE `Hour` = @FromTimestamp");

            var cpuBusy = 0d;
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText.ToString(), connection);
                command.Parameters.AddWithValue("@FromTimestamp", fromTimestamp);
                connection.Open();

                var reader = command.ExecuteReader();
                if (reader.Read())
                    cpuBusy = Convert.ToDouble(reader["AverageCpuBusy"]);
            }

            return cpuBusy;*/
        }
    }
}
