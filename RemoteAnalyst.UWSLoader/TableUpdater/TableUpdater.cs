using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using RemoteAnalyst.UWSLoader.TableUpdater.Models;
using RemoteAnalyst.UWSLoader.BLL;
using log4net;

namespace RemoteAnalyst.UWSLoader.TableUpdater {
    public class TableUpdater {
        private readonly string _systemConnectionString;
        private static readonly ILog Log = LogManager.GetLogger("DBHouseKeeping");
        public TableUpdater(string systemConnectionString) {
            _systemConnectionString = systemConnectionString;
        }

        public void UpdateTablePrimaryKeys() {
            Log.InfoFormat("Starting Table Update for: {0}", DiskLoader.RemovePassword(_systemConnectionString));
            List<TableInformation> tableInfo = GetTableChanges();
            MakeTableChanges(tableInfo, _systemConnectionString);
            Log.Info("Finishing Table Update");
        }

        private void MakeTableChanges(List<TableInformation> tableList, string connectionString) {
            Log.Info("Making Table Changes");
            
            QueryHelper helper = new QueryHelper();
            string query;

            foreach(TableInformation table in tableList) {

                if (!helper.HasPrimaryKey(connectionString, table.TableName, Log)) {
                    query = CreateUpdateInfoColumnQuery(table);
                    if (query.Length > 0) {
                        helper.ExecuteQuery(connectionString, query, Log);
                    }

                    query = CreateUpdateColumnQuery(table);
                    if (query.Length > 0) {
                        helper.ExecuteQuery(connectionString, query, Log);
                    }

                    query = CreateAddColumnQuery(table);
                    if (query.Length > 0) {
                        helper.ExecuteQuery(connectionString, query, Log);
                    }

                    query = CreateAddPrimaryKeyQuery(table);
                    if (!(table.PrimaryKey is null)) {
                        helper.ExecuteQuery(connectionString, query, Log);
                    }
                }                    
            }
        } 

        private string CreateUpdateColumnQuery(TableInformation table) {
            if ((table.RowUpdates is null) || table.RowUpdates.Count <= 0) return "";

            StringBuilder query = new StringBuilder();

            query.Append("ALTER TABLE " + table.TableName + " ");

            foreach(string columnUpdate in table.RowUpdates) {
                query.Append("MODIFY COLUMN " + columnUpdate + ",");
            }

            if (table.RowUpdates.Count > 0) query.Length--;

            return query.ToString();
        }

        private string CreateUpdateInfoColumnQuery(TableInformation table) {
            if ((table.RowUpdateInfo is null) || table.RowUpdateInfo.Count <= 0) return "";
            StringBuilder query = new StringBuilder();

            query.Append("UPDATE " + table.TableName + " ");

            foreach (string columnUpdateInfo in table.RowUpdateInfo) {
                query.Append(columnUpdateInfo + ",");
            }

            if (table.RowUpdateInfo.Count > 0) query.Length--;

            return query.ToString();
        }

        private string CreateAddColumnQuery(TableInformation table) {
            if ((table.RowAdds is null) || table.RowAdds.Count <= 0) return "";

            StringBuilder query = new StringBuilder();

            query.Append("ALTER TABLE " + table.TableName + " ");

            foreach (string columnUpdate in table.RowAdds) {
                query.Append("ADD COLUMN " + columnUpdate + ",");
            }

            if (table.RowAdds.Count > 0) query.Length--;

            return query.ToString();
        }


        private string CreateAddPrimaryKeyQuery(TableInformation table) {
            StringBuilder query = new StringBuilder();

            query.Append("ALTER TABLE " + table.TableName + " ADD PRIMARY KEY " + table.PrimaryKey);

            return query.ToString();
        }

        private List<TableInformation> GetTableChanges() {
            Log.Info("Creating Table Changes");
            
            TableInformation newTable = new TableInformation();
            List<TableInformation> newTableList = new List<TableInformation>();
            newTable.RowUpdates = new List<string>();
            newTable.RowAdds = new List<string>();

            // TransactionProfileTrends
            newTable.TableName = "TransactionProfileTrends";
            newTable.RowUpdates.Add("`FromDateTime` datetime NOT NULL");
            newTable.RowUpdates.Add("`ToDateTime` datetime NOT NULL");
            newTable.PrimaryKey = "(`ProfileId`, `FromDateTime`, `ToDateTime`)";
            newTableList.Add(newTable);


            // QNM_About
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.RowAdds = new List<string>();
            newTable.TableName = "QNM_About";
            newTable.RowAdds.Add("`ID` int PRIMARY KEY NOT NULL AUTO_INCREMENT");
            newTableList.Add(newTable);

            // Notifications
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.RowUpdateInfo = new List<string>();
            newTable.TableName = "Notifications";
            newTable.RowUpdateInfo.Add("SET `Value` = 0 WHERE `Value` IS NULL");
            newTable.RowUpdates.Add("Notification nvarchar (50) NOT NULL");
            newTable.RowUpdates.Add("`Value` int NOT NULL");
            newTable.PrimaryKey = "(`Notification`, `Value`)";
            newTableList.Add(newTable);
            

            // PredefineReports
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "PredefineReports";
            newTable.RowUpdates.Add("ReportType int NOT NULL");
            newTable.RowUpdates.Add("ReportID int NOT NULL");
            newTable.PrimaryKey = "(`ReportType`, `ReportID`)";
            newTableList.Add(newTable);

            // TableTimestamp
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "TableTimestamp";
            newTable.RowUpdates.Add("Start datetime NOT NULL");
            newTable.RowUpdates.Add("End datetime NOT NULL");
            newTable.PrimaryKey = "(`TableName`, `Start`, `End`)";
            newTableList.Add(newTable);

            // TempTableTimestamp
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "TempTableTimestamp";
            newTable.RowUpdates.Add("Start datetime NOT NULL");
            newTable.RowUpdates.Add("End datetime NOT NULL");
            newTable.PrimaryKey = "(`TableName`, `Start`, `End`)";
            newTableList.Add(newTable);

            // EntitiesDB
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "EntitiesDB";
            newTable.PrimaryKey = "(`EntityID`, `CounterID`)";
            newTableList.Add(newTable);

            // QuickTunerThresholds
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "QuickTunerThresholds";
            newTable.PrimaryKey = "(`CPUType`, `Report`, `Parameter`)";
            newTableList.Add(newTable);

            // QuickTuners
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "QuickTuners";
            newTable.RowUpdates.Add("QTID int NOT NULL");
            newTable.PrimaryKey = "(`QTID`)";
            newTableList.Add(newTable);       

            // QTLInks
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "QTLInks";
            newTable.RowUpdates.Add("LinkQTID int NOT NULL");
            newTable.RowUpdates.Add("LinkOrder int NOT NULL");
            newTable.PrimaryKey = "(`QTID`, `LinkQTID`, `LinkOrder`)";
            newTableList.Add(newTable);
            
            // CounterDataDictionary
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "CounterDataDictionary";
            newTable.PrimaryKey = "(`CounterKeyType`, `DataType`, `CPUType`, `CPUSubType`, `FieldName`, `MinimumNumericValue`, `MaximumNumericValue`)";
            newTableList.Add(newTable);
            
            // AlertSummary
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "AlertSummary";
            newTable.PrimaryKey = "(`SystemSerial`, `AlertDate`)";
            newTableList.Add(newTable);
            
            // SystemInterval
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.RowAdds = new List<string>();
            newTable.TableName = "SystemInterval";
            newTable.RowAdds.Add("`ID` int PRIMARY KEY NOT NULL AUTO_INCREMENT");
            newTableList.Add(newTable);
            
            // UWSLoadingStatus
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "UWSLoadingStatus";
            newTable.RowUpdates.Add("SystemSerial varchar(10) NOT NULL");
            newTable.RowUpdates.Add("FileName varchar(300) NOT NULL");
            newTable.PrimaryKey = "(`SystemSerial`, `FileName`)";
            newTableList.Add(newTable);
            
            // PvCollects
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "PvCollects";
            newTable.RowUpdates.Add("FromTimestamp datetime NOT NULL");
            newTable.RowUpdates.Add("ToTimestamp datetime NOT NULL");
            newTable.PrimaryKey = "(`FromTimestamp`, `ToTimestamp`)";
            newTableList.Add(newTable);
            
            // PvCpumany
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "PvCpumany";
            newTable.RowUpdates.Add("FromTimestamp datetime NOT NULL");
            newTable.RowUpdates.Add("ToTimestamp datetime NOT NULL");
            newTable.PrimaryKey = "(`FromTimestamp`, `ToTimestamp`, `CpuNumber`)";
            newTableList.Add(newTable);
            
            // PvCpuonce
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "PvCpuonce";
            newTable.RowUpdates.Add("FromTimestamp datetime NOT NULL");
            newTable.RowUpdates.Add("ToTimestamp datetime NOT NULL");
            newTable.PrimaryKey = "(`FromTimestamp`, `ToTimestamp`, `CpuNumber`)";
            newTableList.Add(newTable);
            
            // PvErrinfo
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "PvErrinfo";
            newTable.RowUpdates.Add("FromTimestamp datetime NOT NULL");
            newTable.RowUpdates.Add("ToTimestamp datetime NOT NULL");
            newTable.PrimaryKey = "(`FromTimestamp`, `ToTimestamp`, `PathwayName`, `ErrCurrentEntity`, `ErrCommand`)";
            newTableList.Add(newTable);
            
            // PvLmstus
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "PvLmstus";
            newTable.RowUpdates.Add("FromTimestamp datetime NOT NULL");
            newTable.RowUpdates.Add("ToTimestamp datetime NOT NULL");
            newTable.PrimaryKey = "(`FromTimestamp`, `ToTimestamp`, `LinkmonName`)";
            newTableList.Add(newTable);
            
            // PvPwylist
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "PvPwylist";
            newTable.RowUpdates.Add("FromTimestamp datetime NOT NULL");
            newTable.RowUpdates.Add("ToTimestamp datetime NOT NULL");
            newTable.PrimaryKey = "(`FromTimestamp`, `ToTimestamp`, `PathwayName`)";
            newTableList.Add(newTable);
            
            // PvPwymany
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "PvPwymany";
            newTable.RowUpdates.Add("FromTimestamp datetime NOT NULL");
            newTable.RowUpdates.Add("ToTimestamp datetime NOT NULL");
            newTable.PrimaryKey = "(`FromTimestamp`, `ToTimestamp`, `PathwayName`)";
            newTableList.Add(newTable);
            
            // PvPwyonce
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "PvPwyonce";
            newTable.RowUpdates.Add("FromTimestamp datetime NOT NULL");
            newTable.RowUpdates.Add("ToTimestamp datetime NOT NULL");
            newTable.PrimaryKey = "(`FromTimestamp`, `ToTimestamp`, `PathwayName`)";
            newTableList.Add(newTable);
            
            // PvScassign
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "PvScassign";
            newTable.RowUpdates.Add("FromTimestamp datetime NOT NULL");
            newTable.RowUpdates.Add("ToTimestamp datetime NOT NULL");
            newTable.PrimaryKey = "(`FromTimestamp`, `ToTimestamp`, `PathwayName`, `ScName`, `LogicalFile`)";
            newTableList.Add(newTable);
            
            // PvScdefine
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "PvScdefine";
            newTable.RowUpdates.Add("FromTimestamp datetime NOT NULL");
            newTable.RowUpdates.Add("ToTimestamp datetime NOT NULL");
            newTable.PrimaryKey = "(`FromTimestamp`, `ToTimestamp`, `PathwayName`, `ScName`, `DefineName`)";
            newTableList.Add(newTable);
            
            // PvScinfo
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "PvScinfo";
            newTable.RowUpdates.Add("FromTimestamp datetime NOT NULL");
            newTable.RowUpdates.Add("ToTimestamp datetime NOT NULL");
            newTable.PrimaryKey = "(`FromTimestamp`, `ToTimestamp`, `PathwayName`, `ScName`)";
            newTableList.Add(newTable);
            
            // PvSclstat
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "PvSclstat";
            newTable.RowUpdates.Add("FromTimestamp datetime NOT NULL");
            newTable.RowUpdates.Add("ToTimestamp datetime NOT NULL");
            newTable.RowUpdates.Add("ScLmName nvarchar(15) NOT NULL");
            newTable.PrimaryKey = "(`FromTimestamp`, `ToTimestamp`, `PathwayName`, `ScName`, `ScLmName`)";
            newTableList.Add(newTable);
            
            // PvScparam
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "PvScparam";
            newTable.RowUpdates.Add("FromTimestamp datetime NOT NULL");
            newTable.RowUpdates.Add("ToTimestamp datetime NOT NULL");
            newTable.PrimaryKey = "(`FromTimestamp`, `ToTimestamp`, `PathwayName`, `ScName`, `ParamName`)";
            newTableList.Add(newTable);
            
            // PvScproc
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "PvScproc";
            newTable.RowUpdates.Add("FromTimestamp datetime NOT NULL");
            newTable.RowUpdates.Add("ToTimestamp datetime NOT NULL");
            newTable.PrimaryKey = "(`FromTimestamp`, `ToTimestamp`, `PathwayName`, `ScName`, `ScProcessName`)";
            newTableList.Add(newTable);
            
            // PvScprstus
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "PvScprstus";
            newTable.RowUpdates.Add("FromTimestamp datetime NOT NULL");
            newTable.RowUpdates.Add("ToTimestamp datetime NOT NULL");
            newTable.PrimaryKey = "(`FromTimestamp`, `ToTimestamp`, `PathwayName`, `ScName`, `ScProcessName`)";
            newTableList.Add(newTable);
            
            // PvScstus
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "PvScstus";
            newTable.RowUpdates.Add("FromTimestamp datetime NOT NULL");
            newTable.RowUpdates.Add("ToTimestamp datetime NOT NULL");
            newTable.PrimaryKey = "(`FromTimestamp`, `ToTimestamp`, `PathwayName`, `ScName`)";
            newTableList.Add(newTable);
            
            // PvSctstat
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "PvSctstat";
            newTable.RowUpdates.Add("FromTimestamp datetime NOT NULL");
            newTable.RowUpdates.Add("ToTimestamp datetime NOT NULL");
            newTable.RowUpdates.Add("ScTcpName nvarchar(15) NOT NULL");
            newTable.PrimaryKey = "(`FromTimestamp`, `ToTimestamp`, `PathwayName`, `ScName`, `ScTcpName`)";
            newTableList.Add(newTable);
            
            // PvTcpinfo
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "PvTcpinfo";
            newTable.RowUpdates.Add("FromTimestamp datetime NOT NULL");
            newTable.RowUpdates.Add("ToTimestamp datetime NOT NULL");
            newTable.PrimaryKey = "(`FromTimestamp`, `ToTimestamp`, `PathwayName`, `TcpName`)";
            newTableList.Add(newTable);          

            // PvTcpstat
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "PvTcpstat";
            newTable.RowUpdates.Add("FromTimestamp datetime NOT NULL");
            newTable.RowUpdates.Add("ToTimestamp datetime NOT NULL");
            newTable.PrimaryKey = "(`FromTimestamp`, `ToTimestamp`, `PathwayName`, `TcpName`)";
            newTableList.Add(newTable);
            
            // PvTcpstus
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "PvTcpstus";
            newTable.RowUpdates.Add("FromTimestamp datetime NOT NULL");
            newTable.RowUpdates.Add("ToTimestamp datetime NOT NULL");
            newTable.PrimaryKey = "(`FromTimestamp`, `ToTimestamp`, `PathwayName`, `TcpName`)";
            newTableList.Add(newTable);
            
            // PvTerminfo
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "PvTerminfo";
            newTable.RowUpdates.Add("FromTimestamp datetime NOT NULL");
            newTable.RowUpdates.Add("ToTimestamp datetime NOT NULL");
            newTable.PrimaryKey = "(`FromTimestamp`, `ToTimestamp`, `PathwayName`, `TcpName`, `TermName`)";
            newTableList.Add(newTable);
            
            // PvTermstat
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "PvTermstat";
            newTable.RowUpdates.Add("FromTimestamp datetime NOT NULL");
            newTable.RowUpdates.Add("ToTimestamp datetime NOT NULL");
            newTable.PrimaryKey = "(`FromTimestamp`, `ToTimestamp`, `PathwayName`, `TermName`)";
            newTableList.Add(newTable);
            
            // PvTermstus
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "PvTermstus";
            newTable.RowUpdates.Add("FromTimestamp datetime NOT NULL");
            newTable.RowUpdates.Add("ToTimestamp datetime NOT NULL");
            newTable.PrimaryKey = "(`FromTimestamp`, `ToTimestamp`, `PathwayName`, `TermName`)";
            newTableList.Add(newTable);
            
            // ApplicationProfileData
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.RowAdds = new List<string>();
            newTable.TableName = "ApplicationProfileData";
            newTable.RowAdds.Add("`ID` int PRIMARY KEY NOT NULL AUTO_INCREMENT");
            newTableList.Add(newTable);

            // UWSArchive
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "UWSArchive";
            newTable.RowUpdates.Add("`FromTimestamp` datetime NOT NULL");
            newTable.RowUpdates.Add("`ToTimestamp` datetime NOT NULL");
            newTable.RowUpdates.Add("`ArchiveID` varchar(300)");
            newTable.PrimaryKey = "(`FromTimestamp`, `ToTimestamp`, `ArchiveID`)";
            newTableList.Add(newTable);
            
            // QNMArchive
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "QNMArchive";
            newTable.RowUpdates.Add("`FromTimestamp` datetime NOT NULL");
            newTable.RowUpdates.Add("`ToTimestamp` datetime NOT NULL");
            newTable.RowUpdates.Add("`ArchiveID` varchar(300)");
            newTable.PrimaryKey = "(`FromTimestamp`, `ToTimestamp`, `ArchiveID`)";
            newTableList.Add(newTable);
            
            // PathwayArchive
            newTable = new TableInformation();
            newTable.RowUpdates = new List<string>();
            newTable.TableName = "PathwayArchive";
            newTable.RowUpdates.Add("`FromTimestamp` datetime NOT NULL");
            newTable.RowUpdates.Add("`ToTimestamp` datetime NOT NULL");
            newTable.RowUpdates.Add("`ArchiveID` varchar(300)");
            newTable.PrimaryKey = "(`FromTimestamp`, `ToTimestamp`, `ArchiveID`)";
            newTableList.Add(newTable);

            return newTableList;
        }
    }
}
