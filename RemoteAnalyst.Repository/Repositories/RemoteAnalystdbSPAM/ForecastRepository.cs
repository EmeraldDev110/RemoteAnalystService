using System;
using System.Collections.Generic;
using RemoteAnalyst.Repository.Models;
using RemoteAnalyst.Repository.Helpers;
using NHibernate;
using NHibernate.Criterion;
using System.Data;
using System.Linq;
using NHibernate.Linq;
using MySqlConnector;

namespace RemoteAnalyst.Repository.Repositories
{
    public class ForecastRepository
    {
        private readonly string _connectionString;

        public ForecastRepository(string connectionString)
        {
            _connectionString = connectionString;
        }


        public DataTable GetForecastData(DateTime startTime, DateTime stopTime)
        {
            var perference = new DataTable();
            string cmdText = @"SELECT FromTimestamp, CpuNumber, CPUBusy, MemoryUsed, CPUQueue, StdDevCPUBusy, StdDevMemoryUsed, StdDevCPUQueue
                               FROM Forecasts WHERE FromTimestamp >= @StartTime AND FromTimestamp <= @StopTime";

            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
            {
                ICollection<Forecast> res = session.QueryOver<Forecast>()
                    .WhereRestrictionOn(x => x.FromTimestamp).IsBetween(startTime).And(stopTime)
                    .List();
                perference = CollectionHelper.ToDataTable(res);
            }

            return perference;
        }

        public DataTable GetForecastIpuData(DateTime startTime, DateTime stopTime)
        {
            var perference = new DataTable();
            string cmdText = @"SELECT FromTimestamp, CpuNumber, IpuNumber, IpuBusy, IpuQueue, StdDevIpuBusy, StdDevIpuQueue
                               FROM ForecastIpus WHERE FromTimestamp >= @StartTime AND FromTimestamp <= @StopTime";

            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
            {
                ICollection<ForecastIPU> res = session.QueryOver<ForecastIPU>()
                    .WhereRestrictionOn(x => x.FromTimestamp).IsBetween(startTime).And(stopTime)
                    .List();
                perference = CollectionHelper.ToDataTable(res);
            }

            return perference;
        }

        public DataTable GetForecastDiskData(DateTime startTime, DateTime stopTime)
        {
            var perference = new DataTable();
            string cmdText = @"SELECT FromTimestamp, DeviceName, QueueLength, StdDevQueueLength, DP2Busy, StdDevDP2Busy
                               FROM ForecastDisks WHERE FromTimestamp >= @StartTime AND FromTimestamp <= @StopTime";

            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
            {
                ICollection<ForecastDisk> res = session.QueryOver<ForecastDisk>()
                    .WhereRestrictionOn(x => x.FromTimestamp).IsBetween(startTime).And(stopTime)
                    .List();
                perference = CollectionHelper.ToDataTable(res);
            }
            return perference;
        }

        public DataTable GetForecastStorageData(DateTime startTime, DateTime stopTime)
        {
            var perference = new DataTable();
            string cmdText = @"SELECT FromTimestamp, DeviceName, UsedPercent, StdDevUsedPercent
                               FROM ForecastStorages WHERE FromTimestamp >= @StartTime AND FromTimestamp <= @StopTime";

            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
            {
                ICollection<ForecastStorage> res = session.QueryOver<ForecastStorage>()
                    .WhereRestrictionOn(x => x.FromTimestamp).IsBetween(startTime).And(stopTime)
                    .List();
                perference = CollectionHelper.ToDataTable(res);
            }

            return perference;
        }

        public DataTable GetForecastProcessData(DateTime startTime, DateTime stopTime)
        {
            var perference = new DataTable();
            string cmdText = @"SELECT FromTimestamp, ProcessName, CpuNumber, Pin, Volume, SubVol, FileName, ProcessBusy,
                                StdDevProcessBusy, RecvQueueLength, StdDevRecvQueueLength
                               FROM ForecastProcesses WHERE FromTimestamp >= @StartTime AND FromTimestamp <= @StopTime";

            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
            {
                ICollection<ForecastProcess> res = session.QueryOver<ForecastProcess>()
                    .WhereRestrictionOn(x => x.FromTimestamp).IsBetween(startTime).And(stopTime)
                    .List();
                perference = CollectionHelper.ToDataTable(res);
            }
            return perference;
        }

        public DataTable GetForecastTmfData(DateTime startTime, DateTime stopTime)
        {
            var perference = new DataTable();
            string cmdText = @"SELECT FromTimestamp, ProcessName, CpuNumber, Pin, Volume, SubVol, FileName, AbortPercent, StdDevAbortPercent
                               FROM ForecastTmfs WHERE FromTimestamp >= @StartTime AND FromTimestamp <= @StopTime";

            using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
            {
                ICollection<ForecastTmf> res = session.QueryOver<ForecastTmf>()
                    .WhereRestrictionOn(x => x.FromTimestamp).IsBetween(startTime).And(stopTime)
                    .List();
                perference = CollectionHelper.ToDataTable(res);
            }
            return perference;
        }
    }
}
