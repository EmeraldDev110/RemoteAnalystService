using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using Ionic.Zip;
using log4net;
using RemoteAnalyst.AWS.S3;
using RemoteAnalyst.BusinessLogic.BLL;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.UWSRelay.BLL {
    internal class JobMeasureCleaner {
        private static readonly ILog Log = LogManager.GetLogger("Cleaner");
        public void Timer_Elapsed(object source, ElapsedEventArgs e) {
            CleanMeasure();
        }

        public void CleanMeasure() {
            string systemLocation = ConnectionString.FTPSystemLocation;

            var helper = new Helper();
            var folders = new DirectoryInfo(systemLocation);
            var zipFiles = new List<string>();

            foreach (var folder in folders.GetDirectories()) {
                try
                {
                    if (Directory.Exists(folder.FullName + "\\UploadFolder"))
                    {
                        //Look for the measure files that ends with .180.
                        var measureDic = new DirectoryInfo(folder.FullName + "\\UploadFolder");
                        foreach (var file in measureDic.GetFiles().Where(x => x.Extension == "" && x.CreationTime.Date <= DateTime.Now.AddDays(-4)))
                        {
                            try
                            {
                                //Check if file is closed.
                                var inUsed = helper.IsFileinUse(file);

                                if (!inUsed)
                                {
                                    //Check for file size is increasing.
                                    var fileSize = helper.IsFileSizeIncreasing(file);

                                    if (!fileSize)
                                    {
                                        /*var archiveThread = new Thread(() => CheckFiles(folder.Name, file));
                                        archiveThread.IsBackground = true;
                                        archiveThread.Start();*/
                                        if (!zipFiles.Contains(file.FullName))
                                            zipFiles.Add(file.FullName);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                //safety net. incase there is an exception on any single file
                            }
                        }

                        if (zipFiles.Count > 0)
                        {
                            //var archiveThread = new Thread(() => ZipFiles(zipFiles)) { IsBackground = true };
                            //archiveThread.Start();
                            try
                            {
                                ZipFiles(folder.Name, zipFiles);
                            }
                            catch
                            {
                            }
                            finally
                            {
                                zipFiles.Clear();
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

        private void ZipFiles(string systemSerial, List<string> zipFiles) {
            Log.Info("*******************************************************");
            Log.InfoFormat("systemSerial: {0}", systemSerial);
            

            var zipFileName = systemSerial + "-" + DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "-Backup";
            var saveLocation = ConnectionString.FTPSystemLocation;
            Log.InfoFormat("Zip File Name: {0}{1}.zip", saveLocation, zipFileName);


            //Zip the file and upload to S3.
            try {
                using (var zip = new ZipFile()) {
                    zip.UseZip64WhenSaving = Zip64Option.Always;
                    Log.InfoFormat("files: {0}{1}.zip", saveLocation, zipFileName);

                    foreach (var file in zipFiles) {
                        Log.InfoFormat("     {0}", file);
                        zip.AddFile(file, string.Empty); //Need to add string.empty to second parameter to make the file display on top of the zip layer.
                    }

                    if (!Directory.Exists(saveLocation)) {
                        Directory.CreateDirectory(saveLocation);
                    }
                    zip.Save(saveLocation + zipFileName + ".zip");
                }

                Log.Info("Delete Measure Files");
                
                try {
                    foreach (var file in zipFiles) {
                        if (File.Exists(file))
                            File.Delete(file);
                    }
                }
                catch (Exception ex) {
                    Log.ErrorFormat("Delete Measure Files error: {0}", ex.Message);
                }

                var retry = 0;
                var success = false;
                //upload zip file to S3.
                Log.Info("S3 Bucket Name: s3-visa-backups");
                do {
                    try {
                        var s3 = new AmazonS3("s3-visa-backups");
                        string fullAWSKey = "Systems";

                        if (File.Exists(saveLocation + zipFileName + ".zip"))
                            s3.WriteToS3MultiThreads(zipFileName + ".zip", saveLocation + zipFileName + ".zip");
                        retry = 5;
                        success = true;
                    }
                    catch (Exception ex) {
                        Thread.Sleep(AmazonError.GetTimeoutDuration(retry));
                        Log.ErrorFormat("S3 Error:" + ex.Message);
                        
                    }
                } while (retry < 5);

                Log.InfoFormat("success: {0}", success);
                

                if (success) {
                    if (File.Exists(saveLocation + zipFileName + ".zip"))
                        File.Delete(saveLocation + zipFileName + ".zip");
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("Error: {0}", ex.Message);
                
            }
        }
    }
}
