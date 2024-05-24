using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.ModelView;
using System.IO;
using log4net;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.Scheduler.Schedules
{
    class StorageAnalysis
    {
        public void LoadStorageAnalysis(ILog log)
        {
            var storageAnalysisService = new StorageAnalysisService(
                                                ConnectionString.ConnectionStringDB,
                                                log);
            var dbMappingService = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
            var list = new List<TempStorageTableView>();
            
            // get all rds name
            var rdsRealNamesTable = storageAnalysisService.GetAllRdsRealNames();
            var rdsInfo = storageAnalysisService.GetRDSInstanceInformation();

            foreach (DataRow row in rdsRealNamesTable.Rows)
            {
                var rdsName = "";
                // run analysis against all databases
                try {
                    rdsName = row["RdsRealName"].ToString();
                    var isAuroraInstance = rdsInfo.Where(x => x.RdsName.Equals(rdsName)).Select(x => x.IsAurora).FirstOrDefault();
                    if(isAuroraInstance) { //Since profile database is a mysql instance. All system databases are in Aurora
                        var dbString = dbMappingService.GetRdsConnectionStringFor(rdsName);
                        var tempDbService = new StorageAnalysisService(dbString, 
                                                            log, 
                                                            ConnectionString.S3Reports, 
                                                            ConnectionString.S3FTP, 
                                                            ConnectionString.S3UWS);
                        list.AddRange(tempDbService.GetStorageSizes());
                    }
                }
                catch(Exception e)
                {
                    log.ErrorFormat("Error getting storage information for {0}: {1}", rdsName, e.Message);
                }
            }
            try
            {
                storageAnalysisService.InsertData(list);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error inserting storage information: {0}", e.Message);
                
            }
        }
    }
}
