using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.FTPRelay.BLL {
    class JobRelay {
        private readonly FileSystemWatcher _watcher = new FileSystemWatcher();
        /// <summary>
        /// Stop the watcher
        /// </summary>
        public void StopJobWatch() {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
            _watcher.EndInit();
        }


        /// <summary>
        /// Initialize the watcher to start the monitoring.
        /// </summary>
        public void StartJobWatch() {
            //watcher = new FileSystemWatcher();
            _watcher.Path = ConnectionString.WatchFolder;

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
            var transferFile = new TransferFile(e.Name);
            var thread = new Thread(transferFile.StartTransfer) {IsBackground = true};
            thread.Start();
        }

    }
}
