using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using DataBrowser.Context;
using log4net;
using MySQLDataBrowser.Concrete;
using MySQLDataBrowser.Model;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices {
    public class MySQLServices {
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

        public void CreateEntityTable(int entityID, string systemSerial, string tableName, 
            IList<ColumnInfoView> columnInfo, bool website, string connectionStringRA, 
            ILog log, bool loader, bool isProcessDirectlySystem, string databasePrefix, string reportConnectionString = "") {
            try {
                IList<string> columnNameList = new List<string>();
                IList<string> typeNameList = new List<string>();
                IList<int> typeValueList = new List<int>();

                foreach (ColumnInfoView columnInfoView in columnInfo) {

                    if (website && columnInfoView.Website.Equals(false)) {
                        continue;
                    }

                    columnNameList.Add(columnInfoView.ColumnName);
                    typeNameList.Add(columnInfoView.TypeName);
                    typeValueList.Add(columnInfoView.TypeValue);
                }

                log.Debug("=================MySQL========Line 122=========");
                log.DebugFormat("Create Entity {0} Table on MySQL", entityID);
                log.DebugFormat("connectionStringRA: {0}", MySQLServices.RemovePassword(connectionStringRA));

                var databaseMappingService = new DatabaseMappingService(connectionStringRA);
                string mySqlConnectionString = databaseMappingService.GetConnectionStringFor(systemSerial);
                log.DebugFormat("mySqlConnectionString: {0}", MySQLServices.RemovePassword(mySqlConnectionString));


                log.DebugFormat("loader: {0}", loader);


                //#if !DEBUG
                if (!loader) {
                    if (reportConnectionString.Length > 0) {
                        mySqlConnectionString = reportConnectionString;
                    }
                    else {
                        //If it is RG, use localhost
                        string serverName = Helper.FindKeyName(mySqlConnectionString, Helper._SERVERKEYNAME);
                        string port = Helper.FindKeyName(mySqlConnectionString, Helper._PORTKEYNAME);
                        log.DebugFormat("serverName: {0}", serverName);
                        log.DebugFormat("port: {0}", port);
        
                        mySqlConnectionString = mySqlConnectionString.Replace(serverName, Helper._LOCALHOST).Replace(port, Helper._LOCALPORT);
                    }
                }
                //#endif
				             
                log.DebugFormat("mySqlConnectionString: {0}", MySQLServices.RemovePassword(mySqlConnectionString));


                //if (mySqlConnectionString.Length.Equals(0)) {
                //    string tempMySqlConnection = "";

                //    //Create Database and update the table.
                //    var mySql = new FileTrendData(tempMySqlConnection);

                //    //mySqlConnectionString = mySql.CreateDatabaseFor("RemoteAnalystdb" + systemSerial);
                //    mySqlConnectionString = mySql.CreateDatabaseFor(databasePrefix + systemSerial);
                //    databaseMappingService.UpdateMySQLConnectionStringFor(systemSerial, mySqlConnectionString);
                //}

                log.DebugFormat("tableName: {0}", tableName);

                //Check if File Table is exists.
                string databaseName = Helper.FindKeyName(mySqlConnectionString, Helper._DATABASEKEYNAME);
                DataContext dc = new DataContext(mySqlConnectionString);
                var fileTrendData = new MySQLCommonRepository(dc);
                bool tableExists = fileTrendData.CheckTableExists(tableName);

                log.DebugFormat("tableExists: {0}", tableExists);


                if (!tableExists) {
                    var entityData = new EntityData(dc);
                    entityData.CreateEntityTableMySQLFor(tableName, columnNameList, typeNameList, typeValueList, false);
                }
            }
            catch (Exception ex) {
                log.Error("*******************************************************");
                log.ErrorFormat("MySql Error: {0}", ex);
            }
        }

        public void InsertEntityDatas(string systemSerial, string tableName, DataTable processData, 
            string connectionStringRA, string systemFolder, int entityID, bool loader, ILog log,
            string reportConnectionString = "") {
            //This is shared by Loader & RG, for RG the conn string should be localhost
            var databaseMappingService = new DatabaseMappingService(connectionStringRA);
            string mySqlConnectionString = databaseMappingService.GetConnectionStringFor(systemSerial);

            if (!loader) {
                if (reportConnectionString.Length > 0) {
                    mySqlConnectionString = reportConnectionString;
                }
                else {
                    string serverName = Helper.FindKeyName(mySqlConnectionString, Helper._SERVERKEYNAME);
                    string port = Helper.FindKeyName(mySqlConnectionString, Helper._PORTKEYNAME);
                    mySqlConnectionString = mySqlConnectionString.Replace(serverName, Helper._LOCALHOST).Replace(port, Helper._LOCALPORT);
                }
            }

            try {
                if (processData.Columns.Contains("DeltaTime")) { 
                    if (processData.AsEnumerable().Any(x => x.Field<double>("DeltaTime").Equals(0))) {
                        processData.AsEnumerable().Where(x => x.Field<double>("DeltaTime").Equals(0))
                            .ToList().ForEach(row => row.Delete());
                    }
                }
            }
            catch (Exception ex) {
                log.Error("*******************************************************");
                log.ErrorFormat("MySql Error: {0}", ex);
            }

            if (!Directory.Exists(systemFolder + systemSerial + @"\")) {
                Directory.CreateDirectory(systemFolder + systemSerial + @"\");
            }

            var entityData = new EntityData(new DataContext(mySqlConnectionString));
            entityData.PopulateEntity(tableName, processData, systemFolder + systemSerial + @"\", entityID);
        }
    }
}