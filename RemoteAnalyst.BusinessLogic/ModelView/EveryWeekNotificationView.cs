using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.BusinessLogic.ModelView {
    public class EveryWeekNotificationView {
        public int NotificationID { get; set; }
        public string SystemSerial { get; set; }
        public int CustomerID { get; set; }
        public int SendWeek { get; set; }
        public int WeekSendHour { get; set; }
        public int LastWeek { get; set; }
        public string Email { get; set; }
    }
}
