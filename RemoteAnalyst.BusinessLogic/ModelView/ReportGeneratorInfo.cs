using System;
using System.Web.Script.Serialization;

namespace RemoteAnalyst.BusinessLogic.ModelView {
    public class ReportGeneratorInfo {
        public string InstanceID { get; set; }
        public string InstanceStartTime { get; set; }
        public string InstanceRunningTime { get; set; }
        public static string GetReportGeneratorInfo(string instanceID, DateTime instanceStartTime, TimeSpan instanceRunningTime) {
            var info = new ReportGeneratorInfo {
                InstanceID = instanceID,
                InstanceStartTime = instanceStartTime.ToString(),
                InstanceRunningTime = Math.Round(instanceRunningTime.TotalMinutes) + " minutes"
            };
            return new JavaScriptSerializer().Serialize(info);
        }
    }
}