using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.UWSRelay.BLL {
    class Scheduler {
        //private Timer _checkFTPLog;
        private Timer _checkMeasure;
        private Timer _checkUWS;
        private Timer _checkTask;
        private Timer _checkWatchFolder;
        private Timer _checkMeasureList;
        private Timer _checkMeasureCleaner;
        private Timer _checkTaskChecker;
        private Timer _checkUWSUpload;

        public void StartScheduleTimers() {
            StartCheckFiles();
            StartJobTask();

            StartCheckMeasureFiles();
            StartCheckWatchFolder();
            StartCheckMeasureList();
            StartMeasureCleaner();
            StartTaskChecker();
            StartUWSUpload();
        }
        private void StartCheckMeasureList() {
            var jobMeasureQueue = new JobMeasureQueue();
            _checkMeasureList = new Timer(60000); //Once every 1 mins.
            _checkMeasureList.Elapsed += jobMeasureQueue.Timer_Elapsed;
            _checkMeasureList.AutoReset = true;
            _checkMeasureList.Enabled = true;
        }

        private void StartMeasureCleaner() {
            var jobMeasureCleaner = new JobMeasureCleaner();
            _checkMeasureCleaner = new Timer(86400000); //Once a day.
            _checkMeasureCleaner.Elapsed += jobMeasureCleaner.Timer_Elapsed;
            _checkMeasureCleaner.AutoReset = true;
            _checkMeasureCleaner.Enabled = true;
        }

        private void StartTaskChecker() {
            var jobTaskChecker = new JobTaskChecker();
            _checkTaskChecker = new Timer(300000); //Once every 5 mins.
            _checkTaskChecker.Elapsed += jobTaskChecker.Timer_Elapsed;
            _checkTaskChecker.AutoReset = true;
            _checkTaskChecker.Enabled = true;
        }

        /*private void StartCheckFTPLog() {
            var ftpLog = new JobFTPLog();
            _checkFTPLog = new Timer(300000); //Once every 5 mins.
            _checkFTPLog.Elapsed += ftpLog.Timer_Elapsed;
            _checkFTPLog.AutoReset = true;
            _checkFTPLog.Enabled = true;
        }*/

        private void StartCheckMeasureFiles() {
            var jobMeasures = new JobMeasures();
            _checkMeasure = new Timer(900000); //Once every 15 mins.
            _checkMeasure.Elapsed += jobMeasures.Timer_Elapsed;
            _checkMeasure.AutoReset = true;
            _checkMeasure.Enabled = true;
        }

        private void StartJobTask() {
            if (ConnectionString.IsProcessDirectlySystem) {
                var jobTask = new JobTask();
                _checkTask = new Timer(300000); //Once every 5 mins.

                _checkTask.Elapsed += jobTask.Timer_Elapsed;
                _checkTask.AutoReset = true;
                _checkTask.Enabled = true;
            }
        }

        private void StartCheckFiles()
        {
            var jobUWS = new JobUWS();
            if (ConnectionString.IsProcessDirectlySystem)
                _checkUWS = new Timer(300000); //Once every 5 mins.
            else
                _checkUWS = new Timer(60000); //Once every 1 mins.

            _checkUWS.Elapsed += jobUWS.Timer_Elapsed;
            _checkUWS.AutoReset = true;
            _checkUWS.Enabled = true;
        }

        private void StartCheckWatchFolder()
        {
            var jobWatchFolder = new JobWatchFolder();
            _checkWatchFolder = new Timer(86400000); //Once a day.
            _checkWatchFolder.Elapsed += jobWatchFolder.Timer_Elapsed;
            _checkWatchFolder.AutoReset = true;
            _checkWatchFolder.Enabled = true;
        }
        private void StartUWSUpload() {
            /*var jobUWSUpload = new JobUWSUpload();
            _checkUWSUpload = new Timer(300000); //Every 5 minute.
            _checkUWSUpload.Elapsed += jobUWSUpload.Timer_Elapsed;
            _checkUWSUpload.AutoReset = true;
            _checkUWSUpload.Enabled = true;*/
        }

    }
}
