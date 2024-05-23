using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using RemoteAnalyst.UWSRelay.BLL;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.FTPRelay.BLL {
    class TransferFile {
        private static readonly ILog Log = LogManager.GetLogger("RelayLog");
        private string _fileName;

        public TransferFile(string fileName) {
            _fileName = fileName;
        }

        internal void StartTransfer() {
            Thread.Sleep(10000);
            //Force all datetime to be in US format.
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            string systemSerial = "";
            try {
                Log.Info("***************************************************************");
                Log.Info("Get job file");
                //Insert INTO SampleInfo if it's Auto load.
                string triggerFileName = _fileName;
                string[] split = _fileName.Split(new[] { '_' });


                var watch = new FileUtil(ConnectionString.WatchFolder);

                string subString = split[0].Substring(3);

                string uwsFileName;
                string fileType = "UWS";
                systemSerial = watch.GetSystemSerial(triggerFileName);

                int tempSerial;
                if (!int.TryParse(systemSerial, out tempSerial)) {
                    return;
                }

                string cpuFileName = watch.GetCPUFileName(triggerFileName);
                string ossFileName = watch.GetOSSFileName(triggerFileName);
                string discopenFileName = watch.GetDISCOPENFileName(triggerFileName);
                string uwsFullPath = "";

                if (subString == "auto") {
                    uwsFileName = watch.GetUWSFileName(triggerFileName);
                    uwsFullPath = ConnectionString.SystemLocation + systemSerial + "\\" + uwsFileName;
                }
                else if (subString == "disk") {
                    uwsFileName = watch.GetUWSFileName(triggerFileName);
                    uwsFullPath = ConnectionString.SystemLocation + systemSerial + "\\" + uwsFileName;
                    fileType = "disk";
                }
                else {
                    return;
                }

                Log.InfoFormat("File FileType : {0}", fileType.ToUpper());
                Log.InfoFormat("Data file name : {0}", uwsFullPath);
                Log.InfoFormat("CPU file name : {0}", cpuFileName);
                Log.InfoFormat("OSS File Name : {0}", ossFileName);
                Log.InfoFormat("DISCOPEN File Name : {0}", discopenFileName);
                Log.Info("Start uploading file");
                
                var uploads = new Uploads();
                //Load UWS File.
                if (File.Exists(uwsFullPath))
                    uploads.UploadUWS(systemSerial, uwsFullPath, fileType, Log);
                else {
                    Log.InfoFormat("Missing File : {0}", uwsFullPath);
                }

                if (File.Exists(ConnectionString.SystemLocation + systemSerial + "\\" + cpuFileName))
                    uploads.UploadCPUInfo(systemSerial, ConnectionString.SystemLocation + systemSerial + "\\" + cpuFileName, Log);

                //Load OSS
                if (ossFileName.Length > 1) {
                    if (File.Exists(ConnectionString.SystemLocation + systemSerial + "\\" + ossFileName))
                        uploads.UploadOSS(systemSerial, ConnectionString.SystemLocation + systemSerial + "\\" + ossFileName, Log);
                }
                //Load DISCOPEN
                if (fileType == "System" && discopenFileName.Length > 1)
                    if (File.Exists(ConnectionString.SystemLocation + systemSerial + "\\" + discopenFileName))
                        uploads.UploadDiscOpen(systemSerial, ConnectionString.SystemLocation + systemSerial + "\\" + discopenFileName, Log);

                //Load Trigger File. Send 101 first.
                var triggerFile101 = triggerFileName.Replace(".txt", ".101");

                if (File.Exists(ConnectionString.WatchFolder + "\\" + triggerFile101))
                    uploads.UploadTrigger(systemSerial, ConnectionString.WatchFolder + "\\" + triggerFile101, Log);
                Thread.Sleep(1000);
                if (File.Exists(ConnectionString.WatchFolder + "\\" + triggerFileName))
                    uploads.UploadTrigger(systemSerial, ConnectionString.WatchFolder + "\\" + triggerFileName, Log);
            }
            catch (Exception ex) {
                Log.ErrorFormat("Error Processing System Folder: {0}, Error {1}", 
                    ConnectionString.SystemLocation, ex);
                var emailNotification = new EmailNotification();
                emailNotification.SendUploadFailedEmail(systemSerial, "UNKNOWN", "UNKNOWN", ex.Message);
            }
        }
    }
}
