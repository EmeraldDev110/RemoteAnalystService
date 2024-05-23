namespace RemoteAnalyst.BusinessLogic.ModelView
{
    public class DeliveryUponLoadView
    {
        public int DU_DeliveryID { get; set; }
        public int DU_ReportID { get; set; }
        public string DU_SystemSerial { get; set; }
        public char DU_PeriodType { get; set; }
        public int DU_PeriodCount { get; set; }
        public string DU_Title { get; set; }

        public int SR_StorageID { get; set; }
        public int SR_GroupDisk { get; set; }
        public int SR_GroupDiskID { get; set; }

        public string RT_ReportType { get; set; }

        public int SR_CustomerID { get; set; }
    }
}