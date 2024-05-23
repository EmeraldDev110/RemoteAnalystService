using log4net;
using RemoteAnalyst.BusinessLogic.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;

namespace RemoteAnalyst.FTPRelay.BLL {
    internal class SystemFolderWatch {
        private static readonly ILog Log = LogManager.GetLogger("SystemFolderWatch");
        public enum FileType {
            UWS,
            Pathway,
            CPUInfo,
            OSS,
            DISCOPEN,
            DISK,
            None
        }

        private Timer _checkFTPLog;

        public void StartScheduleTimers() {
            StartSystemFolderWatch();
//            StartNTSSystemFolderWatch();
        }

        public void StartSystemFolderWatch() {
            //_checkFTPLog = new Timer(1800000); //30 mins.
            _checkFTPLog = new Timer(30000);
            _checkFTPLog.Elapsed += CheckSystemFolder;
            _checkFTPLog.AutoReset = true;
            _checkFTPLog.Enabled = true;
        }

        public void StartNTSSystemFolderWatch() {
            _checkFTPLog = new Timer(1000); //30 mins.
            _checkFTPLog.Elapsed += CheckNTSSystemFolder;
            _checkFTPLog.AutoReset = false;
            _checkFTPLog.Enabled = true;
        }

        internal void CheckSystemFolder(object source, ElapsedEventArgs e) {
            Log.InfoFormat("SystemFolder {0}", ConnectionString.SystemLocation);
            var dirInfo = new DirectoryInfo(ConnectionString.SystemLocation);

            foreach (DirectoryInfo dir in dirInfo.GetDirectories()) {
                Log.InfoFormat("dir: {0}",dir.FullName);
                

                foreach (FileInfo file in dir.GetFiles()) {
                    Log.InfoFormat("file: {0}",file.FullName);
                    
                    if (file.Exists) {
                        string systemSerial = file.Directory.Name;
                        if (file.LastWriteTime.AddMinutes(ConnectionString.MaxFileWaitTime) <= DateTime.Now &&
                            file.LastWriteTime.Date.Equals(DateTime.Now.Date)) {
                            string fileName = file.Name;
                            string fileFullName = file.FullName;

                            Log.Info("GetFileType");
                            
                            FileType fileType = GetFileType(fileName);
                            Log.InfoFormat("fileType: {0}",fileType);
                            
                            if (fileType != FileType.None) {
                                Log.Info("Calling CheckFiels");
                                

                                //Check for Duplicated File Pre Fix.
                                string[] matchPreFix = Directory.GetFiles(file.DirectoryName, fileName.Substring(0, 4) + "*");

                                bool skip = false;
                                if (matchPreFix.Length > 1) {
                                    skip = CheckFiles(file, matchPreFix);
                                }

                                if (File.Exists(file.FullName) && !skip) {
                                    FTPFile(systemSerial, fileType, fileFullName, file);
                                }
                            }
                        }
                        else if (file.LastWriteTime.Date < DateTime.Now.AddDays(-2).Date) {
                            //Delete older files.
                            //
                            //For testing move the file.
                            try {
                                file.Delete();
                            }
                            catch {
                            }
                        }
                    }
                }
            }
        }

        internal void CheckNTSSystemFolder(object source, ElapsedEventArgs e) {
            string systemFolder = ConnectionString.SystemLocation;
            //            string orderFolder = systemFolder + "\\Orders\\";

            var dirInfo = new DirectoryInfo(systemFolder);

            foreach (DirectoryInfo dir in dirInfo.GetDirectories()) {
                //Get each system folder directory

                string orderFolderPath = dir.FullName + "\\Orders\\";
                var orderDirInfo = new DirectoryInfo(orderFolderPath);

                foreach (DirectoryInfo orderDir in orderDirInfo.GetDirectories()) {
                    //Each order ID folder
                    string orderIDPath = orderDir.FullName;
                    var orderIDDirInfo = new DirectoryInfo(orderIDPath);

                    foreach (FileInfo file in orderIDDirInfo.GetFiles()) {
                        //Files in order ID folder
                        if (file.Exists) {
                            string systemSerial = file.Directory.Name;
                            if (file.LastWriteTime.AddMinutes(ConnectionString.MaxFileWaitTime) <= DateTime.Now &&
                                file.LastWriteTime.Date.Equals(DateTime.Now.Date)) {
                                string fileName = file.Name;
                                string fileFullName = file.FullName;

                                FileType fileType = GetFileType(fileName);
                                if (fileType != FileType.None) {
                                    //Check for Duplicated File Pre Fix.
                                    string[] matchPreFix = Directory.GetFiles(file.DirectoryName, fileName.Substring(0, 4) + "*");

                                    bool skip = false;
                                    if (matchPreFix.Length > 1) {
                                        skip = CheckFiles(file, matchPreFix);
                                    }

                                    if (File.Exists(file.FullName) && !skip) {
                                        FTPFile(systemSerial, fileType, fileFullName, file);
                                    }
                                }
                            }
                            else if (file.LastWriteTime.Date < DateTime.Now.AddDays(-2).Date) {
                                try {
                                    file.Delete();
                                }
                                catch {
                                }
                            }
                        }
                    }
                }

                /*foreach (FileInfo file in dir.GetFiles()) {
                    if (file.Exists) {
                        string systemSerial = file.Directory.Name;
                        if (file.LastWriteTime.AddMinutes(ConnectionString.MaxFileWaitTime) <= DateTime.Now &&
                            file.LastWriteTime.Date.Equals(DateTime.Now.Date)) {
                            string fileName = file.Name;
                            string fileFullName = file.FullName;

                            FileType fileType = GetFileType(fileName);
                            if (fileType != FileType.None) {
                                //Check for Duplicated File Pre Fix.
                                string[] matchPreFix = Directory.GetFiles(file.DirectoryName, fileName.Substring(0, 4) + "*");

                                bool skip = false;
                                if (matchPreFix.Length > 1) {
                                    skip = CheckFiles(file, matchPreFix);
                                }

                                if (File.Exists(file.FullName) && !skip) {
                                    FTPFile(systemSerial, fileType, fileFullName, file);
                                }
                            }
                        }
                        else if (file.LastWriteTime.Date < DateTime.Now.AddDays(-2).Date) {
                            try {
                                file.Delete();
                            }
                            catch {
                            }
                        }
                    }
                }*/
            }
        }

        internal void FTPFile(string systemSerial, FileType fileType, string fileFullName, FileInfo file) {
            #region FTP File
            var uploads = new Uploads();
            List<string> triggerLocation;
            bool success;
            switch (fileType) {
                case FileType.UWS:
                    Log.Info("File Type: UWS");
                    
                    success = uploads.UploadUWS(systemSerial, fileFullName, "SYSTEM", Log);
                    Log.InfoFormat("FTP Returned: {0}",success);
                    
                    if (success) {
                        triggerLocation = uploads.CreateTriggerFile(fileType, systemSerial, file.Name);
                        foreach (string trigger in triggerLocation) {
                            uploads.UploadTrigger(systemSerial, trigger, Log);
                        }
                    }
                    break;
                case FileType.CPUInfo:
                    Log.Info("File Type: CPUInfo");
                    
                    success = uploads.UploadCPUInfo(systemSerial, fileFullName, Log);
                    Log.InfoFormat("FTP Returned: {0}",success);
                    
                    if (success) {
                        triggerLocation = uploads.CreateTriggerFile(fileType, systemSerial, file.Name);
                        foreach (string trigger in triggerLocation) {
                            uploads.UploadTrigger(systemSerial, trigger, Log);
                        }
                    }
                    break;
                case FileType.OSS:
                    Log.Info("File Type: OSS");
                    
                    success = uploads.UploadOSS(systemSerial, fileFullName, Log);
                    Log.InfoFormat("FTP Returned: {0}",success);
                    
                    if (success) {
                        triggerLocation = uploads.CreateTriggerFile(fileType, systemSerial, file.Name);
                        foreach (string trigger in triggerLocation) {
                            uploads.UploadTrigger(systemSerial, trigger, Log);
                        }
                    }
                    break;
                case FileType.DISCOPEN:
                    Log.Info("File Type: DISCOPEN");
                    
                    success = uploads.UploadDiscOpen(systemSerial, fileFullName, Log);
                    Log.InfoFormat("FTP Returned: {0}",success);
                    
                    if (success) {
                        triggerLocation = uploads.CreateTriggerFile(fileType, systemSerial, file.Name);
                        foreach (string trigger in triggerLocation) {
                            uploads.UploadTrigger(systemSerial, trigger, Log);
                        }
                    }
                    break;
                case FileType.DISK:
                    Log.Info("File Type: DISK");
                    
                    success = uploads.UploadDisk(systemSerial, fileFullName, Log);
                    Log.InfoFormat("FTP Returned: {0}",success);
                    
                    if (success) {
                        triggerLocation = uploads.CreateTriggerFile(fileType, systemSerial, file.Name);
                        foreach (string trigger in triggerLocation) {
                            uploads.UploadTrigger(systemSerial, trigger, Log);
                        }
                    }
                    break;
                case FileType.Pathway:
                    Log.Info("File Type: Pathway");
                    
                    success = uploads.UploadPathway(systemSerial, fileFullName, Log);
                    Log.InfoFormat("FTP Returned: {0}",success);
                    
                    if (success) {
                        triggerLocation = uploads.CreateTriggerFile(fileType, systemSerial, file.Name);
                        foreach (string trigger in triggerLocation) {
                            uploads.UploadTrigger(systemSerial, trigger, Log);
                        }
                    }
                    break;
            }

            #endregion
        }

        internal bool CheckFiles(FileInfo file, string[] matchPreFix) {
            #region Check Files with Same name pre fix

            bool skip = false;
            //Check the last modified time.
            foreach (string matchFile in matchPreFix.Where(x => x != file.FullName)) {
                var matchFileInfo = new FileInfo(matchFile);
                if (matchFileInfo.Exists) {
                    //Check Time.
                    if (matchFileInfo.LastWriteTime.AddMinutes(ConnectionString.MaxFileWaitTime) <= DateTime.Now &&
                        matchFileInfo.LastWriteTime.Date.Equals(DateTime.Now.Date)) {
                        //Compare File size. if file is smaller or equal to current file, delte the matching file.
                        if (file.Length >= matchFileInfo.Length) {
                            try {
                                matchFileInfo.Delete();
                            }
                            catch {
                            }
                        }
                        else {
                            try {
                                file.Delete();
                            }
                            catch {
                            }
                        }
                    }
                    else {
                        //New data is coming in skip this file.
                        skip = true;
                        break;
                    }
                }
            }
            return skip;

            #endregion
        }

        internal FileType GetFileType(string fileName) {
            var fileType = FileType.UWS;

            if (fileName.StartsWith("RA") && fileName.EndsWith("402")) {
                fileType = FileType.UWS;
            }
            else if (fileName.StartsWith("CPUINFO") && fileName.EndsWith("101")) {
                fileType = FileType.CPUInfo;
            }
            else if (fileName.StartsWith("RA") && fileName.EndsWith("180")) {
                fileType = FileType.OSS;
            }
            else if (fileName.StartsWith("DO") && fileName.EndsWith("402")) {
                fileType = FileType.DISCOPEN;
            }
            else if (fileName.StartsWith("DK") && fileName.EndsWith("101")) {
                fileType = FileType.DISK;
            }
            else if (fileName.StartsWith("RPUWS") || fileName.StartsWith("ZA")) {
                fileType = FileType.Pathway;
            }
            else
                fileType = FileType.None;

            /*if (fileName.StartsWith("U") && !fileName.Contains("UMP") && fileName.EndsWith("402")) {
                if (fileName.StartsWith("RA")) {
                    fileType = FileType.UWS;
                }
                else if (fileName.StartsWith("DO")) {
                    fileType = FileType.DISCOPEN;
                }
            }
            else if (fileName.StartsWith("U") && fileName.EndsWith("101")) {
                fileType = FileType.OSS;
            }
            else if (fileName.StartsWith("UMD") && fileName.EndsWith("101")) {
                fileType = FileType.DISK;
            }
            else if (fileName.StartsWith("UMP") && fileName.EndsWith("402")) {
                fileType = FileType.Pathway;
            }
            else {
                fileType = FileType.None;
            }*/

            return fileType;
        }
    }
}