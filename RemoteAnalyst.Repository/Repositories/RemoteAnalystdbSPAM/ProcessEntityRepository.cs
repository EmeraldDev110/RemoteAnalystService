using MySqlConnector;
using NHibernate;
using NHibernate.Criterion;
using RemoteAnalyst.Repository.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Repositories
{
    public class ProcessEntityRepository
    {
        private readonly string _connectionString;

        public ProcessEntityRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        public double GetTransientRate(int cpuNumber, DateTime startTime)
        {
            var processCount = 0d;
            var transientProcessCount = 0d;
            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "TrendProcessHourly"))
            {
                Models.TrendProcessHourly c = null;
                var res = session.QueryOver<Models.TrendProcessHourly>(() => c)
                                            .Where(() => c.CpuNumber == cpuNumber.ToString("D2") && c.Hour >= startTime).List<Models.TrendProcessHourly>();
                return res[0] != null ? res[0].AverageTransientCount / res[0].AverageProcessCount : 0d;
            }
            /*var cmdText = new StringBuilder();
            cmdText.Append("SELECT AverageTransientCount, AverageProcessCount FROM TrendProcessHourly " +
                           "WHERE CpuNumber = @CpuNum AND Hour >= @Hour");

            var processData = new DataTable();
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText.ToString(), connection);
                command.Parameters.AddWithValue("@Hour", startTime);
                command.Parameters.AddWithValue("@CpuNum", cpuNumber);
                connection.Open();
                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    processCount = Convert.ToDouble(reader["AverageProcessCount"]);
                    transientProcessCount = Convert.ToDouble(reader["AverageTransientCount"]);
                }
            }

            return transientProcessCount / processCount;*/
        }

        /*public List<string> GetTop5DP2DeviceName(List<string> processTableName, List<string> discTableName, long interval)
        {
            var topDp2DeviceName = new List<string>();

            var cmdText = new StringBuilder();

            cmdText.Append("SELECT * FROM ( ");
            for (var x = 0; x < processTableName.Count; x++)
            {
                if (x != 0)
                    cmdText.Append(" UNION ALL ");

                cmdText.Append(@" SELECT ProcessName, (SUM(CpuBusyTime / (" + interval + @" * 1000000)) * 100) / 2 AS DP2Busy
                            FROM " + processTableName[x] + @" WHERE ProcessName IN (
                                SELECT DeviceName FROM " + discTableName[x] + @" GROUP BY DeviceName
                            ) GROUP BY ProcessName ");
            }
            cmdText.Append(") AS A GROUP BY ProcessName ORDER BY DP2Busy DESC LIMIT 5 ");


            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText.ToString(), connection);
                connection.Open();
                var reader = command.ExecuteReader();

                while (reader.Read())
                    topDp2DeviceName.Add(reader["ProcessName"].ToString());
            }

            return topDp2DeviceName;
        }*/

        /*public DataTable GetDP2BusyData(List<string> processTableName, List<string> discTableName, long interval)
        {
            var cmdText = new StringBuilder();

            cmdText.Append("SELECT * FROM ( ");
            for (var x = 0; x < processTableName.Count; x++)
            {
                if (x != 0)
                    cmdText.Append(" UNION ALL ");

                cmdText.Append(@" SELECT ProcessName, FromTimestamp, (SUM(CpuBusyTime / (" + interval + @" * 1000000)) * 100) / 2 AS DP2Busy
                                FROM " + processTableName[x] + @" WHERE ProcessName IN (
		                                SELECT DeviceName FROM " + discTableName[x] + @" GROUP BY DeviceName
                                ) GROUP BY ProcessName, FromTimestamp ");
            }
            cmdText.Append(") AS A GROUP BY ProcessName, FromTimestamp ORDER BY FromTimestamp, ProcessName ");

            var dataTable = new DataTable();
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText.ToString(), connection);
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(dataTable);
            }

            return dataTable;
        }*/

        public DataTable GetFirstAndLastFromTimestamp(DateTime fromTimestamp, DateTime toTimestamp)
        {
            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString, "DetailProcessForForecast"))
            {
                var res = session
                    .CreateCriteria(typeof(Models.DetailProcessForForecast))
                    .Add(Restrictions.Ge("FromTimestamp", fromTimestamp))
                    .Add(Restrictions.Le("FromTimestamp", toTimestamp))
                    .SetProjection(Projections.ProjectionList()
                        .Add(Projections.Alias(Projections.Min("FromTimestamp"), "FirstFromTimestamp"))
                        .Add(Projections.Alias(Projections.Max("FromTimestamp"), "LastFromTimestamp"))
                    )
                    .List<object[]>();
                List<string> propNames = new List<string>() { "FirstFromTimestamp", "LastFromTimestamp" };
                return CollectionHelper.ListToDataTable(res, propNames);
            }
            /*    var cmdText = new StringBuilder();
            cmdText.Append(@"SELECT MIN(FromTimestamp) AS FirstFromTimestamp, MAX(FromTimestamp) AS LastFromTimestamp 
                            FROM DetailProcessForForecast
                            WHERE FromTimestamp >= @FromTimestamp AND FromTimestamp<= @ToTimestamp");

            var dataTable = new DataTable();
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText.ToString(), connection);
                command.Parameters.AddWithValue("@FromTimestamp", fromTimestamp);
                command.Parameters.AddWithValue("@ToTimestamp", toTimestamp);
                command.CommandTimeout = 0;
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(dataTable);
            }

            return dataTable;*/
        }

        /*public DataTable GetAllProcessDataFromForecastTable(DateTime fromTimestamp, DateTime toTimestamp)
        {
            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString,"DetailProcessForForecast"))
            {
                var res = session
                    .CreateCriteria(typeof(Models.DetailProcessForForecast))
                    .Add(Restrictions.Ge("FromTimestamp", fromTimestamp))
                    .Add(Restrictions.Le("FromTimestamp", toTimestamp))
                    .Add(!Restrictions.Like("FromTimestamp", "$SYS%"))
                    .SetProjection(Projections.ProjectionList()
                        .Add(Projections.Alias(Projections.Avg("ProcessBusy"), "ProcessBusy"))
                        .Add(Projections.Alias(Projections.Property("ProcessName"), "ProcessName"))
                        .Add(Projections.Alias(Projections.Property("CpuNumber"), "CpuNum"))
                        .Add(Projections.Alias(Projections.Property("Volume"), "Volume"))
                        .Add(Projections.Alias(Projections.Property("SubVol"), "SubVol"))
                        .Add(Projections.Alias(Projections.Property("FileName"), "FileName"))
                    )
                    .List<object[]>();
                List<string> propNames = new List<string>() { "ProcessBusy", "ProcessName", "CpuNum", "Volume", "SubVol", "FileName" };
                return CollectionHelper.ListToDataTable(res, propNames);
            }
            var cmdText = new StringBuilder();
            cmdText.Append(@" SELECT ProcessName, CpuNumber AS CpuNum, AVG(ProcessBusy) AS ProcessBusy, Volume, SubVol, FileName
                            FROM DetailProcessForForecast WHERE Volume NOT LIKE '$SYS%'
                            AND FromTimestamp >= @FromTimestamp AND FromTimestamp<= @ToTimestamp
                            GROUP BY ProcessName, CpuNumber, Volume, SubVol, FileName
                            HAVING TIME_TO_SEC(TIMEDIFF(MAX(FromTimestamp),MIN(FromTimestamp))) >= @RunTime AND AVG(ProcessBusy) > 0 ");

            var dataTable = new DataTable();
            using (var connection = new MySqlConnection(_connectionString))
            {
                var command = new MySqlCommand(cmdText.ToString(), connection);
                command.Parameters.AddWithValue("@FromTimestamp", fromTimestamp);
                command.Parameters.AddWithValue("@ToTimestamp", toTimestamp);
                var MinimumRunTimeForStaticProcesses = 0.8 * ((toTimestamp - fromTimestamp).TotalSeconds); //0.8 * 86400
                command.Parameters.AddWithValue("@RunTime", MinimumRunTimeForStaticProcesses);
                command.CommandTimeout = 0;
                var adapter = new MySqlDataAdapter(command);
                adapter.Fill(dataTable);
            }

            return dataTable;
        }*/
    }
}
