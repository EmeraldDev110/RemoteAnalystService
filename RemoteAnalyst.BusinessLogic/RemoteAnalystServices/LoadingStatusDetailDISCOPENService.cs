using System;
using System.Collections.Generic;
using System.Data;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class LoadingStatusDetailDISCOPENService {
        private readonly string _connectionString;

        public LoadingStatusDetailDISCOPENService(string connectionString) {
            _connectionString = connectionString;
        }

        public int GetCurrentQueLengthFor(string instanceID) {
            var loadingStatusDetail = new LoadingStatusDetailDISCOPEN(_connectionString);
            return loadingStatusDetail.GetCurrentQueLength(instanceID);
        }

        public int GetCurrentLoadLengthFor(string instanceID) {
            var loadingStatusDetail = new LoadingStatusDetailDISCOPEN(_connectionString);
            return loadingStatusDetail.GetCurrentLoadLength(instanceID);
        }

        public bool InsertLoadingStatusFor(string fileNmae, DateTime inQueTime, string systemSerial, string jobPoolName
            , DateTime selectedStartTime, DateTime selectedStopTime, string instanceID) {
            var loadingStatusDetail = new LoadingStatusDetailDISCOPEN(_connectionString);
            bool retval = loadingStatusDetail.InsertLoadingStatus(fileNmae, inQueTime, systemSerial, jobPoolName, selectedStartTime, selectedStopTime, instanceID);
            return retval;
        }

        public bool DeleteLoadingInfoFor(string fileName, string instanceID) {
            var loadingStatusDetail = new LoadingStatusDetailDISCOPEN(_connectionString);
            bool retval = loadingStatusDetail.DeleteLoadingInfo(fileName, instanceID);
            return retval;
        }

        public bool DeleteLoadingInfoFor(int loadingQueDISCOPENID) {
            var loadingStatusDetail = new LoadingStatusDetailDISCOPEN(_connectionString);
            bool retval = loadingStatusDetail.DeleteLoadingInfo(loadingQueDISCOPENID);
            return retval;
        }

        public bool UpdateLoadingStatusDetailFor(string flag, DateTime processingTime, string fileName, string systemSerial, string instanceID) {
            var loadingStatusDetail = new LoadingStatusDetailDISCOPEN(_connectionString);
            bool retval = loadingStatusDetail.UpdateLoadingStatusDetailDISCOPEN(flag, processingTime, fileName, systemSerial, instanceID);
            return retval;
        }

        public LoadingStatusDetailView GetLoadingStatusDetailFor(string instanceID) {
            var loadingStatusDetail = new LoadingStatusDetailDISCOPEN(_connectionString);
            DataTable dataTable = loadingStatusDetail.GetLoadingStatusDetail(instanceID);
            var loadingStatusDetailView = new LoadingStatusDetailView();

            if (dataTable.Rows.Count == 1) {
                DataRow dr = dataTable.Rows[0];
                loadingStatusDetailView.DataFileName = Convert.ToString(dr["FileName"]);
                loadingStatusDetailView.SystemSerial = Convert.ToString(dr["SystemSerial"]);
                loadingStatusDetailView.SelectedStartTime = Convert.ToDateTime(dr["SelectedStartTime"]);
                loadingStatusDetailView.SelectedStopTime = Convert.ToDateTime(dr["SelectedStopTime"]);
            }
            return loadingStatusDetailView;
        }

        public List<LoadingStatusDetailView> GetStoppedJobsFor(string intanceID) {
            var loadingStatusDetail = new LoadingStatusDetailDISCOPEN(_connectionString);
            DataTable dataTable = loadingStatusDetail.GetStoppedJobs(intanceID);

            var stoppedJobs = new List<LoadingStatusDetailView>();
            foreach (DataRow row in dataTable.Rows) {
                var view = new LoadingStatusDetailView {
                    SystemSerial = Convert.ToString(row["SystemSerial"]),
                    DataFileName = Convert.ToString(row["FileName"]),
                    UWSID = Convert.ToInt32(row["LoadingQueDISCOPENID"])
                };
                stoppedJobs.Add(view);
            }
            return stoppedJobs;
        }
    }
}