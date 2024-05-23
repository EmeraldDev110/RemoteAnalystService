using System;
using System.Data;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices
{
    public class MonthlySysUnratedService
    {
        private readonly string _connectionString;

        public MonthlySysUnratedService(string connectionStringTrend)
        {
            _connectionString = connectionStringTrend;
        }

        public bool CheckDataFor(string systemSerial, DateTime date, int attributeId, string obj)
        {
            var monthlySysUnrated = new MonthlySysUnrated(_connectionString);
            bool exits = monthlySysUnrated.CheckData(systemSerial, date, attributeId, obj);

            return exits;
        }

        public void InsertNewDataFor(string systemSerial, DateTime date, int attributeId, string obj)
        {
            var monthlySysUnrated = new MonthlySysUnrated(_connectionString);
            monthlySysUnrated.InsertNewData(systemSerial, date, attributeId, obj);
        }

        public DataTable GetHourlyDataFor(string systemSerial, int attributeId, string obj, DateTime date)
        {
            var monthlySysUnrated = new MonthlySysUnrated(_connectionString);
            DataTable hourlyData = monthlySysUnrated.GetHourlyData(systemSerial, attributeId, obj, date);

            return hourlyData;
        }

        public void UpdateDataFor(double sumval, double avgValue, int peakhour, int numhours, string systemSerial,
            int attributeId, string obj, DateTime date, string hourValues)
        {
            var monthlySysUnrated = new MonthlySysUnrated(_connectionString);
            monthlySysUnrated.UpdateData(sumval, avgValue, peakhour, numhours, systemSerial, attributeId, obj, date, hourValues);
        }
    }
}