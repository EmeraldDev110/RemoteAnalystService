namespace RemoteAnalyst.BusinessLogic.ModelView
{
    public class TrendReportView
    {
        public string GraphName { get; set; }
        public int TMetricsID { get; set; }
        public int CustomerID { get; set; }
        public string SpanName { get; set; }
        public string PeriodName { get; set; }
        public string ReportName { get; set; }
        public int EntityID { get; set; }
        public int PercentFlag { get; set; }
        public int AverageFlag { get; set; }
        public string YAxisTitle { get; set; }
        public int IncOthersFlag { get; set; }
        public int AllOnlyFlag { get; set; }
        public int TimeEntityFlag { get; set; }
        public int StackFlag { get; set; }
        public bool SumFlag { get; set; }
        public int StorageID { get; set; }
    }
}