using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.ServiceProcess;
using System.Text;
using RemoteAnalyst.BusinessLogic.BLL;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.Enums;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.Scheduler.Schedules;
using Helper = RemoteAnalyst.BusinessLogic.Util.Helper;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using RemoteAnalyst.AWS.Glacier;
using RemoteAnalyst.AWS.S3;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using log4net;
using RemoteAnalyst.BusinessLogic.Util;
using System.Security.Principal;

namespace RemoteAnalyst.Scheduler {
    internal static class Program {
        /// <summary>
        private static void Main() {
#if (!DEBUG)
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new SchedulerService()
            };
            ServiceBase.Run(ServicesToRun);
#else
            // https://stackify.com/log4net-guide-dotnet-logging/

            //var decrypt = new Decrypt();
            //Console.WriteLine(decrypt.strDESEncrypt("SERVER=192.168.1.66;PORT=3306;DATABASE=pmc080627;UID=localanalyst;PASSWORD=goneb4uc;Convert Zero Datetime=True;Pooling=false;AllowLoadLocalInfile=true;"));
            //ReadXML.ImportDataFromXML();
            //var dispatcher = new RANewReportDispatcher();
            //var dateStart = Convert.ToDateTime("2/26/2021 02:05:00 AM");
            //dispatcher.CheckDispatchDaily(dateStart);
            
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
#endif
        }
    }
}