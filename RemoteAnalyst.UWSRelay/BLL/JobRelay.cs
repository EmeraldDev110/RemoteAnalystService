using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using log4net;
using RemoteAnalyst.AWS.Infrastructure;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteLoadingProcessor;

namespace RemoteAnalyst.UWSRelay.BLL {
    /// <summary>
    /// JobRelay class create a file system watcher to monitor the job pool folder. 
    /// Where there are job files coming in, JobWatcher will process the job file, upload data file to S3 and write messages to SQS
    /// </summary>
    public class JobRelay {
        private string _s3BucketDev = string.Empty;
        private string _s3BucketProd = string.Empty;
        private string _sqsQueueDev = string.Empty;
        private string _sqsQueueProd = string.Empty;
        private readonly FileSystemWatcher _watcher = new FileSystemWatcher();
        private List<string> _systemListDev;
        private List<string> _systemListProd;
        private static readonly ILog Log = LogManager.GetLogger("RelayLog");

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
            _systemListDev = new List<string>();
            _systemListProd = new List<string>();
        }

        /// <summary>
        /// Initialize the watcher to start the monitoring.
        /// </summary>
        public void StartJobWatch(List<string> systemListDev, List<string> systemListProd, string s3BucketDev, string sqsQueueDev, string s3BucketProd, string sqsQueueProd) {
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

            _systemListDev = systemListDev;
            _systemListProd = systemListProd;
            _s3BucketDev = s3BucketDev;
            _s3BucketProd = s3BucketProd;
            _sqsQueueDev = sqsQueueDev;
            _sqsQueueProd = sqsQueueProd;
        }

        /// <summary>
        /// Process the job file, upload data file to S3 and write messages to SQS
        /// Write the error log and write to error queue when error happens.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void OnCreated(object source, FileSystemEventArgs e) {
            Thread.Sleep(10000);
            //Force all datetime to be in US format.
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            //Create a Log.
            string s3Bucket = string.Empty;
            string sqsQueue = string.Empty;
            var fileReceivedTime = DateTime.Now;

            try {
                Log.Info("***************************************************************");
                Log.Info("Get job file");
                //Insert INTO SampleInfo if it's Auto load.
                string triggerFileName = e.Name;
                string[] split = e.Name.Split(new[] { '_' });

                //Check for system serial;
                if (_systemListDev.Count == 0 && _systemListProd.Count == 0) {
                    if (!string.IsNullOrEmpty(ConnectionString.S3FTP))
                        s3Bucket = ConnectionString.S3FTP;
                    if (!string.IsNullOrEmpty(ConnectionString.SQSLoad)) 
                        sqsQueue = ConnectionString.SQSLoad;
                }
                else {
                    if (!_systemListDev.Contains(split[1]) && !_systemListProd.Contains(split[1])) {
                        return;
                    }
                    else {
                        if (_systemListDev.Contains(split[1])) {
                            s3Bucket = _s3BucketDev;
                            sqsQueue = _sqsQueueDev;
                        }
                        else if (_systemListProd.Contains(split[1])) {
                            s3Bucket = _s3BucketProd;
                            sqsQueue = _sqsQueueProd;
                        }
                    }
                }

                //Get Max TempUWSID.
                var watch = new FileUtil(ConnectionString.WatchFolder);
                string subString = split[0].Substring(3);

                string uwsFileName;
                string fileType;
                string systemSerial = watch.GetSystemSerial(triggerFileName);

                int tempSerial;
                if (!int.TryParse(systemSerial, out tempSerial)) {
                    return;
                }

                string cpuFileName = watch.GetCPUFileName(triggerFileName);
                string ossFileName = watch.GetOSSFileName(triggerFileName);
                string discopenFileName = watch.GetDISCOPENFileName(triggerFileName);
                string zippedPathwayFileName = string.Empty;

                uwsFileName = watch.GetUWSFileName(triggerFileName);
                string uwsFullPath = ConnectionString.FTPSystemLocation + systemSerial + "\\" + uwsFileName;
                var fileSize = 0l;
                if (File.Exists(uwsFullPath)) {
                    var fileInfo = new FileInfo(uwsFullPath);
                    fileSize = fileInfo.Length;
                }

                if (subString == "auto") {
                    //Check file type;
                    var uwsFileInfo = new UWSFileInfo();
                    fileType = uwsFileInfo.GetFileType(uwsFullPath);
                    if (fileType == "Pathway") {
                        //Check for System Serial from App.config.
                        var systems = ConfigurationManager.AppSettings["PathwaySystems"];
                        List<string> pathwayList = systems.Split(',').ToList();

                        //Check pathway data is compressed or not
                        if (uwsFileName.StartsWith("ZA") && pathwayList.Contains(systemSerial)) {
                            var unzipper = new RemoteProcessor(ConnectionString.ConnectionStringDB);
                            zippedPathwayFileName = uwsFileName;
                            string fileNameUnzipped = uwsFileName + DateTime.Now.Ticks;

                            unzipper.unzip(uwsFullPath, ConnectionString.FTPSystemLocation + systemSerial + "\\" + fileNameUnzipped);
                            uwsFileName = fileNameUnzipped;
                        }
                        else if (uwsFileName.StartsWith("ZA")) {
                            return;
                        }
                    }
                }
                else if (subString == "disk") {
                    uwsFileName = watch.GetUWSFileName(triggerFileName);
                    fileType = "disk";
                }
                else {
                    return;
                }

                //Check for new format.
                string newFormat = watch.ReadFirstLine(triggerFileName);
                if (newFormat.Equals("CPUInfo") || newFormat.Equals("OSS") || newFormat.Equals("DISCOPEN")) {
                    if (newFormat.Equals("CPUInfo")) {
                        cpuFileName = uwsFileName;
                    }
                    if (newFormat.Equals("OSS")) {
                        ossFileName = uwsFileName;
                    }
                    if (newFormat.Equals("DISCOPEN")) {
                        discopenFileName = uwsFileName;
                    }
                    //Remove uwsFileName.
                    uwsFileName = "";
                    
                }
                Log.InfoFormat("File Type : {0}", fileType.ToUpper());
                Log.InfoFormat("Data file name : {0}", uwsFileName);
                Log.InfoFormat("CPU file name : {0}", cpuFileName);
                Log.Info("Start uploading file");
                

                var uploadFile = new UploadFile(s3Bucket);
                var writeMessages = new WriteMessges(sqsQueue);
                var loadingInfoService = new LoadingInfoService(ConnectionString.ConnectionStringDB);

                //Load UWS File.
                if (uwsFileName.Length > 0) {
                    uploadFile.Upload(uwsFileName, systemSerial);
                    Log.Info("Finish uploading UWS file");
                    
                    var uwsId = loadingInfoService.GetMaxUWSIDFor();
                    loadingInfoService.UpdateUWSRelayTimeFor(systemSerial, uwsId, fileReceivedTime, DateTime.Now, uwsFileName, fileSize, (int)BusinessLogic.Enums.FileType.Type.System);
					writeMessages.write(fileType, systemSerial, uwsFileName, uwsId);
                }

                //Load CPU Info
                if (File.Exists(ConnectionString.FTPSystemLocation + systemSerial + "\\" + cpuFileName)) {
                    uploadFile.Upload(cpuFileName, systemSerial);
                    Log.Info("Finish uploading CUP INFO file");
                    
                    var uwsId = loadingInfoService.GetMaxUWSIDFor();
                    loadingInfoService.UpdateUWSRelayTimeFor(systemSerial, uwsId, fileReceivedTime, DateTime.Now, cpuFileName, fileSize, (int)BusinessLogic.Enums.FileType.Type.CPUInfo);
					writeMessages.write("CPUINFO", systemSerial, cpuFileName, uwsId);
                }

                if (ossFileName.Length > 1) {
                    if (File.Exists(ConnectionString.FTPSystemLocation + systemSerial + "\\" + ossFileName)) {
                        uploadFile.Upload(ossFileName, systemSerial);
                        Log.Info("Finish uploading OSS JOURNAL file");
                        
                        var uwsId = loadingInfoService.GetMaxUWSIDFor();
                        loadingInfoService.UpdateUWSRelayTimeFor(systemSerial, uwsId, fileReceivedTime, DateTime.Now, ossFileName, fileSize, (int)BusinessLogic.Enums.FileType.Type.OSS);
						writeMessages.write("JOURNAL", systemSerial, ossFileName, uwsId);
                    }
                }

                if (fileType == "System" && discopenFileName.Length > 1) {
                    if (File.Exists(ConnectionString.FTPSystemLocation + systemSerial + "\\" + discopenFileName)) {
                        uploadFile.Upload(discopenFileName, systemSerial);
                        Log.Info("Finish uploading DISC OPEN file");
                        
                        var uwsId = loadingInfoService.GetMaxUWSIDFor();
                        loadingInfoService.UpdateUWSRelayTimeFor(systemSerial, uwsId, fileReceivedTime, DateTime.Now, discopenFileName, fileSize, (int)BusinessLogic.Enums.FileType.Type.System);
						writeMessages.write(fileType, systemSerial, discopenFileName, uwsId);
                    }
                }

                //Deleta Pathway compressed data.
                if (fileType == "Pathway") {
                    if (File.Exists(ConnectionString.FTPSystemLocation + systemSerial + "\\" + zippedPathwayFileName))
                        File.Delete(ConnectionString.FTPSystemLocation + systemSerial + "\\" + zippedPathwayFileName);
                }
            }
            catch (Exception ex) {
                Log.Error("Error Processing in JobRelay::OnCreated");
                Log.ErrorFormat("System Folder: {0}", ConnectionString.FTPSystemLocation);
                Log.ErrorFormat("Error: {0}", ex);
                
                if (!ConnectionString.IsLocalAnalyst) {
                    var amazonOperations = new AmazonOperations();
                    amazonOperations.WriteErrorQueue("Processing Job File Error: " + ex.Message);
                }
                else {
                    var email = new EmailToSupport(ConnectionString.AdvisorEmail, ConnectionString.SupportEmail,
                        ConnectionString.WebSite,
                        ConnectionString.EmailServer, ConnectionString.EmailPort, ConnectionString.EmailUser,
                        ConnectionString.EmailPassword, ConnectionString.EmailAuthentication,
                        ConnectionString.FTPSystemLocation, ConnectionString.ServerPath,
                        ConnectionString.EmailIsSSL, ConnectionString.IsLocalAnalyst, 
                        ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                    email.SendLocalAnalystErrorMessageEmail("FTP Server - JobRelay.cs", ex.Message, LicenseService.GetProductName(ConnectionString.ConnectionStringDB));
                }
            }
        }
	}
}