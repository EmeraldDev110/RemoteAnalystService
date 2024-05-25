using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Timers;
using RemoteAnalyst.AWS.Infrastructure;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.Enums;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.AWS.Queue;
using RemoteAnalyst.AWS.Queue.View;
using RemoteAnalyst.AWS.EC2;
using Ionic.Zip;
using System.Diagnostics;
using System.Security;
using System.Security.Permissions;
using System.Data;
using log4net;

namespace RemoteAnalyst.UWSRelay.BLL {
    internal class JobUWS {
        
        private static readonly ILog Log = LogManager.GetLogger("RelayLog");
        public void Timer_Elapsed(object source, ElapsedEventArgs e) {
            LoopSystemFolders();
            LoopSystemOrderFolders();
        }

		public void LoopSystemFolders()
        {
            string systemLocation = ConnectionString.FTPSystemLocation;
            var folders = new DirectoryInfo(systemLocation);
            foreach (DirectoryInfo folder in folders.GetDirectories()) {
                var uwsDirectory = new DirectoryInfo(folder.FullName);
                //if (folder.Name == "The Home Deport") {
                //    var subDirectory = uwsDirectory.GetDirectories("Systems");
                //    if (subDirectory.Length > 0) {
                //        var uwsSubDirectory = new DirectoryInfo(subDirectory[0].FullName);
                //        TriggerHomeDepot(uwsSubDirectory);
                //    }
                //}
                //else {
                    //LOOK FOR ALL FILES WITHIN FOLDER(S)
                    var subDirectory = uwsDirectory.GetDirectories("System*");
                    if (subDirectory.Length > 0) {
                        var uwsSubDirectory = new DirectoryInfo(subDirectory[0].FullName);
                        foreach (var subFolder in uwsSubDirectory.GetDirectories()) {
                            uwsDirectory = new DirectoryInfo(subFolder.FullName);
                            TriggerFiles(subFolder, uwsDirectory);
                        }
                    }
                    else {
                        TriggerFiles(folder, uwsDirectory);
                    }
                //}
            }
        }

        private void ProcessQNM(string systemSerial, FileInfo uwSfile, StreamWriter writer) {
            var fileReceivedTime = DateTime.Now;

            //VERIFY EXISTANCE OF "UploadFolder" FOLDER
            var uwsFileDirectory = uwSfile.Directory.FullName.Trim();
            if (!Directory.Exists(uwsFileDirectory + "\\UploadFolder")) {
                Directory.CreateDirectory(uwsFileDirectory + "\\UploadFolder");
            }

            //MOVE FILE TO "UploadFolder" FOLDER 
            var newFileName = uwSfile.Name;
            if (uwSfile.Extension != ".180")
                newFileName += ".180";

            if (!uwSfile.Name.StartsWith("Q"))
                newFileName = "Q" + newFileName;

            var uwsFilePath = uwsFileDirectory + "\\UploadFolder\\" + newFileName;
            File.Move(uwSfile.FullName.Trim(), uwsFilePath);

            //UPLOAD FILE.
            string s3Bucket = "";
            if (!string.IsNullOrEmpty(ConnectionString.S3FTP))
                s3Bucket = ConnectionString.S3FTP;
            var uploadFile = new UploadFile(s3Bucket);
            string fileResult = uploadFile.Upload(uwSfile.Name.Trim(), systemSerial, uwsFilePath, "QNM", Log);

            Log.Info("Finish uploading QNM file");
            
            var loadingInfoService = new LoadingInfoService(ConnectionString.ConnectionStringDB);
            var uwsId = loadingInfoService.GetMaxUWSIDFor();
            loadingInfoService.UpdateUWSRelayTimeFor(systemSerial, uwsId, fileReceivedTime, DateTime.Now, uwSfile.Name.Trim(), uwSfile.Length, (int)BusinessLogic.Enums.FileType.Type.QNM);

            string sqsQueue = "";
            if (!string.IsNullOrEmpty(ConnectionString.SQSLoad))
                sqsQueue = ConnectionString.SQSLoad;
            var writeMessages = new WriteMessges(sqsQueue);
            if (fileResult == "File Deleted") {
                Log.Info("File does not send to S3");
                
            } else {                
                if (ConnectionString.IsLocalAnalyst) {
                    writeMessages.write("QNM", systemSerial, uwSfile.Name.Trim(), uwsId);
                }
                else {
                    writeMessages.SubmitSnsCall("QNM", systemSerial, uwSfile.Name.Trim(), uwsId);
                }
            }
        }

        private void ProcessCLIM(string systemSerial, FileInfo uwSfile, StreamWriter writer) {

            var fileReceivedTime = DateTime.Now;

            //VERIFY EXISTANCE OF "UploadFolder" FOLDER
            var uwsFileDirectory = uwSfile.Directory.FullName.Trim();
            if (!Directory.Exists(uwsFileDirectory + "\\UploadFolder")) {
                Directory.CreateDirectory(uwsFileDirectory + "\\UploadFolder");
            }

            //MOVE FILE TO "UploadFolder" FOLDER
            var newFileName = uwSfile.Name;
            if (uwSfile.Extension != ".180")
                newFileName += ".180";

            if (!uwSfile.Name.StartsWith("Q"))
                newFileName = "CO" + newFileName;
            var uwsFilePath = uwsFileDirectory + "\\UploadFolder\\" + newFileName;
            File.Move(uwSfile.FullName.Trim(), uwsFilePath);

            //UPLOAD FILE.
            string s3Bucket = "";
            if (!string.IsNullOrEmpty(ConnectionString.S3FTP))
                s3Bucket = ConnectionString.S3FTP;
            var uploadFile = new UploadFile(s3Bucket);
            string fileStatus = uploadFile.Upload(uwSfile.Name.Trim(), systemSerial, uwsFilePath, "QNMCLIM", Log);

            Log.Info("Finish uploading QNMCLIM file");
            
            var loadingInfoService = new LoadingInfoService(ConnectionString.ConnectionStringDB);
            var uwsId = loadingInfoService.GetMaxUWSIDFor();
            loadingInfoService.UpdateUWSRelayTimeFor(systemSerial, uwsId, fileReceivedTime, DateTime.Now, uwSfile.Name.Trim(), uwSfile.Length, (int)BusinessLogic.Enums.FileType.Type.QNM);
            
            string sqsQueue = "";
            if (!string.IsNullOrEmpty(ConnectionString.SQSLoad))
                sqsQueue = ConnectionString.SQSLoad;
            var writeMessages = new WriteMessges(sqsQueue);
            if (fileStatus == "File Deleted") {
                Log.Info("File does not send to S3");
            } else {
                if (ConnectionString.IsLocalAnalyst) { 
                    writeMessages.write("QNMCLIM", systemSerial, uwSfile.Name.Trim(), uwsId);
                }
                else { 
                    writeMessages.SubmitSnsCall("QNMCLIM", systemSerial, uwSfile.Name.Trim(), uwsId);
                }
            }            

        }

        private void ProcessUWSSystem(string systemSerial, FileInfo uwSfile, StreamWriter writer) {

            var fileReceivedTime = DateTime.Now;
            int uploadID = 0;
            //VERIFY EXISTANCE OF "UploadFolder" FOLDER
            var uwsFileDirectory = uwSfile.Directory.FullName.Trim();
            if (!Directory.Exists(uwsFileDirectory + "\\UploadFolder")) {
                Directory.CreateDirectory(uwsFileDirectory + "\\UploadFolder");
            }

            //MOVE FILE TO "UploadFolder" FOLDER
            var newFileName = uwSfile.Name;
            if (uwSfile.Extension != ".402")
                newFileName += ".402";
            if(!uwSfile.Name.StartsWith("UMM"))
                newFileName = "UMMV02_" + newFileName;

            var uwsFilePath = uwsFileDirectory + "\\UploadFolder\\" + newFileName;
            File.Move(uwSfile.FullName.Trim(), uwsFilePath);
            Log.InfoFormat("Start uploading UWS file {0}, {1}, {2}", 
                uwSfile.Name, systemSerial, uwsFilePath);

            //Get System Serial, Start Time and Stop Time.
            var collectionType = new UWSFileInfo();
            var uwsVersion = collectionType.UwsFileVersionNewUWSRelay(uwsFilePath);

            var collInfoStartTimestamp = DateTime.MinValue;
            var collInfoEndTimestamp = DateTime.MinValue;

            using (var stream = new FileStream(uwsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                using (var reader = new BinaryReader(stream)) {
                    var myEncoding = new ASCIIEncoding();
                    if (uwsVersion == UWS.Types.Version2013) {
                        int byteLocation = 0;
                        byteLocation += 20;
                        byteLocation += 8;
                        byteLocation += 20;
                        byteLocation += 36;
                        byteLocation += 50;
                        byteLocation += 4;
                        byteLocation += 4;
                        byteLocation += 36;
                        byteLocation += 10;
                        byteLocation += 10;
                        byteLocation += 36;
                        byteLocation += 10;
                        byteLocation += 10;

                        //H-Coll-Info-Start-Timestamp (20)
                        reader.BaseStream.Seek(byteLocation, SeekOrigin.Begin);
                        var newCollInfoStartTimestamp = reader.ReadBytes(20);
                        var uwsCollInfoStartTimestamp = Helper.RemoveNULL(myEncoding.GetString(newCollInfoStartTimestamp).Trim());
                        collInfoStartTimestamp = Convert.ToDateTime(uwsCollInfoStartTimestamp);
                        byteLocation += 20;

                        //H-Coll-Info-End-Timestamp (20)
                        reader.BaseStream.Seek(byteLocation, SeekOrigin.Begin);
                        var newCollInfoEndTimestamp = reader.ReadBytes(20);
                        var uwsCollInfoEndTimestamp = Helper.RemoveNULL(myEncoding.GetString(newCollInfoEndTimestamp).Trim());
                        collInfoEndTimestamp = Convert.ToDateTime(uwsCollInfoEndTimestamp);
                    }

                }
            }

            //UPLOAD FILE.
            string s3Bucket = "";
            if (!string.IsNullOrEmpty(ConnectionString.S3FTP))
                s3Bucket = ConnectionString.S3FTP;
            var uploadFile = new UploadFile(s3Bucket);
            string fileStatus = uploadFile.Upload(uwSfile.Name.Trim(), systemSerial, uwsFilePath, "SYSTEM", Log, uploadID);

            var loadingInfoService = new LoadingInfoService(ConnectionString.ConnectionStringDB);
            var uwsId = loadingInfoService.GetMaxUWSIDFor();

            Log.InfoFormat("uwsId: {0}", uwsId);
            
            if (collInfoStartTimestamp != DateTime.MinValue && collInfoEndTimestamp != DateTime.MinValue) {
                loadingInfoService.UpdateUWSRelayTimeFor(systemSerial, uwsId, fileReceivedTime, DateTime.Now, collInfoStartTimestamp, collInfoEndTimestamp, uwSfile.Name.Trim(), uwSfile.Length);
            }
            else {
                loadingInfoService.UpdateUWSRelayTimeFor(systemSerial, uwsId, fileReceivedTime, DateTime.Now, uwSfile.Name.Trim(), uwSfile.Length, (int)BusinessLogic.Enums.FileType.Type.System);
            }

            string sqsQueue = "";
            if (!string.IsNullOrEmpty(ConnectionString.SQSLoad))
                sqsQueue = ConnectionString.SQSLoad;
            var writeMessages = new WriteMessges(sqsQueue);
            if (fileStatus == "File Deleted") {
                Log.Info("File does not send to S3");
                
            } else {
                writeMessages.write("SYSTEM", systemSerial, uwSfile.Name.Trim(), uwsId, uploadID);
            }
        }

        private void ProcessUWSPathway(string systemSerial, FileInfo uwSfile, StreamWriter writer) {
            var fileReceivedTime = DateTime.Now;

            //VERIFY EXISTANCE OF "UploadFolder" FOLDER
            var uwsFileDirectory = uwSfile.Directory.FullName.Trim();
            if (!Directory.Exists(uwsFileDirectory + "\\UploadFolder")) {
                Directory.CreateDirectory(uwsFileDirectory + "\\UploadFolder");
            }

            //MOVE FILE TO "UploadFolder" FOLDER//MOVE FILE TO "UploadFolder" FOLDER
            var newFileName = uwSfile.Name;
            if (uwSfile.Extension != ".402")
                newFileName += ".402";

            if (!uwSfile.Name.StartsWith("UMM"))
                newFileName = "UMP_" + newFileName;

            var uwsFilePath = uwsFileDirectory + "\\UploadFolder\\" + newFileName;
            File.Move(uwSfile.FullName.Trim(), uwsFilePath);

            //UPLOAD FILE.
            string s3Bucket = "";
            if (!string.IsNullOrEmpty(ConnectionString.S3FTP))
                s3Bucket = ConnectionString.S3FTP;
            var uploadFile = new UploadFile(s3Bucket);
            string fileStatus = uploadFile.Upload(uwSfile.Name.Trim(), systemSerial, uwsFilePath, "PATHWAY", Log);

            Log.Info("Finish uploading PATHWAY file");
            
            var loadingInfoService = new LoadingInfoService(ConnectionString.ConnectionStringDB);
            var uwsId = loadingInfoService.GetMaxUWSIDFor();
            
            string sqsQueue = "";
            if (!string.IsNullOrEmpty(ConnectionString.SQSLoad))
                sqsQueue = ConnectionString.SQSLoad;
            var writeMessages = new WriteMessges(sqsQueue);
            loadingInfoService.UpdateUWSRelayTimeFor(systemSerial, uwsId, fileReceivedTime, DateTime.Now, uwSfile.Name.Trim(), uwSfile.Length, (int)BusinessLogic.Enums.FileType.Type.Pathway);
            if (fileStatus == "File Deleted") {
                Log.Info("File does not send to S3");
                
            } else {
                writeMessages.write("PATHWAY", systemSerial, uwSfile.Name.Trim(), uwsId);
            }
        }
        private void ProcessOss(string systemSerial, FileInfo uwSfile, StreamWriter writer) {
            var fileReceivedTime = DateTime.Now;

            //VERIFY EXISTANCE OF "UploadFolder" FOLDER
            var uwsFileDirectory = uwSfile.Directory.FullName.Trim();
            if (!Directory.Exists(uwsFileDirectory + "\\UploadFolder")) {
                Directory.CreateDirectory(uwsFileDirectory + "\\UploadFolder");
            }

            //MOVE FILE TO "UploadFolder" FOLDER
            var newFileName = uwSfile.Name;
            if (uwSfile.Extension != ".180")
                newFileName += ".180";

            if (!uwSfile.Name.StartsWith("U"))
                newFileName = "U" + newFileName;

            var uwsFilePath = uwsFileDirectory + "\\UploadFolder\\" + newFileName;
            File.Move(uwSfile.FullName.Trim(), uwsFilePath);

            //UPLOAD FILE.
            string s3Bucket = "";
            if (!string.IsNullOrEmpty(ConnectionString.S3FTP))
                s3Bucket = ConnectionString.S3FTP;
            var uploadFile = new UploadFile(s3Bucket);
            string fileStatus = uploadFile.Upload(uwSfile.Name.Trim(), systemSerial, uwsFilePath, "JOURNAL", Log);

            Log.Info("Finish uploading OSS JOURNAL file");
            
            var loadingInfoService = new LoadingInfoService(ConnectionString.ConnectionStringDB);
            var uwsId = loadingInfoService.GetMaxUWSIDFor();
            loadingInfoService.UpdateUWSRelayTimeFor(systemSerial, uwsId, fileReceivedTime, DateTime.Now, uwSfile.Name.Trim(), uwSfile.Length, (int)BusinessLogic.Enums.FileType.Type.OSS);

            string sqsQueue = "";
            if (!string.IsNullOrEmpty(ConnectionString.SQSLoad))
                sqsQueue = ConnectionString.SQSLoad;
            var writeMessages = new WriteMessges(sqsQueue);
            if (fileStatus == "File Deleted") {
                Log.Info("File does not send to S3");
                
            } else {
                if (ConnectionString.IsLocalAnalyst)
                {
                    writeMessages.write("JOURNAL", systemSerial, uwSfile.Name.Trim(), uwsId);
                }
                else
                {
                    writeMessages.SubmitSnsCall("JOURNAL", systemSerial, uwSfile.Name.Trim(), uwsId);
                }
            }
        }

        private void ProcessStorage(string systemSerial, FileInfo uwSfile, StreamWriter writer) {
            var fileReceivedTime = DateTime.Now;

            //VERIFY EXISTANCE OF "UploadFolder" FOLDER
            var uwsFileDirectory = uwSfile.Directory.FullName.Trim();
            if (!Directory.Exists(uwsFileDirectory + "\\UploadFolder")) {
                Directory.CreateDirectory(uwsFileDirectory + "\\UploadFolder");
            }

            //MOVE FILE TO "UploadFolder" FOLDER
            var uwsFilePath = uwsFileDirectory + "\\UploadFolder\\" + uwSfile.Name.Trim();
            File.Move(uwSfile.FullName.Trim(), uwsFilePath);

            //UPLOAD FILE.
            string s3Bucket = "";
            if (!string.IsNullOrEmpty(ConnectionString.S3FTP))
                s3Bucket = ConnectionString.S3FTP;
            var uploadFile = new UploadFile(s3Bucket);
            string fileStatus = uploadFile.Upload(uwSfile.Name.Trim(), systemSerial, uwsFilePath, "DISK", Log);

            Log.Info("Finish uploading DISK file");
            
            var loadingInfoService = new LoadingInfoService(ConnectionString.ConnectionStringDB);
            var uwsId = loadingInfoService.GetMaxUWSIDFor();
            loadingInfoService.UpdateUWSRelayTimeFor(systemSerial, uwsId, fileReceivedTime, DateTime.Now, uwSfile.Name.Trim(), uwSfile.Length, (int)BusinessLogic.Enums.FileType.Type.Storage);

            string sqsQueue = "";
            if (!string.IsNullOrEmpty(ConnectionString.SQSLoad))
                sqsQueue = ConnectionString.SQSLoad;
            var writeMessages = new WriteMessges(sqsQueue);
            if (fileStatus == "File Deleted") {
                Log.Info("File does not send to S3");
                
            } else {
                if (ConnectionString.IsLocalAnalyst)
                {
                    writeMessages.write("DISK", systemSerial, uwSfile.Name.Trim(), uwsId);
                }
                else
                {
                    writeMessages.SubmitSnsCall("DISK", systemSerial, uwSfile.Name.Trim(), uwsId);
                }
            }
        }
        private void MoveToUnknownFolder(FileInfo fi, DirectoryInfo uwsDirectory) {
            var uwsFileDirectory = uwsDirectory.FullName.Trim();
            if (!Directory.Exists(uwsFileDirectory + "\\unknown")) {
                Directory.CreateDirectory(uwsFileDirectory + "\\unknown");
            }
            var uwsFilePath = uwsFileDirectory + "\\unknown\\" + fi.Name.Trim();

            //IF FILE EXISTS. DELETE FILE FROM "unknown" FOLDER
            if (File.Exists(uwsFilePath)) {
                File.Delete(uwsFilePath);
            }

            //MOVE FILE TO "unknown" FOLDER
            File.Move(fi.FullName, uwsFilePath);
        }

        private bool FolderHasAReadPermission(DirectoryInfo folder)
        {
            var permissionSet = new PermissionSet(PermissionState.None);
            var writePermission = new FileIOPermission(FileIOPermissionAccess.Read, folder.FullName);
            permissionSet.AddPermission(writePermission);
            return (permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet));
        }

        private void TriggerFiles(DirectoryInfo folder, DirectoryInfo uwsDirectory) {
            try
            {
                if(!FolderHasAReadPermission(folder))
                {
                    return;
                }
                foreach (FileInfo uwsfile in uwsDirectory.GetFiles())
                {
                    try
                    {
                        if(!File.Exists(uwsfile.FullName)) {
                            continue;
                        }
                        var helper = new Helper();
                        bool fileInUse = helper.IsFileinUse(uwsfile);
                        if (fileInUse) {
                            continue;
                        }
                        //CHECK IF FILE SIZE IS INCREASING
                        bool fileSizeIncreased = helper.IsFileSizeIncreasing(uwsfile);
                        if (fileSizeIncreased) {
                            continue;
                        }
                        String line = null;
                        using (StreamReader reader = new StreamReader(uwsfile.FullName))
                        {
                            line = reader.ReadLine();
                        }
                        if (line == null)
                        {
                            //Do nothing
                        }
                        else if (line.StartsWith("PK"))
                        {
                            HandleFileUnzip(uwsfile, uwsDirectory);
                        }
                        else
                        {
                            HandleTriggerUWSFile(uwsfile, folder);
                        }
                    }
                    catch (Exception ex)
                    {
                        //It is possible the file maybe deleted by another thread. Hence catching the exception
                        //by not doing anything and moving on to the next file
                    }
                }
            }
            catch {
                //Skipping logging since this would be if the folder permission is not present
            }
        }

        private void HandleFileUnzip(FileInfo uwsfile, DirectoryInfo uwsDirectory) {
            var success = UnzipFiles(uwsfile.FullName, uwsDirectory.FullName);
            if (success && File.Exists(uwsfile.FullName)) {
                File.Delete(uwsfile.FullName);
            }
        }

        private void HandleTriggerUWSFile(FileInfo uwSfile, DirectoryInfo folder) {
            var helper = new Helper();
            if (File.Exists(uwSfile.FullName)) // IF FILE DOESN'T EXIST, ANOTHER THREAD ALREADY PROCESS IT.
            {
                //CHECK IF FILE IS IN USE
                bool fileInUse = helper.IsFileinUse(uwSfile);
                if (!fileInUse)
                {
                    //CHECK IF FILE SIZE IS INCREASING
                    bool fileSizeIncreased = helper.IsFileSizeIncreasing(uwSfile);
                    if (!fileSizeIncreased)
                    {
                        //SEND FILE TO S3.
                        var archiveThread = new Thread(() => SendUWSFile(folder.Name, uwSfile));
                        archiveThread.IsBackground = true;
                        archiveThread.Start();
                    }
                }
            }
        }

        internal bool UnzipFiles(string zipFileName, string destinationPath) {
            try {
                using (ZipFile zip = ZipFile.Read(zipFileName)) {
                    foreach (ZipEntry e in zip) {
                        e.Extract(destinationPath);
                    }
                }
                return true;
            } catch {
                return false;
            }
        }

        //This is for NTS
        public void LoopSystemOrderFolders() {
            string systemLocation = ConnectionString.FTPSystemLocation;
            var helper = new Helper();
            var folders = new DirectoryInfo(systemLocation);
            try { 
                foreach (DirectoryInfo dir in folders.GetDirectories()) {
                    string orderFolderPath = dir.FullName + "\\Orders\\";

                    if (!Directory.Exists(orderFolderPath))
                        Directory.CreateDirectory(orderFolderPath);

                    var orderDirInfo = new DirectoryInfo(orderFolderPath);

                    foreach (DirectoryInfo orderDir in orderDirInfo.GetDirectories()) {
                        //Each order ID folder
                        string orderIDPath = orderDir.FullName;
                        var orderIDDirInfo = new DirectoryInfo(orderIDPath);

                        foreach (FileInfo uwsfile in orderIDDirInfo.GetFiles()) {
                            if (!File.Exists(uwsfile.FullName))
                            {
                                continue;
                            }
                            bool fileInUse = helper.IsFileinUse(uwsfile);
                            if (fileInUse)
                            {
                                continue;
                            }
                            //CHECK IF FILE SIZE IS INCREASING
                            bool fileSizeIncreased = helper.IsFileSizeIncreasing(uwsfile);
                            if (fileSizeIncreased)
                            {
                                continue;
                            }
                            String line = null;
                            using (StreamReader reader = new StreamReader(uwsfile.FullName))
                            {
                                line = reader.ReadLine();
                            }
                            if (line == null)
                            {
                                //Do nothing
                            }
                            else if (line.StartsWith("PK"))
                            {
                                HandleFileUnzip(uwsfile, orderIDDirInfo);
                            }
                            else
                            {
                                HandleUWSFile(uwsfile, dir, orderDir);
                            }
                        }
                    }
                }
            }
            catch
            {
                //safety net. incase there is an exception on any single file
            }
        }


        private void HandleUWSFile(FileInfo uwSfile, DirectoryInfo dir, DirectoryInfo orderDir) {
            var helper = new Helper();
            if (File.Exists(uwSfile.FullName)) // IF FILE DOESN'T EXIST, ANOTHER THREAD ALREADY PROCESS IT.
            {
                Log.InfoFormat("uwSfile.FullName: {0}", uwSfile.FullName);
                
                //CHECK IF FILE IS IN USE
                bool fileInUse = helper.IsFileinUse(uwSfile);
                Log.InfoFormat("uwSfile: {0}", uwSfile);
                Log.InfoFormat("fileInUse: {0}", fileInUse);
                

                if (!fileInUse) {
                    //CHECK IF FILE SIZE IS INCREASING
                    bool fileSizeIncreased = helper.IsFileSizeIncreasing(uwSfile);
                    Log.InfoFormat("fileSizeIncreased: {0}", fileSizeIncreased);
                    

                    if (!fileSizeIncreased) {
                        Log.Info("SEND FILE TO S3");
                        
                        //SEND FILE TO S3.
                        var archiveThread = new Thread(() => SendUWSFile(dir.Name, uwSfile, Convert.ToInt32(orderDir.Name)));
                        archiveThread.IsBackground = true;
                        archiveThread.Start();
                        //FOR TESTING PURPOSES COMMENT OUT THREAD FUNCTION CALL - SendUWSFile(folder.Name, UWSfile)) (ABOVE);
                        //SendUWSFile(folder.Name, UWSfile);
                    }
                }
            }
        }

        internal void SendUWSFile(string systemSerial, FileInfo uwSfile, int uploadID = 0) {
            //Create a Log.
            string s3Bucket = "";
            if (!string.IsNullOrEmpty(ConnectionString.S3FTP))
                s3Bucket = ConnectionString.S3FTP;
            string sqsQueue = "";
            if (!string.IsNullOrEmpty(ConnectionString.SQSLoad))
                sqsQueue = ConnectionString.SQSLoad;
            string uwsFileDirectory;
            string uwsFilePath;
            Log.Info("***************************************************************");
            Log.Info("Get job UWSfile");
            
            if (!File.Exists(uwSfile.FullName)) // IF FILE DOESN'T EXIST, ANOTHER THREAD ALREADY PROCESS IT.
            {
                Log.InfoFormat("The File doesn't exists. File Name: {0}", uwSfile.FullName);
                return;
            }

            try {
                Log.InfoFormat("ConnectionString.ConnectionStringDB: {0}", FTPFile.RemovePassword(ConnectionString.ConnectionStringDB));
                
                var fileReceivedTime = DateTime.Now;
                var loadingInfoService = new LoadingInfoService(ConnectionString.ConnectionStringDB);
                var uploadFile = new UploadFile(s3Bucket);
                var writeMessages = new WriteMessges(sqsQueue);

                //IF FILE SIZE IS EQUAL TO ZERO. DO NOT UPLOAD FILE JUST MOVE IT TO "unknown" FOLDER
                var f = new FileInfo(uwSfile.FullName);
                long currentSize = f.Length;

                if (currentSize <= 0) {
                    //WARNING FILE SIZE IS EQUAL TO ZERO
                    Log.InfoFormat("WARNING size for file {0} is zero", uwSfile.FullName.Trim());

                    //VERIFY EXISTANCE OF "unknown" FOLDER
                    uwsFileDirectory = uwSfile.Directory.FullName.Trim();
                    if (!Directory.Exists(uwsFileDirectory + "\\unknown")) {
                        Directory.CreateDirectory(uwsFileDirectory + "\\unknown");
                    }
                    uwsFilePath = uwsFileDirectory + "\\unknown\\" + uwSfile.Name.Trim();

                    //IF FILE EXISTS. DELETE FILE FROM "unknown" FOLDER
                    if (!ConnectionString.IsProcessDirectlySystem) {
                        if (File.Exists(uwsFilePath)) {
                            File.Delete(uwsFilePath);
                        }
                    }

                    //MOVE FILE TO "unknown" FOLDER
                    File.Move(uwSfile.FullName.Trim(), uwsFilePath);
                }
                else if (uwSfile.Extension.Equals("") && ConnectionString.IsProcessDirectlySystem) {
                    //Check if the file is QNM file.
                    var isQNM = false;
                    var isCLIM = false;
                    if (uwSfile.Name.ToUpper().StartsWith("Q") || uwSfile.Name.ToUpper().StartsWith("CO")) {
                        //Read the first line and check to see if it has word "QNM"
                        using (var reader = new StreamReader(uwSfile.FullName)) {
                            var firstLine = reader.ReadLine();
                            if (firstLine.Contains("QNM")) {
                                isQNM = true;
                            }
                            else if (firstLine.Contains("CLIM")) {
                                isCLIM = true;
                            }
                        }
                    }

                    if (!isQNM && !isCLIM) {
                        //Since we are moving the VISA measure file checker to JobTask, we need to comment this out.
                        /*//VERIFY EXISTANCE OF "UploadFolder" FOLDER
                        uwsFileDirectory = uwSfile.Directory.FullName.Trim();
                        if (!Directory.Exists(uwsFileDirectory + "\\UploadFolder")) {
                            Directory.CreateDirectory(uwsFileDirectory + "\\UploadFolder");
                        }

                        //Check for duplicated file.
                        if (File.Exists(uwsFileDirectory + "\\UploadFolder\\" + uwSfile.Name.Trim()))
                            File.Delete(uwsFileDirectory + "\\UploadFolder\\" + uwSfile.Name.Trim());

                        //MOVE FILE TO "UploadFolder" FOLDER
                        uwsFilePath = uwsFileDirectory + "\\UploadFolder\\" + uwSfile.Name.Trim();
                        File.Move(uwSfile.FullName.Trim(), uwsFilePath);

                        var fileInfo = new FileInfo(uwsFilePath);
                        var jobMeasure = new JobMeasures();
                        jobMeasure.SendMeasureFile(systemSerial, fileInfo);*/
                    }
                    else if (!isQNM) {
                        #region QNM
                        //VERIFY EXISTANCE OF "UploadFolder" FOLDER
                        uwsFileDirectory = uwSfile.Directory.FullName.Trim();
                        if (!Directory.Exists(uwsFileDirectory + "\\UploadFolder")) {
                            Directory.CreateDirectory(uwsFileDirectory + "\\UploadFolder");
                        }

                        //MOVE FILE TO "UploadFolder" FOLDER
                        uwsFilePath = uwsFileDirectory + "\\UploadFolder\\" + uwSfile.Name.Trim();
                        File.Move(uwSfile.FullName.Trim(), uwsFilePath);

                        //UPLOAD FILE.
                        string fileStatus = uploadFile.Upload(uwSfile.Name.Trim(), systemSerial, uwsFilePath, "QNM", Log);
                        
                        Log.Info("Finish uploading QNM file");
                        
                        var uwsId = loadingInfoService.GetMaxUWSIDFor();
                        loadingInfoService.UpdateUWSRelayTimeFor(systemSerial, uwsId, fileReceivedTime, DateTime.Now, uwSfile.Name.Trim(), currentSize, (int)BusinessLogic.Enums.FileType.Type.QNM);
                        if (fileStatus == "File Deleted") {
                            Log.Info("File does not send to S3");
                            
                        } else {
                            if (ConnectionString.IsLocalAnalyst) {
                                writeMessages.write("QNM", systemSerial, uwSfile.Name.Trim(), uwsId);
                            }
                            else {
                                writeMessages.SubmitSnsCall("QNM", systemSerial, uwSfile.Name.Trim(), uwsId);
                            }
                        }

                        #endregion
                    }
                    else if (!isCLIM) {
                        #region QNM
                        //VERIFY EXISTANCE OF "UploadFolder" FOLDER
                        uwsFileDirectory = uwSfile.Directory.FullName.Trim();
                        if (!Directory.Exists(uwsFileDirectory + "\\UploadFolder")) {
                            Directory.CreateDirectory(uwsFileDirectory + "\\UploadFolder");
                        }

                        //MOVE FILE TO "UploadFolder" FOLDER
                        uwsFilePath = uwsFileDirectory + "\\UploadFolder\\" + uwSfile.Name.Trim();
                        File.Move(uwSfile.FullName.Trim(), uwsFilePath);

                        //UPLOAD FILE.
                        string fileStatus = uploadFile.Upload(uwSfile.Name.Trim(), systemSerial, uwsFilePath, "QNMCLIM", Log);

                        Log.Info("Finish uploading QNMCLIM file");
                        
                        var uwsId = loadingInfoService.GetMaxUWSIDFor();
                        loadingInfoService.UpdateUWSRelayTimeFor(systemSerial, uwsId, fileReceivedTime, DateTime.Now, uwSfile.Name.Trim(), currentSize, (int)BusinessLogic.Enums.FileType.Type.QNM);
                        if (fileStatus == "File Deleted") {
                            Log.Info("File does not send to S3");
                            
                        } else {
                            if (ConnectionString.IsLocalAnalyst)
                                writeMessages.write("QNMCLIM", systemSerial, uwSfile.Name.Trim(), uwsId);
                            else
                                writeMessages.SubmitSnsCall("QNMCLIM", systemSerial, uwSfile.Name.Trim(), uwsId);
                        }

                        #endregion
                    }
                }
                /*else if (uwSfile.Name.Trim().ToUpper().StartsWith("UMM") && uwSfile.Extension.Equals(".402")) {
                    #region New UMM File Format.

                    #endregion
                }*/
                else if (uwSfile.Name.Trim().ToUpper().StartsWith("U") && !uwSfile.Name.Trim().ToUpper().Contains("UMP") && uwSfile.Extension.Equals(".402")) {//RA & DO fileName.StartsWith("U") && !fileName.Contains("UMP") && fileName.EndsWith("402")
                    #region UWS File
                    //VERIFY EXISTANCE OF "UploadFolder" FOLDER
                    uwsFileDirectory = uwSfile.Directory.FullName.Trim();
                    if (!Directory.Exists(uwsFileDirectory + "\\UploadFolder")) {
                        Directory.CreateDirectory(uwsFileDirectory + "\\UploadFolder");
                    }

                    //MOVE FILE TO "UploadFolder" FOLDER
                    uwsFilePath = uwsFileDirectory + "\\UploadFolder\\" + uwSfile.Name.Trim();
                    File.Move(uwSfile.FullName.Trim(), uwsFilePath);
                    Log.Info("Upload uws");
                    Log.Info("Start uploading UWS file");
                    Log.InfoFormat("uwSfile.Name: {0}", uwSfile.Name);
                    Log.InfoFormat("systemSerial: {0}", systemSerial);
                    Log.InfoFormat("uwsFilePath: {0}", uwsFilePath);
                    
                    
                    //Log.Info("On Queue);
                    //
                    //Get System Serial, Start Time and Stop Time.
                    var collectionType = new UWSFileInfo();
                    var uwsVersion = collectionType.UwsFileVersionNewUWSRelay(uwsFilePath);

                    var collInfoStartTimestamp = DateTime.MinValue;
                    var collInfoEndTimestamp = DateTime.MinValue;

                    using (var stream = new FileStream(uwsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                        using (var reader = new BinaryReader(stream)) {
                            var myEncoding = new ASCIIEncoding();
                            if (uwsVersion == UWS.Types.Version2013) {
                                int byteLocation = 0;
                                byteLocation += 20;
                                byteLocation += 8;
                                byteLocation += 20;
                                byteLocation += 36;
                                byteLocation += 50;
                                byteLocation += 4;
                                byteLocation += 4;
                                byteLocation += 36;
                                byteLocation += 10;
                                byteLocation += 10;
                                byteLocation += 36;
                                byteLocation += 10;
                                byteLocation += 10;

                                //H-Coll-Info-Start-Timestamp (20)
                                reader.BaseStream.Seek(byteLocation, SeekOrigin.Begin);
                                var newCollInfoStartTimestamp = reader.ReadBytes(20);
                                var uwsCollInfoStartTimestamp = Helper.RemoveNULL(myEncoding.GetString(newCollInfoStartTimestamp).Trim());
                                collInfoStartTimestamp = Convert.ToDateTime(uwsCollInfoStartTimestamp);
                                byteLocation += 20;

                                //H-Coll-Info-End-Timestamp (20)
                                reader.BaseStream.Seek(byteLocation, SeekOrigin.Begin);
                                var newCollInfoEndTimestamp = reader.ReadBytes(20);
                                var uwsCollInfoEndTimestamp = Helper.RemoveNULL(myEncoding.GetString(newCollInfoEndTimestamp).Trim());
                                collInfoEndTimestamp = Convert.ToDateTime(uwsCollInfoEndTimestamp);
                            }

                        }
                    }

                    //UPLOAD FILE.
                    string fileStatus = uploadFile.Upload(uwSfile.Name.Trim(), systemSerial, uwsFilePath, "SYSTEM", Log, uploadID);

                    var uwsId = loadingInfoService.GetMaxUWSIDFor();

                    Log.InfoFormat("uwsId: {0}", uwsId);
                    

                    if (collInfoStartTimestamp != DateTime.MinValue && collInfoEndTimestamp != DateTime.MinValue) {
                        loadingInfoService.UpdateUWSRelayTimeFor(systemSerial, uwsId, fileReceivedTime, DateTime.Now, collInfoStartTimestamp, collInfoEndTimestamp, uwSfile.Name.Trim(), currentSize);
                    }
                    else {
                        loadingInfoService.UpdateUWSRelayTimeFor(systemSerial, uwsId, fileReceivedTime, DateTime.Now, uwSfile.Name.Trim(), currentSize, (int)BusinessLogic.Enums.FileType.Type.System);
                    }
                    if (fileStatus == "File Deleted") {
                        Log.Info("File does not send to S3");
                        
                    } else {
                        writeMessages.write("SYSTEM", systemSerial, uwSfile.Name.Trim(), uwsId, uploadID);
                    }
                    #endregion
                }
                else if (uwSfile.Name.Trim().ToUpper().StartsWith("CPUINFO") && uwSfile.Extension.Equals(".101")) {//CPU Info
                    #region CPU INFO
                    //VERIFY EXISTANCE OF "UploadFolder" FOLDER
                    uwsFileDirectory = uwSfile.Directory.FullName.Trim();
                    if (!Directory.Exists(uwsFileDirectory + "\\UploadFolder")) {
                        Directory.CreateDirectory(uwsFileDirectory + "\\UploadFolder");
                    }

                    //MOVE FILE TO "UploadFolder" FOLDER
                    uwsFilePath = uwsFileDirectory + "\\UploadFolder\\" + uwSfile.Name.Trim();
                    File.Move(uwSfile.FullName.Trim(), uwsFilePath);

                    //UPLOAD FILE.
                    string fileStatus = uploadFile.Upload(uwSfile.Name.Trim(), systemSerial, uwsFilePath, "CPUINFO", Log);

                    Log.Info("Finish uploading CUP INFO file");
                    
                    var uwsId = loadingInfoService.GetMaxUWSIDFor();
                    loadingInfoService.UpdateUWSRelayTimeFor(systemSerial, uwsId, fileReceivedTime, DateTime.Now, uwSfile.Name.Trim(), currentSize, (int)BusinessLogic.Enums.FileType.Type.CPUInfo);

                    if (fileStatus == "File Deleted") {
                        Log.Info("File does not send to S3");
                        
                    } else {
                        if (ConnectionString.IsLocalAnalyst)
                            writeMessages.write("CPUINFO", systemSerial, uwSfile.Name.Trim(), uwsId);
                        else
                            writeMessages.SubmitSnsCall("CPUINFO", systemSerial, uwSfile.Name.Trim(), uwsId);
                    }
                    #endregion
                }
                else if (uwSfile.Name.Trim().ToUpper().StartsWith("U") && uwSfile.Extension.Equals(".180")) {//OSS
                    #region OSS
                    //VERIFY EXISTANCE OF "UploadFolder" FOLDER
                    uwsFileDirectory = uwSfile.Directory.FullName.Trim();
                    if (!Directory.Exists(uwsFileDirectory + "\\UploadFolder")) {
                        Directory.CreateDirectory(uwsFileDirectory + "\\UploadFolder");
                    }

                    //MOVE FILE TO "UploadFolder" FOLDER
                    uwsFilePath = uwsFileDirectory + "\\UploadFolder\\" + uwSfile.Name.Trim();
                    File.Move(uwSfile.FullName.Trim(), uwsFilePath);

                    //UPLOAD FILE.
                    string fileStatus = uploadFile.Upload(uwSfile.Name.Trim(), systemSerial, uwsFilePath, "JOURNAL", Log);

                    Log.Info("Finish uploading OSS JOURNAL file");
                    
                    var uwsId = loadingInfoService.GetMaxUWSIDFor();
                    loadingInfoService.UpdateUWSRelayTimeFor(systemSerial, uwsId, fileReceivedTime, DateTime.Now, uwSfile.Name.Trim(), currentSize, (int)BusinessLogic.Enums.FileType.Type.OSS);
                    if (fileStatus == "File Deleted") {
                        Log.Info("File does not send to S3");
                        
                    } else {
                        if (ConnectionString.IsLocalAnalyst)
                            writeMessages.write("JOURNAL", systemSerial, uwSfile.Name.Trim(), uwsId);
                        else
                            writeMessages.SubmitSnsCall("JOURNAL", systemSerial, uwSfile.Name.Trim(), uwsId);
                    }
                    #endregion
                }
                else if (uwSfile.Name.Trim().ToUpper().StartsWith("UMP") && uwSfile.Extension.Equals(".402")) {//Pathway
                    #region Pathway
                    //VERIFY EXISTANCE OF "UploadFolder" FOLDER
                    uwsFileDirectory = uwSfile.Directory.FullName.Trim();
                    if (!Directory.Exists(uwsFileDirectory + "\\UploadFolder")) {
                        Directory.CreateDirectory(uwsFileDirectory + "\\UploadFolder");
                    }

                    //MOVE FILE TO "UploadFolder" FOLDER
                    uwsFilePath = uwsFileDirectory + "\\UploadFolder\\" + uwSfile.Name.Trim();
                    File.Move(uwSfile.FullName.Trim(), uwsFilePath);

                    //UPLOAD FILE.
                    string fileStatus = uploadFile.Upload(uwSfile.Name.Trim(), systemSerial, uwsFilePath, "PATHWAY", Log);

                    Log.Info("Finish uploading PATHWAY file");


                    var uwsId = loadingInfoService.GetMaxUWSIDFor();
                    loadingInfoService.UpdateUWSRelayTimeFor(systemSerial, uwsId, fileReceivedTime, DateTime.Now, uwSfile.Name.Trim(), currentSize, (int)BusinessLogic.Enums.FileType.Type.Pathway);
                    if (fileStatus == "File Deleted") {
                        Log.Info("File does not send to S3");
                        
                    } else {
                        writeMessages.write("PATHWAY", systemSerial, uwSfile.Name.Trim(), uwsId);
                    }
                    #endregion
                }
                else if (uwSfile.Name.Trim().ToUpper().StartsWith("UMD") && uwSfile.Extension.Equals(".101")) {//DISK
                    #region DISK
                    //VERIFY EXISTANCE OF "UploadFolder" FOLDER
                    uwsFileDirectory = uwSfile.Directory.FullName.Trim();
                    if (!Directory.Exists(uwsFileDirectory + "\\UploadFolder")) {
                        Directory.CreateDirectory(uwsFileDirectory + "\\UploadFolder");
                    }

                    //MOVE FILE TO "UploadFolder" FOLDER
                    uwsFilePath = uwsFileDirectory + "\\UploadFolder\\" + uwSfile.Name.Trim();
                    File.Move(uwSfile.FullName.Trim(), uwsFilePath);

                    //UPLOAD FILE.
                    string fileStatus = uploadFile.Upload(uwSfile.Name.Trim(), systemSerial, uwsFilePath, "DISK", Log);

                    Log.Info("Finish uploading DISK file");
                    
                    var uwsId = loadingInfoService.GetMaxUWSIDFor();
                    loadingInfoService.UpdateUWSRelayTimeFor(systemSerial, uwsId, fileReceivedTime, DateTime.Now, uwSfile.Name.Trim(), currentSize, (int)BusinessLogic.Enums.FileType.Type.Storage);
                    if (fileStatus == "File Deleted") {
                        Log.Info("File does not send to S3");
                        
                    } else {
                        if (ConnectionString.IsLocalAnalyst)
                            writeMessages.write("DISK", systemSerial, uwSfile.Name.Trim(), uwsId);
                        else
                            writeMessages.SubmitSnsCall("DISK", systemSerial, uwSfile.Name.Trim(), uwsId);
                    }
                    #endregion
                }
                else if (uwSfile.Name.Trim().ToUpper().StartsWith("Q") && uwSfile.Extension.Equals(".180")) {
                    #region QNM
                    //VERIFY EXISTANCE OF "UploadFolder" FOLDER
                    uwsFileDirectory = uwSfile.Directory.FullName.Trim();
                    if (!Directory.Exists(uwsFileDirectory + "\\UploadFolder")) {
                        Directory.CreateDirectory(uwsFileDirectory + "\\UploadFolder");
                    }

                    //MOVE FILE TO "UploadFolder" FOLDER
                    uwsFilePath = uwsFileDirectory + "\\UploadFolder\\" + uwSfile.Name.Trim();
                    File.Move(uwSfile.FullName.Trim(), uwsFilePath);

                    //UPLOAD FILE.
                    string fileStatus = uploadFile.Upload(uwSfile.Name.Trim(), systemSerial, uwsFilePath, "QNM", Log);

                    Log.Info("Finish uploading QNM file");
                    
                    var uwsId = loadingInfoService.GetMaxUWSIDFor();
                    loadingInfoService.UpdateUWSRelayTimeFor(systemSerial, uwsId, fileReceivedTime, DateTime.Now, uwSfile.Name.Trim(), currentSize, (int)BusinessLogic.Enums.FileType.Type.QNM);
                    if (fileStatus == "File Deleted") {
                        Log.Info("File does not send to S3");
                        
                    } else {
                        if (ConnectionString.IsLocalAnalyst)
                            writeMessages.write("QNM", systemSerial, uwSfile.Name.Trim(), uwsId);
                        else
                            writeMessages.SubmitSnsCall("QNM", systemSerial, uwSfile.Name.Trim(), uwsId);
                    }
                    #endregion
                }
                else if (uwSfile.Name.Trim().ToUpper().StartsWith("CO") && uwSfile.Extension.Equals(".180")) {
                    #region QNM
                    //VERIFY EXISTANCE OF "UploadFolder" FOLDER
                    uwsFileDirectory = uwSfile.Directory.FullName.Trim();
                    if (!Directory.Exists(uwsFileDirectory + "\\UploadFolder")) {
                        Directory.CreateDirectory(uwsFileDirectory + "\\UploadFolder");
                    }

                    //MOVE FILE TO "UploadFolder" FOLDER
                    uwsFilePath = uwsFileDirectory + "\\UploadFolder\\" + uwSfile.Name.Trim();
                    File.Move(uwSfile.FullName.Trim(), uwsFilePath);

                    //UPLOAD FILE.
                    string fileStatus = uploadFile.Upload(uwSfile.Name.Trim(), systemSerial, uwsFilePath, "QNMCLIM", Log);

                    Log.Info("Finish uploading QNMCLIM file");
                    
                    var uwsId = loadingInfoService.GetMaxUWSIDFor();
                    loadingInfoService.UpdateUWSRelayTimeFor(systemSerial, uwsId, fileReceivedTime, DateTime.Now, uwSfile.Name.Trim(), currentSize, (int)BusinessLogic.Enums.FileType.Type.QNM);
                    if (fileStatus == "File Deleted") {
                        Log.Info("File does not send to S3");
                        
                    } else {
                        if (ConnectionString.IsLocalAnalyst)
                            writeMessages.write("QNMCLIM", systemSerial, uwSfile.Name.Trim(), uwsId);
                        else
                            writeMessages.SubmitSnsCall("QNMCLIM", systemSerial, uwSfile.Name.Trim(), uwsId);
                    }
                    #endregion
                }
                else if (uwSfile.Name.Trim().ToUpper().StartsWith("CSVOPF") && uwSfile.Extension.Equals(".180")) {
                    #region EventPro
                    //VERIFY EXISTANCE OF "UploadFolder" FOLDER
                    uwsFileDirectory = uwSfile.Directory.FullName.Trim();
                    if (!Directory.Exists(uwsFileDirectory + "\\UploadFolder")) {
                        Directory.CreateDirectory(uwsFileDirectory + "\\UploadFolder");
                    }

                    //MOVE FILE TO "UploadFolder" FOLDER
                    uwsFilePath = uwsFileDirectory + "\\UploadFolder\\" + uwSfile.Name.Trim();
                    File.Move(uwSfile.FullName.Trim(), uwsFilePath);

                    //UPLOAD FILE.
                    string fileStatus = uploadFile.Upload(uwSfile.Name.Trim(), systemSerial, uwsFilePath, "EVENTPRO", Log);

                    Log.Info("Finish uploading EventPro file");
                    

                    var temp = uwSfile.Name.Split('_');
                    var orderId = Convert.ToInt32(temp[1].Split('.')[0]);
                    if (fileStatus == "File Deleted") {
                        Log.Info("File does not send to S3");
                        
                    } else {
                        writeMessages.WriteToReportQueue("EVENTPRO", orderId, systemSerial, uwSfile.Name.Trim());
                    }
                    #endregion
                }
                else {
                    #region Old format
                    if (uwSfile.Extension.Equals(".402")) {
                        //VERIFY EXISTANCE OF "UploadFolder" FOLDER
                        uwsFileDirectory = uwSfile.Directory.FullName.Trim();
                        if (!Directory.Exists(uwsFileDirectory + "\\UploadFolder")) {
                            Directory.CreateDirectory(uwsFileDirectory + "\\UploadFolder");
                        }

                        //MOVE FILE TO "UploadFolder" FOLDER
                        uwsFilePath = uwsFileDirectory + "\\UploadFolder\\" + uwSfile.Name.Trim();
                        File.Move(uwSfile.FullName.Trim(), uwsFilePath);
                        
                        //Get System Serial, Start Time and Stop Time.
                        var collectionType = new UWSFileInfo();
                        var uwsVersion = collectionType.UwsFileVersionNewUWSRelay(uwsFilePath);

                        var collInfoStartTimestamp = DateTime.MinValue;
                        var collInfoEndTimestamp = DateTime.MinValue;

                        using (var stream = new FileStream(uwsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                            //using (StreamReader reader = new StreamReader(stream))
                            using (var reader = new BinaryReader(stream)) {
                                var myEncoding = new ASCIIEncoding();
                                if (uwsVersion == UWS.Types.Version2013) {
                                    int byteLocation = 0;
                                    byteLocation += 20;
                                    byteLocation += 8;
                                    byteLocation += 20;
                                    byteLocation += 36;
                                    byteLocation += 50;
                                    byteLocation += 4;
                                    byteLocation += 4;
                                    byteLocation += 36;
                                    byteLocation += 10;
                                    byteLocation += 10;
                                    byteLocation += 36;
                                    byteLocation += 10;
                                    byteLocation += 10;

                                    //H-Coll-Info-Start-Timestamp (20)
                                    reader.BaseStream.Seek(byteLocation, SeekOrigin.Begin);
                                    var newCollInfoStartTimestamp = reader.ReadBytes(20);
                                    var uwsCollInfoStartTimestamp = Helper.RemoveNULL(myEncoding.GetString(newCollInfoStartTimestamp).Trim());
                                    collInfoStartTimestamp = Convert.ToDateTime(uwsCollInfoStartTimestamp);
                                    byteLocation += 20;

                                    //H-Coll-Info-End-Timestamp (20)
                                    reader.BaseStream.Seek(byteLocation, SeekOrigin.Begin);
                                    var newCollInfoEndTimestamp = reader.ReadBytes(20);
                                    var uwsCollInfoEndTimestamp = Helper.RemoveNULL(myEncoding.GetString(newCollInfoEndTimestamp).Trim());
                                    collInfoEndTimestamp = Convert.ToDateTime(uwsCollInfoEndTimestamp);
                                }

                            }
                        }


                        //UPLOAD FILE.
                        string fileStatus = uploadFile.Upload(uwSfile.Name.Trim(), systemSerial, uwsFilePath, "SYSTEM", Log);

                        var uwsId = loadingInfoService.GetMaxUWSIDFor();

                        Log.InfoFormat("uwsId: {0}", uwsId);
                        

                        if (collInfoStartTimestamp != DateTime.MinValue && collInfoEndTimestamp != DateTime.MinValue) {
                            loadingInfoService.UpdateUWSRelayTimeFor(systemSerial, uwsId, fileReceivedTime, DateTime.Now, collInfoStartTimestamp, collInfoEndTimestamp, uwSfile.Name.Trim(), currentSize);
                        }
                        else {
                            loadingInfoService.UpdateUWSRelayTimeFor(systemSerial, uwsId, fileReceivedTime, DateTime.Now, uwSfile.Name.Trim(), currentSize, (int)BusinessLogic.Enums.FileType.Type.System);
                        }
                        if (fileStatus == "File Deleted") {
                            Log.Info("File does not send to S3");
                            
                        } else {
                            writeMessages.write("SYSTEM", systemSerial, uwSfile.Name.Trim(), uwsId);
                        }
                    }
                    else if (uwSfile.Name.Trim().ToUpper().StartsWith("CPUINFO") && uwSfile.Extension.Equals(".101")) {
                        //VERIFY EXISTANCE OF "UploadFolder" FOLDER
                        uwsFileDirectory = uwSfile.Directory.FullName.Trim();
                        if (!Directory.Exists(uwsFileDirectory + "\\UploadFolder")) {
                            Directory.CreateDirectory(uwsFileDirectory + "\\UploadFolder");
                        }

                        //MOVE FILE TO "UploadFolder" FOLDER
                        uwsFilePath = uwsFileDirectory + "\\UploadFolder\\" + uwSfile.Name.Trim();
                        File.Move(uwSfile.FullName.Trim(), uwsFilePath);

                        //UPLOAD FILE.
                        string fileStatus = uploadFile.Upload(uwSfile.Name.Trim(), systemSerial, uwsFilePath, "CPUINFO", Log);

                        Log.Info("Finish uploading CUP INFO file");
                        
                        var uwsId = loadingInfoService.GetMaxUWSIDFor();
                        loadingInfoService.UpdateUWSRelayTimeFor(systemSerial, uwsId, fileReceivedTime, DateTime.Now, uwSfile.Name.Trim(), currentSize, (int)BusinessLogic.Enums.FileType.Type.CPUInfo);
                        if (fileStatus == "File Deleted") {
                            Log.Info("File does not send to S3");
                            
                        } else {
                            if (ConnectionString.IsLocalAnalyst)
                                writeMessages.write("CPUINFO", systemSerial, uwSfile.Name.Trim(), uwsId);
                            else
                                writeMessages.SubmitSnsCall("CPUINFO", systemSerial, uwSfile.Name.Trim(), uwsId);
                        }
                    }
                    else if (uwSfile.Name.Trim().ToUpper().StartsWith("RA") && uwSfile.Extension.Equals(".180")) {
                        //VERIFY EXISTANCE OF "UploadFolder" FOLDER
                        uwsFileDirectory = uwSfile.Directory.FullName.Trim();
                        if (!Directory.Exists(uwsFileDirectory + "\\UploadFolder")) {
                            Directory.CreateDirectory(uwsFileDirectory + "\\UploadFolder");
                        }

                        //MOVE FILE TO "UploadFolder" FOLDER
                        uwsFilePath = uwsFileDirectory + "\\UploadFolder\\" + uwSfile.Name.Trim();
                        File.Move(uwSfile.FullName.Trim(), uwsFilePath);

                        //UPLOAD FILE.
                        string fileStatus = uploadFile.Upload(uwSfile.Name.Trim(), systemSerial, uwsFilePath, "JOURNAL", Log);

                        Log.Info("Finish uploading OSS JOURNAL file");
                        
                        var uwsId = loadingInfoService.GetMaxUWSIDFor();
                        loadingInfoService.UpdateUWSRelayTimeFor(systemSerial, uwsId, fileReceivedTime, DateTime.Now, uwSfile.Name.Trim(), currentSize, (int)BusinessLogic.Enums.FileType.Type.OSS);
                        if (fileStatus == "File Deleted") {
                            Log.Info("File does not send to S3");
                            
                        } else {
                            if (ConnectionString.IsLocalAnalyst)
                                writeMessages.write("JOURNAL", systemSerial, uwSfile.Name.Trim(), uwsId);
                            else
                                writeMessages.SubmitSnsCall("JOURNAL", systemSerial, uwSfile.Name.Trim(), uwsId);
                        }
                    }
                    else if (uwSfile.Name.Trim().ToUpper().StartsWith("RPUWS") && uwSfile.Extension.Equals(".180")) {
                        //VERIFY EXISTANCE OF "UploadFolder" FOLDER
                        uwsFileDirectory = uwSfile.Directory.FullName.Trim();
                        if (!Directory.Exists(uwsFileDirectory + "\\UploadFolder")) {
                            Directory.CreateDirectory(uwsFileDirectory + "\\UploadFolder");
                        }

                        //MOVE FILE TO "UploadFolder" FOLDER
                        uwsFilePath = uwsFileDirectory + "\\UploadFolder\\" + uwSfile.Name.Trim();
                        File.Move(uwSfile.FullName.Trim(), uwsFilePath);

                        //UPLOAD FILE.
                        string fileStatus = uploadFile.Upload(uwSfile.Name.Trim(), systemSerial, uwsFilePath, "PATHWAY", Log);

                        Log.Info("Finish uploading PATHWAY file");
                        
                        var uwsId = loadingInfoService.GetMaxUWSIDFor();
                        loadingInfoService.UpdateUWSRelayTimeFor(systemSerial, uwsId, fileReceivedTime, DateTime.Now, uwSfile.Name.Trim(), currentSize, (int)BusinessLogic.Enums.FileType.Type.Pathway);
                        if (fileStatus == "File Deleted") {
                            Log.Info("File does not send to S3");
                            
                        } else {
                            writeMessages.write("PATHWAY", systemSerial, uwSfile.Name.Trim(), uwsId);
                        }
                    }
                    else if (uwSfile.Name.Trim().ToUpper().StartsWith("DK") && uwSfile.Extension.Equals(".101")) {
                        //VERIFY EXISTANCE OF "UploadFolder" FOLDER
                        uwsFileDirectory = uwSfile.Directory.FullName.Trim();
                        if (!Directory.Exists(uwsFileDirectory + "\\UploadFolder")) {
                            Directory.CreateDirectory(uwsFileDirectory + "\\UploadFolder");
                        }

                        //MOVE FILE TO "UploadFolder" FOLDER
                        uwsFilePath = uwsFileDirectory + "\\UploadFolder\\" + uwSfile.Name.Trim();
                        File.Move(uwSfile.FullName.Trim(), uwsFilePath);

                        //UPLOAD FILE.
                        string fileStatus = uploadFile.Upload(uwSfile.Name.Trim(), systemSerial, uwsFilePath, "DISK", Log);

                        Log.Info("Finish uploading DISK file");
                        
                        var uwsId = loadingInfoService.GetMaxUWSIDFor();
                        loadingInfoService.UpdateUWSRelayTimeFor(systemSerial, uwsId, fileReceivedTime, DateTime.Now, uwSfile.Name.Trim(), currentSize, (int)BusinessLogic.Enums.FileType.Type.Storage);

                        if (fileStatus == "File Deleted") {
                            Log.Info("File does not send to S3");
                            
                        } else {
                            if (ConnectionString.IsLocalAnalyst)
                                writeMessages.write("DISK", systemSerial, uwSfile.Name.Trim(), uwsId);
                            else
                                writeMessages.SubmitSnsCall("DISK", systemSerial, uwSfile.Name.Trim(), uwsId);
                        }
                    }
                    #endregion
                    else {
                        string filecode = "175";
                        var nonstopInfo = new NonStopInfoService(ConnectionString.ConnectionStringDB);
                        DataTable nonStopData = nonstopInfo.GetNonStopInfoFor();
                        if (uwSfile.Extension.Equals(".175")) {
                            #region Measure File
                            var systemTable = new System_tblService(ConnectionString.ConnectionStringDB);
                            string systemName = systemTable.GetSystemNameFor(systemSerial).Replace("\\", "");
                            if (nonStopData.Rows.Count > 0) {
                                string ipAddress = nonStopData.Rows[0]["IPAddress"].ToString();
                                string user = nonStopData.Rows[0]["User"].ToString();
                                string volume = nonStopData.Rows[0]["Volume"].ToString();
                                var ftpPort = nonStopData.Rows[0]["FTPPort"].ToString();
                                var monitorPort = nonStopData.Rows[0]["MonitorPort"].ToString();
                                var volumeMeasFH = nonStopData.Rows[0]["VolumeMeasFH"].ToString();
                                var subVolumeMeasFH = nonStopData.Rows[0]["SubVolumeMeasFH"].ToString();

                                string measFH = $"{volumeMeasFH}.{subVolumeMeasFH}.MEAS" + systemTable.GetMeasFHFor(systemSerial);

                                Log.InfoFormat("filecode: {0}", filecode);
                                Log.InfoFormat("systemSerial: {0}", systemSerial);
                                Log.InfoFormat("systemName: {0}", systemName);
                                Log.InfoFormat("measFH: {0}", measFH);
                                

                                //Check for duplicated file.
                                Log.InfoFormat("uwsFileDirectory: {0}", uwSfile.Directory.FullName);
                                
                                uwsFileDirectory = uwSfile.Directory.FullName.Trim();
                                if (!Directory.Exists(uwsFileDirectory + "\\UploadFolder")) {
                                    Directory.CreateDirectory(uwsFileDirectory + "\\UploadFolder");
                                }

                                if (File.Exists(uwsFileDirectory + "\\UploadFolder\\" + uwSfile.Name.Trim()))
                                    File.Delete(uwsFileDirectory + "\\UploadFolder\\" + uwSfile.Name.Trim());

                                //MOVE FILE TO "UploadFolder" FOLDER
                                uwsFilePath = uwsFileDirectory + "\\UploadFolder\\" + uwSfile.Name.Trim();
                                File.Move(uwSfile.FullName.Trim(), uwsFilePath);
                                var fileInfo = new FileInfo(uwsFilePath);

                                //Send data to NonStop.
                                var ftpFile = new FTPFile(systemSerial, systemName, fileInfo.DirectoryName);
                                var message = ftpFile.UploadFile(uwSfile.Name, Log, filecode, measFH);

                                try {
                                    if (message.Length.Equals(0)) {
                                        if (!ConnectionString.IsProcessDirectlySystem) {
                                            if (File.Exists(uwSfile.FullName))
                                                uwSfile.Delete();
                                        }
                                    }
                                } catch (Exception ex) {
                                    Log.ErrorFormat("{0} Delete Error: {1}", uwSfile.Name, ex);   
                                }
                            } else {
                                Log.Info("No NonStop server info found!");                                
                            }
                            #endregion

                        } else {
                            //Check for File Type.
                            using (var reader = new StreamReader(uwSfile.FullName)) {
                                string line = reader.ReadLine();
                                if (line.Contains("PAK")) {
                                    filecode = "1729";
                                }
                            }

                            if (filecode.Equals("1729")) {
                                #region PAK File
                                var systemTable = new System_tblService(ConnectionString.ConnectionStringDB);
                                string systemName = systemTable.GetSystemNameFor(systemSerial).Replace("\\", "");
                                if (nonStopData.Rows.Count > 0) {
                                    string ipAddress = nonStopData.Rows[0]["IPAddress"].ToString();
                                    string user = nonStopData.Rows[0]["User"].ToString();
                                    string volume = nonStopData.Rows[0]["Volume"].ToString();
                                    var ftpPort = nonStopData.Rows[0]["FTPPort"].ToString();
                                    var monitorPort = nonStopData.Rows[0]["MonitorPort"].ToString();
                                    var volumeMeasFH = nonStopData.Rows[0]["VolumeMeasFH"].ToString();
                                    var subVolumeMeasFH = nonStopData.Rows[0]["SubVolumeMeasFH"].ToString();

                                    string measFH = $"{volumeMeasFH}.{subVolumeMeasFH}.MEAS" + systemTable.GetMeasFHFor(systemSerial);

                                    Log.InfoFormat("filecode: {0}", filecode);
                                    Log.InfoFormat("systemSerial: {0}", systemSerial);
                                    Log.InfoFormat("systemName: {0}", systemName);
                                    Log.InfoFormat("measFH: {0}", measFH);
                                    

                                    //Send data to NonStop.
                                    var ftpFile = new FTPFile(systemSerial, systemName, uwSfile.DirectoryName);
                                    var message = ftpFile.UploadFile(uwSfile.Name, Log, filecode, measFH);
                                    try {
                                        if (message.Length.Equals(0)) {
                                            if (!ConnectionString.IsProcessDirectlySystem) {
                                                if (File.Exists(uwSfile.FullName))
                                                    uwSfile.Delete();
                                            }
                                        }
                                    } catch (Exception ex) {
                                        Log.ErrorFormat("{0} Delete Error: {1}", uwSfile.Name, ex);
                                    }
                                } else {
                                    Log.Info("No NonStop server info found!");
                                }
                                #endregion
                            } else {
                                #region Unknown
                                //Error Validating File Name.
                                Log.InfoFormat("ERROR validating file {0} for upload", uwSfile.FullName.Trim());

                                //VERIFY EXISTANCE OF "unknown" FOLDER
                                uwsFileDirectory = uwSfile.Directory.FullName.Trim();
                                if (!Directory.Exists(uwsFileDirectory + "\\unknown")) {
                                    Directory.CreateDirectory(uwsFileDirectory + "\\unknown");
                                }
                                uwsFilePath = uwsFileDirectory + "\\unknown\\" + uwSfile.Name.Trim();

                                //IF FILE EXISTS. DELETE FILE FROM "unknown" FOLDER
                                if (File.Exists(uwsFilePath)) {
                                    File.Delete(uwsFilePath);
                                }

                                //MOVE FILE TO "unknown" FOLDER
                                File.Move(uwSfile.FullName.Trim(), uwsFilePath);

                                if (!ConnectionString.IsLocalAnalyst) {
                                    //SEND ERROR MESSAGE
                                    var amazonOperations = new AmazonOperations();
                                    amazonOperations.WriteErrorQueue("ERROR validating file '" + uwsFilePath + "' for upload");
                                }
                                else {
                                    var email = new EmailToSupport(ConnectionString.AdvisorEmail, ConnectionString.SupportEmail,
                                        ConnectionString.WebSite,
                                        ConnectionString.EmailServer, ConnectionString.EmailPort, ConnectionString.EmailUser,
                                        ConnectionString.EmailPassword, ConnectionString.EmailAuthentication,
                                        ConnectionString.FTPSystemLocation, ConnectionString.ServerPath,
                                        ConnectionString.EmailIsSSL, ConnectionString.IsLocalAnalyst,
                            ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                                    email.SendLocalAnalystErrorMessageEmail("FTP Server - JobUWS.cs", "ERROR validating file '" + uwsFilePath + "' for upload", LicenseService.GetProductName(ConnectionString.ConnectionStringDB));
                                }

                                #endregion 
                            }
                        }
                    }
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("Error {0}", ex);
                
                if (File.Exists(uwSfile.FullName)) // IF FILE DOESN'T EXIST, ANOTHER THREAD ALREADY PROCESS IT.
                {
                    Log.Error("Error Processing in JobUWS::SendUWSFile [File Exists]");
                    Log.ErrorFormat("System Folder: {0}", ConnectionString.FTPSystemLocation);
                    Log.ErrorFormat("FullName: {0}", uwSfile.FullName);
                    Log.ErrorFormat("Error {0}", ex);
                    

                    //VERIFY EXISTANCE OF "unknown" FOLDER
                    uwsFileDirectory = uwSfile.Directory.FullName.Trim();
                    if (!Directory.Exists(uwsFileDirectory + "\\unknown")) {
                        Directory.CreateDirectory(uwsFileDirectory + "\\unknown");
                    }
                    uwsFilePath = uwsFileDirectory + "\\unknown\\" + uwSfile.Name.Trim();

                    //IF FILE EXISTS. DELETE FILE FROM "unknown" FOLDER
                    if (File.Exists(uwsFilePath)) {
                        File.Delete(uwsFilePath);
                    }

                    //MOVE FILE TO "unknown" FOLDER
                    File.Move(uwSfile.FullName.Trim(), uwsFilePath);

                    if (!ConnectionString.IsLocalAnalyst) {
                        //SEND ERROR MESSAGE
                        var amazonOperations = new AmazonOperations();
                        amazonOperations.WriteErrorQueue("Processing UWS File Error: " + ex.Message);
                    }
                    else {
                        var email = new EmailToSupport(ConnectionString.AdvisorEmail, ConnectionString.SupportEmail,
                            ConnectionString.WebSite,
                            ConnectionString.EmailServer, ConnectionString.EmailPort, ConnectionString.EmailUser,
                            ConnectionString.EmailPassword, ConnectionString.EmailAuthentication,
                            ConnectionString.FTPSystemLocation, ConnectionString.ServerPath,
                            ConnectionString.EmailIsSSL, 
                            ConnectionString.IsLocalAnalyst, ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                        email.SendLocalAnalystErrorMessageEmail("FTP Server - JobUWS.cs", ex.Message, LicenseService.GetProductName(ConnectionString.ConnectionStringDB));
                    }
                }
                else {
                    Log.ErrorFormat("Error Processing in JobUWS::SendUWSFile [File does not exist]");
                    Log.ErrorFormat("System Folder: {0}", ConnectionString.FTPSystemLocation);
                    Log.ErrorFormat("FullName: {0}", uwSfile.FullName);
                    Log.ErrorFormat("Error {0}", ex);
                }
            }
        }
    }
}