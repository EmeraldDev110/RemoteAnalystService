using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using RemoteAnalyst.AWS.Glacier;
using RemoteAnalyst.AWS.S3;
using RemoteAnalyst.BusinessLogic.Enums;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;
using Helper = RemoteAnalyst.BusinessLogic.Util.Helper;
using RemoteAnalyst.UWSLoader.BLL;
using log4net;

namespace RemoteAnalyst.UWSLoader.SPAM.BLL {
    class OpenUWSQNMCLIM
    {
        private static readonly ILog Log = LogManager.GetLogger("QNMCLIMLoad");
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

        public OpenUWSQNMCLIM(string systemSerial, string csvLocation, int ntsId, string databasePrefix, int uwsId) {
            if(!string.IsNullOrEmpty(ConnectionString.S3FTP))
                _s3Location = ConnectionString.S3FTP;

            _systemSerial = systemSerial;
            _csvLocation = csvLocation;
            _databaseMapService = new DatabaseMappingService(_connectionString);
            _databaseService = new DatabaseService(_connectionString);
            _ntsId = ntsId;
            _databasePrefix = databasePrefix;
            _uwsId = uwsId;
            if (!ConnectionString.IsLocalAnalyst)
            {
                _s3 = new AmazonS3(_s3Location);
            }
        }

        public void CreateNewData() {
            var loadingInfo = new LoadingInfoService(ConnectionString.ConnectionStringDB);
            try {
                if(_uwsId == 0)
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
                if (ConnectionString.IsLocalAnalyst) {
                    if (File.Exists(_csvLocation)) {
                        var fileInfoCSV = new FileInfo(_csvLocation);
                        if (!Directory.Exists(ConnectionString.ServerPath + "Systems\\" + _systemSerial + "\\"))
                            Directory.CreateDirectory(ConnectionString.ServerPath + "Systems\\" + _systemSerial + "\\");

                        saveLocation = ConnectionString.ServerPath + "Systems\\" + _systemSerial + "\\" + fileInfoCSV.Name;
                        fileInfoCSV.CopyTo(saveLocation);
                    }
                }
                else
                {
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

                #region Insert Data

                var myDataSet = new DataSet();
                using (var reader = new StreamReader(saveLocation)) {
                    Log.Info("Start inserting data");
                    
                    string type = "";
                    while (!reader.EndOfStream) {
                        string line = reader.ReadLine();
                        if (string.IsNullOrEmpty(line)) {
                            continue;
                        }

                        string[] values = line.Split(',');
                        if (values[0].Length.Equals(1)) {
                            char identifier = Convert.ToChar(values[0]);
                            //Skip the header
                            if (identifier == 'H') {
                                continue;
                            }
                        }

                        type = values[0];
                        if (type.Equals("404")) {   //CPU
                            if (!myDataSet.Tables.Contains("CPU")) {
                                //Add DataTable.
                                var table = new DataTable("CPU");
                                var col = new DataColumn("Date Time") {
                                    DataType = typeof(string)
                                };
                                table.Columns.Add(col);
                                col = new DataColumn("CLIM Name") {
                                    DataType = typeof(string)
                                };
                                table.Columns.Add(col);
                                col = new DataColumn("CPU Busy") {
                                    DataType = typeof(double)
                                };
                                table.Columns.Add(col);
                                col = new DataColumn("Memory Free") {
                                    DataType = typeof(double)
                                };
                                table.Columns.Add(col);
                                myDataSet.Tables.Add(table);
                            }
                            DataRow dataRow = myDataSet.Tables["CPU"].NewRow();
                            dataRow["Date Time"] = values[1];
                            dataRow["CLIM Name"] = values[2];
                            dataRow["CPU Busy"] = Convert.ToDouble(values[3]);
                            dataRow["Memory Free"] = Convert.ToDouble(values[4]);
                            myDataSet.Tables["CPU"].Rows.Add(dataRow);
                        }
                        else if (type.Equals("405")) {  //DISC
                            if (!myDataSet.Tables.Contains("DISK")) {
                                //Add DataTable.
                                var table = new DataTable("DISK");
                                var col = new DataColumn("Date Time") {
                                    DataType = typeof(string)
                                };
                                table.Columns.Add(col);
                                col = new DataColumn("CLIM Name") {
                                    DataType = typeof(string)
                                };
                                table.Columns.Add(col);
                                col = new DataColumn("Size") {
                                    DataType = typeof(double)
                                };
                                table.Columns.Add(col);
                                col = new DataColumn("Used") {
                                    DataType = typeof(double)
                                };
                                table.Columns.Add(col);
                                myDataSet.Tables.Add(table);
                            }
                            DataRow dataRow = myDataSet.Tables["Disk"].NewRow();
                            dataRow["Date Time"] = values[1];

                            //Check CLIM Name is in correct format.
                            var climName = values[2].Split('.');

                            if (climName.Length == 2)
                                dataRow["CLIM Name"] = values[2];
                            else {
                                dataRow["CLIM Name"] = climName[0] + "." + climName[climName.Length - 1];
                            }
                            dataRow["Size"] = Convert.ToDouble(values[3]);
                            dataRow["Used"] = Convert.ToDouble(values[4]);
                            myDataSet.Tables["Disk"].Rows.Add(dataRow);
                        }
                    }

                    //Insert data.
                    if (myDataSet.Tables["CPU"] != null && myDataSet.Tables["CPU"].Rows.Count > 0) {
                        InsertCollectedData(myDataSet.Tables["CPU"], "CPU");
                    }
                    if (myDataSet.Tables["Disk"] != null && myDataSet.Tables["Disk"].Rows.Count > 0) {
                        //Check of missing disk data using CPU data.
                        var cpuTimestamps = myDataSet.Tables["CPU"].AsEnumerable().Select(x => x.Field<string>("Date Time")).Distinct().ToList();
                        var cpuClimNames = myDataSet.Tables["CPU"].AsEnumerable().Select(x => x.Field<string>("CLIM Name")).Distinct().ToList();

                        foreach (var climName in cpuClimNames) {
                            var newClimName = climName.Split('.')[0];
                            foreach (var timestamp in cpuTimestamps) {
                                if (!myDataSet.Tables["Disk"].AsEnumerable().Any(x => x.Field<string>("CLIM Name").StartsWith(newClimName) &&
                                                                                     x.Field<string>("Date Time").Equals(timestamp))) {
                                    //Get Disk Clim Name
                                    var diskClimName = myDataSet.Tables["Disk"].AsEnumerable().Where(x => x.Field<string>("CLIM Name").StartsWith(newClimName))
                                        .Select(x => x.Field<string>("CLIM Name")).FirstOrDefault();

                                    if (diskClimName != null) {
                                        if (diskClimName.Length > 0) {
                                            //Insert dummy data.
                                            DataRow dataRow = myDataSet.Tables["Disk"].NewRow();
                                            dataRow["Date Time"] = timestamp;
                                            dataRow["CLIM Name"] = diskClimName;
                                            dataRow["Size"] = -1;
                                            dataRow["Used"] = -1;
                                            myDataSet.Tables["Disk"].Rows.Add(dataRow);
                                        }
                                    }
                                }
                            }
                        }

                        InsertCollectedData(myDataSet.Tables["Disk"], "Disk");
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

            }
            catch (Exception ex) {
                Log.InfoFormat("Exception loading data: {0}", ex.Message);
                

                if (_ntsId != 0) {
                    var uploadMessage = new UploadMessagesService(ConnectionString.ConnectionStringDB);
                    uploadMessage.InsertNewEntryFor(_ntsId, DateTime.Now, "Load Failed");
                }
            }
            finally {
                try {
                    loadingInfo.UpdateFor(_uwsId, _systemName, _fromDate, _toDate, 5); //type 5 is QNM
                }
                catch (Exception ex) {
                    Log.ErrorFormat("Loading Info Error2: {0}", ex.Message);
                    
                }
                Log.Info("Load QNM done");
            }
        }

        private void InsertCollectedData(DataTable myDataTable, string title) {
            switch (title) {
                case "CPU":
                    Log.Info("--- QNM_CLIMCPUDetail ---");
                    var cols = new List<string> {
                        "Date Time",
                        "CLIM Name",
                        "CPU Busy",
                        "Memory Free"
                    };
                    var success = PopulateTable("QNM_CLIMCPUDetail", myDataTable, QNM.QNM_CLIMCPUDetail, cols, ConnectionString.DatabasePrefix);
                    Log.InfoFormat("Populate QNM_CLIMCPUDetail success: {0}", success);
                    
                    break;
                case "Disk":
                    Log.Info("--- QNM_CLIMDiskDetail ---");
                    cols = new List<string> {
                        "Date Time",
                        "CLIM Name",
                        "Size",
                        "Used"
                    };
                    success = PopulateTable("QNM_CLIMDiskDetail", myDataTable, QNM.QNM_CLIMDiskDetail, cols, ConnectionString.DatabasePrefix);
                    Log.InfoFormat("Populate QNM_CLIMDiskDetail success: {0}", success);
                    
                    break;
            }
        }

        private bool PopulateTable(string partialName, DataTable table, string cmdText, List<string> cols, string databasePrefix) {
            var tables = new Dictionary<string, DataTable>();
            //Get from and to time.
            var _fromDate = Convert.ToDateTime(table.AsEnumerable().Select(x => x.Field<string>("Date Time")).Distinct().OrderBy(x => x).FirstOrDefault());
            var _toDate = Convert.ToDateTime(table.AsEnumerable().Select(x => x.Field<string>("Date Time")).Distinct().OrderBy(x => x).LastOrDefault());

            for (DateTime d = _fromDate; d.Date <= _toDate.Date; d = d.AddDays(1)) {
                DateTime d1 = d;
                DataTable temp = table.AsEnumerable().Where(x => DateTime.Parse(x.Field<string>("Date Time")).Date.Equals(d1.Date)).CopyToDataTable();
                tables.Add(partialName + "_" + d1.Date.ToString("yyyy_M_d"), temp);
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
                Log.InfoFormat("Insert data for {0}:{1}", t.Key, success);
                if (!success) {
                    return false;
                }
                exists = _qnm.CheckIndexExists(t.Key);
                if (!exists) {
                    success = _qnm.CreateIndex(t.Key, "Idx_" + t.Key, cols, Log);
                    Log.InfoFormat("Add index for {0}:{1}", t.Key, success);
                }
                else {
                    Log.InfoFormat("Index for {0} already exists", t.Key);
                }
            }
            return success;
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

    }
}
