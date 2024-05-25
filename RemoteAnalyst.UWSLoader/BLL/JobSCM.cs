using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using log4net;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.Repository.MySQLConcrete.RemoteAnalystSPAM;
using RemoteAnalyst.UWSLoader.Email;

namespace RemoteAnalyst.UWSLoader.BLL {
    internal class JobSCM {
        private static readonly ILog Log = LogManager.GetLogger("SCMLoader");
        private readonly int _profileId;
        private readonly string _systemSerial;

        public JobSCM(string systemSerial, int profileId) {
            _systemSerial = systemSerial;
            _profileId = profileId;
        }

        public void GenerateSCMData() {
            //1. Get Profile Information
            var transactionProfile = new TransactionProfileServices(ConnectionString.ConnectionStringDB);
            TransactionProfileInfo transactionInfos = transactionProfile.GetTransactionProfileInfoFor(_systemSerial, _profileId);

            if (transactionInfos != null) {
                //Get MySQL Per System ConnectionString.
                var databaseMapping = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
                string systemMySQLConnectionString = databaseMapping.GetMySQLConnectionStringFor(_systemSerial);

                //Get Database Name.
                string databaseName = Helper.FindKeyName(systemMySQLConnectionString, Helper._DATABASEKEYNAME);

                Log.InfoFormat("systemMySQLConnectionString: {0}", DiskLoader.RemovePassword(systemMySQLConnectionString));
                
                var transactionProfileTrend = new TransactionProfileTrendServices(systemMySQLConnectionString);
                bool exists = transactionProfileTrend.CheckTransactionProfileTrendFor(databaseName);
                if (!exists) {
                    transactionProfileTrend.CreateTransactionProfileTrendFor();
                }

                //2. Get all the dates that can we loaded.
                var currentTables = new CurrentTables(systemMySQLConnectionString);
                var tableTimestamps = new TableTimestamps(systemMySQLConnectionString);
                Dictionary<string, long> fileTables = currentTables.GetFileTableList();
                var fileTableInfo = new Dictionary<string, List<FileTableTimeInfo>>();

                Log.InfoFormat("fileTables count: {0}", fileTables.Count);
                

                foreach (KeyValuePair<string, long> fileTable in fileTables) {
                    DataTable tableTimes = tableTimestamps.GetStartEndTime(fileTable.Key);

                    var timeInfo = new List<FileTableTimeInfo>();
                    foreach (DataRow dr in tableTimes.Rows) {
                        timeInfo.Add(new FileTableTimeInfo {
                            StartDateTime = Convert.ToDateTime(dr["Start"]),
                            EndDateTime = Convert.ToDateTime(dr["End"]),
                            Interval = fileTable.Value
                        });
                    }
                    fileTableInfo.Add(fileTable.Key, timeInfo);
                }

                Log.InfoFormat("fileTableInfo: {0}", fileTableInfo.Count);
                
                //3. Load the data.
                foreach (KeyValuePair<string, List<FileTableTimeInfo>> fileInfo in fileTableInfo) {
                    string[] fileNames = transactionInfos.TransactionFile.Split('.');
                    string volume = fileNames[0];
                    string subVol = fileNames[1];
                    string fileName = fileNames[2];

                    Log.InfoFormat("fileNames: {0} volume: {1} subvol: {2} fileName: {3}", 
                        fileNames, volume, subVol, fileName);

#if DEBUG
                    List<FileTableTimeInfo> temp = fileInfo.Value.GetRange(0,1);
#endif

                    foreach (var fileTableTimeInfo in fileInfo.Value) {
                        Log.InfoFormat("Date: {0}, From: {1}, To: {2}", 
                            fileInfo.Key, fileTableTimeInfo.StartDateTime, fileTableTimeInfo.EndDateTime);
                        
                        //Get CPU Interval.
                        var cpuTableName = fileInfo.Key.Replace("FILE", "CPU");
                        var currentTableServices = new CurrentTableService(systemMySQLConnectionString);
                        var cpuInterval = currentTableServices.GetIntervalFor(cpuTableName);
                        Log.InfoFormat("cpuInterval: {0}", cpuInterval);
                        

                        try {
                            if (transactionInfos.OpenerType.Equals(0)) {
                                //Any
                                Log.Info("Opener Type: Any");
                                transactionProfileTrend.PopulateAnyTPSFor(transactionInfos.TransactionProfileID,
                                    fileInfo.Key, fileTableTimeInfo.StartDateTime, fileTableTimeInfo.EndDateTime, fileTableTimeInfo.Interval,
                                    transactionInfos.IOTransactionRatio, transactionInfos.TransactionCounter,
                                    volume, subVol, fileName, transactionInfos.IsCpuToFile, cpuInterval, ConnectionString.SystemLocation);
                            }
                            else if (transactionInfos.OpenerType.Equals(1)) {
                                //Program File
                                string[] openers = transactionInfos.OpenerName.Split('.');
                                string openerVolume = openers[0];
                                string openerSubVol = openers[1];
                                string openerFileName = openers[2];
                                Log.Info("Opener Type: Program File");
                                transactionProfileTrend.PopulateOpenerProgramFor(transactionInfos.TransactionProfileID,
                                    fileInfo.Key, fileTableTimeInfo.StartDateTime, fileTableTimeInfo.EndDateTime, fileTableTimeInfo.Interval,
                                    transactionInfos.IOTransactionRatio, transactionInfos.TransactionCounter,
                                    volume, subVol, fileName, openerVolume, openerSubVol, openerFileName, transactionInfos.IsCpuToFile, cpuInterval, ConnectionString.SystemLocation);
                            }
                            else if (transactionInfos.OpenerType.Equals(2)) {
                                //Process
                                Log.Info("Opener Type: Process");
                                transactionProfileTrend.PopulateOpenerProcessFor(transactionInfos.TransactionProfileID,
                                    fileInfo.Key, fileTableTimeInfo.StartDateTime, fileTableTimeInfo.EndDateTime, fileTableTimeInfo.Interval,
                                    transactionInfos.IOTransactionRatio, transactionInfos.TransactionCounter,
                                    volume, subVol, fileName, transactionInfos.OpenerName, transactionInfos.IsCpuToFile, cpuInterval, ConnectionString.SystemLocation);
                            }
                        }
                        catch (Exception ex) {
                            Log.ErrorFormat("*************************************************");
                            Log.ErrorFormat("Error: {0}", ex);
                            
                        }
                    }
                }

                Log.Info("Finished, send email..");

#if (DEBUG)
                /*string profileName = transactionProfile.GetTransactionProfileName(_profileId);
                var custInfo = new CusAnalystService(ConnectionString.ConnectionStringDB);
                string custName = custInfo.GetUserNameFor("leizhang@idelji.com");
                var email = new EmailHelper();
                email.SendSCMLoadCompleteEmail("leizhang@idelji.com", "Lei Zhang", profileName, fileTableInfo.First().Value.First().StartDateTime.ToString("yyyy-MM-dd"));*/
#else
                try {
                    string profileName = transactionProfile.GetTransactionProfileName(_profileId);
                    var transactionProfileEmailService = new TransactionProfileEmailServices(ConnectionString.ConnectionStringDB);
                    List<string> emails = transactionProfileEmailService.GetTransactionProfileEmailFor(_profileId);
                    var email = new EmailHelper();
                    var custInfo = new CusAnalystService(ConnectionString.ConnectionStringDB);
                    Log.InfoFormat("Profile name: {0}, Email count: {1}", profileName, emails.Count);
                    if (emails.Count == 0)
                        throw new Exception();
                    foreach (string e in emails) {
                        string custName = custInfo.GetUserNameFor(e);
                        string[] split = custName.Split(' ');
                        string formattedCustName = FirstLetterToUpper(split[0].ToLower()) + " " + FirstLetterToUpper(split[1].ToLower());
                        email.SendSCMLoadCompleteEmail(e, formattedCustName, profileName, fileTableInfo.First().Value.First().StartDateTime.ToString("yyyy-MM-dd"));
                        Log.InfoFormat("Email sent to " + e);
                        
                    }

                    Log.Info("Email sent successfully");
                    
                }
                catch (Exception ex) {
                    Log.ErrorFormat("Error sending email: {0}", ex);
                }
                finally {
                }
#endif
            }
        }
        private string FirstLetterToUpper(string str) {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }
    }
}