using System;
using System.Collections.Generic;
using System.Data;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class LoadingStatusDetailService {
        private readonly string _connectionString;

        public LoadingStatusDetailService(string connectionString) {
            _connectionString = connectionString;
        }

        public IDictionary<int, DateTime> GetProcessingTimeFor(string uwsFileName, string systemSerial) {
            var loadingStatusDetail = new LoadingStatusDetail(_connectionString);

            IDictionary<int, DateTime> loadingTime = loadingStatusDetail.GetProcessingTime(uwsFileName, systemSerial);
            return loadingTime;
        }

        public DateTime GetProcessingTimeFor(int loadingQueID) {
            var loadingStatusDetail = new LoadingStatusDetail(_connectionString);

            DateTime loadingTime = loadingStatusDetail.GetProcessingTime(loadingQueID);
            return loadingTime;
        }

        public int GetCurrentQueueLengthFor(string instanceID) {
            var loadingStatusDetail = new LoadingStatusDetail(_connectionString);
            return loadingStatusDetail.GetCurrentQueueLength(instanceID);
        }

        public bool CheckDuplicatedUWSFor(string fileName) {
            var loadingStatusDetail = new LoadingStatusDetail(_connectionString);
            bool retval = loadingStatusDetail.CheckDuplicatedUWS(fileName);
            return retval;
        }

        public bool InsertLoadingStatusFor(string fileNmae, string customerLogin, DateTime inQueTime,
            string systemSerial, string jobPoolName, int tempUWSID, long fileSize, string type, string instanceID) {
            var loadingStatusDetail = new LoadingStatusDetail(_connectionString);

            bool retval = loadingStatusDetail.InsertLoadingStatus(fileNmae, customerLogin, inQueTime, systemSerial,
                jobPoolName, tempUWSID, fileSize, type, instanceID);
            return retval;
        }

        public bool DeleteLoadingInfoFor(string fileName) {
            var loadingStatusDetail = new LoadingStatusDetail(_connectionString);

            bool retval = loadingStatusDetail.DeleteLoadingInfo(fileName);
            return retval;
        }

        public bool UpdateLoadingStatusDetailFor(string flag, DateTime processingTime, string fileName,
            string systemSerial) {
            var loadingStatusDetail = new LoadingStatusDetail(_connectionString);

            bool retval = loadingStatusDetail.UpdateLoadingStatusDetail(flag, processingTime, fileName, systemSerial);
            return retval;
        }

        public LoadingStatusDetailView GetLoadingStatusDetailFor(string instanceID) {
            var loadingStatusDetail = new LoadingStatusDetail(_connectionString);
            DataTable dataTable = loadingStatusDetail.GetLoadingStatusDetail(instanceID);
            var loadingStatusDetailView = new LoadingStatusDetailView();
            if (dataTable.Rows.Count == 1) {
                DataRow dr = dataTable.Rows[0];
                loadingStatusDetailView.DataFileName = Convert.ToString(dr["FileName"]);
                loadingStatusDetailView.SystemSerial = Convert.ToString(dr["SystemSerial"]);
                loadingStatusDetailView.UWSID = Convert.ToInt32(dr["TempUWSID"]);
                loadingStatusDetailView.LoadType = Convert.ToString(dr["Type"]);
            }
            return loadingStatusDetailView;
        }

        public List<LoadingStatusDetailView> GetStoppedJobsFor(string instanceId) {
            var loadingStatusDetail = new LoadingStatusDetail(_connectionString);
            DataTable dataTable = loadingStatusDetail.GetStoppedJobs(instanceId);
            List<LoadingStatusDetailView> stoppedJobs = new List<LoadingStatusDetailView>();
            foreach (DataRow row in dataTable.Rows) {
                LoadingStatusDetailView view = new LoadingStatusDetailView();
                view.SystemSerial = Convert.ToString(row["SystemSerial"]);
                view.DataFileName = Convert.ToString(row["FileName"]);
                stoppedJobs.Add(view);
            }
            return stoppedJobs;
        }

        public int GetCurrentLoadCountFor(string systemSerial, string instanceId) {
            var loadCount = 0;

            var loadingStatusDetail = new LoadingStatusDetail(_connectionString);
            loadCount = loadingStatusDetail.GetCurrentLoadCount(systemSerial, instanceId);

            return loadCount;
        }

        public void UpdateFileSizeFor(string fileName, long fileSize) {
            var loadingStatusDetail = new LoadingStatusDetail(_connectionString);
            loadingStatusDetail.UpdateFileSize(fileName, fileSize);
        }
    }
}