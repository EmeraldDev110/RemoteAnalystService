using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices
{
    public class DiskInfoService
    {
        private readonly string _connectionString;

        public DiskInfoService(string connectionStringTrend)
        {
            _connectionString = connectionStringTrend;
        }

        public double GetAveragedUsedGBFor(string systemSerial, string diskName)
        {
            var diskInfo = new DiskInfo(_connectionString);
            double capacityGB = diskInfo.GetAveragedUsedGB(systemSerial, diskName);

            return capacityGB;
        }
    }
}