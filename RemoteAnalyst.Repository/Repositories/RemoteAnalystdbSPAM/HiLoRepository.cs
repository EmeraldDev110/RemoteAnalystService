using MySqlConnector;
using NHibernate;
using NHibernate.Criterion;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;
using RemoteAnalyst.Repository.Helpers;
using RemoteAnalyst.Repository.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Repositories
{
    public class HiLoRepository
    {
        private readonly string ConnectionString;

        public HiLoRepository(string connectionStringTrend)
        {
            ConnectionString = connectionStringTrend;
        }
        public DataSet GetHourlyHiLo(string isystemSerial, DateTime[] reportDate, int i)
        {
            var myDataSet = new DataSet();
            DateTime start = reportDate[0];
            DateTime end = reportDate[0].AddDays(1);
            if (reportDate.Length > 1)
            {
                int x = 0;
                for (x = 0; x < reportDate.Length; x++)
                {
                    if (reportDate[x] > DateTime.MinValue)
                    {
                        start = reportDate[x];
                        break;
                    }
                }
                for (x = reportDate.Length - 1; x > -1; x--)
                {
                    if (reportDate[x] > DateTime.MinValue)
                    {
                        end =reportDate[x];
                        break;
                    }
                }
            }
            using (ISession session = NHibernateHelper.OpenSessionCustom(ConnectionString, "HiLo"))
            {
                Models.HiLo c = null;
                var res = session.QueryOver<Models.HiLo>(() => c)
                                        .SelectList(l => l
                                            .Select(s => s.DataDate)
                                            .Select(s => s.AvgVal)
                                            .Select(s => s.Hi)
                                            .Select(s => s.HiCPU)
                                            .Select(s => s.HiIntv)
                                            .Select(s => s.Lo)
                                            .Select(s => s.LoCPU)
                                            .Select(s => s.LoIntv)
                                            .Select(s => s.DataHour))
                                        .Where(() => c.SystemSerialNum == isystemSerial && c.AttributeID == 1 && c.DataDate >= start && c.DataDate <= end).List<object[]>();
                var datasetUtil = new DataSet();
                List<string> propNames = new List<string>() { "DataDate", "AvgVal", "Hi", "HiCPU", "HiIntv",
                                        "Lo", "LoCPU", "LoIntv", "DataHour"};
                DataTable dt = CollectionHelper.ListToDataTable(res, propNames);
                dt.TableName = "HourlyBusy";
                datasetUtil.Tables.Add(dt);
                myDataSet.Merge(datasetUtil);
            }
            return myDataSet;
            /*commandString = "SELECT DataDate, AvgVal, Hi,HiCpu,HiIntv,Lo,LoCpu,LoIntv,DataHour " +
                            "FROM HiLo  " +
                            "WHERE SystemSerialNum = @SystemSerial AND  " +
                            "DataDate BETWEEN @StartDate AND @EndDate " +
                            "AND AttributeID = '1' ";


            var connection = new MySqlConnection(ConnectionString);
            var selectCommand = new MySqlCommand(commandString, connection);

            var datasetUtil = new DataSet();
            //Add Parameter.
            selectCommand.Parameters.AddWithValue("@SystemSerial", isystemSerial);
            int x = 0;
            if (reportDate.Length > 1)
            {
                for (x = 0; x < reportDate.Length; x++)
                {
                    if (reportDate[x] > DateTime.MinValue)
                    {
                        selectCommand.Parameters.AddWithValue("@StartDate", reportDate[x].ToString("yyyy-MM-dd 00:00:00"));
                        break;
                    }
                }
                for (x = reportDate.Length - 1; x > -1; x--)
                {
                    if (reportDate[x] > DateTime.MinValue)
                    {
                        selectCommand.Parameters.AddWithValue("@EndDate", reportDate[x].ToString("yyyy-MM-dd 00:00:00"));
                        break;
                    }
                }
            }
            else
            {
                selectCommand.Parameters.AddWithValue("@StartDate", reportDate[0].ToString("yyyy-MM-dd 00:00:00"));
                selectCommand.Parameters.AddWithValue("@EndDate", reportDate[0].ToString("yyyy-MM-dd 23:59:00"));
            }

            selectCommand.CommandTimeout = 10000;

            var adapter = new MySqlDataAdapter(selectCommand);

            adapter.Fill(datasetUtil, "HourlyBusy");

            myDataSet.Merge(datasetUtil);

            return myDataSet;*/
        }
    }
}
