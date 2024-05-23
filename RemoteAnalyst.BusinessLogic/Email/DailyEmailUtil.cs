using System;
using System.Data;
using System.Linq;

namespace RemoteAnalyst.BusinessLogic.Email {
    public class DailyEmailUtil {

        const string UNDETERMINED = "-1";
        const string CRITICAL = "2";
        const string WARNING = "1";
        const string OK = "0";
        bool IsUndetermined(String value)
        {
            return (value.CompareTo(UNDETERMINED) == 0);
        }
        bool IsCritical(String value)
        {
            return (value.CompareTo(CRITICAL) == 0);
        }
        bool IsWarning(String value)
        {
            return (value.CompareTo(WARNING) == 0);
        }

        String GetColorCode(String value)
        {
            string colorCode = IsUndetermined(value) ? "gray" :
                                IsCritical(value) ? "red" :
                                IsWarning(value) ? "yellow" : "";
            return colorCode;
        }

        String GetColorCode(DataRow row)
        {
            object[] itemArray = row.ItemArray.Skip(2).ToArray();
            bool critical = itemArray.Cast<string>().AsQueryable().Any(val => val == "red");
            bool warning = itemArray.Cast<string>().AsQueryable().Any(val => val == "yellow");
            string colorCode = critical ? "red" : warning ? "yellow" : "";
            return colorCode;
        }

        public DataTable GenerateGridDataTable(int colLength) {
            var table = new DataTable();
            var myDataColumn = new DataColumn { DataType = Type.GetType("System.String"), ColumnName = "Entity" };
            table.Columns.Add(myDataColumn);
            myDataColumn = new DataColumn { DataType = Type.GetType("System.String"), ColumnName = "Counter" };
            table.Columns.Add(myDataColumn);
            for (var i = 0; i < colLength; i++) {
                myDataColumn = new DataColumn { DataType = Type.GetType("System.Int16"), ColumnName = i < 10 ? "0" + i : "" + i };
                table.Columns.Add(myDataColumn);
            }
            return table;
        }

        public DataTable GenerateGridDataTable(DateTime fromTime, DateTime toTime) {
            var table = new DataTable();
            var myDataColumn = new DataColumn { DataType = Type.GetType("System.String"), ColumnName = "Entity" };
            table.Columns.Add(myDataColumn);
            myDataColumn = new DataColumn { DataType = Type.GetType("System.String"), ColumnName = "Counter" };
            table.Columns.Add(myDataColumn);

            for (var i = fromTime; i < toTime; i = i.AddHours(1)) {
                myDataColumn = new DataColumn { DataType = Type.GetType("System.Int16"), ColumnName = i.Hour < 10 ? "0" + i.Hour : "" + i.Hour };
                table.Columns.Add(myDataColumn);
            }
            return table;
        }
        public DataTable GenerateStorageGridDataTable() {
            var table = new DataTable();
            var myDataColumn = new DataColumn { DataType = Type.GetType("System.String"), ColumnName = "Entity" };
            table.Columns.Add(myDataColumn);
            myDataColumn = new DataColumn { DataType = Type.GetType("System.String"), ColumnName = "Counter" };
            table.Columns.Add(myDataColumn);
            myDataColumn = new DataColumn { DataType = Type.GetType("System.Int16"), ColumnName = "00" };
            table.Columns.Add(myDataColumn);
            return table;
        }
        public DataSet CleanupGridDataTable(DataTable table) {
            //First row is yellow count, second row is red count, it will turn the data table into another two-row table, first row is merged count, second is color code
            //TODO - check column index start
            var dataTable = table.Clone();//New table different structure
            var colorTable = table.Clone();
            var dataSet = new DataSet();

            DataRow dataRow = dataTable.NewRow();
            DataRow colorRow = colorTable.NewRow();
            dataRow[table.Columns[0].ColumnName] = table.Rows[0][table.Columns[0].ColumnName];
            dataRow[table.Columns[1].ColumnName] = table.Rows[0][table.Columns[1].ColumnName];
            colorRow[table.Columns[0].ColumnName] = table.Rows[0][table.Columns[0].ColumnName];
            colorRow[table.Columns[1].ColumnName] = table.Rows[0][table.Columns[1].ColumnName];
            for (var i = 2; i < table.Columns.Count; i++) {
                if (table.Rows[1][table.Columns[i].ColumnName].ToString().Length > 0) {
                    if (int.Parse(table.Rows[0][table.Columns[i].ColumnName].ToString()) == -1 ||
                        int.Parse(table.Rows[1][table.Columns[i].ColumnName].ToString()) == -1) {
                        dataRow[table.Columns[i].ColumnName] = int.Parse(table.Rows[0][i].ToString());
                        colorRow[table.Columns[i].ColumnName] = -1;
                    }
                    else if (int.Parse(table.Rows[1][table.Columns[i].ColumnName].ToString()) > 0) {
                        dataRow[table.Columns[i].ColumnName] = int.Parse(table.Rows[1][i].ToString());
                        colorRow[table.Columns[i].ColumnName] = 2; //-1,0,1,2 are color codes - no data, none, yellow, red
                    }
                    else if (int.Parse(table.Rows[0][table.Columns[i].ColumnName].ToString()) > 0) {
                        dataRow[table.Columns[i].ColumnName] = int.Parse(table.Rows[0][i].ToString());
                        colorRow[table.Columns[i].ColumnName] = 1;
                    }
                    else {
                        dataRow[table.Columns[i].ColumnName] = 0;
                        colorRow[table.Columns[i].ColumnName] = 0;
                    }
                }
            }
            dataTable.Rows.Add(dataRow);
            colorTable.Rows.Add(colorRow);
            dataSet.Tables.Add(dataTable);
            dataSet.Tables.Add(colorTable);
            return dataSet;
        }

        public DataSet MergeDataAndColor(DataSet cpuBusy, DataSet cpuQueue, DataSet ipuBusy, DataSet ipuQueue, DataSet diskQueue, DataSet diskDp2, DataSet storage) {
            var dataSet = new DataSet();
            var dataTable = cpuBusy.Tables[0].Clone();
            var colorTable = new DataTable();
            var myDataColumn = new DataColumn { DataType = Type.GetType("System.String"), ColumnName = "Entity" };
            colorTable.Columns.Add(myDataColumn);
            myDataColumn = new DataColumn { DataType = Type.GetType("System.String"), ColumnName = "Counter" };
            colorTable.Columns.Add(myDataColumn);

            for (var i = 0; i < dataTable.Columns.Count - 2; i++) {
                myDataColumn = new DataColumn { DataType = Type.GetType("System.String"), ColumnName = i < 10 ? "0" + i : "" + i };
                colorTable.Columns.Add(myDataColumn);
            }

            dataTable.Merge(cpuBusy.Tables[0]);
            dataTable.Merge(cpuQueue.Tables[0]);
            dataTable.Merge(ipuBusy.Tables[0]);
            dataTable.Merge(ipuQueue.Tables[0]);
            if (diskDp2.Tables.Count > 0) dataTable.Merge(diskDp2.Tables[0]);
            if (diskQueue.Tables.Count > 0) dataTable.Merge(diskQueue.Tables[0]);
            if (storage.Tables.Count > 0) dataTable.Merge(storage.Tables[0]);

            //Color table has 5 rows - including the header color
            DataRow headerColorRow = colorTable.NewRow();

            for (var i = 2; i < colorTable.Columns.Count; i++) {
                if (diskQueue.Tables.Count > 0 && diskDp2.Tables.Count > 0) {
                    if (IsUndetermined(cpuBusy.Tables[1].Rows[0].ItemArray[i].ToString()) &&
                        IsUndetermined(cpuQueue.Tables[1].Rows[0].ItemArray[i].ToString()) &&
                        IsUndetermined(ipuBusy.Tables[1].Rows[0].ItemArray[i].ToString()) &&
                        IsUndetermined(ipuQueue.Tables[1].Rows[0].ItemArray[i].ToString()) &&
                        IsUndetermined(diskDp2.Tables[1].Rows[0].ItemArray[i].ToString()) &&
                        IsUndetermined(diskQueue.Tables[1].Rows[0].ItemArray[i].ToString())) {
                        headerColorRow[i] = "gray";
                    }
                    else if (IsCritical(cpuBusy.Tables[1].Rows[0].ItemArray[i].ToString()) ||
                        IsCritical(cpuQueue.Tables[1].Rows[0].ItemArray[i].ToString()) ||
                        IsCritical(ipuBusy.Tables[1].Rows[0].ItemArray[i].ToString()) ||
                        IsCritical(ipuQueue.Tables[1].Rows[0].ItemArray[i].ToString()) ||
                        IsCritical(diskDp2.Tables[1].Rows[0].ItemArray[i].ToString()) ||
                        IsCritical(diskQueue.Tables[1].Rows[0].ItemArray[i].ToString())) {
                        headerColorRow[i] = "red";
                    }
                    else if (IsWarning(cpuBusy.Tables[1].Rows[0].ItemArray[i].ToString()) ||
                        IsWarning(cpuQueue.Tables[1].Rows[0].ItemArray[i].ToString()) ||
                        IsWarning(ipuBusy.Tables[1].Rows[0].ItemArray[i].ToString()) ||
                        IsWarning(ipuQueue.Tables[1].Rows[0].ItemArray[i].ToString()) ||
                        IsWarning(diskDp2.Tables[1].Rows[0].ItemArray[i].ToString()) ||
                        IsWarning(diskQueue.Tables[1].Rows[0].ItemArray[i].ToString())) {
                        headerColorRow[i] = "yellow";
                    }
                }
                else if (diskQueue.Tables.Count > 0) {
                    if (IsUndetermined(cpuBusy.Tables[1].Rows[0].ItemArray[i].ToString()) &&
                        IsUndetermined(cpuQueue.Tables[1].Rows[0].ItemArray[i].ToString()) &&
                        IsUndetermined(ipuBusy.Tables[1].Rows[0].ItemArray[i].ToString()) &&
                        IsUndetermined(ipuQueue.Tables[1].Rows[0].ItemArray[i].ToString()) &&
                        IsUndetermined(diskQueue.Tables[1].Rows[0].ItemArray[i].ToString())) {
                        headerColorRow[i] = "gray";
                    }
                    else if (IsCritical(cpuBusy.Tables[1].Rows[0].ItemArray[i].ToString()) ||
                        IsCritical(cpuQueue.Tables[1].Rows[0].ItemArray[i].ToString()) ||
                        IsCritical(ipuBusy.Tables[1].Rows[0].ItemArray[i].ToString()) ||
                        IsCritical(ipuQueue.Tables[1].Rows[0].ItemArray[i].ToString()) ||
                        IsCritical(diskQueue.Tables[1].Rows[0].ItemArray[i].ToString())) {
                        headerColorRow[i] = "red";
                    }
                    else if (IsWarning(cpuBusy.Tables[1].Rows[0].ItemArray[i].ToString()) ||
                        IsWarning(cpuQueue.Tables[1].Rows[0].ItemArray[i].ToString()) ||
                        IsWarning(ipuBusy.Tables[1].Rows[0].ItemArray[i].ToString()) ||
                        IsWarning(ipuQueue.Tables[1].Rows[0].ItemArray[i].ToString()) ||
                        IsWarning(diskQueue.Tables[1].Rows[0].ItemArray[i].ToString())) {
                        headerColorRow[i] = "yellow";
                    }
                }
                else {
                    if (IsUndetermined(cpuBusy.Tables[1].Rows[0].ItemArray[i].ToString()) &&
                         IsUndetermined(cpuQueue.Tables[1].Rows[0].ItemArray[i].ToString()) &&
                         IsUndetermined(ipuBusy.Tables[1].Rows[0].ItemArray[i].ToString()) &&
                         IsUndetermined(ipuQueue.Tables[1].Rows[0].ItemArray[i].ToString()))
                    {
                        headerColorRow[i] = "gray";
                    }
                    else if (IsCritical(cpuBusy.Tables[1].Rows[0].ItemArray[i].ToString()) ||
                        IsCritical(cpuQueue.Tables[1].Rows[0].ItemArray[i].ToString()) ||
                        IsCritical(ipuBusy.Tables[1].Rows[0].ItemArray[i].ToString()) ||
                        IsCritical(ipuQueue.Tables[1].Rows[0].ItemArray[i].ToString()))
                    {
                        headerColorRow[i] = "red";
                    }
                    else if (IsWarning(cpuBusy.Tables[1].Rows[0].ItemArray[i].ToString()) ||
                        IsWarning(cpuQueue.Tables[1].Rows[0].ItemArray[i].ToString()) ||
                        IsWarning(ipuBusy.Tables[1].Rows[0].ItemArray[i].ToString()) ||
                        IsWarning(ipuQueue.Tables[1].Rows[0].ItemArray[i].ToString()))
                    {
                        headerColorRow[i] = "yellow";
                    }
                }
            }
            colorTable.Rows.Add(headerColorRow);

            DataRow cpuBusyRow = colorTable.NewRow();
            DataRow cpuQueueRow = colorTable.NewRow();
            DataRow ipuBusyRow = colorTable.NewRow();
            DataRow ipuQueueRow = colorTable.NewRow();
            DataRow diskDp2Row = colorTable.NewRow();
            DataRow diskQueueRow = colorTable.NewRow();
            DataRow storageRow = colorTable.NewRow();

            string cpuBusyColor = "", cpuQueueColor = "", ipuBusyColor = "", ipuQueueColor = "", diskDp2Color = "", diskQueueColor = "", storageColor = "";

            for (var i = 2; i < colorTable.Columns.Count; i++) {
                cpuBusyRow[i] = GetColorCode(cpuBusy.Tables[1].Rows[0].ItemArray[i].ToString());
                cpuQueueRow[i] = GetColorCode(cpuQueue.Tables[1].Rows[0].ItemArray[i].ToString());
                ipuBusyRow[i] = GetColorCode(ipuBusy.Tables[1].Rows[0].ItemArray[i].ToString());
                ipuQueueRow[i] = GetColorCode(ipuQueue.Tables[1].Rows[0].ItemArray[i].ToString());
                if (diskDp2.Tables.Count > 0) {
                    diskDp2Row[i] = GetColorCode(diskDp2.Tables[1].Rows[0].ItemArray[i].ToString());
                }
                if (diskQueue.Tables.Count > 0) {
                    diskQueueRow[i] = GetColorCode(diskQueue.Tables[1].Rows[0].ItemArray[i].ToString());
                }
                if (storage.Tables.Count > 0) {
                    storageRow[i] = GetColorCode(storage.Tables[1].Rows[0].ItemArray[i].ToString());
                }
            }

            cpuBusyColor = GetColorCode(cpuBusyRow);
            cpuQueueColor = GetColorCode(cpuQueueRow);
            ipuBusyColor = GetColorCode(ipuBusyRow);
            ipuQueueColor = GetColorCode(ipuQueueRow);
            if (diskDp2.Tables.Count > 0) {
                diskDp2Color = GetColorCode(diskDp2Row);
            }
            if (diskQueue.Tables.Count > 0) {
                diskQueueColor = GetColorCode(diskQueueRow);
            }
            if (storage.Tables.Count > 0) {
                storageColor = GetColorCode(storageRow);
            }

            cpuBusyRow[0] = cpuBusyColor;
            cpuBusyRow[1] = cpuBusyColor;
            cpuQueueRow[0] = cpuQueueColor;
            cpuQueueRow[1] = cpuQueueColor;
            ipuBusyRow[0] = ipuBusyColor;
            ipuBusyRow[1] = ipuBusyColor;
            ipuQueueRow[0] = ipuQueueColor;
            ipuQueueRow[1] = ipuQueueColor;

            colorTable.Rows.Add(cpuBusyRow);
            colorTable.Rows.Add(cpuQueueRow);
            colorTable.Rows.Add(ipuBusyRow);
            colorTable.Rows.Add(ipuQueueRow);
            if (diskDp2.Tables.Count > 0) {
                diskDp2Row[0] = diskDp2Color;
                diskDp2Row[1] = diskDp2Color;
                colorTable.Rows.Add(diskDp2Row);
            }

            if (diskQueue.Tables.Count > 0) {
                diskQueueRow[0] = diskQueueColor;
                diskQueueRow[1] = diskQueueColor;
                colorTable.Rows.Add(diskQueueRow);
            }
            if (storage.Tables.Count > 0) {
                storageRow[0] = storageColor;
                storageRow[1] = storageColor;
                colorTable.Rows.Add(storageRow);
            }
            dataSet.Tables.Add(dataTable);
            dataSet.Tables.Add(colorTable);
            return dataSet;
        }
    }
}
