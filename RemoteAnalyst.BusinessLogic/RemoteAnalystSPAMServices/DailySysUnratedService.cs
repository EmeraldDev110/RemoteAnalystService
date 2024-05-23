using System;
using System.Collections.Generic;
using System.Data;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices
{
    public class DailySysUnratedService
    {
        private readonly string _connectionString = "";

        public DailySysUnratedService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DataSet GetDataDateFor(int attributeID, DateTime startDate, DateTime endDate, string systemSerial)
        {
            var sysUnrated = new DailySysUnratedRepository(_connectionString);
            DataSet dates = sysUnrated.GetDataDate(attributeID, startDate, endDate, systemSerial);

            return dates;
        }

        public IList<DailySysUnratedView> GetAllSystemDataFor()
        {
            var sysUnrated = new DailySysUnratedRepository(_connectionString);
            DataTable systemData = sysUnrated.GetAllSystemData();
            IList<DailySysUnratedView> dailySysUnrateds = new List<DailySysUnratedView>();

            foreach (DataRow dr in systemData.Rows)
            {
                var view = new DailySysUnratedView();
                view.SystemSerial = Convert.ToString(dr["SystemSerialNum"]);
                view.AttributeId = Convert.ToInt32(dr["AttributeId"]);
                view.Object = Convert.ToString(dr["Object"]);
                dailySysUnrateds.Add(view);
            }

            return dailySysUnrateds;
        }

        public DataTable GetHourlyDataFor(string systemSerial, int attributeId, string obj, int month, int year)
        {
            var sysUnrated = new DailySysUnratedRepository(_connectionString);
            DataTable hourlyData = sysUnrated.GetHourlyData(systemSerial, attributeId, obj, month, year);

            return hourlyData;
        }

        public void DeleteDataFor(DateTime oldDate)
        {
            var dailySysUnrated = new DailySysUnratedRepository(_connectionString);
            dailySysUnrated.DeleteData(oldDate);
        }

        public double CheckHourlyDataFor(string systemSerial, DateTime dataDate, int hour) {
            var dailySysUnrated = new DailySysUnratedRepository(_connectionString);
            var cpuBusy = dailySysUnrated.CheckHourlyData(systemSerial, dataDate, hour);

            return cpuBusy;
        }
    }
}