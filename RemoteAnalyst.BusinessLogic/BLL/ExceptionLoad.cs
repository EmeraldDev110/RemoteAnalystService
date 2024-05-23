using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using log4net;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.BusinessLogic.BLL {

    public class ExceptionLoad
    {
        private static readonly ILog Log = LogManager.GetLogger("ExceptionLoad");
        private readonly string _connectionStringdb;
        private readonly string _connectionStringTrend;
        private readonly string _serverPath;

        public ExceptionLoad(string connectionStringdb, string connectionStringTrend, string serverPath) {
            _connectionStringdb = connectionStringdb;
            _connectionStringTrend = connectionStringTrend;
            _serverPath = serverPath;
        }
        public void InsertException(string systemSerial, DateTime starttime, DateTime stoptime) {
            Log.InfoFormat("SystemSerial: {0}, startTime {1}, stopTime {2}",
                            systemSerial, starttime, stoptime);

            const bool alertException = true;

            var databaseMappingService = new DatabaseMappingService(_connectionStringdb);
            var connectionStringSystem = databaseMappingService.GetConnectionStringFor(systemSerial);

            var cpuTableName = new List<string>();
            var currentTableService = new CurrentTableService(connectionStringSystem);
            cpuTableName.Add(systemSerial + "_CPU_" + starttime.Year + "_" + starttime.Month + "_" + starttime.Day);
            var interval = currentTableService.GetIntervalFor(cpuTableName);

            #region Forecast Data
            //Get Tolernace value;
            var systemTableService = new System_tblService(_connectionStringdb);
            var toleranceInfo = systemTableService.GetToleranceFor(systemSerial);

            var businessTolerance = toleranceInfo.Rows[0].IsNull("BusinessTolerance") ? 5.0 : Convert.ToDouble(toleranceInfo.Rows[0]["BusinessTolerance"]);
            var batchTolerance = toleranceInfo.Rows[0].IsNull("BatchTolerance") ? 10.0 : Convert.ToDouble(toleranceInfo.Rows[0]["BatchTolerance"]);
            var otherTolerance = toleranceInfo.Rows[0].IsNull("OtherTolerance") ? 12.0 : Convert.ToDouble(toleranceInfo.Rows[0]["OtherTolerance"]);

            var forecast = new ForecastService(systemSerial, _connectionStringdb, connectionStringSystem);
            var forecastCpu = forecast.GetForecastCpuDataFor(starttime, stoptime);
            var forecastIpu = forecast.GetForecastIpuDataFor(starttime, stoptime);
            var forecastDisk = forecast.GetForecastDiskDataFor(starttime, stoptime);
            var forecastStorage = forecast.GetForecastStorageDataFor(starttime.AddDays(-1).Date, stoptime.AddDays(-1).Date);
            //var forecastProcess = forecast.GetForecastProcessDataFor(starttime, stoptime);
            //var forecastTmf = forecast.GetForecastTmfDataFor(starttime, stoptime);

            Log.InfoFormat("forecastCpu: {0}, forecastIpu: {1}, forecastDisk: {2}, forecastStorage {3}",
                forecastCpu.Count, forecastIpu.Count, forecastDisk.Count, forecastStorage.Count);

            var systemWeek = new SystemWeekService(systemSerial, _connectionStringdb);
            var systemWeekInfo = systemWeek.GetSystemWeek();
            #endregion

            var chart = new JobProcessorChart(_connectionStringdb, _connectionStringTrend, connectionStringSystem, _serverPath);

            #region Grid
            var dailyEmailUtil = new DailyEmailUtil();

            var cpuBusyGridStructure = dailyEmailUtil.GenerateGridDataTable(starttime, stoptime);
            var cpuBusyGrid = cpuBusyGridStructure.Clone();
            var cpuQueueGrid = cpuBusyGridStructure.Clone();
            var ipuBusyGrid = cpuBusyGridStructure.Clone();
            var ipuQueueGrid = cpuBusyGridStructure.Clone();
            var diskGrid = cpuBusyGridStructure.Clone();
            var storageGrid = cpuBusyGridStructure.Clone();
            #endregion

            Log.Info("Calling GetCPUBusyAlertColor");
            var exceptionList = new List<ExceptionView>();
            chart.GetCPUBusyAlertColor(systemSerial, starttime, stoptime, interval, systemWeekInfo, 
                businessTolerance, batchTolerance, otherTolerance, forecastCpu, alertException, cpuBusyGrid,
                Log, ref exceptionList);

            Log.Info("Calling GetCPUQueueAlertColor");
            chart.GetCPUQueueAlertColor(systemSerial, starttime, stoptime, interval, systemWeekInfo, 
                businessTolerance, batchTolerance, otherTolerance, forecastCpu, alertException, cpuQueueGrid,
                Log, ref exceptionList);

            //Done with CPU, insert Exception data.
            chart.ExpectionUniqueBulkInsert(exceptionList, _serverPath);
            //Clear the exceptionList.
            exceptionList = new List<ExceptionView>();

            Log.Info("Calling GetIPUBusyAlertColor");
            chart.GetIPUBusyAlertColor(systemSerial, starttime, stoptime, interval, systemWeekInfo, 
                businessTolerance, batchTolerance, otherTolerance, forecastIpu, alertException, ipuBusyGrid,
                Log, ref exceptionList);

            Log.Info("Calling GetIpuQueueAlertColor");
            chart.GetIpuQueueAlertColor(systemSerial, starttime, stoptime, interval, systemWeekInfo, 
                businessTolerance, batchTolerance, otherTolerance, forecastIpu, alertException, ipuQueueGrid,
                Log, ref exceptionList);

            //Done with IPU, insert Exception data.
            chart.ExpectionUniqueBulkInsert(exceptionList, _serverPath + "/UWSLoader/");

            try
            {
                Log.Info("Calling GetDiskQueueAlertColor");
                chart.GetDiskQueueAlertColor(systemSerial, starttime, stoptime, interval, systemWeekInfo, 
                    businessTolerance, batchTolerance, otherTolerance, forecastDisk, alertException, 
                    diskGrid, Log, _serverPath + "/UWSLoader/");
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("GetDiskQueueAlertColor Error: {0}", ex);
            }

            try
            {
                Log.Info("Calling GetDiskDP2AlertColor");
                chart.GetDiskDP2AlertColor(systemSerial, starttime, stoptime, interval, systemWeekInfo, 
                    businessTolerance, batchTolerance, otherTolerance, forecastDisk, alertException, diskGrid,
                    Log, _serverPath + "/UWSLoader/");
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("GetDiskDP2AlertColor Error: {0}", ex);
            }

            try
            {
                Log.Info("Calling GetStorageAlertColor");
                //Since Storage gets loaded once a day, we need to load previous date's data.
                chart.GetStorageAlertColor(systemSerial, starttime.AddDays(-1).Date, stoptime.AddDays(-1).Date, 
                    interval, systemWeekInfo, businessTolerance, batchTolerance, otherTolerance, 
                    forecastStorage, alertException, storageGrid, Log, _serverPath + "/UWSLoader/");
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("GetStorageAlertColor Error: {0}", ex);
            }
        }
    }
}
