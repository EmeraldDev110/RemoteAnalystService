using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using DataBrowser.Context;
using log4net;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.Repository.MySQLConcrete.RemoteAnalystSPAM;
using RemoteAnalystTrendLoader.context;

namespace RemoteAnalyst.UWSLoader.JobProcessor {
    class JobApplication {
        private static readonly ILog Log = LogManager.GetLogger("ApplicationLoad");
        private readonly string _systemSerial;
        private readonly int _profileId;
        private readonly int _customerId;
        public JobApplication(string systemSerial, int profileId, int customerId) {
            _systemSerial = systemSerial;
            _profileId = profileId;
            _customerId = customerId;
        }

        internal void GenerateApplicationData() {
            //Get all the deatil data dates.
            //make sure it has cpu, process, and file entities.
            //Get Interval.

            Log.InfoFormat("system Serial: {0}, ProfileId: {1}", _systemSerial, _profileId);
            var databaseMapping = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
            string systemMySqlConnectionString = databaseMapping.GetMySQLConnectionStringFor(_systemSerial);

            Log.InfoFormat("systemMySQLConnectionString: {0}", RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM.Database.RemovePassword(systemMySqlConnectionString));
            

            //2. Get all the dates that can we loaded.
            var currentTables = new CurrentTables(systemMySqlConnectionString);
            var tableTimestamps = new TableTimestamps(systemMySqlConnectionString);
            var processTableList = currentTables.GetProcessTableList();
            var fileTableList = currentTables.GetFileTableList();

            Log.InfoFormat("processTableList count: {0}", processTableList.Count);
            Log.InfoFormat("fileTableList count: {0}", fileTableList.Count);
            

            foreach (var processTable in processTableList) {
                //Per each day, get Start and End Times.
                var tableTimes = tableTimestamps.GetStartEndTime(processTable.Key);

                foreach (DataRow row in tableTimes.Rows) {
                    var fromTimestamp = Convert.ToDateTime(row["Start"]);
                    var toTimestamp = Convert.ToDateTime(row["End"]);
                    var cpuTable = processTable.Key.Replace("PROCESS", "CPU");
                    var tempFileTable = processTable.Key.Replace("PROCESS", "FILE");
                    var fileTable = "";
                    //Check if we have file table on fileTableList.
                    if (fileTableList.ContainsKey(tempFileTable))
                        fileTable = tempFileTable;

                    Log.InfoFormat("cpuTable: {0}", cpuTable);
                    Log.InfoFormat("processTable: {0}", processTable.Key);
                    Log.InfoFormat("fileTable: {0}", fileTable);
                    Log.InfoFormat("fromTimestamp: {0}", fromTimestamp);
                    Log.InfoFormat("toTimestamp: {0}", toTimestamp);
                    Log.InfoFormat("interval: {0}", processTable.Value);
                    

                    //Call the DLL
                    try {
                        var trendLoader = new RemoteAnalystTrendLoader.Services.ApplicationProcess(new PmcContext(), new DataContext(systemMySqlConnectionString));
                        trendLoader.GetApplicationDataForProfile(processTable.Key, fileTable, fromTimestamp, toTimestamp, processTable.Value, _profileId);
                    }
                    catch (Exception ex) {
                        Log.ErrorFormat("Error: {0}", ex);                        
                    }
                }
            }

            var cusAnalyst = new CusAnalystService(ConnectionString.ConnectionStringDB);
            var custInfo = cusAnalyst.GetCustomerEmailFor(_customerId);

            var profileDetail = new ProfileDetailService(ConnectionString.ConnectionStringDB);
            var applicationName = profileDetail.GetApplicationNameFor(_profileId);

            //Send out Email to the customer.
            var email = new ReportEmail(ConnectionString.AdvisorEmail, ConnectionString.SupportEmail, ConnectionString.WebSite, ConnectionString.EmailServer, ConnectionString.EmailPort,
                                        ConnectionString.EmailUser, ConnectionString.EmailPassword, ConnectionString.EmailAuthentication, 
                                        ConnectionString.SystemLocation, ConnectionString.ServerPath,
                                        ConnectionString.EmailIsSSL, ConnectionString.IsLocalAnalyst,
                                        ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
            email.SendApplicationLoadEmail(custInfo.Email, custInfo.FisrtName + ' ' + custInfo.LastName, applicationName);
        }
    }
}
