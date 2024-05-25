using System;
using System.Data;

namespace RemoteAnalyst.Repository.Infrastructure
{
    public static class Helper {
        public static string CommandParameter = "; set net_write_timeout=99999; set net_read_timeout=99999";
        public static DataSet InsertEmptyData(DateTime currentDate)
        {
            var myDataSet = new DataSet();
            var myDataTable = new DataTable("HourlyBusy");
            DataRow myDataRow;
            DataColumn myDataColumn;

            // Create String column.
            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.DateTime");
            myDataColumn.ColumnName = "DataDate";
            // Add the column to the table.
            myDataTable.Columns.Add(myDataColumn);

            for (int x = 0; x < 24; x++)
            {
                // Create Double column.
                myDataColumn = new DataColumn();
                myDataColumn.DataType = Type.GetType("System.Double");
                myDataColumn.ColumnName = "Hour" + x;
                // Add the column to the table.
                myDataTable.Columns.Add(myDataColumn);
            }

            myDataRow = myDataTable.NewRow();
            //string dt = currentDate.ToString();
            myDataRow["DataDate"] = currentDate;
            for (int x = 0; x < 24; x++)
                myDataRow["Hour" + x] = 0;

            myDataTable.Rows.Add(myDataRow);

            // Add the new DataTable to the DataSet.
            myDataSet.Tables.Add(myDataTable);
            return myDataSet;
        }


        public static DataSet InsertEmptyDataSingle(DateTime currentDate) {
            var myDataSet = new DataSet();
            var myDataTable = new DataTable("HourlyBusy");
            DataRow myDataRow;
            DataColumn myDataColumn;

            // Create String column.
            myDataColumn = new DataColumn();
            myDataColumn.DataType = Type.GetType("System.DateTime");
            myDataColumn.ColumnName = "DataDate";
            // Add the column to the table.
            myDataTable.Columns.Add(myDataColumn);

            for (int x = 0; x < 24; x++) {
                // Create Double column.
                myDataColumn = new DataColumn();
                myDataColumn.DataType = Type.GetType("System.Single");
                myDataColumn.ColumnName = "Hour" + x;
                // Add the column to the table.
                myDataTable.Columns.Add(myDataColumn);
            }

            myDataRow = myDataTable.NewRow();
            //string dt = currentDate.ToString();
            myDataRow["DataDate"] = currentDate;
            for (int x = 0; x < 24; x++)
                myDataRow["Hour" + x] = 0;

            myDataTable.Rows.Add(myDataRow);

            // Add the new DataTable to the DataSet.
            myDataSet.Tables.Add(myDataTable);
            return myDataSet;
        }
    }
}