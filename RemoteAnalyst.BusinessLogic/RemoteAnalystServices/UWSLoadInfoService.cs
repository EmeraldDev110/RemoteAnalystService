using System;
using System.Collections.Generic;
using System.Data;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices
{
    public class UWSLoadInfoService
    {
        private readonly string _connectionString = "";

        public UWSLoadInfoService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void InsertDataFor(string systemSerial, DateTime startTime, DateTime stopTime, DateTime loadedStartTime,
            DateTime loadedStopTime)
        {
            var loadingInfo = new UWSLoadInfos(_connectionString);
            loadingInfo.InsertData(systemSerial, startTime, stopTime, loadedStartTime, loadedStopTime);
        }

        public List<LoadedTime> GetLoadedTimeFor(string systemSerial, DateTime startTime, DateTime stopTime)
        {
            var loadingInfo = new UWSLoadInfos(_connectionString);

            var loadedTimes = new List<LoadedTime>();
            var list = new DataTable();
            list = loadingInfo.GetLoadedTime(systemSerial, startTime, stopTime);

            foreach (DataRow dr in list.Rows)
            {
                var loadedTime = new LoadedTime();
                loadedTime.LoadedStartTime = Convert.ToDateTime(dr["LoadedStartTime"]);
                loadedTime.LoadedStopTime = Convert.ToDateTime(dr["LoadedStopTime"]);
                loadedTimes.Add(loadedTime);
            }

            var loadTime = new LoadedTime();
            List<LoadedTime> mergedTimes = loadTime.MergeContinuesTime(loadedTimes);

            return loadedTimes;
        }

        public bool CheckLoadedTimeFor(string systemSerial, DateTime startTime, DateTime stopTime) {
            var loadingInfo = new UWSLoadInfos(_connectionString);
            var exist = loadingInfo.CheckLoadedTime(systemSerial, startTime, stopTime);

            return exist;
        }
    
    }
}