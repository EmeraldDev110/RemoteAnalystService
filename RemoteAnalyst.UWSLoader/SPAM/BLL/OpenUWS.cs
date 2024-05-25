using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using RemoteAnalyst.AWS.S3;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.BusinessLogic.Enums;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using System.Configuration;
using RemoteAnalyst.BusinessLogic.UWSLoader.BaseClass;
using RemoteAnalyst.BusinessLogic.UWSLoader;
using RemoteAnalyst.UWSLoader.Email;
using RemoteAnalyst.UWSLoader.BLL;
using RemoteAnalyst.BusinessLogic.SCM;
using CurrentTableService = RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices.CurrentTableService;
using File = System.IO.File;
using RemoteAnalyst.UWSLoader.DiskBrowser;
using IntervalTrendLoader;
using RemoteAnalyst.BusinessLogic.BLL;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.Util;
using Helper = RemoteAnalyst.BusinessLogic.UWSLoader.Helper;
using RemoteAnalystTrendLoader.Model;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using RemoteAnalyst.Repository.Infrastructure;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;
using log4net;
using RemoteAnalyst.Repository.MySQLConcrete.RemoteAnalystSPAM;
using DataBrowser.Context;
using RemoteAnalystTrendLoader.context;

namespace RemoteAnalyst.UWSLoader.SPAM.BLL
{

    /// <summary>
    /// OpenUWS class reads the UWS file and load data into database.
    /// It loads data into entity tables and call TrendDataLoad to load data into trend tables.
    /// </summary>
    internal class OpenUWS : Header {
        private readonly string _connectionStr = ConnectionString.ConnectionStringDB;
        private readonly string _connectionStrSPAM = ConnectionString.ConnectionStringSPAM;
        private readonly List<WaitHandle> loadJobThread = new List<WaitHandle>();

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

            using (var stream = new FileStream(uwsPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                //using (StreamReader reader = new StreamReader(stream))
                using (reader = new BinaryReader(stream)) {
                    var myEncoding = new ASCIIEncoding();
                    if (uwsVersion == UWS.Types.Version2007) {
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
                        //Read the VProc version.
                        reader.BaseStream.Seek(84, SeekOrigin.Begin);
                        NewCreatorVproc = reader.ReadBytes(NewCreatorVproc.Length);
                        UwsCreatorVproc = Helper.RemoveNULL(myEncoding.GetString(NewCreatorVproc).Trim());

                        //Get version Info.
                        var vProc = new VProcVersionService(ConnectionString.ConnectionStringDB);
                        string className = vProc.GetVProcVersionFor(UwsCreatorVproc);
                        if (className.Equals("HeaderInfoV1"))
                            headerInfo = new HeaderInfoV1();

                        headerInfo.ReadHeader(uwsPath, log, this);
                    }

                    #region Create Index

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
        /// <param name="uwsID"> UWS ID of this load.</param>
        /// <param name="log"> log4net instance.</param>
        /// <param name="uwsVersion"> Enum class that tell version of UWS file </param>
        /// <param name="compayID"> ID of the company.</param>
        /// <param name="selectedStartTime"> The start time that user selected.</param>
        /// <param name="selectedStopTime"> The stop time that user selected.</param>
        /// <param name="databasePrefix"> TdatabasePrefix</param>
        /// <returns></returns>
        internal bool CreateNewData(string uwsPath, int uwsID, ILog log, UWS.Types uwsVersion, int compayID, DateTime selectedStartTime, DateTime selectedStopTime, string databasePrefix, int ntsId = 0) {
            bool loadDISCOPEN = false;
            if (selectedStartTime != DateTime.MinValue && selectedStopTime != DateTime.MinValue) {
                loadDISCOPEN = true;
            }

            log.Info("CreateNewData()");

            OpenNewUWSFile(uwsPath, uwsVersion, log);
            log.Info("After OpenNewUWSFile()");

            bool success = false;
            
            //Check System Serial for VISA. If SystemSerial is 075843, change it to 072826
            if (UWSSerialNumber.Equals("075843")) {
                UWSSerialNumber = "072826";
                log.Info("Changing SystemSerial to 072826");
            }
            //Check if we need to change the System Serial Number.
            var systemSerialConversionService = new SystemSerialConversionService(ConnectionString.ConnectionStringDB);
            var newSystemSerial = systemSerialConversionService.GetConvertionSystemSerialFor(UWSSerialNumber);
            if (newSystemSerial.Length > 0)
                UWSSerialNumber = newSystemSerial;

            #region Update LoadingInfo.

            log.Info("******  Update LoadingInfo  ********");
            try {
                var systemName = UwsSystemName;
                var loadingInfo = new LoadingInfoService(_connectionStr);
                if (systemName.Length > 8) systemName = systemName.Substring(0, 8);

                loadingInfo.UpdateCollectionTimeFor(uwsID, systemName, Convert.ToDateTime(UwsCollInfoStartTimestamp), Convert.ToDateTime(UwsCollInfoEndTimestamp), 4);
            }
            catch (Exception ex) {
                log.ErrorFormat("LoadingInfo Error: {0}", ex.Message);
            }
            log.Info("******  Update LoadingInfo completed  ********");
            
            #endregion

            log.Info("========================= OpenUWS check interval =======================");
            //Check the interval. If the interval is less than 30 seconds, reject the load and update the status.
            if (index.All(x => x.FInterval < 60)) {
                try {
                    log.Info("Update Load Fail Upload Information");
                    
                    //Update the Upload.
                    var uploadService = new UploadService(ConnectionString.ConnectionStringDB);
                    uploadService.UpdateUploadFailInformation(UWSSerialNumber, UwsUwsFileLocation.Trim(),
                        Convert.ToDateTime(UwsCollInfoStartTimestamp), Convert.ToDateTime(UwsCollInfoEndTimestamp),
                        ConnectionString.AdvisorEmail, ConnectionString.SupportEmail,
                        ConnectionString.WebSite,
                        ConnectionString.EmailServer, ConnectionString.EmailPort, ConnectionString.EmailUser,
                        ConnectionString.EmailPassword, ConnectionString.EmailAuthentication,
                        ConnectionString.SystemLocation, ConnectionString.ServerPath,
                        ConnectionString.EmailIsSSL,
                        ConnectionString.IsLocalAnalyst, ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                }
                catch (Exception ex) {
                    log.ErrorFormat("Update Upload Error: {0}", ex.Message);
                }

                log.Info("**********Interval less than 60 seconds!!!!");
                
                success = false;
            }
            else {
                log.Info("========================= OpenUWS check interval >60 =======================");
                DateTime beforeTime = DateTime.Now;

                if (UWSSerialNumber.Length > 0) {
                    log.InfoFormat("========================= OpenUWS check interval >60 UWSSerialNumber : {0}", UWSSerialNumber);
                    //Get ConnectionString
                    var databaseMapService = new DatabaseMappingService(_connectionStr);
                    string newConnectionString = databaseMapService.GetConnectionStringFor(UWSSerialNumber);
                    if (newConnectionString.Length == 0) {

                        //Create New entry on DatabaseMapping.
                        //newConnectionString = Config.ConnectionString.Replace("RemoteAnalystdbSPAM", databasePrefix + UWSSerialNumber);
                        //databaseMapService.InsertNewEntryFor(UWSSerialNumber, newConnectionString);
                        var email = new EmailToSupport(ConnectionString.AdvisorEmail, ConnectionString.SupportEmail,
                            ConnectionString.WebSite,
                            ConnectionString.EmailServer, ConnectionString.EmailPort, ConnectionString.EmailUser,
                            ConnectionString.EmailPassword, ConnectionString.EmailAuthentication,
                            ConnectionString.SystemLocation, ConnectionString.ServerPath,
                            ConnectionString.EmailIsSSL,
                            ConnectionString.IsLocalAnalyst, 
                            ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                        email.SendLocalAnalystErrorMessageEmail("UWS Loader", "Unknown System: " + UWSSerialNumber, LicenseService.GetProductName(ConnectionString.ConnectionStringDB));
                        return false;
                    }

                    //Check database exists.
                    bool databases = true;

                    if (!newConnectionString.Contains("RemoteAnalystdbSPAM")) {
                        databases = databaseMapService.CheckDatabaseFor(newConnectionString);
                    }

                    /*DataTables dataTables = new DataTables(Config.ConnectionString);
                    bool databases = dataTables.CheckDatabase(databaseName);
                    string newConnectionString = "";*/

                    log.InfoFormat("Database exist: {0}",databases);
                    

                    if (!databases) {
                        log.Info("Create Database");
                        
                        //Get Database Name.
                        string databaseName = BusinessLogic.Util.Helper.FindKeyName(newConnectionString, BusinessLogic.Util.Helper._DATABASEKEYNAME);

                        //Create DataBase.
                        var db = new DatabaseService(_connectionStrSPAM);
                        newConnectionString = db.CreateDatabaseFor(databaseName, log);

                        log.Info("Create Tables");

                        db.CreateTablesFor(databaseName, newConnectionString);
                    }

                    //Ryan Ji
                    //Load Discopen data only when loadDISCOPEN is true
                    if (loadDISCOPEN) {
                        success = CreateMultiDayDataSetDISCOPEN(uwsPath, log, newConnectionString, selectedStartTime, selectedStopTime, uwsVersion);
                    }
                    else {
                        success = CreateMultiDayDataSet(uwsPath, log, uwsID, newConnectionString, uwsVersion, ntsId);
                    }
                    log.InfoFormat("Loading RADC: {0}",success);
                    

                    DateTime afterTime = DateTime.Now;
                    TimeSpan span = afterTime - beforeTime;
                    log.InfoFormat("Total RA Load time in minutes: {0}",span.TotalMinutes);
                    

                    if (ConnectionString.IsLocalAnalyst && success) {
                        //Delete the UWS File.
                        if (File.Exists(uwsPath))
                            File.Delete(uwsPath);
                    }

                    try {
                        log.Info("Update the Upload Information");
                        
                        //Update the Upload.
                        var uploadService = new UploadService(ConnectionString.ConnectionStringDB);
                        uploadService.UpdateUploadInformation(UWSSerialNumber, UwsUwsFileLocation.Trim(),
                            Convert.ToDateTime(UwsCollInfoStartTimestamp), Convert.ToDateTime(UwsCollInfoEndTimestamp),
                            ConnectionString.AdvisorEmail, ConnectionString.SupportEmail,
                            ConnectionString.WebSite,
                            ConnectionString.EmailServer, ConnectionString.EmailPort, ConnectionString.EmailUser,
                            ConnectionString.EmailPassword, ConnectionString.EmailAuthentication,
                            ConnectionString.SystemLocation, ConnectionString.ServerPath,
                            ConnectionString.EmailIsSSL,
                            ConnectionString.IsLocalAnalyst, ConnectionString.MailGunSendAPIKey, ConnectionString.MailGunSendDomain);
                    }
                    catch (Exception ex) {
                        log.ErrorFormat("Update Upload Error: {0}",ex.Message);
                        
                    }
                }
                else {
                    log.Info("**********System Serial is Empty!!!!");
                    
                    success = false;
                }
            }
            return success;
        }

        /// <summary>
        /// Get the entity name that shows in entity tables.
        /// </summary>
        /// <param name="entityName">Entity name.</param>
        /// <returns>Return a string value that would shows in entity tables.</returns>
        internal string GetRAEntityName(string entityName) {
            string returnValue;

            switch (entityName.ToUpper()) {
                case "CPU":
                    returnValue = "CPU";
                    break;
                case "DISC":
                    returnValue = "DISK";
                    break;
                case "TMF":
                    returnValue = "TMF";
                    break;
                case "NETLINE":
                    returnValue = "NETLINE";
                    break;
                case "SERVERN":
                    returnValue = "SERVERNET";
                    break;
                case "DISKFIL":
                    returnValue = "DISKFILE";
                    break;
                case "SQLPROC":
                    returnValue = "SQLPROC";
                    break;
                case "SQLSTMT":
                    returnValue = "SQLSTMT";
                    break;
                case "DISCOPE":
                    returnValue = "DISCOPEN";
                    break;
                case "FILE":
                    returnValue = "FILE";
                    break;
                default:
                    returnValue = entityName.ToUpper();
                    break;
            }

            return returnValue;
        }

        /// <summary>
        /// Create the table structure.
        /// </summary>
        /// <param name="columnInfo"> List of ColumnInfoView which contains the column info of the table that going to be created.</param>
        /// <param name="tableName"> Name of the table.</param>
        /// <returns> Return a DataTable that contains the table structure.</returns>
        internal DataTable CreateDataTableColumn(List<ColumnInfoView> columnInfo, string tableName) {
            var myDataSet = new DataSet();
            //This DataTableName has be to start Date(only date part), because I have to compare with data's FromTimestamp.
            //string buildDataTableName = string.Empty;

            var myDataTable = new DataTable();

            // Create new DataColumn, set DataType, ColumnName and add to DataTable.    
            var myDataColumn = new DataColumn { DataType = Type.GetType("System.Int16"), ColumnName = "TSID" };

            // Add the Column to the DataColumnCollection.
            myDataTable.Columns.Add(myDataColumn);

            // Create new DataColumn, set DataType, ColumnName and add to DataTable.    
            myDataColumn = new DataColumn { DataType = Type.GetType("System.Int16"), ColumnName = "DataClass" };
            // Add the Column to the DataColumnCollection.
            myDataTable.Columns.Add(myDataColumn);

            // Create new DataColumn, set DataType, ColumnName and add to DataTable.    
            myDataColumn = new DataColumn { DataType = Type.GetType("System.Int32"), ColumnName = "GID" };
            // Add the Column to the DataColumnCollection.
            myDataTable.Columns.Add(myDataColumn);
            if (tableName == "RadcRaproc") {
                // Create new DataColumn, set DataType, ColumnName and add to DataTable.    
                myDataColumn = new DataColumn { DataType = Type.GetType("System.Int16"), ColumnName = "Proctype" };
                // Add the Column to the DataColumnCollection.
                myDataTable.Columns.Add(myDataColumn);
            }

            if (tableName == "RadcRadskfle" ||
                tableName == "RadcRaproc" ||
                tableName == "RadcRasqlpr" ||
                tableName == "RadcRasqlst") {
                // Create new DataColumn, set DataType, ColumnName and add to DataTable.    
                myDataColumn = new DataColumn { DataType = Type.GetType("System.String"), ColumnName = "Realname" };
                // Add the Column to the DataColumnCollection.
                myDataTable.Columns.Add(myDataColumn);
            }

            if (tableName == "RadcRacpu" ||
                tableName == "RadcRaproc" ||
                tableName == "RadcRatmf" ||
                tableName == "RadcRanetlne" ||
                tableName == "RadcRasvnet" ||
                tableName == "RadcRasqlpr" ||
                tableName == "RadcRasqlst" ||
                tableName == "RadcRadskfle") {
                // Create new DataColumn, set DataType, ColumnName and add to DataTable.    
                myDataColumn = new DataColumn { DataType = Type.GetType("System.DateTime"), ColumnName = "FromTimeDay" };
                // Add the Column to the DataColumnCollection.
                myDataTable.Columns.Add(myDataColumn);
            }

            foreach (ColumnInfoView column in columnInfo) {
                // Create new DataColumn, set DataType, ColumnName and add to DataTable.    
                myDataColumn = new DataColumn { DataType = Type.GetType(GetSystemValueType(column.TypeName)), ColumnName = column.ColumnName };
                // Add the Column to the DataColumnCollection.
                myDataTable.Columns.Add(myDataColumn);
            }

            myDataSet.Tables.Add(myDataTable);

            return myDataSet.Tables[0];
        }

        /// <summary>
        /// Create the table structure for the entity tables.
        /// </summary>
        /// <param name="days"> List of MultiDays. </param>
        /// <param name="columnInfo"> List of ColumnInfoView which contains the column info of the table that going to be created.</param>
        /// <returns> Returns a DataSet that contains the tables structures.</returns>
        internal DataSet CreateDataTableColumn(List<MultiDays> days, List<ColumnInfoView> columnInfo) {
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

                    foreach (ColumnInfoView column in columnInfo) {
                        // Create new DataColumn, set DataType, ColumnName and add to DataTable.    
                        myDataColumn = new DataColumn { DataType = Type.GetType(GetSystemValueType(column.TypeName)), ColumnName = column.ColumnName };
                        // Add the Column to the DataColumnCollection.
                        myDataTable.Columns.Add(myDataColumn);
                    }

                    myDataSet.Tables.Add(myDataTable);
                }
            }

            return myDataSet;
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
        /// <param name="log"> log4net instance.</param>
        /// <param name="uwsID"> UWS ID of this load. </param>
        /// <param name="newConnectionString">Connection string of the system database.</param>
        /// <param name="uwsVersion">Enum class that tell version of UWS file.</param>
        /// <returns> Return a bool value suggests whether the load is successful or not.</returns>
        public bool CreateMultiDayDataSet(string uwsPath, ILog log, int uwsID, string newConnectionString, UWS.Types uwsVersion, int ntsId) {
            log.Info("    -Populating SPAM Database - RemoteAnalyst.UWSLoader.SPAM.BLL.OpenUWS");
            

            bool success = false;
            bool intervalMatch = false;
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
            DateTime startTimeLCT;
            DateTime stopTimeLCT;

            //var discopenHours = new List<string>();
            var uniqueTableName = new List<string>();
            var tableNames = new List<string>();
            var currentTables = new List<MultiDays>();
            var tableTimestamps = new List<MultiDays>();
            var tableNameList = new List<string>(); //Used for updating the archive IDs after uploading to Amazon Glacier
            var entityList = new Dictionary<string, string>();
            var processTableList = new List<string>();
            var dataStartDate = new DateTime();
            var dataStopDate = new DateTime();
            var uwsStartDate = new DateTime();
            var uwsStopDate = new DateTime();
            var currentTable = new CurrentTableService(newConnectionString);
            //var daysMySQL = new List<MultiDays>();//Used to populate the mysql DISC table

            string fileEntity = "";
            bool isProcessDirectlySystem = false;

            //var mySqlPopulate = new List<int> {1, 2, 4, 5, 7, 22};  //1:CPU 2:DISC 4:DISKFIL 5:FILE 7:PROCESS 22: USERDEF

            try {
                var systemTbl = new System_tblService(ConnectionString.ConnectionStringDB);
                systemName = systemTbl.GetSystemNameFor(UWSSerialNumber);


                #region Load Data

                log.InfoFormat("******   Start loading data at {0} *******", DateTime.Now);
                var websiteLoad = false;
                if (ConfigurationManager.AppSettings["WebsiteLoad"] != null)
                    websiteLoad = Convert.ToBoolean(ConfigurationManager.AppSettings["WebsiteLoad"]);


                //Check if the system type is NTS.
                var ntsOrder = false;
                if (systemTbl.IsNTSSystemFor(UWSSerialNumber)) {
                    websiteLoad = false;
                    ntsOrder = true;
                }
                log.InfoFormat("      ntsOrder: {0}, websiteLoad: {1}", ntsOrder, websiteLoad);
                
                using (var stream = new FileStream(uwsPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    using (reader = new BinaryReader(stream)) {
                        double dblActualFileSize = stream.Length;
                        // LOOP THRU ALL ENTITIES
                        foreach (Indices t in index) {
                            if (t.FName.Length != 0 && t.FRecords > 0) {
                                // Get eneityID.
                                var entity = new EntitiesService(newConnectionString);
                                entityID = entity.GetEntityIDFor(t.FName.Trim());
                                if (entityID == 0) continue; // NOT A VALID ENTITY

                                //Check for duplicated process when we have userdef.
                                if (index.Any(x => x.FName.Trim().Equals("USERDEF"))) {
                                    //Check number of index.
                                    if (index.Count.Equals(2) && entityID == (int)Entity.PROCESS) {
                                        log.InfoFormat("****Skiping Entity Name: {0}", t.FName.Trim());
                                        
                                        //Stop the process. 
                                        continue;
                                    } else {
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

                                double dblCalculatedFileSize = (t.FReclen * t.FRecords) + t.FilePosition;
                                if (dblCalculatedFileSize > dblActualFileSize) {
                                    // CALCULATED SIZE IS GREATER THAN ACTUAL SIZE.  PARTIAL FILE RECEIVED. 
                                    var emailText = new StringBuilder();
                                    emailText.Append("<br>A UWS file was partially loaded at " + DateTime.Now + ":");
                                    emailText.Append("<UL>");
                                    emailText.Append("	<LI>");
                                    emailText.Append("		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>Node:  " + t.FSysName + " </DIV>");
                                    emailText.Append("	<LI>");
                                    emailText.Append("		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>Erroneous Entity:  " + t.FName + " </DIV>");
                                    emailText.Append("	<LI>");
                                    emailText.Append("		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>UWS File: " + uwsPath.Trim() + "</DIV>");
                                    emailText.Append("	<LI>");
                                    emailText.Append("		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>Actual File Size: " + dblActualFileSize + "</DIV>");
                                    emailText.Append("	<LI>");
                                    emailText.Append("		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>Computed File Size: " + dblCalculatedFileSize + "</DIV>");
                                    emailText.Append("	</LI>");
                                    emailText.Append("</UL>");
                                    var email = new EmailHelper();
                                    email.SendErrorEmail(emailText.ToString());

                                    //Request for Khody, load the all the data that can we loaded, and update the Current and Table Timestamp.

                                    UpdateLogEntryAndStatus(UwsUwsFileLocation.Trim(), "Partial Data Loaded", "Failed");

                                    //if uwsfilesize is 0, stopTime the load.
                                    if (UwsUwsFileSize.Trim().Equals("0")) {
                                        reader.Close();
                                        stream.Close();
                                        if (File.Exists(uwsPath)) File.Delete(uwsPath);

                                        return false;
                                    }
                                }

                                log.InfoFormat("Entity Name: {0}, Started at {1}", t.FName.Trim(), DateTime.Now);
                                
                                DateTime beforeTime = DateTime.Now;

                                if (systemName.Length == 0) systemName = t.FSysName;
                                if (sampleInterval == 0) sampleInterval = t.FInterval;

                                string dbTableName;
                                if (uwsVersion == UWS.Types.Version2007 || uwsVersion == UWS.Types.Version2009) {
                                    //Start Time.
                                    if (startTime == 0) startTime = t.FStartTime;
                                    else if (startTime > t.FStartTime) startTime = t.FStartTime;

                                    //Stop Time.
                                    if (stopTime == 0) stopTime = t.FStopTime;
                                    else if (stopTime < t.FStopTime) stopTime = t.FStopTime;

                                    //Get table name according to data format.
                                    var mVersion = new MeasureVersionsService(newConnectionString);
                                    dbTableName = mVersion.GetMeasureDBTableNameFor(t.FMeasVer);
                                    log.InfoFormat("      Measure Type: {0}",dbTableName);
                                    

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
                                    log.InfoFormat("      Measure Type: {0}",dbTableName);
                                    

                                    //change the table name to new table. This is only for new uws header version.
                                    if (dbTableName.Equals("ZmsBladeDataDictionary") || dbTableName.Equals("ZmsDataDictionary")) {
                                        var vProc = new VProcVersionService(ConnectionString.ConnectionStringDB);
                                        dbTableName = vProc.GetDataDictionaryFor(UwsCreatorVproc);
                                    }

                                    log.InfoFormat("      New Measure Type: {0}",dbTableName);
                                    
                                    dataStartDate = t.CollEntityStartTime;
                                    dataStopDate = t.CollEntityStoptTime;

                                    //Start Time.
                                    if (uwsStartDate.Equals(DateTime.MinValue)) uwsStartDate = t.CollEntityStartTime;
                                    else if (uwsStartDate > t.CollEntityStartTime) uwsStartDate = t.CollEntityStartTime;

                                    //Stop Time.
                                    if (uwsStopDate.Equals(DateTime.MinValue)) uwsStopDate = t.CollEntityStoptTime;
                                    else if (uwsStopDate < t.CollEntityStoptTime) uwsStopDate = t.CollEntityStoptTime;
                                }

                                log.InfoFormat("      SystemSerial: {0}",UWSSerialNumber);
                                

                                //Get Column type into the List.
                                var dictionary = new DataDictionaryService(newConnectionString);
#if(DEBUG)
                                dbTableName = "ZmsBladeDataDictionaryV1";
#endif
                                IList<ColumnInfoView> columnInfo = dictionary.GetColumnsFor(entityID, dbTableName);

                                int recordLenth = t.FReclen;
                                long filePosition = t.FilePosition;

                                //Round up the seconds.
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

                                //Spcial case for VISA, load the overlap data.
                                var systemTblService = new System_tblService(_connectionStr);
                                isProcessDirectlySystem = systemTblService.isProcessDirectlySystemFor(UWSSerialNumber);

                                if (isProcessDirectlySystem) {
                                    //Check if stop hour is 10, 12, 14, 16
                                    if (dataStopDate.Hour.Equals(10) ||
                                        dataStopDate.Hour.Equals(12) ||
                                        dataStopDate.Hour.Equals(14) ||
                                        dataStopDate.Hour.Equals(16)) {
                                        //Check if stop minute is less than 5%. 2 hrs * 5 % => 6 minutes
                                        var allowanceMinute = 6;
                                        if (dataStopDate.Minute <= allowanceMinute) {
                                            dataStopDate = dataStopDate.AddMinutes(-1 * dataStopDate.Minute);
                                            dataStopDate = dataStopDate.AddSeconds(-1 * dataStopDate.Second);
                                        }
                                    }
                                }

                                log.InfoFormat("      dataStartDate: {0}, dataStopDate: {1}", dataStartDate, dataStopDate);

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
                                        if (entityID.Equals(7)) {
                                            if (!processTableList.Contains(buildTableName))
                                                processTableList.Add(buildTableName);
                                        }
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
                                    if (entityID.Equals(7)) {
                                        if (!processTableList.Contains(buildTableName))
                                            processTableList.Add(buildTableName);
                                    }

                                    multiDays.DontLoad = false;
                                    days.Add(multiDays);

                                    //SAVE GENERIC TABLE NAME FOR UniqueID PROCESSING
                                    if (!entityAccountedFor) {
                                        buildTableName = UWSSerialNumber + "_%_" + dataStopDate.Year + "_" + dataStopDate.Month + "_" + dataStopDate.Day;
                                        uniqueTableName.Add(buildTableName);
                                        entityAccountedFor = true;
                                    }
                                } else {
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
                                    if (entityID.Equals(7)) {
                                        if (!processTableList.Contains(buildTableName))
                                            processTableList.Add(buildTableName);
                                    }
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
                                            mySQLServices.CreateEntityTable(entityID, d.SystemSerial, d.TableName, 
                                                columnInfo, websiteLoad, ConnectionString.ConnectionStringDB, 
                                                log, true, isProcessDirectlySystem, ConnectionString.DatabasePrefix);

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
                                        } else {
                                            //Check if Interval matches.
                                            intervalMatch = false;
                                            long currentInterval = currentTable.GetIntervalFor(d.TableName);
                                            if (currentInterval == d.Interval) {
                                                intervalMatch = true;
                                            } else {
                                                //Check with Temp current table interval.
                                                var tempCurrentTable = new TempCurrentTablesService(newConnectionString);
                                                currentInterval = tempCurrentTable.GetIntervalFor(d.TableName);
                                                if (currentInterval == d.Interval) {
                                                    intervalMatch = true;
                                                }
                                            }

                                            if (intervalMatch) {
                                                log.Info("      Check if Time Stamp don't over laps.");
                                                

                                                var tempTableTimeStamp = new TempTableTimestampService(newConnectionString);
                                                var tableTimeStamp = new TableTimeStampService(newConnectionString);
                                                bool timeOverLap = tableTimeStamp.CheckTimeOverLapFor(d.TableName, d.StartDate, d.EndDate);

                                                if (!isProcessDirectlySystem && !timeOverLap) {
													//Check if Time Stamp don't over laps from TempCurrentTable.
													bool tempTimeOverLap = tableTimeStamp.CheckTimeOverLapFor(d.TableName, d.StartDate, d.EndDate);
													if (!tempTimeOverLap) {
														tempTimeOverLap = tableTimeStamp.CheckTempTimeOverLapFor(d.TableName, d.StartDate, d.EndDate);
														if (!tempTimeOverLap) {
															if (!systemTbl.AllowOverlappingDataFor(UWSSerialNumber)) {
																d.DontLoad = true;
																log.Info("      DontLoad = true***");
																
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
												}else if (isProcessDirectlySystem) {        //Check VISA in this block since VISA has the entire interval for each CPU, TableTimeStamp is insufficient to check duplicate                                            
                                                    var loadingInfo = new LoadingInfoService(_connectionStr);
                                                    bool isLoaded = loadingInfo.IsUwsFileLoad(Path.GetFileName(uwsPath));
                                                    if (isLoaded) {
                                                        if (!systemTbl.AllowOverlappingDataFor(UWSSerialNumber)) {
                                                            d.DontLoad = true;
                                                            log.Info("      DontLoad = true***");
                                                            
                                                        } else {
                                                            //Insert Time Stamps. 
                                                            tempTableTimeStamp.InsertTempTimeStampFor(d.TableName, d.StartDate, d.EndDate, Path.GetFileName(uwsPath));
                                                            tableTimestamps.Add(d);
                                                        }
                                                    } else {
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
											} else {
                                                //Stop this process.
                                                //continue;
                                                //load = false;
                                                //break;
                                                d.DontLoad = true;
                                            }
                                        }

                                        #endregion
                                    } else {
                                        d.DontLoad = true;
                                    }

                                    //Get Entity List.
                                    if (entityID == (int)Entity.CPU) {
                                        if (!entityList.ContainsKey("CPU")) {
                                            entityList.Add("CPU", "[" + d.TableName + "]");
                                        }
                                    } else if (entityID == (int)Entity.DISC) {
                                        if (!entityList.ContainsKey("DISK")) {
                                            entityList.Add("DISK", "[" + d.TableName + "]");
                                        }
                                    } else if (entityID == (int)Entity.PROCESS) {
                                        if (!entityList.ContainsKey("PROCESS")) {
                                            entityList.Add("PROCESS", "[" + d.TableName + "]");
                                        }
                                    } else if (entityID == (int)Entity.TMF) {
                                        if (!entityList.ContainsKey("TMF")) {
                                            entityList.Add("TMF", "[" + d.TableName + "]");
                                        }
                                    } else if (entityID == (int)Entity.FILE) {
                                        if (fileEntity.Length.Equals(0))
                                            fileEntity = d.TableName;

                                        if (!entityList.ContainsKey("FILE")) {
                                            entityList.Add("FILE", "[" + d.TableName + "]");
                                        }
                                    } else if (entityID == (int)Entity.FABRIC) {
                                        if (!entityList.ContainsKey("FABRIC")) {
                                            entityList.Add("FABRIC", "[" + d.TableName + "]");
                                        }
                                    }
                                    /*else if (entityID == 3) { DISCOPE is not need to load Trend Data.
                                        if (!entityList.ContainsKey("DISCOPE")) {
                                            entityList.Add("DISCOPE", "[" + d.TableName + "]");
                                        }
                                    }*/
                                }

                                #endregion


                                if (!columnInfo.Any(x => x.Website.Equals(true)) && !ntsOrder && !ConnectionString.IsLocalAnalyst) {
                                    //IF NONE OF THE COLUMNS ARE "Website" FLAGGED, UPDATE "CurrentTable" AND "TableTimestamp" BEFORE BYPASSING TABLE.
                                    #region Update CurrentTable & TableTimestamp.
                                    if (currentTables.Count > 0) {
                                        var tempCurrentTable = new TempCurrentTablesService(newConnectionString);
                                        foreach (var d in currentTables) {
                                            try {
                                                //Insert basic info into the Current Table.
                                                currentTable.InsertEntryFor(d.TableName, d.EntityID, d.Interval, Convert.ToDateTime(d.StartDate.ToShortDateString()), d.SystemSerial, d.MeasureVersion);
                                            }
                                            catch (Exception ex) {
                                                log.ErrorFormat("Insert CurrentTable: {0}",ex.Message);
                                                
                                            }
                                            try {
                                                //Delete from TempCurrentTable.
                                                tempCurrentTable.DeleteCurrentTableFor(d.TableName);
                                            }
                                            catch (Exception ex) {
                                                log.ErrorFormat("Delete TempCurrentTable: {0}",ex.Message);
                                                
                                            }
                                        }
                                        currentTables.Clear();
                                    }

                                    if (tableTimestamps.Count > 0) {
                                        var tableTimeStamp = new TableTimeStampService(newConnectionString);
                                        var tempTimeStamp = new TempTableTimestampService(newConnectionString);

                                        foreach (var d in tableTimestamps) {
                                            try {
                                                //Insert basic info into the Current Table. - make default status "Active"
                                                tableTimeStamp.InsertEntryFor(d.TableName, d.StartDate, d.EndDate, (int)ArchiveStatus.Status.Active, Path.GetFileName(uwsPath));
                                                tableNameList.Add(d.TableName);
                                            }
                                            catch (Exception ex) {
                                                log.ErrorFormat("Insert TableTimeStamp: {0}",ex.Message);
                                                
                                            }
                                            try {
                                                //Delete from TempCurrentTable.
                                                tempTimeStamp.DeleteTempTimeStampFor(d.TableName);
                                            }
                                            catch (Exception ex) {
                                                log.ErrorFormat("Delete TempTimeStamp: {0}",ex.Message);
                                                
                                            }
                                        }
                                        tableTimestamps.Clear();
                                    }
                                    #endregion

                                    var uwsFileCounts = new UWSFileCountService(ConnectionString.ConnectionStringDB);
                                    var isExists = uwsFileCounts.CheckDuplicateFor(UWSSerialNumber, dataStartDate.Date);
                                    if (isExists) {
                                        uwsFileCounts.UpdateActualFileCountFor(UWSSerialNumber, dataStartDate.Date);
                                    }
                                    continue;
                                }

                                if (entityID == 8 && websiteLoad) {
                                    log.InfoFormat("      ***** Skipping SQLPROC for websiteLoad: {0}", websiteLoad);
                                    
                                    continue;
                                }
                                if (entityID == 9 && websiteLoad) {
                                    log.InfoFormat("      ***** Skipping SQLSTMT for websiteLoad: {0}", websiteLoad);
                                    
                                    continue;
                                }
                                if (entityID == 8 && UWSSerialNumber.CompareTo("078781") == 0) {
                                    log.Info("      ***** Skipping SQLPROC for 078781");
                                    
                                    continue;
                                }
                                if (entityID == 9 && UWSSerialNumber.CompareTo("078781") == 0) {
                                    log.Info("      ***** Skipping SQLSTMT for 078781");
                                    
                                    continue;
                                }
                                if (entityID == 3 && UWSSerialNumber.CompareTo("078781") == 0) {
                                    log.Info("      ***** Skipping DISCOPEN for 078781");
                                    
                                    continue;
                                }

                                //New Logic. IR 6391, skip the discopen entity.
                                if (entityID == 3 && !ntsOrder) {
                                    log.Info("      ***** Skipping DISCOPEN");
                                    
                                    continue;
                                }

                                var myDataSet = new DataSet();

                                try {
                                    #region Create DataSet and insert data

                                    myDataSet = CreateSPAMDataTableColumn(days, columnInfo, websiteLoad);
                                    if (myDataSet.Tables.Count > 0) {
                                        //Test values for tableName From FromTimeStamp.
                                        var tableNameFromTimeStamp = new DateTime();

                                        //Loop through the records.
                                        for (int x = 0; x < t.FRecords; x++) {
                                            reader.BaseStream.Seek(filePosition, SeekOrigin.Begin);
                                            byte[] indexBytes = reader.ReadBytes(recordLenth);
                                            long currentPosition = 0;

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
                                                                    } else {
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
                                                                } else if (column.ColumnName == "ToTimestamp" && (entityID == 3 || entityID == 5)) {
                                                                    toJulian = tempDate;
                                                                }

                                                                if (tempDate == 0) {
                                                                    if (entityID.Equals(22))
                                                                        column.TestValue = DateTime.Now.ToString();
                                                                    else
                                                                        column.TestValue = "";
                                                                } else {
                                                                    var convert = new ConvertJulianTime();
                                                                    int obdTimeStamp = convert.JulianTimeStampToOBDTimeStamp(tempDate);
                                                                    DateTime dbDate = convert.OBDTimeStampToDBDate(obdTimeStamp);

                                                                    if (column.ColumnName == "FromTimestamp") {
                                                                        tableNameFromTimeStamp = dbDate;
                                                                        /*if (!dbDate.Date.Equals(dataStopDate.Date))
                                                                            tableNameFromTimeStamp = dbDate;
                                                                        else
                                                                            tableNameFromTimeStamp = dataStartDate;*/
                                                                    }

                                                                    //check if the entity is CPU and counter is FromTimestamp.
                                                                    if (entityID == 1 && (column.ColumnName.ToUpper().Trim() == "FROMTIMESTAMP" || column.ColumnName.ToUpper().Trim() == "TOTIMESTAMP")) {
                                                                        //Create a list of datetime according to interval.
                                                                        var intervalList = new Dictionary<DateTime, DateTime>();
                                                                        DateTime tempStartDate = dataStartDate;

                                                                        double tenPercent;
                                                                        if (column.ColumnName.ToUpper().Trim() == "FROMTIMESTAMP") {
                                                                            tenPercent = -0.2;
                                                                        } else {
                                                                            tenPercent = 0.2;
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
                                                                                    dbDate = kv.Key.AddSeconds(t.FInterval * (tenPercent * -1));
                                                                                    break;
                                                                                }
                                                                            } else {
                                                                                if (dbDate >= kv.Key && dbDate <= kv.Value) {
                                                                                    dbDate = kv.Value.AddSeconds(t.FInterval * (tenPercent * -1));
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
                                                                column.TestValue = DateTime.MinValue.ToString();
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
                                                    }

                                                    #endregion

                                                    currentPosition += column.TypeValue;
                                                } else {
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
                                                            } else {
                                                                myDataRow[column.ColumnName] = Convert.ToUInt16(column.TestValue);
                                                            }
                                                            break;
                                                        case "BIGINT": //NOTE: VARIABLE COMING IN AS "INT" AND SAVED AS "BIGINT"
                                                            if (column.ColumnName.ToUpper().Trim() == "UNIQUEID") {
                                                                //hard code the UniqueID to 1. this column is for our use.
                                                                myDataRow[column.ColumnName] = 1;
                                                            } else {
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
                                                                tempFromtimestamp = Convert.ToDateTime(column.TestValue);
                                                            } else if (column.ColumnName.ToUpper().Trim() == "TOTIMESTAMP") {
                                                                //if (Convert.ToDateTime(column.TestValue) == oldDataStopDate) {
                                                                //    column.TestValue = dataStopDate.ToString();
                                                                //}//If ToTimestamp is greater than endtime, change the value to endtime.
                                                                if (Convert.ToDateTime(column.TestValue) <= dataStartDate)
                                                                {
                                                                    deleteRow = true;
                                                                }
                                                                if (column.TestValue.Length == 0) {
                                                                    //Use the FromTimestamp
                                                                    column.TestValue = tempFromtimestamp.ToString();
                                                                } else {
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
                                                } else {
                                                    //Add new row into the dataSet.
                                                    myDataSet.Tables[tableNameFromTimeStamp.ToString("yyyy/MMM/dd")].Rows.Add(myDataRow);
                                                    string tempTableName = UWSSerialNumber + "_" + t.FName.Trim().ToUpper() + "_" + tableNameFromTimeStamp.Year + "_" + tableNameFromTimeStamp.Month + "_" + tableNameFromTimeStamp.Day;

                                                    //Insert TableName for UniqueID.
                                                    if (!tableNames.Contains(tempTableName)) {
                                                        tableNames.Add(tempTableName);
                                                    }

                                                    var counter = 0;
                                                    foreach (DataTable table in myDataSet.Tables) {
                                                        counter += table.Rows.Count;
                                                    }

                                                    if (counter > Constants.getInstance(ConnectionString.ConnectionStringDB).BulkLoaderSize) {
                                                        foreach (MultiDays d in days) {
                                                            if (!d.DontLoad) {
                                                                try {
                                                                    if (myDataSet.Tables[d.StartDate.ToString("yyyy/MMM/dd")].Rows.Count > 0) {
                                                                        var mySQLServices = new MySQLServices();
                                                                        mySQLServices.InsertEntityDatas(d.SystemSerial, d.TableName,
                                                                            myDataSet.Tables[d.StartDate.ToString("yyyy/MMM/dd")], ConnectionString.ConnectionStringDB,
                                                                            ConnectionString.SystemLocation, entityID, true, log);
                                                                    }
                                                                }
                                                                catch (Exception ex) {
                                                                    log.ErrorFormat("    -Insert into MySQL Error: {0}", ex.Message);
                                                                    
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
                                                        mySQLServices.InsertEntityDatas(d.SystemSerial, d.TableName, myDataSet.Tables[d.StartDate.ToString("yyyy/MMM/dd")],
                                                            ConnectionString.ConnectionStringDB,
                                                            ConnectionString.SystemLocation, entityID, true, log);
                                                    }
                                                }
                                                catch (Exception ex) {
                                                    log.ErrorFormat("    -Insert into MySQL Error: {0}", ex.Message);
                                                    
                                                }
                                            }
                                        }
                                        DateTime afterTime = DateTime.Now;
                                        TimeSpan timeSpan = afterTime - beforeTime;
                                        log.InfoFormat("    -(Line 1896)EntityID: {0} Total Time in Minutes: {1}", entityID, timeSpan.TotalMinutes);
                                    }

                                    #endregion

                                    #region Update CurrentTable & TableTimestamp.

                                    #region Current Table
                                    if (currentTables.Count > 0) {
                                        var tempCurrentTable = new TempCurrentTablesService(newConnectionString);

                                        foreach (var d in currentTables) {
                                            try {
                                                //Insert basic info into the Current Table.
                                                currentTable.InsertEntryFor(d.TableName, d.EntityID, d.Interval, Convert.ToDateTime(d.StartDate.ToShortDateString()), d.SystemSerial, d.MeasureVersion);
                                            }
                                            catch (Exception ex) {
                                                log.ErrorFormat("Insert CurrentTable: {0}",ex.Message);
                                                
                                            }
                                            try {
                                                //Delete from TempCurrentTable.
                                                tempCurrentTable.DeleteCurrentTableFor(d.TableName);
                                            }
                                            catch (Exception ex) {
                                                log.ErrorFormat("Delete TempCurrentTable: {0}",ex.Message);
                                                
                                            }
                                        }
                                        currentTables.Clear();
                                    }
                                    #endregion

                                    #region TableTimestamp
                                    if (tableTimestamps.Count > 0) {
                                        var tableTimeStamp = new TableTimeStampService(newConnectionString);
                                        var tempTimeStamp = new TempTableTimestampService(newConnectionString);

                                        foreach (var d in tableTimestamps) {
                                            try {
                                                //Insert basic info into the Current Table. - make default status "Active"
                                                tableTimeStamp.InsertEntryFor(d.TableName, d.StartDate, d.EndDate, (int)ArchiveStatus.Status.Active, Path.GetFileName(uwsPath));
                                                tableNameList.Add(d.TableName);
                                            }
                                            catch (Exception ex) {
                                                log.ErrorFormat("Insert TableTimeStamp: {0}",ex.Message);
                                                
                                            }
                                            try {
                                                //Delete from TempCurrentTable.
                                                tempTimeStamp.DeleteTempTimeStampFor(d.TableName);
                                            }
                                            catch (Exception ex) {
                                                log.ErrorFormat("Delete TempTimeStamp: {0}",ex.Message);
                                                
                                            }
                                        }
                                        tableTimestamps.Clear();
                                    }
                                    #endregion
                                    #endregion

                                }
                                catch (Exception ex) {
                                    bool partialRecoverySuccess = false;
                                    if (myDataSet.Tables.Count > 0) {
                                        #region Insert rest of the data.
                                        log.Info("******Insert rest of the data");
                                        
                                        //Insert what's on the dataset.
                                        bool inserted = false;
                                        foreach (MultiDays d in days) {
                                            if (!d.DontLoad) {
                                                try {
                                                    log.InfoFormat("******Insert: {0}",myDataSet.Tables[d.StartDate.ToString("yyyy/MMM/dd")].Rows.Count);
                                                    
                                                    if (myDataSet.Tables[d.StartDate.ToString("yyyy/MMM/dd")].Rows.Count > 0) {
                                                        inserted = true;
                                                        var mySQLServices = new MySQLServices();
                                                        mySQLServices.InsertEntityDatas(d.SystemSerial, d.TableName, myDataSet.Tables[d.StartDate.ToString("yyyy/MMM/dd")], ConnectionString.ConnectionStringDB,
                                                            ConnectionString.SystemLocation, entityID, true, log);
                                                        partialRecoverySuccess = true;
                                                    }
                                                }
                                                catch (Exception exSub) {
                                                    log.ErrorFormat("    -Insert into MySQL Error: {0}", exSub.Message);
                                                    
                                                }
                                            }
                                        }

                                        if (inserted) {
                                            log.Info("******Updating CurrentTable and TableTimestamp");
                                            
                                            #region Current Table

                                            if (currentTables.Count > 0) {
                                                var tempCurrentTable = new TempCurrentTablesService(newConnectionString);

                                                foreach (var d in currentTables) {
                                                    try {
                                                        //Insert basic info into the Current Table.
                                                        currentTable.InsertEntryFor(d.TableName, d.EntityID, d.Interval, Convert.ToDateTime(d.StartDate.ToShortDateString()), d.SystemSerial, d.MeasureVersion);
                                                    }
                                                    catch (Exception exSub) {
                                                        log.ErrorFormat("Insert CurrentTable: {0}",exSub.Message);
                                                        
                                                    }
                                                    try {
                                                        //Delete from TempCurrentTable.
                                                        tempCurrentTable.DeleteCurrentTableFor(d.TableName);
                                                    }
                                                    catch (Exception exSub) {
                                                        log.ErrorFormat("Delete TempCurrentTable: {0}",exSub.Message);
                                                        
                                                    }
                                                }
                                                currentTables.Clear();
                                            }

                                            #endregion

                                            #region TableTimestamp

                                            if (tableTimestamps.Count > 0) {
                                                var tableTimeStamp = new TableTimeStampService(newConnectionString);
                                                var tempTimeStamp = new TempTableTimestampService(newConnectionString);

                                                foreach (var d in tableTimestamps) {
                                                    try {
                                                        //Insert basic info into the Current Table. - make default status "Active"
                                                        tableTimeStamp.InsertEntryFor(d.TableName, d.StartDate, d.EndDate, (int)ArchiveStatus.Status.Active, Path.GetFileName(uwsPath));
                                                        tableNameList.Add(d.TableName);
                                                    }
                                                    catch (Exception exSub) {
                                                        log.ErrorFormat("Insert TableTimeStamp: {0}",exSub.Message);
                                                        
                                                    }
                                                    try {
                                                        //Delete from TempCurrentTable.
                                                        tempTimeStamp.DeleteTempTimeStampFor(d.TableName);
                                                    }
                                                    catch (Exception exSub) {
                                                        log.ErrorFormat("Delete TempTimeStamp: {0}",exSub.Message);
                                                        
                                                    }
                                                }
                                                tableTimestamps.Clear();
                                            }

                                            #endregion
                                        }

                                        #endregion
                                    }

                                    log.ErrorFormat("EntityID: {0}.\n Message: {1}", entityID, ex.Message);
                                    
                                    if (!partialRecoverySuccess) {
                                        throw new Exception(ex.Message);
                                    }
                                    else
                                    {
                                        log.Info("Partially recovered, hence proceeding with other rest of the steps");
                                    }
                                }
                                finally {
                                    columnInfo = null;
                                    myDataSet.Dispose();
                                }
                            }
                        }
                    }
                }

                log.InfoFormat("******   Loading data completed at {0} *******", DateTime.Now);


                #endregion

                try {
                    //Convert starttime and endtime.
                    if (uwsVersion == UWS.Types.Version2007 || uwsVersion == UWS.Types.Version2009) {
                        var julianTime = new ConvertJulianTime();
                        int lctTimeStamp = julianTime.JulianTimeStampToOBDTimeStamp(startTime);

                        startTimeLCT = julianTime.OBDTimeStampToDBDate(lctTimeStamp);
                        lctTimeStamp = julianTime.JulianTimeStampToOBDTimeStamp(stopTime);
                        stopTimeLCT = julianTime.OBDTimeStampToDBDate(lctTimeStamp);
                    } else {
                        startTimeLCT = uwsStartDate;
                        stopTimeLCT = uwsStopDate;
                    }
                    
                    //Round up the seconds.
                    TimeSpan span1 = stopTimeLCT - startTimeLCT;
                    double seconds1 = span1.TotalSeconds;

                    //Get remained seconds.
                    double remainSeconds1 = seconds1 % sampleInterval;
                    if (remainSeconds1 < sampleInterval * 0.1) {
                        stopTimeLCT = stopTimeLCT.AddSeconds(-remainSeconds1);
                    }
                    
                    if (isProcessDirectlySystem) {
                        startTimeLCT = dataStartDate;
                        stopTimeLCT = dataStopDate;
                    }

                    //If stopTimeLCT end at midnight, remove 1 second.
                    if (stopTimeLCT.Hour == 0 && stopTimeLCT.Minute == 0 && stopTimeLCT.Second == 0) {
                        stopTimeLCT = stopTimeLCT.AddSeconds(-1);
                    }

                    #region ZIP the UWS File and save it on S3.
                    var networkLocation = "";


                    log.InfoFormat("******  Zip and save the file to S3 at {0} ********", DateTime.Now);
                    
                    //if (discopenHours.Count > 0) {
                    try {
                        //Zip the UWS file.
                        //Build ZIP File Name.
                        var fileInfo = new FileInfo(uwsPath);
                        string[] fileNames = fileInfo.Name.Split('.');
                        string fileName = UWSSerialNumber + "_" + fileNames[0] + ".zip";

                        log.InfoFormat("****** Before Zip S3 at {0} ********", DateTime.Now);
                        string zipFileLocation = Zipper.CreateZipFile(UWSSerialNumber, uwsPath, fileName, log);
                        log.InfoFormat("****** After Zip S3 at {0} ********", DateTime.Now);
                        
                        string s3Location = string.Empty;
                        if (ConnectionString.IsLocalAnalyst)
                        {
                            networkLocation = ConnectionString.NetworkStorageLocation + "Systems/" + UWSSerialNumber + "/" + fileName;
                            var zipFileInfo = new FileInfo(zipFileLocation);
                            zipFileInfo.CopyTo(networkLocation, true);
                        }
                        else 
                        {
                            s3Location = "Systems/" + UWSSerialNumber + "/" + fileName;

                            log.InfoFormat("****** Before Update to S3 at {0} ********", DateTime.Now);

                            //Upload the file to s3
                            var s3 = new AmazonS3(ConnectionString.S3UWS);
                            s3.WriteToS3WithLocaFile(s3Location, zipFileLocation);
                            log.InfoFormat("****** After Update to S3 at {0} ********", DateTime.Now);
                        }
                        //foreach (string s in discopenHours) {
                        //string[] time = s.Split('|');

                        //DateTime discopenStartTime;
                        //DateTime discopenStopTime;
                        //DateTime.TryParse(time[0], out discopenStartTime);
                        //DateTime.TryParse(time[1], out discopenStopTime);

                        var uwsDirectories = new UWSDirectoryService(_connectionStr);

                        bool duplicate = uwsDirectories.CheckDuplicateTimeFor(UWSSerialNumber, startTimeLCT, stopTimeLCT, s3Location);
                        if (!duplicate) {
                            try {
                                if (File.Exists(zipFileLocation)) {
                                    File.Delete(zipFileLocation);
                                }
                            }
                            catch (Exception ex) {
                                log.ErrorFormat("Zip File Delete Error: {0}",ex.Message);
                                
                            }

                            if (ConnectionString.IsLocalAnalyst) {
                                uwsDirectories.InsertUWSDirectoryFor(UWSSerialNumber, startTimeLCT, stopTimeLCT, networkLocation);
                            }
                            else {
                                uwsDirectories.InsertUWSDirectoryFor(UWSSerialNumber, startTimeLCT, stopTimeLCT, s3Location);
                            }
                        }
                        //}
                    } 
                    catch (Exception ex) {
                        log.ErrorFormat("*****Fail to save UWS data.: {0}", ex.Message);                        
                    }
                    //}
                    log.InfoFormat("******  Zip and save the file to S3 completed at {0}  ********", DateTime.Now);
                    
                    #endregion

                    TimeSpan sp = stopTimeLCT.Date - startTimeLCT.Date;
                    log.InfoFormat("TotalDays: {0}",sp.TotalDays);
                    

                    //Get Database Name.
                    string databaseName = BusinessLogic.Util.Helper.FindKeyName(newConnectionString, BusinessLogic.Util.Helper._DATABASEKEYNAME);

                    bool isLastFile = false;
                    if (isProcessDirectlySystem) {
                        try {
                            log.InfoFormat("Check File Count: UWSSerialNumber: {0}, startTimeLCT.Date: {1}",
                                UWSSerialNumber, startTimeLCT.Date);                            

                            var uwsFileCounts = new UWSFileCountService(ConnectionString.ConnectionStringDB);
                            var isExists = uwsFileCounts.CheckDuplicateFor(UWSSerialNumber, startTimeLCT.Date);
                            if (isExists) {
                                uwsFileCounts.UpdateActualFileCountFor(UWSSerialNumber, startTimeLCT.Date);
                                isLastFile = uwsFileCounts.CheckCurrentCountFor(UWSSerialNumber, startTimeLCT.Date);
                            }

                            isLastFile = CheckVisaTrendLoad(log, newConnectionString, startTimeLCT, stopTimeLCT, startTimeLCT.Date, sampleInterval);

                            log.InfoFormat(" isLastFile: {0}",isLastFile);


                        }
                        catch (Exception ex) {
                            log.ErrorFormat("isLastFile Error: {0}",ex.Message);
                            
                        }
                    }

                    var databaseMappingService = new DatabaseMappingService(ConnectionString.ConnectionStringDB);
                    var mySqlConnectionString = databaseMappingService.GetConnectionStringFor(UWSSerialNumber);

                    DataContext dc = new DataContext(mySqlConnectionString);
                    var doNotloadOtherTrend = false; 
                    log.InfoFormat("doNotloadOtherTrend: {0}",doNotloadOtherTrend);
                    


                    var checkTrend = true;
                    try {
                        var uwsFileInfo = new FileInfo(uwsPath);
                        if (uwsPath.Contains("DO")) {
                            //Check for multi entity file name.
                            if (uwsFileInfo.Name.StartsWith("UMM")) {
                                var tempName = uwsPath.Split('_');
                                checkTrend = tempName[6] != "01E";
                            }
                            else {
                                checkTrend = false;
                            }
                        }
                    }
                    catch (Exception ex) {
                        log.ErrorFormat("checkTrend Error: {0}",ex.Message);
                    }
                    log.InfoFormat("checkTrend: {0}",checkTrend);

                    if (checkTrend) {
                        #region DISC Browser Load

                        var databaseCheck = new Database(mySqlConnectionString);
                        log.Info("******  Load data for disk browser  *******");
                        
                        //var fileEntityTableExists = false; 
                        var processEntityTableExists = false;
                        var cpuTableName = "";
                        var discTableName = "";
                        //fileEntityTableExists = tables.CheckTableFor(fileEntity);
                        foreach (KeyValuePair<string, string> entry in entityList) {
                            string tableName = entry.Value;
                            if (tableName.Contains("["))
                                tableName = tableName.Substring(tableName.IndexOf('[') + 1);
                            if (tableName.Contains("]"))
                                tableName = tableName.Substring(0, tableName.Length - 1);

                            if (entry.Key.Equals("PROCESS")) {
                                processEntityTableExists = databaseCheck.CheckTableExists(tableName, databaseName);
                            }
                            if (entry.Key.Equals("CPU")) {
                                cpuTableName = tableName;
                            }
                            if (entry.Key.Equals("DISK")) {
                                discTableName = tableName;
                            }
                        }
                        log.InfoFormat("processEntityTableExists: {0}", processEntityTableExists);
                        log.InfoFormat("cpuTableName: {0}", cpuTableName);

                        try {
                            if (processEntityTableExists) {
                                if (!isProcessDirectlySystem) {
                                    if (!cpuTableName.Equals("")) {
                                        log.Info("Calling DISK Browser Thread");
                                        
                                        if (sp.TotalDays < 1) {
                                            string cpuTable = UWSSerialNumber + "_CPU_" + dataStartDate.Year + "_" + dataStartDate.Month + "_" + dataStartDate.Day;
                                            string discTable = UWSSerialNumber + "_DISC_" + dataStartDate.Year + "_" + dataStartDate.Month + "_" + dataStartDate.Day;
                                            var diskBrowserLoader = new DiskBrowserLoader();
											var handle = new EventWaitHandle(false, EventResetMode.ManualReset);
											var populateDISCBrowser = new Thread(() => {
												diskBrowserLoader.LoadDiskBrowserData(UWSSerialNumber, newConnectionString, mySqlConnectionString, cpuTable, discTable, sampleInterval);
												handle.Set();
											}) {
                                                IsBackground = true
                                            };
											loadJobThread.Add(handle);
											populateDISCBrowser.Start();
                                        }
                                        else {
                                            for (DateTime dtStart = startTimeLCT; dtStart.Date <= stopTimeLCT.Date; dtStart = dtStart.AddDays(1)) {
                                                string cpuTable = UWSSerialNumber + "_CPU_" + dtStart.Year + "_" + dtStart.Month + "_" + dtStart.Day;
                                                string discTable = UWSSerialNumber + "_DISC_" + dtStart.Year + "_" + dtStart.Month + "_" + dtStart.Day;
                                                log.InfoFormat("Calling DISK Browser: {0}",dtStart);
                                                
                                                var diskBrowserLoader = new DiskBrowserLoader();
                                                diskBrowserLoader.LoadDiskBrowserData(UWSSerialNumber, newConnectionString, mySqlConnectionString, cpuTable, discTable, sampleInterval);
                                            }
                                        }
                                    }
                                }
                                else {
                                    if (isLastFile) {
                                        if (!doNotloadOtherTrend) {
                                            if (sp.TotalDays < 1) {
                                                string cpuTable = UWSSerialNumber + "_CPU_" + dataStartDate.Year + "_" + dataStartDate.Month + "_" + dataStartDate.Day;
                                                string discTable = UWSSerialNumber + "_DISC_" + dataStartDate.Year + "_" + dataStartDate.Month + "_" + dataStartDate.Day;
                                                log.Info("Calling DISK Browser Thread");
                                                
                                                var diskBrowserLoader = new DiskBrowserLoader();
												var handle = new EventWaitHandle(false, EventResetMode.ManualReset);
												var populateDISCBrowser = new Thread(() => {
													diskBrowserLoader.LoadDiskBrowserData(UWSSerialNumber, newConnectionString, mySqlConnectionString, cpuTable, discTable, sampleInterval);
													handle.Set();
												}) {
                                                    IsBackground = true
                                                };
												loadJobThread.Add(handle);
												populateDISCBrowser.Start();
                                            }
                                            else {
                                                for (DateTime dtStart = startTimeLCT; dtStart.Date <= stopTimeLCT.Date; dtStart = dtStart.AddDays(1)) {
                                                    string cpuTable = UWSSerialNumber + "_CPU_" + dtStart.Year + "_" + dtStart.Month + "_" + dtStart.Day;
                                                    string discTable = UWSSerialNumber + "_DISC_" + dtStart.Year + "_" + dtStart.Month + "_" + dtStart.Day;
                                                    log.InfoFormat("Calling DISK Browser: {0}",dtStart);
                                                    
                                                    var diskBrowserLoader = new DiskBrowserLoader();
                                                    diskBrowserLoader.LoadDiskBrowserData(UWSSerialNumber, newConnectionString, mySqlConnectionString, cpuTable, discTable, sampleInterval);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else {
                                if (isLastFile) {
                                    if (!doNotloadOtherTrend) {
                                        string cpuTable = UWSSerialNumber + "_CPU_" + dataStartDate.Year + "_" + dataStartDate.Month + "_" + dataStartDate.Day;
                                        string discTable = UWSSerialNumber + "_DISC_" + dataStartDate.Year + "_" + dataStartDate.Month + "_" + dataStartDate.Day;
                                        log.Info("Calling DISK Browser Thread");
                                        
                                        var diskBrowserLoader = new DiskBrowserLoader();
										var handle = new EventWaitHandle(false, EventResetMode.ManualReset);
										var populateDISCBrowser = new Thread(() => {
											diskBrowserLoader.LoadDiskBrowserData(UWSSerialNumber, newConnectionString, mySqlConnectionString, cpuTable, discTable, sampleInterval);
											handle.Set();
										}) {
                                            IsBackground = true
                                        };
										loadJobThread.Add(handle);
										populateDISCBrowser.Start();
                                    }
                                }
                                else if (!isProcessDirectlySystem) {
                                    //TODO: Check if we have Process Data.
                                    var loadedEntities = currentTable.GetEntitiesFor(startTimeLCT, stopTimeLCT, sampleInterval);
                                    if (loadedEntities.Contains(1) && loadedEntities.Contains(2) && loadedEntities.Contains(7)) {
                                        if (sp.TotalDays < 1) {
                                            log.Info("Calling DISK Browser Thread");
                                            
                                            string cpuTable = UWSSerialNumber + "_CPU_" + dataStartDate.Year + "_" + dataStartDate.Month + "_" + dataStartDate.Day;
                                            string discTable = UWSSerialNumber + "_DISC_" + dataStartDate.Year + "_" + dataStartDate.Month + "_" + dataStartDate.Day;
                                            var diskBrowserLoader = new DiskBrowserLoader();
											var handle = new EventWaitHandle(false, EventResetMode.ManualReset);
											var populateDISCBrowser = new Thread(() => {
												diskBrowserLoader.LoadDiskBrowserData(UWSSerialNumber, newConnectionString, mySqlConnectionString, cpuTable, discTable, sampleInterval);
												handle.Set();
											}) {
                                                IsBackground = true
                                            };
											loadJobThread.Add(handle);
											populateDISCBrowser.Start();
                                        }
                                        else {
                                            for (DateTime dtStart = startTimeLCT; dtStart.Date <= stopTimeLCT.Date; dtStart = dtStart.AddDays(1)) {
                                                string cpuTable = UWSSerialNumber + "_CPU_" + dtStart.Year + "_" + dtStart.Month + "_" + dtStart.Day;
                                                string discTable = UWSSerialNumber + "_DISC_" + dtStart.Year + "_" + dtStart.Month + "_" + dtStart.Day;
                                                log.InfoFormat("Calling DISK Browser: {0}",dtStart);
                                                
                                                var diskBrowserLoader = new DiskBrowserLoader();
                                                diskBrowserLoader.LoadDiskBrowserData(UWSSerialNumber, newConnectionString, mySqlConnectionString, cpuTable, discTable, sampleInterval);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex) {
                            log.ErrorFormat("disk browser Error: {0}",ex.Message);
                            
                        }

                        #endregion
                    }
                    else {
                        log.Info("DISK OPEN Entity, Skip DISC Browser Load");
                        
                    }

                    #region Load FileTrend and populate SCM.

                    log.Info("******  Load FileTrend and populate SCM  *******");
                    
                    string saveLocation = ConnectionString.SystemLocation + UWSSerialNumber;

                    if (fileEntity.Length > 0 && !isProcessDirectlySystem) {
                        #region File Trend

                        string cpuEntity = fileEntity.Replace("FILE", "CPU");
                        log.InfoFormat("fileEntity: {0}", fileEntity);
                        

                        try {
                            log.Info("Start File Trend Load to MySQL");
                            

                            log.InfoFormat("mySqlConnectionString: {0}",DiskLoader.RemovePassword(mySqlConnectionString));


                            if (mySqlConnectionString.Length.Equals(0)) {
                                //Create Database and update the table.
                                var mySql = new MySQLDataBrowser.Model.FileTrendData(new DataContext(ConnectionString.ConnectionStringDB));

                                mySqlConnectionString = mySql.CreateDatabaseFor(ConnectionString.DatabasePrefix + UWSSerialNumber);
                                databaseMappingService.UpdateMySQLConnectionStringFor(UWSSerialNumber, mySqlConnectionString);
                            }

                            if(sp.TotalDays < 1) {
                                var fileTrendData = new MySQLDataBrowser.Model.FileTrendData(dc);
                                var newFileTableName = fileEntity.Replace("FILE", "FILETREND");
                                var fileTableExists = fileTrendData.CheckTableNameFor(newFileTableName);

                                log.InfoFormat("tableExists: {0}",fileTableExists);
                                

                                if (isProcessDirectlySystem) {
                                    var entitiCheck = new Entities(mySqlConnectionString);
                                    string cpuTable = UWSSerialNumber + "_CPU_" + dataStartDate.Year + "_" + dataStartDate.Month + "_" + dataStartDate.Day;
                                    string fileTable = UWSSerialNumber + "_FILE_" + dataStartDate.Year + "_" + dataStartDate.Month + "_" + dataStartDate.Day;

                                    int cpuCount = entitiCheck.GetCPUCount("CPUEntity", cpuTable, startTimeLCT, stopTimeLCT);
                                    int fileCount = entitiCheck.GetOpenerCPUCount(fileTable, startTimeLCT, stopTimeLCT);

                                    log.InfoFormat("cpuCount - VISA: {0}",cpuCount);
                                    log.InfoFormat("fileCount - VISA: {0}",fileCount);
                                    

                                    if (cpuCount.Equals(fileCount)) {
                                        bool fileTrendDuplicate = false;
                                        if (fileTableExists) {
                                            log.InfoFormat("Check for Duplicate Data Table Name - VISA: {0}",fileEntity);
                                            
                                            fileTrendDuplicate = fileTrendData.CheckDuplicateFor(newFileTableName, startTimeLCT);

                                            log.InfoFormat("fileTrendDuplicate - VISA: {0}",fileTrendDuplicate);
                                            
                                        }

                                        if (!fileTrendDuplicate) {
                                            var fileTrend = new MySQLDataBrowser.Model.FileTrend(cpuEntity, fileEntity, newConnectionString, sampleInterval, 
                                                startTimeLCT, stopTimeLCT, saveLocation);
                                            var fileTrendThread = new Thread(() => fileTrend.PopulateFileTrend()) { IsBackground = true };
                                            fileTrendThread.Start();
                                        }
                                    }
                                }
                                else {
                                    bool fileTrendDuplicate = false;
                                    if (fileTableExists) {
                                        log.InfoFormat("Check for Duplicate Data Table Name: {0}",fileEntity);
                                        
                                        fileTrendDuplicate = fileTrendData.CheckDuplicateFor(newFileTableName, startTimeLCT);

                                        log.InfoFormat("fileTrendDuplicate: {0}",fileTrendDuplicate);
                                        
                                    }
                                    if (!fileTrendDuplicate) {
                                        var handle = new EventWaitHandle(false, EventResetMode.ManualReset);
                                        var fileTrend = new MySQLDataBrowser.Model.FileTrend(cpuEntity, fileEntity, newConnectionString, sampleInterval, 
                                            startTimeLCT, stopTimeLCT, saveLocation);
                                        var fileTrendThread = new Thread(() => {
                                            fileTrend.PopulateFileTrend();
                                            handle.Set();
                                        }) { IsBackground = true };
                                        loadJobThread.Add(handle);
                                        fileTrendThread.Start();
                                    }
                                }
                            } else {
                                //multiday
                                for (DateTime dtStart = startTimeLCT; dtStart.Date <= stopTimeLCT.Date; dtStart = dtStart.AddDays(1)) {
                                    string tempCpuEntity = cpuEntity.Replace("_" + startTimeLCT.Year + "_" + startTimeLCT.Month + "_" + startTimeLCT.Day
                                            , "_" + dtStart.Year + "_" + dtStart.Month + "_" + dtStart.Day);
                                    string tempFileEntity = fileEntity.Replace("_" + startTimeLCT.Year + "_" + startTimeLCT.Month + "_" + startTimeLCT.Day
                                            , "_" + dtStart.Year + "_" + dtStart.Month + "_" + dtStart.Day);
                                    //Check if File Trend Table is exists.
                                    var fileTrendData = new MySQLDataBrowser.Model.FileTrendData(dc);
                                    var newFileTableName = tempFileEntity.Replace("FILE", "FILETREND");
                                    var fileTableExists = fileTrendData.CheckTableNameFor(newFileTableName);
                                    log.InfoFormat("tableExists: {0}",fileTableExists);
                                    

                                    bool fileTrendDuplicate = false;
                                    if (fileTableExists) {
                                        log.InfoFormat("Check for Duplicate Data Table Name: {0}",fileEntity);
                                        
                                        fileTrendDuplicate = fileTrendData.CheckDuplicateFor(newFileTableName, startTimeLCT);

                                        log.InfoFormat("fileTrendDuplicate: {0}",fileTrendDuplicate);
                                        
                                    }
                                    if (!fileTrendDuplicate) {
                                        var handle = new EventWaitHandle(false, EventResetMode.ManualReset);
                                        var fileTrend = new MySQLDataBrowser.Model.FileTrend(tempCpuEntity, tempFileEntity, newConnectionString, sampleInterval,
                                            startTimeLCT, stopTimeLCT, saveLocation);
                                        var fileTrendThread = new Thread(() => {
                                            fileTrend.PopulateFileTrend();
                                            handle.Set();
                                        }) { IsBackground = true };
                                        loadJobThread.Add(handle);
                                        fileTrendThread.Start();
                                    }
                                }
                            }                     
                        }
                        catch (Exception ex) {
                            log.Error("*******************************************************");
                            log.ErrorFormat("MySql Error: {0}",ex.Message);
                        }

                        #endregion

                        #region SCM
                        log.Info("*******************************************************");
                        log.Info("Calling SCM Load");
                        log.InfoFormat("UWSSerialNumber: {0}, startTimeLCT: {1}, stopTimeLCT: {2}, " +
                            "fileEntity: {3}, sampleInterval: {4}, ConnectionString.ConnectionStringDB: {5}, databaseName: {6}",
                            UWSSerialNumber, startTimeLCT, stopTimeLCT, fileEntity, sampleInterval, 
                            DiskLoader.RemovePassword(ConnectionString.ConnectionStringDB), databaseName);

                        var scmLoad = new SimpleCapacityModel(mySqlConnectionString);
                        DateTime lct = startTimeLCT;
                        DateTime timeLct = stopTimeLCT;
						var handler = new EventWaitHandle(false, EventResetMode.ManualReset);
						var scmThread = new Thread(() => {
							scmLoad.PopulateSimpleCapacityModelData(UWSSerialNumber, lct, timeLct, fileEntity, sampleInterval, ConnectionString.ConnectionStringDB, databaseName, saveLocation, ConnectionString.SystemLocation);
							handler.Set();
						}) { IsBackground = true };
						loadJobThread.Add(handler);
						scmThread.Start();

                        #endregion
                    }
                    else {
                        try {
                            //Check if CPU is there.
                            if (isProcessDirectlySystem && fileEntity.Length > 0) {

                                var okayToLoad = CheckVisaFileLoad(log, newConnectionString, startTimeLCT.Date);
                                log.InfoFormat("File Okay to Load: {0}",okayToLoad);
                                if (okayToLoad.Length > 0) {
                                    var formatedStartTime = Convert.ToDateTime(okayToLoad.Split('|')[0]);
                                    var formatedStopTime = Convert.ToDateTime(okayToLoad.Split('|')[1]);

                                    #region File Trend

                                    string cpuTable = UWSSerialNumber + "_CPU_" + formatedStartTime.Year + "_" + formatedStartTime.Month + "_" + formatedStartTime.Day;
                                    string fileTable = UWSSerialNumber + "_FILE_" + formatedStartTime.Year + "_" + formatedStartTime.Month + "_" + formatedStartTime.Day;
                                    log.InfoFormat("VISA - File Trend cpuTable: {0}",cpuTable);
                                    log.InfoFormat("VISA - File Trend fileTable: {0}",fileTable);
                                    

                                    var fileTrendData = new MySQLDataBrowser.Model.FileTrendData(dc);
                                    var newFileTableName = fileTable.Replace("FILE", "FILETREND");
                                    log.InfoFormat("VISA - File Trend File Trend Table Name: {0}",newFileTableName);
                                    
                                    var fileTableExists = fileTrendData.CheckTableNameFor(newFileTableName);

                                    bool fileTrendDuplicate = false;
                                    if (fileTableExists) {
                                        log.InfoFormat("Check for Duplicate Data Table Name - VISA: {0}",fileEntity);
                                        
                                        fileTrendDuplicate = fileTrendData.CheckDuplicateFor(newFileTableName, formatedStartTime);

                                        log.InfoFormat("fileTrendDuplicate - VISA: {0}",fileTrendDuplicate);
                                        
                                    }

                                    if (!fileTrendDuplicate) {
										var handle = new EventWaitHandle(false, EventResetMode.ManualReset);
										var fileTrend = new MySQLDataBrowser.Model.FileTrend(cpuTable, fileTable, newConnectionString, sampleInterval, formatedStartTime, 
                                            formatedStopTime, saveLocation);
                                        var fileTrendThread = new Thread(() => {
											fileTrend.PopulateFileTrend();
											handle.Set();
										}) { IsBackground = true };
										loadJobThread.Add(handle);
										fileTrendThread.Start();
                                    }

                                    #endregion

                                    #region SCM
                                    log.Info("*******************************************************");
                                    log.Info("Calling SCM Load");
                                    log.InfoFormat("UWSSerialNumber: {0}, formatedStartTime: {1}, formatedStopTime: {2}, " +
                                        "fileEntity: {3}, sampleInterval: {4}, ConnectionString.ConnectionStringDB: {5}, databaseName: {6}",
                                        UWSSerialNumber, formatedStartTime, formatedStopTime, fileEntity, sampleInterval,
                                        DiskLoader.RemovePassword(ConnectionString.ConnectionStringDB), databaseName);

                                    var scmLoad = new SimpleCapacityModel(mySqlConnectionString);
                                    DateTime lct = formatedStartTime;
                                    DateTime timeLct = formatedStopTime;
									var handler = new EventWaitHandle(false, EventResetMode.ManualReset);
									var scmThread = new Thread(() => {
										scmLoad.PopulateSimpleCapacityModelData(UWSSerialNumber, lct, timeLct, fileEntity, sampleInterval, ConnectionString.ConnectionStringDB, databaseName, saveLocation, ConnectionString.SystemLocation);
										handler.Set();
									}) {
                                        IsBackground = true
                                    };
									loadJobThread.Add(handler);
									scmThread.Start();

                                    #endregion
                                }
                            }
                        }
                        catch (Exception ex) {
                            log.ErrorFormat("******  File Trend CPU FILE Check Error: {0}",ex.Message);
                            
                        }
                    }

                    log.Info("******  Load FileTrend and populate SCM completed *******");

                    #endregion

                    #region Load Trend

                    System.Diagnostics.Process currentProc = System.Diagnostics.Process.GetCurrentProcess();
                    log.Info("******  Load Trend  *****");
                    log.InfoFormat("Before Load Trend Memory: {0}",currentProc.PrivateMemorySize64);

                    var loadTrend = true;
                    try {

                        if (checkTrend) {
                            #region Check Entity
                            if (!(entityList.ContainsKey("CPU") && entityList.ContainsKey("DISK") && entityList.ContainsKey("PROCESS"))) {
                                log.Info("Check if CPU, DISK, & PROCESS are in the database.");
                                
                                var loadedEntities = currentTable.GetEntitiesFor(startTimeLCT, stopTimeLCT, sampleInterval);

                                foreach (var entity in loadedEntities) {
                                    log.InfoFormat("Loaded Entity: {0}",entity);
                                    
                                }

                                if (loadedEntities.Contains(1) && loadedEntities.Contains(2) && loadedEntities.Contains(7)) {
                                    //Check if trend data has been loaded.
                                    var dailyTrend = new DailySysUnratedService(newConnectionString);
                                    var trendValue = dailyTrend.CheckHourlyDataFor(UWSSerialNumber, startTimeLCT.Date, startTimeLCT.Hour);

                                    if (!trendValue.Equals(0))
                                        loadTrend = false;
                                    else {
                                        string buildTableName = "";
                                        foreach (var i in loadedEntities) {
                                            //Update the entityList.
                                            if (i == 1) {
                                                if (!entityList.ContainsKey("CPU")) {
                                                    buildTableName = UWSSerialNumber + "_CPU_" + dataStartDate.Year + "_" + dataStartDate.Month + "_" + dataStartDate.Day;
                                                    entityList.Add("CPU", "[" + buildTableName + "]");
                                                }
                                            }
                                            else if (i == 2) {
                                                if (!entityList.ContainsKey("DISK")) {
                                                    buildTableName = UWSSerialNumber + "_DISC_" + dataStartDate.Year + "_" + dataStartDate.Month + "_" + dataStartDate.Day;
                                                    entityList.Add("DISK", "[" + buildTableName + "]");
                                                }
                                            }
                                            else if (i == 5) {
                                                if (!entityList.ContainsKey("FILE")) {
                                                    buildTableName = UWSSerialNumber + "_FILE_" + dataStartDate.Year + "_" + dataStartDate.Month + "_" + dataStartDate.Day;
                                                    entityList.Add("FILE", "[" + buildTableName + "]");
                                                }
                                            }
                                            else if (i == 7) {
                                                if (!entityList.ContainsKey("PROCESS")) {
                                                    buildTableName = UWSSerialNumber + "_PROCESS_" + dataStartDate.Year + "_" + dataStartDate.Month + "_" + dataStartDate.Day;
                                                    entityList.Add("PROCESS", "[" + buildTableName + "]");
                                                }
                                            }
                                            else if (i == 11) {
                                                if (!entityList.ContainsKey("TMF")) {
                                                    buildTableName = UWSSerialNumber + "_TMF_" + dataStartDate.Year + "_" + dataStartDate.Month + "_" + dataStartDate.Day;
                                                    entityList.Add("TMF", "[" + buildTableName + "]");
                                                }
                                            }
                                        }
                                    }
                                }
                                else {
                                    log.Info("***Don't have all the Entities to load Trend");
                                    
                                    loadTrend = false;
                                }
                            }

                            //Check for VISA.
                            if (isProcessDirectlySystem && isLastFile) {
                                loadTrend = true;
                                //Build entityList
                                if (!entityList.ContainsKey("CPU")) {
                                    var buildTableName = UWSSerialNumber + "_CPU_" + dataStartDate.Year + "_" + dataStartDate.Month + "_" + dataStartDate.Day;
                                    entityList.Add("CPU", "[" + buildTableName + "]");
                                }
                                if (!entityList.ContainsKey("DISK")) {
                                    var buildTableName = UWSSerialNumber + "_DISC_" + dataStartDate.Year + "_" + dataStartDate.Month + "_" + dataStartDate.Day;
                                    entityList.Add("DISK", "[" + buildTableName + "]");
                                }
                                if (!entityList.ContainsKey("PROCESS")) {
                                    var buildTableName = UWSSerialNumber + "_PROCESS_" + dataStartDate.Year + "_" + dataStartDate.Month + "_" + dataStartDate.Day;
                                    entityList.Add("PROCESS", "[" + buildTableName + "]");
                                }
                                if (!entityList.ContainsKey("FILE")) {
                                    var buildTableName = UWSSerialNumber + "_FILE_" + dataStartDate.Year + "_" + dataStartDate.Month + "_" + dataStartDate.Day;
                                    entityList.Add("FILE", "[" + buildTableName + "]");
                                }
                                if (!entityList.ContainsKey("TMF")) {
                                    var buildTableName = UWSSerialNumber + "_TMF_" + dataStartDate.Year + "_" + dataStartDate.Month + "_" + dataStartDate.Day;
                                    entityList.Add("TMF", "[" + buildTableName + "]");
                                }
                            }

                            #endregion

                            if (loadTrend) {
                                if (sp.TotalDays < 1) {
                                    #region Single Day

                                    var newEntityList = new Dictionary<string, string>();
                                    foreach (var i in entityList) {
                                        string tempTableName = i.Value;
                                        string tempKey = i.Key;
                                        tempTableName = tempTableName.Replace("_" + startTimeLCT.Year + "_" + startTimeLCT.Month + "_" + startTimeLCT.Day
                                            , "_" + startTimeLCT.Year + "_" + startTimeLCT.Month + "_" + startTimeLCT.Day);

                                        newEntityList.Add(tempKey, tempTableName);
                                    }

                                    try {
                                        //New Trend Loader.
                                        var trendLoader = new RemoteAnalystTrendLoader.TrendLoad(new PmcContext(), dc,
                                            UWSSerialNumber, startTimeLCT, stopTimeLCT, entityList, sampleInterval, log, ConnectionString.SystemLocation);
                                        var trendErrors = trendLoader.LoadTrend();
                                        if(trendErrors.Count > 0) {
                                            //Added logic for RA-1207 to send email if there
                                            //are errors loading the trend data.
                                            SendLoadTrendErrors(trendErrors,
                                                                UWSSerialNumber, 
                                                                startTimeLCT, 
                                                                stopTimeLCT, 
                                                                entityList, 
                                                                sampleInterval
                                                                );
                                        }

                                    }
                                    catch (Exception ex) {
                                        log.ErrorFormat("Trend Error: {0}",ex.Message);
                                        
                                    }

                                    try {
                                        if (isProcessDirectlySystem) {
                                            //Check to see if you have all the cpu, disc, and process data.
                                            var okayToLoad = CheckVisaIntervalTrendLoad(log, newConnectionString, startTimeLCT.Date);
                                            if (okayToLoad.Length > 0) {
                                                //TODO: Check if any thread is loading the trend.
                                                var visaTrendLoad = new VisaTrendLoaderService(ConnectionString.ConnectionStringDB);
                                                var isTrendLoad = visaTrendLoad.CheckEntry(UWSSerialNumber, startTimeLCT.Date);

                                                if (!isTrendLoad) {
                                                    visaTrendLoad.InsertEntry(UWSSerialNumber, startTimeLCT.Date);

                                                    var formatedStartTime = Convert.ToDateTime(okayToLoad.Split('|')[0]);
                                                    var formatedStopTime = Convert.ToDateTime(okayToLoad.Split('|')[1]);
                                                    log.Info("Calling Interval Trend");
                                                    var cpuTableName = UWSSerialNumber + "_CPU_" + dataStartDate.Year + "_" + dataStartDate.Month + "_" + dataStartDate.Day;
                                                    var visaInterval = currentTable.GetIntervalFor(cpuTableName);

                                                    //For any reason, interval is 0, change it to 60 second (Visa's default interval)
                                                    if (visaInterval == 0)
                                                        visaInterval = 60;

                                                    log.InfoFormat("visaInterval: {0}",visaInterval);
                                                    

                                                    for (var start = formatedStartTime; start < formatedStopTime; start = start.AddHours(1)) {
                                                        log.InfoFormat("start: {0}",start);
                                                        log.InfoFormat("Stop: {0}",start.AddHours(1));
                                                        
                                                        var intervalTrendLoad = new IntervalTrendLoad(UWSSerialNumber, start, start.AddHours(1), entityList, visaInterval, log);
                                                        intervalTrendLoad.LoadTrend();
                                                    }
                                                }
                                            }
                                        }
                                        else {
                                            var intervalTrendLoad = new IntervalTrendLoad(UWSSerialNumber, startTimeLCT, stopTimeLCT, entityList, sampleInterval, log);
                                            intervalTrendLoad.LoadTrend();
                                        }
                                    }
                                    catch (Exception ex) {
                                        log.ErrorFormat("Interval Trend Error: {0}",ex.Message);
                                        log.ErrorFormat(ex.StackTrace);
                                        
                                    }

                                    try {
                                        log.InfoFormat("Calling InsertException at: {0}", DateTime.Now);
                                        

                                        //Forecast Load
                                        var exceptionLoad = new ExceptionLoad(ConnectionString.ConnectionStringDB, ConnectionString.ConnectionStringTrend, ConnectionString.ServerPath);
                                        exceptionLoad.InsertException(UWSSerialNumber, startTimeLCT, stopTimeLCT);
                                    }
                                    catch (Exception ex) {
                                        log.ErrorFormat("InsertException Error at {0}, {1}", DateTime.Now, ex.Message);                                        
                                    }

                                    #endregion
                                }
                                else {
                                    #region Multi Day

                                    for (DateTime dtStart = startTimeLCT; dtStart.Date <= stopTimeLCT.Date; dtStart = dtStart.AddDays(1)) {
                                        var newEntityList = new Dictionary<string, string>();
                                        foreach (var i in entityList) {
                                            string tempTableName = i.Value;
                                            string tempKey = i.Key;
                                            tempTableName = tempTableName.Replace("_" + startTimeLCT.Year + "_" + startTimeLCT.Month + "_" + startTimeLCT.Day
                                                , "_" + dtStart.Year + "_" + dtStart.Month + "_" + dtStart.Day);

                                            newEntityList.Add(tempKey, tempTableName);
                                        }

                                        try {
                                            var trendLoader = new RemoteAnalystTrendLoader.TrendLoad(new PmcContext(), dc, UWSSerialNumber, dtStart.Date, dtStart.Date.AddDays(1), newEntityList, sampleInterval, log, ConnectionString.SystemLocation);
                                            trendLoader.LoadTrend();
                                        }
                                        catch (Exception ex) {
                                            log.ErrorFormat("Trend Error: {0}", ex.Message);
                                            
                                        }
                                        try {
                                            var intervalTrendLoad = new IntervalTrendLoad(UWSSerialNumber, dtStart.Date, dtStart.Date.AddDays(1), newEntityList, sampleInterval, log);
                                            intervalTrendLoad.LoadTrend();
                                        }
                                        catch (Exception ex) {
                                            log.ErrorFormat("Interval Trend Error: {0}", ex.Message);
                                            log.ErrorFormat(ex.StackTrace);
                                            
                                        }

                                        try {
                                            log.InfoFormat("Calling InsertException at: {0}", DateTime.Now);
                                            

                                            //Forecast Load
                                            var exceptionLoad = new ExceptionLoad(ConnectionString.ConnectionStringDB, ConnectionString.ConnectionStringTrend, ConnectionString.ServerPath);
                                            exceptionLoad.InsertException(UWSSerialNumber, dtStart.Date, dtStart.Date.AddDays(1));
                                        }
                                        catch (Exception ex) {
                                            log.ErrorFormat("InsertException Error: {0}", ex.Message);
                                            
                                        }
                                    }

                                    #endregion
                                }
                            }
                        }
                        else {
                            log.Info("DISK OPEN Entity, Skip Trend");
                            
                        }
                    }
                    catch (Exception ex) {
                        log.ErrorFormat("Trend Error: {0}", ex.Message);
                        
                    }

                    currentProc.Refresh();
                    log.InfoFormat("After Load Trend Memory: {0}", currentProc.PrivateMemorySize64);
                    log.InfoFormat("******  Load Trend completed at {0} *****", DateTime.Now);


                    #endregion

                    #region Call SNS - LAMBDA
#if (RDSMove)
                    log.InfoFormat("******  Call Process Watch LAMBDA at {0} *****", DateTime.Now);
#else
                    try
                    {
                        //if (!string.IsNullOrEmpty(ConnectionString.SNSProcessWatch)) {
                        var processAlerts = new MySQLAlert.Model.ProcessWatchAlert(dc);
                        var alertCount = processAlerts.GetAlertIds(UWSSerialNumber).Count;
                        log.InfoFormat("alertCount at {0}:{1}", DateTime.Now, alertCount);

                        if (alertCount > 0) {
                            //log.InfoFormat("Calling LAMBDA: " + ConnectionString.SNSProcessWatch);
                            //
                            //var procssWatchView = new ProcessWatchView();

                            foreach (var tableName in processTableList)
                            {
                                log.InfoFormat("UWSSerialNumber: {0}, startTimeLCT: {1}, stopTimeLCT: {2}, " +
                                    "fileEntity: {3}, sampleInterval: {4}",
                                    UWSSerialNumber, startTimeLCT, stopTimeLCT, fileEntity, sampleInterval);

								var handle = new EventWaitHandle(false, EventResetMode.ManualReset);
								var processWatch = new ProcessWatchAlertService(ConnectionString.ConnectionStringDB,
                                                            mySqlConnectionString, UWSSerialNumber, tableName, startTimeLCT, stopTimeLCT, sampleInterval,
                                                            ConnectionString.EmailServer, ConnectionString.EmailPort, ConnectionString.EmailUser,
                                                            ConnectionString.EmailPassword, ConnectionString.EmailAuthentication, ConnectionString.AdvisorEmail,
                                                            ConnectionString.EmailIsSSL);

                                var processWatchThread = new Thread(() => {
									processWatch.GetProcessWatchFor();
									handle.Set();
								}) {
                                    IsBackground = true
                                };
								loadJobThread.Add(handle);
								processWatchThread.Start();
                            }
                        }
                        //}
                    }
                    catch (Exception ex) {
                        log.ErrorFormat("Process Watch Error: {0}", ex.Message);
                        
                    }
#endif

                    #endregion

                    #region Load Top 20 Process Busy/Queue Static.
                    if (checkTrend && loadTrend) { 
                        if (isProcessDirectlySystem)
                        {
                            log.InfoFormat("Skipping generating Top 20 processes for Visa since it is used only in sending out dailies: {0}", DateTime.Now);
                        }
                        else { 
                            log.InfoFormat("Calling Load Top 20 Process Busy/Queue Static at {0}", DateTime.Now);                            
                            var top20Processes = new Top20Processes();
					        var handleNew = new EventWaitHandle(false, EventResetMode.ManualReset);
					        var top20Thread = new Thread(() => {
						        top20Processes.PopulateTop20Processes(newConnectionString, UWSSerialNumber, startTimeLCT, stopTimeLCT, sampleInterval, ConnectionString.DatabasePrefix, ConnectionString.SystemLocation, log);
						        handleNew.Set();
					        });
					        loadJobThread.Add(handleNew);
					        top20Thread.IsBackground = true;
                            top20Thread.Start();
                        }
                    }
                    else
                    {
                        log.InfoFormat("Skipping Load Top 20 Process Busy/Queue Static checkTrend: {0}", checkTrend + " loadTrend: {0}", loadTrend);
                        
                    }
                    #endregion

                    #region Update LoadingInfo.

                    log.InfoFormat("******  Update LoadingInfo at {0} ********", DateTime.Now);
                    try {
                        var loadingInfo = new LoadingInfoService(_connectionStr);
                        if (systemName.Length > 8) systemName = systemName.Substring(0, 8);

                        if (loadJobThread.Count > 0)
                        {
                            if (LoadJobSynchronizer.SynchromizeLoadJob(loadJobThread.ToArray()))
                            {
                                loadingInfo.UpdateFor(uwsID, systemName, startTimeLCT, stopTimeLCT, 4);
                            }
                        }
                        else
                        {
                            loadingInfo.UpdateFor(uwsID, systemName, startTimeLCT, stopTimeLCT, 4);
                        }
                    }
                    catch (Exception ex) {
                        log.ErrorFormat("LoadingInfo Error at {0}: {1}, {2}", DateTime.Now, ex.Message, ex.StackTrace);
                    }
                    log.InfoFormat("******  Update LoadingInfo completed at {0} ********", DateTime.Now);

                    //Update Message.
                    if (!ntsId.Equals(0)) {
                        var uploadService = new UploadService(_connectionStr);
                        uploadService.UploadCollectionStartTimeFor(ntsId, startTimeLCT);
                        uploadService.UploadCollectionToTimeFor(ntsId, stopTimeLCT);
                    }
#endregion
                }
                catch (Exception ex) {
                    log.ErrorFormat("After SPAM Load Error: {0}", ex.Message);
                    
                }

                success = true;
            }
            catch (Exception ex) {
                log.ErrorFormat("EntityID: " + entityID + ". " + ex.Message);
                
                success = false;

                /*//Check to see if I can update LoadingInfo.
                if (startTime != 0 && stopTime != 0) {
                    try {
                        //Convert starttime and endtime.
                        var julianTime = new ConvertJulianTime();
                        int lctTimeStamp = julianTime.JulianTimeStampToOBDTimeStamp(startTime);
                        startTimeLCT = julianTime.OBDTimeStampToDBDate(lctTimeStamp);

                        lctTimeStamp = julianTime.JulianTimeStampToOBDTimeStamp(stopTime);
                        stopTimeLCT = julianTime.OBDTimeStampToDBDate(lctTimeStamp);

                        //Round up the seconds.
                        TimeSpan span1 = stopTimeLCT - startTimeLCT;
                        double seconds1 = span1.TotalSeconds;

                        //Get remained seconds.
                        double remainSeconds1 = seconds1 % sampleInterval;

                        stopTimeLCT = stopTimeLCT.AddSeconds(-remainSeconds1);

                        var loadingInfo = new LoadingInfoService(_connectionStr);
                        systemName = systemName.Substring(0, 8);

                        loadingInfo.UpdateFor(uwsID, systemName, startTimeLCT, stopTimeLCT, 4);
                    }
                    catch (Exception ex1) {
                        log.ErrorFormat("LoadingInfo Error1: " + ex1.Message);
                        
                    }
                }*/

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
                if (!intervalMatch) {
                    UpdateLogEntryAndStatus(UwsUwsFileLocation.Trim(), "Mismatch of Time Interval", "Failed");
                }
                if (File.Exists(uwsPath))
                    File.Delete(uwsPath);

                GC.Collect();
            }


            return success;
        }

        /// <summary>
        /// Load the data as a DISCOPEN load.
        /// </summary>
        /// <param name="uwsPath"> Full path of the UWS data file.</param>
        /// <param name="log">log4net instance.</param>
        /// <param name="newConnectionString">Connection string of the system database.</param>
        /// <param name="selectedStartTime">The start time that user selected.</param>
        /// <param name="selectedStopTime"> The stop time that user selected.</param>
        /// <param name="uwsVersion"> Enum class that tell version of UWS file.</param>
        /// <returns> Return a bool value suggests whether the load is successful or not.</returns>
        public bool CreateMultiDayDataSetDISCOPEN(string uwsPath, ILog log, string newConnectionString, DateTime selectedStartTime, DateTime selectedStopTime, UWS.Types uwsVersion) {
            log.Info("    -Populateing SPAM Database");
            

            bool success;
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

            var uniqueTableName = new List<string>();
            var tableNames = new List<string>();

            log.InfoFormat("newConnectionString: {0}", DiskLoader.RemovePassword(newConnectionString));
            try {
#region Load Data

                using (var stream = new FileStream(uwsPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    var dicInfo = new DirectoryInfo(stream.Name);
                    using (reader = new BinaryReader(stream)) {
                        //Loop thorugh the entity.
                        foreach (Indices t in index) {
                            //Load ONLY (3 - DISCOPEN, 8 - SQLPROC, 9 - SQLSTMT).
                            if (entityID == 0 || !(entityID == 3 || entityID == 8 || entityID == 9)) {
                                continue;
                            }

                            if (t.FName.Length != 0 && t.FRecords > 0) {
                                //Get eneityID.
                                var entity = new EntitiesService(newConnectionString);
                                entityID = entity.GetEntityIDFor(t.FName.Trim());

                                //Load ONLY DISCOPEN.
                                if (entityID == 0 || entityID != 3) {
                                    continue;
                                }

                                log.InfoFormat("EntityID: {0}, Started at {1}", t.FName.Trim(), DateTime.Now);
                                
                                DateTime beforeTime = DateTime.Now;

                                if (systemName.Length == 0) {
                                    systemName = t.FSysName;
                                }

                                //Interval.
                                if (sampleInterval == 0) {
                                    sampleInterval = t.FInterval;
                                }
                                string dbTableName;

                                DateTime dataStartDate;
                                DateTime dataStopDate;
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
                                    

                                    //change the table name to new table.
                                    if (dbTableName.Equals("ZmsBladeDataDictionary") || dbTableName.Equals("ZmsDataDictionary")) {
                                        var vProc = new VProcVersionService(ConnectionString.ConnectionStringDB);
                                        dbTableName = vProc.GetDataDictionaryFor(UwsCreatorVproc);
                                    }

                                    log.InfoFormat("      New Measure Type: {0}", dbTableName);
                                    
                                    dataStartDate = t.CollEntityStartTime;
                                    dataStopDate = t.CollEntityStoptTime;
                                }

                                log.InfoFormat("      SystemSerial: {0}", UWSSerialNumber);
                                

                                //Get Column type into the List.
                                var dictionary = new DataDictionaryService(newConnectionString);
                                IList<ColumnInfoView> columnInfo = dictionary.GetColumnsFor(entityID, dbTableName);
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
                                log.InfoFormat("      dataStartDate: {0}", dataStartDate);
                                log.InfoFormat("      dataStopDate: {0}", dataStopDate);
                                

                                var uwsDirectory = new UWSDirectoryService(_connectionStr);

                                //Load only the data within selected time range.
                                if (((selectedStartTime >= dataStartDate && selectedStartTime < dataStopDate) ||
                                     (selectedStopTime <= dataStopDate && selectedStopTime > dataStartDate)) ||
                                    dataStartDate >= selectedStartTime && dataStartDate < selectedStopTime) {
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

                                    log.InfoFormat("buildTableName: {0}", buildTableName);
                                    
                                    var myDataSet = new DataSet();
                                    var loadInfo = new UWSLoadInfoService(_connectionStr);

                                    List<LoadedTime> loadedTimes = loadInfo.GetLoadedTimeFor(UWSSerialNumber, dataStartDate, dataStopDate);

                                    bool duplicateLoad = false;
                                    //Check if selected range is already loaded.
                                    if (loadedTimes.Any(lt => selectedStartTime >= lt.LoadedStartTime && selectedStopTime <= lt.LoadedStopTime)) {
                                        duplicateLoad = true;
                                        log.Info("      *****Duplicated Data");
                                        
                                    }
                                    /*foreach (LoadedTime lt in loadedTimes) {
                                        if (selectedStartTime >= lt.LoadedStartTime && selectedStopTime <= lt.LoadedStopTime) {
                                    */
                                    if (duplicateLoad) {
                                        try {
                                            log.Info("Calling UpdateLoadingFor with following param (Line 2063):");
                                            log.InfoFormat("UWSSerialNumber: {0}", UWSSerialNumber);
                                            log.InfoFormat("dataStartDate: {0}", dataStartDate);
                                            log.InfoFormat("dataStopDate: {0}", dataStopDate);
                                            
                                            uwsDirectory.UpdateLoadingFor(UWSSerialNumber, dataStartDate, dataStopDate, 0); //0 is not loading.
                                            continue;
                                        }
                                        catch (Exception ex) {
                                            log.ErrorFormat("UpdateLoadingFor Error: {0}", ex.Message);
                                            
                                        }
                                    }

                                    try {
                                        log.Info("Calling UpdateLoadingFor with following param (Line 2078):");
                                        log.InfoFormat("UWSSerialNumber: {0}", UWSSerialNumber);
                                        log.InfoFormat("dataStartDate: {0}", dataStartDate);
                                        log.InfoFormat("dataStopDate: {0}", dataStopDate);
                                        
                                        uwsDirectory.UpdateLoadingFor(UWSSerialNumber, dataStartDate, dataStopDate, 1); //1 is loading.
                                    }
                                    catch (Exception ex) {
                                        log.ErrorFormat("UpdateLoadingFor Error: {0}", ex.Message);
                                        
                                    }
                                    try {
#region Create DataSet and insert data

                                        //Create DataSet with DataTable(s).
                                        myDataSet = CreateSPAMDataTableColumn(days, columnInfo, false);

                                        if (myDataSet.Tables.Count > 0) {
                                            //Create Data Table Column.
                                            //DataTable myDataTable = CreateDataTableColumn(columnInfo);

                                            //Test values for tableName From FromTimeStamp.
                                            var tableNameFromTimeStamp = new DateTime();

                                            /*int x = 0;
                                            if (dataStartDate >= selectedStartTime && dataStopDate <= selectedStopTime) {
                                                //Load everything.
                                            }
                                            else {
                                                UWSSearch search = new UWSSearch();
                                                UWSLookup lookup =  search.GetDataLocation(UWSPath, filePosition, index[i].FRecords, index[i].FReclen, selectedStartTime, dataStartDate, dataStopDate);
                                                //Get 
                                                x = lookup.RecordIndex;
                                                filePosition = lookup.RecordFilePosition;
                                            }*/

                                            //Loop through the records.
                                            for (int x = 0; x < t.FRecords; x++) {
#region Read each byte.

                                                reader.BaseStream.Seek(filePosition, SeekOrigin.Begin);
                                                byte[] indexBytes = reader.ReadBytes(recordLenth);
                                                long currentPosition = 0;

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
                                                                    throw new Exception(ex.Message);
                                                                }
                                                                break;
                                                            case "INT": //NOTE: VARIABLE COMING IN AS "SMALLINT" AND SAVED AS "INT"
                                                                try {
                                                                    tempShort[0] = indexBytes[currentPosition + 1];
                                                                    tempShort[1] = indexBytes[currentPosition];
                                                                    //myDataRow[column.ColumnName] = Convert.ToInt16(BitConverter.ToInt16(tempShort, 0));
                                                                    column.TestValue = BitConverter.ToUInt16(tempShort, 0).ToString();
                                                                }
                                                                catch (Exception ex) {
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
                                                                }
                                                                catch (Exception ex) {
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
                                                                    throw new Exception(ex.Message);
                                                                }
                                                                break;
                                                            case "BIT":
                                                                try {
                                                                    byte tempByte = indexBytes[currentPosition];
                                                                    //Convert to Binary.
                                                                    string binaryValue = Convert.ToString(tempByte, 2);

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
                                                                    throw new Exception(ex.Message);
                                                                }
                                                                break;
                                                            case "TINYINT":
                                                                try {
                                                                    //myDataRow[column.ColumnName] = indexBytes[currentPosition];
                                                                    column.TestValue = indexBytes[currentPosition].ToString();
                                                                }
                                                                catch (Exception ex) {
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
                                                                            var intervalList = new List<DateTime>();
                                                                            DateTime tempStartDate = dataStartDate;
                                                                            //Add start time.
                                                                            intervalList.Add(tempStartDate);

                                                                            while (tempStartDate < dataStopDate) {
                                                                                tempStartDate = tempStartDate.AddSeconds(t.FInterval);
                                                                                intervalList.Add(tempStartDate);
                                                                            }

                                                                            intervalList.Sort();
                                                                            //Delete the last list, because last collection is after the collection stop time.
                                                                            //intervalList.RemoveAt(intervalList.Count - 1);

                                                                            for (int z = 0; z < intervalList.Count; z++) {
                                                                                DateTime intervalTime = intervalList[z];

                                                                                if (intervalTime > dbDate) {
                                                                                    dbDate = intervalList[z - 1];
                                                                                    break;
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
                                                    var tempStartTime = new DateTime();

                                                    foreach (ColumnInfoView column in columnInfo) {
#region Switch

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
                                                                    tempStartTime = Convert.ToDateTime(column.TestValue);
                                                                    if (tempStartTime >= dataStopDate) {
                                                                        deleteRow = true;
                                                                    }
                                                                }
                                                                else if (column.ColumnName.ToUpper().Trim() == "TOTIMESTAMP") {
                                                                    //if (Convert.ToDateTime(column.TestValue) == oldDataStopDate) {
                                                                    //    column.TestValue = dataStopDate.ToString();
                                                                    //}//If ToTimestamp is greater than endtime, change the value to endtime.
                                                                    DateTime tempStopTime = Convert.ToDateTime(column.TestValue);
                                                                    if (tempStopTime > dataStopDate) {
                                                                        column.TestValue = dataStopDate.ToString();
                                                                    }
                                                                }

                                                                myDataRow[column.ColumnName] = Convert.ToDateTime(column.TestValue);
                                                                break;
                                                        }

#endregion
                                                    }
                                                    //Check if the time is within selected range.
                                                    if (!deleteRow) {
                                                        if (tempStartTime >= selectedStartTime && tempStartTime <= selectedStopTime) {
                                                            if (loadedTimes.Count > 0) {
                                                                foreach (LoadedTime dt in loadedTimes) {
                                                                    //I'm using > because if >= is used, it will insert duplicate data.
                                                                    if (tempStartTime > dt.LoadedStartTime && tempStartTime <= dt.LoadedStopTime) {
                                                                        deleteRow = true;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        else {
                                                            deleteRow = true;
                                                        }
                                                    }

                                                    if (myDataRow["FromTimestamp"].ToString().Equals(myDataRow["ToTimestamp"].ToString())) {
                                                        deleteRow = true;
                                                    }

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

                                                        //check to see if the row has more then BulkLoaderSize rows.
                                                        if (myDataSet.Tables[tableNameFromTimeStamp.ToString("yyyy/MMM/dd")].Rows.Count >
                                                            Constants.getInstance(ConnectionString.ConnectionStringDB).BulkLoaderSize) {
                                                            //Insert into the table.
                                                            var insertTables = new DataTableService(newConnectionString);

                                                            insertTables.InsertSPAMEntityDataFor(tempTableName, myDataSet.Tables[tableNameFromTimeStamp.ToString("yyyy/MMM/dd")], selectedStartTime, selectedStopTime, dicInfo.FullName);

                                                            //Clear the myDataTable.
                                                            myDataSet.Tables[tableNameFromTimeStamp.ToString("yyyy/MMM/dd")].Rows.Clear();
                                                        }
                                                    }
                                                }

                                                //Increase the start reading position.
                                                filePosition += recordLenth;

#endregion
                                            }

                                            //Insert into the database.
                                            var tables = new DataTableService(newConnectionString);
                                            foreach (MultiDays d in days) {
                                                if (!d.DontLoad) {
                                                    tables.InsertSPAMEntityDataFor(d.TableName, myDataSet.Tables[d.StartDate.ToString("yyyy/MMM/dd")], selectedStartTime, selectedStopTime, dicInfo.FullName);
                                                }
                                            }
                                            DateTime afterTime = DateTime.Now;
                                            TimeSpan timeSpan = afterTime - beforeTime;
                                            log.InfoFormat("    -(Line 3873)EntityID: {0} Total Time in Minutes: {1}", entityID, timeSpan.TotalMinutes);
                                        }

#endregion
                                    }
                                    catch (Exception ex) {
                                        log.ErrorFormat("EntityID: {0}.\n Message: {1}", entityID, ex.Message);
                                        throw new Exception(ex.Message);
                                    }
                                    finally {                                   
                                        columnInfo = null;
                                        myDataSet.Dispose();

                                        try {
                                            log.Info("Calling UpdateLoadingFor with following param (Line 2548):");
                                            log.InfoFormat("UWSSerialNumber: {0}", UWSSerialNumber);
                                            log.InfoFormat("dataStartDate: {0}", dataStartDate);
                                            log.InfoFormat("dataStopDate: {0}", dataStopDate);                                            
                                            uwsDirectory.UpdateLoadingFor(UWSSerialNumber, dataStartDate, dataStopDate, 0); //0 is not loading.

                                        }
                                        catch (Exception ex) {
                                            log.ErrorFormat("UpdateLoadingFor Error: {0}", ex.Message);
                                            
                                        }
                                        loadInfo.InsertDataFor(UWSSerialNumber, dataStartDate, dataStopDate, selectedStartTime, selectedStopTime);
                                    }
                                }
                                else {
                                    uwsDirectory.UpdateLoadingFor(UWSSerialNumber, dataStartDate, dataStopDate, 0); //0 is not loading.
                                    log.InfoFormat("No data within selected time range");
                                    
                                }
                            }
                        }
                    }
                }

#endregion

                success = true;
            }
            catch (Exception ex) {
                log.ErrorFormat("EntityID: " + entityID + ". " + ex.Message);
                
                success = false;
            }
            finally {
                //Delete Temp file.
                if (File.Exists(uwsPath)) {
                    File.Delete(uwsPath);
                }

                GC.Collect();
            }

            return success;
        }

        public bool CheckVisaTrendLoad(ILog log, string mySqlConnectionString, DateTime startTimeLCT, DateTime stopTimeLCT, DateTime dataStartDate, long sampleInterval) {
            var currentTable = new CurrentTableService(mySqlConnectionString);

            log.InfoFormat("Check on the database to see if you have all the entities & CPUs for Trend Load.");
            
            //Check on the database to see if you have all the entities & CPUs for Disk Browser Load.
            var loadedEntities = currentTable.GetEntitiesFor(startTimeLCT, stopTimeLCT, sampleInterval);

            bool okaytoLoad = false;
            if (loadedEntities.Contains(1) && loadedEntities.Contains(2) && loadedEntities.Contains(7)) {
                log.InfoFormat("Check Number of CPUs on all the tables.");
                

                var entitiCheck = new Entities(mySqlConnectionString);
                string cpuTable = UWSSerialNumber + "_CPU_" + dataStartDate.Year + "_" + dataStartDate.Month + "_" + dataStartDate.Day;
                string processTable = UWSSerialNumber + "_PROCESS_" + dataStartDate.Year + "_" + dataStartDate.Month + "_" + dataStartDate.Day;
                string discTable = UWSSerialNumber + "_DISC_" + dataStartDate.Year + "_" + dataStartDate.Month + "_" + dataStartDate.Day;

                int cpuCount = entitiCheck.GetCPUCount("CPUEntity", cpuTable, startTimeLCT, stopTimeLCT);
                int processCount = entitiCheck.GetCPUCount("ProcessEntity", processTable, startTimeLCT, stopTimeLCT);
                int discCount = entitiCheck.GetCPUCount("DailyDisk", discTable, startTimeLCT, stopTimeLCT);

                log.InfoFormat("cpuCount: {0}", cpuCount);
                log.ErrorFormat("processCount: {0}", processCount);
                log.ErrorFormat("discCount: {0}", discCount);
                

                if (cpuCount.Equals(processCount) && cpuCount.Equals(discCount)) {
                    okaytoLoad = true;

                    try {
                        //Do a hourly spot check.
                        for (var x = startTimeLCT; x < stopTimeLCT; x = x.AddMinutes(30)) {
                            log.InfoFormat("Checking time : {0} ~ {1}", x, x.AddMinutes(30));

                            var cpuExits = entitiCheck.CheckTime("CPUEntity", cpuTable, x, x.AddMinutes(30));
                            var processExits = entitiCheck.CheckTime("ProcessEntity", processTable, x, x.AddMinutes(30));
                            var discExits = entitiCheck.CheckTime("DailyDisk", discTable, x, x.AddMinutes(30));

                            log.InfoFormat("cpuExits : {0}", cpuExits);
                            log.InfoFormat("processExits : {0}", processExits);
                            log.InfoFormat("discExits : {0}", discExits);
                            

                            if (!cpuExits || !processExits || !discExits) {
                                log.Info("Returning False");
                                return false;
                            }
                        }
                    }
                    catch (Exception ex) {
                        log.ErrorFormat("CheckVisaTrendLoad Error: {0}", ex.Message);
                        
                        return false;
                    }
                }
            }

            return okaytoLoad;
        }

        public bool CheckVisaFileData(ILog log, string mySqlConnectionString, DateTime startTimeLCT, DateTime stopTimeLCT, string fileTableName) {
            try {
                var entitiCheck = new Entities(mySqlConnectionString);
                //Do a hourly spot check.
                for (var x = startTimeLCT; x < stopTimeLCT; x = x.AddMinutes(60))
                {
                    log.InfoFormat("Checking time : {0} ~ {1}", x, x.AddMinutes(60));
                    var fileExits = entitiCheck.CheckTime("FileEntity", fileTableName, x, x.AddMinutes(60));
                    log.InfoFormat("fileExits : {0}", fileExits);

                    if (!fileExits) {
                        log.InfoFormat("Returning False");
                        return false;
                    }
                }
            }
            catch (Exception ex) {
                log.ErrorFormat("CheckVisaTrendLoad Error: {0}", ex.Message);
                
                return false;
            }

            return true;
        }

        public string CheckVisaFileLoad(ILog log, string mySqlConnectionString, DateTime dataStartDate) {
            var tableTimestamp = new TableTimeStampService(mySqlConnectionString);

            log.Info("Check on the database to see if you have all the data for File Load.");

            //Check on the database to see if you have all the entities & CPUs for Disk Browser Load.
            var loadedData = tableTimestamp.GetLoadedFileDataFor(dataStartDate, dataStartDate.AddDays(1));

            var timeRange = "";
            //Get Table Names.
            var tableNames = loadedData.AsEnumerable().Select(x => x.Field<string>("TableName")).Distinct().ToList();

            if (tableNames.Count == 0)
                timeRange = "";
            else {
                foreach (var tableName in tableNames) {

                    log.InfoFormat("tableName: {0}", tableName);
                    

                    var times = loadedData.AsEnumerable().Where(x => x.Field<string>("TableName") == tableName).OrderBy(x => x.Field<DateTime>("Start")).ToList();
                    if (times.Count == 1) {
                        var fromTime = times.Select(x => x.Field<DateTime>("Start")).Single();
                        var toTime = times.Select(x => x.Field<DateTime>("End")).Single();

                        var timediff = toTime - fromTime;
                        if (timediff.TotalHours >= 8)
                            timeRange = fromTime + "|" + toTime;
                        else {
                            timeRange = "";
                            return timeRange;
                        }
                    }
                    else if (times.Count > 1) {
                        var previousEndTime = new DateTime();
                        for (var x = 0; x < times.Count; x++) {
                            if (x == 0) {
                                previousEndTime = Convert.ToDateTime(times[0]["End"]);
                            }
                            else {
                                var startTime = Convert.ToDateTime(times[x]["Start"]);
                                var endTime = Convert.ToDateTime(times[x]["End"]);

                                var compareSpan = (startTime - previousEndTime).TotalSeconds;

                                //if (startTime != previousEndTime) {
                                if (compareSpan > 60) { //If the time diff is greater than 60 seconds, it's not continues.
                                    timeRange = "";
                                    return timeRange;
                                }
                                else {
                                    previousEndTime = endTime;
                                }
                            }
                        }

                        var fromTime = times.Min(x => x.Field<DateTime>("Start"));
                        var toTime = times.Max(x => x.Field<DateTime>("End"));

                        var timediff = toTime - fromTime;
                        if (timediff.TotalHours >= 8)
                            timeRange = fromTime + "|" + toTime;
                        else {
                            timeRange = "";
                            return timeRange;
                        }
                    }
                    else {
                        timeRange = "";
                        return timeRange;
                    }

                    log.InfoFormat("okaytoLoad: {0}", timeRange);
                    
                }
            }
            return timeRange;
        }

        public string CheckVisaIntervalTrendLoad(ILog log, string mySqlConnectionString, DateTime dataStartDate) {
            var tableTimestamp = new TableTimeStampService(mySqlConnectionString);

            log.Info("Check on the database to see if you have all the data for Interval Trend Load.");
            
            //Check on the database to see if you have all the entities & CPUs for Disk Browser Load.
            var loadedData = tableTimestamp.GetLoadedDataFor(dataStartDate, dataStartDate.AddDays(1));

            var timeRange = "";
            //Get Table Names.
            var tableNames = loadedData.AsEnumerable().Select(x => x.Field<string>("TableName")).Distinct().ToList();

            if (tableNames.Count < 3)
                timeRange = "";
            else {

                //
                var entityCheck = new Entities(mySqlConnectionString);
                DataTable timeIntervalCountPerEntity = entityCheck.GetTimeIntervalCountPerEntity(tableNames);

                foreach (var tableName in tableNames) {

                    log.InfoFormat("tableName: {0}", tableName);


                    var times = loadedData.AsEnumerable().Where(x => x.Field<string>("TableName") == tableName).OrderBy(x => x.Field<DateTime>("Start")).ToList();
                    if (times.Count == 1) {
                        var fromTime = times.Select(x => x.Field<DateTime>("Start")).Single();
                        var toTime = times.Select(x => x.Field<DateTime>("End")).Single();

                        var timediff = toTime - fromTime;
                        if (timediff.TotalHours >= 8)
                            timeRange = fromTime + "|" + toTime;
                        else {
                            timeRange = "";
                            return timeRange;
                        }
                    }
                    else if (times.Count > 1) {
                        var previousEndTime = new DateTime();
                        for (var x = 0; x < times.Count; x++) {
                            if (x == 0) {
                                previousEndTime = Convert.ToDateTime(times[0]["End"]);
                            }
                            else {
                                var startTime = Convert.ToDateTime(times[x]["Start"]);
                                var endTime = Convert.ToDateTime(times[x]["End"]);

                                if (startTime != previousEndTime) {
                                    timeRange = "";
                                    return timeRange;
                                }
                                else {
                                    previousEndTime = endTime;
                                }
                            }
                        }
                        


                        var fromTime = times.Min(x => x.Field<DateTime>("Start"));
                        var toTime = times.Max(x => x.Field<DateTime>("End"));
                        var timediff = toTime - fromTime;
                        if (timediff.TotalHours >= 8)
                            timeRange = fromTime + "|" + toTime;
                        else {
                            timeRange = "";
                            return timeRange;
                        }
                    }
                    else {
                        timeRange = "";
                        return timeRange;
                    }

                    log.InfoFormat("okaytoLoad: {0}", timeRange);
                    
                }
            }
            return timeRange;
        }

        public void SendLoadTrendErrors(List<TrendError> trendErrors, 
                                            String UWSSerialNumber,
                                            DateTime startTimeLCT,
                                            DateTime stopTimeLCT,
                                             Dictionary<string, string> entityList,
                                            long sampleInterval)
        {
            var emailText = new StringBuilder();
            emailText.Append("<br>Error loading trend");
            foreach (var trendError in trendErrors)
            {
                emailText.Append("<UL>");
                emailText.Append("	<LI>");
                emailText.Append("		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>Source:  " + trendError.Source + " </DIV>");
                if(trendError.Cause != null) {
                    emailText.Append("	<LI>");
                    emailText.Append("		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>Message:  " + trendError.Cause.Message + " </DIV>");
                    emailText.Append("	<LI>");
                    emailText.Append("		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>StackTrace: " + trendError.Cause.StackTrace + "</DIV>");
                }
                emailText.Append("	<LI>");
                emailText.Append("		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>UWSSerialNumber: " + UWSSerialNumber + "</DIV>");
                emailText.Append("	<LI>");
                emailText.Append("		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>StartTimeLCT: " + startTimeLCT + "</DIV>");
                emailText.Append("	<LI>");
                emailText.Append("		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>StopTimeLCT: " + stopTimeLCT + "</DIV>");
                if(entityList != null) {
                    foreach (KeyValuePair<string, string> entry in entityList)
                    {
                        emailText.Append("	<LI>");
                            emailText.Append("		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>" + entry.Key + ": " + entry.Value + "</DIV>");
                    }
                }
                emailText.Append("	<LI>");
                emailText.Append("		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>Sample Interval: " + sampleInterval + "</DIV>");
                emailText.Append("	</LI>");
                emailText.Append("</UL>");
            }

            var email = new EmailHelper();
            email.SendErrorEmail(emailText.ToString());
        }

        public void UpdateLogEntryAndStatus(string fileName, string logEntry, string logStatus) {
            try {
                // Update log Information on RA Website UI
                var uploadFileNameService = new UploadFileNameServices(_connectionStr);
                var orderId = uploadFileNameService.GetOrderIdFor(fileName);
                var uploadMessage = new UploadMessagesService(_connectionStr);
                var uploads = new Uploads(_connectionStr);

                uploadMessage.InsertNewEntryFor(orderId, DateTime.Now, logEntry);
                uploads.UpdateLoadedStatus(orderId, logStatus);
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }

        }
    }
}