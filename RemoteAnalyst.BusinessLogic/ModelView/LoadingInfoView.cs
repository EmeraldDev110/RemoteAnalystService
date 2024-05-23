using System;

namespace RemoteAnalyst.BusinessLogic.ModelView
{
    public class LoadingInfoView
    {
        public LoadingInfoView()
        {
            SystemSerial = "";
            FileName = "";
            SystemName = "";
            SampleType = -1;
            StartTime = DateTime.MinValue;
            StopTime = DateTime.MinValue;
        }

        public string SystemSerial { get; set; }
        public string FileName { get; set; }
        public int UWSRetentionDay { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public int SampleType { get; set; }
        public string SystemName { get; set; }
    }
}