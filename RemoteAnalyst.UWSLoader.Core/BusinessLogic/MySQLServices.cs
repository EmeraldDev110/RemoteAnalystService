using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using DataBrowser.Context;
using log4net;
using MySQLDataBrowser.Model;
using RemoteAnalyst.UWSLoader.Core.ModelView;


namespace RemoteAnalyst.UWSLoader.Core.BusinessLogic {
    class MySQLServices {
        private bool _remoteAnalyst;
        public MySQLServices(bool remoteAnalyst) {
            _remoteAnalyst = remoteAnalyst;
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

        public void CreateEntityTable(int entityID, string systemSerial, string tableName, IList<ColumnInfoView> columnInfo, bool website, string ConnectionStringDB, ILog log, bool isProcessDirectlySystem) {
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

                log.Info("=================MySQL======Line 69===========");
                log.InfoFormat("Create Entity {0} Table on MySQL", entityID);
                string mySqlConnectionString = ConnectionStringDB;
                log.InfoFormat("mySqlConnectionString: {0}", MySQLServices.RemovePassword(mySqlConnectionString));

                //Check if File Table is exists.
                DataContext dataContext = new DataContext(mySqlConnectionString);
                var fileTrendData = new FileTrendData(dataContext);
                bool tableExists = fileTrendData.CheckTableNameFor(tableName);

                log.InfoFormat("tableExists: {0}", tableExists);
                if (!tableExists) {
                    var entityData = new EntityData(dataContext);
                    entityData.CreateEntityTableMySQLFor(tableName, columnNameList, typeNameList, typeValueList, isProcessDirectlySystem);
                }
            }
            catch (Exception ex) {
                log.Error("*******************************************************");
                log.ErrorFormat("MySql Error: {0}", ex);                
            }
        }

        public void InsertEntityDatas(string systemSerial, string tableName, DataTable processData, string ConnectionStringDB, string systemFolder, int entityID) {
            string mySqlConnectionString = ConnectionStringDB;

            DataContext dataContext = new DataContext(mySqlConnectionString);
            var entityData = new EntityData(dataContext);
            entityData.PopulateEntity(tableName, processData, systemFolder + systemSerial + @"\", entityID);
        }
    }
}
