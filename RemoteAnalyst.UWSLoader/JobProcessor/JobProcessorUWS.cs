using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using RemoteAnalyst.AWS.Infrastructure;
using RemoteAnalyst.AWS.Queue;
using RemoteAnalyst.AWS.S3;
using RemoteAnalyst.BusinessLogic.BLL;
using RemoteAnalyst.BusinessLogic.Enums;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.BusinessLogic.ModelService;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.Trigger.JobPool;
using RemoteAnalyst.UWSLoader.BLL;
using RemoteAnalyst.UWSLoader.Email;
using RemoteAnalyst.UWSLoader.SPAM;
using RemoteAnalyst.UWSLoader.SPAM.BLL;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.Util;
using log4net;

namespace RemoteAnalyst.UWSLoader.JobProcessor {
    /// <summary>
    /// JobProcessUWS class processes SYSTEM and PATHWAY data.
    /// Call the actual data processing classes to process the data.
    /// Create and send SQS messages for UponLoad reports.
    /// Send out emails to users after load complete.
    /// </summary>
    internal class JobProcessorUWS {
        private static readonly ILog Log = LogManager.GetLogger("DataLoad");
        private readonly string _connectionStr = ConnectionString.ConnectionStringDB;
        private readonly string _connectionStrSPAM = ConnectionString.ConnectionStringSPAM;
        private readonly string _connectionStrTrend = ConnectionString.ConnectionStringTrend;
        private readonly string _filename = string.Empty;
        private readonly string _loadType = string.Empty;
        private readonly string _newNsid = string.Empty;
        private readonly string _strEntities = string.Empty;

        // private readonly string _strReportName = string.Empty;
        private readonly int _tempUWSID;
        private DateTime _retentionDate;
        private string _advisorEmail = string.Empty;
        //string uponloadiReport = "";
        private string _cpubusyCharts = "";

        private int _fileType = 4;
        private int _filenamecount = 1;
        private string _filepath = string.Empty;
        private FileInfo _finfo;
        private string _firstname = string.Empty;
        private long _fsize = 1000;
        private bool _isSystem;
        private string _lastname = string.Empty;
        private DateTime _loadEndTime = DateTime.MinValue;
        //Move the variable out side the function
        private DateTime _loadStartTime = DateTime.MinValue;
        private string _newFileName = string.Empty;

        private string _nodename = string.Empty;
        private bool _opened;
        private bool _result;
        private int _retentionDay;
        private int _scope = -1;
        private string _starttime = string.Empty;
        private string _stoptime = string.Empty;
        private string _tempSystemSerial = string.Empty;
        private string uponloadreport = "";

        private string _userEmail = string.Empty;
        private string _uwsFileUnzip = "";
        private string _uwsfile = string.Empty;
        private int _uwsid;
        private int _ntsID;

        public JobProcessorUWS(string fName, int tempUWSId, string systemSerial, string loadType, int ntsID = 0) {
            _filename = fName;
            _tempUWSID = tempUWSId;
            //Check if we need to change the System Serial Number.
            var systemSerialConversionService = new SystemSerialConversionService(ConnectionString.ConnectionStringDB);
            var newSystemSerial = systemSerialConversionService.GetConvertionSystemSerialFor(systemSerial);
            if (newSystemSerial.Length > 0)
                _tempSystemSerial = newSystemSerial;
            else
                _tempSystemSerial = systemSerial;
            _loadType = loadType;
        }

        /// <summary>
        /// Check the file type. Calling different function for PATHWAY and SYSTEM load.
        /// Used to unzipp the file. For this release we do not accept zipped PATHWAY or SYSTEM data.
        /// </summary>
        /// <param name="companyID"> The company ID of the system.</param>
        public bool CheckSPAMData(int companyID) {
            bool retVal = false;
            Process currentProc = Process.GetCurrentProcess();

            var collectionType = new UWSFileInfo();
            UWS.Types checkSPAM = collectionType.UwsFileVersionNew(_uwsfile);

            Log.InfoFormat("_loadType: {0}", _loadType);

            //Now use 'loadtype' to tell whether it's pathway data
            if (_loadType == "PATHWAY") {
                Log.InfoFormat("fileType: {0}", _fileType);
                
                if (_fileType == 4) {
                    Log.Info("Calling RemoteLoading.dll!");
                }
                else {
                    //Load new pathway logic.
                    Log.Info("Calling New Loading for Pathway!");

                    //Give database connection.
                    //SPAM collection.
                    Config.UWSPath = _uwsfile;
                    Config.ConnectionString = ConnectionString.ConnectionStringSPAM;
                    Config.RAConnectionString = ConnectionString.ConnectionStringDB;

                    //Ryan Ji
                    //Do not append ticks
                    if (!_uwsfile.EndsWith(".180"))
                        _uwsFileUnzip = _uwsfile;
                    else {
                        if (_uwsfile.Contains("RPUWS")) {
                            _uwsFileUnzip = _uwsfile;
                        }
                        else if (_uwsfile.StartsWith("DO") && _uwsfile.EndsWith(".180")) {
                            _uwsFileUnzip = _uwsfile;
                        }
                        else {
                            if (_uwsfile.EndsWith(".180")) {
                                try {
                                    //Unzip the Pathway data.
                                    Log.Info("Unzipping the UWS File");
                                    
                                    _uwsFileUnzip = _uwsfile + DateTime.Now.Ticks;
                                    var remoteProcessor = new RemoteLoadingProcessor.RemoteProcessor(ConnectionString.ConnectionStringDB);
                                    remoteProcessor.unzip(_uwsfile, _uwsFileUnzip);
                                    Log.InfoFormat("Unzipping File Name: {0}",_uwsFileUnzip);
                                }
                                catch (Exception ex) {
                                    Log.ErrorFormat("Unzipping Error: {0}",ex.Message);
                                }
                            }
                            else {
                                Log.Info("File is not a zip file.");
                                
                                _uwsFileUnzip = _uwsfile;
                            }
                        }
                    }

                    //Load to new database.
                    var databaseMapService = new DatabaseMappingService(_connectionStr);
                    string newConnectionString = databaseMapService.GetConnectionStringFor(_tempSystemSerial);

                    if (newConnectionString.Length > 0) {
                        Log.InfoFormat("PARAMETERS: {0}, {1}, {2}", _uwsFileUnzip,
                            DiskLoader.RemovePassword(ConnectionString.ConnectionStringDB),
                            DiskLoader.RemovePassword(newConnectionString));
                        
                        var loadPathway = new Pathway.Loader.OpenUWSPathway(_uwsFileUnzip, Log, _tempSystemSerial);
                        var pathwayCollectionInfo = loadPathway.CreateNewData();
                        retVal = true;
                        var loadingInfoService = new LoadingInfoService(_connectionStr);
                        loadingInfoService.UpdateLoadedTimeFor(_uwsid);
						//Update start and stop time for Pathway.
						loadingInfoService.UpdateCollectionTimeFor(_uwsid, pathwayCollectionInfo.FromTimestamp, pathwayCollectionInfo.ToTimestamp);

						//Upload to AWS s3 uws-files buckets.
						if (_uwsid != 0) {
                            try {
                                if (ConnectionString.IsLocalAnalyst)
                                {
                                    var fileInfo = new FileInfo(_uwsFileUnzip);
                                    Log.Info("******  Save file to Network Storage  *********");

                                    Log.InfoFormat("file name: {0}", fileInfo.Name);

                                    string newFileName = _tempSystemSerial + "_" + fileInfo.Name;
                                    var networkLocation = ConnectionString.NetworkStorageLocation + "Systems/" + _tempSystemSerial + "/" + newFileName;
                                    try
                                    {
                                        fileInfo.CopyTo(networkLocation, true);
                                        fileInfo.Delete();
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.ErrorFormat("Error occured when save file to Network Location: {0}", ex);

                                    }
                                    var pathwayDirectories = new PathwayDirectoryService(ConnectionString.ConnectionStringDB);
                                    pathwayDirectories.InsertpathwayDirectoryFor(_uwsid, _tempSystemSerial, pathwayCollectionInfo.FromTimestamp, pathwayCollectionInfo.ToTimestamp, networkLocation);
                                }
                                else {
                                    //Insert into s3.
                                    var fileInfo = new FileInfo(_uwsFileUnzip);
                                    var loadingInfo = new LoadingInfoService(ConnectionString.ConnectionStringDB);

                                    //First upload to s3, then insert in PathwayDirectories in mainDB
                                    Log.Info("******  Upload to Amazon S3  *********");
                                    
                                    Log.InfoFormat("Check archive ID using file name: {0}",fileInfo.Name);
                                    

									string newFileName = _tempSystemSerial + "_" + fileInfo.Name;
									//Upload to s3
									string s3FilePath = "Systems/" + _tempSystemSerial + "/" + newFileName;
									var s3 = new AmazonS3(ConnectionString.S3UWS);

									Log.Info("Start multithread upload file to s3.");
									
									//s3.WriteToS3WithLocaFile(s3FilePath, fileInfo.FullName);
									var uploadFileMultithread = new Thread(() => s3.WriteToS3WithLocaFile(s3FilePath, fileInfo.FullName));
									uploadFileMultithread.IsBackground = true;
									uploadFileMultithread.Start();

									var pathwayDirectories = new PathwayDirectoryService(ConnectionString.ConnectionStringDB);

									bool duplicate = pathwayDirectories.CheckDuplicateTimeFor(_tempSystemSerial, pathwayCollectionInfo.FromTimestamp, pathwayCollectionInfo.ToTimestamp, s3FilePath);
									if (!duplicate) {
										try {
											if (File.Exists(fileInfo.FullName)) {
												File.Delete(fileInfo.FullName);
											}
										}
										catch (Exception ex) {
											Log.ErrorFormat("File Delete Error: {0}", ex);
										}
										pathwayDirectories.InsertpathwayDirectoryFor(_uwsid, _tempSystemSerial, pathwayCollectionInfo.FromTimestamp, pathwayCollectionInfo.ToTimestamp, s3FilePath);
									}

									Log.Info("******  Upload to Amazon S3 completed  *********");
                                    
                                }
							}
                            catch (Exception ex) {
                                Log.ErrorFormat("Exception when save file: {0}",ex.Message);
                                
                            }
                        }
                    }
                    else {
                        Log.Info("New Pathway Load: No schema Exists!");
                        
                    }
                }
            }
            else {
                Log.Info("Calling New Loading!");
                
                //Give database connection.
                //SPAM collection.
                Config.UWSPath = _uwsfile;
                Config.ConnectionString = ConnectionString.ConnectionStringSPAM;
                Config.RAConnectionString = ConnectionString.ConnectionStringDB;

                bool zipOkay = false;
                //Check if file needs to be unzip.
                if (_uwsfile.Contains(".402")) {
                    zipOkay = true;
                    _uwsFileUnzip = _uwsfile;
                }
                else {
                    //Checking File Type.
                    var fileExtention = Path.GetExtension(_uwsfile);
                    if (fileExtention.Equals(".180") || fileExtention.Length == 0) {
                        if (!_uwsfile.Contains("DO")) {
                            //unzip the processs.
                            Log.Info("Unzipping the UWS File");
                            
                            _uwsFileUnzip = _uwsfile + DateTime.Now.Ticks;

                            currentProc.Refresh();
                            Log.InfoFormat("Before ZIP Memory :" + currentProc.PrivateMemorySize64);
                            

                            try {
                                //Check if the file is UWS file.
                                var isNewUws = collectionType.IsLatestUWSFile(_uwsfile);

                                if (!isNewUws) {
                                    if (!_uwsfile.ToLower().Contains("ppwy")) {
                                        //Need to UNZIP the UWS file again.
                                        var remoteProcessor = new RemoteLoadingProcessor.RemoteProcessor(ConnectionString.ConnectionStringDB);
                                        zipOkay = remoteProcessor.unzip(_uwsfile, _uwsFileUnzip);
                                    }
                                    else
                                    {
                                        zipOkay = true;
                                        _uwsFileUnzip = _uwsfile;
                                    }
                                }
                                else {
                                    zipOkay = true;
                                    _uwsFileUnzip = _uwsfile;
                                }
                            }
                            catch (Exception ex) {
                                Log.ErrorFormat(ex.Message);
                                
                            }

                            currentProc.Refresh();
                            Log.InfoFormat("After ZIP Memory :" + currentProc.PrivateMemorySize64);
                            
                        }
                        else {
                            Log.Info("***********************UNKNOWN FILE EXTENSION!!");
                            
                            zipOkay = true;
                            _uwsFileUnzip = _uwsfile;
                        }
                    }
                    else {
                        Log.Info("***********************UNKNOWN FILE EXTENSION!!!");
                        
                        zipOkay = true;
                        _uwsFileUnzip = _uwsfile;
                    }
                }

                if (zipOkay) {
                    currentProc.Refresh();
                    Log.InfoFormat("Before Calling OpenUWS Memory :" + currentProc.PrivateMemorySize64);
                    
                    //Load data.
                    var openUWS = new OpenUWS();
                    retVal = openUWS.CreateNewData(_uwsFileUnzip, _uwsid, Log, checkSPAM, companyID, DateTime.MinValue, DateTime.MinValue, ConnectionString.DatabasePrefix, _ntsID);

				    currentProc.Refresh();
                    Log.InfoFormat("After Calling OpenUWS Memory :" + currentProc.PrivateMemorySize64);
                    
					try {
						//Check CustomerOrders Table.
						var customerFiles = new CustomerOrderService(ConnectionString.ConnectionStringDB);
						var uploadId = customerFiles.GetUploadIDBySystemSeirlaAndFileName(_tempSystemSerial, Path.GetFileName(_uwsfile));

						var uploadService = new UploadService(ConnectionString.ConnectionStringDB);
						var uploadMessageService = new UploadMessagesService(ConnectionString.ConnectionStringDB);
						if (retVal && uploadId != -1) {
							uploadService.UpdateLoadedDateFor(uploadId);
							uploadService.UpdateLoadedStatusFor(uploadId, "Loaded");
							uploadMessageService.InsertNewEntryFor(uploadId, DateTime.Now, "Finish Loading to MySQL");
						}
						else if(uploadId == -1) {
							Log.Info("Could not find fileName in CustomerOrder Table. So this is probably not from NTS/Upload File feature.");
							
						}
						else {
							uploadService.UpdateLoadedStatusFor(uploadId, "Error");
							uploadMessageService.InsertNewEntryFor(uploadId, DateTime.Now, "Load Failed");
						}
					}
					catch (Exception ex) {
						Log.ErrorFormat("CustomerOrder error: {0}",ex);
						
					}
                }
            }
            return retVal;
        }

        public void CreateCSVDataSet(string csvfile, DataSet masterDataSet) {
            StreamReader streamreader = File.OpenText(csvfile);
            string line;

#region DataSet Columns

            var myDataTable = new DataTable("CSVFile");

            // Create new DataColumn, set DataType, ColumnName and add to DataTable.
            var myDataColumn = new DataColumn { DataType = Type.GetType("System.String"), ColumnName = "CSVData" };
            myDataTable.Columns.Add(myDataColumn);

#endregion DataSet Columns

            while ((line = streamreader.ReadLine()) != null) {
                DataRow myDataRow = myDataTable.NewRow();
                myDataRow["CSVData"] = line;
                myDataTable.Rows.Add(myDataRow);
            }
            streamreader.Close();
            masterDataSet.Tables.Add(myDataTable);
        }

        /// <summary>
        /// Main function of this class.
        /// Call UWS loader to load the data in database.
        /// Update the loading status in the database.
        /// </summary>
        public void ProcessJob() {
            //Force all datetime to be in US format.
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Process currentProc = Process.GetCurrentProcess();

            try {
                _opened = false;
                var systemTbl = new System_tblService(_connectionStr);
                int companyID = systemTbl.GetCompanyIDFor(_tempSystemSerial);

#region Get basic information

                if (_loadType == "SYSTEM" || _loadType == "PATHWAY") {
                    //Ryan Ji
                    //Move the code to a seperate function
                    UWSAutoLoad();
                }
                else {
                    UWSManuLoad();
                }

#endregion Get basic information

                currentProc.Refresh();
                Log.InfoFormat("Before Load Memory: {0}",currentProc.PrivateMemorySize64);
                Log.InfoFormat("Log Created at {0}, System Serial: {1}, UWS ID: {2}, UWS File: {3}",
                    DateTime.Now, _tempSystemSerial, _uwsid, _uwsfile);

                /////////////////////////////////////////////////////////////////////////////
                //Check duplicate data.
                //get basic info from loadinginfo table.
                /////////////////////////////////////////////////////////////////////////////
                if (_filename.StartsWith("DO")) {
                    _uwsfile = ConnectionString.SystemLocation + _tempSystemSerial + "\\" + _filename;
                    if (!GetLicenseInfo()) {
                        try {
                            File.Delete(_uwsfile);
                        }
                        catch {
                        }
                        return;
                    }
                    CheckSPAMData(companyID);
                    return;
                }
                Log.Info("Checking duplicate data!");


                GetLoadPeriod(_uwsid);

                if (_loadStartTime != DateTime.MinValue && _loadEndTime != DateTime.MinValue) {
                    //Call UploadInfo to check duplicate.
                    var sampleInfo = new SampleInfoService(_connectionStr);
                    bool okToLoad = sampleInfo.CheckDuplicateDataFor(_tempSystemSerial, _loadStartTime, _loadEndTime,
                        _isSystem);
                    if (okToLoad) {
                        //stop the loading.
                        Log.Info("Duplicated data stop processing!");
                        
                        return;
                    }
                }
                else {
                    Log.InfoFormat("Cannot compare - StartTime : {0}, EndTime : {1}", _loadStartTime, _loadEndTime);
                }
                Log.Info("Getting Retention Day!");

                _retentionDay = systemTbl.GetRetentionDayFor(_tempSystemSerial);
                Log.InfoFormat("Retention Day: {0}",_retentionDay);

                if (!GetLicenseInfo()) {
                    try {
                        File.Delete(_uwsfile);
                    }
                    catch {
                    }
                    return;
                }

                //Update NTS Order Message
                var uploadMessageService = new UploadMessagesService(_connectionStr);
                if (!_ntsID.Equals(0)) {
                    var uploadService = new UploadService(_connectionStr);
                    uploadService.UpdateLoadedStatusFor(_ntsID, "Loaindg");

                    var fileInfo = new FileInfo(_uwsfile);
                    uploadMessageService.InsertNewEntryFor(_ntsID, DateTime.Now, "Loading " + fileInfo.Name + " to MySQL");
                }

                Log.Info("CheckSPAMData: ");
                
                //Check for SAPM data.
                _result = CheckSPAMData(companyID);

                //Update NTS Order Message
                if (!_ntsID.Equals(0)) {
                    var uploadService = new UploadService(_connectionStr);
                    if (_result) {
                        uploadService.UpdateLoadedDateFor(_ntsID);
                        uploadService.UpdateLoadedStatusFor(_ntsID, "Loaded");
                        var fileInfo = new FileInfo(_uwsfile);
                        uploadMessageService.InsertNewEntryFor(_ntsID, DateTime.Now, "Finish Loading " + fileInfo.Name + " to MySQL");
                    }
                    else {
                        uploadService.UpdateLoadedStatusFor(_ntsID, "Error");
                        uploadMessageService.InsertNewEntryFor(_ntsID, DateTime.Now, "Load Failed");
                    }
                }

                //Insert RemoteLoading returned a false. (Didn't complete the loading.)
                var loadingInfoService = new LoadingInfoService(_connectionStr);
                if (!_result) {
                    loadingInfoService.UpdateLoadingStatusFor(_uwsid, "Fail");
					var info = loadingInfoService.GetLoadingInfoFor(_uwsid);
					_stoptime = Convert.ToString(info.StopTime);

					_nodename = info.SystemName;
					_starttime = Convert.ToString(info.StartTime);
					SendLoadFailEmail();
                }

                var loadingInfoView = loadingInfoService.GetLoadingInfoFor(_uwsid);

                if (loadingInfoView.SystemName != "" && loadingInfoView.StartTime != DateTime.MinValue &&
                    loadingInfoView.StopTime != DateTime.MinValue) {
                    _stoptime = Convert.ToString(loadingInfoView.StopTime);

                    _nodename = loadingInfoView.SystemName;
                    _starttime = Convert.ToString(loadingInfoView.StartTime);
                    if (loadingInfoView.SampleType == 4) {
                        _isSystem = true;
                    }
                }

                _retentionDate = DateTime.Now;
                //Add RetentionDate.
                _retentionDate = _retentionDate.AddDays(_retentionDay);
                if (_newNsid.Length > 0) {
                    var sampleInfoService = new SampleInfoService(_connectionStr);
                    sampleInfoService.UpdateExpireInfoFor(_retentionDate, _newNsid);

                    Log.InfoFormat("Comparing " + Convert.ToDateTime(_starttime) + " to " +
                                         Convert.ToDateTime(_stoptime));
                    
                    //Change Pathway start & end date if they are the same.
                    if (DateTime.Compare(Convert.ToDateTime(_starttime), Convert.ToDateTime(_stoptime)) == 0) {
                        DateTime tempStopTime = Convert.ToDateTime(_stoptime);
                        tempStopTime = tempStopTime.AddDays(1);
                        _stoptime = tempStopTime.ToString();

                        loadingInfoService.UpdateStopTimeFor(_stoptime, _uwsid);
                        sampleInfoService.UpdateStopTimeFor(_stoptime, _newNsid);
                    }
                }

                var raInfoService = new RAInfoService(_connectionStr);
                _advisorEmail = raInfoService.GetQueryValueFor("advisoremail");

                Log.InfoFormat("_loadType: {0}",_loadType);
                

                //Check if data is System.
                if (_loadType.Equals("SYSTEM")) {
                    //TODO: Check for NTS System
                    var ntsOrder = false;
                    if (systemTbl.IsNTSSystemFor(_tempSystemSerial)) {
                        ntsOrder = true;
                    }
                    if (!ConnectionString.IsLocalAnalyst) {
                        if (!ntsOrder) {
#region Check Email

                            //1. Get all the users for this system.
                            var cusAnalystService = new CusAnalystService(_connectionStr);
                            var customerList = cusAnalystService.GetCustomersFor(companyID);

                            //2. loop through customerID and check notification.
                            var notifications = new NotificationPreferenceService(ConnectionString.ConnectionStringDB);
                            foreach (var i in customerList) {
                                //3. Check if user has Every Load notificatin.
                                var notification = notifications.CheckIsEveryLoadFor(_tempSystemSerial, i);
                                if (notification.IsEveryLoad) {
                                    var startTime = Convert.ToDateTime(_starttime);
                                    var stopTime = Convert.ToDateTime(_stoptime);

                                    var dailyEmail = new DailyEmail(ConnectionString.EmailServer, ConnectionString.ServerPath,
                                        ConnectionString.EmailPort, ConnectionString.EmailUser, ConnectionString.EmailPassword, ConnectionString.EmailAuthentication,
                                        ConnectionString.SystemLocation, ConnectionString.AdvisorEmail, ConnectionString.ConnectionStringDB, ConnectionString.ConnectionStringSPAM,
                                        ConnectionString.ConnectionStringTrend, ConnectionString.SupportEmail, ConnectionString.WebSite,
                                        ConnectionString.EmailIsSSL, ConnectionString.IsLocalAnalyst,
                ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);

                                    var cust = new CusAnalystService(ConnectionString.ConnectionStringDB);
                                    var email = cust.GetEmailAddressFor(i);
                                    var emailList = new List<string> {email};

                                    dailyEmail.SendLoadEmail(startTime, stopTime, emailList, _tempSystemSerial, _nodename, ConnectionString.SystemLocation, ConnectionString.DatabasePrefix);
                                }
                                else if (notification.IsEmailCritical || notification.IsEmailMajor) {
                                    bool criticalAlert = false;
                                    bool majorAlert = false;
                                    var startTime = Convert.ToDateTime(_starttime);
                                    var stopTime = Convert.ToDateTime(_stoptime);

                                    var databaseMappingService = new DatabaseMappingService(_connectionStr);
                                    string connectionStringSystem = databaseMappingService.GetConnectionStringFor(_tempSystemSerial);
                                    if (connectionStringSystem == "") {
                                        connectionStringSystem = _connectionStrSPAM;
                                    }

                                    var consolidatedService = new ConsolidatedAlertService(_connectionStr, connectionStringSystem);

                                    if (notification.IsEmailCritical) {
                                        criticalAlert = consolidatedService.CheckAlertFor(startTime, stopTime, _tempSystemSerial, i, Alert.Alerts.Critical);
                                    }
                                    if (notification.IsEmailMajor) {
                                        majorAlert = consolidatedService.CheckAlertFor(startTime, stopTime, _tempSystemSerial, i, Alert.Alerts.Major);
                                    }

                                    if (criticalAlert || majorAlert) {
                                        //Send Alert Email.

                                        var dailyEmail = new DailyEmail(ConnectionString.EmailServer, ConnectionString.ServerPath,
                                            ConnectionString.EmailPort, ConnectionString.EmailUser, ConnectionString.EmailPassword, ConnectionString.EmailAuthentication,
                                            ConnectionString.SystemLocation, ConnectionString.AdvisorEmail, ConnectionString.ConnectionStringDB, ConnectionString.ConnectionStringSPAM,
                                            ConnectionString.ConnectionStringTrend, ConnectionString.SupportEmail, ConnectionString.WebSite,
                                            ConnectionString.EmailIsSSL, ConnectionString.IsLocalAnalyst,
                                            ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);


                                        var cust = new CusAnalystService(ConnectionString.ConnectionStringDB);
                                        var email = cust.GetEmailAddressFor(i);
                                        var emailList = new List<string> {email};

                                        dailyEmail.SendLoadEmail(startTime, stopTime, emailList, _tempSystemSerial, _nodename, ConnectionString.SystemLocation, ConnectionString.DatabasePrefix);
                                    }
                                }
                            }
#endregion
                        }
                        else {
                            if (!_ntsID.Equals(0)) {
                                //Check if all the files has been loaded.
                                var loaded = uploadMessageService.CheckIfAllFilesLoaded(_ntsID);
                                if (loaded) {
                                    //Send Load complete email to customers.
                                    var uploadService = new UploadService(_connectionStr);
                                    var customerID = uploadService.GetCustomerIDFor(_ntsID);
                                    var startTime = Convert.ToDateTime(_starttime);
                                    var stopTime = Convert.ToDateTime(_stoptime);

                                    var dailyEmail = new DailyEmail(ConnectionString.EmailServer, ConnectionString.ServerPath,
                                        ConnectionString.EmailPort, ConnectionString.EmailUser, ConnectionString.EmailPassword, ConnectionString.EmailAuthentication,
                                        ConnectionString.SystemLocation, ConnectionString.AdvisorEmail, ConnectionString.ConnectionStringDB, ConnectionString.ConnectionStringSPAM,
                                        ConnectionString.ConnectionStringTrend, ConnectionString.SupportEmail, ConnectionString.WebSite,
                                        ConnectionString.EmailIsSSL, ConnectionString.IsLocalAnalyst,
                                        ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);

                                    var cust = new CusAnalystService(ConnectionString.ConnectionStringDB);
                                    var email = cust.GetEmailAddressFor(customerID);
                                    var emailList = new List<string> {email};

                                    dailyEmail.SendLoadEmail(startTime, stopTime, emailList, _tempSystemSerial, _nodename, ConnectionString.SystemLocation, ConnectionString.DatabasePrefix);
                                }
                            }
                        }
                    }
                }
                loadingInfoService.UpdateLoadingStatusFor(_uwsid, "Sned");
            }
            catch (Exception e) {
                try {
                    Log.Error("***********************************************************");
                    Log.ErrorFormat("Job file - {0}, uwsID {1}, Error: {2}",
                        _filename, _uwsid, e);

                    if (ConnectionString.IsLocalAnalyst)
                    {
                        var email = new EmailToSupport(ConnectionString.AdvisorEmail, ConnectionString.SupportEmail,
                            ConnectionString.WebSite,
                            ConnectionString.EmailServer, ConnectionString.EmailPort, ConnectionString.EmailUser,
                            ConnectionString.EmailPassword, ConnectionString.EmailAuthentication,
                            ConnectionString.SystemLocation, ConnectionString.ServerPath,
                            ConnectionString.EmailIsSSL, ConnectionString.IsLocalAnalyst,
                            ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                        email.SendLocalAnalystErrorMessageEmail("UWS Loader - JobProcessorUWS.cs", e.Message, LicenseService.GetProductName(ConnectionString.ConnectionStringDB));
                    }
                    else
                    {
                        //Put log on S3
                        var amazonOperations = new AmazonOperations();
						StringBuilder errorMessage = new StringBuilder();
						errorMessage.Append("Source: ProcessJob:JobProcessorUWS \r\n");
						errorMessage.Append("_uwsid: " + _uwsid + "\r\n");
						errorMessage.Append("_loadType: " + _loadType + "\r\n");
						errorMessage.Append("Message: " + e.Message + "\r\n");
						errorMessage.Append("StackTrace: " + e.StackTrace + "\r\n");
						amazonOperations.WriteErrorQueue(errorMessage.ToString());
                        var email = new EmailHelper();
                        email.SendErrorEmail("Unknown error: " + e.Message);
                    }
                    //Didn't complete the loading. Unknow error.
                    LoadingInfoService loadingInfo;
                    if (_uwsid != 0) {
                        loadingInfo = new LoadingInfoService(_connectionStr);
                        loadingInfo.UpdateLoadingStatusFor(_uwsid, "Fail");
                    }
                    else {
                        //Get uwsid.
                        string[] split = _filename.Split(new[] {
                            '_'
                        });
                        if (split[0].Substring(3) != "auto") {
                            _uwsid = Convert.ToInt32(split[0].Substring(3));
                            loadingInfo = new LoadingInfoService(_connectionStr);
                            loadingInfo.UpdateLoadingStatusFor(_uwsid, "Fail");
                        }
                    }
                }
                catch {
                }
            }
            finally {
                try {
                    if (!_uwsFileUnzip.Contains(".402")) {
                        if (File.Exists(_uwsFileUnzip)) {
                            File.Delete(_uwsFileUnzip);
                        }
                    }
                }
                catch {
                    //Do nothing.
                }

                Log.Info("Delete from LoadingStatus.");
                
                //Change CurrentLoad value.
                var process = new JobProcess();

                try {
                    Log.InfoFormat("Calling InsertLoadingStatusDetail FileName: {0}",_filename);
                    
                    bool insertChecker = process.InsertLoadingStatusDetail(false, _filename, 0, "SYSTEM", _tempSystemSerial);

                    Log.InfoFormat("insertChecker: {0}",insertChecker);
                    
                    if (insertChecker) {
                        if (!_filename.StartsWith("DO"))
                            process.ChangeStatus(-1);
                    }
                }
                catch (Exception ex) {
                    Log.ErrorFormat("-Error clearing LoadingStatusDetail");
                    Log.ErrorFormat("-Error Log: {0}",ex.Message);
                    
                }

                try {
                    if (_result) {
                        System_tblService systemTblService = new System_tblService(_connectionStr);
                        var archiveRetension = systemTblService.GetArchiveRetensionValueFor(_tempSystemSerial);
                        if (archiveRetension < 1 && File.Exists(_uwsFileUnzip)) {
                            File.Delete(_uwsFileUnzip);
                        }
                    }
                }
                catch (Exception ex) {
                    Log.ErrorFormat("File Delete Error: {0}",ex.Message);
                    
                }

                currentProc.Refresh();
                Log.InfoFormat("Before Calling GC Memory :" + currentProc.PrivateMemorySize64);
                

                //Force Garge Collector to run.
                GC.Collect();

                currentProc.Refresh();
                Log.InfoFormat("After Calling GC Memory :" + currentProc.PrivateMemorySize64);
            }
        }

        //public int RetValue() {
        //    return 0;
        //}
        public void SendLoadFailEmail() {
            //close the log file.
            var emailText = new StringBuilder();
            emailText.Append("<br>An analysis with the following characteristics failed to load at " + DateTime.Now + ":");
            emailText.Append("<UL>");
            emailText.Append("	<LI>");
            emailText.Append("		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>Node: " + _nodename + "</DIV>");
            emailText.Append("	<LI>");
            emailText.Append("		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>Start Time: " + _starttime + "</DIV>");
            emailText.Append("	<LI>");
            emailText.Append("		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>End Time: " + _stoptime + "</DIV>");
            emailText.Append("	</LI>");
            emailText.Append("</UL>");
            //Send Email to us.
            var email = new EmailHelper();
            email.SendErrorEmail(emailText.ToString());
        }

        /// <summary>
        /// Check to see if load should be delay.
        /// </summary>
        private void CheckDelay() {
            var dtBeforeInQueTime = new DateTime();

            var statusDetail = new LoadingStatusDetailService(_connectionStr);
            IDictionary<int, DateTime> loadingTime = statusDetail.GetProcessingTimeFor(_filename, _tempSystemSerial);

            if (loadingTime.Count > 0) {
                KeyValuePair<int, DateTime> kv = loadingTime.First();
                int loadingQueID = kv.Key;
                DateTime dtInQueTime = kv.Value;

                //subtract loadingQueID by one to get preview data.
                if (loadingQueID != 0) {
                    loadingQueID--;
                    dtBeforeInQueTime = statusDetail.GetProcessingTimeFor(loadingQueID);
                }

                if (dtBeforeInQueTime != DateTime.MinValue) {
                    //Check if the datetime diff is more then one min.
                    TimeSpan tsTimeDiff = dtInQueTime - dtBeforeInQueTime;
                    if (tsTimeDiff.Minutes < 1) {
                        //sleep for 30 sec.
                        Thread.Sleep(30000);
                    }
                }
            }
        }

        /// <summary>
        /// Check the loading period and the sample type for the load.
        /// </summary>
        /// <param name="uwsid"> The UWS ID for the load. This ID is used in LoadingInfo table</param>
        private void GetLoadPeriod(int uwsid) {
            var loadingInfoService = new LoadingInfoService(_connectionStr);
            var loadingInfoView = loadingInfoService.GetLoadingPeriodFor(uwsid.ToString());

            if (loadingInfoView.StartTime != DateTime.MinValue) {
                _loadStartTime = loadingInfoView.StartTime;
                Log.InfoFormat("Got StartTime : {0}",_loadStartTime);
                
            }
            else {
                Log.Info("Got StartTime NULL");
                
            }
            if (loadingInfoView.StopTime != DateTime.MinValue) {
                _loadEndTime = loadingInfoView.StopTime;
                Log.InfoFormat("Got EndTime : {0}",_loadEndTime);
                
            }
            else {
                Log.Info("Got EndTime NULL");
                
            }
            //-1 means the value is null
            if (loadingInfoView.SampleType != -1) {
                int type = loadingInfoView.SampleType;
                Log.InfoFormat("Got SampleType : {0}",type);
                
                if (type == 4) {
                    _isSystem = true;
                    _fileType = 4;
                }
                else {
                    _fileType = 3;
                }
            }
            else {
                Log.Info("Got SampleType NULL");
                
            }
        }

        /// <summary>
        /// Check if the system has the license for RemoteAnalyst.
        /// </summary>
        private bool GetLicenseInfo() {
            Log.Info("Checking License!");
            //Get End Date.
            try {
                var systemTbl = new System_tblService(_connectionStr);
                //System_tblServices systemTblServices = systemTbl.GetEndDate(tempSystemSerial);
                IDictionary<string, string> endDate = systemTbl.GetEndDateFor(_tempSystemSerial);
                foreach (KeyValuePair<string, string> kv in endDate) {
                    if (kv.Value.Length == 0) {
                        Log.Info("No License");
                        
                        return false;
                    }
                    if (kv.Value != "") {
                        //Decrypt the End date.
                        var decrypt = new Decrypt();

                        string decryptInfo = decrypt.strDESDecrypt(kv.Value);
                        string decryptSystemSerial = decryptInfo.Split(' ')[0].Trim();
                        string decryptDate = decryptInfo.Split(' ')[1].Trim(); //get the date [0] is systemName.

                        if (decryptSystemSerial == _tempSystemSerial) {
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

                            int timeZoneIndex = systemTbl.GetTimeZoneFor(_tempSystemSerial);
                            DateTime systemLocalTime = TimeZoneInformation.ToLocalTime(timeZoneIndex, DateTime.Now);

                            //Check end date. Set 7 days grace days
                            if (planEndDate.Date < systemLocalTime.Date)
                            {
#if EVALUATION_COPY
                                Log.Info("License Expired");
#else
                                Log.Info("License Expired and out of 7 days grace period");
#endif
                                return false;
                            }
                        }
                        else {
                            Log.Info("Invaid or different SystemSerial");
                            
                            return false;
                        }
                    }
                    else {
                        Log.Info("No System Information Obtained");
                        
                        return false;
                    }
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("Unexpected Error: {0}",ex.Message);
                if (ConnectionString.IsLocalAnalyst)
                {
                    var email = new EmailToSupport(ConnectionString.AdvisorEmail, ConnectionString.SupportEmail,
                        ConnectionString.WebSite,
                        ConnectionString.EmailServer, ConnectionString.EmailPort, ConnectionString.EmailUser,
                        ConnectionString.EmailPassword, ConnectionString.EmailAuthentication,
                        ConnectionString.SystemLocation, ConnectionString.ServerPath,
                        ConnectionString.EmailIsSSL, ConnectionString.IsLocalAnalyst,
                        ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                    email.SendLocalAnalystErrorMessageEmail("UWS Loader - JobProcessorUWS.cs", ex.Message, LicenseService.GetProductName(ConnectionString.ConnectionStringDB));
                }
                else {
                    var amazonOperations = new AmazonOperations();
					StringBuilder errorMessage = new StringBuilder();
					errorMessage.Append("Source: ProcessJob:JobProcessorUWSUnexpected Error \r\n");
					errorMessage.Append("_uwsid: " + _uwsid + "\r\n");
					errorMessage.Append("_loadType: " + _loadType + "\r\n");
					errorMessage.Append("Message: " + ex.Message + "\r\n");
					errorMessage.Append("StackTrace: " + ex.StackTrace + "\r\n");
					amazonOperations.WriteErrorQueue(errorMessage.ToString());
                }

                
                //If fail to get a respone from the server exit the loading.
                return false;
            }
            Log.Info("License okay!");
            return true;
        }

        /// <summary>
        /// Create the full file path.
        /// Read the UWS file to get it's file type.
        /// Update the LoadingStatus table.
        /// </summary>
        private void UWSAutoLoad() {
            try {
                _userEmail = "";

                //Fixing System Serial # to prepend zeroes if needed and trimming whitespace
                int sysSerialLength = _tempSystemSerial.Length;
                if (sysSerialLength < 6) {
                    //Prepend Leading Zeroes
                    for (int i = 0; i < (6 - sysSerialLength); i++) {
                        _tempSystemSerial = "0" + _tempSystemSerial;
                    }
                }
                //filepath = pathbase + "\\customer\\" + userEmail + "\\" + tempStr;
                string systemLocation = ConnectionString.SystemLocation;
                _filepath = systemLocation + _tempSystemSerial + "\\" + _filename;

                //Ryan Ji
                //Move the process of CPU Info file to another function.

                if (File.Exists(_filepath)) {
                    _finfo = new FileInfo(_filepath);
                    _fsize = _finfo.Length;
                }

                /*//Get File Type.
                using (var sr = new StreamReader(filepath)) {
                    string line = sr.ReadLine();
                    line += sr.ReadLine();
                    line += sr.ReadLine();
                    line += sr.ReadLine();
                    line += sr.ReadLine();

                    //Check the if UWS File is SYSTEM OR PATHWAY.
                    //there is a "Collection State String" on second line of UWS File for Pathway.
                    //If watch it's a Pathway collection and if not it's a System collection.
                    if (line.IndexOf("COLLECTION State String") == -1) {
                        fileType = 4;
                    }
                    else {
                        fileType = 3;
                    }
                }*/

                using (var stream = new FileStream(_filepath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    using (var reader = new BinaryReader(stream)) {
                        var myEncoding = new ASCIIEncoding();
                        reader.BaseStream.Seek(1096, SeekOrigin.Begin);
                        byte[] keywordByte = reader.ReadBytes(24);
                        string keyword = myEncoding.GetString(keywordByte).Trim();

                        if (keyword.IndexOf("COLLECTION State String") == -1)
                            _fileType = 4;
                        else
                            _fileType = 3;
                    }
                }
                
                _uwsfile = _filepath;
                _uwsid = _tempUWSID;

                var loadingInfo = new LoadingInfoService(_connectionStr);
                loadingInfo.UpdateFor(_filename, _tempSystemSerial, _fsize.ToString(), _fileType.ToString(), _tempUWSID.ToString());
            }
            catch (Exception ex) {
                Log.ErrorFormat("**** UWSAutoLoad Error: {0}",ex.Message);
                
            }
        }

        private void UWSManuLoad() {
            try {
                var loadingInfoService = new LoadingInfoService(_connectionStr);
                IDictionary<string, string> loadingInfoView = loadingInfoService.GetSystemInfoFor(_tempUWSID.ToString());

                foreach (KeyValuePair<string, string> kv in loadingInfoView) {
                    if (kv.Key != "" || kv.Value != "") {
                        _tempSystemSerial = kv.Key;
                        _uwsfile = kv.Value;

                        loadingInfoService.UpdateFor(_tempUWSID.ToString());
                        _uwsid = _tempUWSID;
                    }
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("**** UWSManuLoad Error: {0}",ex.Message);
                
            }
        }
    }
}