using System;

namespace RemoteAnalyst.BusinessLogic.ModelView {
    public class LoadingStatusDetailView {
        public LoadingStatusDetailView() {
            SystemSerial = "";
            DataFileName = "";
            LoadType = "";
            UWSID = -1;
            SelectedStartTime = new DateTime();
            SelectedStopTime = new DateTime();
        }

        public string SystemSerial { get; set; }
        public string DataFileName { get; set; }
        public string LoadType { get; set; }
        public int UWSID { get; set; }
        public DateTime SelectedStartTime { get; set; }
        public DateTime SelectedStopTime { get; set; }
        public string InstanceID { get; set; }
    }
}