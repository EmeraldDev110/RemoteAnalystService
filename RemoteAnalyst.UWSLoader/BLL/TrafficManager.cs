using log4net;
using RemoteAnalyst.AWS.CloudWatch;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RemoteAnalyst.UWSLoader.BLL
{
    internal class TrafficManager
    {
        private string _instanceId;
        private readonly ILog _log;

        internal TrafficManager(ILog log, string instanceId)
        {
            _instanceId = instanceId;
            _log = log;
        }

        internal bool IsLoaderOKToLoad()
        {
            return IsLoaderActive();
        }

        internal bool IsLoaderActive()
        {
            var monitorEC2 = new MonitorService(ConnectionString.ConnectionStringDB, _log);
            return monitorEC2.IsLoaderActive(_instanceId);
        }

        public bool CheckLoaderCreditBalance()
        {
            var raInfo = new RAInfoService(ConnectionString.ConnectionStringDB);
            var creditBalanceWindow = raInfo.GetValueFor("CreditBalanceWindow");
            var creditBalanceWindowValue = 0.6d;
            if (creditBalanceWindow.Length != 0)
                creditBalanceWindowValue = double.Parse(creditBalanceWindow) / 100;

            _log.DebugFormat("creditBalanceWindow: {0}, creditBalanceWindowValue: {1}", creditBalanceWindow, creditBalanceWindowValue);
            var myCreditBalance = 0d;
            var avgCreditBalance = 0d;
            var monitorEC2 = new MonitorService(ConnectionString.ConnectionStringDB, _log);

            var cloudWatch = new AmazonCloudWatch();

            List<EC2View> loaderEC2s = monitorEC2.GetEc2();
            _log.DebugFormat("loaderEC2s: {0}", loaderEC2s.Count);
            if (loaderEC2s.Count == 0)
            {
                return true; // Default OK since other EC2s may not have been configured.
            }
            else { 
                foreach (EC2View loaderEC2 in loaderEC2s)
                {
                    var instanceCreditBalance = cloudWatch.GetEC2CPUCreditBalance(loaderEC2.InstanceId);
                    if (loaderEC2.InstanceId == _instanceId)
                    {
                        myCreditBalance = instanceCreditBalance;
                    }
                    avgCreditBalance += instanceCreditBalance;
                }
                avgCreditBalance /= Convert.ToDouble(loaderEC2s.Count);

                return ((myCreditBalance / avgCreditBalance) >= creditBalanceWindowValue);
            }
        }

        internal bool CheckLoaderCpuBusy()
        {
            bool isOverCpuLimit = false;
            var totalCpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            var totalCpuBusy = totalCpu.NextValue();
            System.Threading.Thread.Sleep(1000);
            totalCpuBusy = totalCpu.NextValue();

            var raInfo = new RAInfoService(ConnectionString.ConnectionStringDB);
            var loaderBusyLimit = raInfo.GetValueFor("LoaderBusyLimit");

            if (loaderBusyLimit.Length == 0)
                loaderBusyLimit = "60";

            if (totalCpuBusy > double.Parse(loaderBusyLimit))
                isOverCpuLimit = true;

            return isOverCpuLimit;
        }

        internal bool CheckRdsCpuBusy(string rdsName)
        {
            var isOverRdsCpuLimit = false;

            MonitorService monitorService = new MonitorService(ConnectionString.ConnectionStringDB, _log);
            var rdsCpuBusy = monitorService.GetRDSCpuBusy(rdsName);

            var raInfo = new RAInfoService(ConnectionString.ConnectionStringDB);
            var rdsBusyLimit = raInfo.GetValueFor("RDSBusyLimit");

            if (rdsBusyLimit.Length == 0)
                rdsBusyLimit = "70";


            if (rdsCpuBusy > double.Parse(rdsBusyLimit))
                isOverRdsCpuLimit = true;
            return isOverRdsCpuLimit;
        }

        internal bool CheckSystemLoadOverLimit(string systemSerial)
        {
            var isSystemLoadOverLimit = false;

            var raInfo = new RAInfoService(ConnectionString.ConnectionStringDB);
            var systemLoadLimit = Convert.ToInt32(raInfo.GetValueFor("SystemLoadLimit"));

            var systemTblService = new System_tblService(ConnectionString.ConnectionStringDB);
            var perSystemLoadLimit = systemTblService.GetPerSystemLoadLimit(systemSerial);
            var loadLimit = Math.Min(systemLoadLimit, perSystemLoadLimit);

            var loadingStatusDetail = new LoadingStatusDetailService(ConnectionString.ConnectionStringDB);
            var currentLoadCount = loadingStatusDetail.GetCurrentLoadCountFor(systemSerial, _instanceId);

            if (currentLoadCount >= loadLimit)
                isSystemLoadOverLimit = true;

            return isSystemLoadOverLimit;
        }

        internal bool isTimeToCheckRdsMoveQ(DateTime serverNow, int timeZoneIndex)
        {

            string timeZoneName = TimeZoneIndexConverter.ConvertIndexToName(timeZoneIndex);

            //Check timezone
            TimeZoneInfo est = TimeZoneInfo.FindSystemTimeZoneById(timeZoneName);
            DateTime targetTime = TimeZoneInfo.ConvertTime(serverNow, est);
            //Do not do RDSMove at system local 12 AM, due to precessing previous date data.
            if (targetTime.Hour == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
