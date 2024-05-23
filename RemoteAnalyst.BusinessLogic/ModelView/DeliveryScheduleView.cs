using System;

namespace RemoteAnalyst.BusinessLogic.ModelView
{
    public class DeliveryScheduleView
    {
        public int DeliveryID { get; set; }
        public int TrendReportID { get; set; }
        public string SystemSerial { get; set; }
        public string FrequencyName { get; set; }
        public int SendDay { get; set; }
        public int SendMonth { get; set; }
        public string GroupName { get; set; }
        public int StartTime { get; set; }
        public int StopTime { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime StopDate { get; set; }
        public int ProcessDate { get; set; }
        public string Title { get; set; }
        public  bool IsSchedule { get; set; }

        public string ReportType { get; set; }
        public char PeriodType { get; set; }
        public int PeriodCount { get; set; }
        public string ReportName { get; set; }

        public string QLenAlert { get; set; }
        public string FOpenAlert { get; set; }
        public string FLockWaitAlert { get; set; }
        public string MinProcBusy { get; set; }
        public string ExSourceCPU { get; set; }
        public string ExDestCPU { get; set; }
        public string ExProgName { get; set; }


        public bool IsWeekdays { get; set; }
        public bool IsReportDataLast { get; set; }
        public int FrequencyWeekday { get; set; }
        public int FrequencyMonthCount { get; set; }
        public int ReportDataWeekday { get; set; }

        public int ProfileId { get; set; }
    }
}