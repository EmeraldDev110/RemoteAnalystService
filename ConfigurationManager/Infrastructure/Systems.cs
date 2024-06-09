using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager.Infrastructure {
    internal class Systems {
        public string SystemName { get; set; }
        public string SystemSerial { get; set; }
        public string TimeZone { get; set; }
        public string MeashFH { get; set; }
        public string Location { get; set; }
        public string NonStopIP { get; set; }
        public string MonitorPort { get; set; }
        public string StoreVolume { get; set; }
        public string FtpUserName { get; set; }
        public string FtpPassword { get; set; }
        public string FtpPort { get; set; }
        public string MeasFhVolume { get; set; }
        public string MeasFhSubVolume { get; set; }
    }
}
