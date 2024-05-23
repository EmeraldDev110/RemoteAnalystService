
namespace RemoteAnalyst.BusinessLogic.ModelView {
    public class ScheduleView {
        public string SystemSerial { get; set; }

        public string SystemName { get; set; }
        public int ScheduleId { get; set; }
        public string Type { get; set; }
        public string Frequency { get; set; }
        public int DailyOn { get; set; }
        public int DailyAt { get; set; }
        //public bool DailyPreviouse { get; set; }
        public int WeeklyOn { get; set; }
        public int WeeklyFor { get; set; }
        public int WeeklyFrom { get; set; }
        public int WeeklyTo { get; set; }
        public int MonthlyOn { get; set; }
        public int MonthlyOnWeekDay { get; set; }
        public int MonthlyFor { get; set; }
        public int MonthlyFrom { get; set; }
        public int MonthlyTo { get; set; }
        public bool IsMonthlyOn { get; set; }
        public bool IsMonthlyFor { get; set; }
        public string Email { get; set; }
        public string DetailTypeId { get; set; }
        public bool AlertException { get; set; }
        public string ReportFromHour { get; set; }
        public string ReportToHour { get; set; }
        public bool HourBoundaryTrigger { get; set; }
        public bool Overlap { get; set; }
        public string BatchProgram { get; set; }

        public string BatchId { get; set; }

        public string ReportDownloadId { get; set; }
    }
}
