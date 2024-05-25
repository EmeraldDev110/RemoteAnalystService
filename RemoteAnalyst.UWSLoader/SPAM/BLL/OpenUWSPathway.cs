using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.BusinessLogic.UWSLoader.BaseClass;
using RemoteAnalyst.BusinessLogic.UWSLoader;
using log4net;

namespace RemoteAnalyst.UWSLoader.SPAM.BLL {

    /// <summary>
    /// OpenUWSPathway class reads the pathway file and load the data into database.
    /// </summary>
    internal class OpenUWSPathway : Header {
        private readonly ILog _log;
        private readonly int _UWSID;
        private readonly string _UWSPath = "";
        private readonly string _UWSUnzipPath = "";
        private readonly string connectionStr = BusinessLogic.Util.ConnectionString.ConnectionStringDB;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="uwsID"> UWS ID of this load.</param>
        /// <param name="uwsPath"> Full path of the file.</param>
        /// <param name="uwsUnzipPath"> Full path of the unzipped data file. 
        /// For now we do not accept zipped file so uwsPath and uwsUnzipPath would have the same value</param>
        /// <param name="newFileLog"> Stream writer of the log file.</param>
        public OpenUWSPathway(int uwsID, string uwsPath, string uwsUnzipPath, ILog log) {
            _UWSID = uwsID;
            _UWSPath = uwsPath;
            _UWSUnzipPath = uwsUnzipPath;
            _log = log;
        }

        /// <summary>
        /// Main function of this class.
        /// Call functions to read data and load data into database.
        /// </summary>
        /// <returns> Return a bool value suggests whether the load is successful or not.</returns>
        internal bool CreateNewData() {
            bool success = false;
            _log.Info("Calling OpenUWSPathwayFile");
            
            OpenUWSPathwayFile(_UWSUnzipPath);

            _log.Info("Calling CreatePathwayDataSet");
            

            success = CreatePathwayDataSet(_UWSUnzipPath, _UWSID);
            DateTime beforeTime = DateTime.Now;

            _log.InfoFormat("*Load Pathway: {0}", success);
            

            DateTime afterTime = DateTime.Now;
            TimeSpan span = afterTime - beforeTime;
            _log.InfoFormat("*Total Pathway Load time in minutes: {0}", span.TotalMinutes);
            

            return success;
        }
        
        /// <summary>
        ///  Open the UWS pathway file and load data into the variables that defined in Header.
        /// </summary>
        /// <param name="uwsPath"> Full path of the UWS pathway file.</param>
        /// <returns></returns>
        public bool OpenUWSPathwayFile(string uwsPath) {
            using (var stream = new FileStream(uwsPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                //using (StreamReader reader = new StreamReader(stream))
                using (reader = new BinaryReader(stream)) {
                    var myEncoding = new ASCIIEncoding();

                    #region Basic Header Info

                    //Identifier
                    reader.BaseStream.Seek(0, SeekOrigin.Begin);
                    UwsIdentifierByte = reader.ReadBytes(UwsIdentifierByte.Length);
                    UwsIdentifier = myEncoding.GetString(UwsIdentifierByte).Trim();

                    //Key.
                    reader.BaseStream.Seek(10, SeekOrigin.Begin);
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
                    reader.BaseStream.Seek(36, SeekOrigin.Begin);
                    UwsHLen = reader.ReadInt16();
                    UwsHLen = Helper.Reverse(UwsHLen);

                    //UwsHVersion
                    reader.BaseStream.Seek(38, SeekOrigin.Begin);
                    UwsHVersion = reader.ReadInt16();
                    UwsHVersion = Helper.Reverse(UwsHVersion);

                    //UwsXLen
                    reader.BaseStream.Seek(40, SeekOrigin.Begin);
                    UwsXLen = reader.ReadInt16();
                    UwsXLen = Helper.Reverse(UwsXLen);

                    //UwsXRecords
                    reader.BaseStream.Seek(42, SeekOrigin.Begin);
                    UwsXRecords = reader.ReadInt16();
                    UwsXRecords = Helper.Reverse(UwsXRecords);

                    //UwsSignatureTypeByte
                    reader.BaseStream.Seek(66, SeekOrigin.Begin);
                    UwsSignatureTypeByte = reader.ReadBytes(UwsSignatureTypeByte.Length);
                    UwsSignatureType = myEncoding.GetString(UwsSignatureTypeByte).Trim();

                    //UwsVersion
                    reader.BaseStream.Seek(216, SeekOrigin.Begin);
                    UwsVersion = reader.ReadInt32();
                    UwsVersion = Helper.Reverse(UwsVersion);

                    //UwsVstringByte
                    reader.BaseStream.Seek(220, SeekOrigin.Begin);
                    UwsVstringByte = reader.ReadBytes(UwsVstringByte.Length);
                    UwsVstring = myEncoding.GetString(UwsVstringByte).Trim();

                    //UwsSystemNameByte
                    reader.BaseStream.Seek(284, SeekOrigin.Begin);
                    UwsSystemNameByte = reader.ReadBytes(UwsSystemNameByte.Length);
                    UwsSystemName = myEncoding.GetString(UwsSystemNameByte).Trim();

                    //UwsGMTStartTimestamp
                    reader.BaseStream.Seek(712, SeekOrigin.Begin);
                    UwsGMTStartTimestamp = reader.ReadInt64();
                    UwsGMTStartTimestamp = Helper.Reverse(UwsGMTStartTimestamp);
                    //Need to do / 10000 to get current julian time
                    UwsGMTStartTimestamp /= 10000;

                    //UwsGMTStopTimestamp
                    reader.BaseStream.Seek(720, SeekOrigin.Begin);
                    UwsGMTStopTimestamp = reader.ReadInt64();
                    UwsGMTStopTimestamp = Helper.Reverse(UwsGMTStopTimestamp);
                    //Need to do / 10000 to get current julian time
                    UwsGMTStopTimestamp /= 10000;

                    //UwsLCTStartTimestamp
                    reader.BaseStream.Seek(728, SeekOrigin.Begin);
                    UwsLCTStartTimestamp = reader.ReadInt64();
                    UwsLCTStartTimestamp = Helper.Reverse(UwsLCTStartTimestamp);
                    //Need to do / 10000 to get current julian time
                    UwsLCTStartTimestamp /= 10000;

                    //UwsSampleInterval
                    reader.BaseStream.Seek(736, SeekOrigin.Begin);
                    UwsSampleInterval = reader.ReadInt64();
                    UwsSampleInterval = Helper.Reverse(UwsSampleInterval);

                    //UwsCdataClassId
                    reader.BaseStream.Seek(1488, SeekOrigin.Begin);
                    UwsCdataClassId = reader.ReadInt32();
                    UwsCdataClassId = Helper.Reverse(UwsCdataClassId);

                    //UwsCollectorVersion
                    reader.BaseStream.Seek(2092, SeekOrigin.Begin);
                    UwsCollectorVersion = reader.ReadInt32();
                    UwsCollectorVersion = Helper.Reverse(UwsCollectorVersion);

                    //UwsCollectorVstringByte
                    reader.BaseStream.Seek(2098, SeekOrigin.Begin);
                    UwsCollectorVstringByte = reader.ReadBytes(UwsCollectorVstringByte.Length);
                    UwsCollectorVstring = myEncoding.GetString(UwsCollectorVstringByte).Trim();

                    #endregion

                    #region Create Index

                    //Create Index.
                    //byte[] indexBytes = new byte[60];
                    var indexBytes = new byte[62];
                    var tempShortBytes = new byte[2];
                    var tempIntBytes = new byte[4];

                    int indexPosition = UwsHLen;
                    long tempLen = (Convert.ToInt64(UwsXRecords) * Convert.ToInt64(UwsXLen));
                    long dataPosition = indexPosition + tempLen;

                    //List<Indices> index = new List<Indices>();
                    for (int x = 0; x < UwsXRecords; x++) {
                        var indexer = new Indices();

                        reader.BaseStream.Seek(indexPosition, SeekOrigin.Begin);
                        indexBytes = reader.ReadBytes(indexBytes.Length);

                        //Get Index Name (first 8 bytes).
                        indexer.FName = myEncoding.GetString(indexBytes, 0, 8);

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

                        //Index File Postion.
                        indexer.FilePosition = dataPosition;

                        //Insert into the List.
                        index.Add(indexer);

                        indexPosition += UwsXLen;
                        tempLen = (Convert.ToInt64(indexer.FRecords) * Convert.ToInt64(indexer.FReclen));
                        dataPosition = dataPosition + tempLen;
                    }

                    #endregion
                }
            }

            return true;
        }

        /// <summary>
        /// Load the data into the database.
        /// </summary>
        /// <param name="unzipedFile">Full path of the Pathway file.</param>
        /// <param name="uwsID"> UWS ID of this load</param>
        /// <returns>Return a bool value suggests whether the load is successful or not.</returns>
        internal bool CreatePathwayDataSet(string unzipedFile, int uwsID) {
            bool success = false;
            var tempShort = new byte[2];
            var tempInt = new byte[4];
            var tempLong = new byte[8];
            string tempString = string.Empty;
            long tempDate = 0;
            int recordLenth = 0;
            long filePosition = 0;

            //ConnectionString
            string ConnectionString = Config.RAConnectionString;
            //Get NSID.
            var sampleService = new SampleService(ConnectionString);
            int nsid = sampleService.GetMaxNSIDFor();

            _log.InfoFormat("UwsSystemName: {0}", UwsSystemName);
            _log.InfoFormat("nsid: {0}", nsid);
            _log.InfoFormat("_UWSPath: {0}", _UWSPath);
            

            //Code change for David's new UWS.
            string systemName = UwsSystemName;

            //Insert info into Sample.
            //sampleService.InsertNewEntryPathwayFor(UwsSystemName.Substring(0, 8), nsid, _UWSPath);
            sampleService.InsertNewEntryPathwayFor(systemName, nsid, _UWSPath);

            //SampleInfo values.
            string systemSerial = UWSSerialNumber;
            string counterName = "";

            var startTimeLCT = new DateTime();
            var stopTimeLCT = new DateTime();
            long sampleInterval = UwsSampleInterval;
            string sysContent = "";
            int entityID = 0;
            try {
                #region Open File and Load Data.

                _log.Info("    -Opening Pathway UWS to populate table");
                
                using (var stream = new FileStream(unzipedFile, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    var dicInfo = new DirectoryInfo(stream.Name);
                    //using (StreamReader reader = new StreamReader(stream))
                    using (reader = new BinaryReader(stream)) {
                        //Loop thorugh the entity.
                        for (int i = 0; i < index.Count; i++) {
                            if (index[i].FName.Length != 0 && index[i].FRecords > 0) {
                                counterName = index[i].FName;
                                _log.InfoFormat("    -Counter: {0}, Started", counterName);
                                
                                DateTime beforeTime = DateTime.Now;

                                string tableName = "pv" + counterName;
                                //Get table name according to data format.
                                //Get Column type into the List.
                                var dictionaryService = new DataDictionaryService(ConnectionString);
                                IList<ColumnInfoView> columnInfoList = dictionaryService.GetPathwayColumnsFor(tableName);

                                recordLenth = index[i].FReclen;
                                filePosition = index[i].FilePosition;
                                var indexBytes = new byte[recordLenth];
                                long currentPosition = 0;

                                try {
                                    #region Create DataSet and insert data

                                    //Create DataSet with DataTable(s).
                                    DataTable myDataTable = CreateDataTableColumn(columnInfoList, tableName);

                                    DataRow myDataRow;
                                    //GID test. no one knows why we need GID.
                                    int gID = 0;

                                    //Loop through the records.
                                    for (int x = 0; x < index[i].FRecords; x++) {
                                        #region each record

                                        reader.BaseStream.Seek(filePosition, SeekOrigin.Begin);
                                        indexBytes = reader.ReadBytes(recordLenth);
                                        currentPosition = 0;

                                        //this will create newline for each loop.
                                        //Create new row.
                                        //string insertdbString = string.Empty;
                                        myDataRow = myDataTable.NewRow();

                                        //Insert nsid, "4", GID.
                                        myDataRow["TSID"] = nsid;
                                        myDataRow["DataClass"] = 4;
                                        myDataRow["GID"] = gID;

                                        foreach (ColumnInfoView column in columnInfoList) {
                                            if (column.ColumnName == "TSID") {
                                                continue;
                                            }
                                            if (column.ColumnName == "DataClass") {
                                                continue;
                                            }
                                            if (column.ColumnName == "GID") {
                                                continue;
                                            }

                                            // This condition is to check if the number of fields in the UWS file is less than in TabelTable, we have to stop getting data and feed in just 'Null'
                                            if (currentPosition < index[i].FReclen) {
                                                #region Switch

                                                switch (column.TypeName.ToUpper().Trim()) {
                                                    case "SHORT":
                                                        tempShort[0] = indexBytes[currentPosition + 1];
                                                        tempShort[1] = indexBytes[currentPosition];
                                                        column.TestValue = BitConverter.ToInt16(tempShort, 0).ToString();
                                                        column.TypeValue = 2;
                                                        break;
                                                    case "LONG":
                                                        tempInt[0] = indexBytes[currentPosition + 3];
                                                        tempInt[1] = indexBytes[currentPosition + 2];
                                                        tempInt[2] = indexBytes[currentPosition + 1];
                                                        tempInt[3] = indexBytes[currentPosition + 0];
                                                        column.TestValue = BitConverter.ToInt32(tempInt, 0).ToString();
                                                        column.TypeValue = 4;
                                                        break;
                                                    case "ULONG":
                                                        tempInt[0] = indexBytes[currentPosition + 3];
                                                        tempInt[1] = indexBytes[currentPosition + 2];
                                                        tempInt[2] = indexBytes[currentPosition + 1];
                                                        tempInt[3] = indexBytes[currentPosition + 0];
                                                        column.TestValue = BitConverter.ToUInt32(tempInt, 0).ToString();
                                                        column.TypeValue = 4;
                                                        break;
                                                    case "DOUBLE":
                                                        tempLong[0] = indexBytes[currentPosition + 7];
                                                        tempLong[1] = indexBytes[currentPosition + 6];
                                                        tempLong[2] = indexBytes[currentPosition + 5];
                                                        tempLong[3] = indexBytes[currentPosition + 4];
                                                        tempLong[4] = indexBytes[currentPosition + 3];
                                                        tempLong[5] = indexBytes[currentPosition + 2];
                                                        tempLong[6] = indexBytes[currentPosition + 1];
                                                        tempLong[7] = indexBytes[currentPosition + 0];
                                                        column.TestValue = BitConverter.ToInt64(tempLong, 0).ToString();
                                                        column.TypeValue = 8;
                                                        break;
                                                    case "TEXT":
                                                        tempString = "";
                                                        for (int z = 0; z < column.TypeValue; z++) {
                                                            tempString += Convert.ToChar(indexBytes[currentPosition + z]);
                                                        }
                                                        column.TestValue = tempString.Trim();
                                                            break;
                                                    case "DATE":
                                                        tempLong[0] = indexBytes[currentPosition + 7];
                                                        tempLong[1] = indexBytes[currentPosition + 6];
                                                        tempLong[2] = indexBytes[currentPosition + 5];
                                                        tempLong[3] = indexBytes[currentPosition + 4];
                                                        tempLong[4] = indexBytes[currentPosition + 3];
                                                        tempLong[5] = indexBytes[currentPosition + 2];
                                                        tempLong[6] = indexBytes[currentPosition + 1];
                                                        tempLong[7] = indexBytes[currentPosition + 0];
                                                        tempDate = Convert.ToInt64(BitConverter.ToInt64(tempLong, 0));
                                                        //Need to do / 10000 to get current julian time
                                                        tempDate /= 10000;

                                                        if (tempDate == 0) {
                                                            column.TestValue = "";
                                                        }
                                                        else {
                                                            var convert = new ConvertJulianTime();
                                                            int obdTimeStamp = convert.JulianTimeStampToOBDTimeStamp(tempDate);
                                                            DateTime dbDate = convert.OBDTimeStampToDBDate(obdTimeStamp);
                                                            column.TestValue = dbDate.ToString();
                                                        }
                                                        column.TypeValue = 8;
                                                        break;
                                                }

                                                #endregion

                                                currentPosition += column.TypeValue;
                                            }
                                            else {
                                                column.TestValue = "";
                                            }
                                        }

                                        //Populate Datatable.
                                        foreach (ColumnInfoView column in columnInfoList) {
                                            if (column.ColumnName == "TSID") {
                                                continue;
                                            }
                                            if (column.ColumnName == "DataClass") {
                                                continue;
                                            }
                                            if (column.ColumnName == "GID") {
                                                continue;
                                            }

                                            #region Switch

                                            switch (column.TypeName.ToUpper().Trim()) {
                                                case "SHORT":
                                                    myDataRow[column.ColumnName] = Convert.ToInt16(column.TestValue);
                                                    break;
                                                case "LONG":
                                                    myDataRow[column.ColumnName] = Convert.ToInt32(column.TestValue);
                                                    break;
                                                case "ULONG":
                                                    myDataRow[column.ColumnName] = Convert.ToUInt32(column.TestValue);
                                                    break;
                                                case "DOUBLE":
                                                    myDataRow[column.ColumnName] = Convert.ToInt64(column.TestValue);
                                                    break;
                                                case "TINYINT":
                                                    myDataRow[column.ColumnName] = Convert.ToByte(column.TestValue);
                                                    break;
                                                case "TEXT":
                                                    myDataRow[column.ColumnName] = column.TestValue;
                                                    break;
                                                case "DATE":
                                                    myDataRow[column.ColumnName] = Convert.ToDateTime(column.TestValue);
                                                    break;

                                            }

                                            #endregion
                                        }

                                        //Add new row into the dataSet.
                                        myDataTable.Rows.Add(myDataRow);

                                        //check to see if the row has more then BulkLoaderSize rows.
                                        if (myDataTable.Rows.Count >
                                            Repository.Infrastructure.Constants.getInstance(Config.RAConnectionString).BulkLoaderSize) {
                                            //Insert into the table.
                                            var InsertTables = new DataTableService(Config.RAConnectionString);
                                            InsertTables.InsertEntityDataFor(tableName, myDataTable, dicInfo.Parent.FullName);

                                            //Clear the myDataTable.
                                            myDataTable.Rows.Clear();
                                        }

                                        //Increase the start reading position.
                                        filePosition += recordLenth;

                                        #endregion

                                        gID++;
                                    } // End For

                                    //Insert into the database.
                                    var tables = new DataTableService(Config.RAConnectionString);
                                    tables.InsertEntityDataFor(tableName, myDataTable, dicInfo.Parent.FullName);
                                    DateTime afterTime = DateTime.Now;
                                    TimeSpan timeSpan = afterTime - beforeTime;
                                    _log.InfoFormat("    -Counter: {0} Total Time in Minutes: {1}", counterName, timeSpan.TotalMinutes);
                                    myDataTable = null;
                                    #endregion
                                }
                                catch (Exception ex) {
                                    _log.ErrorFormat("Counter: {0}.\n RA Message: {1}\n RA File Position = {2}",
                                                        counterName, ex.Message, filePosition);
                                    success = false;
                                    throw new Exception(ex.Message);
                                }
                                finally {
                                    dictionaryService = null;
                                    columnInfoList = null;
                                }
                            }
                        }
                    }
                }

                #endregion

                //Convert starttime and endtime.
                var julianTime = new ConvertJulianTime();
                int lctTimeStamp = julianTime.JulianTimeStampToOBDTimeStamp(UwsLCTStartTimestamp);
                startTimeLCT = julianTime.OBDTimeStampToDBDate(lctTimeStamp);

                lctTimeStamp = julianTime.JulianTimeStampToOBDTimeStamp(UwsLCTStartTimestamp + UwsGMTStopTimestamp - UwsGMTStartTimestamp);
                stopTimeLCT = julianTime.OBDTimeStampToDBDate(lctTimeStamp);

                //Round up the seconds.
                TimeSpan span1 = stopTimeLCT - startTimeLCT;
                double seconds1 = span1.TotalSeconds;
                //Get remained seconds.
                double remainSeconds1 = seconds1 % sampleInterval;

                stopTimeLCT = stopTimeLCT.AddSeconds(-remainSeconds1);

                //IR6653
                //If stopTimeLCT == startTimeLCT, add one day into stopTimeLCT
                if (stopTimeLCT == startTimeLCT)
                {
                    stopTimeLCT = stopTimeLCT.AddDays(1);
                }


                //Update LoadingInfo.
                var loadInfo = new LoadingInfoService(connectionStr);
                loadInfo.UpdateFor(uwsID, systemName, startTimeLCT, stopTimeLCT, 3);

                //Insert sampleInfo data.
                var sampleInfo = new SampleInfoService(connectionStr);
                sampleInfo.InsertNewEntryFor(nsid, systemName, systemSerial, startTimeLCT, stopTimeLCT, sampleInterval, uwsID, sysContent, 3);
                success = true;
                }
            catch (Exception ex) {
                _log.ErrorFormat("EntityID: {0}. {1}", entityID, ex);
                success = false;
            }
            finally {
                _log.InfoFormat("    -Finished populating RA Database with {0}", success);
                GC.Collect();
            }
            return success;
        }

        /// <summary>
        /// Create the table structure.
        /// </summary>
        /// <param name="columnInfo"> List of ColumnInfoView which contains the column info of the table that going to be created.</param>
        /// <param name="tableName"> Name of the table.</param>
        /// <returns> Return a DataTable that contains the table structure.</returns>
        internal DataTable CreateDataTableColumn(IList<ColumnInfoView> columnInfo, string tableName) {
            //This DataTableName has be to start Date(only date part), because I have to compare with data's FromTimestamp.

            var myDataTable = new DataTable();
            DataColumn myDataColumn;

            if (columnInfo[0].ColumnName != "TSID") {
                // Create new DataColumn, set DataType, ColumnName and add to DataTable.    
                myDataColumn = new DataColumn();
                myDataColumn.DataType = Type.GetType("System.Int16");
                myDataColumn.ColumnName = "TSID";
                // Add the Column to the DataColumnCollection.
                myDataTable.Columns.Add(myDataColumn);
            }

            if (columnInfo[1].ColumnName != "DataClass") {
                // Create new DataColumn, set DataType, ColumnName and add to DataTable.    
                myDataColumn = new DataColumn();
                myDataColumn.DataType = Type.GetType("System.Int16");
                myDataColumn.ColumnName = "DataClass";
                // Add the Column to the DataColumnCollection.
                myDataTable.Columns.Add(myDataColumn);
            }

            if (columnInfo[2].ColumnName != "GID") {
                // Create new DataColumn, set DataType, ColumnName and add to DataTable.    
                myDataColumn = new DataColumn();
                myDataColumn.DataType = Type.GetType("System.Int32");
                myDataColumn.ColumnName = "GID";
                // Add the Column to the DataColumnCollection.
                myDataTable.Columns.Add(myDataColumn);
            }
            foreach (ColumnInfoView column in columnInfo) {
                // Create new DataColumn, set DataType, ColumnName and add to DataTable.    
                myDataColumn = new DataColumn();
                myDataColumn.DataType = Type.GetType(GetSystemValueType(column.TypeName));
                myDataColumn.ColumnName = column.ColumnName;
                // Add the Column to the DataColumnCollection.
                myDataTable.Columns.Add(myDataColumn);
            }

            return myDataTable;
        }

        /// <summary>
        /// Get the system data type according to the SQL Server data type.
        /// </summary>
        /// <param name="type"> SQL Server data type. </param>
        /// <returns> Return a string value which is system data type that used in creating datatable.</returns>
        internal string GetSystemValueType(string type) {
            string returnType = string.Empty;
            switch (type.ToUpper()) {
                case "DATE":
                    returnType = "System.DateTime";
                    break;
                case "DOUBLE":
                    returnType = "System.Double";
                    break;
                case "LONG":
                    returnType = "System.Int32";
                    break;
                case "ULONG":
                    returnType = "System.UInt32";
                    break;
                case "SHORT":
                    returnType = "System.Int16";
                    break;
                case "TEXT":
                    returnType = "System.String";
                    break;
                default:
                    returnType = "System.String";
                    break;
            }
            return returnType;
        }
    }
}