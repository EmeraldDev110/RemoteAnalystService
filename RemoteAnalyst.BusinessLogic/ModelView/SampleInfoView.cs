using System;

namespace RemoteAnalyst.BusinessLogic.ModelView
{
    public class SampleInfoView
    {
        public SampleInfoView()
        {
            NSID = 0;
            SystemName = "";
            SystemSerial = "";
            StartDate = DateTime.MinValue;
            EndDate = DateTime.MaxValue;
        }

        public int NSID { get; set; }
        public string SystemName { get; set; }
        public string SystemSerial { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Entities { get; set; }
    }
}