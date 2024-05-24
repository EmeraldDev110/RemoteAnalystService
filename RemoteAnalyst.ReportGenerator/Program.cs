using System;
using System.Threading;
using RemoteAnalyst.ReportGenerator.BLL;
using System.Configuration;
using RemoteAnalyst.BusinessLogic.Util;
using System.ServiceProcess;

namespace RemoteAnalyst.ReportGenerator {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main() {
#if (!DEBUG)
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new ReportGeneratorService() 
            };
            ServiceBase.Run(ServicesToRun);
#else
            try
            {
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["S3XML"]))
                {
                    ReadXML.ImportDataFromXML();
                }
                else
                {
                    ReadXML.ImportDataFromXMLS3();
                }

                if (ConnectionString.IsLocalAnalyst)
                {
                    var jobScheduleLocalAnalyst = new JobScheduleLocalAnalyst();
                    jobScheduleLocalAnalyst.StartTriggerTimers();
                }
                else
                {
                    var sqs = new Amazon();
                    var threadTrend = new Thread(sqs.ReadQueue);
                    threadTrend.IsBackground = true;
                    threadTrend.Start();
                    /* Dead code, need to research 
                    var watcher = new JobWatcher();
                    string folderPath = ConnectionString.WatchFolder;
                    watcher.StartJobWatch(folderPath);
                    */
                }

                var schedule = new JobSchedules();
                schedule.StartScheduleTimers();
            }
            catch (Exception ex)
            {

            }
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
#endif
        }
    }
}