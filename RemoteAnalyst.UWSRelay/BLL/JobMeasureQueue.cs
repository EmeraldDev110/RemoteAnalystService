using log4net;
using RemoteAnalyst.BusinessLogic.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.UWSRelay.BLL {
    class JobMeasureQueue {
        private static readonly ILog Log = LogManager.GetLogger("RelayLog");

        public void Timer_Elapsed(object source, System.Timers.ElapsedEventArgs e) {
            StartLoad();
        }

        public void StartLoad() {
            if (ConnectionString.MeasureList.Count > 0) {
                Log.Info("*******************************************************");
                Log.InfoFormat("CurrentCount: {0}", ConnectionString.MeasureList.Count);
                Log.InfoFormat("CurrentUploadCount: {0}", ConnectionString.CurrentUploadCount);
                Log.InfoFormat("UploadQueue: {0}", ConnectionString.UploadQueue);
                

                if (ConnectionString.CurrentUploadCount < ConnectionString.UploadQueue) {
                    ConnectionString.CurrentUploadCount++;
                    var dicEntry = ConnectionString.MeasureList.First();
                    var key = dicEntry.Key;
                    var value = dicEntry.Value;

                    try {
                        var uwSfile = new FileInfo(key);
                        var uwsFileDirectory = uwSfile.Directory.FullName.Trim();
                        var uwsFilePath = uwsFileDirectory + "\\UploadFolder\\" + uwSfile.Name.Trim();

                        Log.InfoFormat("uwSfile.FullName: {0}", uwSfile.FullName.Trim());
                        Log.InfoFormat("uwsFilePath: {0}", uwsFilePath);
                        Log.Info("Move the File!");
                        

                        File.Move(uwSfile.FullName.Trim(), uwsFilePath);

                        Log.Info("Call SendMeasureFile");
                        
                        var fileInfo = new FileInfo(uwsFilePath);
                        var jobMeasure = new JobMeasures();
                        jobMeasure.SendMeasureFile(value, fileInfo);
                        Log.Info("Finish SendMeasureFile");
                        
                    }
                    catch (Exception ex) {
                        Log.ErrorFormat("Error: {0}", ex);
                    }
                    finally {
                        ConnectionString.MeasureList.Remove(key);
                        ConnectionString.CurrentUploadCount--;
                    }
                }
            }
        }
    }
}
