using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.Repository.Concrete;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using RemoteAnalyst.Repository.Repositories;
using RemoteAnalyst.UWSLoader.BLL;

namespace RemoteAnalyst.UWSLoader {
    partial class UWSLoaderService : ServiceBase {
        private readonly JobLoader _loader = new JobLoader();

        public UWSLoaderService() {
            InitializeComponent();

            ServiceName = "UWSLoader";
            EventLog.Source = "UWSLoader";
            EventLog.Log = "Application";

            // These Flags set whether or not to handle that specific
            //  type of event. Set to true if you need it, false otherwise.
            CanHandlePowerEvent = true;
            CanHandleSessionChangeEvent = true;
            CanPauseAndContinue = false;
            CanShutdown = true;
            CanStop = true;

            if (!EventLog.SourceExists("UWSLoader")) {
                EventLog.CreateEventSource("UWSLoader", "Application");
            }
        }

        protected override void OnStart(string[] args) {
            // TODO: Add code here to start your service.
            try {
#if RDSMove
                EventLog.WriteEntry("UWSLoader-RDSMove", "Running as RDSMove UWS Loader");
#endif
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["S3XML"])) {
                    EventLog.WriteEntry("UWSLoader", "Reading from XML File");
                    ReadXML.ImportDataFromXML();
                }
                else {
                    EventLog.WriteEntry("UWSLoader", "Reading from S3 XML File: " + ConfigurationManager.AppSettings["S3XML"]);
                    // Trying to read the XML file for 3 times
                    var attempts_left = 3;
                    do
                    {
                        EventLog.WriteEntry("UWSLoader", "Reading from XML File");
                        try
                        {
                            ReadXML.ImportDataFromXMLS3();
                            // Successful, so don't need to retry, force out of loop
                            attempts_left = 0;
                        }
                        catch (Exception ex)
                        {
                            attempts_left--;
                            EventLog.WriteEntry("UWSLoader", "Error in reading S3 XML File:" + ex.Message + " attempts left " + attempts_left);
                        }
                    } while (attempts_left > 0);

                }
                if (!LicenseService.IsValidProductIndentifierKey(ConnectionString.ConnectionStringDB)) {
                    EventLog.WriteEntry("UWSLoader", "Invalid Product Indentifier Key. Please contact " + ConnectionString.SupportEmail);
                }
                else {
                    ConnectionString.IsLocalAnalyst = LicenseService.IsLocalAnalystOrPMC(ConnectionString.ConnectionStringDB);
                    EventLog.WriteEntry("UWSLoader", "S3 FTP " + ConnectionString.S3FTP);
                    EventLog.WriteEntry("UWSLoader", "SQSLoad " + ConnectionString.SQSLoad);
                    EventLog.WriteEntry("UWSLoader", "PrimaryEC2 " + ConnectionString.PrimaryEC2);
                    //update connection strings
                    DataTable systemSerialList = new DataTable();
                    SystemRepository systemService = new SystemRepository();
                    systemSerialList = systemService.GetAllSystems();
                    var service = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
                    foreach (DataRow row in systemSerialList.Rows)
                    {
                        string connStr = service.GetAllConnectionStrings(row["SystemSerial"].ToString());
                        if (!connStr.Contains("AllowLoadLocalInfile=true"))
                            connStr += ";AllowLoadLocalInfile=true;";
                        if (!connStr.Contains("Allow User Variables=true"))
                            connStr += ";Allow User Variables=true;";
                        service.UpdateConnectionString(row["SystemSerial"].ToString(), connStr, connStr, ConnectionString.ConnectionStringDB, Convert.ToString(ConnectionString.IsLocalAnalyst));
                        EventLog.WriteEntry("UWSLoader", "DatabaseMappings updated");
                    }
                    EventLog.WriteEntry("UWSLoader", "Starting Job Watch ...");
                    StartJobWatch();
                    EventLog.WriteEntry("UWSLoader", "Job Watch started.");
                }
            }
            catch (Exception ex) {
                EventLog.WriteEntry("UWSLoader", "Service Start Error: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        protected override void OnStop() {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
            StopJobWatch();
        }

        private void StartJobWatch() {
            //Call jobwatcher class.
            //string folderPath = ConnectionString.WatchFolder;
            //watcher.StartJobWatch(folderPath);
            _loader.StartCheckQueue();
        }

        private void StopJobWatch() {
            _loader.StopJobWatch();
        }
    }
}