using System;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices
{
    public class MonthlyDiskService
    {
        private readonly string _connectionString;

        public MonthlyDiskService(string connectionStringTrend)
        {
            _connectionString = connectionStringTrend;
        }

        public bool CheckDataFor(string systemSerial, DateTime date, string diskName)
        {
            var monthlyDisk = new MonthlyDisk(_connectionString);
            bool exits = monthlyDisk.CheckData(systemSerial, date, diskName);

            return exits;
        }

        public void InsertNewDataFor(string systemSerial, DateTime date, string diskName)
        {
            var monthlyDisk = new MonthlyDisk(_connectionString);
            monthlyDisk.InsertNewData(systemSerial, date, diskName);
        }

        public void UpdateDataFor(string systemSerial, string diskName, DateTime tempDate, double firstDayGB,
            double lastDayGB, double avgUsedGB, double deltaMB, double deltaPercent)
        {
            var monthlyDisk = new MonthlyDisk(_connectionString);
            monthlyDisk.UpdateData(systemSerial, diskName, tempDate, firstDayGB, lastDayGB, avgUsedGB, deltaMB,
                deltaPercent);
        }

        public void DeleteDataFor(DateTime oldDate) {
            var monthlyDisk = new MonthlyDisk(_connectionString);
            monthlyDisk.DeleteData(oldDate);
        }
    }
}