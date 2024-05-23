using System;
using System.Collections.Generic;
using System.Data;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices
{
    public class DeliveryScheduleService
    {
        private readonly string _connectionString;

        public DeliveryScheduleService(string connectionStriong)
        {
            _connectionString = connectionStriong;
        }

        public IList<DeliveryScheduleView> GetSchdulesFor(int typeID)
        {
            var deliverySchedules = new DeliverySchedules(_connectionString);
            DataTable schedules = deliverySchedules.GetSchdules(typeID);
            IList<DeliveryScheduleView> allSchedules = new List<DeliveryScheduleView>();

            foreach (DataRow dr in schedules.Rows)
            {
                var view = new DeliveryScheduleView();
                view.DeliveryID = Convert.ToInt32(dr["DS_DeliveryID"]);
                view.TrendReportID = Convert.ToInt32(dr["DS_TrendReportID"]);
                view.SystemSerial = Convert.ToString(dr["DS_SystemSerial"]);
                view.FrequencyName = Convert.ToString(dr["FR_FrequencyName"]);
                view.SendDay = Convert.ToInt32(dr["DS_SendDay"]);
                view.SendMonth = Convert.ToInt32(dr["DS_SendMonth"]);
                allSchedules.Add(view);
            }

            return allSchedules;
        }

        public IList<DeliveryScheduleView> GetQTSchduleFor()
        {
            var deliverySchedules = new DeliverySchedules(_connectionString);
            DataTable schedules = deliverySchedules.GetQTSchdule();
            IList<DeliveryScheduleView> allSchedules = new List<DeliveryScheduleView>();

            foreach (DataRow dr in schedules.Rows)
            {
                var view = new DeliveryScheduleView();
                view.DeliveryID = Convert.ToInt32(dr["DS_DeliveryID"]);
                view.TrendReportID = Convert.ToInt32(dr["DS_TrendReportID"]);
                view.SystemSerial = Convert.ToString(dr["DS_SystemSerial"]);
                view.FrequencyName = Convert.ToString(dr["FR_FrequencyName"]);
                view.SendDay = Convert.ToInt32(dr["DS_SendDay"]);
                view.SendMonth = Convert.ToInt32(dr["DS_SendMonth"]);

                view.ReportName = Convert.ToString(dr["ReportName"]);
                if (!dr.IsNull("DS_StartTime"))
                    view.StartTime = Convert.ToInt32(dr["DS_StartTime"]);
                else
                    view.StopTime = -1;

                if (!dr.IsNull("DS_StopTime"))
                    view.StopTime = Convert.ToInt32(dr["DS_StopTime"]);

                view.ProcessDate = Convert.ToInt32(dr["DS_ProcessDate"]);
                view.Title = Convert.ToString(dr["DS_Title"]);

                view.QLenAlert = Convert.ToString(dr["QLenAlert"]);
                view.FOpenAlert = Convert.ToString(dr["FOpenAlert"]);
                view.FLockWaitAlert = Convert.ToString(dr["FLockWaitAlert"]);
                view.MinProcBusy = Convert.ToString(dr["MinProcBusy"]);
                view.ExSourceCPU = Convert.ToString(dr["ExSourceCPU"]);
                view.ExDestCPU = Convert.ToString(dr["ExDestCPU"]);
                view.ExProgName = Convert.ToString(dr["ExProgName"]);

                if (!dr.IsNull("DS_IsWeekdays")) {
                    view.IsWeekdays = Convert.ToBoolean(dr["DS_IsWeekdays"]);
                    view.FrequencyWeekday = Convert.ToInt32(dr["DS_FrequencyWeekday"]);
                    view.FrequencyMonthCount = Convert.ToInt32(dr["DS_FrequencyMonthCount"]);
                }

                if (!dr.IsNull("DS_IsReportDataLast")) {
                    view.IsReportDataLast = Convert.ToBoolean(dr["DS_IsReportDataLast"]);
                    view.ReportDataWeekday = Convert.ToInt32(dr["DS_ReportDataWeekday"]);
                }


                allSchedules.Add(view);
            }

            return allSchedules;
        }

        public IList<DeliveryScheduleView> GetDPASchduleFor()
        {
            var deliverySchedules = new DeliverySchedules(_connectionString);
            DataTable schedules = deliverySchedules.GetDPASchdule();
            IList<DeliveryScheduleView> allSchedules = new List<DeliveryScheduleView>();

            foreach (DataRow dr in schedules.Rows)
            {
                var view = new DeliveryScheduleView();
                view.DeliveryID = Convert.ToInt32(dr["DS_DeliveryID"]);
                view.TrendReportID = Convert.ToInt32(dr["DS_TrendReportID"]);
                view.SystemSerial = Convert.ToString(dr["DS_SystemSerial"]);
                view.FrequencyName = Convert.ToString(dr["FR_FrequencyName"]);
                view.SendDay = Convert.ToInt32(dr["DS_SendDay"]);
                view.SendMonth = Convert.ToInt32(dr["DS_SendMonth"]);

                view.GroupName = Convert.ToString(dr["GroupName"]);

                if (!dr.IsNull("DS_StartTime"))
                    view.StartTime = Convert.ToInt32(dr["DS_StartTime"]);
                else
                    view.StopTime = -1;

                if (!dr.IsNull("DS_StopTime"))
                    view.StopTime = Convert.ToInt32(dr["DS_StopTime"]);

                view.ProcessDate = Convert.ToInt32(dr["DS_ProcessDate"]);
                view.Title = Convert.ToString(dr["DS_Title"]);

                if (!dr.IsNull("DS_IsWeekdays")) {
                    view.IsWeekdays = Convert.ToBoolean(dr["DS_IsWeekdays"]);
                    view.FrequencyWeekday = Convert.ToInt32(dr["DS_FrequencyWeekday"]);
                    view.FrequencyMonthCount = Convert.ToInt32(dr["DS_FrequencyMonthCount"]);
                }

                if (!dr.IsNull("DS_IsReportDataLast")) {
                    view.IsReportDataLast = Convert.ToBoolean(dr["DS_IsReportDataLast"]);
                    view.ReportDataWeekday = Convert.ToInt32(dr["DS_ReportDataWeekday"]);
                }

                allSchedules.Add(view);
            }

            return allSchedules;
        }

        public IList<DeliveryScheduleView> GetTPSSchduleFor() {
            var deliverySchedules = new DeliverySchedules(_connectionString);
            DataTable schedules = deliverySchedules.GetTPSSchdule();
            IList<DeliveryScheduleView> allSchedules = new List<DeliveryScheduleView>();

            foreach (DataRow dr in schedules.Rows) {
                var view = new DeliveryScheduleView();
                view.DeliveryID = Convert.ToInt32(dr["DS_DeliveryID"]);
                view.TrendReportID = Convert.ToInt32(dr["DS_TrendReportID"]);
                view.SystemSerial = Convert.ToString(dr["DS_SystemSerial"]);
                view.FrequencyName = Convert.ToString(dr["FR_FrequencyName"]);
                view.SendDay = Convert.ToInt32(dr["DS_SendDay"]);
                view.SendMonth = Convert.ToInt32(dr["DS_SendMonth"]);

                view.ReportName = Convert.ToString(dr["TransactionProfileName"]);
                view.TrendReportID = Convert.ToInt32(dr["TransactionProfileId"]);

                if (!dr.IsNull("DS_StartTime"))
                    view.StartTime = Convert.ToInt32(dr["DS_StartTime"]);
                else
                    view.StopTime = -1;

                if (!dr.IsNull("DS_StopTime"))
                    view.StopTime = Convert.ToInt32(dr["DS_StopTime"]);

                view.ProcessDate = Convert.ToInt32(dr["DS_ProcessDate"]);
                view.Title = Convert.ToString(dr["DS_Title"]);

                if (!dr.IsNull("DS_IsWeekdays")) {
                    view.IsWeekdays = Convert.ToBoolean(dr["DS_IsWeekdays"]);
                    view.FrequencyWeekday = Convert.ToInt32(dr["DS_FrequencyWeekday"]);
                    view.FrequencyMonthCount = Convert.ToInt32(dr["DS_FrequencyMonthCount"]);
                }

                if (!dr.IsNull("DS_IsReportDataLast")) {
                    view.IsReportDataLast = Convert.ToBoolean(dr["DS_IsReportDataLast"]);
                    view.ReportDataWeekday = Convert.ToInt32(dr["DS_ReportDataWeekday"]);
                }

                allSchedules.Add(view);
            }

            return allSchedules;
        }
        public IList<DeliveryScheduleView> GetSchduleDataFor(int deliveryID)
        {
            var deliverySchedules = new DeliverySchedules(_connectionString);
            DataTable schedules = deliverySchedules.GetSchduleData(deliveryID);
            IList<DeliveryScheduleView> allSchedules = new List<DeliveryScheduleView>();

            foreach (DataRow dr in schedules.Rows)
            {
                var view = new DeliveryScheduleView();
                view.TrendReportID = Convert.ToInt32(dr["DS_TrendReportID"]);
                view.SystemSerial = Convert.ToString(dr["DS_SystemSerial"]);
                view.ReportType = Convert.ToString(dr["RT_ReportType"]);
                view.PeriodType = Convert.ToChar(dr["DS_PeriodType"]);
                view.PeriodCount = Convert.ToInt32(dr["DS_PeriodCount"]);
                view.TrendReportID = Convert.ToInt32(dr["DS_TrendReportID"]);
                view.Title = Convert.ToString(dr["DS_Title"]);
                allSchedules.Add(view);
            }

            return allSchedules;
        }
    }
}