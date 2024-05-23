namespace RemoteAnalyst.BusinessLogic.ModelView
{
    public class StorageAnalysisView
    {
        public string SystemSerial { get; set; }
		public string SystemName { get; set; }
		public string CompanyName { get; set; }
        public int ActiveSizeInMB { get; set; }
        public int TrendSizeInMB { get; set; }
        public int S3SizeInMB { get; set; }
		public string GeneratedDate { get; set; }
    }

    public class TempStorageTableView
    {
        public string SchemaName { get; set; }
        public string SystemSerial { get; set; }
        public int ActiveSizeInMB { get; set; }
        public int TrendSizeInMB { get; set; }
        public float S3SizeInMB { get; set; }
    }

	public class GraphStorageView {
		public string SystemName { get; set; }
		public int StorageUsageInMB { get; set; }
		public string GeneratedDate { get; set; }
	}
}
