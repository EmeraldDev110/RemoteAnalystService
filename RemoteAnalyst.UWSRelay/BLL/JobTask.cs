using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using log4net;
using RemoteAnalyst.AWS.Infrastructure;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.Enums;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.UWSRelay.BLL {
    class JobTask {
        private static readonly ILog Log = LogManager.GetLogger("RelayLog");

        public void Timer_Elapsed(object source, ElapsedEventArgs e) {
            LoopSystemFolders();
        }

        public void LoopSystemFolders() {
            string systemLocation = ConnectionString.FTPSystemLocation;
            var helper = new Helper();

            var folders = new DirectoryInfo(systemLocation);
            foreach (DirectoryInfo folder in folders.GetDirectories()) {
                //LOOK FOR ALL FILES WITHIN FOLDER(S)
                var uwsDirectory = new DirectoryInfo(folder.FullName);
                foreach (FileInfo uwSfile in uwsDirectory.GetFiles()) {
                    if (File.Exists(uwSfile.FullName)) // IF FILE DOESN'T EXIST, ANOTHER THREAD ALREADY PROCESS IT.
                    {
                        //CHECK IF FILE IS IN USE
                        bool fileInUse = helper.IsFileinUse(uwSfile);
                        if (!fileInUse) {
                            //CHECK IF FILE SIZE IS INCREASING
                            bool fileSizeIncreased = helper.IsFileSizeIncreasing(uwSfile);
                            if (!fileSizeIncreased) {
                                //SEND FILE TO S3.
                                CreatTaskFile(folder.Name, uwSfile);
                            }
                        }
                    }
                }
            }
        }

        internal void CreatTaskFile(string systemSerial, FileInfo uwSfile) {
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
            Log.Info("Get job UWSfile at");
            

            if (!File.Exists(uwSfile.FullName)) // IF FILE DOESN'T EXIST, ANOTHER THREAD ALREADY PROCESS IT.
            {
                Log.ErrorFormat("The File doesn't exists. File Name: {0}", uwSfile.FullName);
                return;
            }

            try {
                Log.InfoFormat("ConnectionString.ConnectionStringDB: {0}", FTPFile.RemovePassword(ConnectionString.ConnectionStringDB));

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
                        //VERIFY EXISTANCE OF "UploadFolder" FOLDER
                        uwsFileDirectory = uwSfile.Directory.FullName.Trim();
                        if (!Directory.Exists(uwsFileDirectory + "\\UploadFolder")) {
                            Directory.CreateDirectory(uwsFileDirectory + "\\UploadFolder");
                        }

                        /*//New logic. Add the value to global value and have a timmer send the data.
                        uwsFilePath = uwsFileDirectory + "\\UploadFolder\\" + uwSfile.Name.Trim();
                        if (!ConnectionString.MeasureList.ContainsKey(uwSfile.FullName.Trim())) {
                            ConnectionString.MeasureList.Add(uwSfile.FullName.Trim(), systemSerial);
                        }*/

                        //Check for duplicated file.
                        if (File.Exists(uwsFileDirectory + "\\UploadFolder\\" + uwSfile.Name.Trim()))
                            File.Delete(uwsFileDirectory + "\\UploadFolder\\" + uwSfile.Name.Trim());

                        //MOVE FILE TO "UploadFolder" FOLDER
                        uwsFilePath = uwsFileDirectory + "\\UploadFolder\\" + uwSfile.Name.Trim();
                        File.Move(uwSfile.FullName.Trim(), uwsFilePath);

                        var fileInfo = new FileInfo(uwsFilePath);
                        var jobMeasure = new JobMeasures();
                        jobMeasure.SendMeasureFile(systemSerial, fileInfo);
                    }
                }
            }
            catch (Exception ex) {
                if (File.Exists(uwSfile.FullName)) // IF FILE DOESN'T EXIST, ANOTHER THREAD ALREADY PROCESS IT.
                {
                    Log.Error("Error Processing in JobTask::CreatTaskFile [File Exists]");
                    Log.ErrorFormat("System Folder: {0}", ConnectionString.FTPSystemLocation);
                    Log.ErrorFormat("FullName: {0}", uwSfile.FullName);
                    Log.ErrorFormat("Exception: {0}", ex.Message);

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
                            ConnectionString.EmailIsSSL, ConnectionString.IsLocalAnalyst,
                            ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                        email.SendLocalAnalystErrorMessageEmail("FTP Server - JobUWS.cs", ex.Message, LicenseService.GetProductName(ConnectionString.ConnectionStringDB));
                    }
                }
                else {
                    Log.Error("Error Processing in JobTask::CreatTaskFile [File does not exists]");
                    Log.ErrorFormat("System Folder: {0}", ConnectionString.FTPSystemLocation);
                    Log.ErrorFormat("FullName: {0}", uwSfile.FullName);
                    Log.ErrorFormat("Exception {0}", ex.Message);
                }
            }
        }
    }
}
