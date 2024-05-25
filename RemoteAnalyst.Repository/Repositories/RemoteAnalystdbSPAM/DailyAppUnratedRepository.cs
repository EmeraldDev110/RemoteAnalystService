
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
    public class DailyAppUnratedRepository
    {
        private readonly string _connectionString;

        public DailyAppUnratedRepository(string connectionStringTrend)
        {
            _connectionString = connectionStringTrend;
        }

        public DataTable GetAllApplicationData()
        {
            string cmdText = @"SELECT DISTINCT SystemSerialNum, AttributeId, AppId FROM DailyAppUnrated
                                WHERE SystemSerialNum != '000000' ORDER BY SystemSerialNum";
            var systemData = new DataTable();

            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
            {
                ICollection<object[]> res = session.QueryOver<DailyAppUnrated>()
                    .Where(x => x.SystemSerialNum != "000000")
                    .SelectList(list => list
                        .Select(x => x.SystemSerialNum)
                        .Select(x => x.AttributeID)
                        .Select(x => x.AppId)
                    ).OrderBy(x => x.SystemSerialNum).Asc
                    .List<object[]>();
                systemData = CollectionHelper.ListToDataTable(res, new List<string>() { "SystemSerialNum", "AttributeId", "AppId" });
            }

            return systemData;
        }

        public DataTable GetHourlyData(string systemSerial, int attributeId, string obj, int month, int year)
        {
            string cmdText = @"SELECT `Hour0` ,`Hour1` ,`Hour2`  ,`Hour3` ,`Hour4` ,`Hour5` ,`Hour6`
                            ,`Hour7` ,`Hour8` ,`Hour9` ,`Hour10` ,`Hour11` ,`Hour12` ,`Hour13` ,`Hour14`
                            ,`Hour15` ,`Hour16` ,`Hour17` ,`Hour18` ,`Hour19` ,`Hour20` ,`Hour21` ,`Hour22` ,`Hour23` 
                            , AvgVal, PeakHour, NumHours
                            FROM DailyAppUnrated WHERE MONTH(DataDate)= @Month AND YEAR(DataDate)=@Year AND 
                            SystemSerialNum = @SystemSerial AND AttributeId = @AttributeId AND AppId = @Object";

            var systemData = new DataTable();
            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
            {
                ICollection<DailyAppUnrated> res = session.QueryOver<DailyAppUnrated>()
                    .Where(x => x.DataDate.Month == month)
                    .And(x => x.DataDate.Year == year)
                    .And(x => x.SystemSerialNum == systemSerial)
                    .And(x => x.AttributeID == attributeId)
                    .And(x => x.AppId == Convert.ToInt32(obj))
                    .List<DailyAppUnrated>();
                systemData = CollectionHelper.ToDataTable(res);
            }
            return systemData;
        }

        public void DeleteData(DateTime oldDate)
        {
            string cmdText = "DELETE FROM DailyAppUnrated WHERE DataDate < @OldDate";

            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
            {
                session.Query<DailyAppUnrated>()
                    .Where(a => a.DataDate < oldDate)
                    .Delete();
            }
        }
    }
}
