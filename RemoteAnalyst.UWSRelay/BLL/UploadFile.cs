using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RemoteAnalyst.AWS.Glacier;
using RemoteAnalyst.AWS.S3;
using RemoteAnalyst.BusinessLogic.BLL;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using System.Windows.Forms;
using log4net;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.UWSRelay.BLL {
    /// <summary>
    /// Build the full path and upload the data file to S3 bucket.
    /// </summary>
    class UploadFile {
        string _s3Bucket = string.Empty;

        public UploadFile(string s3Bucket) {
            _s3Bucket = s3Bucket;
        }

        public void Upload(string fileName, string systemSerial) {
            int retry = 0;
            bool success = false;

            if (systemSerial == "078998")
                systemSerial = "078831";

            string tempSystemSerial = systemSerial;
            //add '0' before 5 digit system serial
            while (tempSystemSerial.Length < 6) tempSystemSerial = "0" + tempSystemSerial;

            do {
                try {
					System_tblService system_TblService = new System_tblService(ConnectionString.ConnectionStringDB);
					var systemAndExpiredDate = system_TblService.GetEndDateFor(tempSystemSerial);
                    if (systemAndExpiredDate.Count == 0) {
                        //system not found
                        return;
                    }
					Decrypt decrypt = new Decrypt();
					var expireDate = Convert.ToDateTime(decrypt.strDESDecrypt(systemAndExpiredDate.Values.FirstOrDefault()).Split(' ')[1]);
					int timeZoneIndex = system_TblService.GetTimeZoneFor(tempSystemSerial);
					DateTime systemLocalTime = TimeZoneInformation.ToLocalTime(timeZoneIndex, DateTime.Now);

					if (systemLocalTime.Date > expireDate.AddDays(7).Date) {
						//out of grace 7 days of grace period
						return;
					}

					var s3 = new AmazonS3(_s3Bucket);
                    string fullAWSKey = "Systems/" + tempSystemSerial + "/" + fileName;
                    string localPath = ConnectionString.SystemLocation + systemSerial + "\\" + fileName;

                    if (File.Exists(localPath)) s3.WriteToS3MultiThreads(fullAWSKey, localPath);
                    retry = 5;
                    success = true;
                }
                catch (Exception ex) {
                    Thread.Sleep(AmazonError.GetTimeoutDuration(retry));
                    retry++;
                    if (retry == 5)
                        AmazonError.WriteLog(ex, "UploadFile.cs: Upload(" + fileName + ", " + systemSerial + ")",
                            ConnectionString.AdvisorEmail,
                            ConnectionString.SupportEmail,
                            ConnectionString.WebSite,
                            ConnectionString.EmailServer,
                            ConnectionString.EmailPort,
                            ConnectionString.EmailUser,
                            ConnectionString.EmailPassword,
                            ConnectionString.EmailAuthentication,
                            ConnectionString.SystemLocation, ConnectionString.ServerPath,
                            ConnectionString.EmailIsSSL,
                            ConnectionString.IsLocalAnalyst,
                            ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                }
            } while (retry < 5);

            if (success) {
                if (File.Exists(ConnectionString.SystemLocation + systemSerial + "\\" + fileName)) {
                    try {
                        File.Delete(ConnectionString.SystemLocation + systemSerial + "\\" + fileName);
                    }
                    catch {
                    }
                }
            }
        }

        public string Upload(string fileName, string systemSerial, string localPath, string type, ILog log, int uploadID = 0) {
            bool success = false;
            //var archiveID = "";
            if (ConnectionString.IsLocalAnalyst) {
                if (File.Exists(localPath)) {
                    var remoteFolder = ConnectionString.NetworkStorageLocation + "Systems\\" + systemSerial + "\\";

                    log.InfoFormat("remoteFolder: {0}", remoteFolder);
                    if (!Directory.Exists(remoteFolder)) {
                        log.Info("Create Network Folder");
                        Directory.CreateDirectory(remoteFolder);
                    }

                    var remoteLocation = ConnectionString.NetworkStorageLocation + "Systems\\" + systemSerial + "\\" + fileName;
                    log.InfoFormat("remoteLocation: {0}", remoteLocation);
                    log.InfoFormat("localPath: {0}", localPath);
                    
                    File.Copy(localPath, remoteLocation, true);
                    success = true;
                }
            }
            else {
                /*if (type.Equals("SYSTEM")) {
                    success = false;
                    ConnectionString.UWSUploadList.Add(new UWSUploadInfo {
                        FileName = fileName,
                        SystemSerial = systemSerial,
                        LocalPath = localPath,
                        Type = type,
                        UploadId = uploadID
                    });
                }
                else {*/
                int retry = 0;

                if (systemSerial == "078998")
                    systemSerial = "078831";

                log.InfoFormat("systemSerial: {0}", systemSerial);
                


                string tempSystemSerial = systemSerial;
                //add '0' before 5 digit system serial
                while (tempSystemSerial.Length < 6) tempSystemSerial = "0" + tempSystemSerial;

                log.InfoFormat("tempSystemSerial: {0}", tempSystemSerial);
                
                do {
                    try {
                        System_tblService system_TblService = new System_tblService(ConnectionString.ConnectionStringDB);
                        var systemAndExpiredDate = system_TblService.GetEndDateFor(tempSystemSerial);
                        if (systemAndExpiredDate.Count == 0) {
                            log.InfoFormat("{0} not found in System_tbl", tempSystemSerial);
                            
                            if (File.Exists(localPath)) {
                                try {
                                    File.Delete(localPath);
                                    log.InfoFormat("Delete unregistered system's measure file : {0}", localPath);
                                    var loadingInfoService = new LoadingInfoService(ConnectionString.ConnectionStringDB);
                                    loadingInfoService.DeleteLoadingInfoByFileName(fileName);
                                    log.InfoFormat("Deleted {0} from loadingInfo table", fileName);
                                }
                                catch {
                                }
                            }
                            return "File Deleted";
                        }
                        else { 
                            Decrypt decrypt = new Decrypt();
                            var expireDate = Convert.ToDateTime(decrypt.strDESDecrypt(systemAndExpiredDate.Values.FirstOrDefault()).Split(' ')[1]);
                            int timeZoneIndex = system_TblService.GetTimeZoneFor(tempSystemSerial);
                            DateTime systemLocalTime = TimeZoneInformation.ToLocalTime(timeZoneIndex, DateTime.Now);

                            if (systemLocalTime.Date > expireDate.AddDays(7).Date) {
                                log.InfoFormat("{0} is out of 7 days of grace period", tempSystemSerial);
                                
                                //out of 7 days of grace period
                                if (File.Exists(localPath)) {
                                    try {
                                        File.Delete(localPath);
                                        log.InfoFormat("Delete expired system's measure file : {0}", localPath);
                                        
                                    }
                                    catch {

                                    }
                                }
                                return "File Deleted";
                            }
                        }
						var s3 = new AmazonS3(_s3Bucket);
                        string fullAWSKey = "Systems/" + tempSystemSerial + "/" + fileName;
                        //string localPath = ConnectionString.SystemLocation + systemSerial + "\\" + fileName;
                        log.InfoFormat("fullAWSKey: {0}", fullAWSKey);
                        

                        //IAmazonGlacier amazonGlacier = new AmazonGlacierRA();
                        if (File.Exists(localPath)) {
                            s3.WriteToS3MultiThreads(fullAWSKey, localPath);
                            //archiveID = amazonGlacier.UploadToGlacier(vaultName, fullAWSKey, localPath);
                        }
                        retry = 5;
                        success = true;
                        log.InfoFormat("success: {0}", success);
                        
                    }
                    catch (Exception ex) {
                        log.ErrorFormat("retry: {0}", retry);
                        log.ErrorFormat("Error: {0}", ex.Message);
                        Thread.Sleep(AmazonError.GetTimeoutDuration(retry));
                        retry++;
                        if (retry == 5)
                            AmazonError.WriteLog(ex, "UploadFile.cs: Upload("
                                + fileName + ", " + systemSerial + ", " + localPath + ", " + type + ", " + uploadID + ")",
                                ConnectionString.AdvisorEmail,
                                ConnectionString.SupportEmail,
                                ConnectionString.WebSite,
                                ConnectionString.EmailServer,
                                ConnectionString.EmailPort,
                                ConnectionString.EmailUser,
                                ConnectionString.EmailPassword,
                                ConnectionString.EmailAuthentication,
                                ConnectionString.SystemLocation, ConnectionString.ServerPath,
                                ConnectionString.EmailIsSSL,
                                ConnectionString.IsLocalAnalyst,
                            ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                    }
                } while (retry < 5);
                //}
            }
            if (success) {
                if (File.Exists(localPath)) {
                    try {
                        File.Delete(localPath);
                    }
                    catch {
                    }
                }
            }
            return "";
        }
    }
}
