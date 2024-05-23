using System;
using System.Collections.Generic;
using System.Data;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices
{
    public class DataDictionaryService
    {
        private readonly string ConnectionString = "";

        public DataDictionaryService(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public IList<ColumnInfoView> GetColumnsFor(int indexName, string tableName)
        {
            var dataDictionary = new DataDictionary(ConnectionString);
            DataTable columnInfo = dataDictionary.GetColumns(indexName, tableName);
            IList<ColumnInfoView> ColumnInfoList = new List<ColumnInfoView>();
            foreach (DataRow dr in columnInfo.Rows)
            {
                var columnInfoView = new ColumnInfoView();
                columnInfoView.ColumnName = dr["ColumnName"].ToString();
                columnInfoView.TypeName = dr["ColumnType"].ToString();
                columnInfoView.TypeValue = Convert.ToInt32(dr["ColumnSize"]);
                columnInfoView.Website = Convert.ToBoolean(dr["Website"]);
                ColumnInfoList.Add(columnInfoView);
            }
            return ColumnInfoList;
        }

        public IList<ColumnInfoView> GetPathwayColumnsFor(string tableName)
        {
            var dataDictionary = new DataDictionary(ConnectionString);
            DataTable columnInfo = dataDictionary.GetPathwayColumns(tableName);
            IList<ColumnInfoView> ColumnInfoList = new List<ColumnInfoView>();
            foreach (DataRow dr in columnInfo.Rows)
            {
                var columnInfoView = new ColumnInfoView();
                columnInfoView.ColumnName = dr["FName"].ToString();
                columnInfoView.TypeName = dr["FType"].ToString();
                columnInfoView.TypeValue = Convert.ToInt32(dr["FSize"]);
                ColumnInfoList.Add(columnInfoView);
            }
            return ColumnInfoList;
        }

    }
}