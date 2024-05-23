using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.BusinessLogic.ModelView {
    public class NotificationPreferenceView {
        public bool IsEveryLoad { get; set; }
        public bool IsEveryHour { get; set; }
        public bool IsDaily { get; set; }
        public bool IsEmailCritical { get; set; }
        public bool IsEmailMajor { get; set; }

    }
}
