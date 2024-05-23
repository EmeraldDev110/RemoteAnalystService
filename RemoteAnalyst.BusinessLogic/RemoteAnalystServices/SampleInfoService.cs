using System;
using System.Data;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices
{
    public class SampleInfoService
    {
        private readonly string ConnectionString = "";

        public SampleInfoService(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public void InsertNewEntryFor(int nsid, string systemName, string systemSerial, DateTime startTimeLCT,
            DateTime stopTimeLCT, long sampleInterval, int uwsID, string sysContent, int type)
        {
            var sampleInfo = new SampleInfo(ConnectionString);
            sampleInfo.InsertNewEntry(nsid, systemName, systemSerial, startTimeLCT, stopTimeLCT, sampleInterval, uwsID,
                sysContent, type);
        }

        public bool CheckDuplicateDataFor(string systemSerial, DateTime startTime, DateTime endTime, bool isSystem)
        {
            var sampleInfo = new SampleInfo(ConnectionString);
            bool isDupliacate = sampleInfo.CheckDuplicateData(systemSerial, startTime, endTime, isSystem);
            return isDupliacate;
        }

        public void UpdateExpireInfoFor(DateTime retentionDate, string newNsid)
        {
            var sampleInfo = new SampleInfo(ConnectionString);
            sampleInfo.UpdateExpireInfo(retentionDate, newNsid);
        }

        public void UpdateStopTimeFor(string stopTime, string newNsid)
        {
            var sampleInfo = new SampleInfo(ConnectionString);
            sampleInfo.UpdateStopTime(stopTime, newNsid);
        }
    }
}