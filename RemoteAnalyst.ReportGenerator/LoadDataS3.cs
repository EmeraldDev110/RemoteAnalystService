using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Ionic.Zip;
using RemoteAnalyst.AWS.Glacier;
using RemoteAnalyst.AWS.S3;
using RemoteAnalyst.BusinessLogic.BLL;
using RemoteAnalyst.BusinessLogic.Enums;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.UWSLoader;
using RemoteAnalyst.ReportGenerator.BLL;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.Util;
using log4net;

namespace RemoteAnalyst.ReportGenerator {
    internal class LoadDataS3 {
        private static readonly ILog Log = LogManager.GetLogger("GlacierProcess");
        /// <summary>
        /// This function will get the list of files from S3 and load data.
        /// </summary>
        /// <param name="systemSerial">systemSerial</param>
        /// <param name="startTime">report start time</param>
        /// <param name="endTime">report end time</param>
        /// <param name="qt">true when calling from QT</param>
        internal bool LoadUWSFiles(string systemSerial, DateTime startTime, DateTime endTime, bool qt, string databasePostfix, int reportDownloadID, string systemName, List<string> emailList) {
            try {
                var subFolder = DateTime.Now.Ticks;
                string systemLocation = ConnectionString.SystemLocation;
                return GetFilesList(systemSerial, startTime, endTime, qt, databasePostfix, subFolder, reportDownloadID, systemName, emailList);
            }
            catch (Exception ex) {
                //Log.InfoFormat("LoadUWSFiles exception: {0}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Get list of files from UWSDirectory Table.
        /// </summary>
        /// <param name="systemSerial">systemSerial</param>
        /// <param name="startTime">startTime</param>
        /// <param name="endTime">endTime</param>
        /// <param name="writer">log file</param>
        /// <param name="qt">true when calling from QT</param>
        /// <param name="databasePostfix">databasePostfix</param>
        private bool GetFilesList(string systemSerial, DateTime startTime, DateTime 
            endTime, bool qt, string databasePostfix, long subFolder, int reportDownloadID, string systemName, List<string> emailList) {
            var checker = new DISCOPENChecker(ConnectionString.SystemLocation, ConnectionString.ConnectionStringDB);
            var directory = new UWSDirectoryService(ConnectionString.ConnectionStringDB);
            List<string> uwsFiles = checker.CheckFiles(systemSerial, startTime, endTime, qt);
            //RA-705 - To notify user if any download fails
            Dictionary<string, string> filesFailed = new Dictionary<string, string>();
            bool success = false;

            Log.InfoFormat("uwsFiles.Count: {0}", uwsFiles.Count);
            Log.InfoFormat("Start time: {0}", startTime);
            Log.InfoFormat("End time: {0}", endTime);
            Log.InfoFormat("List of files to load: {0}", String.Join(",", uwsFiles.ToArray()));
            Log.InfoFormat("Load started at: {0}", DateTime.Now);
            Log.InfoFormat("reportDownloadID: {0}", reportDownloadID);
            
            if (uwsFiles.Count == 0)
            {
                Log.Info("Since no files to download from S3 returning failure");
                return false;
            }
            var loadingStatus = new CheckLoad();
            bool databaseExistence;

            var reportDownloadLogService = new ReportDownloadLogService(ConnectionString.ConnectionStringDB);
            reportDownloadLogService.InsertNewLogFor(reportDownloadID, DateTime.Now, "Total AWS files: " + uwsFiles.Count);
            
            try {
                if (uwsFiles.Count > 0) {
                    databaseExistence = loadingStatus.CheckDatabaseExistence(systemSerial, databasePostfix);
					long directorySize = Directory.GetFiles(ConnectionString.ServerPath, "*", SearchOption.AllDirectories).Sum(t => (new FileInfo(t).Length));
					Log.InfoFormat("Folder {0}, size before load: {1}", ConnectionString.ServerPath, directorySize);
					Log.InfoFormat("databaseExistence: {0}", databaseExistence);
					

                    var fileNumber = 0;
                    foreach (var uwsFile in uwsFiles) {

                        fileNumber++;

                        var fileName = "";
                        if (uwsFile.StartsWith("Glacier")) {
                            var temp = uwsFile.Split('|');
                            fileName = temp[1];
                        }
                        else fileName = uwsFile;

                        reportDownloadLogService.InsertNewLogFor(reportDownloadID, DateTime.Now, "Loading File " + fileName + " (" +
                            fileNumber + " of " + uwsFiles.Count + ")");
                        
                        var duplicate = false;
                        if (databaseExistence) {
                                Log.InfoFormat("Database existing ! Check entry: {0}", fileName);
                                duplicate = loadingStatus.CheckEntry(systemSerial, fileName);
                        }
                        Log.InfoFormat("Check Duplicate for: {0}|{1}", systemSerial, fileName);
                        Log.InfoFormat("Duplicate: {0}", duplicate);
                        

                        if (!duplicate) {
                            var download = false;
                            Log.Info("Check file is being downloaded or not");
                            if (File.Exists(ConnectionString.ServerPath + "\\" + fileName)) {
                                download = true;
                                Log.Info("It is being accessed");
                            }

                            if (!download) {//If the file is accessed by another process, means it is being loaded, skip this part
                                Log.InfoFormat("Not duplicate hour/file: {0}", fileName);
                                string saveLocation = "";
                                
                                if (ConnectionString.IsLocalAnalyst) {
                                    Log.InfoFormat("Download from Network Location: {0}|{1}", systemSerial, fileName);
                                    
                                    if (File.Exists(fileName)) {
                                        var fileInfo = new FileInfo(fileName);
                                        saveLocation = ConnectionString.ServerPath + @"\Systems\" + systemSerial + "\\" + subFolder + "\\" + fileInfo.Name;
                                        fileInfo.CopyTo(saveLocation);
                                    }
                                    else {
                                        Log.Info("File doesn't exists, continue to next file");
                                        
                                        filesFailed.Add(fileName, "D");
                                        continue;
                                    }
                                }
                                else
                                {

                                    Log.InfoFormat("Download from S3 or Glacier: {0}|{1}", systemSerial, uwsFile);
                                    saveLocation = DownloadFilesFromS3(uwsFile);
                                    if (saveLocation.Equals(""))
                                    { //It's possible that two same orders are downloading the same file
                                        Log.Info("Download failed, continue to next file");
                                        filesFailed.Add(fileName, "D");
                                        continue;
                                    }
                                }
                                //Set the flag to true so we know when the load is done.
                                directory.UpdateLoadingFor(systemSerial, startTime, endTime, 1);
                                Log.Info("Start loading UWS file.");
                                
                                success = LoadUWSFile(systemSerial, saveLocation, fileName, false, databasePostfix, subFolder, ConnectionString.DatabasePrefix, startTime, endTime);
                                Log.InfoFormat("Load UWSFile " + fileName + " successful ? " + success);
                                
                                if (!success) {
                                    directory.UpdateLoadingFor(systemSerial, fileName, 0);
                                    if (databaseExistence) {//Put a fix here, if the first file failed to unzip, the DB won't be created, removing the entry could cause exception
                                        loadingStatus.RemoveLoadingStatus(systemSerial, fileName);
                                        Log.InfoFormat("Error occurred, remove the UWSLoadingStatus entry: {0}", fileName);
                                        
                                    }
                                    Log.InfoFormat("Error occurred, remove the UWSLoadingStatus entry: {0}", fileName);
                                    if (!filesFailed.ContainsKey(fileName)) filesFailed.Add(fileName, "L");
                                    if (uwsFiles.Count > 1) {//If we have multiple files to load, one of them failed, need to continue to next load
                                        continue;
                                    }//but if we only have 1 file, need to stop load or report generatrion
                                    return false;
                                }
                                else {
                                    databaseExistence = true;
                                }
                                //Set the flag to false after loading is done.
                                directory.UpdateLoadingFor(systemSerial, fileName, 0);
                                //Remove from UWSLoadingStatus
//#if !DEBUG
                                if(!ConnectionString.IsLocalAnalyst)
                                    loadingStatus.RemoveLoadingStatus(systemSerial, fileName);
//#endif

                                Log.InfoFormat("Remove UWSLoadingStatus entry: {0}", fileName);
                                
                            }
                            else {
                                Log.InfoFormat("It is being accessed: {0}", fileName);
                            }
                        }
                        else {
                            Log.InfoFormat("It's duplicate hour: {0}", fileName);
                        }
                    }
					directorySize = Directory.GetFiles(ConnectionString.ServerPath, "*", SearchOption.AllDirectories).Sum(t => (new FileInfo(t).Length));
					Log.InfoFormat("Folder " + ConnectionString.ServerPath + " size after load: {0}", directorySize);
					
				}
            }
            catch (Exception ex) {
                Log.InfoFormat("Exception occurred when loading UWS file: {0}", ex);
                
                return false;
            }
            finally {
            }

            //Wait until all the loads (duplicate loades) are complete.
            if (uwsFiles.Count > 0) {
                bool loading = true;
                while (loading) {
                    foreach (var uwsFile in uwsFiles) {
                        var fileName = "";
                        if (uwsFile.StartsWith("Glacier")) {
                            var temp = uwsFile.Split('|');
                            fileName = temp[1];
                        }
                        else fileName = uwsFile;

                        var duplicate = loadingStatus.CheckEntry(systemSerial, fileName);
                        if (duplicate) {
                            loading = true;
                            break;
                        }
                        loading = false;
                    }

                    //wait 1 mins.
                    Thread.Sleep(60000);
                }
            }

            //RA-705 Send email to support
            if (filesFailed.Count > 0) {
                string subject = "";
                string desc = "";
                if (ConnectionString.IsLocalAnalyst) {
                    subject = LicenseService.GetProductName(ConnectionString.ConnectionStringDB) + " - Issue noted";
                    desc = "network share folder";
                }
                else {
                    subject = "Remote Analyst - Issue noted";
                    desc = "Amazon S3 due to S3 retention rule.";
                }
                var email = new EmailToSupport(ConnectionString.AdvisorEmail, ConnectionString.SupportEmail,
                            ConnectionString.WebSite,
                            ConnectionString.EmailServer, ConnectionString.EmailPort, ConnectionString.EmailUser,
                            ConnectionString.EmailPassword, ConnectionString.EmailAuthentication,
                            ConnectionString.SystemLocation, ConnectionString.ServerPath,
                            ConnectionString.EmailIsSSL, 
                            ConnectionString.IsLocalAnalyst, 
                            ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                 var reportDownloads = new ReportDownloadService(ConnectionString.ConnectionStringDB);
                DateTime reqDate = reportDownloads.GetRequestDateFor(reportDownloadID);
#if DEBUG
                reqDate = DateTime.Now;
#endif
                string type = qt ? "QT" : "DPA";
                string reportName = systemName.Replace("\\", "") + "(" + systemSerial + ")" + " - " + type + " for " + startTime.ToString("yyyy-MM-dd HHmm") + " to " + endTime.ToString("yyyy-MM-dd HHmm") + ".xls";

                var custInfo = new CusAnalystService(ConnectionString.ConnectionStringDB);
                emailList.Add(ConnectionString.SupportEmail);
                foreach (string custEmail in emailList) {
                    string custName = custInfo.GetUserNameFor(custEmail);
                    if (custName.Length.Equals(0)) {
                        custName = "Customer";
                    }
                    email.SendFileLoadErrorEmail(subject, desc, custEmail, custName, ConnectionString.SupportEmail, systemSerial, filesFailed, reportName, reqDate, startTime, endTime);
                }
            }
            
            return success;
        }

        /// <summary>
        /// Download files from S3
        /// </summary>
        /// <param name="fileLocation">S3 File Location</param>
        /// <returns></returns>
        private string DownloadFilesFromS3(string fileLocation) {
            string saveFolder = ConnectionString.ServerPath;
            string saveLocation = "";
            if (ConnectionString.IsLocalAnalyst) {
                if (File.Exists(fileLocation)) {
                    var fi = new FileInfo(fileLocation);
                    fi.CopyTo(saveFolder + fi.Name);
                }
                else {
                    Log.InfoFormat("File does not exists {0}", fileLocation);
                    return "";
                }
            }
            else
            {
                Log.InfoFormat("ConnectionString.S3UWS: {0}", ConnectionString.S3UWS);

                try
                {
                    if (fileLocation.StartsWith("Glacier"))
                    {
                        var temp = fileLocation.Split('|');
                        if (!string.IsNullOrEmpty(ConnectionString.VaultName))
                        {
                            IAmazonGlacier amazonGlacier = new AmazonGlacierRA();
                            amazonGlacier.FastGlacierDownload(ConnectionString.VaultName, temp[2], saveFolder + "\\" + temp[1]);
                            saveLocation = saveFolder + "\\" + temp[1];
                            FileInfo s3DownloadFileInfo = new FileInfo(saveLocation);
                            Log.InfoFormat("Download file name: {0} size: {1}", fileLocation, s3DownloadFileInfo.Length);
                        }
                    }
                    else
                    {
                        var s3 = new AmazonS3(ConnectionString.S3UWS);
                        saveLocation = s3.ReadS3(fileLocation, saveFolder);
                        FileInfo s3DownloadFileInfo = new FileInfo(saveLocation);
                        Log.InfoFormat("Download file name: {0} size: {1}", fileLocation, s3DownloadFileInfo.Length);
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Download error: {0}", ex.Message);
                    return "";
                }
            }
            return saveLocation;
        }

        /// <summary>
        /// Load Downloaded file to Local Database.
        /// </summary>
        /// <param name="systemSerial">systemSerial</param>
        /// <param name="saveLocation">Unzip file location</param>
        /// <param name="writer">log file</param>
        internal bool LoadUWSFile(string systemSerial, string saveLocation, string uwsFile, bool duplicate, string databasePostfix, long subFolder, string databasePrefix, DateTime startTime, DateTime endTime) {
            string saveFolder = ConnectionString.ServerPath;
            string unzippedLocation = saveFolder + "\\Systems\\" + systemSerial + "\\" + subFolder + "\\";
            bool success = false;

            if (File.Exists(saveLocation)) {
                try {
                    string filename;
                    if (saveLocation.EndsWith(".zip")) {
                        using (ZipFile zip = ZipFile.Read(saveLocation)) {
                            ZipEntry entry = zip.First();
                            entry.Extract(unzippedLocation, ExtractExistingFileAction.OverwriteSilently);
                            filename = unzippedLocation + "\\" + entry.FileName;
                        }
                    }
                    else {
                        filename = saveLocation;
                    }

                    DateTime currTime = DateTime.Now;
                    Log.Info("***********************************************************************");
                    Log.InfoFormat("Loading UWS data: {0}", currTime);
                    Log.InfoFormat("System serial number: {0}", systemSerial);
                    Log.InfoFormat("saveLocation: {0}", saveLocation);
                    Log.InfoFormat("File Name: {0}", filename);
                    

                    var collectionType = new UWSFileInfo();
                    Log.Info("Get Uws file version..");
                    
                    UWS.Types checkSPAM = collectionType.UwsFileVersionNew(filename);

                    var loadUWS = new LoadUWS(ConnectionString.ConnectionStringDB, false);
                    Log.Info("CreateNewData");
                    
                    success = loadUWS.CreateNewData(filename, Log, checkSPAM, ConnectionString.TempDatabaseConnectionString, ConnectionString.MasterDatabaseConnectionString, false, uwsFile, duplicate, ConnectionString.IsLocalAnalyst, databasePostfix, databasePrefix, startTime, endTime);

                    if (success) {
                        Log.InfoFormat("Load DATA successfully in {0}", (DateTime.Now - currTime));
                        Log.InfoFormat("Delete zip files & UWS file: {0}", saveLocation);
                        
                        //Delete zip files
                        if (File.Exists(saveLocation)) {
                            File.Delete(saveLocation);
                        }
                        //Delete UWS file
                        if (File.Exists(filename)) {
                            File.Delete(filename);
                        }
                    }
                    else {
                        Log.Info("Load DATA failed...");
                    }
                }
                catch (Exception ex) {
                    Log.InfoFormat("Error: {0}", ex.Message);
                    return false;
                }
            }
            return success;
        }
    }
}