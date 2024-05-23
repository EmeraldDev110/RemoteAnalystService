using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.BusinessLogic.ModelView {
    public class BatchView {

        public int TimeZone { get; set; }
        public int BatchSequenceProfileId { get; set; }
        public int Order { get; set; }
        public string SystemSerial { get; set; }
        public string SystemName { get; set; }
        public string ConnectionString { get; set; }
        public string BatchName { get; set; }
        public string EmailList { get; set; }
        public string ProgramFiles { get; set; }
        public DateTime StartWindowStart { get; set; }
        public DateTime StartWindowEnd { get; set; }
        public DateTime ExpectedFinishBy { get; set; }
        public char [] StartWindowDoW { get; set; }
        public bool AlertIfDoesNotStartOnTime { get; set; }
        public bool AlertIfDoesNotFinishOnTime { get; set; }
        public bool AlertIfOrderNotFollowed { get; set; }

    }
}
