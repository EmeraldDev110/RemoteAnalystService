using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Ionic.Zip;
using log4net;
using RemoteAnalyst.AWS.EC2;
using RemoteAnalyst.AWS.Glacier;
using RemoteAnalyst.AWS.Infrastructure;
using RemoteAnalyst.AWS.S3;
using RemoteAnalyst.BusinessLogic.BLL;
using RemoteAnalyst.BusinessLogic.Enums;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.UWSLoader.Email;
using RemoteAnalyst.UWSLoader.JobProcessor;
using RemoteAnalyst.UWSLoader.SPAM.BLL;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.Util;
using System.Text;

namespace RemoteAnalyst.UWSLoader.BLL.Process {
    /// <summary>
    /// ProcessData class get the data file form S3.
    /// Call job processor class to load the data.
    /// </summary>
    public class ProcessData {
        private static readonly ILog Log = LogManager.GetLogger("ProcessLoad");
        private readonly string _s3Location = string.Empty;
        private string _loadType = string.Empty;
        private string _systemSerial = string.Empty;
        private DateTime _startTime;
        private DateTime _stopTime;
        private int _uwsId;

        private string _saveLocation = "";
        private int _ntsID;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="loadType"> Type of the load.</param>
        /// <param name="systemSerial"> System serial number.</param>
        /// <param name="location"> S3 location of the data file.</param>
        public ProcessData(string loadType, string systemSerial, string location, int uwsId, int ntsID = 0) {
            _loadType = loadType;
            _systemSerial = systemSerial;
            _s3Location = location;
            _uwsId = uwsId;
            _ntsID = ntsID;
        }

        public ProcessData(string loadType, string systemSerial, List<string> archiveID, DateTime startTime, DateTime stopTime, bool isGlacier, int uwsId) {
            _loadType = loadType;
            _systemSerial = systemSerial;
            _startTime = startTime;
            _stopTime = stopTime;
            _uwsId = uwsId;
        }

        /// <summary>
        /// Get the data file from S3.
        /// Process all the load type except DISCOPEN load.
        /// Check if the load is a duplicated one.
        /// Call JobProcessorData to process SYSTEM and PATHWAY data. Call JobProcessorDisk to process DISK data.
        /// </summary>
        public void StartProcess() {
            var watch = new Watcher();
            string saveFolder = ConnectionString.ServerPath;
            string dataFileName = "";
            try {
                Log.InfoFormat("_uwsId: {0}", _uwsId);
                Log.InfoFormat("loadType: {0}", _loadType);
                Log.InfoFormat("s3location: {0}", _s3Location);
                int tempUWSID = 0;
                if (_uwsId == 0)
                    tempUWSID = watch.GetMaxUWSID();
                else
                    tempUWSID = _uwsId;

                //var archiveId = "";

                bool duplicated = false;

                if (_loadType == "SYSTEM" || _loadType == "PATHWAY" || _loadType == "UWS") {
                        //if the file is from Glacier, go to local folder to find the file name
                    if (ConnectionString.IsLocalAnalyst) {
                        Log.InfoFormat("_s3Location: {0}", _s3Location);
                            

                        if (File.Exists(_s3Location)) {
                            var fileInfo = new FileInfo(_s3Location);
                            dataFileName = fileInfo.Name;
                        }
                    }
                    else
                    {
                        var temp = _s3Location.Split('|');
                        if (temp.Length == 1)
                            dataFileName = _s3Location.Split('/')[2];
                        else
                        {
                            dataFileName = temp[0].Split('/')[2];
                        }
                    }

                    //Glacier, don't need check duplicate , no entry
                    //duplicated = watch.CheckDuplicatedUWS(dataFileName);
                    //if (!duplicated) {
                    //Get UWS file
                    try
                    {
                        
                        if (ConnectionString.IsLocalAnalyst) {
                            if (File.Exists(_s3Location)) {
                                if (!Directory.Exists(saveFolder + "Systems\\" + _systemSerial + "\\")) {
                                    Directory.CreateDirectory(saveFolder + "Systems\\" + _systemSerial + "\\");
                                }
                                var fileInfo = new FileInfo(_s3Location);
                                _saveLocation = saveFolder + "Systems\\" + _systemSerial + "\\" + fileInfo.Name;
                                Log.InfoFormat("SaveLocation: {0}", _saveLocation);
                                    
                                if (File.Exists(_saveLocation))
                                    File.Delete(_saveLocation);

                                //fileInfo.MoveTo(_saveLocation);
                                fileInfo.CopyTo(_saveLocation);
                                Log.InfoFormat("_ntsID: {0}", _ntsID);
                                    

                                //Once download, delete the original file.
                                fileInfo.Delete();
                            }
                        }
                        else
                        {
                            Log.InfoFormat("Start Downloading Data File: {0}", dataFileName.Trim());

                            var retry = 0;
                            while (retry < 3)
                            {
                                try
                                {
                                    //Check the file extention.
                                    //If .zip, go to s3-prod-remoteanalyst-uws-files.
                                    //After download, unzip the file.
                                    if (_s3Location.ToLower().EndsWith("zip"))
                                    {
                                        //var s3UwsFileLocation = "s3-prod-remoteanalyst-uws-files";
                                        Log.InfoFormat("S3Bucket: {0}", ConnectionString.S3UWS);

                                        var s3 = new AmazonS3(ConnectionString.S3UWS);
                                        var tempSavelocation = s3.ReadS3(_s3Location, saveFolder);

                                        var fileInfo = new FileInfo(tempSavelocation);
                                        var saveFolderLocation = fileInfo.DirectoryName;

                                        //Unzip the file and update _saveLocation, dataFileName.
                                        using (var zip = ZipFile.Read(tempSavelocation))
                                        {
                                            var entry = zip.First();
                                            entry.Extract(saveFolderLocation, ExtractExistingFileAction.OverwriteSilently);
                                            _saveLocation = saveFolderLocation + "\\" + entry.FileName;

                                            dataFileName = entry.FileName;
                                        }

                                        //Delete the zip file.
                                        try
                                        {
                                            File.Delete(tempSavelocation);
                                        }
                                        catch (Exception ex)
                                        {
                                            Log.ErrorFormat("File Delete Error: {0}", ex.Message);

                                        }
                                    }
                                    else
                                    {
                                        Log.InfoFormat("S3Bucket: {0}", ConnectionString.S3FTP);

                                        var s3 = new AmazonS3(ConnectionString.S3FTP);
                                        _saveLocation = s3.ReadS3(_s3Location, saveFolder);
                                    }
                                    retry = 3;
                                }
                                catch (Exception ex)
                                {
                                    Thread.Sleep(30000);
                                    retry++;

                                    if (retry == 3)
                                        throw new Exception(ex.Message);
                                }
                            }
                            Log.InfoFormat("Finish Downloading Data File SaveLocation: {0}", _saveLocation);

                        }
                    }
                    catch (Exception ex) {
                        Log.ErrorFormat("************************************************");
                        Log.ErrorFormat("AWS Error: Could not get the file from S3. S3 Location: {0}, Error: {1}", 
                                            _s3Location, ex);
                        if (ConnectionString.IsLocalAnalyst) {
                            var email = new EmailToSupport(ConnectionString.AdvisorEmail, ConnectionString.SupportEmail,
                                ConnectionString.WebSite,
                                ConnectionString.EmailServer, ConnectionString.EmailPort, ConnectionString.EmailUser,
                                ConnectionString.EmailPassword, ConnectionString.EmailAuthentication,
                                ConnectionString.SystemLocation, ConnectionString.ServerPath,
                                ConnectionString.EmailIsSSL,
                                ConnectionString.IsLocalAnalyst, ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                            email.SendLocalAnalystErrorMessageEmail("UWSLoader - ProcessData.cs", ex.Message, LicenseService.GetProductName(ConnectionString.ConnectionStringDB));
                        }
                        else
                        {
                            var amazonOperations = new AmazonOperations();
                            StringBuilder errorMessage = new StringBuilder();
                            errorMessage.Append("Source: ProcessData:StartProcess \r\n");
                            errorMessage.Append("_s3Location: " + _s3Location + "\r\n");
                            errorMessage.Append("_loadType: " + _loadType + "\r\n");
                            errorMessage.Append("_systemSerial: " + _systemSerial + "\r\n");
                            errorMessage.Append("_startTime: " + _startTime + "\r\n");
                            errorMessage.Append("_stopTime: " + _stopTime + "\r\n");
                            errorMessage.Append("Message: " + ex.Message + "\r\n");
                            errorMessage.Append("StackTrace: " + ex.StackTrace + "\r\n");
                            amazonOperations.WriteErrorQueue(errorMessage.ToString());
                        }
                        return;
                        //Write error queue
                    }
                    if (dataFileName.StartsWith("DO")) {
                        var processUWS = new JobProcessorUWS(dataFileName, tempUWSID, _systemSerial, _loadType, 0);
                        try {
                            Log.Info("DISCOPEN file, start processing");
                                
                            processUWS.ProcessJob();
                        }
                        catch (Exception ex) {
                            Log.ErrorFormat("ERROR: {0}", ex);
                                
                        }
                        finally {
                            Log.Info("Finish Processing DISCOPEN file");                                
                        }
                        return;
                    }

                    Log.InfoFormat("_loadType: {0}", _loadType);
                        
                    if (_loadType == "UWS") {
                        int customerID = 0;
                        //Check if it's valid UWS File.
                        var uwsInfo = new UWSFileInfo();
                        bool isUWS = uwsInfo.IsValidUWSFile(_saveLocation, ConnectionString.ConnectionStringDB);
                        Log.InfoFormat("isUWS: {0}", isUWS);
                            

                        if (isUWS) {
                            #region
                            //Get File Type.
                            UWS.Types type = uwsInfo.UwsFileVersionNew(_saveLocation);

                            //For UWS message, second line is customerID.
                            //int.TryParse(_systemSerial, out customerID);
                            //Log.InfoFormat("customerID: {0}", customerID);
                            //

                            //Get System Serial.
                            _systemSerial = uwsInfo.UWSSystemSerial(_saveLocation, type);
                            Log.InfoFormat("SystemSerial: {0}", _systemSerial);
                                

                            //Check if System belongs to the customer.
                            //var systemTable = new System_tblService(ConnectionString.ConnectionStringDB);
                            //bool isUserSystem = systemTable.IsUserSystemFor(customerID, _systemSerial);

                            //Log.InfoFormat("isUserSystem: {0}", isUserSystem);
                            //
                            bool isUserSystem = true;
                            if (!isUserSystem) {
                                //Delete Downloaded File.
                                if (File.Exists(_saveLocation))

                                    File.Delete(_saveLocation);

                                if (customerID == 0)
                                    int.TryParse(_systemSerial, out customerID);

                                var custInfo = new CusAnalystService(ConnectionString.ConnectionStringDB);

                                //Send Email to Customer.
                                //Get Customer's Email.
                                string custEmail = custInfo.GetEmailAddressFor(customerID);
                                //Get Customer Name.
                                string custName = custInfo.GetUserNameFor(custEmail);

                                var email = new EmailHelper();
                                email.SendErrorEmail(custEmail, custName, UploadError.Types.NoMatch);
                                return;
                            }
                            #endregion
                        }
                        else {
                            //Delete Downloaded File.
                            if (File.Exists(_saveLocation))
                                File.Delete(_saveLocation);

                            //Send Email to Customer.
                            if (customerID == 0)
                                int.TryParse(_systemSerial, out customerID);

                            var custInfo = new CusAnalystService(ConnectionString.ConnectionStringDB);
                            //Get Customer's Email.
                            string custEmail = custInfo.GetEmailAddressFor(customerID);
                            //Get Customer Name.
                            string custName = custInfo.GetUserNameFor(custEmail);

                            var email = new EmailHelper();
                            email.SendErrorEmail(custEmail, custName, UploadError.Types.Fail);
                            return;
                        }

                        if (!ConnectionString.IsLocalAnalyst) {
                            if (File.Exists(saveFolder + "\\Systems\\" + _systemSerial + "\\" + dataFileName))
                                File.Delete(saveFolder + "\\Systems\\" + _systemSerial + "\\" + dataFileName);
                            //Move the File to System Folder and update _saveLocation.
                            if (File.Exists(_saveLocation))
                                File.Move(_saveLocation, saveFolder + "\\Systems\\" + _systemSerial + "\\" + dataFileName);

                            _saveLocation = saveFolder + "\\Systems\\" + _systemSerial + "\\" + dataFileName;
                        }

                        _loadType = "SYSTEM";   //Change the load type to System.
                    }
                    duplicated = watch.CheckDuplicateFromSampleInfo(_systemSerial, _saveLocation, Log);
                    //}
                }
                    
                if (!duplicated) {
                    watch.InsertLoadingInfo(_saveLocation, tempUWSID);
                }
                
                if (_loadType == "DISK") {
                    if (!ConnectionString.IsLocalAnalyst) {
                        try {
                            var s3 = new AmazonS3(ConnectionString.S3FTP);
                            _saveLocation = s3.ReadS3(_s3Location, saveFolder);
                        }
                        catch (Exception ex) {
                            Log.ErrorFormat("************************************************");
                            Log.ErrorFormat("AWS Error: Could not get the file from S3, S3 Location: {0}, Error: {1}", 
                                                _s3Location, ex);
                            if (ConnectionString.IsLocalAnalyst)
                            {
                                var email = new EmailToSupport(ConnectionString.AdvisorEmail, ConnectionString.SupportEmail,
                                    ConnectionString.WebSite,
                                    ConnectionString.EmailServer, ConnectionString.EmailPort, ConnectionString.EmailUser,
                                    ConnectionString.EmailPassword, ConnectionString.EmailAuthentication,
                                    ConnectionString.SystemLocation, ConnectionString.ServerPath,
                                    ConnectionString.EmailIsSSL,
                                    ConnectionString.IsLocalAnalyst, ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                                email.SendLocalAnalystErrorMessageEmail("UWSLoader - ProcessData.cs", ex.Message, LicenseService.GetProductName(ConnectionString.ConnectionStringDB));
                            }
                            else {
                                var amazonOperations = new AmazonOperations();
                                StringBuilder errorMessage = new StringBuilder();
                                errorMessage.Append("Source: ProcessData:StartProcess 2 \r\n");
                                errorMessage.Append("_s3Location: " + _s3Location + "\r\n");
                                errorMessage.Append("_loadType: " + _loadType + "\r\n");
                                errorMessage.Append("_systemSerial: " + _systemSerial + "\r\n");
                                errorMessage.Append("_startTime: " + _startTime + "\r\n");
                                errorMessage.Append("_stopTime: " + _stopTime + "\r\n");
                                errorMessage.Append("Message: " + ex.Message + "\r\n");
                                errorMessage.Append("StackTrace: " + ex.StackTrace + "\r\n");
                                amazonOperations.WriteErrorQueue(errorMessage.ToString());
                            }
                            return;
                            //Write error queue
                        }
                        dataFileName = _s3Location.Split('|')[0].Split('/')[2];
                    }
                    else if (File.Exists(_s3Location)) {
                        if (File.Exists(saveFolder + "\\Systems\\" + _systemSerial + "\\" + dataFileName))
                            File.Delete(saveFolder + "\\Systems\\" + _systemSerial + "\\" + dataFileName);

                        var fileInfo = new FileInfo(_s3Location);
                        fileInfo.CopyTo(saveFolder + "\\Systems\\" + _systemSerial + "\\" + fileInfo.Name, true);
                        _saveLocation = saveFolder + "\\Systems\\" + _systemSerial + "\\" + fileInfo.Name;
                        dataFileName = fileInfo.Name;
                    }
                    else {
                        _saveLocation = _s3Location;
                        if (File.Exists(_saveLocation))
                        {
                            var fi = new FileInfo(_saveLocation);
                            dataFileName = fi.Name;
                        }
                    }
                    watch.InsertLoadingInfoDisk(_systemSerial, dataFileName);
                }

                if (!duplicated) {
                    string instanceID = "";
                    if (!ConnectionString.IsLocalAnalyst)
                    {
                        var ec2 = new AmazonEC2();
                        instanceID = ec2.GetEC2ID();
                    }

                    int queCount = watch.CurrentLoadingQue(instanceID);
                    //Check to see if ArrayList is 0 and currentLoad is less then Max Load.
                    //if ((watch.CheckLoading(instanceID) && queCount == 0)) {
                    try {
                        if (_loadType == "SYSTEM" || _loadType == "PATHWAY") {
                            bool updateChecker = false;
                            //Check if load discopen or not

                            var process = new JobProcess();
                            var processUWS = new JobProcessorUWS(dataFileName, tempUWSID, _systemSerial, _loadType, _ntsID);

                            //Get file size and update the LoadingStatusDetail.
                            var fileInfo = new UWSFileInfo();
                            string fullFilePath = ConnectionString.SystemLocation + _systemSerial + "\\" + dataFileName;
                            var fileSize = fileInfo.GetFileSize(fullFilePath);
                            var loadingStatusDetailService = new LoadingStatusDetailService(ConnectionString.ConnectionStringDB);
                            loadingStatusDetailService.UpdateFileSizeFor(dataFileName, fileSize);

                            process.ChangeStatus(+1);
                            processUWS.ProcessJob();
                        }
                        else {
                            bool updateChecker = false;

                            var process = new JobProcess();
                            bool insertChecker = process.InsertLoadingStatusDetail(true, dataFileName, tempUWSID, _loadType, _systemSerial);
                            if (insertChecker) {
                                updateChecker = process.UpdateLoadingStatusDetail(dataFileName, _systemSerial);
                            }
                            
                            if (updateChecker) {
                                process.ChangeStatus(+1);
                            }
                            //Looks for the first char of the file name.
                            var jobDisk = new JobProcessorDisk(dataFileName, tempUWSID, _systemSerial);
                            jobDisk.ProcessJobDisk();
                        }
                    }
                    catch (Exception ex) {
                        Log.ErrorFormat("Error when processing the data, Load Type: {0}, SystemSerial: {1}, FileName: {2}, Error: {3}", 
                            _loadType, _systemSerial, dataFileName, ex);
                        
                        if (!ConnectionString.IsLocalAnalyst) {
                            var amazonOperations = new AmazonOperations();
                            StringBuilder errorMessage = new StringBuilder();
                            errorMessage.Append("Source: ProcessData:StartProcess 3 \r\n");
                            errorMessage.Append("_s3Location: " + _s3Location + "\r\n");
                            errorMessage.Append("_loadType: " + _loadType + "\r\n");
                            errorMessage.Append("_systemSerial: " + _systemSerial + "\r\n");
                            errorMessage.Append("_startTime: " + _startTime + "\r\n");
                            errorMessage.Append("_stopTime: " + _stopTime + "\r\n");
                            errorMessage.Append("Message: " + ex.Message + "\r\n");
                            errorMessage.Append("StackTrace: " + ex.StackTrace + "\r\n");
                            amazonOperations.WriteErrorQueue(errorMessage.ToString());
                        }
                    }
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("Failed to load Data {0}", ex);
                if (!ConnectionString.IsLocalAnalyst) {
                    var amazonOperations = new AmazonOperations();
                    StringBuilder errorMessage = new StringBuilder();
                    errorMessage.Append("Source: ProcessData:StartProcess 3 \r\n");
                    errorMessage.Append("_s3Location: " + _s3Location + "\r\n");
                    errorMessage.Append("_loadType: " + _loadType + "\r\n");
                    errorMessage.Append("_systemSerial: " + _systemSerial + "\r\n");
                    errorMessage.Append("_startTime: " + _startTime + "\r\n");
                    errorMessage.Append("_stopTime: " + _stopTime + "\r\n");
                    errorMessage.Append("Message: " + ex.Message + "\r\n");
                    errorMessage.Append("StackTrace: " + ex.StackTrace + "\r\n");
                    amazonOperations.WriteErrorQueue(errorMessage.ToString());
                }
            }
        }
    
    }
}