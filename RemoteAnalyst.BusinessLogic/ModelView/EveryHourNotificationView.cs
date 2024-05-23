using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.BusinessLogic.ModelView {
    public class EveryHourNotificationView {
        public int NotificationID { get; set; }
        public string SystemSerial { get; set; }
        public int CustomerID { get; set; }
        public int EveryHour { get; set; }
        public int StartHour { get; set; }
        public string Email { get; set; }
    }
}
