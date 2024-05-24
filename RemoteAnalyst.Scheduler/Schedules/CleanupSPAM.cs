using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using DataBrowser.Context;
using log4net;
using MySQLDataBrowser.Model;
using RemoteAnalyst.AWS.Infrastructure;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.Repository.Concrete.Model;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;

namespace RemoteAnalyst.Scheduler.Schedules {
    /// <summary>
    /// CleanupSPAM delete data from System database and System UWS file.
    /// </summary>
    internal class CleanupSPAM {
        private readonly LoadingInfoService _loadingInfo = new LoadingInfoService(ConnectionString.ConnectionStringDB);
        private List<string> _expiredSystem = new List<string>();
        private static readonly ILog Log = LogManager.GetLogger("CleanupSPAM");
        /// <summary>
        /// Timer_Elapsed is a event that gets call by Scheduler to start the schedule task.
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="e">Timer ElapsedEventArgs</param>
        public void Timer_Elapsed(object source, ElapsedEventArgs e) {
            int currHour = BusinessLogic.Util.Helper.RoundUp(DateTime.Now, TimeSpan.FromMinutes(15)).Hour;
            //int currHour = DateTime.Now.Hour;
            if (currHour.Equals(3)) {
                RefreshExpiredSystemList();
                DoCleanup();
            }
        }

        private void RefreshExpiredSystemList()
        {
            var sysInfo = new System_tblService(ConnectionString.ConnectionStringDB);
            _expiredSystem = sysInfo.GetExpiredSystemFor(ConnectionString.IsLocalAnalyst);
        }

        public void DoCleanup() {
            DeleteSPAM();
            DeletePathway();
            DeleteDetailDataForForecast();
            DeleteDailiesTopProcesses();
            DeleteQNM();
            PerformStorageAnalysis();
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
        /// <summary>
        /// DeleteSPAM gets the System Serial and Retention Day and call FindSPAMData and FindUWSDirectories
        /// </summary>
        public void DeleteSPAM() {
            try {
                IDictionary<string, int> retentionDays = _loadingInfo.GetExpertReportRetentionDayFor();
                foreach (KeyValuePair<string, int> kv in retentionDays) {
                    try {
                        {
							if (_expiredSystem.Contains(kv.Key))
                            {
                                continue;   // Skip cleanup logic for expired systems. Should drop the database manually
                            }
							Log.InfoFormat("calling FindSPAMData: For {0} retention {1}", 
                                kv.Key, kv.Value);
							//Call delete function.
							FindSPAMData(kv);
                        }
                    }
                    catch (Exception ex) {
						Log.ErrorFormat("DeleteSPAM error for {0} error: {1}", kv.Key, ex);
                    }
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("CleanupSPAM Error: {0}", ex);
                if (!ConnectionString.IsLocalAnalyst) {
                    var amazon = new AmazonOperations();
                    amazon.WriteErrorQueue("CleanupSPAM Error: " + ex.Message);
                }
            }
            finally {
                Log.Error("Close the stream writer");
                ConnectionString.TaskCounter--;
            }
        }

        /// <summary>
        /// FindSPAMData calculate the retention date and call DeleteSPAMData
        /// </summary>
        /// <param name="retention">KeyValuePair with System Serial Number and Retention Day</param>
        private void FindSPAMData(KeyValuePair<string, int> retention) {
            Log.Info("Finding SPAM data");
            try {
                string connectionString = ConnectionString.ConnectionStringSPAM;
                var databaseMap = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
                string newConnectionString = databaseMap.GetConnectionStringFor(retention.Key);
                if (newConnectionString.Length == 0) {
                    newConnectionString = connectionString;
                }

                var sysData = new DataTableService(newConnectionString);
                var tempString = newConnectionString.Split(';');
                var databaseName = "";
                foreach (var s in tempString) {
                    if (s.ToUpper().Contains("DATABASE")) {
                        databaseName = s.Split('=')[1];
                    }
                }
                IDictionary<string, DateTime> tableCreateDate = sysData.GetCreatedDateFor(retention.Key, newConnectionString, databaseName);

                IDictionary<string, string> tables = new Dictionary<string, string>();

                foreach (KeyValuePair<string, DateTime> kv in tableCreateDate) {
                    var retentionDays = retention.Value;
                    if (kv.Key.ToLower().Contains("sqlproc") ||
                            kv.Key.ToLower().Contains("sqlstmt") ||
                        kv.Key.ToLower().Contains("discope") ||
                        kv.Key.ToLower().Contains("file") ||
                        kv.Key.ToLower().Contains("netline") ||
                        kv.Key.ToLower().Contains("oss")) {
                        retentionDays /= 6;
                    }

                    DateTime checkRetentionDate = kv.Value.AddDays(retentionDays); //Table creation date + retention days
                    if (checkRetentionDate < DateTime.Today) {
                        //Add it to the list.
                        if (!tables.ContainsKey(kv.Key)) {
                            tables.Add(kv.Key, retention.Key);
                            Log.InfoFormat("Add to cleanup list: {0}", kv.Key);
                        }
                    }
                }
				
                if (tables.Count > 0) {
                    Log.InfoFormat("Table count for {0} deletion: {1}", retention.Key, tables.Count);
                    DeleteSPAMData(retention.Key, tables, newConnectionString);
                }
                else {
					Log.InfoFormat("No tables to clean up for {0}", retention.Key);
				}
				
			}
            catch (Exception e) {
				Log.ErrorFormat("Exception occurred when finding spam data: {0}", e);
            }
        }

		/// <summary>
		/// DeleteSPAMData deletes System data from Per System Database
		/// </summary>
		/// <param name="serialNumber">Serial number of the system</param>
		/// <param name="tables">Table names that needs to be deleted</param>
		/// <param name="newConnectionString">System Database connection string</param>
		/// <param name="Log"></param>
		private void DeleteSPAMData(string serialNumber, IEnumerable<KeyValuePair<string, string>> tables, string newConnectionString) {
            try {
				Log.InfoFormat("Doing cleanup of tables for {0}", serialNumber);
                var entityTables = new DataTableService(newConnectionString);
                var currentTables = new CurrentTableService(newConnectionString);
                var tableTimestamp = new TableTimeStampService(newConnectionString);
                var systemTblService = new System_tblService(ConnectionString.ConnectionStringDB);
                TableTimeStampService tableTimeStampService = new TableTimeStampService(newConnectionString);
				int archiveRetention = systemTblService.GetArchiveRetensionValueFor(serialNumber);
				foreach (KeyValuePair<string, string> table in tables) {
                    //Need the start & stop time to get the unique row
                    List<ArchiveStatusView> archiveDetails = tableTimeStampService.GetArchiveDetailsPerTableFor(table.Key);
					try {
						entityTables.DropTableFor(table.Key);
                        Log.InfoFormat("Drop table {0}", table.Key);
					}
                    catch(Exception ex) {
						Log.ErrorFormat("MySql Drop Failed: {0}", ex);
					}
                                        
					string discBrowserTable = "";
                    try {
                        if (newConnectionString.Length > 0) {
                            var mySql = new MySQLDataBrowser.Model.FileTrendData(new DataContext(newConnectionString));                          
                            if(table.Key.Contains("FILE")) {
                                discBrowserTable = table.Key;
                                discBrowserTable = discBrowserTable.Replace("FILE", "DISKBROWSER");
                                mySql.DropDatabaseFor(table.Key.Replace("FILE", "FILETREND"));
                            }
                        }
                    }
                    catch (Exception ex) {
                        Log.ErrorFormat("MySql Drop Failed: {0}", ex);
                    }

                    //Drop disk browser table
                    try {
                        if (discBrowserTable.Length > 0) {
                            Log.Info("Start drop disk browser table");
							Log.InfoFormat("newConnectionString: {0}", CleanupSPAM.RemovePassword(newConnectionString));
                            Log.InfoFormat("discBrowserTable: {1}", discBrowserTable);
							var discTrendData = new DISCTrendData(new DataContext(newConnectionString));
                            discTrendData.DropDatabaseFor(discBrowserTable);
                        }
                    }
                    catch (Exception ex) {
						Log.ErrorFormat("MySql Drop Failed: {0}", ex);
                        Log.ErrorFormat("Table name: {0}", discBrowserTable);
					}

                    bool deleteEntryCurrentTable = true;
					var deleteArchiveList = new List<TableTimestampQueryParameter>();
					var updateArchiveList = new List<TableTimestampQueryParameter>();
					foreach (var archiveStatusView in archiveDetails) {
                        if (string.IsNullOrEmpty(archiveStatusView.ArchiveID)) {
                            if (!table.Key.Contains("_CPU_")) {
								deleteArchiveList.Add(new TableTimestampQueryParameter {
									TableName = archiveStatusView.TableName,
									StartTime = archiveStatusView.StartTime,
									StopTime = archiveStatusView.StopTime
								});
							}
						}
						else {
							if (archiveRetention > 0) {
								updateArchiveList.Add(new TableTimestampQueryParameter {
									TableName = archiveStatusView.TableName,
									StartTime = archiveStatusView.StartTime,
									StopTime = archiveStatusView.StopTime,
									Status = (int)archiveStatusView.Status
								});
							}
						}
					}

					if (deleteArchiveList.Count > 0) {
						tableTimestamp.DeleteEntryFor(deleteArchiveList);
					}
					if (updateArchiveList.Count > 0) {
						tableTimestamp.UpdateStatusUsingTableNameFor(updateArchiveList);
					}
					//Due to data drop, need to loop through the TableTimestamp entries by each interval 
					//as long as there are entries in TableTimestamp, it shouldn't delete entry in CurrentTable
					if (deleteEntryCurrentTable) {
						Log.InfoFormat("Delete entry in CurrentTable {0}", table.Key);
                        currentTables.DeleteEntryFor(table.Key);
                    }
					
                }
				Log.Info("Glacier clean up job done");
            }
            catch (Exception e) {
                Log.ErrorFormat("Exception orrcurred when cleanning SPAM data {0}", e);
            }
        }

        private void DeleteDailiesTopProcesses() {
            var deleteDate = DateTime.Now.AddDays(-45); //Per Khody's request set the retention day for DailiesTopProcesses to 45 days.
            IDictionary<string, int> retentionDays = _loadingInfo.GetPathwayRetentionDayFor();

            foreach (var r in retentionDays) {
                try {
                    if (_expiredSystem.Contains(r.Key))
                    {
                        continue;   // Skip cleanup logic for expired systems. Should drop the database manually
                    }
                    var databaseMap = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
                    string newConnectionString = databaseMap.GetConnectionStringFor(r.Key);
                    var sysData = new DataTableService(newConnectionString);
                    var databaseCheck = new Database(newConnectionString);
                    var databaseName = RemoteAnalyst.BusinessLogic.Util.Helper.FindKeyName(newConnectionString, "DATABASE");

                    var tableNameList = new List<string> {
                        "DailiesTopProcesses"
                    };

                    if (newConnectionString.Length > 0) {
                        //Delete 
                        foreach (var tableName in tableNameList) {
                            bool checkTableExists = databaseCheck.CheckTableExists(tableName, databaseName);
                            if (checkTableExists) {
                                var cmdText = "DELETE FROM " + tableName + " WHERE FromTimestamp < '" + deleteDate.ToString("yyyy-MM-dd") + "'";
                                sysData.RunCommandFor(cmdText);
								sysData.RunCommandFor("ANALZYE TABLE " + tableName);
                            }
                        }
                    }
                }
                catch (Exception ex) {
                    Log.ErrorFormat("CleanupDetailDataForForecast Error: {0}", ex);
                }
            }
        }

        /////////////////////QNM//////////////////////////////////////
        public void DeleteQNM() {
            try {
                Log.Info("************************************************");
                Log.Info("Start QNM data cleanup job");

                //TODO: Change Pathway Retention Days to QNM Retention Days.
                IDictionary<string, int> retentionDays = _loadingInfo.GetQNMRetentionDayFor();
                Log.InfoFormat("retentionDays count: {0}", retentionDays.Count);
                foreach (KeyValuePair<string, int> kv in retentionDays) {
                    if (_expiredSystem.Contains(kv.Key))
                    {
                        continue;   // Skip cleanup logic for expired systems. Should drop the database manually
                    }
                    Log.InfoFormat("calling FindSPAMData: {0}", kv);
                    DeleteQNMData(kv);
                }
                Log.Info("Cleanup QNM job done");
            }
            catch (Exception ex) {
                Log.ErrorFormat("Cleanup QNM Error: {0}", ex);
                if (!ConnectionString.IsLocalAnalyst) {
                    var amazon = new AmazonOperations();
                    amazon.WriteErrorQueue("Cleanup QNM Error: " + ex.Message);
                }
                else {
                    var email = new EmailToSupport(ConnectionString.AdvisorEmail, ConnectionString.SupportEmail,
                        ConnectionString.WebSite,
                        ConnectionString.EmailServer, ConnectionString.EmailPort, ConnectionString.EmailUser,
                        ConnectionString.EmailPassword, ConnectionString.EmailAuthentication,
                        ConnectionString.SystemLocation, ConnectionString.ServerPath,
                        ConnectionString.EmailIsSSL, ConnectionString.IsLocalAnalyst, 
                        ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                    email.SendLocalAnalystErrorMessageEmail("Scheduler - CleanupSPAM.cs - QNM", ex.Message, LicenseService.GetProductName(ConnectionString.ConnectionStringDB));
                }
            }
        }

        private void DeleteQNMData(KeyValuePair<string, int> retention) {
            try {
                var databaseMap = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
                string newConnectionString = databaseMap.GetConnectionStringFor(retention.Key);
                if (newConnectionString.Length > 0) {
                    var databaseName = Helper.FindKeyName(newConnectionString, "DATABASE");
                    var databaseCheck = new DatabaseService(newConnectionString);

                    DateTime deleteDate = DateTime.Now.AddDays(retention.Value * -1);
                    //Get list of dates to be deleted.
                    var qnmAbout = new QnmService(newConnectionString);
                    var qnmAboutExists = databaseCheck.CheckTableExistsFor("QNM_About", databaseName);
                    if (qnmAboutExists) {
                        var dateList = qnmAbout.GetDeleteDatesFor(deleteDate);
						if (dateList.Count == 0) {
							Log.InfoFormat("Nothing to delete for: {0} - {1}", 
                                retention.Key, deleteDate);
							return;
						}

						var qnmTableNameList = new List<string> {
                            "QNM_TCPProcessDetail",
                            "QNM_TCPPacketsDetail",
                            "QNM_TCPSubnetDetail",
                            "QNM_TCPv6Detail",
                            "QNM_TCPv6SubnetDetail",
                            "QNM_SLSADetail",
                            "QNM_CLIMDetail",
                            "QNM_ExpandPathDetail",
                            "QNM_ProbeRoundTripDetail",
                            "QNM_CLIMCPUDetail",
                            "QNM_CLIMDiskDetail"
                        };
                        var sysData = new DataTableService(newConnectionString);
                        var loadingInfo = new LoadingInfoService(ConnectionString.ConnectionStringDB);
                        var sampleType = 5;
                        var cmdText = "";
						var qnmTablesInDB = databaseCheck.GetQNMTableNamesInDatabaseFor(databaseName);
						var deleteFromQNMAboutList = new List<String>();
						var deleteParameterName = "@From";
						var deleteCmdText = "DELETE FROM QNM_About WHERE `FROM` = " + deleteParameterName;

						var updateLoadingInfoStatusList = new List<LoadingInfoParameter>();

						foreach (var dateTime in dateList) {
                            foreach (var tableName in qnmTableNameList) {
                                var qnmTableName = tableName + "_" + dateTime.Year + "_" + dateTime.Month + "_" + dateTime.Day;
								if (qnmTablesInDB.Contains(qnmTableName.ToLower())) {
									cmdText = @"DROP TABLE " + qnmTableName;
                                    sysData.RunCommandFor(cmdText);
                                    Log.InfoFormat("For {0} Dropped Table {1}", retention.Key, qnmTableName);
									
								}
							}
							//Add entry to delete from QNM About table
							deleteFromQNMAboutList.Add(dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
							//Update the status to DEL.
							updateLoadingInfoStatusList.Add(new LoadingInfoParameter {
								SystemSerial = retention.Key,
								SampleType = sampleType,
								StartTime = dateTime
							});
						}
						//Delete entry from the QNM About table.
						if (deleteFromQNMAboutList.Count > 0) {
                            databaseCheck.BulkDeleteSingleParameterFor(deleteCmdText, deleteParameterName, deleteFromQNMAboutList);
                            sysData.RunCommandFor("ANALYZE TABLE QNM_About");
						}
						if (updateLoadingInfoStatusList.Count > 0) {
							loadingInfo.UpdateStatusFor(updateLoadingInfoStatusList);
						}
                    }
                }
            }
            catch (Exception e) {
                Log.ErrorFormat("Exception occurred when deleting QNM data: {0}", e.Message);
            }
        }

        /////////////////////Pathway//////////////////////////////////////
        public void DeletePathway() {
            try {
                Log.InfoFormat("************************************************");
                Log.Info("Start Pathway data cleanup job");
                IDictionary<string, int> retentionDays = _loadingInfo.GetPathwayRetentionDayFor();
                Log.InfoFormat("retentionDays count: {0}", retentionDays.Count);
                foreach (KeyValuePair<string, int> kv in retentionDays) {
                                        if (_expiredSystem.Contains(kv.Key))
                    {
                        continue;   // Skip cleanup logic for expired systems. Should drop the database manually
                    }
                    Log.InfoFormat("calling FindSPAMData: {0}", kv);
                    DeletePathwayData(kv);
                }
                Log.Info("Cleanup Pathway job done");
            }
            catch (Exception ex) {
                Log.ErrorFormat("Cleanup Pathway Error: {0}", ex);
                if (!ConnectionString.IsLocalAnalyst) {
                    var amazon = new AmazonOperations();
                    amazon.WriteErrorQueue("Cleanup Pathway Error: " + ex.Message);
                }
                else {
                    var email = new EmailToSupport(ConnectionString.AdvisorEmail, ConnectionString.SupportEmail,
                        ConnectionString.WebSite,
                        ConnectionString.EmailServer, ConnectionString.EmailPort, ConnectionString.EmailUser,
                        ConnectionString.EmailPassword, ConnectionString.EmailAuthentication,
                        ConnectionString.SystemLocation, ConnectionString.ServerPath,
                        ConnectionString.EmailIsSSL, ConnectionString.IsLocalAnalyst, 
                        ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                    email.SendLocalAnalystErrorMessageEmail("Scheduler - CleanupSPAM.cs - Pathway", ex.Message, LicenseService.GetProductName(ConnectionString.ConnectionStringDB));
                }
            }
        }

        private void DeletePathwayData(KeyValuePair<string, int> retention) {
            try {
                var sampleType = 3;
                var databaseMap = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
                string newConnectionString = databaseMap.GetConnectionStringFor(retention.Key);
                if (newConnectionString.Length > 0) {
                    DateTime deleteDate = DateTime.Now.AddDays(retention.Value * -1);

                    var pathwayTableNameList = new List<string> {
                        "PvAlerts",
                        "PvCollects",
                        "PvCPUBusies",
                        "PvCpumany",
                        "PvCpuonce",
                        "PvErrinfo",
                        "PvLmstus",
                        "PvPwylist",
                        "PvPwymany",
                        "PvPwyonce",
                        "PvScassign",
                        "PvScdefine",
                        "PvScinfo",
                        "PvSclstat",
                        "PvScparam",
                        "PvScproc",
                        "PvScprstus",
                        "PvScstus",
                        "PvSctstat",
                        "PvTcpinfo",
                        "PvTcpstat",
                        "PvTcpstus",
                        "PvTerminfo",
                        "PvTermstat",
                        "PvTermstus"
                    };

                    var sysData = new DataTableService(newConnectionString);

                    foreach (var tableName in pathwayTableNameList) {
                        var cmdText = "DELETE FROM " + tableName + " WHERE FromTimestamp < '" + deleteDate.ToString("yyyy-MM-dd") + "'";
                        sysData.RunCommandFor(cmdText);
						sysData.RunCommandFor("ANALZYE TABLE " + tableName);
                    }


                    //Update the status to DEL.
                    var loadingInfo = new LoadingInfoService(ConnectionString.ConnectionStringDB);
                    loadingInfo.BulkUpdateStatusFor(retention.Key, sampleType, deleteDate);
                }
            }
            catch (Exception e) {
                Log.ErrorFormat("Exception occurred when deleting Pathway data: {0}", e.Message);
            }
        }
        public void DeleteDetailDataForForecast() {
            //12 Weeks. 12 * 7.
            var deleteDate = DateTime.Now.AddDays(-84);
            IDictionary<string, int> retentionDays = _loadingInfo.GetPathwayRetentionDayFor();

            foreach (var r in retentionDays) {
                try {
                    if (_expiredSystem.Contains(r.Key))
                    {
                        continue;   // Skip cleanup logic for expired systems. Should drop the database manually
                    }
                    var databaseMap = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
                    string newConnectionString = databaseMap.GetConnectionStringFor(r.Key);
                    var sysData = new DataTableService(newConnectionString);
                    var databaseCheck = new Database(newConnectionString);
                    var databaseName = RemoteAnalyst.BusinessLogic.Util.Helper.FindKeyName(newConnectionString, "DATABASE");

                    var tableNameList = new List<string> {
                        "DetailDiskForForecast",
                        "DetailProcessForForecast",
                        "DetailTmfForForecast"
                    };

                    if (newConnectionString.Length > 0) {
                        //Delete 
                        foreach (var tableName in tableNameList) {
                            bool checkTableExists = databaseCheck.CheckTableExists(tableName, databaseName);
                            if (checkTableExists) {
                                var cmdText = "DELETE FROM " + tableName + " WHERE FromTimestamp < '" + deleteDate.ToString("yyyy-MM-dd") + "'";
                                sysData.RunCommandFor(cmdText);
								sysData.RunCommandFor("ANALZYE TABLE " + tableName);
                            }
                        }
                    }
                }
                catch (Exception ex) {
                    Log.ErrorFormat("CleanupDetailDataForForecast Error: {0}", ex);
                }
            }

        }

        public void PerformStorageAnalysis()
        {
            Log.Info("Starting storage analysis");
			try
            {
                var storageAnalysis = new StorageAnalysis();
                storageAnalysis.LoadStorageAnalysis(Log);
            }
            catch(Exception e)
            {
                Log.ErrorFormat("Storage Analysis Error {0}", e.Message);
            }
            Log.Info("Storage analysis complete");
		}
    }
}