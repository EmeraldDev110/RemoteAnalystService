using System;

namespace RemoteAnalyst.BusinessLogic.Email
{
    public class EmailList {
        public string SystemSerial { get; set; }
        public string SystemName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public int CustomerId { get; set; }
        public int ScheduleId { get; set; }
        public string Email { get; set; }
        public string DetailTypeId { get; set; }
        public string ReportFromHour { get; set; }
        public string ReportToHour { get; set; }
        public bool AlertException { get; set; }
        public bool IsLowPin { get; set; }
        public bool IsHighPin { get; set; }
        public bool IsAllSubVol { get; set; }
        public string SubVols { get; set; }
        public string BatchProgram { get; set; }

        public string BatchId { get; set; }
        public string ReportDownloadId { get; set; }
}
}