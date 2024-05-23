namespace RemoteAnalyst.BusinessLogic.ModelView {
    public class TMonScheduleView {
        public string SystemSerial { get; set; }
        public string TransSchedule { get; set; }
        public string WeekDays { get; set; }
        public string FirstTransmissionTime { get; set; }
        public int TransmissionPossibleDelay { get; set; }
        public int Interval { get; set; }
        public int LoadTime { get; set; }
        public char ActiveFlag { get; set; }
    }
}