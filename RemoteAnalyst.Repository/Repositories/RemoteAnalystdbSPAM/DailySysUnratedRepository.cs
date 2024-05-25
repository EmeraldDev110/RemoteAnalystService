using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using RemoteAnalyst.Repository.Helpers;
using RemoteAnalyst.Repository.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace RemoteAnalyst.Repository.Repositories
{
    public class DailySysUnratedRepository
    {
        private readonly string _connectionString;

        public DailySysUnratedRepository(string connectionStringTrend)
        {
            _connectionString = connectionStringTrend;
        }

        public DataTable GetAllSystemData()
        {
            string cmdText = @"SELECT DISTINCT SystemSerialNum, AttributeId, Object FROM DailySysUnrated
                               WHERE SystemSerialNum != '000000' ORDER BY SystemSerialNum";
            var systemData = new DataTable();

            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
            {
                ICollection<object[]> res = session.QueryOver<DailySysUnrated>()
                    .Where(x => x.SystemSerialNum != "000000")
                    .SelectList(list => list
                        .Select(x => x.SystemSerialNum)
                        .Select(x => x.AttributeID)
                        .Select(x => x.Object)
                    ).OrderBy(x => x.SystemSerialNum).Asc
                    .List<object[]>();
                systemData = CollectionHelper.ListToDataTable(res, new List<string>() { "SystemSerialNum", "AttributeId", "Object" });
            }

            return systemData;
        }
        public DataSet GetDataDate(int attributeID, DateTime startDate, DateTime endDate, string systemSerial)
        {

            string cmdText = @"SELECT DISTINCT DataDate FROM DailySysUnrated
                                WHERE SystemSerialNum = @SystemSerial AND AttributeID = @AttributeID 
                                AND DataDate BETWEEN @StartDate AND @EndDate
                                AND AvgVal IS NOT NULL ORDER BY DataDate ASC";
            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
            {
                ICollection<DateTime> res = session
                    .CreateCriteria(typeof(DailySysUnrated))
                    .SetProjection(Projections.Distinct(Projections.Property("DataDate")))
                    .Add(Restrictions.Eq("SystemSerialNum", systemSerial))
                    .Add(Restrictions.Eq("AttributeID", attributeID))
                    .Add(Restrictions.Between("DataDate", startDate, endDate))
                    .Add(Restrictions.IsNotNull("AvgVal"))
                    .AddOrder(Order.Asc("DataDate"))
                    .List<DateTime>();
                DataTable dt = CollectionHelper.ToDataTable(res, new List<string>() { "Date" });
                dt.Columns["Date"].ColumnName = "DataDate";
                DataSet ds = new DataSet();
                ds.Tables.Add(dt);
                ds.Tables[0].TableName = "Interval";
                return ds;
            }
        }
        public DataTable GetHourlyData(string systemSerial, int attributeId, string obj, int month, int year)
        {
            string cmdText = @"SELECT `Hour0` ,`Hour1` ,`Hour2`  ,`Hour3` ,`Hour4` ,`Hour5` ,`Hour6`
                            ,`Hour7` ,`Hour8` ,`Hour9` ,`Hour10` ,`Hour11` ,`Hour12` ,`Hour13` ,`Hour14`
                            ,`Hour15` ,`Hour16` ,`Hour17` ,`Hour18` ,`Hour19` ,`Hour20` ,`Hour21` ,`Hour22` ,`Hour23`
                            , AvgVal, PeakHour, NumHours
                            FROM DailySysUnrated WHERE MONTH(DataDate)= @Month AND YEAR(DataDate)= @Year AND 
                            SystemSerialNum = @SystemSerial AND AttributeId = @AttributeId AND Object = @Object";

            var systemData = new DataTable();
            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
            {
                ICollection<DailySysUnrated> res = session.QueryOver<DailySysUnrated>()
                    .Where(x => x.DataDate.Month == month)
                    .And(x => x.DataDate.Year == year)
                    .And(x => x.SystemSerialNum == systemSerial)
                    .And(x => x.AttributeID == attributeId)
                    .And(x => x.Object == obj)
                    .List<DailySysUnrated>();
                systemData = CollectionHelper.ToDataTable(res);
            }
            return systemData;
        }

        public void DeleteData(DateTime oldDate)
        {
            string cmdText = "DELETE FROM DailySysUnrated WHERE DataDate < @OldDate";

            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
            {
                session.Query<DailySysUnrated>()
                    .Where(a => a.DataDate < oldDate)
                    .Delete();
            }
        }

        public double CheckHourlyData(string systemSerial, DateTime dataDate, int hour)
        {
            double cpuBusy = 0D;

            var cmdText = @"SELECT Hour" + hour + @" FROM DailySysUnrated
                        WHERE SystemSerialNum = @SystemSerial AND AttributeID = 1 AND Object = '00'
                        AND DataDate = @StartDate";

            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
            {
                cpuBusy = session.CreateCriteria<DailySysUnrated>()
                    .Add(Restrictions.Eq("SystemSerialNum", systemSerial))
                    .Add(Restrictions.Eq("AttributeID", 1))
                    .Add(Restrictions.Eq("Object", "00"))
                    .Add(Restrictions.Eq("DataDate", dataDate))
                    .SetProjection(Projections.Property("Hour" + hour))
                    .UniqueResult<double>();
            }

            return cpuBusy;
        }
    }
}
