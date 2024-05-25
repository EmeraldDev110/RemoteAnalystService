using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using RemoteAnalyst.UWSLoader.Core.ModelView;
using RemoteAnalyst.UWSLoader.Core.Repository;

namespace RemoteAnalyst.UWSLoader.Core.BusinessLogic {
    class DataDictionaryService {
        private readonly string _connectionString = "";

        public DataDictionaryService(string connectionString) {
            _connectionString = connectionString;
        }

        public IList<ColumnInfoView> GetColumnsFor(int indexName, string tableName) {
            var dataDictionary = new DataDictionary(_connectionString);
            DataTable columnInfo = dataDictionary.GetColumns(indexName, tableName);
            IList<ColumnInfoView> ColumnInfoList = new List<ColumnInfoView>();
            foreach (DataRow dr in columnInfo.Rows) {
                var columnInfoView = new ColumnInfoView();
                columnInfoView.ColumnName = dr["ColumnName"].ToString();
                columnInfoView.TypeName = dr["ColumnType"].ToString();
                columnInfoView.TypeValue = Convert.ToInt32(dr["ColumnSize"]);
                columnInfoView.Website = Convert.ToBoolean(dr["Website"]);
                ColumnInfoList.Add(columnInfoView);
            }
            return ColumnInfoList;
        }

        public IList<ColumnInfoView> GetPathwayColumnsFor(string tableName) {
            var dataDictionary = new DataDictionary(_connectionString);
            DataTable columnInfo = dataDictionary.GetPathwayColumns(tableName);
            IList<ColumnInfoView> ColumnInfoList = new List<ColumnInfoView>();
            foreach (DataRow dr in columnInfo.Rows) {
                var columnInfoView = new ColumnInfoView();
                columnInfoView.ColumnName = dr["FName"].ToString();
                columnInfoView.TypeName = dr["FType"].ToString();
                columnInfoView.TypeValue = Convert.ToInt32(dr["FSize"]);
                ColumnInfoList.Add(columnInfoView);
            }
            return ColumnInfoList;
        }

        public IList<ColumnInfoView> GetExtraDISCEnityColumns() {//Hard code the extra columns for DISC entity table
            IList<ColumnInfoView> columnInfoList = new List<ColumnInfoView>();
            columnInfoList.Add(new ColumnInfoView() {
                ColumnName = "IORate",
                TypeName = "FLOAT",
                TypeValue = 8
            });
            columnInfoList.Add(new ColumnInfoView() {
                ColumnName = "QueueLength",
                TypeName = "FLOAT",
                TypeValue = 8
            });
            columnInfoList.Add(new ColumnInfoView() {
                ColumnName = "CacheHitRate",
                TypeName = "FLOAT",
                TypeValue = 8
            });
            columnInfoList.Add(new ColumnInfoView() {
                ColumnName = "DP2Busy",
                TypeName = "FLOAT",
                TypeValue = 8
            });
            columnInfoList.Add(new ColumnInfoView() {
                ColumnName = "BusiestFileName",
                TypeName = "NVARCHAR",
                TypeValue = 24
            });
            columnInfoList.Add(new ColumnInfoView() {
                ColumnName = "BusiestFileIO",
                TypeName = "FLOAT",
                TypeValue = 8
            });

            return columnInfoList;
        } 
    }
}
