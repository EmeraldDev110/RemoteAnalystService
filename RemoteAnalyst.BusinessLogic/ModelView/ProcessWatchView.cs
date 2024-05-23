using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace RemoteAnalyst.BusinessLogic.ModelView {
    public class ProcessWatchView {
        public string SystemSerial { get; set; }
        public string FromTime { get; set; }
        public string ToTime { get; set; }
        public string TableName { get; set; }
        public string Interval { get; set; }

        public string GetProcessWatchView(string systemSerial, DateTime fromTime, DateTime toTime, string tableName, long interval) {
            var view = new ProcessWatchView {
                SystemSerial = systemSerial,
                FromTime = fromTime.ToString(),
                ToTime = toTime.ToString(),
                TableName = tableName,
                Interval = interval.ToString()
            };
            return new JavaScriptSerializer().Serialize(view);
        }
    }
}
