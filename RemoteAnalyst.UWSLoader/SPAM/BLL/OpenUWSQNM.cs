using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using IntervalTrendLoader;
using log4net;
using RemoteAnalyst.AWS.Glacier;
using RemoteAnalyst.AWS.S3;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;
using RemoteAnalyst.UWSLoader.BLL;

namespace RemoteAnalyst.UWSLoader.SPAM.BLL {
    public class OpenUWSQNM {
        private static readonly ILog Log = LogManager.GetLogger("QNMLoad");
        private readonly string _connectionString = ConnectionString.ConnectionStringDB;
        private readonly string _csvLocation; //On S3
        private readonly DatabaseMappingService _databaseMapService;
        private readonly DatabaseService _databaseService;
        private readonly AmazonS3 _s3;
        private readonly string _s3Location = ConnectionString.S3FTP;
        private readonly string _systemLocation = ConnectionString.SystemLocation;
        private readonly string _systemSerial;
        private DateTime _fromDate;
        private long _interval;
        private QNM _qnm;
        private DateTime _toDate;
        private string _systemName;
        private int _uwsId;
        private int _ntsId;
        private string _dataVersionNumber;
        private string _databasePrefix;

        public OpenUWSQNM(string systemSerial, string csvLocation, int ntsId, string databasePrefix, int uwsId) {
            if (!string.IsNullOrEmpty(ConnectionString.S3FTP))
                _s3Location = ConnectionString.S3FTP;

            _systemSerial = systemSerial;
            _csvLocation = csvLocation;
            _databaseMapService = new DatabaseMappingService(_connectionString);
            _databaseService = new DatabaseService(_connectionString);
            _ntsId = ntsId;
            _uwsId = uwsId;
            _databasePrefix = databasePrefix;

            if (!ConnectionString.IsLocalAnalyst) { 
                _s3 = new AmazonS3(_s3Location);
            }
        }

        public void CreateNewData() {
            var loadingInfo = new LoadingInfoService(ConnectionString.ConnectionStringDB);
            try {
                if (_uwsId == 0)
                    _uwsId = loadingInfo.GetMaxUWSIDFor();

                var systemInfo = new System_tblService(ConnectionString.ConnectionStringDB);
                _systemName = systemInfo.GetSystemNameFor(_systemSerial);

                //Insert into 
                Log.Info("CreateNewData");
                string newConnectionString = _databaseMapService.GetConnectionStringFor(_systemSerial);
                Log.InfoFormat("newConnectionString: {0}", DiskLoader.RemovePassword(newConnectionString));
                

                if (newConnectionString.Length == 0) {
                    newConnectionString = Config.ConnectionString.Replace("RemoteAnalystdbSPAM", _databasePrefix + _systemSerial);
                    _databaseMapService.InsertNewEntryFor(_systemSerial, newConnectionString);
                }
                _qnm = new QNM(_systemLocation + _systemSerial + "/", newConnectionString);

                bool exists = _databaseMapService.CheckDatabaseFor(newConnectionString);
                Log.InfoFormat("Database exists: {0}", exists);
                

                if (!exists) {
                    Log.Info("Create Database");
                    
                    string databaseName = Helper.FindKeyName(newConnectionString, Helper._DATABASEKEYNAME);
                    newConnectionString = _databaseService.CreateDatabaseFor(databaseName, Log);
                }

                string saveLocation = "";
                if (ConnectionString.IsLocalAnalyst)
                {
                    Log.InfoFormat("_csvLocation: {0}", _csvLocation);


                    if (File.Exists(_csvLocation))
                    {
                        var fileInfoCSV = new FileInfo(_csvLocation);
                        if (!Directory.Exists(ConnectionString.ServerPath + "\\Systems\\" + _systemSerial + "\\"))
                            Directory.CreateDirectory(ConnectionString.ServerPath + "\\Systems\\" + _systemSerial + "\\");

                        saveLocation = ConnectionString.ServerPath + "\\Systems\\" + _systemSerial + "\\" + fileInfoCSV.Name;
                        fileInfoCSV.CopyTo(saveLocation);
                    }
                }
                else {
                    Log.InfoFormat("File location on S3: {0}", _csvLocation);
                    saveLocation = _s3.ReadS3(_csvLocation, ConnectionString.ServerPath);
                    Log.InfoFormat("saveLocation: {0}", saveLocation);
                }

                var fileInfo = new FileInfo(saveLocation);
                try {
                    loadingInfo.UpdateFor(fileInfo.Name, _systemSerial, fileInfo.Length.ToString(), "5", _uwsId.ToString()); //5: QNM

                    if (_ntsId != 0) {
                        var uploadMessage = new UploadMessagesService(ConnectionString.ConnectionStringDB);
                        uploadMessage.InsertNewEntryFor(_ntsId, DateTime.Now, "Loading " + fileInfo.Name + " to MySQL");
                    }
                }
                catch (Exception ex) {
                    Log.ErrorFormat("Loading Info Error1: {0}", ex.Message);
                    
                }

                //Get Data Version Number from the csv file.
                using (var reader = new StreamReader(saveLocation)) {
                    var firstLine = reader.ReadLine();
                    var tempValues = firstLine.Split('|');
                    _dataVersionNumber = tempValues[2];
                }

                #region Insert Data

                using (var reader = new StreamReader(saveLocation)) {
                    Log.Info("Start inserting data");
                    
                    string type = "";
                    while (!reader.EndOfStream) {
                        string line = reader.ReadLine();
                        if (string.IsNullOrEmpty(line)) {
                            continue;
                        }

                        string[] values = line.Split('|');
                        char identifier = Convert.ToChar(values[0]);
                        //Skip the header
                        if (identifier == 'H') {
                            continue;
                        }

                        string headerType = Convert.ToString(values[1]);
                        type = Convert.ToString(values[1]);
                        string title = Convert.ToString(values[2]);
                        long recordNumber = Convert.ToInt64(values[3]);

                        if (identifier == 'S' && recordNumber > 0) {
                            #region Multi table

                            var myDataSet = new DataSet();
                            for (long x = 0; x < recordNumber; x++) {
                                line = reader.ReadLine();
                                if (string.IsNullOrEmpty(line)) {
                                    continue;
                                }

                                //If the line is too long, the extra charaterters will be appended to second line
                                if (line.Length >= 239) {
                                    line = ReadMoreLines(line, reader, ref x);
                                }

                                //Read 'G' (Group Header) line
                                values = line.Split('|');
                                identifier = Convert.ToChar(values[0]);
                                headerType = Convert.ToString(values[1]);
                                long gCount = Convert.ToInt64(values[2]);
                                if (headerType.Length == 0) {
                                    headerType = x.ToString();
                                }

                                //Create the DataTable
                                var myDataTable = new DataTable(headerType);
                                DataRow myDataRow;
                                DataColumn myDataColumn;

                                //Loop through number of datarow
                                for (long i = 0; i < gCount; i++) {
                                    line = reader.ReadLine();
                                    if (string.IsNullOrEmpty(line)) {
                                        continue;
                                    }
                                    if (line.Length >= 239) {
                                        line = ReadMoreLines(line, reader, ref i);
                                    }

                                    values = line.Split('|');
                                    type = Convert.ToString(values[0]);

                                    //"T", "R", "I", "B"
                                    if (type == "T") {
                                        for (int o = 1; o < values.Length; o++) {
                                            myDataColumn = new DataColumn();
                                            myDataColumn.DataType = Type.GetType("System.String");
                                            myDataColumn.ColumnName = values[o];
                                            myDataTable.Columns.Add(myDataColumn);
                                        }
                                    }
                                    else if (type == "R") {
                                        if (myDataTable.Columns.Count == 0) {
                                            for (int o = 1; o < values.Length; o++) {
                                                myDataColumn = new DataColumn();
                                                myDataColumn.DataType = Type.GetType("System.String");
                                                myDataColumn.ColumnName = o.ToString();
                                                myDataTable.Columns.Add(myDataColumn);
                                            }
                                        }
                                        myDataRow = myDataTable.NewRow();
                                        for (int o = 1; o < values.Length; o++) {
                                            myDataRow[o - 1] = values[o];
                                        }
                                        myDataTable.Rows.Add(myDataRow);
                                    }
                                    else if (type == "I") {
                                        myDataColumn = new DataColumn();
                                        myDataColumn.DataType = Type.GetType("System.String");
                                        myDataColumn.ColumnName = "2"; //no format
                                        myDataTable.Columns.Add(myDataColumn);

                                        long iCount = Convert.ToInt64(values[2]);
                                        for (int o = 0; o < iCount; o++) {
                                            myDataRow = myDataTable.NewRow();
                                            myDataRow[0] = reader.ReadLine().Replace("B|", "");
                                            myDataTable.Rows.Add(myDataRow);
                                            i++;
                                        }
                                    }
                                    else if (type == "B") {
                                        for (int o = 1; o < values.Length; o++) {
                                            myDataColumn = new DataColumn();
                                            myDataColumn.DataType = Type.GetType("System.String");
                                            myDataColumn.ColumnName = o.ToString();
                                            myDataTable.Columns.Add(myDataColumn);
                                        }
                                        myDataRow = myDataTable.NewRow();
                                        for (int o = 1; o < values.Length; o++) {
                                            myDataRow[o - 1] = values[o];
                                        }
                                        myDataTable.Rows.Add(myDataRow);
                                    }
                                }
                                x += gCount;
                                myDataSet.Tables.Add(myDataTable);
                            }

                            if (myDataSet.Tables[0].Rows.Count > 0)
                                InsertCollectedData(myDataSet, title, ConnectionString.DatabasePrefix);

                            #endregion
                        }
                    }
                }
                Log.Info("Finish loading data");

                #endregion

                if (_ntsId != 0) {
                    var uploadMessage = new UploadMessagesService(ConnectionString.ConnectionStringDB);
                    uploadMessage.InsertNewEntryFor(_ntsId, DateTime.Now, "Finish Loading " + fileInfo.Name + " to MySQL");

                    var upload = new UploadService(ConnectionString.ConnectionStringDB);
                    upload.UpdateLoadedStatusFor(_ntsId, "Loaded");
                }

                try {
                    //Insert QNM Trend.
                    var intervalNetworkLoad = new IntervalNetworkLoad(_systemSerial, _fromDate, _toDate, _interval, Log);
                    intervalNetworkLoad.LoadNetwork();
                }
                catch (Exception ex) {
                    Log.ErrorFormat("Exception loading Trend data: {0}", ex.Message);
                    
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("Exception loading data: {0}", ex.Message);
                

                if (_ntsId != 0) {
                    var uploadMessage = new UploadMessagesService(ConnectionString.ConnectionStringDB);
                    uploadMessage.InsertNewEntryFor(_ntsId, DateTime.Now, "Load Failed");
                }
            }
            finally {
                try {
                    loadingInfo.UpdateFor(_uwsId, _systemName, _fromDate, _toDate, 5); //type 5 is QNM
					QNMDirectoriesService qnmDirectoriesService = new QNMDirectoriesService(ConnectionString.ConnectionStringDB);
					qnmDirectoriesService.InsertQNMDirectoryFor(_uwsId, _systemSerial, _fromDate, _toDate, _csvLocation);
                }
                catch (Exception ex) {
                    Log.ErrorFormat("Loading Info Error2: {0}", ex.Message);
                    
                }
                Log.Info("Load QNM done");
            }
        }

        private void InsertCollectedData(DataSet myDataSet, string title, string databasePrefix) {
            switch (title) {
                case "About":
                    Log.Info("--- About ---");
                    bool tableExists = _qnm.CheckTableExists("QNM_About", databasePrefix + _systemSerial);
                    Log.InfoFormat("QNM_About exsits: {0}", tableExists);
                    bool success = false;
                    if (!tableExists) {
                        _qnm.CreateTable(QNM.QNM_About);
                    }
                    bool entryExits = _qnm.CheckAboutExists(myDataSet.Tables[0]);
                    Log.InfoFormat("entryExits: {0}", entryExits);
                    if (!entryExits) {
                        _qnm.InsertAbout(myDataSet.Tables[0], _dataVersionNumber);
                    }
                    _fromDate = DateTime.Parse(myDataSet.Tables[0].Rows[1]["2"].ToString());
                    _toDate = DateTime.Parse(myDataSet.Tables[0].Rows[2]["2"].ToString());
                    Log.InfoFormat("_fromDate: {0}", _fromDate);
                    Log.InfoFormat("_toDate:" + _toDate);

                    if (_ntsId != 0) {
                        var uploads = new UploadService(ConnectionString.ConnectionStringDB);
                        uploads.UploadCollectionStartTimeFor(_ntsId, _fromDate);
                        uploads.UploadCollectionToTimeFor(_ntsId, _toDate);
                    }

                    string intervalStr = myDataSet.Tables[0].Rows[3]["2"].ToString();
                    try {
                        if (intervalStr.ToUpper().Contains("SECONDS")) {
                            _interval = long.Parse(intervalStr.Split(' ')[0]);
                        }
                        else {
                            _interval = long.Parse(intervalStr.Split(' ')[0]) * 60;
                        }
                    }
                    catch (Exception ex) {
                        Log.ErrorFormat("Parse interval failed: {0}", ex.Message);
                    }
                    break;
                case "TCP/IP Bytes":
                    Log.Info("--- QNM_TCPProcessDetail ---");
                    var cols = new List<string> {
                        "Date Time",
                        "Process Name"
                    };
                    success = PopulateTable("QNM_TCPProcessDetail", myDataSet.Tables[1], QNM.QNM_TCPProcessDetail, cols, ConnectionString.DatabasePrefix);
                    Log.InfoFormat("Populate QNM_TCPProcessDetail success: {0}", success);
                    
                    break;
                case "TCP/IP Packets":
                    Log.Info("--- QNM_TCPPacketsDetail ---");
                    cols = new List<string> {
                        "Date Time",
                        "Process Name"
                    };
                    success = PopulateTable("QNM_TCPPacketsDetail", myDataSet.Tables[1], QNM.QNM_TCPPacketsDetail, cols, ConnectionString.DatabasePrefix);
                    Log.InfoFormat("Populate QNM_TCPPacketsDetail success: {0}", success);
                    
                    break;
                case "TCP/IP Subnet Packets":
                    Log.Info("--- QNM_TCPSubnetDetail ---");
                    cols = new List<string> {
                        "Date Time",
                        "Subnet Process Name (IP Address)"
                    };
                    success = PopulateTable("QNM_TCPSubnetDetail", myDataSet.Tables[1], QNM.QNM_TCPSubnetDetail, cols, ConnectionString.DatabasePrefix);
                    Log.InfoFormat("Populate QNM_TCPSubnetDetail success: {0}", success);
                    
                    break;
                case "TCP/IPv6 Bytes":
                    Log.Info("--- QNM_TCPv6Detail ---");
                    cols = new List<string> {
                        "Date Time",
                        "Monitor Name"
                    };
                    success = PopulateTable("QNM_TCPv6Detail", myDataSet.Tables[1], QNM.QNM_TCPv6Detail, cols, ConnectionString.DatabasePrefix);
                    Log.InfoFormat("Populate QNM_TCPv6Detail success: {0}", success);
                    
                    break;
                case "TCP/IPv6 Subnet Packets":
                    Log.Info("--- QNM_TCPv6SubnetDetail ---");
                    cols = new List<string> {
                        "Date Time",
                        "SUBNET Monitor Name (IP Address)"
                    };
                    success = PopulateTable("QNM_TCPv6SubnetDetail", myDataSet.Tables[1], QNM.QNM_TCPv6SubnetDetail, cols, ConnectionString.DatabasePrefix);
                    Log.InfoFormat("Populate QNM_TCPv6SubnetDetail success: {0}", success);
                    break;
                case "SLSA Octets":
                    Log.Info("--- QNM_SLSASummary ---");
                    tableExists = _qnm.CheckTableExists("QNM_SLSASummary", databasePrefix + _systemSerial);
                    Log.InfoFormat("QNM_SLSASummary exsits: {0}", tableExists);
                    if (!tableExists) {
                        _qnm.CreateTable(QNM.QNM_SLSASummary);
                    }

                    //Remove Total Octets
                    myDataSet.Tables[0].Columns.Remove("Total Octets");

                    DataTable newDataTable = myDataSet.Tables[0].Clone();

                    List<string> pifNames = _qnm.GetAllUniquePIFNames();
                    foreach (DataRow row in myDataSet.Tables[0].Rows) {
                        if (!pifNames.Contains(row["PIF Name"].ToString())) {
                            newDataTable.Rows.Add(row.ItemArray);
                        }
                    }

                    success = _qnm.InsertData("`QNM_SLSASummary`", newDataTable, Log);
                    Log.InfoFormat("QNM_SLSASummary: {0}", success);

                    Log.Info("--- QNM_SLSADetail ---");
                    cols = new List<string> {
                        "Date Time",
                        "PIF Name"
                    };
                    success = PopulateTable("QNM_SLSADetail", myDataSet.Tables[1], QNM.QNM_SLSADetail, cols, ConnectionString.DatabasePrefix);
                    Log.InfoFormat("Populate QNM_SLSADetail success: {0}", success);
                    
                    break;
                case "CLIM Bytes":
                    Log.Info("--- QNM_CLIMDetail ---");
                    cols = new List<string> {
                        "Date Time",
                        "CLIM Name"
                    };
                    success = PopulateTable("QNM_CLIMDetail", myDataSet.Tables[1], QNM.QNM_CLIMDetail, cols, ConnectionString.DatabasePrefix);
                    Log.InfoFormat("Populate QNM_CLIMDetail success: {0}", success);
                    
                    break;
                case "Expand Path":
                    Log.Info("--- QNM_ExpandPathDetail ---");
                    cols = new List<string> {
                        "Date Time",
                        "Device Name"
                    };
                    success = PopulateTable("QNM_ExpandPathDetail", myDataSet.Tables[1], QNM.QNM_ExpandPathDetail, cols, ConnectionString.DatabasePrefix);
                    Log.InfoFormat("Populate QNM_ExpandPathDetail success: {0}", success);
                    
                    break;
                case "Probe Round Trips":
                    Log.Info("--- QNM_ProbeRoundTripDetail ---");
                    cols = new List<string> {
                        "Date Time",
                        "Selected System"
                    };
                    success = PopulateTable("QNM_ProbeRoundTripDetail", myDataSet.Tables[0], QNM.QNM_ProbeRoundTripDetail, cols, ConnectionString.DatabasePrefix);
                    Log.InfoFormat("Populate QNM_ProbeRoundTripDetail success: {0}", success);
                    
                    break;
            }
        }

        private string ReadMoreLines(string line, StreamReader reader, ref long i) {
            while (line.Length >= 239) {
                int peek = reader.Peek();
                if (peek == 13) {
                    reader.ReadLine();
                    peek = reader.Peek();
                }
                char temp = Convert.ToChar(peek);
                if (temp != 'T' && temp != 'R' && temp != 'I' && temp != 'B') {
                    line += reader.ReadLine();
                    i++;
                }
                //Check if peek is T, R, I, B
                peek = reader.Peek();
                temp = Convert.ToChar(peek);
                if (temp == 'S' || temp == 'T' || temp == 'R' || temp == 'I' || temp == 'B') {
                    break;
                }
            }
            return line;
        }

        private bool PopulateTable(string partialName, DataTable table, string cmdText, List<string> cols, string databasePrefix) {
            var tables = new Dictionary<string, DataTable>();
            for (DateTime d = _fromDate; d.Date <= _toDate.Date; d = d.AddDays(1)) {
                DateTime d1 = d;
                var rows = table.AsEnumerable().Where(x => DateTime.Parse(x.Field<string>("Date Time")).Date.Equals(d1.Date));
                if (rows.Any()) { 
                    tables.Add(partialName + "_" + d1.Date.ToString("yyyy_M_d"), rows.CopyToDataTable());
                }
            }

            bool success = false;
            foreach (KeyValuePair<string, DataTable> t in tables) {
                bool exists = _qnm.CheckTableExists(t.Key, databasePrefix + _systemSerial);
                if (!exists) {
                    string tempCmdText = cmdText.Replace(partialName, t.Key);
                    Log.InfoFormat("Create table: {0}", t.Key);
                    _qnm.CreateTable(tempCmdText);
                }
                else {
                    //Check if inserting the duplicate entries
                    DateTime fromDateTime = t.Value.AsEnumerable().Select(x => DateTime.Parse(x.Field<string>("Date Time"))).Min();
                    DateTime toDateTime = t.Value.AsEnumerable().Select(x => DateTime.Parse(x.Field<string>("Date Time"))).Max();
                    Log.InfoFormat("fromDateTime: {0}", fromDateTime);
                    Log.InfoFormat("toDateTime: {0}", toDateTime);
                    exists = _qnm.CheckDetailsExists(t.Key, fromDateTime.AddSeconds(_interval * 0.1), toDateTime.AddSeconds(-_interval * 0.1));
                    if (exists) {
                        Log.Info("Entries exist. Return");
                        return true;
                    }
                }
                success = _qnm.InsertData(t.Key, t.Value, Log);
                Log.InfoFormat("Insert data for {0}: {1}", t.Key, success);
                if (!success) {
                    return false;
                }
                exists = _qnm.CheckIndexExists(t.Key);
                if (!exists) {
                    success = _qnm.CreateIndex(t.Key, "Idx_" + t.Key, cols, Log);
                    Log.InfoFormat("Add index for {0}: {1}", t.Key, success);
                }
                else {
                    Log.InfoFormat("Index for {0} already exists", t.Key);
                }
            }
            return success;
        }
    }
}