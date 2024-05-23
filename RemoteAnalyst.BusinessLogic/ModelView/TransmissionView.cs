namespace RemoteAnalyst.BusinessLogic.ModelView
{
    public class TransmissionView
    {
        public string CompanyName { get; set; }
        public string SystemName { get; set; }
        public string SystemSerial { get; set; }
        public string Description { get; set; }
        public char Frequency { get; set; }
        public int ProcessingHour { get; set; }
        public string ProcessingDate { get; set; }
        public string PlanEndDate { get; set; }
    }
}