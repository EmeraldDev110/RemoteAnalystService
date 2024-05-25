using System;
using System.IO;
using log4net;
using RemoteAnalyst.AWS.Glacier;
using RemoteAnalyst.AWS.Infrastructure;
using RemoteAnalyst.AWS.S3;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.UWSLoader.BLL.Process {
    /// <summary>
    /// ProcessOSSJRNL get the OSS JRNL data file form S3.
    /// Call job processor class to load the data.
    /// </summary>
    internal class ProcessOSSJRNL
    {
        private static readonly ILog Log = LogManager.GetLogger("JobLoader");
        private readonly string _ossFileName = string.Empty;
        private readonly string _s3Location = string.Empty;
        private readonly string _systemSerial = string.Empty;
        private readonly int _uwsId;

        private string _saveLocation = "";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="systemSerial"> System serial number.</param>
        /// <param name="fileName"> CPUInfo file name.</param>
        /// <param name="location"> S3 location of the data file.</param>
        public ProcessOSSJRNL(string systemSerial, string fileName, string location, int uwsId) {
            _systemSerial = systemSerial;
            _ossFileName = fileName;
            _s3Location = location;
            _uwsId = uwsId;
        }

        /// <summary>
        /// Get OSS data file from S3.
        /// Call OSSJRNLService to process the data file.
        /// </summary>
        public void StartProcess() {
            var uwsId = 0;
            try { 
                if (ConnectionString.IsLocalAnalyst) {
                    if (File.Exists(_s3Location)) {
                        var fileInfo = new FileInfo(_s3Location);
                        if (!Directory.Exists(ConnectionString.ServerPath + "Systems\\" + _systemSerial + "\\"))
                            Directory.CreateDirectory(ConnectionString.ServerPath + "Systems\\" + _systemSerial + "\\");

                        _saveLocation = ConnectionString.ServerPath + "Systems\\" + _systemSerial + "\\" + fileInfo.Name;
                        fileInfo.CopyTo(_saveLocation);
                    }
                }
                else
                {
                    string saveFolder = ConnectionString.ServerPath;
                    var s3 = new AmazonS3(ConnectionString.S3FTP);
                    _saveLocation = s3.ReadS3(_s3Location, saveFolder);
                }

                //Get ConnectionString
                var databaseMapService = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
                string newConnectionString = databaseMapService.GetConnectionStringFor(_systemSerial);
                if (newConnectionString.Length == 0) {
                    newConnectionString = ConnectionString.ConnectionStringSPAM;
                }

                var jobOSS = new OSSJRNLService(newConnectionString);
                Log.Info("***********************************************************************");
                Log.Info("Loading OSS JRNL data");
                Log.InfoFormat("System serial number: {0}", _systemSerial);
                Log.InfoFormat("Save Location : {0}", _saveLocation);
                
                var loadingInfo = new LoadingInfoService(ConnectionString.ConnectionStringDB);

                try {
                    if(_uwsId == 0)
                        uwsId = loadingInfo.GetMaxUWSIDFor();
                    else
                        uwsId = _uwsId;
                    var fileInfo = new FileInfo(_saveLocation);

                    loadingInfo.UpdateFor(fileInfo.Name, _systemSerial, fileInfo.Length.ToString(), "0", uwsId.ToString()); //0: OSS JRNL
                }
                catch (Exception ex) {
                    Log.ErrorFormat("Loading Info Error1: {0}", ex);
                    
                }

                jobOSS.LoadOSSJRNL(_saveLocation, _systemSerial, ConnectionString.SystemLocation);
                Log.Info("Finish OSS JRNL loading");
                
                
                //Delete the OSS File.
                Log.Info("Deleting OSS JRNL");
                
                if (File.Exists(_saveLocation)) {
                    File.Delete(_saveLocation);
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("Failed to load OSS JRNL");
                
                Log.ErrorFormat("Error Message: {0}", ex.Message);
                
                Log.ErrorFormat("Error StackTrace: {0}", ex.StackTrace);
            }
            finally {
                try {
                    var systemInfo = new System_tblService(ConnectionString.ConnectionStringDB);
                    var systemName = systemInfo.GetSystemNameFor(_systemSerial);
                    var loadingInfo = new LoadingInfoService(ConnectionString.ConnectionStringDB);
                    loadingInfo.UpdateFor(uwsId, systemName, DateTime.Now, DateTime.Now, 0); //type 0 is OSS JRNL
                }
                catch (Exception ex) {
                    Log.ErrorFormat("Loading Info Error2: {0}", ex.Message);
                }
            }
        }
    }
}