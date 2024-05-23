using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using log4net;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices {
    /// <summary>
    /// OSSJRNLService get's the OSS JRNL data and loads it to the database
    /// </summary>
    public class OSSJRNLService {
        private readonly string _connectionString;

        /// <summary>
        /// Constrator
        /// </summary>
        /// <param name="connectionStringSPAM">Per System Database Connection</param>
        public OSSJRNLService(string connectionStringSPAM) {
            _connectionString = connectionStringSPAM;
        }

        /// <summary>
        /// LoadOSSJRNL loads OSS JRNL data to database.
        /// </summary>
        /// <param name="ossFilePath">OSS JRNL File Full Path</param>
        /// <param name="systemSerial">System Serial Number</param>
        /// <param name="tempSaveLocation">Temp CSV Save Location</param>
        public void LoadOSSJRNL(string ossFilePath, string systemSerial, string tempSaveLocation) {
            try {
                if (File.Exists(ossFilePath)) {
                    //Create database.
                    var oss = new OSSJRNL(_connectionString);
                    string tableName = systemSerial + "_OSS_Names";

                    //Check for duplicate table.
                    bool tableExists = oss.CheckDuplicate(tableName);

                    if (!tableExists) {
                        oss.CreateOSSTable(tableName);
                        oss.CreateOSSIndex(tableName);
                    }

                    var ossNames = new List<OSSInfo>();

                    //Populate data.
                    using (var reader = new StreamReader(ossFilePath)) {
                        while (!reader.EndOfStream) {
                            string ossDatas = reader.ReadLine();
                            string[] ossData = ossDatas.Split('|');
                            if (ossData.Length > 1) {
                                string ossFileType = ossData[0].Trim();
                                string fileName = ossData[1].Trim();
                                string pathName = ossData[2].Trim();
                                if (pathName.Length != 0) {
                                    ossNames.Add(new OSSInfo {
                                        FileType = ossFileType,
                                        FileName = fileName,
                                        PathName = pathName
                                    });
                                }
                            }
                        }
                    }

                    if (ossNames.Count > 0) {
                        //Convert List to DataSet.
                        var ossNamesTable = ConvertListToDataTable(ossNames);
                        //Bulk Insert ossNames.
                        var dataTables = new DataTables(_connectionString);
                        dataTables.InsertEntityData(tableName, ossNamesTable, tempSaveLocation);
                    }
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public DataTable ConvertListToDataTable(List<OSSInfo> ossNames) {
            var myDataTable = new DataTable("OSS");

            var myDataColumn = new DataColumn { DataType = Type.GetType("System.String"), ColumnName = "FileType" };
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn {DataType = Type.GetType("System.String"), ColumnName = "FileName"};
            myDataTable.Columns.Add(myDataColumn);

            myDataColumn = new DataColumn {DataType = Type.GetType("System.String"), ColumnName = "PathName"};
            myDataTable.Columns.Add(myDataColumn);

            foreach (var ossName in ossNames) {
                var myDataRow = myDataTable.NewRow();
                myDataRow["FileType"] = ossName.FileType;
                myDataRow["FileName"] = ossName.FileName;
                myDataRow["PathName"] = ossName.PathName;
                myDataTable.Rows.Add(myDataRow);
            }
            return myDataTable;
        }

    }

    public class OSSInfo {
        public string FileType { get; set; }
        public string FileName { get; set; }
        public string PathName { get; set; }
    }
}