using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;

namespace RemoteAnalyst.BusinessLogic.SCM {
    public class SimpleCapacityModel {
        private static readonly ILog Log = LogManager.GetLogger("SCMLoader");
        private readonly string _connectionString;

        public SimpleCapacityModel(string connectionString) {
            _connectionString = connectionString;
        }

        public static string RemovePassword(string connectionString)
        {
            try
            {
                if (String.IsNullOrEmpty(connectionString))
                {
                    return connectionString;
                }
                if ((connectionString.Contains("PASSWORD") && connectionString.Contains(";")) || (connectionString.Contains("password") && connectionString.Contains(";")))
                {
                    List<string> strlist = connectionString.Split(';').ToList();
                    for (int i = 0; i < strlist.Count; i++)
                    {
                        if (strlist[i].Contains("PASSWORD") || connectionString.Contains("password"))
                        {
                            strlist.Remove(strlist[i]);
                            break;
                        }
                    }
                    string concat = String.Join(";", strlist.ToArray());
                    return concat;
                }
                else
                {
                    return connectionString;
                }
            }
            catch (Exception e)
            {
                return connectionString;
            }
        }

        public void PopulateSimpleCapacityModelData(string systemSerial, DateTime fromDateTime, DateTime toDateTime, string fileTableName, long interval, string connectionStringRA, string databaseName, string systemLocation, string tempSaveLocation) {
            try {
                Log.InfoFormat("_connectionString: {0}", SimpleCapacityModel.RemovePassword(_connectionString));
                
                //Get CPU Interval.
                var cpuTableName = fileTableName.Replace("FILE", "CPU");
                var currentTableServices = new CurrentTableService(_connectionString);
                var cpuInterval = currentTableServices.GetIntervalFor(cpuTableName);
                Log.InfoFormat("cpuInterval: {0}", cpuInterval);
                

                Log.Info("Get the Profile Info");
                
                //Get the Profile Info.
                var transactionProfile = new TransactionProfileServices(connectionStringRA);
                List<TransactionProfileInfo> transactionInfos = transactionProfile.GetTransactionProfileInfoFor(systemSerial);

                //Get MySQL Per System ConnectionString.
                var databaseMapping = new DatabaseMappingService(connectionStringRA);
                string systemMySQLConnectionString = databaseMapping.GetMySQLConnectionStringFor(systemSerial);

                Log.InfoFormat("systemMySQLConnectionString: {0}", SimpleCapacityModel.RemovePassword(systemMySQLConnectionString));
                Log.InfoFormat("transactionInfos: {0}", transactionInfos.Count);
                

                if (transactionInfos.Count <= 0) {
                    return;
                }

                var transactionProfileTrend = new TransactionProfileTrendServices(systemMySQLConnectionString);
                bool exists = transactionProfileTrend.CheckTransactionProfileTrendFor(databaseName);
                if (!exists) {
                    transactionProfileTrend.CreateTransactionProfileTrendFor();
                }

                foreach (TransactionProfileInfo profileInfo in transactionInfos) {
                    string[] fileNames = profileInfo.TransactionFile.Split('.');
                    string volume = fileNames[0];
                    string subVol = fileNames[1];
                    string fileName = fileNames[2];

                    Log.InfoFormat("fileNames: {0}", fileNames);
                    Log.InfoFormat("volume: {0}", volume);
                    Log.InfoFormat("subVol: {0}", subVol);
                    Log.InfoFormat("fileName: {0}", fileName);
                    

                    try {
                        if (profileInfo.OpenerType.Equals(0)) {
                            transactionProfileTrend.PopulateAnyTPSFor(profileInfo.TransactionProfileID,
                                fileTableName, fromDateTime, toDateTime, interval,
                                profileInfo.IOTransactionRatio, profileInfo.TransactionCounter,
                                volume, subVol, fileName, profileInfo.IsCpuToFile, cpuInterval, tempSaveLocation);
                        }
                        else if (profileInfo.OpenerType.Equals(1)) {
                            string[] openers = profileInfo.OpenerName.Split('.');
                            string openerVolume = openers[0];
                            string openerSubVol = openers[1];
                            string openerFileName = openers[2];

                            transactionProfileTrend.PopulateOpenerProgramFor(profileInfo.TransactionProfileID,
                                fileTableName, fromDateTime, toDateTime, interval,
                                profileInfo.IOTransactionRatio, profileInfo.TransactionCounter,
                                volume, subVol, fileName, openerVolume, openerSubVol, openerFileName, profileInfo.IsCpuToFile, cpuInterval, tempSaveLocation);
                        }
                        else if (profileInfo.OpenerType.Equals(2)) {
                            transactionProfileTrend.PopulateOpenerProcessFor(profileInfo.TransactionProfileID,
                                fileTableName, fromDateTime, toDateTime, interval,
                                profileInfo.IOTransactionRatio, profileInfo.TransactionCounter,
                                volume, subVol, fileName, profileInfo.OpenerName, profileInfo.IsCpuToFile, cpuInterval, tempSaveLocation);
                        }
                    }
                    catch (Exception ex) {
                        Log.ErrorFormat("*************************************************");
                        Log.ErrorFormat("Error: {0}", ex.Message);
                        
                    }
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("*************************************************");
                Log.ErrorFormat("Error: {0}", ex.Message);
                
            }
        }
    }
}