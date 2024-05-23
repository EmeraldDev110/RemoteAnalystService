using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using System.Data;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class ScheduleService {
        private readonly string _connectionString;

        public ScheduleService(string connectionStriong)
        {
            _connectionString = connectionStriong;
        }

        public string[] GetIgnoreVolumesFor(int scheduleId) {
            ScheduleStorageDetail scheduleStorageDetail = new ScheduleStorageDetail(_connectionString);
            string ignoreVolumeString = scheduleStorageDetail.GetIgnoreVolumes(scheduleId);
            string[] ignoreVolumes = ignoreVolumeString.Split(',');
            return ignoreVolumes;
        }

        public DataTable GetScheduleStorageThresholdFor(int scheduleId) {  
            ScheduleStorageDetail scheduleStorageDetail = new ScheduleStorageDetail(_connectionString);
            DataTable storageThresh = scheduleStorageDetail.getStorageThreshold(scheduleId);
            return storageThresh;
        }

        public List<ScheduleView> GetSchdulesFor(int typeID) {
            var schedules = new Schedules(_connectionString);
            DataTable scheduleData = schedules.GetSchedules(typeID);

            var scheduleList = new List<ScheduleView>();

            foreach (DataRow dr in scheduleData.Rows) {
                var view = new ScheduleView {
                    SystemSerial = dr["SystemSerial"].ToString(),
                    SystemName = dr["SystemName"].ToString(),
                    ScheduleId = Convert.ToInt32(dr["ScheduleId"]),
                    Type = GetTypeName(Convert.ToInt32(dr["TypeId"])),
                    Frequency = GetFrequencyName(Convert.ToInt32(dr["FrequencyId"])),
                    DailyOn = dr.IsNull("DailyOn") ? 0 : Convert.ToInt32(dr["DailyOn"]),
                    DailyAt = dr.IsNull("DailyAt") ? 0 : Convert.ToInt32(dr["DailyAt"]),
                    WeeklyOn = dr.IsNull("WeeklyOn") ? 0 : Convert.ToInt32(dr["WeeklyOn"]),
                    WeeklyFor = dr.IsNull("WeeklyFor") ? 0 : Convert.ToInt32(dr["WeeklyFor"]),
                    WeeklyFrom = dr.IsNull("WeeklyFrom") ? 0 : Convert.ToInt32(dr["WeeklyFrom"]),
                    WeeklyTo = dr.IsNull("WeeklyTo") ? 0 : Convert.ToInt32(dr["WeeklyTo"]),
                    MonthlyOn = dr.IsNull("MonthlyOn") ? 0 : Convert.ToInt32(dr["MonthlyOn"]),
                    MonthlyOnWeekDay = dr.IsNull("MonthlyOnWeekDay") ? 0 : Convert.ToInt32(dr["MonthlyOnWeekDay"]),
                    MonthlyFor = dr.IsNull("MonthlyFor") ? 0 : Convert.ToInt32(dr["MonthlyFor"]),
                    MonthlyFrom = dr.IsNull("MonthlyFrom") ? 0 : Convert.ToInt32(dr["MonthlyFrom"]),
                    MonthlyTo = dr.IsNull("MonthlyTo") ? 0 : Convert.ToInt32(dr["MonthlyTo"]),
                    IsMonthlyOn = !dr.IsNull("IsMonthlyOn") && Convert.ToBoolean(dr["IsMonthlyOn"]),
                    IsMonthlyFor = !dr.IsNull("IsMonthlyFor") && Convert.ToBoolean(dr["IsMonthlyFor"]),
                    DetailTypeId = dr.IsNull("DetailTypeId") ? "0" : Convert.ToString(dr["DetailTypeId"]),
                    Email = dr["Email"].ToString(),
                    ReportFromHour = dr.IsNull("ReportFromHour")? "" : dr["ReportFromHour"].ToString(),
                    ReportToHour = dr.IsNull("ReportToHour") ? "" : dr["ReportToHour"].ToString(),
                    AlertException = !dr.IsNull("AlertException") && Convert.ToBoolean(dr["AlertException"]),
                    HourBoundaryTrigger = dr.IsNull("HourBoundaryTrigger") ? true : Convert.ToBoolean(dr["HourBoundaryTrigger"]),
                    Overlap = Convert.ToBoolean(dr["Overlapping"]),
                    BatchProgram = dr.IsNull("ProgramFile") ? "" : dr["ProgramFile"].ToString(),
                    BatchId = dr.IsNull("BatchSequenceProfileId") ? "" : dr["BatchSequenceProfileId"].ToString(),
                    ReportDownloadId = dr["ReportDownloadId"].ToString(),
                };
                scheduleList.Add(view);
            }

            return scheduleList;
        }

        public DataTable GetPinInfoFor(int scheduleId) {
            var schedules = new Schedules(_connectionString);
            var pinInfo = schedules.GetPinInfo(scheduleId);

            return pinInfo;
        }

        public string GetQTParamFor(int scheduleId) {
            var schedules = new Schedules(_connectionString);
            var scheduleData = schedules.GetQTParam(scheduleId);

            return scheduleData;
        }

        public string GetDDParamFor(int scheduleId) {
            var schedules = new Schedules(_connectionString);
            var scheduleData = schedules.GetDDParam(scheduleId);

            return scheduleData;
        }

        private string GetTypeName(int typeId) {
            var name = "";
            switch (typeId) {
                case 3:
                    name = "Quick Tuner"; break;
                case 4:
                    name = "Deep Dive"; break;
                case 5:
                    name = "Storage"; break;
                case 6:
                    name = "Network"; break;
                case 7:
                    name = "Daily"; break;
                case 8:
                    name = "Weekly"; break;
                case 9:
                    name = "Monthly"; break;
				case 11:
					name = "Pathway"; break;
            }
            return name;
        }
        private string GetFrequencyName(int freq) {
            var name = "";
            switch (freq) {
                case 1:
                    name = "Daily"; break;
                case 2:
                    name = "Weekly"; break;
                case 3:
                    name = "Monthly"; break;
            }
            return name;
        }
    }
}
