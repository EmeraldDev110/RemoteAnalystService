using log4net;
using RemoteAnalyst.BusinessLogic.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace RemoteAnalyst.FTPRelay.BLL {
    class Uploads {
        private readonly IUploadFile _uploadFile = new UploadFile();

        internal bool UploadUWS(string systemSerial, string uwsFileName, string fileType, ILog log) {
            bool ftpSuccess = true;
            var fileInfo = new FileInfo(uwsFileName);

            var serverToConnect = GetFTPServer();
            //foreach (var ftpServer in ConnectionString.FTPServers) {
            var retry = 0;
            while (retry <= ConnectionString.MaxRetries) {
                var remoteFileName = "/Systems/" + systemSerial + "/" + fileInfo.Name;
                var systemDirectory = "/Systems/" + systemSerial + "/";
                //Increase the ConnectionString.FTPServers count by 1.
                ConnectionString.FTPServers[serverToConnect]++;
                var message = _uploadFile.Upload(serverToConnect, uwsFileName, remoteFileName, systemDirectory);
                if (message.Length == 0) {
                    log.Info("Finish uploading UWS file");
                    
                    try {
                        log.Info("Deleting File");
                        if (File.Exists(uwsFileName)) {
                            File.Delete(uwsFileName);
                        }
                    }
                    catch (Exception ex) {
                        log.ErrorFormat("Error: {0}", ex);                        
                    }
                    return ftpSuccess;
                }
                retry++;
                log.InfoFormat("UWS Uploading Failed. Retry: {0}", retry);
                log.InfoFormat("UWS Uploading Failed. Message: {0}", message);
                
                if (retry <= ConnectionString.MaxRetries)
                    Thread.Sleep(ConnectionString.RetryInterval);
                else {
                    var emailNotification = new EmailNotification();
                    emailNotification.SendUploadFailedEmail(systemSerial, uwsFileName, fileType.ToUpper(), message);
                    ftpSuccess = false;
                }
            }
            //}

            return ftpSuccess;
        }

        internal bool UploadCPUInfo(string systemSerial, string cpuFileName, ILog log) {
            bool ftpSuccess = true;
            //Load CPU Info
            var fileInfo = new FileInfo(cpuFileName);

            var serverToConnect = GetFTPServer();
            //foreach (var ftpServer in ConnectionString.FTPServers) {
                int retry = 0;
                while (retry <= ConnectionString.MaxRetries) {
                    string remoteFileName = "/Systems/" + systemSerial + "/" + fileInfo.Name;
                    var systemDirectory = "/Systems/" + systemSerial + "/";
                    //Increase the ConnectionString.FTPServers count by 1.
                    ConnectionString.FTPServers[serverToConnect]++;
                    var message = _uploadFile.Upload(serverToConnect, cpuFileName, remoteFileName, systemDirectory);
                    if (message.Length == 0) {
                        log.Info("Finish uploading CUP INFO file");
                        
                        try {
                            log.Info("Deleting File");
                            

                            if (File.Exists(cpuFileName)) {
                                File.Delete(cpuFileName);
                            }
                        }
                        catch (Exception ex) {
                            log.ErrorFormat("Error: {0}", ex);
                            
                        }
                        return ftpSuccess;
                    }
                    retry++;
                    log.InfoFormat("CPU Info Uploading Failed. Retry: {0}", retry);
                    
                    if (retry <= ConnectionString.MaxRetries)
                        Thread.Sleep(ConnectionString.RetryInterval);
                    else {
                    var emailNotification = new EmailNotification();
                        emailNotification.SendUploadFailedEmail(systemSerial, cpuFileName, "CPU INFO", message);
                        ftpSuccess = false;
                    }
                }
            //}
            return ftpSuccess;
        }

        internal bool UploadOSS(string systemSerial, string ossFileName, ILog log) {
            bool ftpSuccess = true;
            var fileInfo = new FileInfo(ossFileName);

            var serverToConnect = GetFTPServer();
            //foreach (var ftpServer in ConnectionString.FTPServers) {
                int retry = 0;
                while (retry <= ConnectionString.MaxRetries) {
                    string remoteFileName = "/Systems/" + systemSerial + "/" + fileInfo.Name;
                    var systemDirectory = "/Systems/" + systemSerial + "/";
                    //Increase the ConnectionString.FTPServers count by 1.
                    ConnectionString.FTPServers[serverToConnect]++;
                    var message = _uploadFile.Upload(serverToConnect, ossFileName, remoteFileName, systemDirectory);
                    if (message.Length == 0) {
                        log.Info("Finish uploading OSS file");
                        
                        try {
                            log.Info("Deleting File");
                            

                            if (File.Exists(ossFileName)) {
                                File.Delete(ossFileName);
                            }
                        }
                        catch (Exception ex) {
                            log.ErrorFormat("Error: {0}", ex);
                            
                        }
                        return ftpSuccess;
                    }
                    retry++;
                    log.InfoFormat("OSS Uploading Failed. Retry: {0}", retry);
                    
                    if (retry <= ConnectionString.MaxRetries)
                        Thread.Sleep(ConnectionString.RetryInterval);
                    else {
                        var emailNotification = new EmailNotification();
                        emailNotification.SendUploadFailedEmail(systemSerial, ossFileName, "OSS", message);
                        ftpSuccess = false;
                    }
                }
            //}
            return ftpSuccess;
        }

        internal bool UploadDiscOpen(string systemSerial, string discopenFileName, ILog log) {
            bool ftpSuccess = true;
            var fileInfo = new FileInfo(discopenFileName);

            var serverToConnect = GetFTPServer();
            //foreach (var ftpServer in ConnectionString.FTPServers) {
                int retry = 0;
                while (retry <= ConnectionString.MaxRetries) {
                    string remoteFileName = "/Systems/" + systemSerial + "/" + fileInfo.Name;
                    var systemDirectory = "/Systems/" + systemSerial + "/";
                    //Increase the ConnectionString.FTPServers count by 1.
                    ConnectionString.FTPServers[serverToConnect]++;
                    var message = _uploadFile.Upload(serverToConnect, discopenFileName, remoteFileName, systemDirectory);
                    if (message.Length == 0) {
                        log.Info("Finish uploading DISCOPEN file");
                        
                        try {
                            log.Info("Deleting File");
                            

                            if (File.Exists(discopenFileName)) {
                                File.Delete(discopenFileName);
                            }
                        }
                        catch (Exception ex) {
                            log.ErrorFormat("Error: {0}", ex);
                            
                        }
                        return ftpSuccess;
                    }
                    retry++;
                    log.InfoFormat("DISCOPEN Uploading Failed. Retry: {0}", retry);
                    
                    if (retry <= ConnectionString.MaxRetries)
                        Thread.Sleep(ConnectionString.RetryInterval);
                    else {
                        var emailNotification = new EmailNotification();
                        emailNotification.SendUploadFailedEmail(systemSerial, discopenFileName, "DISCOPEN", message);
                        ftpSuccess = false;
                    }
                }
            //}
            return ftpSuccess;
        }

        internal bool UploadDisk(string systemSerial, string diskFileName, ILog log) {
            bool ftpSuccess = true;
            var fileInfo = new FileInfo(diskFileName);

            var serverToConnect = GetFTPServer();
            //foreach (var ftpServer in ConnectionString.FTPServers) {
                int retry = 0;
                while (retry <= ConnectionString.MaxRetries) {
                    string remoteFileName = "/Systems/" + systemSerial + "/" + fileInfo.Name;
                    var systemDirectory = "/Systems/" + systemSerial + "/";
                    //Increase the ConnectionString.FTPServers count by 1.
                    ConnectionString.FTPServers[serverToConnect]++;
                    var message = _uploadFile.Upload(serverToConnect, diskFileName, remoteFileName, systemDirectory);
                    if (message.Length == 0) {
                        log.Info("Finish uploading DISCOPEN file");
                        
                        try {
                            log.Info("Deleting File");
                            

                            if (File.Exists(diskFileName)) {
                                File.Delete(diskFileName);
                            }
                        }
                        catch (Exception ex) {
                            log.ErrorFormat("Error: {0}", ex);
                            
                        }
                        return ftpSuccess;
                    }
                    retry++;
                    log.InfoFormat("DISK Uploading Failed. Retry: {0}", retry);
                    
                    if (retry <= ConnectionString.MaxRetries)
                        Thread.Sleep(ConnectionString.RetryInterval);
                    else {
                        var emailNotification = new EmailNotification();
                        emailNotification.SendUploadFailedEmail(systemSerial, diskFileName, "DISK", message);
                        ftpSuccess = false;
                    }
                }
            //}
            return ftpSuccess;
        }
        public bool UploadPathway(string systemSerial, string pathwayFileName, ILog log) {
            bool ftpSuccess = true;
            var fileInfo = new FileInfo(pathwayFileName);

            var serverToConnect = GetFTPServer();
            //foreach (var ftpServer in ConnectionString.FTPServers) {
                int retry = 0;
                while (retry <= ConnectionString.MaxRetries) {
                    string remoteFileName = "/Systems/" + systemSerial + "/" + fileInfo.Name;
                    var systemDirectory = "/Systems/" + systemSerial + "/";
                    //Increase the ConnectionString.FTPServers count by 1.
                    ConnectionString.FTPServers[serverToConnect]++;
                    var message = _uploadFile.Upload(serverToConnect, pathwayFileName, remoteFileName, systemDirectory);
                    if (message.Length == 0) {
                        log.Info("Finish uploading Pathway file");
                        
                        try {
                            log.Info("Deleting File");
                            

                            if (File.Exists(pathwayFileName)) {
                                File.Delete(pathwayFileName);
                            }
                        }
                        catch (Exception ex) {
                            log.ErrorFormat("Error: {0}", ex);
                            
                        }
                        return ftpSuccess;
                    }
                    retry++;
                    log.InfoFormat("Pathway Uploading Failed. Retry: {0}", retry);
                    
                    if (retry <= ConnectionString.MaxRetries)
                        Thread.Sleep(ConnectionString.RetryInterval);
                    else {
                        var emailNotification = new EmailNotification();
                        emailNotification.SendUploadFailedEmail(systemSerial, pathwayFileName, "Pathway", message);
                        ftpSuccess = false;
                    }
                }
            //}
            return ftpSuccess;
        }

        internal bool UploadTrigger(string systemSerial, string triggerFileName, ILog log) {
            bool ftpSuccess = true;
            var fileInfo = new FileInfo(triggerFileName);

            var serverToConnect = GetFTPServer();
            //foreach (var ftpServer in ConnectionString.FTPServers) {
                int retry = 0;
                while (retry <= ConnectionString.MaxRetries) {
                    string remoteFileName = "/JobPool/" + fileInfo.Name;
                    var triggerDirectory = "/JobPool/";
                    //Increase the ConnectionString.FTPServers count by 1.
                    ConnectionString.FTPServers[serverToConnect]++;
                    var message = _uploadFile.Upload(serverToConnect, triggerFileName, remoteFileName, triggerDirectory);
                    if (message.Length == 0) {
                        log.Info("Finish uploading Trigger file");
                        
                        try {
                            log.Info("Deleting File");
                            

                            if (File.Exists(triggerFileName)) {
                                File.Delete(triggerFileName);
                            }
                        }
                        catch (Exception ex) {
                            log.ErrorFormat("Error: {0}", ex);
                            
                        }
                        return ftpSuccess;
                    }
                    retry++;
                    log.InfoFormat("Trigger Uploading Failed: {0}", retry);
                    
                    if (retry <= ConnectionString.MaxRetries)
                        Thread.Sleep(ConnectionString.RetryInterval);
                    else {
                        var emailNotification = new EmailNotification();
                        emailNotification.SendUploadFailedEmail(systemSerial, triggerFileName, "TRIGGER", message);
                        ftpSuccess = false;
                    }
                }
            //}
            return ftpSuccess;
        }

        internal List<string> CreateTriggerFile(SystemFolderWatch.FileType fileType, string systemSerial, string fileName) {
            var message = new StringBuilder();

            if (fileType == SystemFolderWatch.FileType.UWS)
                message.Append("SYSTEM\n" + fileName + "\n" + systemSerial);
            else if (fileType == SystemFolderWatch.FileType.Pathway)
                message.Append("PATHWAY\n" + fileName + "\n" + systemSerial);
            else if (fileType == SystemFolderWatch.FileType.DISK)
                message.Append("DISK\n" + fileName + "\n" + systemSerial);
            else if (fileType == SystemFolderWatch.FileType.CPUInfo)
                message.Append("CPUInfo\n" + fileName + "\n" + systemSerial);
            else if (fileType == SystemFolderWatch.FileType.OSS)
                message.Append("OSS\n" + fileName + "\n" + systemSerial);
            else if (fileType == SystemFolderWatch.FileType.DISCOPEN)
                message.Append("DISCOPEN\n" + fileName + "\n" + systemSerial);

            string jobPoolPath = ConnectionString.SystemLocation + systemSerial + "\\";

            string buildFileName = "";
            if(fileType != SystemFolderWatch.FileType.DISK)
                buildFileName = jobPoolPath + "jobauto_" + systemSerial + "_" + DateTime.Now.Ticks;
            else
                buildFileName = jobPoolPath + "jobdisk_" + systemSerial + "_" + DateTime.Now.Ticks;
            
            string fileNameTxt = buildFileName + ".txt";
            string fileName101 = buildFileName + ".101";

            using (var writer = new StreamWriter(fileName101)) {
                writer.Write(message);
                
            }

            using (var writer = new StreamWriter(fileNameTxt)) {
                writer.Write(message);
                
            }
            var fileList = new List<string> {
                fileName101,
                fileNameTxt
            };
            return fileList;
        }

        public string GetFTPServer() {
            var orderedList = ConnectionString.FTPServers.OrderBy(x => x.Value).ToList();

            return orderedList[0].Key;
        }
    }
}
