using System;
using System.Collections.Generic;
using System.Data;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices
{
    public class ReportDownloadService
    {
        private readonly string _connectionString;

        public ReportDownloadService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int InsertNewReportFor(string systemSerial, DateTime startDateTime, DateTime stopDateTime, int typeId, int custId) {
            var reportDownloads = new ReportDownloads(_connectionString);
            var reportDownloadId = reportDownloads.InsertNewReport(systemSerial, startDateTime, stopDateTime, typeId, custId);

            return reportDownloadId;
        }

        public void UpdateFileLocationFor(int reportDownlaodId, string fileLocation) {
            var reportDownloads = new ReportDownloads(_connectionString);
            reportDownloads.UpdateFileLocation(reportDownlaodId, fileLocation);
        }

        public void UpdateStatusFor(int reportDownlaodId, short status) {
            var reportDownloads = new ReportDownloads(_connectionString);
            reportDownloads.UpdateStatus(reportDownlaodId, status);
        }

        public List<int> GetProcessingIdsFor() {
			//need to uncomment
			var reportDownlods = new ReportDownloads(_connectionString);
			var reportDownloadIds = reportDownlods.GetProcessingIds();

			//need to remove
			//var reportDownloadIds = new List<int>();
			//reportDownloadIds.Add(19509);

			return reportDownloadIds;
        }

        public ReportDetail GetReportDetailFor(int reportDownloadId) {
            var reportDownlods = new ReportDownloads(_connectionString);
			var reportDetail = reportDownlods.GetReportDetail(reportDownloadId);

            var reportDetailInfo = new ReportDetail();

            if (reportDetail.Rows.Count > 0) {
                reportDetailInfo.SystemName = reportDetail.Rows[0]["SystemName"].ToString();
                reportDetailInfo.SystemSerial = reportDetail.Rows[0]["SystemSerial"].ToString();
                reportDetailInfo.StartTime = Convert.ToDateTime(reportDetail.Rows[0]["StartTime"]);
                reportDetailInfo.EndTime = Convert.ToDateTime(reportDetail.Rows[0]["EndTime"]);
				reportDetailInfo.ReportType = Convert.ToInt32(reportDetail.Rows[0]["TypeId"]).ToString();
				//reportDetailInfo.ReportType = Convert.ToInt32(reportDetail.Rows[0]["TypeId"]).Equals(3) ? "QT" : "DPA";
				if (reportDetail.Rows[0].IsNull("FullName"))
                    reportDetailInfo.OrderBy = "Scheduled";
                else
                    reportDetailInfo.OrderBy = reportDetail.Rows[0]["FullName"].ToString();
            }

            return reportDetailInfo;
        }

        public DateTime GetRequestDateFor(int reportDownlaodId) {
            var reportDownlods = new ReportDownloads(_connectionString);
            return reportDownlods.GetReportRequestDate(reportDownlaodId);
        }
    }
}