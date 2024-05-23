using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class NotificationPreferenceService {
        private readonly string _connectionString = "";

        public NotificationPreferenceService(string connectionString) {
            _connectionString = connectionString;
        }

        public NotificationPreferenceView CheckIsEveryLoadFor(string systemSerial, int customerID) {
            var view = new NotificationPreferenceView();
            var notificationPreferences = new NotificationPreferences(_connectionString);
            var notification = notificationPreferences.CheckIsEveryLoad(systemSerial, customerID);

            foreach (DataRow dr in notification.Rows.Cast<DataRow>().Where(dr => !dr.IsNull("IsEveryLoad"))) {
                view.IsEveryLoad = Convert.ToBoolean(dr["IsEveryLoad"]);
                view.IsEmailCritical = Convert.ToBoolean(dr["IsEmailCritical"]);
                view.IsEmailMajor = Convert.ToBoolean(dr["IsEmailMajor"]);
            }

            return view;
        }

        public bool CheckIsDailyFor(string systemSerial, int customerID) {
            var view = new NotificationPreferenceView();
            var notificationPreferences = new NotificationPreferences(_connectionString);
            bool exist = notificationPreferences.CheckIsDailyLoad(systemSerial, customerID);

            return exist;
        }

        public List<EveryHourNotificationView> GetEveryHourSystemsFor() {
            var notificationPreferences = new NotificationPreferences(_connectionString);
            var notification = notificationPreferences.GetEveryHourSystems();

            return (from DataRow dr in notification.Rows
                where !dr.IsNull("NotificationID")
                select new EveryHourNotificationView {
                    NotificationID = Convert.ToInt32(dr["NotificationID"]), 
                    SystemSerial = Convert.ToString(dr["SystemSerial"]),
                    CustomerID = Convert.ToInt32(dr["CustomerID"]),
                    EveryHour = Convert.ToInt32(dr["EveryHour"]),
                    StartHour = Convert.ToInt32(dr["StartHour"]),
                    Email = Convert.ToString(dr["Email"])
                }).ToList();
        }

        public List<EveryDayNotificationView> GetEveryDailySystemsFor() {
            var notificationPreferences = new NotificationPreferences(_connectionString);
            var notification = notificationPreferences.GetEveryDailySystems();

            return (from DataRow dr in notification.Rows
                    where !dr.IsNull("NotificationID")
                    select new EveryDayNotificationView {
                        NotificationID = Convert.ToInt32(dr["NotificationID"]),
                        SystemSerial = Convert.ToString(dr["SystemSerial"]),
                        CustomerID = Convert.ToInt32(dr["CustomerID"]),
                        SendHour = Convert.ToInt32(dr["SendHour"]),
                        IsPreviousDay = Convert.ToBoolean(dr["IsPreviousDay"]),
                        IsLastHour = Convert.ToBoolean(dr["IsLastHour"]),
                        LastHour = Convert.ToInt32(dr["LastHour"]),
                        Email = Convert.ToString(dr["Email"])
                    }).ToList();
        }

        public List<EveryWeekNotificationView> GetEveryWeeklySystemsFor() {
            var notificationPreferences = new NotificationPreferences(_connectionString);
            var notification = notificationPreferences.GetEveryWeeklySystems();

            return (from DataRow dr in notification.Rows
                    where !dr.IsNull("NotificationID")
                    select new EveryWeekNotificationView {
                        NotificationID = Convert.ToInt32(dr["NotificationID"]),
                        SystemSerial = Convert.ToString(dr["SystemSerial"]),
                        CustomerID = Convert.ToInt32(dr["CustomerID"]),
                        SendWeek = Convert.ToInt32(dr["SendWeek"]),
                        WeekSendHour = Convert.ToInt32(dr["WeekSendHour"]),
                        LastWeek = Convert.ToInt32(dr["LastWeek"]),
                        Email = Convert.ToString(dr["Email"])
                    }).ToList();
        }
    }
}