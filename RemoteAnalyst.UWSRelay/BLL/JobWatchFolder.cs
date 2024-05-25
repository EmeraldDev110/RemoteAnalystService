using log4net;
using RemoteAnalyst.BusinessLogic.Util;
using System;
using System.IO;
using System.Timers;

namespace RemoteAnalyst.UWSRelay.BLL {
    internal class JobWatchFolder {
        private static readonly ILog Log = LogManager.GetLogger("Cleaner");
        public void Timer_Elapsed(object source, ElapsedEventArgs e) {
            LoopWatchFolder();
        }

        public void LoopWatchFolder() {
            string systemLocation = ConnectionString.WatchFolder;

            //Look for the all files in folder.
            var uwsDirectory = new DirectoryInfo(systemLocation);
            var helper = new Helper();

            foreach (FileInfo uwSfile in uwsDirectory.GetFiles()) {
                if (File.Exists(uwSfile.FullName)) // IF FILE DOESN'T EXIST, ANOTHER THREAD ALREADY PROCESS IT.
                {
                    //Check if file is closed.
                    bool inUsed = helper.IsFileinUse(uwSfile);
                    if (!inUsed) {
                        //IF THE CREATION DATE IS MORE THAN A DAY OLD.  DELETE THE FILE.
                        //DateTime dtCompare = DateTime.Today.Date.AddDays(1); //FOR TESTING PURPOSES ADD 1 DAY TO DATE.
                        DateTime dtCompare = DateTime.Today.Date.AddDays(-1);
                        if (uwSfile.CreationTime.Date.CompareTo(dtCompare) <= 0) {
                            //FILE IS MORE THAN A DAY OLD, DELETE FILE.
                            DeleteFile(uwSfile);
                        }
                    }
                }
            }
        }

        private void DeleteFile(FileInfo uwSfile) {
            if (!File.Exists(uwSfile.FullName)) // IF FILE DOESN'T EXIST, ANOTHER THREAD ALREADY PROCESS IT.
            {
                return;
            }

            try {
                uwSfile.Delete();
                Log.Info("***************************************************************");
                Log.Info("Deleting trigger file");
                Log.InfoFormat("INFORMATIONAL deleted file {0}", uwSfile.FullName.Trim());
            }
            catch (Exception ex) {
                if (File.Exists(uwSfile.FullName)) // IF FILE DOESN'T EXIST, ANOTHER THREAD ALREADY PROCESS IT.
                {
                    Log.Error("Error in JobWatchFolder::DeleteFile [File Exists] Processing");
                    Log.ErrorFormat("System Folder: {0}", ConnectionString.WatchFolder);
                    Log.ErrorFormat("Error {0}", ex.Message);
                }
            }
        }
    }
}