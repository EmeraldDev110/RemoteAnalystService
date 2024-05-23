using System;

namespace RemoteAnalyst.BusinessLogic.Infrastructure {
    public class ForecastData {
        public string Hour { get; set; }
        public int CpuNumber { get; set; }
        public int IpuNumber { get; set; }
        public double CpuBusy { get; set; }
        public double MemoryUsed { get; set; }
        public double Queue { get; set; }
        public double IpuBusy { get; set; }
        public double IpuQueue { get; set; }
        public double StdDevCpuBusy { get; set; }
        public double StdDevMemoryUsed { get; set; }
        public double StdDevQueue { get; set; }
        public double StdDevIpuBusy { get; set; }
        public double StdDevIpuQueue { get; set; }
        public DateTime ForecastDateTime { get; set; }
    }

    public class ForecastDiskData {
        public string Hour { get; set; }
        public string DeviceName { get; set; }
        public double QueueLength { get; set; }
        public double StdDevQueueLength { get; set; }
        public double DP2Busy { get; set; }
        public double StdDevDP2Busy { get; set; }
        public DateTime ForecastDateTime { get; set; }
    }

    public class ForecastStorageData {
        public string DeviceName { get; set; }
        public double UsedPercent { get; set; }
        public double StdDevUsedPercent { get; set; }
        public DateTime ForecastDateTime { get; set; }
    }

    public class ForecastProcessData {
        public string Hour { get; set; }
        public string ProcessName { get; set; }
        public int CpuNumber { get; set; }
        public int Pin { get; set; }
        public string Volume { get; set; }
        public string SubVol { get; set; }
        public string FileName { get; set; }

        public double ProcessBusy { get; set; }
        public double StdDevProcessBusy { get; set; }
        public double RecvQueueLength { get; set; }
        public double StdDevRecvQueueLength { get; set; }

        public DateTime ForecastDateTime { get; set; }
    }

    public class ForecastTMFData {
        public string Hour { get; set; }
        public string ProcessName { get; set; }
        public int CpuNumber { get; set; }
        public int Pin { get; set; }
        public string Volume { get; set; }
        public string SubVol { get; set; }
        public string FileName { get; set; }

        public double AbortPercent { get; set; }
        public double StdDevAbortPercent { get; set; }

        public DateTime ForecastDateTime { get; set; }
    }
}
