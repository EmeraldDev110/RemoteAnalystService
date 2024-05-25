using System;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;
using log4net;
using RemoteAnalyst.AWS.EC2;
using RemoteAnalyst.AWS.Glacier;
using RemoteAnalyst.AWS.Infrastructure;
using RemoteAnalyst.AWS.S3;
using RemoteAnalyst.BusinessLogic.BLL;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.Enums;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.UWSLoader.SPAM.BLL;

namespace RemoteAnalyst.UWSLoader.BLL.Process {
    /// <summary>
    /// get the data file form S3 and unzip it.
    /// Call OpenUWS class to load the data directly.
    /// </summary>
    public class ProcessDISCOPEN
    {
        private static readonly ILog Log = LogManager.GetLogger("DISCOPENDataLoad");
        private readonly string _s3Location = string.Empty;
        private readonly DateTime _selectedStartTime = DateTime.MinValue;
        private readonly DateTime _selectedStopTime = DateTime.MaxValue;
        private readonly string _systemSerial = string.Empty;
        private string _saveLocation = "";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="systemSerial"> System serial number.</param>
        /// <param name="location"> S3 location of the data file.</param>
        /// <param name="selectedStartTime"> Start timestamp that user select.</param>
        /// <param name="selectedStopTime"> Stop timestamp that user select.</param>
        public ProcessDISCOPEN(string systemSerial, string location, DateTime selectedStartTime, DateTime selectedStopTime) {
            _systemSerial = systemSerial;
            _s3Location = location;
            _selectedStartTime = selectedStartTime;
            _selectedStopTime = selectedStopTime;
        }

        /// <summary>
        /// Get the data file fromr S3.
        /// Call OpenUWS to load the data into database.
        /// </summary>
        public void StartProcess() {
            string saveFolder = ConnectionString.ServerPath;
            
            try {
                if (_s3Location.Contains(".zip")) {
                    if (ConnectionString.IsLocalAnalyst) {
                        _saveLocation = _s3Location;
                    }
                    else
                    {
                        var s3 = new AmazonS3(ConnectionString.S3UWS);
                        _saveLocation = s3.ReadS3(_s3Location, saveFolder);
                    }

                    string unzippedLocation = saveFolder + "\\Systems\\" + _systemSerial;

                    if (File.Exists(_saveLocation)) {
                        string filename;
                        using (ZipFile zip = ZipFile.Read(_saveLocation)) {
                            if (zip.Entries.Count > 1) {
                                //Multiple files in the zip file.
                            }
                            ZipEntry entry = zip.First();
                            entry.Extract(unzippedLocation, ExtractExistingFileAction.OverwriteSilently);
                            filename = unzippedLocation + "/" + entry.FileName;
                        }
                        //Create log file
                        // Open Upon Load sending Log file 

                        DateTime currTime = DateTime.Now;
                        Log.Info("***********************************************************************");
                        Log.InfoFormat("Loading DISCOPEN data: {0}", currTime);
                        Log.InfoFormat("System serial number: {0}, start time: {1}, stop time: {2}", 
                            _systemSerial, _selectedStartTime, _selectedStopTime);
                        Log.InfoFormat("_s3Location: {0}", _s3Location);
                        Log.InfoFormat("File Name: {0}", filename);
                        
                        var collectionType = new UWSFileInfo();
                        UWS.Types checkSPAM = collectionType.UwsFileVersionNew(filename);

                        var openUWS = new OpenUWS();
                        bool success = openUWS.CreateNewData(filename, 0, Log, checkSPAM, 0, _selectedStartTime, _selectedStopTime, ConnectionString.DatabasePrefix);

                        if (success) {
                            Log.InfoFormat("Load DISCOPEN DATA successfully in {0}", (DateTime.Now - currTime));
                            Log.InfoFormat("Delete zip files: {0}", _saveLocation);
                            
                            //Delete zip files
                            if (File.Exists(_saveLocation)) {
                                File.Delete(_saveLocation);
                            }
                        }
                    }
                    else {
                        Log.InfoFormat("Call UpdateLoadingFor with : {0} and {1}", _systemSerial, _s3Location);
                        var uwsDirectory = new UWSDirectoryService(ConnectionString.ConnectionStringDB);
                        uwsDirectory.UpdateLoadingFor(_systemSerial, _s3Location, 0);
                    }
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("Failed to load DISCOPEN Data: StackTrace: {0}", ex.StackTrace);               

                if (ConnectionString.IsLocalAnalyst) {

                    var email = new EmailToSupport(ConnectionString.AdvisorEmail, ConnectionString.SupportEmail,
                        ConnectionString.WebSite,
                        ConnectionString.EmailServer, ConnectionString.EmailPort, ConnectionString.EmailUser,
                        ConnectionString.EmailPassword, ConnectionString.EmailAuthentication,
                        ConnectionString.SystemLocation, ConnectionString.ServerPath,
                        ConnectionString.EmailIsSSL,
                        ConnectionString.IsLocalAnalyst, ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                    email.SendLocalAnalystErrorMessageEmail("UWSLoader - ProcessDISCOPEN.cs", ex.Message, LicenseService.GetProductName(ConnectionString.ConnectionStringDB));
                }
                else
                {
                    var amazonOperations = new AmazonOperations();
                    StringBuilder errorMessage = new StringBuilder();
                    errorMessage.Append("Source: ProcessDISCOPEN \r\n");
                    errorMessage.Append("_s3Location: " + _s3Location + "\r\n");
                    errorMessage.Append("_systemSerial: " + _systemSerial + "\r\n");
                    errorMessage.Append("Message: " + ex.Message + "\r\n");
                    errorMessage.Append("StackTrace: " + ex.StackTrace + "\r\n");
                    amazonOperations.WriteErrorQueue(errorMessage.ToString());
                }
            }
            finally {
                string instanceID = "";
                if (!ConnectionString.IsLocalAnalyst) {
                    var ec2 = new AmazonEC2();
                    instanceID = ec2.GetEC2ID();
                }

                Log.InfoFormat("Call DeleteLoadingInfoFor with : {0} and {1}", _s3Location, instanceID);
                

                var loadingStatusDetailService = new LoadingStatusDetailDISCOPENService(ConnectionString.ConnectionStringDB);
                loadingStatusDetailService.DeleteLoadingInfoFor(_s3Location, instanceID);

                Log.InfoFormat("Call UpdateLoadingFor with : {0} and {1}", _systemSerial, _s3Location);
                
                var uwsDirectory = new UWSDirectoryService(ConnectionString.ConnectionStringDB);
                //uwsDirectory.UpdateLoadingFor(_systemSerial, _selectedStartTime, _selectedStopTime, 0); //0 is not loading.
                try {
                    uwsDirectory.UpdateLoadingFor(_systemSerial, _s3Location, 0);
                }
                catch (Exception ex) {
                    Log.ErrorFormat("Error : {0}", ex.Message);                    
                }
            }
        }
    }
}