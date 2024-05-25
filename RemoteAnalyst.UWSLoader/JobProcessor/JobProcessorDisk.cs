using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using RemoteAnalyst.AWS.Infrastructure;
using RemoteAnalyst.AWS.Queue;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.Enums;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.Trigger.JobPool;
using RemoteAnalyst.UWSLoader.BLL;
using RemoteAnalyst.UWSLoader.Email;
using RemoteAnalyst.UWSLoader.SPAM.BLL;

namespace RemoteAnalyst.UWSLoader.JobProcessor {
    /// <summary>
    /// JobProcessorDisk class processes DISK data.
    /// Call the actual data processing classes to process the data.
    /// Create and send SQS messages for UponLoad storage reports.
    /// </summary>
    internal class JobProcessorDisk {
        private static readonly ILog Log = LogManager.GetLogger("JobLoader");
        private readonly string connectionStr = ConnectionString.ConnectionStringDB;
        private readonly string connectionStrTrend = ConnectionString.ConnectionStringTrend;
        private readonly string filename = string.Empty;
        private string tempSystemSerial = string.Empty;
        private int tempUWSID;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fName"> File name of the data file.</param>
        /// <param name="tempUWSid"> UWS ID of this load.</param>
        /// <param name="systemSerial"> System serial number.</param>
        public JobProcessorDisk(string fName, int tempUWSid, string systemSerial) {
            filename = fName;
            tempUWSID = tempUWSid;
            tempSystemSerial = systemSerial;
        }

        /// <summary>
        /// Main function of this class.
        /// Call disk loader to load the data in database.
        /// Update the loading status in the database.
        /// </summary>
        public void ProcessJobDisk() {
            //Force all datetime to be in US format.
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            string tempfile = string.Empty;
            string filePath = string.Empty;
            string tempStr = string.Empty;
            string userEmail = string.Empty;
            var dtDataDate = new DateTime();
            int uwsId = 0;

            try {
                int sysSerialLength = tempSystemSerial.Length;
                if (sysSerialLength < 6) {
                    //Prepend Leading Zeroes
                    for (int i = 0; i < (6 - sysSerialLength); i++) {
                        tempSystemSerial = "0" + tempSystemSerial;
                    }
                }

                string systemLocation = ConnectionString.SystemLocation;
                filePath = systemLocation + tempSystemSerial + "\\" + filename;
                var fileInfo = new FileInfo(filePath);

                try {
                    var loadingInfo = new LoadingInfoService(ConnectionString.ConnectionStringDB);
                    if (tempUWSID == 0)
                        uwsId = loadingInfo.GetMaxUWSIDFor();
                    else
                        uwsId = tempUWSID;
                    loadingInfo.UpdateFor(filePath, tempSystemSerial, fileInfo.Length.ToString(), "2", uwsId.ToString()); //5: QNM
                }
                catch { }
                Log.InfoFormat("filePath {0}, tempSystemSerial {1}", filePath, tempSystemSerial);

                //**************************************************************
                //close the File.

                //*************************************************************
                //Get Date from UWS File.
                try {
                    using (var sr = new StreamReader(filePath)) {
                        string line = string.Empty;
                        string dataDate = string.Empty;

                        //Goto 9th line of the file.
                        for (int x = 0; x < 9; x++) {
                            sr.ReadLine();
                        }

                        if ((line = sr.ReadLine()) != null) {
                            //Get Date.
                            dataDate = line.Substring(21).Trim();
                            dtDataDate = Convert.ToDateTime(dataDate);
                        }
                        else {
                            throw new Exception("no date to read");
                        }
                    }
                }
                catch (Exception ex) {
                    Log.ErrorFormat("Exception {0}", ex);
                    if (ConnectionString.IsLocalAnalyst)
                    {
                        var email = new EmailToSupport(ConnectionString.AdvisorEmail, ConnectionString.SupportEmail,
                            ConnectionString.WebSite,
                            ConnectionString.EmailServer, ConnectionString.EmailPort, ConnectionString.EmailUser,
                            ConnectionString.EmailPassword, ConnectionString.EmailAuthentication,
                            ConnectionString.SystemLocation, ConnectionString.ServerPath,
                            ConnectionString.EmailIsSSL, ConnectionString.IsLocalAnalyst,
                            ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                        email.SendLocalAnalystErrorMessageEmail("UWSLoader - JobProcessDisk.cs", ex.Message, LicenseService.GetProductName(ConnectionString.ConnectionStringDB));
                    }
                    else {
                        var amazonOperations = new AmazonOperations();
                        StringBuilder errorMessage = new StringBuilder();
                        errorMessage.Append("Source: JobProcessorDisk \r\n");
                        errorMessage.Append("Message: " + ex.Message + "\r\n");
                        errorMessage.Append("StackTrace: " + ex.StackTrace + "\r\n");
                        amazonOperations.WriteErrorQueue(errorMessage.ToString());
                    }
                    //Insert Error.
                    //Need to do this Tomorrow
                    var remoteFail = new LoadingInfoDiskService(connectionStr);
                    remoteFail.UpdateFailedToLoadDiskFor(tempStr);

                    return;
                }

                //*******************************************************************
                //Change for Korea.
                //*******************************************************************
                //Get End Date.
                try {
                    IDictionary<string, string> endDate = new Dictionary<string, string>();
                    var systemService = new System_tblService(connectionStr);
                    endDate = systemService.GetEndDateFor(tempSystemSerial);
                    if (endDate.Count != 0) {
                        if (endDate.First().Value.Length == 0) {
                            Log.Info("No License");                            
                            return;
                        }
                        else if (endDate.First().Value != "") {
                            var decrypt = new Decrypt();
                            string decryptDate = decrypt.strDESDecrypt(endDate.First().Value).Split(' ')[1]; //get the date [0] is systemName.
                            //Get End Date.
                            DateTime planEndDate = Convert.ToDateTime(decryptDate);
#if EVALUATION_COPY
                            //For Eval, license should not be extended.
#else
                            //Check end date. Set 7 days grace days
                            planEndDate = planEndDate.AddDays(7);
                            if (ConnectionString.IsLocalAnalyst)
                            {
                                //Extend the license date for one more year.
                                planEndDate = planEndDate.AddYears(1);
                            }
#endif

                            int timeZoneIndex = systemService.GetTimeZoneFor(tempSystemSerial);
                            DateTime systemLocalTime = TimeZoneInformation.ToLocalTime(timeZoneIndex, DateTime.Now);

                            if (planEndDate.Date < systemLocalTime.Date)
                            {
#if EVALUATION_COPY
                                Log.Info("License Expired");
#else
                                Log.Info("License Expired and out of 7 days grace period");
#endif                                                                            
                                return;
                            }
                        }
                    }
                    else {
                        Log.Info("No System Information Obtained");                        
                        return;
                    }
                }
                catch {
                    //If fail to get a respone from the server exit the loading.

                    //Insert Error.
                    var remoteFail = new LoadingInfoDiskService(connectionStr);
                    remoteFail.UpdateFailedToLoadDiskFor(tempStr);
                    return;
                }

                //*************************************************************
                try {
                    Log.Info("Before calling DiskLoader Class");
                    
                    
                    //Call diskLoader class to insert data into database.
                    var loader = new DiskLoader(filePath, Log, tempSystemSerial, dtDataDate);
                    bool success = loader.loadDiskData();
                    Log.InfoFormat("Return: {0}", success);
                    
                    if (success) {
                        var jobProcess = new LoadingInfoDiskService(connectionStr);
                        //declear the loadedtime in LoadingInfoDisk.
                        jobProcess.UpdateLoadingInfoDiskFor(filename);
                    }
                }
                catch (Exception ex) {
                    Log.ErrorFormat("Exception {0}", ex);
                    if (ConnectionString.IsLocalAnalyst)
                    {
                        var email = new EmailToSupport(ConnectionString.AdvisorEmail, ConnectionString.SupportEmail,
                            ConnectionString.WebSite,
                            ConnectionString.EmailServer, ConnectionString.EmailPort, ConnectionString.EmailUser,
                            ConnectionString.EmailPassword, ConnectionString.EmailAuthentication,
                            ConnectionString.SystemLocation, ConnectionString.ServerPath,
                            ConnectionString.EmailIsSSL, ConnectionString.IsLocalAnalyst,
                            ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                        email.SendLocalAnalystErrorMessageEmail("UWSLoader - JobProcessDisk.cs", ex.Message, LicenseService.GetProductName(ConnectionString.ConnectionStringDB));
                    }
                    else {
                        var amazonOperations = new AmazonOperations();
                        StringBuilder errorMessage = new StringBuilder();
                        errorMessage.Append("Source: JobProcessorDisk 2 \r\n");
                        errorMessage.Append("Message: " + ex.Message + "\r\n");
                        errorMessage.Append("StackTrace: " + ex.StackTrace + "\r\n");
                        amazonOperations.WriteErrorQueue(errorMessage.ToString());
                    }
                    //Insert Error.
                    var remoteFail = new LoadingInfoDiskService(connectionStr);
                    remoteFail.UpdateFailedToLoadDiskFor(tempStr);
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("Exception {0}" + ex);

                //Insert Error.
                if (ConnectionString.IsLocalAnalyst)
                {
                    var email = new EmailToSupport(ConnectionString.AdvisorEmail, ConnectionString.SupportEmail,
                        ConnectionString.WebSite,
                        ConnectionString.EmailServer, ConnectionString.EmailPort, ConnectionString.EmailUser,
                        ConnectionString.EmailPassword, ConnectionString.EmailAuthentication,
                        ConnectionString.SystemLocation, ConnectionString.ServerPath,
                        ConnectionString.EmailIsSSL, ConnectionString.IsLocalAnalyst,
                        ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                    email.SendLocalAnalystErrorMessageEmail("UWSLoader - JobProcessDisk.cs", ex.Message, LicenseService.GetProductName(ConnectionString.ConnectionStringDB));
                }
                else {
                    var amazonOperations = new AmazonOperations();
                    StringBuilder errorMessage = new StringBuilder();
                    errorMessage.Append("Source: JobProcessorDisk 3 \r\n");
                    errorMessage.Append("Message: " + ex.Message + "\r\n");
                    errorMessage.Append("StackTrace: " + ex.StackTrace + "\r\n");
                    amazonOperations.WriteErrorQueue(errorMessage.ToString());
                }
                var remoteFail = new LoadingInfoDiskService(connectionStr);
                remoteFail.UpdateFailedToLoadDiskFor(tempStr);
            }
            finally {

                try {
                    var systemInfo = new System_tblService(ConnectionString.ConnectionStringDB);
                    var systemName = systemInfo.GetSystemNameFor(tempSystemSerial);
                    var loadingInfo = new LoadingInfoService(ConnectionString.ConnectionStringDB);
                    loadingInfo.UpdateFor(uwsId, systemName, dtDataDate, dtDataDate, 2); //type 2 is Storage
                }
                catch (Exception ex) {
                    Log.ErrorFormat("Loading Info Error2: {0}", ex);
                }

                //Change CurrentLoad value.
                var process = new JobProcess();
                bool insertChecker = process.InsertLoadingStatusDetail(false, filename, 0, "DISK", tempSystemSerial);

                Log.InfoFormat("insertChecker: {0}", insertChecker);
                
                if (insertChecker) {
                    process.ChangeStatus(-1);
                }
                
                //Update LoadingInfoDisk.
            }
        }
    }
}