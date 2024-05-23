using System;
using System.Collections.Generic;
using System.Data;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class TMonScheduleService {
        private readonly TMonSchedule tMonSchedule;

        public TMonScheduleService(TMonSchedule tMonSchedule) {
            this.tMonSchedule = tMonSchedule;
        }

        public List<TMonScheduleView> GetTMonSchedulesFor() {
            List<TMonScheduleView> tMonScheduleViews = new List<TMonScheduleView>();
            DataTable schedules = tMonSchedule.GetTMonSchedules();

            foreach (DataRow dr in schedules.Rows) {
                var tMonScheduleView = new TMonScheduleView {
                    SystemSerial = Convert.ToString(dr["SystemSerial"]),
                    TransSchedule = Convert.ToString(dr["TransSchedule"]),
                    WeekDays = Convert.ToString(dr["WeekDays"]),
                    FirstTransmissionTime = Convert.ToString(dr["FirstTransmissionTime"]),
                    Interval = Convert.ToInt32(dr["Interval"]),
                    ActiveFlag = Convert.ToChar(dr["ActiveFlag"])
                };
                tMonScheduleViews.Add(tMonScheduleView);
            }
            return tMonScheduleViews;
        }

        public int GetDelayFor(string systemSerial) {
            return tMonSchedule.GetDelay(systemSerial);
        }

        public int GetLoadTimeFor(string systemSerial) {
            return tMonSchedule.GetLoadTime(systemSerial);
        }
    }
}