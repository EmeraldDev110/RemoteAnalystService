namespace RemoteAnalyst.BusinessLogic.ModelView {
    public class TransactionProfileInfo {
        public int TransactionProfileID { get; set; }
        public string TransactionFile { get; set; }
        public short OpenerType { get; set; }
        public string OpenerName { get; set; }
        public short TransactionCounter { get; set; }
        public double IOTransactionRatio { get; set; }
        public bool IsCpuToFile { get; set; }
    }
}
