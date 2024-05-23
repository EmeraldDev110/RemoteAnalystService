using System;
using System.Collections.Generic;
using System.Data;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices
{
    public class DailyAppUnratedService
    {
        private readonly string _connectionString = "";

        public DailyAppUnratedService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IList<DailySysUnratedView> GetAllApplicationDataFor()
        {
            var sysUnrated = new DailyAppUnratedRepository(_connectionString);
            DataTable systemData = sysUnrated.GetAllApplicationData();
            IList<DailySysUnratedView> dailySysUnrateds = new List<DailySysUnratedView>();

            foreach (DataRow dr in systemData.Rows)
            {
                var view = new DailySysUnratedView();
                view.SystemSerial = Convert.ToString(dr["SystemSerialNum"]);
                view.AttributeId = Convert.ToInt32(dr["AttributeId"]);
                view.Object = Convert.ToString(dr["AppId"]);
                dailySysUnrateds.Add(view);
            }

            return dailySysUnrateds;
        }

        public DataTable GetHourlyDataFor(string systemSerial, int attributeId, string obj, int month, int year)
        {
            var appUnrated = new DailyAppUnratedRepository(_connectionString);
            DataTable hourlyData = appUnrated.GetHourlyData(systemSerial, attributeId, obj, month, year);

            return hourlyData;
        }

        public void DeleteDataFor(DateTime oldDate)
        {
            var dailyAppUnrated = new DailyAppUnratedRepository(_connectionString);
            dailyAppUnrated.DeleteData(oldDate);
        }
    }
}