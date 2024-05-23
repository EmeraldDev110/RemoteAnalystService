using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.BusinessLogic.ModelView {
    public class EveryDayNotificationView {
        public int NotificationID { get; set; }
        public string SystemSerial { get; set; }
        public int CustomerID { get; set; }
        public int SendHour { get; set; }
        public bool IsPreviousDay { get; set; }
        public bool IsLastHour { get; set; }
        public int LastHour { get; set; }
        public string Email { get; set; }
    }
}
