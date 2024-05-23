using System.Linq;
using RemoteAnalyst.BusinessLogic.Enums;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.BusinessLogic.UWSLoader.BaseClass;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using RemoteAnalyst.Repository.Infrastructure;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;
using log4net;

namespace RemoteAnalyst.BusinessLogic.UWSLoader {
    public class LoadUWS : Header {
        private readonly string _connectionStr;
        private readonly bool _mainDBLoad;

        public LoadUWS(string connectionRemoteAnalyst, bool mainDBLoad) {
            _connectionStr = connectionRemoteAnalyst;
            _mainDBLoad = mainDBLoad;
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
        /// Open the UWS file and load data into the variables that defined in Header.
        /// </summary>
        /// <param name="uwsPath"> Full path of the UWS data file.</param>
        /// <param name="uwsVersion"> Enum class that tell version of UWS file</param>
        /// <param name="writer"> StreamWriter</param>
        /// <returns> Return a bool value suggests whether the reading is successful or not.</returns>
        public bool OpenNewUWSFile(string uwsPath, UWS.Types uwsVersion, ILog log) {
            //string UWSPath = Config.UWSPath;
            IHeaderInfo headerInfo = null;
            log.Info("OpenNewUWSFile");
            using (var stream = new FileStream(uwsPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                //using (StreamReader reader = new StreamReader(stream))
                using (reader = new BinaryReader(stream)) {
                    var myEncoding = new ASCIIEncoding();
                    if (uwsVersion == UWS.Types.Version2007) {
                        log.Info("Version 2007");
                        
                        #region Basic Header Info

                        //Identifier
                        reader.BaseStream.Seek(0, SeekOrigin.Begin);
                        UwsIdentifierByte = reader.ReadBytes(UwsIdentifierByte.Length);
                        UwsIdentifier = myEncoding.GetString(UwsIdentifierByte).Trim();

                        //Key.
                        reader.BaseStream.Seek(8, SeekOrigin.Begin);
                        UwsKeyByte = reader.ReadBytes(UwsKeyByte.Length);
                        UwsKey = myEncoding.GetString(UwsKeyByte).Trim();

                        //System Serial
                        reader.BaseStream.Seek(18, SeekOrigin.Begin);
                        SystemSerialByte = reader.ReadBytes(SystemSerialByte.Length);
                        UWSSerialNumber = myEncoding.GetString(SystemSerialByte).Trim();
                        while (UWSSerialNumber.Length < 6) {
                            UWSSerialNumber = "0" + UWSSerialNumber;
                        }

                        //UwsHLen
                        reader.BaseStream.Seek(28, SeekOrigin.Begin);
                        UwsHLen = reader.ReadInt16();
                        UwsHLen = Helper.Reverse(UwsHLen);

                        //UwsHVersion
                        reader.BaseStream.Seek(30, SeekOrigin.Begin);
                        UwsHVersion = reader.ReadInt16();
                        UwsHVersion = Helper.Reverse(UwsHVersion);

                        //UwsXLen
                        reader.BaseStream.Seek(32, SeekOrigin.Begin);
                        UwsXLen = reader.ReadInt16();
                        UwsXLen = Helper.Reverse(UwsXLen);

                        //UwsXRecords
                        reader.BaseStream.Seek(34, SeekOrigin.Begin);
                        UwsXRecords = reader.ReadInt16();
                        UwsXRecords = Helper.Reverse(UwsXRecords);

                        //UwsSignatureTypeByte
                        reader.BaseStream.Seek(36, SeekOrigin.Begin);
                        UwsSignatureTypeByte = reader.ReadBytes(UwsSignatureTypeByte.Length);
                        UwsSignatureType = myEncoding.GetString(UwsSignatureTypeByte).Trim();

                        //UwsVersion
                        reader.BaseStream.Seek(54, SeekOrigin.Begin);
                        UwsVersion = reader.ReadInt32();
                        UwsVersion = Helper.Reverse(UwsVersion);

                        //UwsVstringByte
                        reader.BaseStream.Seek(58, SeekOrigin.Begin);
                        UwsVstringByte = reader.ReadBytes(UwsVstringByte.Length);
                        UwsVstring = myEncoding.GetString(UwsVstringByte).Trim();

                        //UwsSystemNameByte
                        reader.BaseStream.Seek(88, SeekOrigin.Begin);
                        UwsSystemNameByte = reader.ReadBytes(UwsSystemNameByte.Length);
                        UwsSystemName = myEncoding.GetString(UwsSystemNameByte).Trim();

                        //UwsCdataClassId
                        reader.BaseStream.Seek(98, SeekOrigin.Begin);
                        UwsCdataClassId = reader.ReadInt32();
                        UwsCdataClassId = Helper.Reverse(UwsCdataClassId);

                        //UwsCollectorVersion
                        reader.BaseStream.Seek(102, SeekOrigin.Begin);
                        UwsCollectorVersion = reader.ReadInt32();
                        UwsCollectorVersion = Helper.Reverse(UwsCollectorVersion);

                        //UwsCollectorVstringByte
                        reader.BaseStream.Seek(106, SeekOrigin.Begin);
                        UwsCollectorVstringByte = reader.ReadBytes(UwsCollectorVstringByte.Length);
                        UwsCollectorVstring = myEncoding.GetString(UwsCollectorVstringByte).Trim();

                        #endregion
                    }
                    else if (uwsVersion == UWS.Types.Version2009) {
                        log.Info("Version 2009");
                        
                        #region Basic Header Info

                        //Key.
                        reader.BaseStream.Seek(0, SeekOrigin.Begin);
                        UwsKeyByte = reader.ReadBytes(UwsKeyByte.Length);
                        UwsKey = myEncoding.GetString(UwsKeyByte).Trim();

                        //System Serial
                        reader.BaseStream.Seek(10, SeekOrigin.Begin);
                        SystemSerialByte = reader.ReadBytes(SystemSerialByte.Length);
                        UWSSerialNumber = myEncoding.GetString(SystemSerialByte).Trim();
                        while (UWSSerialNumber.Length < 6) {
                            UWSSerialNumber = "0" + UWSSerialNumber;
                        }

                        //UwsHLen
                        reader.BaseStream.Seek(20, SeekOrigin.Begin);
                        UwsHLen = reader.ReadInt16();
                        UwsHLen = Helper.Reverse(UwsHLen);

                        //UwsXLen
                        reader.BaseStream.Seek(22, SeekOrigin.Begin);
                        UwsXLen = reader.ReadInt16();
                        UwsXLen = Helper.Reverse(UwsXLen);

                        //UwsXRecords
                        reader.BaseStream.Seek(24, SeekOrigin.Begin);
                        UwsXRecords = reader.ReadInt16();
                        UwsXRecords = Helper.Reverse(UwsXRecords);

                        //UwsSystemNameByte
                        reader.BaseStream.Seek(44, SeekOrigin.Begin);
                        UwsSystemNameByte = reader.ReadBytes(UwsSystemNameByte.Length);
                        UwsSystemName = myEncoding.GetString(UwsSystemNameByte).Trim();

                        //UwsVstringByte
                        reader.BaseStream.Seek(54, SeekOrigin.Begin);
                        UwsVstringByte = reader.ReadBytes(UwsVstringByte.Length);
                        UwsVstring = myEncoding.GetString(UwsVstringByte).Trim();

                        //Identifier
                        //reader.BaseStream.Seek(0, SeekOrigin.Begin);
                        //UwsIdentifierByte = reader.ReadBytes(UwsIdentifierByte.Length);
                        //UwsIdentifier = myEncoding.GetString(UwsIdentifierByte).Trim();

                        //UwsHVersion
                        //reader.BaseStream.Seek(30, SeekOrigin.Begin);
                        //UwsHVersion = reader.ReadInt16();
                        //UwsHVersion = Reverse(UwsHVersion);

                        //UwsSignatureTypeByte
                        //reader.BaseStream.Seek(36, SeekOrigin.Begin);
                        //UwsSignatureTypeByte = reader.ReadBytes(UwsSignatureTypeByte.Length);
                        //UwsSignatureType = myEncoding.GetString(UwsSignatureTypeByte).Trim();

                        //UwsVersion
                        //reader.BaseStream.Seek(54, SeekOrigin.Begin);
                        //UwsVersion = reader.ReadInt32();
                        //UwsVersion = Reverse(UwsVersion);

                        //UwsVstringByte
                        //reader.BaseStream.Seek(58, SeekOrigin.Begin);
                        //UwsVstringByte = reader.ReadBytes(UwsVstringByte.Length);
                        //UwsVstring = myEncoding.GetString(UwsVstringByte).Trim();

                        //UwsCdataClassId
                        //reader.BaseStream.Seek(98, SeekOrigin.Begin);
                        //UwsCdataClassId = reader.ReadInt32();
                        //UwsCdataClassId = Reverse(UwsCdataClassId);

                        //UwsCollectorVersion
                        //reader.BaseStream.Seek(102, SeekOrigin.Begin);
                        //UwsCollectorVersion = reader.ReadInt32();
                        //UwsCollectorVersion = Reverse(UwsCollectorVersion);

                        //UwsCollectorVstringByte
                        //reader.BaseStream.Seek(106, SeekOrigin.Begin);
                        //UwsCollectorVstringByte = reader.ReadBytes(UwsCollectorVstringByte.Length);
                        //UwsCollectorVstring = myEncoding.GetString(UwsCollectorVstringByte).Trim();

                        #endregion
                    }
                    else if (uwsVersion == UWS.Types.Version2013) {
                        log.Info("Version 2013");
                        
                        //Read the VProc version.
                        reader.BaseStream.Seek(84, SeekOrigin.Begin);
                        NewCreatorVproc = reader.ReadBytes(NewCreatorVproc.Length);
                        UwsCreatorVproc = Helper.RemoveNULL(myEncoding.GetString(NewCreatorVproc).Trim());

                        //Get version Info.
                        var vProc = new VProcVersionService(_connectionStr);
                        string className = vProc.GetVProcVersionFor(UwsCreatorVproc);
                        log.InfoFormat("ClassName: {0}", className);
                        
                        if (className.Equals("HeaderInfoV1"))
                            headerInfo = new HeaderInfoV1();

                        log.Info("Read header");
                        
                        headerInfo.ReadHeader(uwsPath, log, this);
                    }

                    #region Create Index
                    log.Info("Create index");
                    
                    //Create Index.
                    //byte[] indexBytes = new byte[60];
                    var indexBytes = new byte[62];
                    var tempShortBytes = new byte[2];
                    var tempIntBytes = new byte[4];
                    var tempLongBytes = new byte[8];

                    int indexPosition = UwsHLen;
                    long tempLen = (Convert.ToInt64(UwsXRecords) * Convert.ToInt64(UwsXLen));
                    long dataPosition = indexPosition + tempLen;
                    //int UwsFileRecords = 0;

                    //List<Indices> index = new List<Indices>();
                    for (int x = 0; x < UwsXRecords; x++) {
                        var indexer = new Indices();

                        if (uwsVersion == UWS.Types.Version2007 || uwsVersion == UWS.Types.Version2009) {
                            #region Index
                            reader.BaseStream.Seek(indexPosition, SeekOrigin.Begin);
                            indexBytes = reader.ReadBytes(indexBytes.Length);

                            //Get Index Name (first 8 bytes).
                            indexer.FName = myEncoding.GetString(indexBytes, 0, 7);

                            //Index Type.
                            tempShortBytes[0] = indexBytes[9];
                            tempShortBytes[1] = indexBytes[8];
                            indexer.FType = Convert.ToInt16(BitConverter.ToInt16(tempShortBytes, 0));

                            //Index Length.
                            tempShortBytes[0] = indexBytes[11];
                            tempShortBytes[1] = indexBytes[10];
                            indexer.FReclen = Convert.ToInt16(BitConverter.ToInt16(tempShortBytes, 0));

                            //Index Dump Occurs.
                            tempIntBytes[0] = indexBytes[15];
                            tempIntBytes[1] = indexBytes[14];
                            tempIntBytes[2] = indexBytes[13];
                            tempIntBytes[3] = indexBytes[12];
                            indexer.FRecords = Convert.ToInt32(BitConverter.ToInt32(tempIntBytes, 0));

                            //Index System Name (10 bytes).
                            indexer.FSysName = myEncoding.GetString(indexBytes, 16, 9).Trim();

                            //Index Meas Version.
                            indexer.FMeasVer = myEncoding.GetString(indexBytes, 26, 3).Trim();

                            //Index Start Time.
                            tempLongBytes[0] = indexBytes[37];
                            tempLongBytes[1] = indexBytes[36];
                            tempLongBytes[2] = indexBytes[35];
                            tempLongBytes[3] = indexBytes[34];
                            tempLongBytes[4] = indexBytes[33];
                            tempLongBytes[5] = indexBytes[32];
                            tempLongBytes[6] = indexBytes[31];
                            tempLongBytes[7] = indexBytes[30];
                            //Need to do / 10000 to get current julian time
                            indexer.FStartTime = Convert.ToInt64(BitConverter.ToInt64(tempLongBytes, 0)) / 10000;

                            //Index Stop Time
                            tempLongBytes[0] = indexBytes[45];
                            tempLongBytes[1] = indexBytes[44];
                            tempLongBytes[2] = indexBytes[43];
                            tempLongBytes[3] = indexBytes[42];
                            tempLongBytes[4] = indexBytes[41];
                            tempLongBytes[5] = indexBytes[40];
                            tempLongBytes[6] = indexBytes[39];
                            tempLongBytes[7] = indexBytes[38];
                            //Need to do / 10000 to get current julian time
                            double temp = BitConverter.ToInt64(tempLongBytes, 0);
                            indexer.FStopTime = Convert.ToInt64(temp) / 10000;

                            //Index Interval.
                            tempLongBytes[0] = indexBytes[53];
                            tempLongBytes[1] = indexBytes[52];
                            tempLongBytes[2] = indexBytes[51];
                            tempLongBytes[3] = indexBytes[50];
                            tempLongBytes[4] = indexBytes[49];
                            tempLongBytes[5] = indexBytes[48];
                            tempLongBytes[6] = indexBytes[47];
                            tempLongBytes[7] = indexBytes[46];
                            indexer.FInterval = Convert.ToInt64(BitConverter.ToInt64(tempLongBytes, 0));

                            //Index Start Day.
                            tempIntBytes[0] = indexBytes[57];
                            tempIntBytes[1] = indexBytes[56];
                            tempIntBytes[2] = indexBytes[55];
                            tempIntBytes[3] = indexBytes[54];
                            indexer.FStartDay = Convert.ToInt32(BitConverter.ToInt32(tempIntBytes, 0));

                            //Index Stop Day.
                            tempIntBytes[0] = indexBytes[61];
                            tempIntBytes[1] = indexBytes[60];
                            tempIntBytes[2] = indexBytes[59];
                            tempIntBytes[3] = indexBytes[58];
                            indexer.FStopDay = Convert.ToInt32(BitConverter.ToInt32(tempIntBytes, 0));

                            //Index File Postion.
                            indexer.FilePosition = dataPosition;
                            #endregion
                        }
                        else {
                            indexer = headerInfo.ReaderEntityHeader(uwsPath, log, indexPosition, dataPosition);

                            //Other info that needs to sync the data.
                            indexer.FSysName = UwsSystemName;
                            indexer.FMeasVer = UwsMeasureDllVersion;
                        }

                        //Insert into the List.
                        index.Add(indexer);

                        indexPosition += UwsXLen;
                        tempLen = (Convert.ToInt64(indexer.FRecords) * Convert.ToInt64(indexer.FReclen));
                        dataPosition = dataPosition + tempLen;
                        //UwsFileRecords += indexer.FRecords;
                    }

                    #endregion
                }
            }

            return true;
        }

        /// <summary>
        /// Take the paramters, check if the system exists. If not then create system databases.
        /// Check if the load is a DISCOPEN load or a regular load, then call different to load the data.
        /// </summary>
        /// <param name="uwsPath"> Full path of the UWS data file.</param>
        /// <param name="log"> Stream writer of the log file.</param>
        /// <param name="uwsVersion"> Enum class that tell version of UWS file </param>
        /// <param name="tempDatabaseConnString">Store the temp connection string for system DB</param>
        /// <param name="masterDatabaseConnString">Master DB to connect when creating a new DB</param>
        /// <returns></returns>
        public bool CreateNewData(string uwsPath, ILog log, UWS.Types uwsVersion, string tempDatabaseConnString, 
                                string masterDatabaseConnString, bool glacierLoad, string uwsFile, bool duplicate, bool isLocalAnalyst, string databasePostfix,
                                string databasePrefix, DateTime? startTime = null, DateTime? stopTime = null) {
            log.Info("Start OpenNewUWSFile");
            log.InfoFormat("uwsPath: {0}", uwsPath);
            
            OpenNewUWSFile(uwsPath, uwsVersion, log);
            log.Info("OpenNewUWSFile completed...");
            
            bool success;

            DateTime beforeTime = DateTime.Now;
            //Check if we need to change the System Serial Number.
            var systemSerialConversionService = new SystemSerialConversionService(_connectionStr);
            var newSystemSerial = systemSerialConversionService.GetConvertionSystemSerialFor(UWSSerialNumber);
            var isSystemSerialChange = false;
            if (newSystemSerial.Length > 0) {
                UWSSerialNumber = newSystemSerial;
                isSystemSerialChange = true;
            }

            if (UWSSerialNumber.Length > 0) {
                //Get ConnectionString
                var databaseMapService = new DatabaseMappingService(_connectionStr);
                string newConnectionString = "";
                if (!_mainDBLoad)
                    newConnectionString = databaseMapService.GetConnectionStringDynamicReportGenerator(UWSSerialNumber, tempDatabaseConnString, isLocalAnalyst, databasePostfix);
                else {
                    newConnectionString = databaseMapService.GetConnectionStringFor(UWSSerialNumber);
                }
                log.InfoFormat("Database newConnectionString: {0}", LoadUWS.RemovePassword(newConnectionString));
                
                //Check database exists.
                bool databases = databaseMapService.CheckDatabaseFor(newConnectionString);

                if (!databases) {
                    log.Info("Create database");
                    
                    //Get Database Name.
                    string databaseName = Util.Helper.FindKeyName(newConnectionString, Util.Helper._DATABASEKEYNAME);

                    log.InfoFormat("databaseName: {0}", databaseName);
                    
                    //Create DataBase.
                    string masterConnectionString = masterDatabaseConnString;
                    var db = new DatabaseService(masterConnectionString);
                    newConnectionString = db.CreateDatabaseFor(databaseName, log);
                    if (newConnectionString.Length == 0)
                        return false;

                    log.Info("Create Tables");
                    
                    db.CreateTablesFor(databaseName, newConnectionString);//This is for creating other tables
                }

                //After creating the database, insert the UWSLoadingStatus entry
                if (!duplicate) {
#if !DEBUG
                    var loadingStatus = new UWSLoadingStatusService(newConnectionString);
                    loadingStatus.InsertUWSLoadingStatusFor(UWSSerialNumber, uwsFile);
#endif
                    log.InfoFormat("Insert the UWSLoadingStatus entry after creating database: {0}", uwsFile);
                }

                log.Info("CreateMultiDayDataSet");
                success = CreateMultiDayDataSet(uwsPath, log, newConnectionString, uwsVersion, glacierLoad, databasePrefix, startTime, stopTime);//This is for creating entity tables.
                log.InfoFormat("Loading RADC: {0}", success);
                

                //Update the Systeam Name.
                if (isSystemSerialChange) {
                    log.Info("Update the Systeam Name");
                    

                    try {
                        var systemTable = new System_tblService(_connectionStr);
                        var newSystemName = systemTable.GetSystemNameFor(UWSSerialNumber);

                        string databaseName = Util.Helper.FindKeyName(newConnectionString, Util.Helper._DATABASEKEYNAME);
                        //Update SystemName on Detail Database.
                        var db = new DatabaseService(newConnectionString);
                        var detailTableList = db.GetDetailTableList(UWSSerialNumber, databaseName);

                        foreach (var detailTable in detailTableList) {
                            log.InfoFormat("Updating: {0}", detailTable);
                            

                            db.UpdateSystemName(detailTable, newSystemName);
                        }
                    }
                    catch (Exception ex) {
                        log.ErrorFormat("Error: {0}", ex);
                        
                    }
                }

                DateTime afterTime = DateTime.Now;
                TimeSpan span = afterTime - beforeTime;
                log.InfoFormat("Total RA Load time in minutes: {0}", span.TotalMinutes);
                
            }
            else {
                log.Info("**********System Serial is Empty!!!!");
                
                success = false;
            }
            return success;
        }

        /// <summary>
        /// Create the table structure for the entity tables.
        /// </summary>
        /// <param name="days"> List of MultiDays. </param>
        /// <param name="columnInfo"> List of ColumnInfoView which contains the column info of the table that going to be created.</param>
        /// <param name="websiteLoad"> Is the load for Website or not.</param>
        /// <returns> Returns a DataSet that contains the tables structures.</returns>
        internal DataSet CreateSPAMDataTableColumn(List<MultiDays> days, IList<ColumnInfoView> columnInfo, bool websiteLoad) {
            var myDataSet = new DataSet();
            //This DataTableName has be to start Date(only date part), because I have to compare with data's FromTimestamp.

            foreach (MultiDays d in days) {
                if (!d.DontLoad) {
                    string buildDataTableName = d.StartDate.ToString("yyyy/MMM/dd");
                    var myDataTable = new DataTable(buildDataTableName);

                    // Create new DataColumn, set DataType, ColumnName and add to DataTable.    
                    var myDataColumn = new DataColumn { DataType = Type.GetType("System.Int32"), ColumnName = "EntityCounterID" };
                    // Add the Column to the DataColumnCollection.
                    myDataTable.Columns.Add(myDataColumn);

                    foreach (var column in columnInfo) {
                        if (websiteLoad && column.Website.Equals(false)) continue;

                        myDataColumn = new DataColumn {
                            DataType = Type.GetType(GetSystemValueType(column.TypeName)),
                            ColumnName = column.ColumnName
                        };
                        // Add the Column to the DataColumnCollection.
                        myDataTable.Columns.Add(myDataColumn);
                    }

                    myDataSet.Tables.Add(myDataTable);
                }
            }

            return myDataSet;
        }

        /// <summary>
        /// Get the system data type according to the SQL Server data type.
        /// </summary>
        /// <param name="type"> SQL Server data type. </param>
        /// <returns> Return a string value which is system data type that used in creating datatable.</returns>
        internal string GetSystemValueType(string type) {
            string returnType;
            switch (type.ToUpper()) {
                case "DATETIME":
                    returnType = "System.DateTime";
                    break;
                case "FLOAT":
                    returnType = "System.Double";
                    break;
                case "SMALLINT":
                    returnType = "System.Int16";
                    break;
                case "INT":
                    returnType = "System.UInt16";
                    break;
                case "BIGINT":
                    returnType = "System.UInt32";
                    break;
                case "BIT":
                    returnType = "System.Byte";
                    break;
                case "TINYINT":
                    returnType = "System.Byte";
                    break;
                case "VARCHAR":
                    returnType = "System.String";
                    break;
                default:
                    returnType = "System.String";
                    break;
            }
            return returnType;
        }

        /// <summary>
        /// Load the data as a regular load.
        /// When meet DISCOPEN entity, create entry in UWSDirectories table, zip the UWS file and upload it to S3.
        /// Call TrendDataLoad to load data into trend tables.
        /// </summary>
        /// <param name="uwsPath"> Full path of the UWS data file.</param>
        /// <param name="log"> Stream writer of the log file.</param>
        /// <param name="uwsID"> UWS ID of this load. </param>
        /// <param name="newConnectionString">Connection string of the system database.</param>
        /// <param name="uwsVersion">Enum class that tell version of UWS file.</param>
        /// <returns> Return a bool value suggests whether the load is successful or not.</returns>
        public bool CreateMultiDayDataSet(string uwsPath, ILog log, string newConnectionString, UWS.Types uwsVersion, bool glacierLoad,
                                        string databasePrefix, DateTime? reportStartTime = null, DateTime? reportStopTime = null) {
            log.Info("    -Populateing SPAM Database");
            log.InfoFormat("newConnectionString: {0}", RemovePassword(newConnectionString));
            

            bool success = false;
            bool entityAccountedFor = false;
            var tempShort = new byte[2];
            var tempInt = new byte[4];
            var tempLong = new byte[8];
            //string strIndexValues = string.Empty;
            string systemName = string.Empty;
            int entityID = 0;
            long startTime = 0L;
            long stopTime = 0L;
            long sampleInterval = 0L;

            //var discopenHours = new List<string>();

            var uniqueTableName = new List<string>();
            var tableNames = new List<string>();
            var currentTables = new List<MultiDays>();
            var tableTimestamps = new List<MultiDays>();
            var entityList = new Dictionary<string, string>();

            var dataStartDate = new DateTime();
            var dataStopDate = new DateTime();

            var uwsStartDate = new DateTime();
            var uwsStopDate = new DateTime();
            var currentTable = new CurrentTableService(newConnectionString);

            string fileEntity = "";
            bool isProcessDirectlySystem = false;

            var dicInfo = new DirectoryInfo(uwsPath);
            var localLoad = false;
            if(newConnectionString.Contains("localhost")) {
                localLoad = true;
            }
            try {
                #region Load Data

                log.Info("Start loading data...");
                
                var websiteLoad = false;
                using (var stream = new FileStream(uwsPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    //using (StreamReader reader = new StreamReader(stream))
                    using (reader = new BinaryReader(stream)) {
                        //Loop thorugh the entity.
                        foreach (Indices t in index) {
                            if (t.FName.Length != 0 && t.FRecords > 0) {
                                //Get eneityID.
                                var entity = new EntitiesService(newConnectionString);
                                entityID = entity.GetEntityIDFor(t.FName.Trim());

                                log.InfoFormat("entityID: {0}", entityID);
                                
                                if (entityID == 0) {
                                    continue;
                                }
                                if((entityID == 8 || entityID == 9) && !localLoad) {
                                    log.InfoFormat("      ***** Skipping SQLPROC/SQLSTMT for localLoad: {0}", localLoad);
                                    continue;
                                }

                                if ((reportStartTime != null && reportStopTime != null) && glacierLoad) {
                                    if (!(t.CollEntityStartTime >= reportStartTime && t.CollEntityStartTime <= reportStopTime)) {
                                        log.Info("****CollEntityStartTime NOT within reportTime");
                                        log.InfoFormat("****CollEntityStartTime: {0}", t.CollEntityStartTime);
                                        log.InfoFormat("****reportStartTime: {0}", reportStartTime);
                                        log.InfoFormat("****reportStopTime: {0}", reportStopTime);                                        
                                        continue;
                                    }
                                    else {
                                        if (entityID == 3) {
                                            log.Info("      ***** Skipping DISCOPEN for Main DB Load.");
                                            continue;
                                        }
                                        //websiteLoad = true; 
                                    }
                                }

                                //Check for duplicated process when we have userdef.
                                if (index.Any(x => x.FName.Trim().Equals("USERDEF"))) {
                                    //Check number of index.
                                    if (index.Count.Equals(2) && entityID == (int)Entity.PROCESS) {
                                        log.InfoFormat("****Skiping Entity Name:{0}", t.FName.Trim());
                                        //Stop the process. 
                                        continue;
                                    }
                                    else {
                                        //Check if it has two process.
                                        var processCount = index.Count(x => x.FName.Trim().Equals("PROCESS"));

                                        if (processCount.Equals(2) && entityList.ContainsKey("PROCESS") && entityID == (int)Entity.PROCESS) {
                                            log.InfoFormat("****Skiping Entity Name:{0}", t.FName.Trim());
                                            continue;
                                        }
                                    }
                                }

                                //Check for duplicated process when we have userdef.
                                if (index.Any(x => x.FName.Trim().Equals("USERDEF"))) {
                                    //Check number of index.
                                    if (index.Count.Equals(2) && entityID == (int)Entity.PROCESS) {
                                        log.InfoFormat("****Skiping Entity Name:{0}", t.FName.Trim());
                                        //Stop the process. 
                                        continue;
                                    }
                                    else {
                                        //Check if it has two process.
                                        var processCount = index.Count(x => x.FName.Trim().Equals("PROCESS"));

                                        if (processCount.Equals(2) && entityID == (int)Entity.PROCESS) {
                                            var maxProcessCount = index.Where(x => x.FName.Trim().Equals("PROCESS")).Max(x => x.FRecords);

                                            if (t.FRecords != maxProcessCount) {
                                                log.InfoFormat("****Skiping Entity Name: {0}", t.FName.Trim());
                                                log.InfoFormat("****maxProcessCount: {0}", maxProcessCount);
                                                log.InfoFormat("****t.FRecords: {0}", t.FRecords);
                                                
                                                continue;
                                            }
                                        }
                                    }
                                }

                                log.InfoFormat("Entity Name:{0}, Started", t.FName.Trim());
                                
                                DateTime beforeTime = DateTime.Now;

                                if (systemName.Length == 0) {
                                    systemName = t.FSysName;
                                }

                                //Interval.
                                if (sampleInterval == 0) {
                                    sampleInterval = t.FInterval;
                                }

                                string dbTableName;

                                if (uwsVersion == UWS.Types.Version2007 || uwsVersion == UWS.Types.Version2009) {
                                    //Start Time.
                                    if (startTime == 0) {
                                        startTime = t.FStartTime;
                                    }
                                    else if (startTime > t.FStartTime) {
                                        startTime = t.FStartTime;
                                    }
                                    //Stop Time.
                                    if (stopTime == 0) {
                                        stopTime = t.FStopTime;
                                    }
                                    else if (stopTime < t.FStopTime) {
                                        stopTime = t.FStopTime;
                                    }

                                    //Get table name according to data format.
                                    var mVersion = new MeasureVersionsService(newConnectionString);
                                    dbTableName = mVersion.GetMeasureDBTableNameFor(t.FMeasVer);
                                    log.InfoFormat("      Measure Type: {0}", dbTableName);
                                    

                                    //Create Table name.
                                    var convertTime = new ConvertJulianTime();
                                    int timeStamp = convertTime.JulianTimeStampToOBDTimeStamp(t.FStartTime);
                                    dataStartDate = convertTime.OBDTimeStampToDBDate(timeStamp);
                                    timeStamp = convertTime.JulianTimeStampToOBDTimeStamp(t.FStopTime);
                                    dataStopDate = convertTime.OBDTimeStampToDBDate(timeStamp);
                                }
                                else {
                                    //Get table name according to data format.
                                    var mVersion = new MeasureVersionsService(newConnectionString);
                                    dbTableName = mVersion.GetMeasureDBTableNameFor(t.FMeasVer);
                                    log.InfoFormat("      Measure Type: {0}", dbTableName);
                                    

                                    //change the table name to new table. This is only for new uws header version.
                                    if (dbTableName.Equals("ZmsBladeDataDictionary") || dbTableName.Equals("ZmsDataDictionary")) {
                                        var vProc = new VProcVersionService(_connectionStr);
                                        dbTableName = vProc.GetDataDictionaryFor(UwsCreatorVproc);
                                    }

                                    log.InfoFormat("      New Measure Type: {0}", dbTableName);
                                    
                                    dataStartDate = t.CollEntityStartTime;
                                    dataStopDate = t.CollEntityStoptTime;

                                    if (uwsStartDate.Equals(DateTime.MinValue)) {
                                        uwsStartDate = t.CollEntityStartTime;
                                    }
                                    else if (uwsStartDate > t.CollEntityStartTime) {
                                        uwsStartDate = t.CollEntityStartTime;
                                    }
                                    //Stop Time.
                                    if (uwsStopDate.Equals(DateTime.MinValue)) {
                                        uwsStopDate = t.CollEntityStoptTime;
                                    }
                                    else if (uwsStopDate < t.CollEntityStoptTime) {
                                        uwsStopDate = t.CollEntityStoptTime;
                                    }
                                }

                                log.InfoFormat("      SystemSerial: {0}", UWSSerialNumber);
                                

                                //Get Column type into the List.
                                var dictionary = new DataDictionaryService(newConnectionString);
                                IList<ColumnInfoView> columnInfo = dictionary.GetColumnsFor(entityID, dbTableName);

                                log.InfoFormat("columnInfo.Count: {0}", columnInfo.Count);
                                
                                //string tableName = string.Empty;

                                int recordLenth = t.FReclen;
                                long filePosition = t.FilePosition;

                                //Round up the seconds.
                                //DateTime oldDataStopDate = dataStopDate;
                                TimeSpan span = dataStopDate - dataStartDate;
                                double seconds = span.TotalSeconds;
                                //Get remained seconds.
                                double remainSeconds = seconds % t.FInterval;
                                if (remainSeconds < t.FInterval * 0.1) {
                                    dataStopDate = dataStopDate.AddSeconds(-remainSeconds);
                                }
                                //Lei: IR 7323. Above if block covers the scenario when the time is within 90 seconds scope,
                                //the time will be rounded down, for example 06:15:59 will be cut down to 06:15:00,
                                //below if block covers the round-up scenario, for example 06:59:59 will be rounded up to 07:00:00
                                if (remainSeconds < t.FInterval && remainSeconds > t.FInterval * 0.9) {
                                    dataStopDate = dataStopDate.AddSeconds(t.FInterval - remainSeconds);
                                }
                                log.InfoFormat("      dataStartDate: {0}", dataStartDate);
                                log.InfoFormat("      dataStopDate: {0}", dataStopDate);
                                

                                #region CheckMulti Day

                                var days = new List<MultiDays>();

                                //Check to see if there are multi days. if yes, move data to currect table.
                                string buildTableName;
                                MultiDays multiDays;
                                if (dataStartDate.Day != dataStopDate.Day) {
                                    //Check to see how many days.
                                    for (DateTime x = dataStartDate; x < dataStopDate; x = x.AddDays(1)) {
                                        multiDays = new MultiDays {
                                            StartDate = x,
                                            EndDate = new DateTime(x.Year, x.Month, x.Day, 23, 59, 59),
                                            EntityID = entityID,
                                            Interval = t.FInterval,
                                            SystemSerial = UWSSerialNumber,
                                            MeasureVersion = t.FMeasVer
                                        };
                                        buildTableName = UWSSerialNumber + "_" + t.FName.Trim().ToUpper() + "_" + x.Year + "_" + x.Month + "_" + x.Day;
                                        multiDays.TableName = buildTableName;
                                        multiDays.DontLoad = false;
                                        days.Add(multiDays);

                                        //SAVE GENERIC TABLE NAME FOR UniqueID PROCESSING
                                        if (!entityAccountedFor) {
                                            buildTableName = UWSSerialNumber + "_%_" + x.Year + "_" + x.Month + "_" + x.Day;
                                            uniqueTableName.Add(buildTableName);
                                        }
                                    }
                                    //Add stop date. I'm putting this here so that I can get ending time.
                                    multiDays = new MultiDays {
                                        StartDate = new DateTime(dataStopDate.Year, dataStopDate.Month, dataStopDate.Day),
                                        EndDate = dataStopDate,
                                        EntityID = entityID,
                                        Interval = t.FInterval,
                                        SystemSerial = UWSSerialNumber,
                                        MeasureVersion = t.FMeasVer
                                    };
                                    buildTableName = UWSSerialNumber + "_" + t.FName.Trim().ToUpper() + "_" + dataStopDate.Year + "_" + dataStopDate.Month + "_" + dataStopDate.Day;
                                    multiDays.TableName = buildTableName;
                                    multiDays.DontLoad = false;
                                    days.Add(multiDays);

                                    //SAVE GENERIC TABLE NAME FOR UniqueID PROCESSING
                                    if (!entityAccountedFor) {
                                        buildTableName = UWSSerialNumber + "_%_" + dataStopDate.Year + "_" + dataStopDate.Month + "_" + dataStopDate.Day;
                                        uniqueTableName.Add(buildTableName);
                                        entityAccountedFor = true;
                                    }
                                }
                                else {
                                    //Add single day.
                                    multiDays = new MultiDays {
                                        StartDate = dataStartDate,
                                        EndDate = dataStopDate,
                                        EntityID = entityID,
                                        Interval = t.FInterval,
                                        SystemSerial = UWSSerialNumber,
                                        MeasureVersion = t.FMeasVer
                                    };
                                    //multiDays.StartDate = new DateTime(dataStopDate.Year, dataStopDate.Month, dataStopDate.Day);
                                    buildTableName = UWSSerialNumber + "_" + t.FName.Trim().ToUpper() + "_" + dataStartDate.Year + "_" + dataStartDate.Month + "_" + dataStartDate.Day;
                                    multiDays.TableName = buildTableName;
                                    multiDays.DontLoad = false;
                                    days.Add(multiDays);

                                    //SAVE GENERIC TABLE NAME FOR UniqueID PROCESSING
                                    if (!entityAccountedFor) {
                                        buildTableName = UWSSerialNumber + "_%_" + dataStartDate.Year + "_" + dataStartDate.Month + "_" + dataStartDate.Day;
                                        uniqueTableName.Add(buildTableName);
                                        entityAccountedFor = true;
                                    }
                                }

                                #endregion

                                //Spcial case for VISA, load the overlap data.
                                var systemTblService = new System_tblService(_connectionStr);
                                isProcessDirectlySystem = systemTblService.isProcessDirectlySystemFor(UWSSerialNumber);

                                log.InfoFormat("isProcessDirectlySystem: {0}", isProcessDirectlySystem);
                                

                                #region Create Table

                                var databaseCheck = new Database(newConnectionString);
                                var databaseName = RemoteAnalyst.BusinessLogic.Util.Helper.FindKeyName(newConnectionString, "DATABASE");

                                //Check to see if table name exists.
                                foreach (MultiDays d in days) {
                                    bool checkTableExists = databaseCheck.CheckTableExists(d.TableName, databaseName);

                                    if (d.StartDate != d.EndDate) {
                                        #region Create Table

                                        if (!checkTableExists) {
                                            //Create MySQL Table. Function checks for table name.
                                            var mySQLServices = new MySQLServices();
                                            mySQLServices.CreateEntityTable(entityID, d.SystemSerial, d.TableName, columnInfo, websiteLoad, 
                                                _connectionStr, log, false, isProcessDirectlySystem, databasePrefix, newConnectionString);

                                            //Create Table.
                                            //entity.CreateEntityTable(d.TableName, columnInfo, true, websiteLoad);

                                            //Insert basic info into the Current Table.
                                            //currentTable.InsertCurrentTable(d.TableName, d.EntityID, d.Interval, d.StartDate, d.SystemSerial, d.MeasureVersion);
                                            var tempCurrentTable = new TempCurrentTablesService(newConnectionString);
                                            tempCurrentTable.InsertCurrentTableFor(d.TableName, d.EntityID, d.Interval, d.StartDate, d.SystemSerial, d.MeasureVersion);

                                            //Insert Time Stamps.
                                            var tableTimeStamp = new TempTableTimestampService(newConnectionString);
                                            tableTimeStamp.InsertTempTimeStampFor(d.TableName, d.StartDate, d.EndDate, Path.GetFileName(uwsPath));

                                            currentTables.Add(d);
                                            tableTimestamps.Add(d);
                                        }
                                        else {
                                            //Check if Interval matches.
                                            bool intervalMatch = false;
                                            long currentInterval = currentTable.GetIntervalFor(d.TableName);
                                            if (currentInterval == d.Interval) {
                                                intervalMatch = true;
                                            }
                                            else {
                                                //Check with Temp current table interval.
                                                var tempCurrentTable = new TempCurrentTablesService(newConnectionString);
                                                currentInterval = tempCurrentTable.GetIntervalFor(d.TableName);
                                                if (currentInterval == d.Interval) {
                                                    intervalMatch = true;
                                                }
                                            }

                                            if (intervalMatch) {
                                                log.Info("Check if Time Stamp don't over laps.");
                                                

                                                //Spcial case for VISA, load the overlap data.
                                                var tempTableTimeStamp = new TempTableTimestampService(newConnectionString);

                                                if (!isProcessDirectlySystem) {
                                                    //Check if Time Stamp don't over laps.
                                                    var tableTimeStamp = new TableTimeStampService(newConnectionString);
                                                    bool timeOverLap = tableTimeStamp.CheckTimeOverLapFor(d.TableName, d.StartDate, d.EndDate);
                                                    if (!timeOverLap) {
                                                        //Check if Time Stamp don't over laps from TempCurrentTable.
                                                        bool tempTimeOverLap = tableTimeStamp.CheckTimeOverLapFor(d.TableName, d.StartDate, d.EndDate);
                                                        if (!tempTimeOverLap) {
                                                            tempTimeOverLap = tableTimeStamp.CheckTempTimeOverLapFor(d.TableName, d.StartDate, d.EndDate);
                                                            if (!tempTimeOverLap) {

                                                                if (glacierLoad && !d.TableName.Contains("_CPU_")) {
                                                                    tempTableTimeStamp.InsertTempTimeStampFor(d.TableName, d.StartDate, d.EndDate, Path.GetFileName(uwsPath));
                                                                    tableTimestamps.Add(d);
                                                                }
                                                                else {
                                                                    log.InfoFormat("DontLoad to true for: {0}", d.TableName);
                                                                    
                                                                    d.DontLoad = true;
                                                                }
                                                            }
                                                            else {
                                                                //Insert Time Stamps. 
                                                                tempTableTimeStamp.InsertTempTimeStampFor(d.TableName, d.StartDate, d.EndDate, Path.GetFileName(uwsPath));
                                                                tableTimestamps.Add(d);
                                                            }
                                                        }
                                                        else {
                                                            //Insert Time Stamps. 
                                                            tempTableTimeStamp.InsertTempTimeStampFor(d.TableName, d.StartDate, d.EndDate, Path.GetFileName(uwsPath));
                                                            tableTimestamps.Add(d);
                                                        }
                                                    }
                                                    else {
                                                        //Insert Time Stamps. 
                                                        tempTableTimeStamp.InsertTempTimeStampFor(d.TableName, d.StartDate, d.EndDate, Path.GetFileName(uwsPath));
                                                        tableTimestamps.Add(d);
                                                    }
                                                }
                                                else {
                                                    //Insert Time Stamps. 
                                                    tempTableTimeStamp.InsertTempTimeStampFor(d.TableName, d.StartDate, d.EndDate, Path.GetFileName(uwsPath));
                                                    tableTimestamps.Add(d);
                                                }
                                            }
                                            else {
                                                //Stop this process.
                                                //continue;
                                                //load = false;
                                                //break;
                                                d.DontLoad = true;
                                            }
                                        }

                                        #endregion
                                    }
                                    else {
                                        d.DontLoad = true;
                                    }
                                    //Get Entity List.
                                    if (entityID == (int)Entity.CPU) {
                                        if (!entityList.ContainsKey("CPU")) {
                                            entityList.Add("CPU", "[" + d.TableName + "]");
                                        }
                                    }
                                    else if (entityID == (int)Entity.DISC) {
                                        if (!entityList.ContainsKey("DISK")) {
                                            entityList.Add("DISK", "[" + d.TableName + "]");
                                        }
                                    }
                                    /*else if (entityID == 4) { DISKFILE is not need to load Trend Data.
                                        if (!entityList.ContainsKey("DISKFILE")) {
                                            entityList.Add("DISKFILE", "[" + d.TableName + "]");
                                        }
                                    }*/
                                    else if (entityID == (int)Entity.PROCESS) {
                                        if (!entityList.ContainsKey("PROCESS")) {
                                            entityList.Add("PROCESS", "[" + d.TableName + "]");
                                        }
                                    }
                                    else if (entityID == (int)Entity.TMF) {
                                        if (!entityList.ContainsKey("TMF")) {
                                            entityList.Add("TMF", "[" + d.TableName + "]");
                                        }
                                    }
                                    else if (entityID == (int)Entity.FILE) {
                                        if (fileEntity.Length.Equals(0))
                                            fileEntity = d.TableName;
                                    }
                                    /*else if (entityID == 3) { DISCOPE is not need to load Trend Data.
                                        if (!entityList.ContainsKey("DISCOPE")) {
                                            entityList.Add("DISCOPE", "[" + d.TableName + "]");
                                        }
                                    }*/
                                }

                                #endregion

                                var myDataSet = new DataSet();

                                try {
                                    #region Create DataSet and insert data

                                    //Create DataSet with DataTable(s).
                                    //myDataSet = new DataSet();
                                    myDataSet = CreateSPAMDataTableColumn(days, columnInfo, websiteLoad);

                                    if (myDataSet.Tables.Count > 0) {
                                        //Test values for tableName From FromTimeStamp.
                                        var tableNameFromTimeStamp = new DateTime();

                                        //Loop through the records.
                                        for (int x = 0; x < t.FRecords; x++) {
                                            reader.BaseStream.Seek(filePosition, SeekOrigin.Begin);
                                            byte[] indexBytes = reader.ReadBytes(recordLenth);
                                            long currentPosition = 0;
                                            //Dictionary<string, string> insertDictionary = new Dictionary<string, string>();

                                            //this will create newline for each loop.
                                            //myDataRow = myDataTable.NewRow();

                                            //test.
                                            double fromJulian = 0;
                                            double toJulian = 0;

                                            foreach (ColumnInfoView column in columnInfo) {
                                                if (column.ColumnName == "FromJulian" && (entityID == 3 || entityID == 5)) {
                                                    //myDataRow[column.ColumnName] = fromJulian;
                                                    column.TestValue = fromJulian.ToString();
                                                    continue;
                                                }
                                                if (column.ColumnName == "ToJulian" && (entityID == 3 || entityID == 5)) {
                                                    //myDataRow[column.ColumnName] = toJulian;
                                                    column.TestValue = toJulian.ToString();
                                                    continue;
                                                }

                                                // This condition is to check if the number of fields in the UWS file is less than in TabelTable, we have to stop getting data and feed in just 'Null'
                                                if (currentPosition < t.FReclen) {
                                                    #region Switch

                                                    switch (column.TypeName) {
                                                        case "SMALLINT":
                                                            try {
                                                                tempShort[0] = indexBytes[currentPosition + 1];
                                                                tempShort[1] = indexBytes[currentPosition];
                                                                //myDataRow[column.ColumnName] = Convert.ToInt16(BitConverter.ToInt16(tempShort, 0));
                                                                column.TestValue = BitConverter.ToInt16(tempShort, 0).ToString();
                                                            }
                                                            catch (Exception ex) {
                                                                column.TestValue = "0";
                                                                log.ErrorFormat("EntityID: {0}" +
                                                                                      "\n column TypeName: {1}" +
                                                                                      "\n current position: {2}" +
                                                                                      "\n index FReclen: {3}" +
                                                                                      "\n Message: {4}",
                                                                                      entityID, column.TypeName,
                                                                                      currentPosition, t.FReclen,
                                                                                      ex.Message);

                                                                throw new Exception(ex.Message);
                                                            }
                                                            break;
                                                        case "INT": //NOTE: VARIABLE COMING IN AS "SMALLINT" AND SAVED AS "INT"
                                                            try {
                                                                tempShort[0] = indexBytes[currentPosition + 1];
                                                                tempShort[1] = indexBytes[currentPosition];
                                                                //myDataRow[column.ColumnName] = Convert.ToInt16(BitConverter.ToInt16(tempShort, 0));
                                                                column.TestValue = BitConverter.ToUInt16(tempShort, 0).ToString();

                                                                if ((column.ColumnName.ToUpper().Equals("IPUS") ||
                                                                    column.ColumnName.ToUpper().Equals("IPUNUM")) && column.TestValue.Equals("0")) {
                                                                    //If the Ipus number is 0, we need to change this to 1. 
                                                                    column.TestValue = "1";
                                                                }
                                                            }
                                                            catch (Exception ex) {
                                                                column.TestValue = "0";
                                                                log.ErrorFormat("EntityID: {0}" +
                                                                                     "\n column TypeName: {1}" +
                                                                                     "\n current position: {2}" +
                                                                                     "\n index FReclen: {3}" +
                                                                                     "\n Message: {4}",
                                                                                     entityID, column.TypeName,
                                                                                     currentPosition, t.FReclen,
                                                                                     ex.Message);

                                                                throw new Exception(ex.Message);
                                                            }
                                                            break;
                                                        case "BIGINT": //NOTE: VARIABLE COMING IN AS "INT" AND SAVED AS "BIGINT"
                                                            try {
                                                                tempInt[0] = indexBytes[currentPosition + 3];
                                                                tempInt[1] = indexBytes[currentPosition + 2];
                                                                tempInt[2] = indexBytes[currentPosition + 1];
                                                                tempInt[3] = indexBytes[currentPosition + 0];
                                                                //myDataRow[column.ColumnName] = Convert.ToInt32(BitConverter.ToInt32(tempInt, 0));
                                                                column.TestValue = BitConverter.ToUInt32(tempInt, 0).ToString();

                                                                /*if ((column.ColumnName.ToUpper().Equals("IPUS") ||
                                                                    column.ColumnName.ToUpper().Equals("IPUNUM")) && column.TestValue.Equals("0")) {
                                                                    //If the Ipus number is 0, we need to change this to 1. 
                                                                    column.TestValue = "1";
                                                                }*/
                                                            }
                                                            catch (Exception ex) {
                                                                column.TestValue = "0";
                                                                log.ErrorFormat("EntityID: {0}" +
                                                                                     "\n column TypeName: {1}" +
                                                                                     "\n current position: {2}" +
                                                                                     "\n index FReclen: {3}" +
                                                                                     "\n Message: {4}",
                                                                                     entityID, column.TypeName,
                                                                                     currentPosition, t.FReclen,
                                                                                     ex.Message);

                                                                throw new Exception(ex.Message);
                                                            }
                                                            break;
                                                        case "FLOAT":
                                                            try {
                                                                tempLong[0] = indexBytes[currentPosition + 7];
                                                                tempLong[1] = indexBytes[currentPosition + 6];
                                                                tempLong[2] = indexBytes[currentPosition + 5];
                                                                tempLong[3] = indexBytes[currentPosition + 4];
                                                                tempLong[4] = indexBytes[currentPosition + 3];
                                                                tempLong[5] = indexBytes[currentPosition + 2];
                                                                tempLong[6] = indexBytes[currentPosition + 1];
                                                                tempLong[7] = indexBytes[currentPosition + 0];
                                                                //myDataRow[column.ColumnName] = Convert.ToInt64(BitConverter.ToInt64(tempLong, 0));
                                                                column.TestValue = BitConverter.ToInt64(tempLong, 0).ToString();
                                                            }
                                                            catch (Exception ex) {
                                                                column.TestValue = "0";
                                                                log.ErrorFormat("EntityID: {0}" +
                                                                                     "\n column TypeName: {1}" +
                                                                                     "\n current position: {2}" +
                                                                                     "\n index FReclen: {3}" +
                                                                                     "\n Message: {4}",
                                                                                     entityID, column.TypeName,
                                                                                     currentPosition, t.FReclen,
                                                                                     ex.Message);

                                                                throw new Exception(ex.Message);
                                                            }
                                                            break;
                                                        case "BIT":
                                                            try {
                                                                byte tempByte = indexBytes[currentPosition];
                                                                //Convert to Binary.
                                                                string binaryValue = Convert.ToString(tempByte, 2);
                                                                if (binaryValue.Length < 8) {
                                                                    binaryValue = binaryValue.PadLeft(8, '0');
                                                                }

                                                                if (column.ColumnName.ToUpper() == "MEAS_CLIM_REL") {
                                                                    column.TestValue = binaryValue[0].ToString();
                                                                    continue;
                                                                }
                                                                if (column.ColumnName.ToUpper() == "MEAS_PATH_SEL") {
                                                                    column.TestValue = binaryValue[1].ToString();
                                                                    continue;
                                                                }
                                                                if (column.ColumnName.ToUpper() == "MEAS_CLIM_DEVICE") {
                                                                    column.TestValue = binaryValue[2].ToString();
                                                                    continue;
                                                                }
                                                                if (column.ColumnName.ToUpper() == "FILLER1") {
                                                                    column.TestValue = binaryValue[3].ToString();
                                                                    continue;
                                                                }
                                                                if (column.ColumnName.ToUpper() == "FILLER2") {
                                                                    column.TestValue = binaryValue[4].ToString();
                                                                    continue;
                                                                }
                                                                if (column.ColumnName.ToUpper() == "FILLER3") {
                                                                    column.TestValue = binaryValue[5].ToString();
                                                                    continue;
                                                                }
                                                                if (column.ColumnName.ToUpper() == "FILLER4") {
                                                                    column.TestValue = binaryValue[6].ToString();
                                                                    continue;
                                                                }
                                                                if (column.ColumnName.ToUpper() == "FILLER5") {
                                                                    column.TestValue = binaryValue[7].ToString();
                                                                }
                                                            }
                                                            catch (Exception ex) {
                                                                column.TestValue = "0";
                                                                log.ErrorFormat("EntityID: {0}" +
                                                                                     "\n column TypeName: {1}" +
                                                                                     "\n current position: {2}" +
                                                                                     "\n index FReclen: {3}" +
                                                                                     "\n Message: {4}",
                                                                                     entityID, column.TypeName,
                                                                                     currentPosition, t.FReclen,
                                                                                     ex.Message);

                                                                throw new Exception(ex.Message);
                                                            }
                                                            break;
                                                        case "TINYINT":
                                                            try {
                                                                //myDataRow[column.ColumnName] = indexBytes[currentPosition];
                                                                column.TestValue = indexBytes[currentPosition].ToString();
                                                            }
                                                            catch (Exception ex) {
                                                                column.TestValue = "0";
                                                                log.ErrorFormat("EntityID: {0}" +
                                                                                     "\n column TypeName: {1}" +
                                                                                     "\n current position: {2}" +
                                                                                     "\n index FReclen: {3}" +
                                                                                     "\n Message: {4}",
                                                                                     entityID, column.TypeName,
                                                                                     currentPosition, t.FReclen,
                                                                                     ex.Message);

                                                                throw new Exception(ex.Message);
                                                            }
                                                            break;
                                                        case "NVARCHAR":
                                                            try {
                                                                string tempString = "";
                                                                for (int z = 0; z < column.TypeValue; z++) {
                                                                    if ((indexBytes[currentPosition] > 125) || (indexBytes[currentPosition] < 32) || (indexBytes[currentPosition] == 39)) {
                                                                        tempString += "";
                                                                    }
                                                                    else {
                                                                        tempString += Convert.ToChar(indexBytes[currentPosition + z]);
                                                                    }
                                                                }
                                                                //myDataRow[column.ColumnName] = tempString.Trim();
                                                                column.TestValue = tempString.Trim();
                                                            }
                                                            catch (Exception ex) {
                                                                column.TestValue = "";
                                                                log.ErrorFormat("EntityID: {0}" +
                                                                                     "\n column TypeName: {1}" +
                                                                                     "\n current position: {2}" +
                                                                                     "\n index FReclen: {3}" +
                                                                                     "\n Message: {4}",
                                                                                     entityID, column.TypeName,
                                                                                     currentPosition, t.FReclen,
                                                                                     ex.Message);
                                                                throw new Exception(ex.Message);
                                                            }
                                                            break;
                                                        case "DATETIME":
                                                            try {
                                                                tempLong[0] = indexBytes[currentPosition + 7];
                                                                tempLong[1] = indexBytes[currentPosition + 6];
                                                                tempLong[2] = indexBytes[currentPosition + 5];
                                                                tempLong[3] = indexBytes[currentPosition + 4];
                                                                tempLong[4] = indexBytes[currentPosition + 3];
                                                                tempLong[5] = indexBytes[currentPosition + 2];
                                                                tempLong[6] = indexBytes[currentPosition + 1];
                                                                tempLong[7] = indexBytes[currentPosition + 0];
                                                                long tempDate = Convert.ToInt64(BitConverter.ToInt64(tempLong, 0));
                                                                //Need to do / 10000 to get current julian time
                                                                tempDate /= 10000;

                                                                if (column.ColumnName == "FromTimestamp" && (entityID == 3 || entityID == 5)) {
                                                                    fromJulian = tempDate;
                                                                }
                                                                else if (column.ColumnName == "ToTimestamp" && (entityID == 3 || entityID == 5)) {
                                                                    toJulian = tempDate;
                                                                }

                                                                if (tempDate == 0) {
                                                                    //myDataRow[column.ColumnName] = DBNull.Value;
                                                                    column.TestValue = "";
                                                                }
                                                                else {
                                                                    var convert = new ConvertJulianTime();
                                                                    int obdTimeStamp = convert.JulianTimeStampToOBDTimeStamp(tempDate);
                                                                    DateTime dbDate = convert.OBDTimeStampToDBDate(obdTimeStamp);

                                                                    if (column.ColumnName == "FromTimestamp") {
                                                                        tableNameFromTimeStamp = dbDate;
                                                                    }

                                                                    //check if the entity is CPU and counter is FromTimestamp.
                                                                    if (entityID == 1 && (column.ColumnName.ToUpper().Trim() == "FROMTIMESTAMP" || column.ColumnName.ToUpper().Trim() == "TOTIMESTAMP")) {
                                                                        //Create a list of datetime according to interval.
                                                                        var intervalList = new Dictionary<DateTime, DateTime>();
                                                                        DateTime tempStartDate = dataStartDate;

                                                                        double tenPercent;
                                                                        if (column.ColumnName.ToUpper().Trim() == "FROMTIMESTAMP") {
                                                                            tenPercent = -0.1;
                                                                        }
                                                                        else {
                                                                            tenPercent = 0.1;
                                                                        }
                                                                        intervalList.Add(tempStartDate.AddSeconds(t.FInterval * tenPercent),
                                                                            tempStartDate.AddSeconds(t.FInterval).AddSeconds(t.FInterval * tenPercent));
                                                                        while (tempStartDate < dataStopDate) {
                                                                            tempStartDate = tempStartDate.AddSeconds(t.FInterval);
                                                                            intervalList.Add(tempStartDate.AddSeconds(t.FInterval * tenPercent),
                                                                                tempStartDate.AddSeconds(t.FInterval).AddSeconds(t.FInterval * tenPercent));
                                                                        }

                                                                        //Get foramtted From and To Timestamp.
                                                                        foreach (var kv in intervalList) {
                                                                            if (column.ColumnName.ToUpper().Trim() == "FROMTIMESTAMP") {
                                                                                if (dbDate >= kv.Key && dbDate <= kv.Value) {
                                                                                    dbDate = kv.Key.AddSeconds(t.FInterval * 0.1);
                                                                                    break;
                                                                                }
                                                                            }
                                                                            else {
                                                                                if (dbDate >= kv.Key && dbDate <= kv.Value) {
                                                                                    dbDate = kv.Value.AddSeconds(t.FInterval * -0.1);
                                                                                    break;
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                    //myDataRow[column.ColumnName] = dbDate;
                                                                    //for multiday use only. If toTimestamp is next day. substract one sec.
                                                                    if (column.ColumnName.ToUpper().Trim() == "TOTIMESTAMP" && dbDate.Hour == 0 && dbDate.Minute == 0 && dbDate.Second == 0) {
                                                                        dbDate = dbDate.AddSeconds(-1);
                                                                    }
                                                                    column.TestValue = dbDate.ToString();
                                                                }
                                                            }
                                                            catch (Exception ex) {
                                                                column.TestValue = DateTime.MinValue.ToString(); log.ErrorFormat("EntityID: {0}" +
                                                                                     "\n column TypeName: {1}" +
                                                                                     "\n current position: {2}" +
                                                                                     "\n index FReclen: {3}" +
                                                                                     "\n Message: {4}",
                                                                                     entityID, column.TypeName,
                                                                                     currentPosition, t.FReclen,
                                                                                     ex.Message);
                                                                throw new Exception(ex.Message);
                                                            }
                                                            break;
                                                    }

                                                    #endregion

                                                    currentPosition += column.TypeValue;
                                                }
                                                else {
                                                    //myDataRow[column.ColumnName] = DBNull.Value;
                                                    column.TestValue = "";
                                                }
                                            }

                                            //Create newline for each loop.
                                            if (myDataSet.Tables.Contains(tableNameFromTimeStamp.ToString("yyyy/MMM/dd"))) {
                                                DataRow myDataRow = myDataSet.Tables[tableNameFromTimeStamp.ToString("yyyy/MMM/dd")].NewRow();
                                                bool deleteRow = false;
                                                var tempFromtimestamp = DateTime.MinValue;

                                                foreach (var column in columnInfo) {
                                                    #region Switch
                                                    if (websiteLoad && column.Website.Equals(false)) continue;

                                                    switch (column.TypeName) {
                                                        case "SMALLINT":
                                                            myDataRow[column.ColumnName] = Convert.ToInt16(column.TestValue);
                                                            break;
                                                        case "INT": //NOTE: VARIABLE COMING IN AS "SMALLINT" AND SAVED AS "INT"
                                                            if (column.ColumnName.ToUpper().Trim() == "UNIQUEID") {
                                                                //hard code the UniqueID to 1. this column is for our use.
                                                                myDataRow[column.ColumnName] = 1;
                                                            }
                                                            else {
                                                                myDataRow[column.ColumnName] = Convert.ToUInt16(column.TestValue);
                                                            }
                                                            break;
                                                        case "BIGINT": //NOTE: VARIABLE COMING IN AS "INT" AND SAVED AS "BIGINT"
                                                            if (column.ColumnName.ToUpper().Trim() == "UNIQUEID") {
                                                                //hard code the UniqueID to 1. this column is for our use.
                                                                myDataRow[column.ColumnName] = 1;
                                                            }
                                                            else {
                                                                myDataRow[column.ColumnName] = Convert.ToUInt32(column.TestValue);
                                                            }
                                                            break;
                                                        case "FLOAT":
                                                            myDataRow[column.ColumnName] = Convert.ToInt64(column.TestValue);
                                                            break;
                                                        case "BIT":
                                                            myDataRow[column.ColumnName] = Convert.ToByte(column.TestValue);
                                                            break;
                                                        case "TINYINT":
                                                            myDataRow[column.ColumnName] = Convert.ToByte(column.TestValue);
                                                            break;
                                                        case "NVARCHAR":
                                                            myDataRow[column.ColumnName] = column.TestValue;
                                                            break;
                                                        case "DATETIME":
                                                            if (column.ColumnName.ToUpper().Trim() == "FROMTIMESTAMP") {
                                                                if (Convert.ToDateTime(column.TestValue) >= dataStopDate) {
                                                                    deleteRow = true;
                                                                }
                                                                else if (reportStartTime != null && reportStopTime != null) {
                                                                    //If the value doesn't fall into the range, don't load.
                                                                    if (!(Convert.ToDateTime(column.TestValue).Ticks >= reportStartTime.Value.Ticks && Convert.ToDateTime(column.TestValue).Ticks <= reportStopTime.Value.Ticks)) {
                                                                        deleteRow = true;
                                                                    }
                                                                }
                                                                tempFromtimestamp = Convert.ToDateTime(column.TestValue);
                                                            }
                                                            else if (column.ColumnName.ToUpper().Trim() == "TOTIMESTAMP") {
                                                                if (column.TestValue.Length == 0) {
                                                                    //Use the FromTimestamp
                                                                    column.TestValue = tempFromtimestamp.ToString();
                                                                }
                                                                else {
                                                                    if (Convert.ToDateTime(column.TestValue) > dataStopDate) {
                                                                        column.TestValue = dataStopDate.ToString();
                                                                    }
                                                                }
                                                            }

                                                            myDataRow[column.ColumnName] = Convert.ToDateTime(column.TestValue);
                                                            break;
                                                    }

                                                    #endregion
                                                }

                                                //Per John's request, we are removing this if statment.
                                                /*if (myDataRow["FromTimestamp"].ToString().Equals(myDataRow["ToTimestamp"].ToString())) {
                                                    deleteRow = true;
                                                }*/

                                                if (deleteRow) {
                                                    //skip this row.
                                                }
                                                else {
                                                    //Add new row into the dataSet.
                                                    myDataSet.Tables[tableNameFromTimeStamp.ToString("yyyy/MMM/dd")].Rows.Add(myDataRow);
                                                    string tempTableName = UWSSerialNumber + "_" + t.FName.Trim().ToUpper() + "_" + tableNameFromTimeStamp.Year + "_" + tableNameFromTimeStamp.Month + "_" + tableNameFromTimeStamp.Day;

                                                    //Insert TableName for UniqueID.
                                                    if (!tableNames.Contains(tempTableName)) {
                                                        tableNames.Add(tempTableName);
                                                    }

                                                    var counter = 0;
                                                    foreach (DataTable table in myDataSet.Tables) {
                                                        counter = table.Rows.Count;
                                                    }

                                                    if (counter > Constants.getInstance(_connectionStr).BulkLoaderSize) {
                                                        foreach (MultiDays d in days) {
                                                            if (!d.DontLoad) {
                                                                try {
                                                                    if (myDataSet.Tables[d.StartDate.ToString("yyyy/MMM/dd")].Rows.Count > 0) {
                                                                        var mySQLServices = new MySQLServices();
                                                                        mySQLServices.InsertEntityDatas(d.SystemSerial, d.TableName, myDataSet.Tables[d.StartDate.ToString("yyyy/MMM/dd")], _connectionStr,
                                                                            dicInfo.Parent.FullName + "\\", entityID, false,
                                                                            log, newConnectionString);
                                                                    }
                                                                }
                                                                catch (Exception ex) {
                                                                    log.ErrorFormat("    -Insert into MySQL Error: {0}", ex);
                                                                    
                                                                }
                                                            }
                                                        }
                                                        myDataSet = null;
                                                        GC.Collect();
                                                        myDataSet = CreateSPAMDataTableColumn(days, columnInfo, websiteLoad);
                                                    }
                                                }
                                            }

                                            //Increase the start reading position.
                                            filePosition += recordLenth;
                                        }

                                        //Insert into the database.
                                        foreach (MultiDays d in days) {
                                            if (!d.DontLoad) {
                                                try {
                                                    if (myDataSet.Tables[d.StartDate.ToString("yyyy/MMM/dd")].Rows.Count > 0) {
                                                        var mySQLServices = new MySQLServices();
                                                        mySQLServices.InsertEntityDatas(d.SystemSerial, d.TableName, myDataSet.Tables[d.StartDate.ToString("yyyy/MMM/dd")], _connectionStr,
                                                            dicInfo.Parent.Parent.Parent.FullName + "\\", entityID, false, log, newConnectionString);
                                                    }
                                                }
                                                catch (Exception ex) {
                                                    log.ErrorFormat("    -Insert into MySQL Error: {0}", ex);
                                                    
                                                }
                                            }
                                        }

                                        /*//Insert into the database.
                                        var tables = new DataTableService(newConnectionString);
                                        foreach (MultiDays d in days) {
                                            if (!d.DontLoad) {
                                                tables.InsertSPAMEntityDataFor(d.TableName, myDataSet.Tables[d.StartDate.ToString("yyyy/MMM/dd")], dicInfo.FullName);
                                            }
                                        }*/
                                        DateTime afterTime = DateTime.Now;
                                        TimeSpan timeSpan = afterTime - beforeTime;
                                        log.InfoFormat("    -(Line 1717) EntityID: {0}, Total Time in Minutes: {1}", entityID, timeSpan.TotalMinutes);
                                        
                                    }

                                    #endregion
                                }
                                catch (Exception ex) {
                                    log.ErrorFormat("EntityID: {0}.\n Message:", entityID, ex.Message);
                                    
                                    //throw new Exception(ex.Message);
                                }
                                finally {
                                    columnInfo = null;
                                    myDataSet.Dispose();
                                }
                            }
                        }
                    }
                }

                #endregion

                #region Update CurrentTable & TableTimestamp.
                if (currentTables.Count > 0) {
                    var tempCurrentTable = new TempCurrentTablesService(newConnectionString);
                    foreach (var d in currentTables) {
                        try {
                            //Insert basic info into the Current Table.
                            currentTable.InsertEntryFor(d.TableName, d.EntityID, d.Interval, Convert.ToDateTime(d.StartDate.ToShortDateString()), d.SystemSerial, d.MeasureVersion);
                        }
                        catch (Exception ex) {
                            log.ErrorFormat("Insert CurrentTable: {0}", ex);
                            
                        }
                        try {
                            //Delete from TempCurrentTable.
                            tempCurrentTable.DeleteCurrentTableFor(d.TableName);
                        }
                        catch (Exception ex) {
                            log.ErrorFormat("Delete TempCurrentTable: {0}", ex);
                            
                        }
                    }
                }

                log.ErrorFormat("tableTimestamps.Count: {0}", tableTimestamps.Count);
                
                if (tableTimestamps.Count > 0) {
                    var tableTimeStamp = new TableTimeStampService(newConnectionString);
                    var tempTimeStamp = new TempTableTimestampService(newConnectionString);

                    foreach (var d in tableTimestamps) {
                        try {
                            //Following does not apply anymore. Every load should be Active.
                            /*//If user places Glacier order, the tableTimestamp will always be archived, because when the data is loaded in EC2
                            //data will be destroyed after the report is done, the website will never have the active data.
                            if (glacierLoad) {
                                tableTimeStamp.InsertEntryFor(d.TableName, d.StartDate, d.EndDate, (int)ArchiveStatus.Status.Archived);
                            }
                            else {
                                tableTimeStamp.InsertEntryFor(d.TableName, d.StartDate, d.EndDate, (int)ArchiveStatus.Status.Active);
                            }*/
                            var duplicate = tableTimeStamp.CheckDuplicateFor(d.TableName, d.StartDate, d.EndDate);
                            if(!duplicate)
                                tableTimeStamp.InsertEntryFor(d.TableName, d.StartDate, d.EndDate, (int)ArchiveStatus.Status.Active, Path.GetFileName(uwsPath));
                            else
                                tableTimeStamp.UpdateStatusUsingTableNameFor(d.TableName, d.StartDate, d.EndDate, (int)ArchiveStatus.Status.Active);
                        }
                        catch (Exception ex) {
                            log.ErrorFormat("Insert TableTimeStamp: {0}", ex);
                            
                        }
                        try {
                            //Delete from TempCurrentTable.
                            tempTimeStamp.DeleteTempTimeStampFor(d.TableName);
                        }
                        catch (Exception ex) {
                            log.ErrorFormat("Delete TempTimeStamp: {0}", ex);
                            
                        }
                    }

                    if (glacierLoad) {
                        try {
                            var buildCpuTableName = tableTimestamps.FirstOrDefault().TableName.Split('_');
                            var cpuTableName = buildCpuTableName[0] + "_CPU_" + buildCpuTableName[2] + "_" + buildCpuTableName[3] + "_" + buildCpuTableName[4];

                            log.Info("Update Glacier CPU Table to Active");
                            
                            //if glacier, update cpu tableTimestamp to active.
                            tableTimeStamp.InsertEntryFor(cpuTableName, tableTimestamps.FirstOrDefault().StartDate, tableTimestamps.FirstOrDefault().EndDate, (int)ArchiveStatus.Status.Active, Path.GetFileName(uwsPath));
                        }
                        catch (Exception ex) {
                            log.ErrorFormat("Update Glacier CPU Table to Active Error: {0}", ex);
                            
                        }
                    }
                }
                
                #endregion

                success = true;
            }
            catch (Exception ex) {
                log.ErrorFormat("EntityID: {0}, {1}.", entityID, ex.Message);
                
                success = false;

                //Update CurrentTable & TableTimestamp.
                if (currentTables.Count > 0) {
                    var tempCurrentTable = new TempCurrentTablesService(newConnectionString);
                    var tableTimestamp = new TableTimeStampService(newConnectionString);
                    var tempTableTimestamp = new TempTableTimestampService(newConnectionString);
                    //DataTables dataTable = new DataTables();
                    foreach (MultiDays d in currentTables) {
                        //Insert basic info into the Current Table.
                        currentTable.DeleteEntryFor(d.TableName);
                        //Delete from TempCurrentTable.
                        tempCurrentTable.DeleteCurrentTableFor(d.TableName);
                        //Delete from TableTimestamp.
                        tableTimestamp.DeleteEntryFor(d.TableName);
                        //Delete from TempTableTimestamp.
                        tempTableTimestamp.DeleteTempTimeStampFor(d.TableName);
                    }
                }
            }
            finally {
                GC.Collect();
            }

            return success;
        }

    }
}
