using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using log4net;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.UWSRelay.BLL {
    class JobMeasures {
        private static readonly ILog Log = LogManager.GetLogger("RelayLog");
        public void Timer_Elapsed(object source, System.Timers.ElapsedEventArgs e) {
            LoopSystemFolders();
        }

        public void LoopSystemFolders() {
            string systemLocation = ConnectionString.FTPSystemLocation;

            var helper = new Helper();
            var folders = new DirectoryInfo(systemLocation);
            foreach (var folder in folders.GetDirectories()) {
                try
                {
                    if (Directory.Exists(folder.FullName + "\\measure"))
                    {
                        //Look for the measure files that ends with .180.
                        var measureDic = new DirectoryInfo(folder.FullName + "\\measure");
                        foreach (var file in measureDic.GetFiles().Where(x => x.Extension != ".101"))
                        {

                            //Check if file is closed.
                            var inUsed = helper.IsFileinUse(file);

                            if (!inUsed)
                            {
                                //Check for file size is increasing.
                                var fileSize = helper.IsFileSizeIncreasing(file);

                                if (!fileSize)
                                {
                                    //Send the file to NSDEMO and trigger the load.
                                    var archiveThread = new Thread(() => SendMeasureFile(folder.Name, file));
                                    archiveThread.IsBackground = true;
                                    archiveThread.Start();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //safety net. incase there is an exception on any single file
                }
            }
        }

        public void SendMeasureFile(string systemSerial, FileInfo file, bool isRetry = false) {
            try {
                Log.Info("*********************************************");
                Log.InfoFormat("file.Name: {0}",  file.Name);
                Log.InfoFormat("isRetry: {0}",  isRetry);
                

                string filecode = "175";
                using (var reader = new StreamReader(file.FullName)) {
                    string line = reader.ReadLine();
                    if (line.Contains("PAK")) {
                        filecode = "1729";
                    }
                }
                Log.InfoFormat("ConnectionString.ConnectionStringDB: {0}",  FTPFile.RemovePassword(ConnectionString.ConnectionStringDB));
                

                //var jobMeasureFile = new JobMeasureFileService(ConnectionString.ConnectionStringDB);
                //jobMeasureFile.InsertEntryFor(file.Name);

                //string systemSerial = folder.Name;
                var systemTable = new System_tblService(ConnectionString.ConnectionStringDB);
                string systemName = systemTable.GetSystemNameFor(systemSerial).Replace("\\", "");

                //string measFH = "$IDEL35.MEAS.MEAS" + systemTable.GetMeasFHFor(systemSerial);
                var nonstopInfo = new NonStopInfoService(ConnectionString.ConnectionStringDB);
                DataTable nonStopData = nonstopInfo.GetNonStopInfoFor();
                if (nonStopData.Rows.Count > 0) {
                    string ipAddress = nonStopData.Rows[0]["IPAddress"].ToString();
                    string user = nonStopData.Rows[0]["User"].ToString();
                    string volume = nonStopData.Rows[0]["Volume"].ToString();
                    var ftpPort = nonStopData.Rows[0]["FTPPort"].ToString();
                    var monitorPort = nonStopData.Rows[0]["MonitorPort"].ToString();
                    var volumeMeasFH = nonStopData.Rows[0]["VolumeMeasFH"].ToString();
                    var subVolumeMeasFH = nonStopData.Rows[0]["SubVolumeMeasFH"].ToString();

                    string measFH = $"{volumeMeasFH}.{subVolumeMeasFH}.MEAS" + systemTable.GetMeasFHFor(systemSerial);
                    Log.InfoFormat("systemSerial: {0}",  systemSerial);
                    Log.InfoFormat("systemName: {0}",  systemName);
                    Log.InfoFormat("measFH: {0}",  measFH);
                    

                    //Send data to NonStop.
                    var ftpFile = new FTPFile(systemSerial, systemName, file.DirectoryName);
                    var message = ftpFile.UploadFile(file.Name, Log, filecode, measFH, isRetry);


                    //jobMeasureFile.DeleteEntryFor(file.Name);
                    //Delete the trigger file.
                    try {
                        if (message.Length.Equals(0)) {
                            if (!ConnectionString.IsProcessDirectlySystem) {
                                if (File.Exists(file.FullName))
                                    file.Delete();
                            }
                        }
                    } catch (Exception ex) {
                        Log.ErrorFormat("{0} Delete Error: {1}", ex, file.Name);
                    } 
                } else {
                    Log.Info("No NonStop server info found!");                                       
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("Error: {0}", ex);
            }
        }
    }
}
