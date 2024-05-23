using System;
using System.Data;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices
{
    public class MonthlyAppUnratedService
    {
        private readonly string _connectionString;

        public MonthlyAppUnratedService(string connectionStringTrend)
        {
            _connectionString = connectionStringTrend;
        }

        public bool CheckDataFor(string systemSerial, DateTime date, int attributeId, string obj)
        {
            var monthlyAppUnrated = new MonthlyAppUnrated(_connectionString);
            bool exits = monthlyAppUnrated.CheckData(systemSerial, date, attributeId, obj);

            return exits;
        }

        public void InsertNewDataFor(string systemSerial, DateTime date, int attributeId, string obj)
        {
            var monthlyAppUnrated = new MonthlyAppUnrated(_connectionString);
            monthlyAppUnrated.InsertNewData(systemSerial, date, attributeId, obj);
        }

        public DataTable GetHourlyDataFor(string systemSerial, int attributeId, string obj, DateTime date)
        {
            var monthlyAppUnrated = new MonthlyAppUnrated(_connectionString);
            DataTable hourlyData = monthlyAppUnrated.GetHourlyData(systemSerial, attributeId, obj, date);

            return hourlyData;
        }

        public void UpdateDataFor(DataTable monthlyData)
        {
            var monthlyAppUnrated = new MonthlyAppUnrated(_connectionString);
            monthlyAppUnrated.UpdateData(monthlyData);
        }

        public void UpdateDataFor(double sumval, double avgVal, int peakhour, int numhours, string systemSerial,
            int attributeId, string obj, DateTime date, string hourValue)
        {
            var monthlyAppUnrated = new MonthlyAppUnrated(_connectionString);
            monthlyAppUnrated.UpdateData(sumval, avgVal, peakhour, numhours, systemSerial, attributeId, obj, date, hourValue);
        }
    }
}