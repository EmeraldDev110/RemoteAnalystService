using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using RemoteAnalyst.BusinessLogic.Enums;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices
{
    public class LoadingInfoService {
        private readonly string _connectionString = "";

        public LoadingInfoService(string connectionString) {
            _connectionString = connectionString;
        }

        public int GetMaxUWSIDFor() {
            var uwsID = 0;
            try {
                var loadingInfo = new LoadingInfo(_connectionString);
                uwsID = loadingInfo.GetMaxUWSID();

                loadingInfo.Insert(uwsID, 0);
            }
            catch { }
            return uwsID;
        }

        public void UpdateUWSRelayTimeFor(string systemSerial, int uwsId, DateTime ftpreceivedTime, DateTime s3SentTime, string fileName, long fileSize, int sampleType) {
            try {
                //Get rds Name.
                var databaseMapping = new DatabaseMappingService(_connectionString);
                var connectionString = databaseMapping.GetConnectionStringFor(systemSerial);

                var rdsName = "";
                var tempString = connectionString.Split(';');
                foreach (var s in tempString) {
                    if (s.ToUpper().Contains("SERVER=")) {
                        rdsName = s.Split('=')[1].Split('.')[0];
                    }
                }

                var loadingInfo = new LoadingInfo(_connectionString);
                loadingInfo.UpdateUWSRelayTime(systemSerial, uwsId, ftpreceivedTime, s3SentTime, fileName, fileSize, sampleType, rdsName);
            }
            catch { }
        }

        public void UpdateUWSRelayTimeFor(string systemSerial, int uwsId, DateTime ftpreceivedTime, DateTime s3SentTime, DateTime collectionStartTime, DateTime collectionStopTime, string fileName, long fileSize) {
            try {
                //Get rds Name.
                var databaseMapping = new DatabaseMappingService(_connectionString);
                var connectionString = databaseMapping.GetConnectionStringFor(systemSerial);

                var rdsName = "";
                var tempString = connectionString.Split(';');
                foreach (var s in tempString) {
                    if (s.ToUpper().Contains("SERVER=")) {
                        rdsName = s.Split('=')[1].Split('.')[0];
                    }
                }

                var loadingInfo = new LoadingInfo(_connectionString);
                loadingInfo.UpdateUWSRelayTime(systemSerial, uwsId, ftpreceivedTime, s3SentTime, collectionStartTime, collectionStopTime, fileName, fileSize, rdsName);
            }
            catch { }

        }
        public IDictionary<string, string> GetSystemInfoFor(string uwsID) {
            var loadingInfo = new LoadingInfo(_connectionString);
            DataTable systemInfo = loadingInfo.GetSystemInfo(uwsID);
            IDictionary<string, string> loadingInfoView = new Dictionary<string, string>();
            if (systemInfo.Rows.Count == 1) {
                DataRow dr = systemInfo.Rows[0];
                loadingInfoView.Add(Convert.ToString(dr["systemserial"]), Convert.ToString(dr["filename"]));
            }
            return loadingInfoView;
        }

        public LoadingInfoView GetLoadingPeriodFor(string uwsID) {
            var loadingInfo = new LoadingInfo(_connectionString);
            DataTable loadingPeriod = loadingInfo.GetLoadingPeriod(uwsID);
            var loadingInfoView = new LoadingInfoView();
            if (loadingPeriod.Rows.Count == 1) {
                DataRow dr = loadingPeriod.Rows[0];
                if (!dr.IsNull("starttime")) {
                    loadingInfoView.StartTime = Convert.ToDateTime(dr["starttime"]);
                }
                if (!dr.IsNull("stoptime")) {
                    loadingInfoView.StopTime = Convert.ToDateTime(dr["stoptime"]);
                }
                if (!dr.IsNull("SampleType")) {
                    loadingInfoView.SampleType = Convert.ToInt32(dr["SampleType"]);
                }
            }
            return loadingInfoView;
        }

        public LoadingInfoView GetLoadingInfoFor(int uwsid) {
            var loadingInfoView = new LoadingInfoView();
            var loadingInfo = new LoadingInfo(_connectionString);
            DataTable loadingInfoTable = loadingInfo.GetLoadingInfo(uwsid);
            if (loadingInfoTable.Rows.Count == 1) {
                DataRow dr = loadingInfoTable.Rows[0];
                if (!dr.IsNull("systemname")) {
                    loadingInfoView.SystemName = Convert.ToString(dr["systemname"]);
                }
                if (!dr.IsNull("starttime")) {
                    loadingInfoView.StartTime = Convert.ToDateTime(dr["starttime"]);
                }
                if (!dr.IsNull("stoptime")) {
                    loadingInfoView.StopTime = Convert.ToDateTime(dr["stoptime"]);
                }
                if (!dr.IsNull("SampleType")) {
                    loadingInfoView.SampleType = Convert.ToInt32(dr["SampleType"]);
                }
            }
            return loadingInfoView;
        }

        public IDictionary<string, int> GetUWSRetentionDayFor() {
            var loadingInfo = new LoadingInfo(_connectionString);
            IDictionary<string, int> retentionDays = loadingInfo.GetUWSRetentionDay();
            return retentionDays;
        }

        public IDictionary<string, int> GetExpertReportRetentionDayFor() {
            var loadingInfo = new LoadingInfo(_connectionString);

            IDictionary<string, int> retentionDays = new Dictionary<string, int>();
            retentionDays = loadingInfo.GetExpertReportRetentionDay();

            return retentionDays;
        }

        public IDictionary<string, int> GetQNMRetentionDayFor() {
            var loadingInfo = new LoadingInfo(_connectionString);

            IDictionary<string, int> retentionDays = new Dictionary<string, int>();
            retentionDays = loadingInfo.GetQNMRetentionDay();

            return retentionDays;

        }
        public IDictionary<string, int> GetPathwayRetentionDayFor() {
            var loadingInfo = new LoadingInfo(_connectionString);

            IDictionary<string, int> retentionDays = new Dictionary<string, int>();
            retentionDays = loadingInfo.GetPathwayRetentionDay();

            return retentionDays;
        }

        public List<string> GetUWSFileNameFor(string systemSerial, DateTime uploadedtime) {
            var loadingInfo = new LoadingInfo(_connectionString);

            List<string> fileName = loadingInfo.GetUWSFileName(systemSerial, uploadedtime);

            return fileName;
        }

        public void InsertFor(int tempUWSID, int customerID) {
            var loadingInfo = new LoadingInfo(_connectionString);
            loadingInfo.Insert(tempUWSID, customerID);
        }

        public void UpdateFor(string filepath, string systemSerial, string fileSize, string fileType, string uwsID) {
            var loadingInfo = new LoadingInfo(_connectionString);
            loadingInfo.Update(filepath, systemSerial, fileSize, fileType, uwsID);
        }

        public void UpdateFor(int uwsID, string systemName, DateTime startTime, DateTime stopTime, int type) {
            var loadingInfo = new LoadingInfo(_connectionString);
            if (systemName.StartsWith(@"\\"))
                systemName = systemName.Replace(@"\\", @"\");

            loadingInfo.Update(uwsID, systemName, startTime, stopTime, type);
        }

        public void UpdateCollectionTimeFor(int uwsID, string systemName, DateTime startTime, DateTime stopTime, int type) {
            var loadingInfo = new LoadingInfo(_connectionString);
            if (systemName.StartsWith(@"\\"))
                systemName = systemName.Replace(@"\\", @"\");

            loadingInfo.UpdateCollectionTime(uwsID, systemName, startTime, stopTime, type);
        }

        public void UpdateCollectionTimeFor(int uwsID, DateTime startTime, DateTime stopTime) {
            var loadingInfo = new LoadingInfo(_connectionString);
            loadingInfo.UpdateCollectionTime(uwsID, startTime, stopTime);
        }
        public void UpdateLoadedTimeFor(int uwsID) {
            var loadingInfo = new LoadingInfo(_connectionString);
            loadingInfo.UpdateLoadedTime(uwsID);
        }

        public void UpdateFor(string uwsID) {
            var loadingInfo = new LoadingInfo(_connectionString);

            loadingInfo.Update(uwsID);
        }

        public void UpdateFileStatFor(string systemSerial, DateTime endDate) {
            var loadingInfo = new LoadingInfo(_connectionString);
            loadingInfo.UpdateFileStat(systemSerial, endDate);
        }

        public void UpdateLoadingStatusFor(int uwsID, string status) {
            var loadingInfo = new LoadingInfo(_connectionString);
            loadingInfo.UpdateLoadingStatus(uwsID, status);
        }

        public void UpdateStopTimeFor(string stopTime, int uwsID) {
            var loadingInfo = new LoadingInfo(_connectionString);
            loadingInfo.UpdateStopTime(stopTime, uwsID);
        }

        public string GetUMPFullFileNameFor(string fileName, string systemSerial, bool jumpToNextDay) {
            var loadingInfo = new LoadingInfo(_connectionString);
            return loadingInfo.GetUMPFullFileName(fileName, systemSerial, jumpToNextDay);
        }

        public string GetLoadCompleteTimeFor(string fileName, string systemSerial) {
            var loadingInfo = new LoadingInfo(_connectionString);
            return loadingInfo.GetLoadCompleteTime(fileName, systemSerial);
        }

        public void UpdateStatusFor(List<LoadingInfoParameter> parameters) {
            var loadingInfo = new LoadingInfo(_connectionString);
            loadingInfo.UpdateStatus(parameters);
        }

        public void BulkUpdateStatusFor(string systemSerial, int sampleType, DateTime startTime) {
            var loadingInfo = new LoadingInfo(_connectionString);
            loadingInfo.BulkUpdateStatus(systemSerial, sampleType, startTime);
        }

        public void UpdateInstanceIDFor(int uwsId, string instanceId) {
            var loadingInfo = new LoadingInfo(_connectionString);
            loadingInfo.UpdateInstanceID(uwsId, instanceId);
        }

        public List<long> GetLoadCountTodayFor(string instanceId) {
            var loadingInfo = new LoadingInfo(_connectionString);
            var loadInfo = loadingInfo.GetLoadInfoForToday(instanceId, DateTime.Now);

            return loadInfo;
        }

        public List<long> GetRdsLoadCountTodayFor(string rdsName) {
            var loadingInfo = new LoadingInfo(_connectionString);
            var loadInfo = loadingInfo.GetRdsLoadInfoForToday(rdsName, DateTime.Now);

            return loadInfo;
        }
        public List<long> GetRdsOtherLoadCountTodayFor(string rdsName) {
            var loadingInfo = new LoadingInfo(_connectionString);
            var loadInfo = loadingInfo.GetRdsOtherLoadInfoForToday(rdsName, DateTime.Now);

            return loadInfo;
        }

        public void DeleteLoadingInfoOlderThanXDaysAgo(int loadingInfoRetentionDays) {
            var loadingInfo = new LoadingInfo(_connectionString);
            loadingInfo.DeleteLoadingInfoOlderThanXDaysAgo(loadingInfoRetentionDays);
        }

		public void DeleteLoadingInfoByUWSID(int uwsId) {
			var loadingInfo = new LoadingInfo(_connectionString);
			loadingInfo.DeleteLoadingInfoByUWSID(uwsId);
		}

        public void DeleteLoadingInfoByFileName(string fileName) {
            var loadingInfo = new LoadingInfo(_connectionString);
            loadingInfo.DeleteLoadingInfoByFileName(fileName);
        }

        public bool IsUwsFileLoad(string uwsFileName) {
            var loadingInfo = new LoadingInfo(_connectionString);
            string loadStatus = loadingInfo.GetFirstUWSLoadRecordByUWSName(uwsFileName);
            if(loadStatus == "Sned" || loadStatus == "Lded") {
                return true;
            } else {
                return false;
            }
        }
    }
}