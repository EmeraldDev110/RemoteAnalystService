using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using DataBrowser.Entities;
using DataBrowser.Model;
using MySQLDataBrowser.Model;
using RemoteAnalyst.AWS.Queue;
using RemoteAnalyst.BusinessLogic.BLL;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.Enums;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.BusinessLogic.SCM;
using RemoteAnalyst.BusinessLogic.UWSLoader.BaseClass;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;
using RemoteAnalyst.UWSLoader.BLL;
using RemoteAnalyst.UWSLoader.BLL.Process;
using RemoteAnalyst.UWSLoader.BLL.SQS;
using RemoteAnalyst.UWSLoader.DiskBrowserLookBack;
using RemoteAnalyst.UWSLoader.Email;
using RemoteAnalyst.UWSLoader.JobProcessor;
using RemoteAnalyst.UWSLoader.SPAM;
using RemoteAnalyst.UWSLoader.SPAM.BLL;
using CurrentTableService = RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices.CurrentTableService;
using RemoteAnalystTrendLoader.Model;
using RemoteAnalyst.AWS.CloudWatch;
using RemoteAnalyst.UWSLoader.TableUpdater;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.UWSLoader {
    internal static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main() {
#if (DEBUG == false)
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] {
                new UWSLoaderService()
            };
            ServiceBase.Run(ServicesToRun);
#endif
#if DEBUG

            ReadXML.ImportDataFromXML();

            //TrafficManager tm = new TrafficManager("i-0db5aadafaf13f3d0");
            //bool isOK = tm.CheckLoaderCreditBalance();
            //System.Console.WriteLine("Instance is " + isOK);
            //var checkUWS = new CheckQue();
            //checkUWS.CheckUWS(null, null);
            var jobUWS = new JobProcessorUWS("UMMV02_212548209051383606_080627_0414_0400_0414_0500_12E_26512632_U5973289.402",
                                    1,
                                    "080627",
                                    "SYSTEM");
            jobUWS.ProcessJob();
            /*
            var systemPath = @"C:\RemoteAnalyst\Systems\";
            var systemSerial = "080984";
            var databaseMappingService = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
            var connectionStringSystem = databaseMappingService.GetConnectionStringFor(systemSerial);
            var chart = new JobProcessorChart(ConnectionString.ConnectionStringDB,
                ConnectionString.ConnectionStringTrend, connectionStringSystem, systemPath);
            var exceptionList = new List<ExceptionView>();

            var starttime = Convert.ToDateTime("8/19/2022 12:00:00 PM");
            var stoptime = Convert.ToDateTime("8/19/2022 12:59:59 PM");
            var interval = 900;
            var forecast = new ForecastService(systemSerial, ConnectionString.ConnectionStringDB, connectionStringSystem);
            var forecastCpu = forecast.GetForecastCpuDataFor(starttime, stoptime);
            var forecastIpu = 0;
            var forecastDisk = 0;
            var forecastStorage = 0;

            StreamWriter newFileLog = null;
            newFileLog = Helper.GetLog("ExceptionTest", systemPath);

            newFileLog.WriteLine("*******************************************************");
            newFileLog.WriteLine("SystemSerial: " + systemSerial);

            var systemWeek = new SystemWeekService(systemSerial, ConnectionString.ConnectionStringDB);
            var systemWeekInfo = systemWeek.GetSystemWeek();

            var dailyEmailUtil = new DailyEmailUtil();

            var cpuBusyGridStructure = dailyEmailUtil.GenerateGridDataTable(starttime, stoptime);
            var cpuBusyGrid = cpuBusyGridStructure.Clone();

            chart.GetCPUBusyAlertColor(systemSerial, starttime, 
                stoptime, interval, 
                systemWeekInfo, 
                0, 0, 0,
                forecastCpu, true, cpuBusyGrid, newFileLog, ref exceptionList);
            */
#endif
        }
    }
}