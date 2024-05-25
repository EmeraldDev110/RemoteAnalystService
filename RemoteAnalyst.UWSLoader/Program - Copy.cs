using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using DataBrowser.Entities;
using DataBrowser.Model;
using MySQLDataBrowser.Model;
using RemoteAnalyst.AWS.AutoScaling;
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

            var read = new ReadXML();
            read.ImportDataFromXML();

            //var checkUWS = new CheckQue();
            //checkUWS.CheckUWS(null, null);
            var jobUWS = new JobProcessorUWS("UMMV02_212515363116503202_078566_0329_1600_0329_2000_01E_524288_U5505010.402",
                6,
                "078566",
                "SYSTEM");
            jobUWS.ProcessJob();
#endif
        }
    }
}