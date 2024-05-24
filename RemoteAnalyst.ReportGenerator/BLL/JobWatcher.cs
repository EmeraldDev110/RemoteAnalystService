using System.IO;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.ReportGenerator.BLL {
    internal class JobWatcher {
        private readonly FileSystemWatcher _watcher = new FileSystemWatcher();

        public void StopJobWatch() {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
            _watcher.EndInit();
        }

        public void StartJobWatch(string folderToWatch) {
            //watcher = new FileSystemWatcher();
            _watcher.Path = folderToWatch;

            _watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                   | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            // Only watch text files.
            _watcher.Filter = "*.txt";

            // Add event handlers.
            _watcher.Created += OnCreated;

            // Begin watching.
            _watcher.EnableRaisingEvents = true;
        }

        private void OnCreated(object source, FileSystemEventArgs e) {
            //Insert trigger file to Database.
            if (e.Name.Contains("jobReportTrend") ||
                e.Name.Contains("jobReportStorageSchdule") ||
                e.Name.Contains("jobReportStorage") ||
                e.Name.Contains("jobReportQT") ||
                e.Name.Contains("jobReport") ||
                e.Name.Contains("jobReportPathway")) {
                var reportQueues = new ReportQueueService(ConnectionString.ConnectionStringDB);
                if (e.Name.Contains("jobReportTrend")) {
                    reportQueues.InsertNewQueueFor(e.Name, (int) BusinessLogic.Enums.Report.Types.Trend);
                }
                else if (e.Name.Contains("jobReportStorage")) {
                    reportQueues.InsertNewQueueFor(e.Name, (int) BusinessLogic.Enums.Report.Types.Storage);
                }
                else if (e.Name.Contains("jobReportQT")) {
                    reportQueues.InsertNewQueueFor(e.Name, (int) BusinessLogic.Enums.Report.Types.QT);
                }
                else if (e.Name.Contains("jobReportPathway")) {
                    reportQueues.InsertNewQueueFor(e.Name, (int)BusinessLogic.Enums.Report.Types.Pathway);
                }
                else if (e.Name.Contains("jobReportQNM")) {
                    reportQueues.InsertNewQueueFor(e.Name, (int)BusinessLogic.Enums.Report.Types.QNM);
                }
                else if (e.Name.Contains("jobReportEventPro")) {
                    reportQueues.InsertNewQueueFor(e.Name, (int)BusinessLogic.Enums.Report.Types.EventPro);
                }
                else if (e.Name.Contains("jobReport")) {
                    reportQueues.InsertNewQueueFor(e.Name, (int) BusinessLogic.Enums.Report.Types.DPA);
                }
            }
        }
    }
}