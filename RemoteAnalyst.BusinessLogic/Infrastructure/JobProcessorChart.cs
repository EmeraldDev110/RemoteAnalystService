using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI.DataVisualization.Charting;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using log4net;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.BusinessLogic.Infrastructure {
    public class JobProcessorChart {
        private readonly string ConnectionString;
        private readonly string ConnectionStringSPAM;
        private readonly string ConnectionStringTrend;
        private readonly string ServerPath;

        public JobProcessorChart(string connectionString, string connectionStringTrend, string connectionStringSPAM,
            string serverPath) {
            ConnectionString = connectionString;
            ConnectionStringTrend = connectionStringTrend;
            ConnectionStringSPAM = connectionStringSPAM;
            ServerPath = serverPath;
        }
        
        public bool CheckTableFor(string systemSerial, List<string> tableNames) {
            var databaseMapService = new DatabaseMappingService(ConnectionString);
            string newConnectionString = databaseMapService.GetConnectionStringFor(systemSerial);

            var databaseCheck = new Database(newConnectionString);
            var databaseName = Helper.FindKeyName(newConnectionString, "DATABASE");
            bool exists = false;

            foreach (var tableName in tableNames) {
                exists = databaseCheck.CheckTableExists(tableName, databaseName);
                if (exists)
                    break;
            }

            return exists;
        }
        
        public string CreateDiskQueuePerInterval(DataTable diskQueue, DataTable lastWeek, DataTable lastMonth, string path, ILog log, long interval, DateTime lastWeekStartTime, DateTime lastMonthStartTime) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            var chart = new Chart();
            chart.Width = 900;
            chart.Height = 250;
            chart.Palette = ChartColorPalette.EarthTones;
            var chartarea = new ChartArea();
            chartarea.AxisY.LabelStyle.Font = new Font("Calibri", 8);
            chartarea.AxisX.LabelStyle.Font = new Font("Calibri", 8);
            chartarea.AxisY.IsLabelAutoFit = false;
            chartarea.AxisX.IsLabelAutoFit = false;
            //chartarea.AxisY.LabelStyle.Format = "{#}";

            chartarea.AxisY.MajorGrid.Enabled = true;
            chartarea.BorderWidth = 0;
            chartarea.AxisY.Title = "Disk Queue Length";
            //Display Axis every 30 mins.
            if (interval == 1800)
                chartarea.AxisX.Interval = 1;
            else {
                if (interval > 1800) {
                    var newInterval = interval / 1800;
                    chartarea.AxisX.Interval = (int)newInterval;
                }
                else {
                    var newInterval = 1800 / interval;
                    chartarea.AxisX.Interval = (int)newInterval;
                }
            }
            chartarea.AxisX.LabelStyle.Angle = -60;
            var dataDate = diskQueue.AsEnumerable().Select(x => x.Field<DateTime>("FromTimestamp")).Distinct().ToList();
            var dayCount = dataDate.Last() - dataDate.First();

            if (dayCount.TotalDays < 1) {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.MajorTickMark.Enabled = false;
                //chartarea.AxisX.IntervalType = DateTimeIntervalType.Hours;
                //chartarea.AxisX.IntervalOffsetType = DateTimeIntervalType.Hours;
                chartarea.AxisX.LabelStyle.Format = "HH:mm";
            }
            else if (dayCount.TotalDays > 1 && dayCount.TotalDays <= 7) {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.MajorTickMark.Enabled = false;
                //chartarea.AxisX.IntervalType = DateTimeIntervalType.Hours;
                //chartarea.AxisX.IntervalOffsetType = DateTimeIntervalType.Auto;
                chartarea.AxisX.LabelStyle.Format = "HH:mm";
            }
            else if (dayCount.TotalDays <= 14) {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.IntervalType = DateTimeIntervalType.Days;
                chartarea.AxisX.IntervalOffset = 0;
                chartarea.AxisX.IntervalOffsetType = DateTimeIntervalType.Auto;
                chartarea.AxisX.LabelStyle.Format = "MMM/dd/yy\ndddd";
            }
            else {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.IntervalType = DateTimeIntervalType.Days;
                chartarea.AxisX.IntervalOffset = 0;
                chartarea.AxisX.IntervalOffsetType = DateTimeIntervalType.Auto;
                chartarea.AxisX.LabelStyle.Format = "MMM/dd/yy";
            }
            chartarea.AxisY.TitleFont = new Font("Calibri", 8);
            chartarea.AxisY.MajorGrid.LineColor = Color.Silver;
            chartarea.AxisX.MajorGrid.LineColor = Color.Silver;

            //Legend format.
            chart.Legends.Clear();
            chart.Legends.Add("Default");
            chart.Legends[0].LegendStyle = LegendStyle.Row;
            chart.Legends[0].IsDockedInsideChartArea = true;
            chart.Legends[0].Alignment = StringAlignment.Center;
            chart.Legends[0].Font = new Font("Calibri", 6);
            chart.Legends[0].BorderDashStyle = ChartDashStyle.NotSet;
            chart.Legends[0].Docking = Docking.Bottom;


            try {
                var serialBusy = new Series("Peak Disk Queue");
                serialBusy.ChartType = SeriesChartType.Spline;
                serialBusy["PointWidth"] = "0.8";
                serialBusy.LegendText = "Peak Disk Queue";
                serialBusy.ToolTip = "Peak Disk Queue";
                serialBusy.Color = Color.Red;

                serialBusy.IsXValueIndexed = true;
                serialBusy.XValueType = ChartValueType.DateTime;
                serialBusy.XAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");
                serialBusy.YAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");

                //populate all the data.
                for (int x = 0; x < diskQueue.Rows.Count; x++) {
                    var dpAdjust = new DataPoint();
                    dpAdjust.XValue = Convert.ToDateTime(diskQueue.Rows[x]["FromTimestamp"]).ToOADate();
                    dpAdjust.YValues[0] = Convert.ToDouble(diskQueue.Rows[x]["QueueLength"]);
                    serialBusy.Points.Add(dpAdjust);
                }

                chart.Series.Add(serialBusy);
            }
            catch {
            }

            #region Last Week.
            try {
                if (lastWeek != null && lastWeek.Rows.Count > 0) {
                    var serialBusyWeek = new Series("Last Week");
                    serialBusyWeek.ChartType = SeriesChartType.Spline;
                    serialBusyWeek["PointWidth"] = "0.8";
                    serialBusyWeek.LegendText = "Last " + lastWeekStartTime.ToString("dddd");
                    serialBusyWeek.ToolTip = "Last " + lastWeekStartTime.ToString("dddd"); ;
                    serialBusyWeek.Color = Color.Green;

                    serialBusyWeek.IsXValueIndexed = true;
                    serialBusyWeek.XValueType = ChartValueType.DateTime;
                    serialBusyWeek.XAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");
                    serialBusyWeek.YAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");

                    //adjust the time.
                    for (int x = 0; x < diskQueue.Rows.Count; x++) {
                        var dpAdjust = new DataPoint();
                        if (lastWeek.AsEnumerable().Any(i => i.Field<DateTime>("DateTime").AddDays(7)
                                                   .Equals(Convert.ToDateTime(diskQueue.Rows[x]["FromTimestamp"])))) {
                            dpAdjust.XValue = Convert.ToDateTime(diskQueue.Rows[x]["FromTimestamp"]).ToOADate();
                            dpAdjust.YValues[0] = Convert.ToDouble(lastWeek.Rows[x]["DiskQueueLength"]);

                        }
                        else {
                            dpAdjust.XValue = Convert.ToDateTime(diskQueue.Rows[x]["FromTimestamp"]).ToOADate();
                            dpAdjust.YValues[0] = 0;
                        }
                        serialBusyWeek.Points.Add(dpAdjust);
                    }
                    chart.Series.Add(serialBusyWeek);
                }
            }
            catch {
            }
            #endregion

            #region Last Month.
            try {
                if (lastMonth != null && lastMonth.Rows.Count > 0) {

                    var serialBusyMonth = new Series("Last Month");
                    serialBusyMonth.ChartType = SeriesChartType.Spline;
                    serialBusyMonth["PointWidth"] = "0.8";
                    serialBusyMonth.LegendText = GetWeekOfMonth(lastMonthStartTime) + lastMonthStartTime.ToString("dddd") + " last month";
                    serialBusyMonth.ToolTip = GetWeekOfMonth(lastMonthStartTime) + lastMonthStartTime.ToString("dddd") + " last month";
                    serialBusyMonth.Color = Color.DeepSkyBlue;

                    serialBusyMonth.IsXValueIndexed = true;
                    serialBusyMonth.XValueType = ChartValueType.DateTime;
                    serialBusyMonth.XAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");
                    serialBusyMonth.YAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");

                    for (int x = 0; x < diskQueue.Rows.Count; x++) {
                        var dpAdjust = new DataPoint();
                        if (lastMonth.AsEnumerable().Any(i => i.Field<DateTime>("DateTime")
                                                    .Equals(Helper.GetLastMonthDate(Convert.ToDateTime(diskQueue.Rows[x]["FromTimestamp"]))))) {
                            dpAdjust.XValue = Convert.ToDateTime(diskQueue.Rows[x]["FromTimestamp"]).ToOADate();
                            dpAdjust.YValues[0] = Convert.ToDouble(lastMonth.Rows[x]["DiskQueueLength"]);

                        }
                        else {
                            dpAdjust.XValue = Convert.ToDateTime(diskQueue.Rows[x]["FromTimestamp"]).ToOADate();
                            dpAdjust.YValues[0] = 0;
                        }
                        serialBusyMonth.Points.Add(dpAdjust);
                    }
                    chart.Series.Add(serialBusyMonth);
                }
            }
            catch {
            }
            #endregion

            chart.ChartAreas.Add(chartarea);

            chart.ImageType = ChartImageType.Jpeg;
            if (!Directory.Exists(path + "TempImg\\")) {
                Directory.CreateDirectory(path + "TempImg\\");
            }
            string saveLocation = path + "TempImg\\ChartPicDiskQueue_" + DateTime.Now.Ticks + ".jpg";
            //Save the chart image.
            chart.SaveImage(saveLocation);
            return saveLocation;
        }

        public string CreatePeakProcessBusyPerInterval(Dictionary<DateTime, double> processBusy, Dictionary<DateTime, double> lastWeek, Dictionary<DateTime, double> lastMonth, string path, ILog log, long interval, DateTime lastWeekStartTime, DateTime lastMonthStartTime) {

            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            var chart = new Chart();
            chart.Width = 900;
            chart.Height = 250;
            chart.Palette = ChartColorPalette.EarthTones;
            var chartarea = new ChartArea();
            chartarea.AxisY.LabelStyle.Font = new Font("Calibri", 8);
            chartarea.AxisX.LabelStyle.Font = new Font("Calibri", 8);
            chartarea.AxisY.IsLabelAutoFit = false;
            chartarea.AxisX.IsLabelAutoFit = false;
            chartarea.AxisY.LabelStyle.Format = "{#}%";

            chartarea.AxisY.MajorGrid.Enabled = true;
            chartarea.BorderWidth = 0;
            chartarea.AxisY.Title = "Percent Process Busy";
            //Display Axis every 30 mins.
            if (interval == 1800)
                chartarea.AxisX.Interval = 1;
            else {
                if (interval > 1800) {
                    var newInterval = interval / 1800;
                    chartarea.AxisX.Interval = (int)newInterval;
                }
                else {
                    var newInterval = 1800 / interval;
                    chartarea.AxisX.Interval = (int)newInterval;
                }
            }
            chartarea.AxisX.LabelStyle.Angle = -60;
            var dayCount = processBusy.Last().Key - processBusy.First().Key;

            if (dayCount.TotalDays < 1) {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.MajorTickMark.Enabled = false;
                //chartarea.AxisX.IntervalType = DateTimeIntervalType.Hours;
                //chartarea.AxisX.IntervalOffsetType = DateTimeIntervalType.Hours;
                chartarea.AxisX.LabelStyle.Format = "HH:mm";
            }
            else if (dayCount.TotalDays > 1 && dayCount.TotalDays <= 7) {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.MajorTickMark.Enabled = false;
                //chartarea.AxisX.IntervalType = DateTimeIntervalType.Hours;
                //chartarea.AxisX.IntervalOffsetType = DateTimeIntervalType.Auto;
                chartarea.AxisX.LabelStyle.Format = "HH:mm";
            }
            else if (dayCount.TotalDays <= 14) {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.IntervalType = DateTimeIntervalType.Days;
                chartarea.AxisX.IntervalOffset = 0;
                chartarea.AxisX.IntervalOffsetType = DateTimeIntervalType.Auto;
                chartarea.AxisX.LabelStyle.Format = "MMM/dd/yy\ndddd";
            }
            else {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.IntervalType = DateTimeIntervalType.Days;
                chartarea.AxisX.IntervalOffset = 0;
                chartarea.AxisX.IntervalOffsetType = DateTimeIntervalType.Auto;
                chartarea.AxisX.LabelStyle.Format = "MMM/dd/yy";
            }
            chartarea.AxisY.TitleFont = new Font("Calibri", 8);
            chartarea.AxisY.MajorGrid.LineColor = Color.Silver;
            chartarea.AxisX.MajorGrid.LineColor = Color.Silver;

            //Legend format.
            chart.Legends.Clear();
            chart.Legends.Add("Default");
            chart.Legends[0].LegendStyle = LegendStyle.Row;
            chart.Legends[0].IsDockedInsideChartArea = true;
            chart.Legends[0].Alignment = StringAlignment.Center;
            chart.Legends[0].Font = new Font("Calibri", 6);
            chart.Legends[0].BorderDashStyle = ChartDashStyle.NotSet;
            chart.Legends[0].Docking = Docking.Bottom;

            try {
                var serialBusy = new Series("Peak Process Queue");
                serialBusy.ChartType = SeriesChartType.Spline;
                serialBusy["PointWidth"] = "0.8";
                serialBusy.LegendText = "Percent Process Busy";
                serialBusy.ToolTip = "Percent Process Busy";
                serialBusy.Color = Color.Red;

                serialBusy.IsXValueIndexed = true;
                serialBusy.XValueType = ChartValueType.DateTime;
                serialBusy.XAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");
                serialBusy.YAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");

                //populate all the data.
                foreach (var value in processBusy) {
                    var dpAdjust = new DataPoint();
                    dpAdjust.XValue = Convert.ToDateTime(value.Key).ToOADate();
                    dpAdjust.YValues[0] = Convert.ToDouble(value.Value);
                    serialBusy.Points.Add(dpAdjust);
                }

                chart.Series.Add(serialBusy);
            }
            catch {
            }
            chart.ChartAreas.Add(chartarea);


            #region Last Week.
            try {
                if (lastWeek != null && lastWeek.Count > 0) {
                    var serialBusyWeek = new Series("Last Week");
                    serialBusyWeek.ChartType = SeriesChartType.Spline;
                    serialBusyWeek["PointWidth"] = "0.8";
                    serialBusyWeek.LegendText = "Last " + lastWeekStartTime.ToString("dddd"); ;
                    serialBusyWeek.ToolTip = "Last " + lastWeekStartTime.ToString("dddd"); ;
                    serialBusyWeek.Color = Color.Green;

                    serialBusyWeek.IsXValueIndexed = true;
                    serialBusyWeek.XValueType = ChartValueType.DateTime;
                    serialBusyWeek.XAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");
                    serialBusyWeek.YAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");

                    foreach (var value in processBusy) {
                        var dpAdjust = new DataPoint();
                        if (lastWeek.Any(i => i.Key.AddDays(7).Equals(value.Key))) {
                            dpAdjust.XValue = Convert.ToDateTime(value.Key).ToOADate();
                            dpAdjust.YValues[0] = lastWeek.Where(i => i.Key.AddDays(7).Equals(value.Key)).Select(x => x.Value).First();
                        }
                        else {
                            dpAdjust.XValue = Convert.ToDateTime(value.Key).ToOADate();
                            dpAdjust.YValues[0] = 0;
                        }
                        serialBusyWeek.Points.Add(dpAdjust);
                    }
                    chart.Series.Add(serialBusyWeek);
                }
            }
            catch {
            }
            #endregion

            #region Last Month.
            try {
                if (lastMonth != null && lastMonth.Count > 0) {
                    var serialBusyMonth = new Series("Last Month");
                    serialBusyMonth.ChartType = SeriesChartType.Spline;
                    serialBusyMonth["PointWidth"] = "0.8";
                    serialBusyMonth.LegendText = GetWeekOfMonth(lastMonthStartTime) + lastMonthStartTime.ToString("dddd") + " last month";
                    serialBusyMonth.ToolTip = GetWeekOfMonth(lastMonthStartTime) + lastMonthStartTime.ToString("dddd") + " last month";
                    serialBusyMonth.Color = Color.DeepSkyBlue;

                    serialBusyMonth.IsXValueIndexed = true;
                    serialBusyMonth.XValueType = ChartValueType.DateTime;
                    serialBusyMonth.XAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");
                    serialBusyMonth.YAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");

                    foreach (var value in processBusy) {
                        var dpAdjust = new DataPoint();
                        if (lastMonth.Any(i => i.Key.Equals(Helper.GetLastMonthDate(value.Key)))) {
                            dpAdjust.XValue = Convert.ToDateTime(value.Key).ToOADate();
                            dpAdjust.YValues[0] = lastMonth.Where(i => i.Key.Equals(Helper.GetLastMonthDate(value.Key))).Select(x => x.Value).First();
                        }
                        else {
                            dpAdjust.XValue = Convert.ToDateTime(value.Key).ToOADate();
                            dpAdjust.YValues[0] = 0;
                        }
                        serialBusyMonth.Points.Add(dpAdjust);
                    }
                    chart.Series.Add(serialBusyMonth);
                }
            }
            catch {
            }
            #endregion

            chart.ImageType = ChartImageType.Jpeg;
            if (!Directory.Exists(path + "TempImg\\")) {
                Directory.CreateDirectory(path + "TempImg\\");
            }
            string saveLocation = path + "TempImg\\ChartPicProcessBusy_" + DateTime.Now.Ticks + ".jpg";
            //Save the chart image.
            chart.SaveImage(saveLocation);
            return saveLocation;
        }

        public string CreatePeakProcessQueuePerInterval(Dictionary<DateTime, double> processBusy, Dictionary<DateTime, double> lastWeek, Dictionary<DateTime, double> lastMonth, string path, ILog log, long interval, DateTime lastWeekStartTime, DateTime lastMonthStartTime) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            var chart = new Chart();
            chart.Width = 900;
            chart.Height = 250;
            chart.Palette = ChartColorPalette.EarthTones;
            var chartarea = new ChartArea();
            chartarea.AxisY.LabelStyle.Font = new Font("Calibri", 8);
            chartarea.AxisX.LabelStyle.Font = new Font("Calibri", 8);
            chartarea.AxisY.IsLabelAutoFit = false;
            chartarea.AxisX.IsLabelAutoFit = false;
            //chartarea.AxisY.LabelStyle.Format = "{#}";

            chartarea.AxisY.MajorGrid.Enabled = true;
            chartarea.BorderWidth = 0;
            chartarea.AxisY.Title = "Process Receive Queue Length";
            //Display Axis every 30 mins.
            if (interval == 1800)
                chartarea.AxisX.Interval = 1;
            else {
                if (interval > 1800) {
                    var newInterval = interval / 1800;
                    chartarea.AxisX.Interval = (int)newInterval;
                }
                else {
                    var newInterval = 1800 / interval;
                    chartarea.AxisX.Interval = (int)newInterval;
                }
            }
            chartarea.AxisX.LabelStyle.Angle = -60;
            var dayCount = processBusy.Last().Key - processBusy.First().Key;

            if (dayCount.TotalDays < 1) {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.MajorTickMark.Enabled = false;
                //chartarea.AxisX.IntervalType = DateTimeIntervalType.Hours;
                //chartarea.AxisX.IntervalOffsetType = DateTimeIntervalType.Hours;
                chartarea.AxisX.LabelStyle.Format = "HH:mm";
            }
            else if (dayCount.TotalDays > 1 && dayCount.TotalDays <= 7) {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.MajorTickMark.Enabled = false;
                //chartarea.AxisX.IntervalType = DateTimeIntervalType.Hours;
                //chartarea.AxisX.IntervalOffsetType = DateTimeIntervalType.Auto;
                chartarea.AxisX.LabelStyle.Format = "HH:mm";
            }
            else if (dayCount.TotalDays <= 14) {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.IntervalType = DateTimeIntervalType.Days;
                chartarea.AxisX.IntervalOffset = 0;
                chartarea.AxisX.IntervalOffsetType = DateTimeIntervalType.Auto;
                chartarea.AxisX.LabelStyle.Format = "MMM/dd/yy\ndddd";
            }
            else {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.IntervalType = DateTimeIntervalType.Days;
                chartarea.AxisX.IntervalOffset = 0;
                chartarea.AxisX.IntervalOffsetType = DateTimeIntervalType.Auto;
                chartarea.AxisX.LabelStyle.Format = "MMM/dd/yy";
            }
            chartarea.AxisY.TitleFont = new Font("Calibri", 8);
            chartarea.AxisY.MajorGrid.LineColor = Color.Silver;
            chartarea.AxisX.MajorGrid.LineColor = Color.Silver;

            //Legend format.
            chart.Legends.Clear();
            chart.Legends.Add("Default");
            chart.Legends[0].LegendStyle = LegendStyle.Row;
            chart.Legends[0].IsDockedInsideChartArea = true;
            chart.Legends[0].Alignment = StringAlignment.Center;
            chart.Legends[0].Font = new Font("Calibri", 6);
            chart.Legends[0].BorderDashStyle = ChartDashStyle.NotSet;
            chart.Legends[0].Docking = Docking.Bottom;

            try {
                var serialBusy = new Series("Peak Process Queue");
                serialBusy.ChartType = SeriesChartType.Spline;
                serialBusy["PointWidth"] = "0.8";
                serialBusy.LegendText = "Process Receive Queue Length";
                serialBusy.ToolTip = "Process Receive Queue Length";
                serialBusy.Color = Color.Red;

                serialBusy.IsXValueIndexed = true;
                serialBusy.XValueType = ChartValueType.DateTime;
                serialBusy.XAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");
                serialBusy.YAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");

                //populate all the data.
                foreach (var value in processBusy) {
                    var dpAdjust = new DataPoint();
                    dpAdjust.XValue = Convert.ToDateTime(value.Key).ToOADate();
                    dpAdjust.YValues[0] = Convert.ToDouble(value.Value);
                    serialBusy.Points.Add(dpAdjust);
                }

                chart.Series.Add(serialBusy);
            }
            catch {
            }


            #region Last Week.
            try {
                if (lastWeek != null && lastWeek.Count > 0) {
                    var serialBusyWeek = new Series("Last Week");
                    serialBusyWeek.ChartType = SeriesChartType.Spline;
                    serialBusyWeek["PointWidth"] = "0.8";
                    serialBusyWeek.LegendText = "Last " + lastWeekStartTime.ToString("dddd");
                    serialBusyWeek.ToolTip = "Last " + lastWeekStartTime.ToString("dddd");
                    serialBusyWeek.Color = Color.Green;

                    serialBusyWeek.IsXValueIndexed = true;
                    serialBusyWeek.XValueType = ChartValueType.DateTime;
                    serialBusyWeek.XAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");
                    serialBusyWeek.YAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");

                    foreach (var value in processBusy) {
                        var dpAdjust = new DataPoint();
                        if (lastWeek.Any(i => i.Key.AddDays(7).Equals(value.Key))) {
                            dpAdjust.XValue = Convert.ToDateTime(value.Key).ToOADate();
                            dpAdjust.YValues[0] = lastWeek.Where(i => i.Key.AddDays(7).Equals(value.Key)).Select(x => x.Value).First();
                        }
                        else {
                            dpAdjust.XValue = Convert.ToDateTime(value.Key).ToOADate();
                            dpAdjust.YValues[0] = 0;
                        }
                        serialBusyWeek.Points.Add(dpAdjust);
                    }
                    chart.Series.Add(serialBusyWeek);
                }
            }
            catch {
            }
            #endregion

            #region Last Month.
            try {
                if (lastMonth != null && lastMonth.Count > 0) {
                    var serialBusyMonth = new Series("Last Month");
                    serialBusyMonth.ChartType = SeriesChartType.Spline;
                    serialBusyMonth["PointWidth"] = "0.8";
                    serialBusyMonth.LegendText = GetWeekOfMonth(lastMonthStartTime) + lastMonthStartTime.ToString("dddd") + " last month";
                    serialBusyMonth.ToolTip = GetWeekOfMonth(lastMonthStartTime) + lastMonthStartTime.ToString("dddd") + " last month";
                    serialBusyMonth.Color = Color.DeepSkyBlue;

                    serialBusyMonth.IsXValueIndexed = true;
                    serialBusyMonth.XValueType = ChartValueType.DateTime;
                    serialBusyMonth.XAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");
                    serialBusyMonth.YAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");

                    foreach (var value in processBusy) {
                        var dpAdjust = new DataPoint();
                        if (lastMonth.Any(i => i.Key.Equals(Helper.GetLastMonthDate(value.Key)))) {
                            dpAdjust.XValue = Convert.ToDateTime(value.Key).ToOADate();
                            dpAdjust.YValues[0] = lastMonth.Where(i => i.Key.Equals(Helper.GetLastMonthDate(value.Key))).Select(x => x.Value).First();
                        }
                        else {
                            dpAdjust.XValue = Convert.ToDateTime(value.Key).ToOADate();
                            dpAdjust.YValues[0] = 0;
                        }
                        serialBusyMonth.Points.Add(dpAdjust);
                    }
                    chart.Series.Add(serialBusyMonth);
                }
            }
            catch {
            }
            #endregion

            chart.ChartAreas.Add(chartarea);

            chart.ImageType = ChartImageType.Jpeg;
            if (!Directory.Exists(path + "TempImg\\")) {
                Directory.CreateDirectory(path + "TempImg\\");
            }
            string saveLocation = path + "TempImg\\ChartPicProcessQueue_" + DateTime.Now.Ticks + ".jpg";
            //Save the chart image.
            chart.SaveImage(saveLocation);
            return saveLocation;
        }

        public string CreateProcessAbortPerInterval(Dictionary<DateTime, ProcessTransaction> processBusy, Dictionary<DateTime, double> lastWeek, Dictionary<DateTime, double> lastMonth, string path, ILog log, long interval, DateTime lastWeekStartTime, DateTime lastMonthStartTime) {
            var chart = new Chart();
            chart.Width = 900;
            chart.Height = 250;
            chart.Palette = ChartColorPalette.EarthTones;
            var chartarea = new ChartArea();
            chartarea.AxisY.LabelStyle.Font = new Font("Calibri", 8);
            chartarea.AxisX.LabelStyle.Font = new Font("Calibri", 8);
            chartarea.AxisY.IsLabelAutoFit = false;
            chartarea.AxisX.IsLabelAutoFit = false;
            chartarea.AxisY.LabelStyle.Format = "#,0";

            chartarea.AxisY.MajorGrid.Enabled = true;
            chartarea.BorderWidth = 0;
            chartarea.AxisY.Title = "Transactions";

            //AxisY2

            chartarea.AxisY2.Maximum = Double.NaN;

            chartarea.AxisY2.Enabled = AxisEnabled.True;
            chartarea.AxisY2.Minimum = 0;
            chartarea.AxisY2.RoundAxisValues();
            chartarea.AxisY2.LabelStyle.Font = new Font("Calibri", 8);
            chartarea.AxisY2.TitleFont = new Font("Calibri", 8);
            chartarea.AxisY2.IsLabelAutoFit = false;
            chartarea.AxisY2.Title = "Abort (%)";
            chartarea.AxisY2.MajorGrid.Enabled = false;


            //Display Axis every 30 mins.
            if (interval == 1800)
                chartarea.AxisX.Interval = 1;
            else {
                if (interval > 1800) {
                    var newInterval = interval / 1800;
                    chartarea.AxisX.Interval = (int)newInterval;
                }
                else {
                    var newInterval = 1800 / interval;
                    chartarea.AxisX.Interval = (int)newInterval;
                }
            }
            chartarea.AxisX.LabelStyle.Angle = -60;
            var dayCount = processBusy.Last().Key - processBusy.First().Key;

            if (dayCount.TotalDays < 1) {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.MajorTickMark.Enabled = false;
                //chartarea.AxisX.IntervalType = DateTimeIntervalType.Hours;
                //chartarea.AxisX.IntervalOffsetType = DateTimeIntervalType.Hours;
                chartarea.AxisX.LabelStyle.Format = "HH:mm";
            }
            else if (dayCount.TotalDays > 1 && dayCount.TotalDays <= 7) {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.MajorTickMark.Enabled = false;
                //chartarea.AxisX.IntervalType = DateTimeIntervalType.Hours;
                //chartarea.AxisX.IntervalOffsetType = DateTimeIntervalType.Auto;
                chartarea.AxisX.LabelStyle.Format = "HH:mm";
            }
            else if (dayCount.TotalDays <= 14) {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.IntervalType = DateTimeIntervalType.Days;
                chartarea.AxisX.IntervalOffset = 0;
                chartarea.AxisX.IntervalOffsetType = DateTimeIntervalType.Auto;
                chartarea.AxisX.LabelStyle.Format = "MMM/dd/yy\ndddd";
            }
            else {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.IntervalType = DateTimeIntervalType.Days;
                chartarea.AxisX.IntervalOffset = 0;
                chartarea.AxisX.IntervalOffsetType = DateTimeIntervalType.Auto;
                chartarea.AxisX.LabelStyle.Format = "MMM/dd/yy";
            }
            chartarea.AxisY.TitleFont = new Font("Calibri", 8);
            chartarea.AxisY.MajorGrid.LineColor = Color.Silver;
            chartarea.AxisX.MajorGrid.LineColor = Color.Silver;

            //Legend format.
            chart.Legends.Clear();
            chart.Legends.Add("Default");
            chart.Legends[0].LegendStyle = LegendStyle.Row;
            chart.Legends[0].IsDockedInsideChartArea = true;
            chart.Legends[0].Alignment = StringAlignment.Center;
            chart.Legends[0].Font = new Font("Calibri", 6);
            chart.Legends[0].BorderDashStyle = ChartDashStyle.NotSet;
            chart.Legends[0].Docking = Docking.Bottom;

            try {
                var serialBusy = new Series("Aborted");
                serialBusy.ChartType = SeriesChartType.StackedColumn;
                serialBusy["PointWidth"] = "0.8";
                serialBusy.LegendText = "Aborted";
                serialBusy.ToolTip = "Aborted";
                serialBusy.Color = Color.Red;

                serialBusy.IsXValueIndexed = true;
                serialBusy.XValueType = ChartValueType.DateTime;
                serialBusy.XAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");
                serialBusy.YAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");

                //populate all the data.
                foreach (var value in processBusy) {
                    var dpAdjust = new DataPoint();
                    dpAdjust.XValue = Convert.ToDateTime(value.Key).ToOADate();
                    dpAdjust.YValues[0] = Convert.ToDouble(value.Value.AbortTrans);
                    serialBusy.Points.Add(dpAdjust);
                }

                chart.Series.Add(serialBusy);
            }
            catch {
            }

            try {
                var serialBusy = new Series("Completed");
                serialBusy.ChartType = SeriesChartType.StackedColumn;
                serialBusy["PointWidth"] = "0.8";
                serialBusy.LegendText = "Completed";
                serialBusy.ToolTip = "Completed";
                serialBusy.Color = Color.Green;

                serialBusy.IsXValueIndexed = true;
                serialBusy.XValueType = ChartValueType.DateTime;
                serialBusy.XAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");
                serialBusy.YAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");

                //populate all the data.
                foreach (var value in processBusy) {
                    var dpAdjust = new DataPoint();
                    dpAdjust.XValue = Convert.ToDateTime(value.Key).ToOADate();
                    dpAdjust.YValues[0] = Convert.ToDouble(value.Value.BeginTrans - value.Value.AbortTrans);
                    serialBusy.Points.Add(dpAdjust);
                }

                chart.Series.Add(serialBusy);
            }
            catch {
            }

            try {
                var serialBusy = new Series("AbourtPercent");
                serialBusy.ChartType = SeriesChartType.Spline;
                serialBusy["PointWidth"] = "0.8";
                serialBusy.LegendText = "% Aborted";
                serialBusy.ToolTip = "% Aborted";
                serialBusy.Color = Color.DeepSkyBlue;

                serialBusy.IsXValueIndexed = true;
                serialBusy.XValueType = ChartValueType.DateTime;
                serialBusy.XAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");
                serialBusy.YAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Secondary");

                //populate all the data.
                foreach (var value in processBusy) {
                    var dpAdjust = new DataPoint();
                    dpAdjust.XValue = Convert.ToDateTime(value.Key).ToOADate();
                    dpAdjust.YValues[0] = Convert.ToDouble((value.Value.AbortTrans / value.Value.BeginTrans) * 100);
                    serialBusy.Points.Add(dpAdjust);
                }

                chart.Series.Add(serialBusy);
            }
            catch {
            }

            try {
                if (lastWeek != null && lastWeek.Count > 0) {
                    var serialBusyWeek = new Series("Last Week");
                    serialBusyWeek.ChartType = SeriesChartType.Spline;
                    serialBusyWeek["PointWidth"] = "0.8";
                    serialBusyWeek.LegendText = "Last " + lastWeekStartTime.ToString("dddd");
                    serialBusyWeek.ToolTip = "Last " + lastWeekStartTime.ToString("dddd");
                    serialBusyWeek.Color = Color.DarkOrange;

                    serialBusyWeek.IsXValueIndexed = true;
                    serialBusyWeek.XValueType = ChartValueType.DateTime;
                    serialBusyWeek.XAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");
                    serialBusyWeek.YAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Secondary");

                    foreach (var value in processBusy) {
                        var dpAdjust = new DataPoint();
                        if (lastWeek.Any(i => i.Key.AddDays(7).Equals(value.Key))) {
                            dpAdjust.XValue = Convert.ToDateTime(value.Key).ToOADate();
                            dpAdjust.YValues[0] = lastWeek.Where(i => i.Key.AddDays(7).Equals(value.Key)).Select(x => x.Value).First();
                        }
                        else {
                            dpAdjust.XValue = Convert.ToDateTime(value.Key).ToOADate();
                            dpAdjust.YValues[0] = 0;
                        }
                        serialBusyWeek.Points.Add(dpAdjust);
                    }

                    chart.Series.Add(serialBusyWeek);
                }
            }
            catch {
            }

            try {
                if (lastMonth != null && lastMonth.Count > 0) {
                    var serialBusyMonth = new Series("Last Month");
                    serialBusyMonth.ChartType = SeriesChartType.Spline;
                    serialBusyMonth["PointWidth"] = "0.8";
                    serialBusyMonth.LegendText = GetWeekOfMonth(lastMonthStartTime) + lastMonthStartTime.ToString("dddd") + " last month";
                    serialBusyMonth.ToolTip = GetWeekOfMonth(lastMonthStartTime) + lastMonthStartTime.ToString("dddd") + " last month";
                    serialBusyMonth.Color = Color.Magenta;

                    serialBusyMonth.IsXValueIndexed = true;
                    serialBusyMonth.XValueType = ChartValueType.DateTime;
                    serialBusyMonth.XAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");
                    serialBusyMonth.YAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Secondary");

                    foreach (var value in processBusy) {
                        var dpAdjust = new DataPoint();
                        if (lastMonth.Any(i => i.Key.Equals(Helper.GetLastMonthDate(value.Key)))) {
                            dpAdjust.XValue = Convert.ToDateTime(value.Key).ToOADate();
                            dpAdjust.YValues[0] = lastMonth.Where(i => i.Key.Equals(Helper.GetLastMonthDate(value.Key))).Select(x => x.Value).First();
                        }
                        else {
                            dpAdjust.XValue = Convert.ToDateTime(value.Key).ToOADate();
                            dpAdjust.YValues[0] = 0;
                        }
                        serialBusyMonth.Points.Add(dpAdjust);
                    }

                    chart.Series.Add(serialBusyMonth);
                }
            }
            catch {
            }

            chart.ChartAreas.Add(chartarea);

            chart.ImageType = ChartImageType.Jpeg;
            if (!Directory.Exists(path + "TempImg\\")) {
                Directory.CreateDirectory(path + "TempImg\\");
            }
            string saveLocation = path + "TempImg\\ChartPicProcessAbort_" + DateTime.Now.Ticks + ".jpg";
            //Save the chart image.
            chart.SaveImage(saveLocation);
            return saveLocation;
        }

        public string CreateObjectsGrid(DataSet grids, string encryptSystemSerial, DateTime startTime, DateTime stopTime, bool isLocalAnalyst, string websiteAddress) {
            StringBuilder sb = new StringBuilder();
            sb.Append("<table style='width:100%;font-size:12px;font-family:Calibri;text-align:center' cellpadding='0' cellspacing='0' border='1'>");
            //Header
            sb.Append("<tr>");
            for (int j = 1; j < grids.Tables[0].Columns.Count; j++) {
                if (j < 2) {
                    sb.Append("<th colspan='2'>");
                }
                else {
                    //string headerBorderStyle = j == 2 ? "border-top: 1px solid black; border-left: 1px solid black" : j == grids.Tables[0].Columns.Count - 1 ? "border-top: 1px solid black; border-right: 1px solid black" : "border-top: 1px solid black;";
                    string headerBorderStyle = j == 2 ? "border-right:0;" : j == grids.Tables[0].Columns.Count - 1 ? "border-left:0;" : "border-left:0;border-right:0;";
                    string headerColor = grids.Tables[1].Rows[0][j].ToString();
                    if (headerColor.Equals("")) {
                        headerColor = "green";
                    }
                    sb.Append("<th style='width:30px;" + headerBorderStyle + "background-color:" + headerColor + ";'>");
                    sb.Append(grids.Tables[0].Columns[j].ColumnName);
                }
                sb.Append("</th>" + Environment.NewLine);
            }
            sb.Append("</tr>" + Environment.NewLine);
            for (int i = 0; i < grids.Tables[0].Rows.Count; i++) {
                if (grids.Tables[0].Rows[i][0].ToString() != "Storage") {
                    sb.Append("<tr>");
                    for (int j = 0; j < grids.Tables[0].Columns.Count; j++) {
                        var value = grids.Tables[0].Rows[i][j].ToString();
                        string cellColor = grids.Tables[1].Rows[i + 1][j].ToString().Trim();
                        if (j < 2) {
                            sb.Append("<td style='width:60px;");
                            if (i % 2 == 0 || j % 2 == 1) {
								if (cellColor.Length > 0) {
									sb.Append("background-color:" + cellColor + ";");
								}
								else {
									sb.Append("background-color: green;");
								}
                            }
                            sb.Append("'>");
                            if (i % 2 == 0 || j % 2 == 1) {
                                //Add hyperlinks
                                string graphName = "";
                                if (value.Equals("Busy") && grids.Tables[0].Rows[i][0].ToString() == "CPU") {
                                    graphName = "CpuBusyGraph";
                                }
                                else if (value.Equals("Queue") && grids.Tables[0].Rows[i][0].ToString() == "CPU")
                                    graphName = "CPUQueueLengthGraph";
                                else if (value.Equals("Busy") && grids.Tables[0].Rows[i][0].ToString() == "IPU") {
                                    graphName = "IpuBusyGraph";
                                }
                                else if (value.Equals("Queue") && grids.Tables[0].Rows[i][0].ToString() == "IPU") {
                                    graphName = "IPUQueueLengthGraph";
                                }
                                else if (value.Equals("Queue") && grids.Tables[0].Rows[i][0].ToString() == "Disk") {
                                    graphName = "Top20Disks";
                                }
								if (!value.Equals("DP2 Busy")) {
									if (j == 1) sb.Append("<a style='color:blue;position:relative;display:block;' href='#" + graphName + "'>");
								}
                                sb.Append(value);
								if (!value.Equals("DP2 Busy")) {
									if (j == 1) sb.Append("</a>");
								}
                            }
                        }
                        else {
                            if (cellColor.Equals("")) {
                                cellColor = "green";
                            }
                            sb.Append("<td style='width:30px;background-color:" + cellColor + ";'>");
                            if (value.Equals("0") || value.Equals("-1"))
                            {
                                value = "";
                                sb.Append(value);
                            }
                            else {
                                //Build StartTime.
                                var columnName = grids.Tables[0].Columns[j].ColumnName;
                                var tempDateTime = Convert.ToDateTime(startTime.ToString("yyyy-MM-dd") + " " + columnName + ":00:00");

                                if (tempDateTime >= startTime && tempDateTime <= stopTime) { }
                                else {
                                    tempDateTime = Convert.ToDateTime(stopTime.ToString("yyyy-MM-dd") + " " + columnName + ":00:00");
                                }

                                var encrypt = new Decrypt();
                                var encryptStartTime = encrypt.strDESEncrypt(tempDateTime.ToString("yyyy-MM-dd HH:mm"));

                                var exceptionEntiry = "";

                                if (grids.Tables[0].Rows[i][0].ToString() == "CPU")
                                    exceptionEntiry = "CPU";
                                else if (grids.Tables[0].Rows[i][0].ToString() == "IPU")
                                    exceptionEntiry = "IPU";
                                else if (grids.Tables[0].Rows[i][0].ToString() == "Disk")
                                    exceptionEntiry = "Disk";

                                if (!isLocalAnalyst) {
                                    sb.Append("<a style='color:blue;position:relative;display:block;' href='" + websiteAddress + "/EmailLogin.aspx?User=@User&System=" +
                                              encryptSystemSerial + "&StartDate=" + encryptStartTime + "&ExceptionEntity=" + exceptionEntiry + "'>" + value + "</a>");
                                }
                                else {
                                    sb.Append("<a style='color:blue;position:relative;display:block;' href='" + websiteAddress + "/EmailLogin.aspx?User=@User&System=" +
                                              encryptSystemSerial + "&StartDate=" + encryptStartTime + "&ExceptionEntity=" + exceptionEntiry + "'>" + value + "</a>");
                                }
                            }
                        }
                        sb.Append("</td>" + Environment.NewLine);
                    }
                    sb.Append("</tr>" + Environment.NewLine);
                }
                else {
                    sb.Append("<tr>");
                    for (int j = 0; j < 3; j++) {
                        var value = grids.Tables[0].Rows[i][j].ToString();
                        string cellColor = grids.Tables[1].Rows[i + 1][j].ToString().Trim();
                        if (j < 2) {
                            sb.Append("<td style='width:60px;");
							if (cellColor.Length > 0) {
								sb.Append("background-color:" + cellColor + ";");
							}
							else {
								sb.Append("background-color: green;");
							}
                            sb.Append("'>");

							if (value == "Used %" && cellColor.Length > 0) {
								string graphName = "Top20Storage";
								//Add hyperlinks
								sb.Append("<a style='color:blue;position:relative;display:block;' href='#" + graphName + "'>");
								sb.Append(value);
								sb.Append("</a>");
							}
							else {
								sb.Append(value);
							}
                        }
                        else {
                            value = grids.Tables[0].Rows[i][2].ToString();
                            if (cellColor.Equals("")) {
                                cellColor = "green";
                            }
                            sb.Append("<td colspan='" + (grids.Tables[0].Columns.Count - 2) + "' style='width:30px;background-color:" + cellColor + ";padding-left:5px;text-align:left;'>");
                            if (value.Equals("0") || value.Equals("-1")) 
                            { 
                                value = "";
                                sb.Append(value);
                            }
                            else {
                                //Build StartTime.
                                var columnName = grids.Tables[0].Columns[j].ColumnName;
                                var tempDateTime = Convert.ToDateTime(startTime.ToString("yyyy-MM-dd") + " " + columnName + ":00:00");

                                if (tempDateTime >= startTime && tempDateTime <= stopTime) { }
                                else {
                                    tempDateTime = Convert.ToDateTime(stopTime.ToString("yyyy-MM-dd") + " " + columnName + ":00:00");
                                }

                                var encrypt = new Decrypt();
                                var encryptStartTime = encrypt.strDESEncrypt(tempDateTime.ToString("yyyy-MM-dd HH:mm"));

                                if (!isLocalAnalyst) {
                                    sb.Append("<a style='color: blue;position:relative;display:block;' href='" + websiteAddress + "/EmailLogin.aspx?User=@User&System=" +
                                              encryptSystemSerial + "&StartDate=" + encryptStartTime + "&ExceptionEntity=Storage'>" + value + "</a>");
                                }
                                else {
                                    sb.Append("<a style='color: blue;position:relative;display:block;' href='" + websiteAddress + "/EmailLogin.aspx?User=@User&System=" +
                                              encryptSystemSerial + "&StartDate=" + encryptStartTime + "&ExceptionEntity=Storage'>" + value + "</a>");
                                }
                            }

                        }
                        sb.Append("</td>" + Environment.NewLine);
                    }
                    sb.Append("</tr>" + Environment.NewLine);
                }
            }
            sb.Append("</table><br>");
            return sb.ToString();
        }

        public string CreateChartPerInterval(string systemSerial, DateTime startDate, DateTime endDate, string path, ILog log, long interval, ref bool hourDrop, ref List<System.DateTime[]> hourDropPeriods) {
            //IR 6466
            log.Info("On CreateChartPerInterval");
            
            string alerttxt = string.Empty;
            var sysUnrated = new DailySysUnratedService(ConnectionStringSPAM);
            if ((Convert.ToDateTime(endDate).Subtract(startDate)).Days >= 1) {
                DataSet dset = sysUnrated.GetDataDateFor(1, startDate, endDate, systemSerial);
                if (dset.Tables["Interval"].Rows.Count == 0) {
                    return alerttxt;
                }
                var reportDate = new DateTime[dset.Tables["Interval"].Rows.Count];
                for (int i = 0; i < dset.Tables["Interval"].Rows.Count; i++) {
                    reportDate[i] = Convert.ToDateTime(dset.Tables["Interval"].Rows[i]["DataDate"].ToString());
                }
                string strReturn = CreateIntervalCPUBusyDundasChart(systemSerial, reportDate, startDate, endDate, path, 
                                                                        interval, ref hourDrop, ref hourDropPeriods);
                strReturn += ",";
                strReturn += CreateIntervalCPUQueueDundasChart(systemSerial, reportDate, startDate, endDate, path, interval);
                return strReturn;
            }
            else {
                var reportDate = new DateTime[1];
                reportDate[0] = Convert.ToDateTime(startDate);

                string strReturn = "";
                log.Info("Before CreatePerCPUDundasChart");
                
                strReturn = CreateIntervalCPUBusyDundasChart(systemSerial, reportDate, startDate, endDate, path, 
                                                                interval, ref hourDrop, ref hourDropPeriods);
                strReturn += ",";
                strReturn += CreateIntervalCPUQueueDundasChart(systemSerial, reportDate, startDate, endDate, path, interval);
                return strReturn;
            }
        }

        public string CreateIPUChartPerInterval(string systemSerial, DateTime startDate, DateTime endDate, string path, ILog log, long interval) {
            //IR 6466
            log.Info("On CreateChartPerInterval");
            

            string alerttxt = string.Empty;
            var sysUnrated = new DailySysUnratedService(ConnectionStringSPAM);
            if ((Convert.ToDateTime(endDate).Subtract(startDate)).Days >= 1) {
                DataSet dset = sysUnrated.GetDataDateFor(1, startDate, endDate, systemSerial);
                if (dset.Tables["Interval"].Rows.Count == 0) {
                    return alerttxt;
                }
                var reportDate = new DateTime[dset.Tables["Interval"].Rows.Count];
                for (int i = 0; i < dset.Tables["Interval"].Rows.Count; i++) {
                    reportDate[i] = Convert.ToDateTime(dset.Tables["Interval"].Rows[i]["DataDate"].ToString());
                }
                string strReturn = CreateIntervalIPUBusyDundasChart(systemSerial, reportDate, startDate, endDate, path, interval);
                strReturn += ",";
                strReturn += CreateIntervalIPUQueueDundasChart(systemSerial, reportDate, startDate, endDate, path, interval);
                return strReturn;
            }
            else {
                var reportDate = new DateTime[1];
                reportDate[0] = Convert.ToDateTime(startDate);

                string strReturn = "";
                log.Info("Before CreatePerIPUDundasChart");
                
                strReturn = CreateIntervalIPUBusyDundasChart(systemSerial, reportDate, startDate, endDate, path, interval);
                strReturn += ",";
                strReturn += CreateIntervalIPUQueueDundasChart(systemSerial, reportDate, startDate, endDate, path, interval);
                return strReturn;
            }
        }

        public string CreateApplicationChartPerInterval(string systemSerial, DateTime startDate, DateTime endDate, string path, ILog log, long interval, ref bool hourDrop) {
            //IR 6466
            log.Info("On CreateApplicationChartPerInterval");
            
            
            var reportDate = new DateTime[1];
            reportDate[0] = Convert.ToDateTime(startDate);

            string strReturn = "";
            log.Info("Before CreateApplicationIntervalCPUBusyDundasChart");
            
            strReturn = CreateApplicationIntervalCPUBusyDundasChart(systemSerial, reportDate, startDate, endDate, path, interval);
            return strReturn;
        }

        public DataTable GetCPUBusyAlertColor(string systemSerial, DateTime startDate, DateTime endDate, long interval, Dictionary<int, SystemWeekInfo> systemweekInfo,
                                        double businessTolerance, double batchTolerance, double otherTolerance, List<ForecastData> forecastData, bool alertException, DataTable cpuBusyGrid, ILog log, ref List<ExceptionView> exceptionList) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            string dateFormat = "yyyy-MM-dd HH:mm:ss"; 
            var sysUnrated = new DailySysUnratedService(ConnectionStringSPAM);
            var reportDate = new List<DateTime>();

            if ((Convert.ToDateTime(endDate).Subtract(startDate)).Days >= 1) {
                DataSet dset = sysUnrated.GetDataDateFor(1, startDate, endDate, systemSerial);
                for (int i = 0; i < dset.Tables["Interval"].Rows.Count; i++) {
                    reportDate.Add(Convert.ToDateTime(dset.Tables["Interval"].Rows[i]["DataDate"].ToString()));
                }
            }
            else {
                reportDate.Add(Convert.ToDateTime(startDate));
            }

            //Get data from detail table.
            var databaseMapService = new DatabaseMappingService(ConnectionString);
            string newConnectionString = databaseMapService.GetConnectionStringFor(systemSerial);


            //Get TableNames.
            var databaseCheck = new Database(newConnectionString);
            var databaseName = Helper.FindKeyName(newConnectionString, "DATABASE");
            var cpuTrendTable = new CPUTrendTableRepository(newConnectionString);
            var dataTable = cpuTrendTable.GetCPUBusyInterval(startDate.ToString(dateFormat), endDate.ToString(dateFormat));
            var cpuLists = dataTable.AsEnumerable().Select(x => x.Field<UInt64>("CPUNumber")).Distinct().ToList();

            var totalMaxValue = 0D;
            dataTable = dataTable.AsEnumerable().OrderBy(x => x.Field<DateTime>("Date & Time")).CopyToDataTable();
            var dataIntervals = dataTable.AsEnumerable().Select(x => x.Field<DateTime>("Date & Time")).Distinct().ToList();

            var alertExceptionColor = Color.White;
            int yellowForecastCount = 0;
            int redForecastCount = 0;

            if (dataIntervals.Count > 0) {
                DataRow yellowCountRow = cpuBusyGrid.NewRow();
                DataRow redCountRow = cpuBusyGrid.NewRow();
                int yellowGridCount = 0, redGridCount = 0, prevHour = -1; double cpuBusy = 0;
                yellowCountRow["Entity"] = "CPU"; yellowCountRow["Counter"] = "Busy";
                redCountRow["Entity"] = "CPU"; redCountRow["Counter"] = "Busy";
                bool applyForecastCount = false;
                var systemWeekExceptionService = new SystemWeekExceptionService(ConnectionString);

                foreach (var dataInterval in dataIntervals) {
                    foreach (var cpuNum in cpuLists) {
                        cpuBusy = dataTable.AsEnumerable().Where(x => x.Field<UInt64>("CPUNumber").Equals(cpuNum) && x.Field<DateTime>("Date & Time").Equals(dataInterval)).Select(x => x.Field<double>("Busy")).FirstOrDefault();
                        if (forecastData.Count > 0) {
                            var isForecastData = forecastData.Any(
                                x => x.ForecastDateTime.Equals(dataInterval) 
                                && (Convert.ToInt16(x.CpuNumber) == Convert.ToInt16(cpuNum)));
                            if (alertException && isForecastData) {
                                applyForecastCount = true;
                                //var toleranceValue = GetSystemWeekInfoHourData(dataInterval.Hour, systemweekInfo[(int)dataInterval.DayOfWeek], businessTolerance, batchTolerance, otherTolerance);
                                var thresholdTypeId = GetThresholdTypeId(dataInterval.Hour, systemweekInfo[(int)dataInterval.DayOfWeek]);
                                var systemWeekThresholds = new SystemWeekThresholdsRepository();
                                var threadholds = systemWeekThresholds.GetCpuBusy(systemSerial, thresholdTypeId);

                                var forecastDataSub = forecastData.Where(x => x.ForecastDateTime.Equals(dataInterval) && 
                                (Convert.ToInt16(x.CpuNumber) == Convert.ToInt16(cpuNum))).Select(x => x.CpuBusy).FirstOrDefault();
                                var stdDev = forecastData.Where(x => x.ForecastDateTime.Equals(dataInterval) && 
                                (Convert.ToInt16(x.CpuNumber) == Convert.ToInt16(cpuNum))).Select(x => x.StdDevCpuBusy).FirstOrDefault();

                                /*//Check if we have IsChanged on SystemWeekException for this daysofweek and hour.
                                var hour = dataInterval.Hour.ToString("D2");
                                var daysOfWeek = (int)dataInterval.DayOfWeek;
                                var isChanged = systemWeekExceptionService.GetIsChangedValueFor(systemSerial, 1, 1, daysOfWeek, hour);

                                if (isChanged.Count > 0 && isChanged.FirstOrDefault().Key) {
                                    forecastDataSub = isChanged.FirstOrDefault().Value;
                                }*/

                                var defaultValueMajor = GetThreasholdDefaultValue(thresholdTypeId, "CPUBusyMajor");
                                var exceptionMajor = defaultValueMajor;
                                if (threadholds.Rows.Count > 0)
                                    exceptionMajor = threadholds.Rows[0].IsNull("CPUBusyMajor") ? defaultValueMajor : Convert.ToDouble(threadholds.Rows[0]["CPUBusyMajor"]);

                                var upperRange = forecastDataSub + exceptionMajor + stdDev;
                                var lowerRange = Math.Abs(forecastDataSub - (exceptionMajor + stdDev));
                                
                                if (dataInterval.Hour != prevHour) {
                                    if (prevHour != -1) {
                                        string hourHeader = prevHour < 10 ? "0" + prevHour : "" + prevHour;
                                        yellowCountRow[hourHeader] = yellowForecastCount;
                                        redCountRow[hourHeader] = redForecastCount;
                                    }
                                    yellowForecastCount = 0;
                                    redForecastCount = 0;
                                    prevHour = dataInterval.Hour;
                                }

                                if (cpuBusy > upperRange) {
                                    redForecastCount++;
                                    alertExceptionColor = Color.Red;
                                    exceptionList.Add(new ExceptionView {
                                        FromTimestamp = dataInterval,
                                        Instance = cpuNum.ToString("D2"),
                                        EntityId = "CPU",
                                        CounterId = "Busy",
                                        Actual = cpuBusy,
                                        Upper = upperRange,
                                        Lower = lowerRange,
                                        DisplayRed = true,
                                        IsException = true
                                    });
                                }
                                else {
                                    //Check half of toleranceValue.
                                    var defaultValueMinor = GetThreasholdDefaultValue(thresholdTypeId, "CPUBusyMinor");
                                    var exceptionMinor = defaultValueMinor;
                                    if (threadholds.Rows.Count > 0)
                                        exceptionMinor = threadholds.Rows[0].IsNull("CPUBusyMinor") ? defaultValueMinor : Convert.ToDouble(threadholds.Rows[0]["CPUBusyMinor"]);

                                    var upperRangeSub = forecastDataSub + exceptionMinor + stdDev;
                                    var lowerRangeSub = Math.Abs(forecastDataSub - (exceptionMinor + stdDev));

                                    if (cpuBusy > upperRangeSub) {
                                        yellowForecastCount++;
                                        if (alertExceptionColor != Color.Red) {
                                            alertExceptionColor = Color.Yellow;
                                        }
                                        exceptionList.Add(new ExceptionView {
                                            FromTimestamp = dataInterval,
                                            Instance = cpuNum.ToString("D2"),
                                            EntityId = "CPU",
                                            CounterId = "Busy",
                                            Actual = cpuBusy,
                                            Upper = upperRangeSub,
                                            Lower = lowerRangeSub,
                                            DisplayRed = false,
                                            IsException = true
                                        });
                                    }
                                    else {
                                        //Normal values.
                                        exceptionList.Add(new ExceptionView {
                                            FromTimestamp = dataInterval,
                                            Instance = cpuNum.ToString("D2"),
                                            EntityId = "CPU",
                                            CounterId = "Busy",
                                            Actual = cpuBusy,
                                            Upper = upperRangeSub,
                                            Lower = lowerRangeSub,
                                            DisplayRed = false,
                                            IsException = false
                                        });
                                    }
                                }
                            }
                            else {
                                if (cpuBusy > totalMaxValue)
                                    totalMaxValue = cpuBusy;
                                //Grid code - after loop through all CPUs for that interval - regular count
                                if (dataInterval.Hour != prevHour) {
                                    if (prevHour != -1) {
                                        string hourHeader = prevHour < 10 ? "0" + prevHour : "" + prevHour;
                                        yellowCountRow[hourHeader] = yellowGridCount;
                                        redCountRow[hourHeader] = redGridCount;
                                    }
                                    yellowGridCount = 0;
                                    redGridCount = 0;
                                    prevHour = dataInterval.Hour;
                                }
                                if (cpuBusy > 79 && cpuBusy < 90) {
                                    yellowGridCount++;
                                }
                                else if (cpuBusy >= 90) {
                                    redGridCount++;
                                }
                            }
                        }
                        else {
                            if (cpuBusy > totalMaxValue)
                                totalMaxValue = cpuBusy;
                        }
                    }
                }
                try {
                    //Set the last hour
                    if (prevHour != -1)
                    {
                        string lHourHeader = prevHour < 10 ? "0" + prevHour : "" + prevHour;
                        yellowCountRow[lHourHeader] = applyForecastCount ? yellowForecastCount : yellowGridCount;
                        redCountRow[lHourHeader] = applyForecastCount ? redForecastCount : redGridCount;
                    }
                    cpuBusyGrid.Rows.Add(yellowCountRow);
                    cpuBusyGrid.Rows.Add(redCountRow);
                }
                catch (Exception ex) {
					log.Error("************ [JobProcessorChart] GetCPUBusyAlertColor Error ************");
                    log.Error(ex.Message);
				}
            }

            return cpuBusyGrid;
        }

        public DataTable GetIPUBusyAlertColor(string systemSerial, DateTime startDate, DateTime endDate, long interval, Dictionary<int, SystemWeekInfo> systemweekInfo,
                                        double businessTolerance, double batchTolerance, double otherTolerance, List<ForecastData> forecastData, bool alertException, DataTable ipuBusyGrid, ILog log, ref List<ExceptionView> exceptionList) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            string dateFormat = "yyyy-MM-dd HH:mm:ss";
            var sysUnrated = new DailySysUnratedService(ConnectionStringSPAM);
            var reportDate = new List<DateTime>();

            if ((Convert.ToDateTime(endDate).Subtract(startDate)).Days >= 1) {
                DataSet dset = sysUnrated.GetDataDateFor(1, startDate, endDate, systemSerial);
                for (int i = 0; i < dset.Tables["Interval"].Rows.Count; i++) {
                    reportDate.Add(Convert.ToDateTime(dset.Tables["Interval"].Rows[i]["DataDate"].ToString()));
                }
            }
            else {
                reportDate.Add(Convert.ToDateTime(startDate));
            }

            //Get data from detail table.
            var databaseMapService = new DatabaseMappingService(ConnectionString);
            string newConnectionString = databaseMapService.GetConnectionStringFor(systemSerial);


            //Get TableNames.
            var databaseCheck = new Database(newConnectionString);
            var databaseName = Helper.FindKeyName(newConnectionString, "DATABASE");
            var cpuTrendTable = new CPUTrendTableRepository(newConnectionString);
            var dataTable = cpuTrendTable.GetIPUBusyInterval(startDate.ToString(dateFormat), endDate.ToString(dateFormat));

            //var exceptionList = new List<ExceptionView>();
            var alertExceptionColor = Color.White;
            int yellowForecastCount = 0;
            int redForecastCount = 0;
            //var ipuBusyGridCpy = ipuBusyGridStructure.Clone();
            if (dataTable.Rows.Count > 0) {
                DataRow yellowCountRow = ipuBusyGrid.NewRow();
                DataRow redCountRow = ipuBusyGrid.NewRow();
                int yellowGridCount = 0, redGridCount = 0, prevHour = -1; 
                double ipuBusy = 0;
                yellowCountRow["Entity"] = "IPU"; yellowCountRow["Counter"] = "Busy";
                redCountRow["Entity"] = "IPU"; redCountRow["Counter"] = "Busy";
                bool applyForecastCount = false;
                var systemWeekExceptionService = new SystemWeekExceptionService(ConnectionString);
                var totalMaxValue = 0d;

                foreach (DataRow row in dataTable.Rows) {
                    ipuBusy = Convert.ToDouble(row["Busy"]);
                    if (forecastData.Count > 0) {
                        var cpuNum = Convert.ToInt32(row["CPUNumber"]);
                        var ipuNum = Convert.ToInt32(row["IPUNumber"]);
                        DateTime dateInterval = Convert.ToDateTime(row["Date & Time"]);
                        var isForecastData = forecastData.Any(x => x.ForecastDateTime.Equals(dateInterval) &&
                                                                    (Convert.ToInt16(x.CpuNumber) == Convert.ToInt16(cpuNum)) &&
                                                                    (Convert.ToInt16(x.IpuNumber) == Convert.ToInt16(ipuNum)));
                        if (dateInterval.Hour == 23) {
                            string stop = "";
                        }
                        if (alertException && isForecastData) {
                            applyForecastCount = true;
                            //var toleranceValue = GetSystemWeekInfoHourData(dataInterval.Hour, systemweekInfo[(int)dataInterval.DayOfWeek], businessTolerance, batchTolerance, otherTolerance);
                            var thresholdTypeId = GetThresholdTypeId(dateInterval.Hour, systemweekInfo[(int)dateInterval.DayOfWeek]);
                            var forecastDataSub = forecastData.Where(x => x.ForecastDateTime.Equals(dateInterval) &&
                                                                            (Convert.ToInt16(x.CpuNumber) == Convert.ToInt16(cpuNum)) &&
                                                                            (Convert.ToInt16(x.IpuNumber) == Convert.ToInt16(ipuNum))).Select(x => x.IpuBusy).FirstOrDefault();
                            var stdDev = forecastData.Where(x => x.ForecastDateTime.Equals(dateInterval) &&
                                                                        (Convert.ToInt16(x.CpuNumber) == Convert.ToInt16(cpuNum)) &&
                                                                        (Convert.ToInt16(x.IpuNumber) == Convert.ToInt16(ipuNum))).Select(x => x.StdDevIpuBusy).FirstOrDefault();
                            var systemWeekThresholds = new SystemWeekThresholdsRepository();
                            var threadholds = systemWeekThresholds.GetIpuBusy(systemSerial, thresholdTypeId);

                            var defaultValueMajor = GetThreasholdDefaultValue(thresholdTypeId, "IPUBusyMajor");

                            var exceptionMajor = defaultValueMajor;
                            if (threadholds.Rows.Count > 0)
                                exceptionMajor = threadholds.Rows[0].IsNull("IPUBusyMajor") ? defaultValueMajor : Convert.ToDouble(threadholds.Rows[0]["IPUBusyMajor"]);

                            var upperRange = forecastDataSub + exceptionMajor + stdDev;
                            var lowerRange = Math.Abs(forecastDataSub - (exceptionMajor + stdDev));

                            if (dateInterval.Hour != prevHour) {
                                if (prevHour != -1) {
                                    string hourHeader = prevHour < 10 ? "0" + prevHour : "" + prevHour;
                                    yellowCountRow[hourHeader] = yellowForecastCount;
                                    redCountRow[hourHeader] = redForecastCount;
                                }
                                yellowForecastCount = 0;
                                redForecastCount = 0;
                                prevHour = dateInterval.Hour;
                            }

                            if (ipuBusy > upperRange) {
                                redForecastCount++;
                                alertExceptionColor = Color.Red;

                                exceptionList.Add(new ExceptionView {
                                    FromTimestamp = dateInterval,
                                    Instance = cpuNum.ToString("D2") + ":" + ipuNum.ToString("D2"),
                                    EntityId = "IPU",
                                    CounterId = "Busy",
                                    Actual = ipuBusy,
                                    Upper = upperRange,
                                    Lower = lowerRange,
                                    DisplayRed = true,
                                    IsException = true
                                });
                            }
                            else {
                                //Check half of toleranceValue.
                                var defaultValueMinor = GetThreasholdDefaultValue(thresholdTypeId, "IPUBusyMinor");
                                var exceptionMinor = defaultValueMinor;
                                if (threadholds.Rows.Count > 0)
                                    exceptionMinor = threadholds.Rows[0].IsNull("IPUBusyMinor") ? defaultValueMinor : Convert.ToDouble(threadholds.Rows[0]["IPUBusyMinor"]);

                                var upperRangeSub = forecastDataSub + exceptionMinor + stdDev;
                                var lowerRangeSub = Math.Abs(forecastDataSub - (exceptionMinor + stdDev));

                                if (ipuBusy > upperRangeSub) {
                                    yellowForecastCount++;
                                    if (alertExceptionColor != Color.Red) {
                                        alertExceptionColor = Color.Yellow;
                                    }
                                    exceptionList.Add(new ExceptionView {
                                        FromTimestamp = dateInterval,
                                        Instance = cpuNum.ToString("D2") + ":" + ipuNum.ToString("D2"),
                                        EntityId = "IPU",
                                        CounterId = "Busy",
                                        Actual = ipuBusy,
                                        Upper = upperRangeSub,
                                        Lower = lowerRangeSub,
                                        DisplayRed = false,
                                        IsException = true
                                    });
                                }
                                else {
                                    exceptionList.Add(new ExceptionView {
                                        FromTimestamp = dateInterval,
                                        Instance = cpuNum.ToString("D2") + ":" + ipuNum.ToString("D2"),
                                        EntityId = "IPU",
                                        CounterId = "Busy",
                                        Actual = ipuBusy,
                                        Upper = upperRangeSub,
                                        Lower = lowerRangeSub,
                                        DisplayRed = false,
                                        IsException = false
                                    });
                                }
                            }

                        }
                        else {
                            if (ipuBusy > totalMaxValue)
                                totalMaxValue = ipuBusy;
                            //Grid code - after loop through all CPUs for that interval
                            if (dateInterval.Hour != prevHour) {
                                if (prevHour != -1) {
                                    string hourHeader = prevHour < 10 ? "0" + prevHour : "" + prevHour;
                                    yellowCountRow[hourHeader] = yellowGridCount;
                                    redCountRow[hourHeader] = redGridCount;
                                }
                                yellowGridCount = 0;
                                redGridCount = 0;
                                prevHour = dateInterval.Hour;
                            }
                            if (ipuBusy > 79 && ipuBusy < 90) {
                                yellowGridCount++;
                            }
                            else if (ipuBusy >= 90) {
                                redGridCount++;
                            }
                        }
                    }
                    else {
                        if (ipuBusy > totalMaxValue)
                            totalMaxValue = ipuBusy;
                    }
                }
                try {
                    //Set the last hour
                    if (prevHour != -1)
                    {
                        string lHourHeader = prevHour < 10 ? "0" + prevHour : "" + prevHour;
                        yellowCountRow[lHourHeader] = applyForecastCount ? yellowForecastCount : yellowGridCount;
                        redCountRow[lHourHeader] = applyForecastCount ? redForecastCount : redGridCount;
                    }
                    ipuBusyGrid.Rows.Add(yellowCountRow);
                    ipuBusyGrid.Rows.Add(redCountRow);
                }
                catch (Exception ex) {
					log.Error("************ [JobProcessorChart] GetIPUBusyAlertColor Error ************");
					log.ErrorFormat("Exceotuib {0}", ex.Message);
				}
            }
            return ipuBusyGrid;
        }
        

        private double GetThreasholdDefaultValue(int threasholdTypeId, string entityType) {
            var defaultValue = 0d;

            switch (entityType) {
                case "CPUBusyMinor":
                    if (threasholdTypeId == 1) defaultValue = 10;
                    else if (threasholdTypeId == 2) defaultValue = 13;
                    else if (threasholdTypeId == 3) defaultValue = 15;
                    break;
                case "CPUBusyMajor":
                    if (threasholdTypeId == 1) defaultValue = 20;
                    else if (threasholdTypeId == 2) defaultValue = 23;
                    else if (threasholdTypeId == 3) defaultValue = 25;
                    break;
                case "CPUQueueLengthMinor":
                    if (threasholdTypeId == 1) defaultValue = 5;
                    else if (threasholdTypeId == 2) defaultValue = 8;
                    else if (threasholdTypeId == 3) defaultValue = 13;
                    break;
                case "CPUQueueLengthMajor":
                    if (threasholdTypeId == 1) defaultValue = 10;
                    else if (threasholdTypeId == 2) defaultValue = 13;
                    else if (threasholdTypeId == 3) defaultValue = 15;
                    break;
                case "IPUBusyMinor":
                    if (threasholdTypeId == 1) defaultValue = 20;
                    else if (threasholdTypeId == 2) defaultValue = 23;
                    else if (threasholdTypeId == 3) defaultValue = 25;
                    break;
                case "IPUBusyMajor":
                    if (threasholdTypeId == 1) defaultValue = 40;
                    else if (threasholdTypeId == 2) defaultValue = 43;
                    else if (threasholdTypeId == 3) defaultValue = 45;
                    break;
                case "IPUQueueLengthMinor":
                    if (threasholdTypeId == 1) defaultValue = 5;
                    else if (threasholdTypeId == 2) defaultValue = 8;
                    else if (threasholdTypeId == 3) defaultValue = 13;
                    break;
                case "IPUQueueLengthMajor":
                    if (threasholdTypeId == 1) defaultValue = 10;
                    else if (threasholdTypeId == 2) defaultValue = 13;
                    else if (threasholdTypeId == 3) defaultValue = 15;
                    break;
                case "DiskQueueLengthMinor":
                    if (threasholdTypeId == 1) defaultValue = 5;
                    else if (threasholdTypeId == 2) defaultValue = 8;
                    else if (threasholdTypeId == 3) defaultValue = 13;
                    break;
                case "DiskQueueLengthMajor":
                    if (threasholdTypeId == 1) defaultValue = 10;
                    else if (threasholdTypeId == 2) defaultValue = 13;
                    else if (threasholdTypeId == 3) defaultValue = 15;
                    break;
                case "DiskDP2Minor":
                    if (threasholdTypeId == 1) defaultValue = 20;
                    else if (threasholdTypeId == 2) defaultValue = 23;
                    else if (threasholdTypeId == 3) defaultValue = 25;
                    break;
                case "DiskDP2Major":
                    if (threasholdTypeId == 1) defaultValue = 30;
                    else if (threasholdTypeId == 2) defaultValue = 33;
                    else if (threasholdTypeId == 3) defaultValue = 35;
                    break;
                case "StorageMinor":
                    if (threasholdTypeId == 1) defaultValue = 10;
                    else if (threasholdTypeId == 2) defaultValue = 10;
                    else if (threasholdTypeId == 3) defaultValue = 10;
                    break;
                case "StorageMajor":
                    if (threasholdTypeId == 1) defaultValue = 20;
                    else if (threasholdTypeId == 2) defaultValue = 20;
                    else if (threasholdTypeId == 3) defaultValue = 20;
                    break;
            }

            return defaultValue;
        }
        private int GetThresholdTypeId(int hour, SystemWeekInfo systemWeekInfo) {
            var thresholdTypeId = 0;
            var tolerance = 0;

            switch (hour) {
                case 0: tolerance = systemWeekInfo.Hour00; break;
                case 1: tolerance = systemWeekInfo.Hour01; break;
                case 2: tolerance = systemWeekInfo.Hour02; break;
                case 3: tolerance = systemWeekInfo.Hour03; break;
                case 4: tolerance = systemWeekInfo.Hour04; break;
                case 5: tolerance = systemWeekInfo.Hour05; break;
                case 6: tolerance = systemWeekInfo.Hour06; break;
                case 7: tolerance = systemWeekInfo.Hour07; break;
                case 8: tolerance = systemWeekInfo.Hour08; break;
                case 9: tolerance = systemWeekInfo.Hour09; break;
                case 10: tolerance = systemWeekInfo.Hour10; break;
                case 11: tolerance = systemWeekInfo.Hour11; break;
                case 12: tolerance = systemWeekInfo.Hour12; break;
                case 13: tolerance = systemWeekInfo.Hour13; break;
                case 14: tolerance = systemWeekInfo.Hour14; break;
                case 15: tolerance = systemWeekInfo.Hour15; break;
                case 16: tolerance = systemWeekInfo.Hour16; break;
                case 17: tolerance = systemWeekInfo.Hour17; break;
                case 18: tolerance = systemWeekInfo.Hour18; break;
                case 19: tolerance = systemWeekInfo.Hour19; break;
                case 20: tolerance = systemWeekInfo.Hour20; break;
                case 21: tolerance = systemWeekInfo.Hour21; break;
                case 22: tolerance = systemWeekInfo.Hour22; break;
                case 23: tolerance = systemWeekInfo.Hour23; break;
            }

            //1: Business, 2: Batch, 3: Other
            if (tolerance == 0) thresholdTypeId = 3;
            else if (tolerance == 1) thresholdTypeId = 1;
            else thresholdTypeId = 2;

            return thresholdTypeId;
        }

        public DataTable GetCPUQueueAlertColor(string systemSerial, DateTime startDate, DateTime endDate, long interval, Dictionary<int, SystemWeekInfo> systemweekInfo,
                                        double businessTolerance, double batchTolerance, double otherTolerance, List<ForecastData> forecastData, bool alertException, DataTable cpuQueueGrid, ILog log, ref List<ExceptionView> exceptionList) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            string dateFormat = "yyyy-MM-dd HH:mm:ss"; var sysUnrated = new DailySysUnratedService(ConnectionStringSPAM);
            var reportDate = new List<DateTime>();

            if ((Convert.ToDateTime(endDate).Subtract(startDate)).Days >= 1) {
                DataSet dset = sysUnrated.GetDataDateFor(1, startDate, endDate, systemSerial);
                for (int i = 0; i < dset.Tables["Interval"].Rows.Count; i++) {
                    reportDate.Add(Convert.ToDateTime(dset.Tables["Interval"].Rows[i]["DataDate"].ToString()));
                }
            }
            else {
                reportDate.Add(Convert.ToDateTime(startDate));
            }

            //Get data from detail table.
            var databaseMapService = new DatabaseMappingService(ConnectionString);
            string newConnectionString = databaseMapService.GetConnectionStringFor(systemSerial);


            //Get TableNames.
            var databaseCheck = new Database(newConnectionString);
            var databaseName = Helper.FindKeyName(newConnectionString, "DATABASE");
            var cpuTrendTable = new CPUTrendTableRepository(newConnectionString);
            var dataTable = cpuTrendTable.GetCPUQueueInterval(startDate.ToString(dateFormat), endDate.ToString(dateFormat));
            var cpuLists = dataTable.AsEnumerable().Select(x => x.Field<UInt64>("CPUNumber")).Distinct().ToList();

            dataTable = dataTable.AsEnumerable().OrderBy(x => x.Field<DateTime>("Date & Time")).CopyToDataTable();
            var dataIntervals = dataTable.AsEnumerable().Select(x => x.Field<DateTime>("Date & Time")).Distinct().ToList();
            //var exceptionList = new List<ExceptionView>();

            var totalMaxValue = 0D;
            var alertExceptionColor = Color.White;
            int yellowForecastCount = 0;
            int redForecastCount = 0;
            //var cpuQueueGridCpy = cpuQueueGridStructure.Clone();
            if (dataIntervals.Count > 0) {
                DataRow yellowCountRow = cpuQueueGrid.NewRow();
                DataRow redCountRow = cpuQueueGrid.NewRow();
                int yellowGridCount = 0, redGridCount = 0, prevHour = -1; double cpuQueue = 0;
                yellowCountRow["Entity"] = "CPU"; yellowCountRow["Counter"] = "Queue";
                redCountRow["Entity"] = "CPU"; redCountRow["Counter"] = "Queue";
                bool applyForecastCount = false;
                var systemWeekExceptionService = new SystemWeekExceptionService(ConnectionString);

                foreach (var dataInterval in dataIntervals) {
                    foreach (var cpuNum in cpuLists) {
                        cpuQueue = dataTable.AsEnumerable().Where(x => x.Field<UInt64>("CPUNumber").Equals(cpuNum) && x.Field<DateTime>("Date & Time").Equals(dataInterval)).Select(x => x.Field<double>("Queue")).FirstOrDefault();
                        if (forecastData.Count > 0) {
                            var isForecastData = forecastData.Any(x => x.ForecastDateTime.Equals(dataInterval) && 
                            (Convert.ToInt16(x.CpuNumber) == Convert.ToInt16(cpuNum)));
                            if (alertException && isForecastData) {
                                applyForecastCount = true;
                                //var toleranceValue = GetSystemWeekInfoHourData(dataInterval.Hour, systemweekInfo[(int)dataInterval.DayOfWeek], businessTolerance, batchTolerance, otherTolerance);
                                var thresholdTypeId = GetThresholdTypeId(dataInterval.Hour, systemweekInfo[(int)dataInterval.DayOfWeek]);
                                var systemWeekThresholds = new SystemWeekThresholdsRepository();
                                var threadholds = systemWeekThresholds.GetCpuQueueLength(systemSerial, thresholdTypeId);

                                var forecastDataSub = forecastData.Where(x => x.ForecastDateTime.Equals(dataInterval) &&
                                (Convert.ToInt16(x.CpuNumber) == Convert.ToInt16(cpuNum))).Select(x => x.Queue).FirstOrDefault();
                                var stdDev = forecastData.Where(x => x.ForecastDateTime.Equals(dataInterval) &&
                                (Convert.ToInt16(x.CpuNumber) == Convert.ToInt16(cpuNum))).Select(x => x.StdDevQueue).FirstOrDefault();
                                
                                /*//Check if we have IsChanged on SystemWeekException for this daysofweek and hour.
                                var hour = dataInterval.Hour.ToString("D2");
                                var daysOfWeek = (int)dataInterval.DayOfWeek;
                                var isChanged = systemWeekExceptionService.GetIsChangedValueFor(systemSerial, 1, 2, daysOfWeek, hour);
                                if (isChanged.Count > 0 && isChanged.FirstOrDefault().Key) {
                                    forecastDataSub = isChanged.FirstOrDefault().Value;
                                }*/

                                var defaultValueMajor = GetThreasholdDefaultValue(thresholdTypeId, "CPUQueueLengthMajor");
                                var exceptionMajor = defaultValueMajor;
                                if (threadholds.Rows.Count > 0)
                                    exceptionMajor = threadholds.Rows[0].IsNull("CPUQueueLengthMajor") ? defaultValueMajor : Convert.ToDouble(threadholds.Rows[0]["CPUQueueLengthMajor"]);

                                var upperRange = forecastDataSub + exceptionMajor + stdDev;
                                var lowerRange = Math.Abs(forecastDataSub - (exceptionMajor + stdDev));

                                if (dataInterval.Hour != prevHour) {
                                    if (prevHour != -1) {
                                        string hourHeader = prevHour < 10 ? "0" + prevHour : "" + prevHour;
                                        yellowCountRow[hourHeader] = yellowForecastCount;
                                        redCountRow[hourHeader] = redForecastCount;
                                    }
                                    yellowForecastCount = 0;
                                    redForecastCount = 0;
                                    prevHour = dataInterval.Hour;
                                }
                                if (cpuQueue > upperRange) {
                                    redForecastCount++;
                                    alertExceptionColor = Color.Red;
                                    exceptionList.Add(new ExceptionView {
                                        FromTimestamp = dataInterval,
                                        Instance = cpuNum.ToString("D2"),
                                        EntityId = "CPU",
                                        CounterId = "Queue",
                                        Actual = cpuQueue,
                                        Upper = upperRange,
                                        Lower = lowerRange,
                                        DisplayRed = true,
                                        IsException = true
                                    });
                                }
                                else {
                                    //Check half of toleranceValue.
                                    var defaultValueMinor = GetThreasholdDefaultValue(thresholdTypeId, "CPUQueueLengthMinor");
                                    var exceptionMinor = defaultValueMinor;
                                    if (threadholds.Rows.Count > 0)
                                        exceptionMinor = threadholds.Rows[0].IsNull("CPUQueueLengthMinor") ? defaultValueMinor : Convert.ToDouble(threadholds.Rows[0]["CPUQueueLengthMinor"]);
                                    var upperRangeSub = forecastDataSub + exceptionMinor + stdDev;
                                    var lowerRangeSub = Math.Abs(forecastDataSub - (exceptionMinor + stdDev));

                                    if (cpuQueue > upperRangeSub) {
                                        yellowForecastCount++;
                                        if (alertExceptionColor != Color.Red) {
                                            alertExceptionColor = Color.Yellow;
                                        }
                                        exceptionList.Add(new ExceptionView {
                                            FromTimestamp = dataInterval,
                                            Instance = cpuNum.ToString("D2"),
                                            EntityId = "CPU",
                                            CounterId = "Queue",
                                            Actual = cpuQueue,
                                            Upper = upperRangeSub,
                                            Lower = lowerRangeSub,
                                            DisplayRed = false,
                                            IsException = true
                                        });
                                    }
                                    else {
                                        exceptionList.Add(new ExceptionView {
                                            FromTimestamp = dataInterval,
                                            Instance = cpuNum.ToString("D2"),
                                            EntityId = "CPU",
                                            CounterId = "Queue",
                                            Actual = cpuQueue,
                                            Upper = upperRangeSub,
                                            Lower = lowerRangeSub,
                                            DisplayRed = false,
                                            IsException = false
                                        });
                                    }
                                }
                            }
                            else {
                                if (cpuQueue > totalMaxValue)
                                    totalMaxValue = cpuQueue;
                                //Grid code - after loop through all CPUs for that interval
                                if (dataInterval.Hour != prevHour) {
                                    if (prevHour != -1) {
                                        string hourHeader = prevHour < 10 ? "0" + prevHour : "" + prevHour;
                                        yellowCountRow[hourHeader] = yellowGridCount;
                                        redCountRow[hourHeader] = redGridCount;
                                    }
                                    yellowGridCount = 0;
                                    redGridCount = 0;
                                    prevHour = dataInterval.Hour;
                                }
                                if (cpuQueue >= 5 && cpuQueue < 10) {
                                    yellowGridCount++;
                                }
                                else if (cpuQueue >= 10) {
                                    redGridCount++;
                                }
                            }
                        }
                        else {
                            if (cpuQueue > totalMaxValue)
                                totalMaxValue = cpuQueue;
                        }
                    }
                }

                try {
                    //Set the last hour
                    if (prevHour != -1) { 
                        string lHourHeader = prevHour < 10 ? "0" + prevHour : "" + prevHour;
                        yellowCountRow[lHourHeader] = applyForecastCount ? yellowForecastCount : yellowGridCount;
                        redCountRow[lHourHeader] = applyForecastCount ? redForecastCount : redGridCount;
                    }
                    cpuQueueGrid.Rows.Add(yellowCountRow);
                    cpuQueueGrid.Rows.Add(redCountRow);
                }
                catch (Exception ex) {
					log.Error("************ [JobProcessorChart] GetCPUQueueAlertColor Error ************");
					log.Error(ex.Message);					
				}
            }

            /*if (alertException && forecastData.Count > 0)
            {
                cpuQueueGrid = cpuQueueGridCpy;
                return alertExceptionColor;
            }

            var color = Color.White;
            if (totalMaxValue >= 5 && totalMaxValue < 10)
                color = Color.Yellow;
            else if (totalMaxValue >= 10)
                color = Color.Red;

            cpuQueueGrid = cpuQueueGridCpy;
            return color;*/
            //if (exceptionList.Count > 0) {
            //    ExpectionBulkInsert(databaseName, exceptionList, tempSaveLocation);
            //}

            return cpuQueueGrid;
        }

        public DataTable GetIpuQueueAlertColor(string systemSerial, DateTime startDate, DateTime endDate, long interval, Dictionary<int, SystemWeekInfo> systemweekInfo,
            double businessTolerance, double batchTolerance, double otherTolerance, List<ForecastData> forecastData, bool alertException, DataTable ipuQueueGrid, ILog log, ref List<ExceptionView> exceptionList) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            string dateFormat = "yyyy-MM-dd HH:mm:ss"; 
            var sysUnrated = new DailySysUnratedService(ConnectionStringSPAM);
            var reportDate = new List<DateTime>();

            if ((Convert.ToDateTime(endDate).Subtract(startDate)).Days >= 1) {
                DataSet dset = sysUnrated.GetDataDateFor(1, startDate, endDate, systemSerial);
                for (int i = 0; i < dset.Tables["Interval"].Rows.Count; i++) {
                    reportDate.Add(Convert.ToDateTime(dset.Tables["Interval"].Rows[i]["DataDate"].ToString()));
                }
            }
            else {
                reportDate.Add(Convert.ToDateTime(startDate));
            }

            //Get data from detail table.
            var databaseMapService = new DatabaseMappingService(ConnectionString);
            string newConnectionString = databaseMapService.GetConnectionStringFor(systemSerial);

            //Get TableNames.
            var databaseCheck = new Database(newConnectionString);
            var databaseName = Helper.FindKeyName(newConnectionString, "DATABASE");
            var cpuTrendTable = new CPUTrendTableRepository(newConnectionString);
            var dataTable = cpuTrendTable.GetIPUQueueInterval(startDate.ToString(dateFormat), endDate.ToString(dateFormat));

            //var exceptionList = new List<ExceptionView>();
            var totalMaxValue = 0D;
            var alertExceptionColor = Color.White;
            int yellowForecastCount = 0;
            int redForecastCount = 0;
            //var ipuQueueGridCpy = ipuQueueGridStructure.Clone();
            if (dataTable.Rows.Count > 0) {
                DataRow yellowCountRow = ipuQueueGrid.NewRow();
                DataRow redCountRow = ipuQueueGrid.NewRow();
                int yellowGridCount = 0, redGridCount = 0, prevHour = -1; 
                double ipuQueue = 0;
                yellowCountRow["Entity"] = "IPU"; yellowCountRow["Counter"] = "Queue";
                redCountRow["Entity"] = "IPU"; redCountRow["Counter"] = "Queue";
                bool applyForecastCount = false;
                var systemWeekExceptionService = new SystemWeekExceptionService(ConnectionString);

                foreach (DataRow row in dataTable.Rows) {
                    ipuQueue = Convert.ToDouble(row["Queue"]);
                    if (forecastData.Count > 0) {
                        var cpuNum = Convert.ToInt32(row["CPUNumber"]);
                        var ipuNum = Convert.ToInt32(row["IPUNumber"]);
                        DateTime dateInterval = Convert.ToDateTime(row["Date & Time"]);
                        var isForecastData = forecastData.Any(x => x.ForecastDateTime.Equals(dateInterval) && 
                        (Convert.ToInt16(x.CpuNumber) == Convert.ToInt16(cpuNum)) && (Convert.ToInt16(x.IpuNumber) == Convert.ToInt16(ipuNum)));
                        if (alertException && isForecastData) {
                            applyForecastCount = true;
                            //var toleranceValue = GetSystemWeekInfoHourData(dateInterval.Hour, systemweekInfo[(int)dateInterval.DayOfWeek], businessTolerance, batchTolerance, otherTolerance);
                            var thresholdTypeId = GetThresholdTypeId(dateInterval.Hour, systemweekInfo[(int)dateInterval.DayOfWeek]);
                            var systemWeekThresholds = new SystemWeekThresholdsRepository();
                            var threadholds = systemWeekThresholds.GetIpuQueueLength(systemSerial, thresholdTypeId);

                            var forecastDataSub = forecastData.Where(x => x.ForecastDateTime.Equals(dateInterval) &&
                                                                            (Convert.ToInt16(x.CpuNumber) == Convert.ToInt16(cpuNum)) && 
                                                                            (Convert.ToInt16(x.IpuNumber) == Convert.ToInt16(ipuNum))).Select(x => x.IpuQueue).FirstOrDefault();
                            var stdDev = forecastData.Where(x => x.ForecastDateTime.Equals(dateInterval) &&
                                                                            (Convert.ToInt16(x.CpuNumber) == Convert.ToInt16(cpuNum)) && 
                                                                            (Convert.ToInt16(x.IpuNumber) == Convert.ToInt16(ipuNum))).Select(x => x.StdDevIpuQueue).FirstOrDefault();

                            var defaultValueMajor = GetThreasholdDefaultValue(thresholdTypeId, "IPUQueueLengthMajor");
                            var exceptionMajor = defaultValueMajor;
                            if (threadholds.Rows.Count > 0)
                                exceptionMajor = threadholds.Rows[0].IsNull("IPUQueueLengthMajor") ? defaultValueMajor : Convert.ToDouble(threadholds.Rows[0]["IPUQueueLengthMajor"]);
                            var upperRange = forecastDataSub + exceptionMajor + stdDev;
                            var lowerRange = Math.Abs(forecastDataSub - (exceptionMajor + stdDev));

                            if (dateInterval.Hour != prevHour) {
                                if (prevHour != -1) {
                                    string hourHeader = prevHour < 10 ? "0" + prevHour : "" + prevHour;
                                    yellowCountRow[hourHeader] = yellowForecastCount;
                                    redCountRow[hourHeader] = redForecastCount;
                                }
                                yellowForecastCount = 0;
                                redForecastCount = 0;
                                prevHour = dateInterval.Hour;
                            }

                            if (ipuQueue > upperRange) {
                                redForecastCount++;
                                alertExceptionColor = Color.Red;

                                exceptionList.Add(new ExceptionView {
                                    FromTimestamp = dateInterval,
                                    Instance = cpuNum.ToString("D2") + ":" + ipuNum.ToString("D2"),
                                    EntityId = "IPU",
                                    CounterId = "Queue",
                                    Actual = ipuQueue,
                                    Upper = upperRange,
                                    Lower = lowerRange,
                                    DisplayRed = true,
                                    IsException = true
                                });
                            }
                            else {
                                //Check half of toleranceValue.
                                var defaultValueMinor = GetThreasholdDefaultValue(thresholdTypeId, "IPUQueueLengthMinor");
                                var exceptionMinor = defaultValueMinor;
                                if (threadholds.Rows.Count > 0)
                                    exceptionMinor = threadholds.Rows[0].IsNull("IPUQueueLengthMinor") ? defaultValueMinor : Convert.ToDouble(threadholds.Rows[0]["IPUQueueLengthMinor"]);
                                var upperRangeSub = forecastDataSub + exceptionMinor + stdDev;
                                var lowerRangeSub = Math.Abs(forecastDataSub - (exceptionMinor + stdDev));

                                if (ipuQueue > upperRangeSub) {
                                    yellowForecastCount++;
                                    if (alertExceptionColor != Color.Red) {
                                        alertExceptionColor = Color.Yellow;
                                    }
                                    exceptionList.Add(new ExceptionView {
                                        FromTimestamp = dateInterval,
                                        Instance = cpuNum.ToString("D2") + ":" + ipuNum.ToString("D2"),
                                        EntityId = "IPU",
                                        CounterId = "Queue",
                                        Actual = ipuQueue,
                                        Upper = upperRangeSub,
                                        Lower = lowerRangeSub,
                                        DisplayRed = false,
                                        IsException = true
                                    });
                                }
                                else {
                                    exceptionList.Add(new ExceptionView {
                                        FromTimestamp = dateInterval,
                                        Instance = cpuNum.ToString("D2") + ":" + ipuNum.ToString("D2"),
                                        EntityId = "IPU",
                                        CounterId = "Queue",
                                        Actual = ipuQueue,
                                        Upper = upperRangeSub,
                                        Lower = lowerRangeSub,
                                        DisplayRed = false,
                                        IsException = false
                                    });
                                }
                            }

                        }
                        else {
                            if (ipuQueue > totalMaxValue)
                                totalMaxValue = ipuQueue;
                            //Grid code - after loop through all CPUs for that interval
                            if (dateInterval.Hour != prevHour) {
                                if (prevHour != -1) {
                                    string hourHeader = prevHour < 10 ? "0" + prevHour : "" + prevHour;
                                    yellowCountRow[hourHeader] = yellowGridCount;
                                    redCountRow[hourHeader] = redGridCount;
                                }
                                yellowGridCount = 0;
                                redGridCount = 0;
                                prevHour = dateInterval.Hour;
                            }
                            if (ipuQueue >= 5 && ipuQueue < 10) {
                                yellowGridCount++;
                            }
                            else if (ipuQueue >= 10) {
                                redGridCount++;
                            }
                        }
                    }
                    else {
                        if (ipuQueue > totalMaxValue)
                            totalMaxValue = ipuQueue;
                    }
                }

                try {
                    //Set the last hour
                    if (prevHour != -1)
                    {
                        string lHourHeader = prevHour < 10 ? "0" + prevHour : "" + prevHour;
                        yellowCountRow[lHourHeader] = applyForecastCount ? yellowForecastCount : yellowGridCount;
                        redCountRow[lHourHeader] = applyForecastCount ? redForecastCount : redGridCount;
                    }
                    ipuQueueGrid.Rows.Add(yellowCountRow);
                    ipuQueueGrid.Rows.Add(redCountRow);
                }
                catch(Exception ex) {
					log.Error("************ [JobProcessorChart] GetIpuQueueAlertColor Error ************");
					log.Error(ex.Message);					
				}
            }
            return ipuQueueGrid;
        }

        public DataTable GetDiskDP2AlertColor(string systemSerial, DateTime startDate, DateTime endDate, long interval, Dictionary<int, SystemWeekInfo> systemweekInfo,
            double businessTolerance, double batchTolerance, double otherTolerance, List<ForecastDiskData> forecastData, bool alertException, DataTable diskGrid, ILog log, string tempSaveLocation) {

            var diskBrowserables = new List<string>();
            var databaseName = Helper.FindKeyName(ConnectionStringSPAM, "DATABASE");
            var databaseCheck = new Database(ConnectionStringSPAM);

            for (var start = startDate.Date; start <= endDate.Date; start = start.AddDays(1)) {
                var cpuTableName = systemSerial + "_DISKBROWSER_" + start.Year + "_" + start.Month + "_" + start.Day;
                var exists = databaseCheck.CheckTableExists(cpuTableName, databaseName);

                if (exists)
                    diskBrowserables.Add(cpuTableName);
            }


            var diskBrowser = new DiskBrowserRepository(ConnectionStringSPAM);
            var diskQueueData = diskBrowser.GetDP2Busy(diskBrowserables, startDate, endDate);

            var alertExceptionColor = Color.White;
            int yellowForecastCount = 0;
            int redForecastCount = 0;

            var exceptionList = new List<ExceptionView>();
            var totalMaxValue = 0d;
            if (diskQueueData.Rows.Count > 0) {
                DataRow yellowCountRow = diskGrid.NewRow();
                DataRow redCountRow = diskGrid.NewRow();
                int yellowGridCount = 0, redGridCount = 0, prevHour = -1;
                yellowCountRow["Entity"] = "Disk";
                yellowCountRow["Counter"] = "DP2";
                redCountRow["Entity"] = "Disk";
                redCountRow["Counter"] = "DP2";
                var systemWeekExceptionService = new SystemWeekExceptionService(ConnectionString);

                bool applyForecastCount = false;
                foreach (DataRow row in diskQueueData.Rows) {
                    var deviceName = row["DeviceName"].ToString();
                    var fromTimestamp = Convert.ToDateTime(row["FromTimestamp"]);
                    var dp2Busy = Convert.ToDouble(row.IsNull("DP2Busy") ? 0 : row["DP2Busy"]);

					if (forecastData.Count > 0) {
                        var isForecastData = forecastData.Any(x => x.ForecastDateTime.Equals(fromTimestamp) && x.DeviceName.Equals(deviceName));
                        if (alertException && isForecastData) {
                            applyForecastCount = true;
                            
                            var thresholdTypeId = GetThresholdTypeId(fromTimestamp.Hour, systemweekInfo[(int)fromTimestamp.DayOfWeek]);
                            var systemWeekThresholds = new SystemWeekThresholdsRepository();
                            var threadholds = systemWeekThresholds.GetDiskDP2(systemSerial, thresholdTypeId);

                            var forecastDataSub = forecastData.Where(x => x.ForecastDateTime.Equals(fromTimestamp) &&
                                                                          x.DeviceName.Equals(deviceName)).Select(x => x.DP2Busy).FirstOrDefault();
                            
                            var stdDev = forecastData.Where(x => x.ForecastDateTime.Equals(fromTimestamp) &&
                                                                         x.DeviceName.Equals(deviceName)).Select(x => x.StdDevDP2Busy).FirstOrDefault();

                            var defaultValueMajor = GetThreasholdDefaultValue(thresholdTypeId, "DiskDP2Major");
                            var exceptionMajor = defaultValueMajor;
                            if (threadholds.Rows.Count > 0)
                                exceptionMajor = threadholds.Rows[0].IsNull("DiskDP2Major") ? defaultValueMajor : Convert.ToDouble(threadholds.Rows[0]["DiskDP2Major"]);
                            var upperRange = forecastDataSub + exceptionMajor + stdDev;
                            var lowerRange = Math.Abs(forecastDataSub - (exceptionMajor + stdDev));

                            if (fromTimestamp.Hour != prevHour) {
                                if (prevHour != -1) {
                                    string hourHeader = prevHour < 10 ? "0" + prevHour : "" + prevHour;
                                    yellowCountRow[hourHeader] = yellowForecastCount;
                                    redCountRow[hourHeader] = redForecastCount;
                                }
                                yellowForecastCount = 0;
                                redForecastCount = 0;
                                prevHour = fromTimestamp.Hour;
                            }

                            if (dp2Busy > upperRange) {
                                redForecastCount++;
                                alertExceptionColor = Color.Red;

                                exceptionList.Add(new ExceptionView {
                                    FromTimestamp = fromTimestamp,
                                    Instance = deviceName,
                                    EntityId = "Disk",
                                    CounterId = "DP2",
                                    Actual = dp2Busy,
                                    Upper = upperRange,
                                    Lower = lowerRange,
                                    DisplayRed = true
                                });
                            }
                            else {
                                //Check half of toleranceValue.
                                var defaultValueMinor = GetThreasholdDefaultValue(thresholdTypeId, "DiskDP2Minor");
                                var exceptionMinor = defaultValueMinor;
                                if (threadholds.Rows.Count > 0)
                                    exceptionMinor = threadholds.Rows[0].IsNull("DiskDP2Minor") ? defaultValueMinor : Convert.ToDouble(threadholds.Rows[0]["DiskDP2Minor"]);
                                var upperRangeSub = forecastDataSub + exceptionMinor + stdDev;
                                var lowerRangeSub = Math.Abs(forecastDataSub - (exceptionMinor + stdDev));
                                
                                if (dp2Busy > upperRangeSub) {
                                    yellowForecastCount++;
                                    if (alertExceptionColor != Color.Red) {
                                        alertExceptionColor = Color.Yellow;
                                    }

                                    exceptionList.Add(new ExceptionView {
                                        FromTimestamp = fromTimestamp,
                                        Instance = deviceName,
                                        EntityId = "Disk",
                                        CounterId = "DP2",
                                        Actual = dp2Busy,
                                        Upper = upperRangeSub,
                                        Lower = lowerRangeSub,
                                        DisplayRed = false
                                    });
                                }
                            }

                        }
                        else {
                            if (dp2Busy > totalMaxValue)
                                totalMaxValue = dp2Busy;
                            //Grid code - after loop through all CPUs for that interval
                            if (fromTimestamp.Hour != prevHour) {
                                if (prevHour != -1) {
                                    string hourHeader = prevHour < 10 ? "0" + prevHour : "" + prevHour;
                                    yellowCountRow[hourHeader] = yellowGridCount;
                                    redCountRow[hourHeader] = redGridCount;
                                }
                                yellowGridCount = 0;
                                redGridCount = 0;
                                prevHour = fromTimestamp.Hour;
                            }
                            if (dp2Busy >= 1 && dp2Busy < 30) {
                                yellowGridCount++;
                            }
                            else if (dp2Busy >= 30) {
                                redGridCount++;
                            }
                        }
                    }
                    else {
                        if (dp2Busy > totalMaxValue)
                            totalMaxValue = dp2Busy;
                    }
                }
				try {
					//Set the last hour
                    if (prevHour != -1)
                    {
                        string lHourHeader = prevHour < 10 ? "0" + prevHour : "" + prevHour;
                        yellowCountRow[lHourHeader] = applyForecastCount ? yellowForecastCount : yellowGridCount;
                        redCountRow[lHourHeader] = applyForecastCount ? redForecastCount : redGridCount;
                    }
					diskGrid.Rows.Add(yellowCountRow);
					diskGrid.Rows.Add(redCountRow);
				}
				catch (Exception ex) {
					log.Error("************ [JobProcessorChart] GetDiskDP2AlertColor Error ************");
					log.Error(ex.Message);
				}
            }

            if (exceptionList.Count > 0) {
                ExpectionBulkInsert(databaseName, exceptionList, tempSaveLocation);
            }

            return diskGrid;
        }

        public DataTable GetDiskQueueAlertColor(string systemSerial, DateTime startDate, DateTime endDate, long interval, Dictionary<int, SystemWeekInfo> systemweekInfo,
            double businessTolerance, double batchTolerance, double otherTolerance, List<ForecastDiskData> forecastData, bool alertException, DataTable diskGrid, ILog log, string tempSaveLocation) {
            var diskBrowserables = new List<string>();
            var databaseName = Helper.FindKeyName(ConnectionStringSPAM, "DATABASE");
            var databaseCheck = new Database(ConnectionStringSPAM);

            for (var start = startDate.Date; start <= endDate.Date; start = start.AddDays(1)) {
                var cpuTableName = systemSerial + "_DISKBROWSER_" + start.Year + "_" + start.Month + "_" + start.Day;
                var exists = databaseCheck.CheckTableExists(cpuTableName, databaseName);

                if (exists)
                    diskBrowserables.Add(cpuTableName);
            }


            var diskBrowser = new DiskBrowserRepository(ConnectionStringSPAM);
            var diskQueueData = diskBrowser.GetQueueLength(diskBrowserables, startDate, endDate);

            var alertExceptionColor = Color.White;
            int yellowForecastCount = 0;
            int redForecastCount = 0;

            var exceptionList = new List<ExceptionView>();
            var totalMaxValue = 0d;
            if (diskQueueData.Rows.Count > 0) {
                DataRow yellowCountRow = diskGrid.NewRow();
                DataRow redCountRow = diskGrid.NewRow();
                int yellowGridCount = 0, redGridCount = 0, prevHour = -1;
                yellowCountRow["Entity"] = "Disk";
                yellowCountRow["Counter"] = "Queue";
                redCountRow["Entity"] = "Disk";
                redCountRow["Counter"] = "Queue";
                var systemWeekExceptionService = new SystemWeekExceptionService(ConnectionString);

                bool applyForecastCount = false;
                foreach (DataRow row in diskQueueData.Rows) {
                    var deviceName = row["DeviceName"].ToString();
                    var fromTimestamp = Convert.ToDateTime(row["FromTimestamp"]);
                    var queueLength = Convert.ToDouble(row["QueueLength"]);

                    if (forecastData.Count > 0) {
                        var isForecastData = forecastData.Any(x => x.ForecastDateTime.Equals(fromTimestamp) && x.DeviceName.Equals(deviceName));
                        if (alertException && isForecastData) {
                            applyForecastCount = true;
                            //var toleranceValue = GetSystemWeekInfoHourData(fromTimestamp.Hour, systemweekInfo[(int)fromTimestamp.DayOfWeek], businessTolerance, batchTolerance, otherTolerance);
                            var thresholdTypeId = GetThresholdTypeId(fromTimestamp.Hour, systemweekInfo[(int)fromTimestamp.DayOfWeek]);
                            var systemWeekThresholds = new SystemWeekThresholdsRepository();
                            var threadholds = systemWeekThresholds.GetDiskQueueLength(systemSerial, thresholdTypeId);

                            var forecastDataSub = forecastData.Where(x => x.ForecastDateTime.Equals(fromTimestamp) &&
                                                                          x.DeviceName.Equals(deviceName)).Select(x => x.QueueLength).FirstOrDefault();

                            /*//Check if we have IsChanged on SystemWeekException for this daysofweek and hour.
                            var hour = fromTimestamp.Hour.ToString("D2");
                            var daysOfWeek = (int)fromTimestamp.DayOfWeek;
                            var isChanged = systemWeekExceptionService.GetIsChangedValueFor(systemSerial, 3, 1, daysOfWeek, hour);
                            if (isChanged.Count > 0 && isChanged.FirstOrDefault().Key) {
                                //Change the forecastDataSub base on the customer's input.
                                forecastDataSub = isChanged.FirstOrDefault().Value;
                            }*/
                            

                            var stdDev = forecastData.Where(x => x.ForecastDateTime.Equals(fromTimestamp) &&
                                                                         x.DeviceName.Equals(deviceName)).Select(x => x.StdDevQueueLength).FirstOrDefault();

                            var defaultValueMajor = GetThreasholdDefaultValue(thresholdTypeId, "DiskQueueLengthMajor");
                            var exceptionMajor = defaultValueMajor;
                            if (threadholds.Rows.Count > 0)
                                exceptionMajor = threadholds.Rows[0].IsNull("DiskQueueLengthMajor") ? defaultValueMajor : Convert.ToDouble(threadholds.Rows[0]["DiskQueueLengthMajor"]);
                            var upperRange = forecastDataSub + exceptionMajor + stdDev;
                            var lowerRange = Math.Abs(forecastDataSub - (exceptionMajor + stdDev));

                            if (fromTimestamp.Hour != prevHour) {
                                if (prevHour != -1) {
                                    string hourHeader = prevHour < 10 ? "0" + prevHour : "" + prevHour;
                                    yellowCountRow[hourHeader] = yellowForecastCount;
                                    redCountRow[hourHeader] = redForecastCount;
                                }
                                yellowForecastCount = 0;
                                redForecastCount = 0;
                                prevHour = fromTimestamp.Hour;
                            }

                            if (queueLength > upperRange) {
                                redForecastCount++;
                                alertExceptionColor = Color.Red;

                                exceptionList.Add(new ExceptionView {
                                    FromTimestamp = fromTimestamp,
                                    Instance = deviceName,
                                    EntityId = "Disk",
                                    CounterId = "Queue",
                                    Actual = queueLength,
                                    Upper = upperRange,
                                    Lower = lowerRange,
                                    DisplayRed = true
                                });
                            }
                            else {
                                //Check half of toleranceValue.
                                var defaultValueMinor = GetThreasholdDefaultValue(thresholdTypeId, "DiskQueueLengthMinor");
                                var exceptionMinor = defaultValueMinor;
                                if (threadholds.Rows.Count > 0)
                                    exceptionMinor = threadholds.Rows[0].IsNull("DiskQueueLengthMinor") ? defaultValueMinor : Convert.ToDouble(threadholds.Rows[0]["DiskQueueLengthMinor"]);
                                var upperRangeSub = forecastDataSub + exceptionMinor + stdDev;
                                var lowerRangeSub = Math.Abs(forecastDataSub - (exceptionMinor + stdDev));

                                if (queueLength > upperRangeSub) {
                                    yellowForecastCount++;
                                    if (alertExceptionColor != Color.Red) {
                                        alertExceptionColor = Color.Yellow;
                                    }

                                    exceptionList.Add(new ExceptionView {
                                        FromTimestamp = fromTimestamp,
                                        Instance = deviceName,
                                        EntityId = "Disk",
                                        CounterId = "Queue",
                                        Actual = queueLength,
                                        Upper = upperRangeSub,
                                        Lower = lowerRangeSub,
                                        DisplayRed = false
                                    });
                                }
                            }

                        }
                        else {
                            if (queueLength > totalMaxValue)
                                totalMaxValue = queueLength;
                            //Grid code - after loop through all CPUs for that interval
                            if (fromTimestamp.Hour != prevHour) {
                                if (prevHour != -1) {
                                    string hourHeader = prevHour < 10 ? "0" + prevHour : "" + prevHour;
                                    yellowCountRow[hourHeader] = yellowGridCount;
                                    redCountRow[hourHeader] = redGridCount;
                                }
                                yellowGridCount = 0;
                                redGridCount = 0;
                                prevHour = fromTimestamp.Hour;
                            }
                            if (queueLength >= 1 && queueLength < 2) {
                                yellowGridCount++;
                            }
                            else if (queueLength >= 2) {
                                redGridCount++;
                            }
                        }
                    }
                    else {
                        if (queueLength > totalMaxValue)
                            totalMaxValue = queueLength;
                    }
                }

				try {
                    //Set the last hour
                    if (prevHour != -1)
                    {
                        string lHourHeader = prevHour < 10 ? "0" + prevHour : "" + prevHour;
                        yellowCountRow[lHourHeader] = applyForecastCount ? yellowForecastCount : yellowGridCount;
                        redCountRow[lHourHeader] = applyForecastCount ? redForecastCount : redGridCount;
                    }
					diskGrid.Rows.Add(yellowCountRow);
					diskGrid.Rows.Add(redCountRow);
				}
				catch (Exception ex) {
					log.Error("************ [JobProcessorChart] GetDiskQueueAlertColor Error ************");
					log.Error(ex.Message);
				}
            }

            if (exceptionList.Count > 0) {
                ExpectionBulkInsert(databaseName, exceptionList, tempSaveLocation);
            }

            return diskGrid;
        }

        public DataTable GetStorageAlertColor(string systemSerial, DateTime startDate, DateTime endDate, long interval, Dictionary<int, SystemWeekInfo> systemweekInfo,
            double businessTolerance, double batchTolerance, double otherTolerance, List<ForecastStorageData> forecastData, bool alertException, DataTable diskGrid, ILog log, string tempSaveLocation) {
            

            var dailyDisk = new DailyDisk(ConnectionStringSPAM);
            var storageData = dailyDisk.GetDailyDiskInfo(startDate.Date, endDate.Date);

            var alertExceptionColor = Color.White;
            int yellowForecastCount = 0;
            int redForecastCount = 0;

            var exceptionList = new List<ExceptionView>();
            var totalMaxValue = 0d;
            if (storageData.Rows.Count > 0) {
                DataRow yellowCountRow = diskGrid.NewRow();
                DataRow redCountRow = diskGrid.NewRow();
                int yellowGridCount = 0, redGridCount = 0;
                yellowCountRow["Entity"] = "Storage";
                yellowCountRow["Counter"] = "Used %";
                redCountRow["Entity"] = "Storage";
                redCountRow["Counter"] = "Used %";

                bool applyForecastCount = false;
                foreach (DataRow row in storageData.Rows) {
                    var deviceName = row["DeviceName"].ToString();
                    var fromTimestamp = Convert.ToDateTime(row["FromTimestamp"]);
                    var usedPercent = Convert.ToDouble(row["UsedPercent"]);

                    if (forecastData.Count > 0) {
                        var isForecastData = forecastData.Any(x => x.ForecastDateTime.Equals(fromTimestamp) && x.DeviceName.Equals(deviceName));
                        if (alertException && isForecastData) {
                            applyForecastCount = true;
                            var forecastDataSub = forecastData.Where(x => x.ForecastDateTime.Equals(fromTimestamp) &&
                                                                          x.DeviceName.Equals(deviceName)).Select(x => x.UsedPercent).FirstOrDefault();
                            var stdDev = forecastData.Where(x => x.ForecastDateTime.Equals(fromTimestamp) &&
                                                                         x.DeviceName.Equals(deviceName)).Select(x => x.StdDevUsedPercent).FirstOrDefault();

                            var systemWeekThresholds = new SystemWeekThresholdsRepository();
                            var threadholds = systemWeekThresholds.GetStorage(systemSerial, 1);

                            var defaultValueMajor = GetThreasholdDefaultValue(1, "StorageMajor");
                            var exceptionMajor = defaultValueMajor;
                            if (threadholds.Rows.Count > 0)
                                exceptionMajor = threadholds.Rows[0].IsNull("StorageMajor") ? defaultValueMajor : Convert.ToDouble(threadholds.Rows[0]["StorageMajor"]);
                            var upperRange = forecastDataSub + exceptionMajor + stdDev;
                            var lowerRange = Math.Abs(forecastDataSub - (exceptionMajor + stdDev));
                            
                            if (usedPercent > upperRange) {
                                redForecastCount++;
                                alertExceptionColor = Color.Red;

                                exceptionList.Add(new ExceptionView {
                                    FromTimestamp = fromTimestamp,
                                    Instance = deviceName,
                                    EntityId = "Storage",
                                    CounterId = "Used%",
                                    Actual = usedPercent,
                                    Upper = upperRange,
                                    Lower = lowerRange,
                                    DisplayRed = true
                                });
                            }
                            else {
                                var defaultValueMinor = GetThreasholdDefaultValue(1, "StorageMinor");
                                var exceptionMinor = defaultValueMinor;
                                if (threadholds.Rows.Count > 0)
                                    exceptionMinor = threadholds.Rows[0].IsNull("StorageMinor") ? defaultValueMinor : Convert.ToDouble(threadholds.Rows[0]["StorageMinor"]);
                                var upperRangeSub = forecastDataSub + exceptionMinor + stdDev;
                                var lowerRangeSub = Math.Abs(forecastDataSub - (exceptionMinor + stdDev));

                                if (usedPercent > upperRangeSub) {
                                    yellowForecastCount++;
                                    if (alertExceptionColor != Color.Red) {
                                        alertExceptionColor = Color.Yellow;
                                    }
                                    exceptionList.Add(new ExceptionView {
                                        FromTimestamp = fromTimestamp,
                                        Instance = deviceName,
                                        EntityId = "Storage",
                                        CounterId = "Used%",
                                        Actual = usedPercent,
                                        Upper = upperRangeSub,
                                        Lower = lowerRangeSub,
                                        DisplayRed = false
                                    });
                                }
                            }
                        }
                        else {
                            if (usedPercent > totalMaxValue)
                                totalMaxValue = usedPercent;
                            if (usedPercent >= 80 && usedPercent < 90) {
                                yellowGridCount++;
                            }
                            else if (usedPercent >= 90) {
                                redGridCount++;
                            }
                        }
                    }
                    else {
                        if (usedPercent > totalMaxValue)
                            totalMaxValue = usedPercent;
                    }
                }

				try {
					yellowCountRow[2] = applyForecastCount ? yellowForecastCount : yellowGridCount;
					redCountRow[2] = applyForecastCount ? redForecastCount : redGridCount;
					diskGrid.Rows.Add(yellowCountRow);
					diskGrid.Rows.Add(redCountRow);
				}
				catch (Exception ex) {
					log.Error("************ [JobProcessorChart] GetStorageAlertColor Error ************");
					log.Error(ex.Message);
				}
            }

            if (exceptionList.Count > 0) {
                var databaseName = Helper.FindKeyName(ConnectionStringSPAM, "DATABASE");
                ExpectionBulkInsert(databaseName, exceptionList, tempSaveLocation);
            }

            return diskGrid;
        }
        private string CreateIntervalCPUBusyDundasChart(string systemSerial, DateTime[] dataDate, DateTime startTime, DateTime endTime, string path, long interval, ref bool hourDrop,
            ref List<System.DateTime[]> hourDropPeriods) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            string dateFormat = "yyyy-MM-dd HH:mm:ss";
            var chart = new Chart();
            chart.Width = 900;
            chart.Height = 250;
            chart.Palette = ChartColorPalette.EarthTones;
            var chartarea = new ChartArea();
            chartarea.AxisY.LabelStyle.Font = new Font("Calibri", 8);
            chartarea.AxisX.LabelStyle.Font = new Font("Calibri", 8);
            chartarea.AxisY.IsLabelAutoFit = false;
            chartarea.AxisX.IsLabelAutoFit = false;
            chartarea.AxisY.LabelStyle.Format = "{#}%";

            chartarea.AxisY.MajorGrid.Enabled = true;
            chartarea.BorderWidth = 0;
            chartarea.AxisY.Maximum = 100;
            chartarea.AxisY.Interval = 10;
            chartarea.AxisY.Title = "CPU Busy (%)";
            //Display Axis every 30 mins.
            if (interval == 1800)
                chartarea.AxisX.Interval = 1;
            else {
                if (interval > 1800) {
                    var newInterval = interval / 1800;
                    chartarea.AxisX.Interval = (int)newInterval;
                }
                else {
                    var newInterval = 1800 / interval;
                    chartarea.AxisX.Interval = (int)newInterval;
                }
            }
            chartarea.AxisX.LabelStyle.Angle = -60;
            if (dataDate.Length == 1) {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.MajorTickMark.Enabled = false;
                //chartarea.AxisX.IntervalType = DateTimeIntervalType.Hours;
                //chartarea.AxisX.IntervalOffsetType = DateTimeIntervalType.Hours;
                chartarea.AxisX.LabelStyle.Format = "HH:mm";
            }
            else if (dataDate.Length > 1 && dataDate.Length <= 7) {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.MajorTickMark.Enabled = false;
                //chartarea.AxisX.IntervalType = DateTimeIntervalType.Hours;
                //chartarea.AxisX.IntervalOffsetType = DateTimeIntervalType.Auto;
                chartarea.AxisX.LabelStyle.Format = "HH:mm";
            }
            else if (dataDate.Length <= 14) {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.IntervalType = DateTimeIntervalType.Days;
                chartarea.AxisX.IntervalOffset = 0;
                chartarea.AxisX.IntervalOffsetType = DateTimeIntervalType.Auto;
                chartarea.AxisX.LabelStyle.Format = "MMM/dd/yy\ndddd";
            }
            else {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.IntervalType = DateTimeIntervalType.Days;
                chartarea.AxisX.IntervalOffset = 0;
                chartarea.AxisX.IntervalOffsetType = DateTimeIntervalType.Auto;
                chartarea.AxisX.LabelStyle.Format = "MMM/dd/yy";
            }
            chartarea.AxisY.TitleFont = new Font("Calibri", 8);
            chartarea.AxisY.MajorGrid.LineColor = Color.Silver;
            chartarea.AxisX.MajorGrid.LineColor = Color.Silver;

            //Legend format.
            chart.Legends.Clear();
            chart.Legends.Add("Default");
            chart.Legends[0].LegendStyle = LegendStyle.Row;
            chart.Legends[0].IsDockedInsideChartArea = true;
            chart.Legends[0].Alignment = StringAlignment.Center;
            chart.Legends[0].Font = new Font("Calibri", 6);
            chart.Legends[0].BorderDashStyle = ChartDashStyle.NotSet;
            chart.Legends[0].Docking = Docking.Bottom;

            //Get data from detail table.
            var databaseMapService = new DatabaseMappingService(ConnectionString);
            string newConnectionString = databaseMapService.GetConnectionStringFor(systemSerial);


            //Get TableNames.
            var databaseCheck = new Database(newConnectionString);
            var databaseName = Helper.FindKeyName(newConnectionString, "DATABASE");
            var cpuTrendTable = new CPUTrendTableRepository(newConnectionString);
            var dataTable = cpuTrendTable.GetCPUBusyInterval(startTime.ToString(dateFormat), endTime.ToString(dateFormat));

            //Add Average CPU Busy.
            try {
                var serialBusyAverage = new Series("Average");
                serialBusyAverage.ChartType = SeriesChartType.SplineArea;
                serialBusyAverage["PointWidth"] = "0.8";
                serialBusyAverage.LegendText = "Average";
                serialBusyAverage.ToolTip = "Average";
                serialBusyAverage.Color = Color.LightSteelBlue;

                serialBusyAverage.IsXValueIndexed = true;
                serialBusyAverage.XValueType = ChartValueType.DateTime;
                serialBusyAverage.XAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");
                serialBusyAverage.YAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");

                var hourDropStart = new System.DateTime();
                var hourDropEnd = new System.DateTime();
                //loop through each date and check if we not missing any data.
                for (var start = startTime; start < endTime; start = start.AddSeconds(interval)) {
                    var dpAdjust = new DataPoint();
                    if (dataTable.AsEnumerable().Any(x => x.Field<DateTime>("Date & Time").Equals(start))) {
                        var averageValue = dataTable.AsEnumerable().Where(x => x.Field<DateTime>("Date & Time").Equals(start)).Average(x => x.Field<double>("Busy"));
                        dpAdjust.XValue = start.ToOADate();
                        dpAdjust.YValues[0] = averageValue;
                        serialBusyAverage.Points.Add(dpAdjust);
                        if (!hourDropStart.Equals(new System.DateTime()))
                        {
                            hourDropPeriods.Add(new System.DateTime[] { hourDropStart, hourDropEnd });
                            hourDropStart = new System.DateTime();
                            hourDropEnd = new System.DateTime();
                        }
                    }
                    else {
                        dpAdjust.XValue = start.ToOADate();
                        dpAdjust.YValues[0] = 0;
                        serialBusyAverage.Points.Add(dpAdjust);
                        hourDrop = true;
                        if (hourDropStart.Equals(new System.DateTime()))
                        {
                            hourDropStart = start;
                        }
                        hourDropEnd = start.AddSeconds(interval);
                    }
                }
                chart.Series.Add(serialBusyAverage);
            }
            catch {
            }



            try {
                var subData = dataTable.AsEnumerable().Select(x => x.Field<UInt64>("CPUNumber")).Distinct();
                foreach (var y in subData) {


                    //myDataTable = myDataTable.AsEnumerable().OrderBy(x => x.Field<DateTime>("Date & Time")).CopyToDataTable();
                    var serialBusy = new Series("CPU" + y);
                    serialBusy.ChartType = SeriesChartType.Spline;
                    serialBusy["PointWidth"] = "0.8";
                    // Set series tooltips
                    if (y < 10) {
                        serialBusy.LegendText = "0" + y;
                        serialBusy.ToolTip = "CPU 0" + y;
                    }
                    else {
                        serialBusy.LegendText = " " + y;
                        serialBusy.ToolTip = "CPU " + y;
                    }

                    serialBusy.IsXValueIndexed = true;
                    serialBusy.XValueType = ChartValueType.DateTime;
                    serialBusy.XAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");
                    serialBusy.YAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");

                    var myDataTable = dataTable.AsEnumerable().Where(x => x.Field<UInt64>("CPUNumber").Equals(y)).OrderBy(x => x.Field<DateTime>("Date & Time")).CopyToDataTable();
                    //populate all the data.
                    for (var start = startTime; start < endTime; start = start.AddSeconds(interval)) {
                        if (myDataTable.AsEnumerable().Any(x => x.Field<DateTime>("Date & Time").Equals(start))) {
                            //DataRow newRow = myDataTable.NewRow();
                            //newRow["Date & Time"] = start;
                            //newRow["Busy"] = 0;
                            //newRow["CPUNumber"] = y;
                            //myDataTable.Rows.Add(newRow);
                            var averageValue = myDataTable.AsEnumerable().Where(x => x.Field<DateTime>("Date & Time").Equals(start)).Average(x => x.Field<double>("Busy"));
                            var dpAdjust = new DataPoint();
                            dpAdjust.XValue = Convert.ToDateTime(start).ToOADate();
                            dpAdjust.YValues[0] = averageValue; //Convert.ToDouble(myDataTable.Rows[x]["Busy"]);
                            serialBusy.Points.Add(dpAdjust);
                        }
                        else {
                            var dpAdjust = new DataPoint();
                            dpAdjust.XValue = Convert.ToDateTime(start).ToOADate();
                            dpAdjust.YValues[0] = 0; //Convert.ToDouble(myDataTable.Rows[x]["Busy"]);
                            serialBusy.Points.Add(dpAdjust);
                            hourDrop = true;
                        }
                    }

                    //for (int x = 0; x < myDataTable.Rows.Count; x++) {
                    //    if (Convert.ToDateTime(myDataTable.Rows[x]["Date & Time"]) >= startTime &&
                    //        Convert.ToDateTime(myDataTable.Rows[x]["Date & Time"]) <= endTime) {
                    //        var dpAdjust = new DataPoint();
                    //        dpAdjust.XValue = Convert.ToDateTime(myDataTable.Rows[x]["Date & Time"]).ToOADate();
                    //        dpAdjust.YValues[0] = Convert.ToDouble(myDataTable.Rows[x]["Busy"]);
                    //        serialBusy.Points.Add(dpAdjust);
                    //    }
                    //    else {
                    //        var dpAdjust = new DataPoint();
                    //        dpAdjust.XValue = Convert.ToDateTime(myDataTable.Rows[x]["Date & Time"]).ToOADate();
                    //        dpAdjust.YValues[0] = 0;
                    //        serialBusy.Points.Add(dpAdjust);
                    //    }
                    //}

                    chart.Series.Add(serialBusy);
                }
            }
            catch (Exception e) {

            }

            chart.ChartAreas.Add(chartarea);

            chart.ImageType = ChartImageType.Jpeg;
            //chart.ImageUrl = "TempImg\\ChartPic_#SEQ(100,5)";
            if (!Directory.Exists(path + "TempImg\\")) {
                Directory.CreateDirectory(path + "TempImg\\");
            }
            string saveLocation = path + "TempImg\\ChartPicBusy_" + DateTime.Now.Ticks + ".jpg";
            //Save the chart image.
            chart.SaveImage(saveLocation);
            return saveLocation;
        }

        private string CreateIntervalCPUQueueDundasChart(string systemSerial, DateTime[] dataDate, DateTime startTime, DateTime endTime, string path, long interval) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            string dateFormat = "yyyy-MM-dd HH:mm:ss";
            var chart = new Chart();
            chart.Width = 900;
            chart.Height = 250;
            chart.Palette = ChartColorPalette.EarthTones;
            var chartarea = new ChartArea();
            chartarea.AxisY.LabelStyle.Font = new Font("Calibri", 8);
            chartarea.AxisX.LabelStyle.Font = new Font("Calibri", 8);
            chartarea.AxisY.IsLabelAutoFit = false;
            chartarea.AxisX.IsLabelAutoFit = false;
            //chartarea.AxisY.LabelStyle.Format = "N0";

            chartarea.AxisY.MajorGrid.Enabled = true;
            chartarea.BorderWidth = 0;
            chartarea.AxisY.Title = "CPU Queue Length";
            //Display Axis every 30 mins.
            if (interval == 1800)
                chartarea.AxisX.Interval = 1;
            else {
                if (interval > 1800) {
                    var newInterval = interval / 1800;
                    chartarea.AxisX.Interval = (int)newInterval;
                }
                else {
                    var newInterval = 1800 / interval;
                    chartarea.AxisX.Interval = (int)newInterval;
                }
            }
            chartarea.AxisX.LabelStyle.Angle = -60;
            if (dataDate.Length == 1) {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.MajorTickMark.Enabled = false;
                //chartarea.AxisX.IntervalType = DateTimeIntervalType.Hours;
                //chartarea.AxisX.IntervalOffsetType = DateTimeIntervalType.Hours;
                chartarea.AxisX.LabelStyle.Format = "HH:mm";
            }
            else if (dataDate.Length > 1 && dataDate.Length <= 7) {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.MajorTickMark.Enabled = false;
                //chartarea.AxisX.IntervalType = DateTimeIntervalType.Hours;
                //chartarea.AxisX.IntervalOffsetType = DateTimeIntervalType.Auto;
                chartarea.AxisX.LabelStyle.Format = "HH:mm";
            }
            else if (dataDate.Length <= 14) {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.IntervalType = DateTimeIntervalType.Days;
                chartarea.AxisX.IntervalOffset = 0;
                chartarea.AxisX.IntervalOffsetType = DateTimeIntervalType.Auto;
                chartarea.AxisX.LabelStyle.Format = "MMM/dd/yy\ndddd";
            }
            else {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.IntervalType = DateTimeIntervalType.Days;
                chartarea.AxisX.IntervalOffset = 0;
                chartarea.AxisX.IntervalOffsetType = DateTimeIntervalType.Auto;
                chartarea.AxisX.LabelStyle.Format = "MMM/dd/yy";
            }
            chartarea.AxisY.TitleFont = new Font("Calibri", 8);
            chartarea.AxisY.MajorGrid.LineColor = Color.Silver;
            chartarea.AxisX.MajorGrid.LineColor = Color.Silver;

            //Legend format.
            chart.Legends.Clear();
            chart.Legends.Add("Default");
            chart.Legends[0].LegendStyle = LegendStyle.Row;
            chart.Legends[0].IsDockedInsideChartArea = true;
            chart.Legends[0].Alignment = StringAlignment.Center;
            chart.Legends[0].Font = new Font("Calibri", 6);
            chart.Legends[0].BorderDashStyle = ChartDashStyle.NotSet;
            chart.Legends[0].Docking = Docking.Bottom;

            /*//Add title.
            var title = new Title();
            if (dataDate.Length == 1)
                title.Text = "Period for " + dataDate[0].ToLongDateString();
            else
                title.Text = "Period from " + dataDate[0].ToLongDateString() + " through " + dataDate[dataDate.Length - 1].ToLongDateString();

            title.Font = new Font("Calibri", 10);
            chart.Titles.Add(title);*/
            //Get data from detail table.
            var databaseMapService = new DatabaseMappingService(ConnectionString);
            string newConnectionString = databaseMapService.GetConnectionStringFor(systemSerial);


            //Get TableNames.
            var databaseCheck = new Database(newConnectionString);
            var databaseName = Helper.FindKeyName(newConnectionString, "DATABASE");
            var cpuTrendTable = new CPUTrendTableRepository(newConnectionString);
            var dataTable = cpuTrendTable.GetCPUQueueInterval(startTime.ToString(dateFormat), endTime.ToString(dateFormat));

            //Add Average CPU Busy.
            try {
                var serialBusyAverage = new Series("Average");
                serialBusyAverage.ChartType = SeriesChartType.SplineArea;
                serialBusyAverage["PointWidth"] = "0.8";
                serialBusyAverage.LegendText = "Average";
                serialBusyAverage.ToolTip = "Average";
                serialBusyAverage.Color = Color.LightSteelBlue;

                serialBusyAverage.IsXValueIndexed = true;
                serialBusyAverage.XValueType = ChartValueType.DateTime;
                serialBusyAverage.XAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");
                serialBusyAverage.YAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");

                //loop through each date and check if we not missing any data.
                for (var start = startTime; start < endTime; start = start.AddSeconds(interval)) {
                    var dpAdjust = new DataPoint();
                    if (dataTable.AsEnumerable().Any(x => x.Field<DateTime>("Date & Time").Equals(start))) {
                        var averageValue = dataTable.AsEnumerable().Where(x => x.Field<DateTime>("Date & Time").Equals(start)).Average(x => x.Field<double>("Queue"));
                        dpAdjust.XValue = start.ToOADate();
                        dpAdjust.YValues[0] = averageValue;
                        serialBusyAverage.Points.Add(dpAdjust);
                    }
                    else {
                        dpAdjust.XValue = start.ToOADate();
                        dpAdjust.YValues[0] = 0;
                        serialBusyAverage.Points.Add(dpAdjust);
                    }
                }
                chart.Series.Add(serialBusyAverage);
            }
            catch {
            }

            try {
                var subData = dataTable.AsEnumerable().Select(x => x.Field<UInt64>("CPUNumber")).Distinct();
                foreach (var y in subData) {


                    //myDataTable = myDataTable.AsEnumerable().OrderBy(x => x.Field<DateTime>("Date & Time")).CopyToDataTable();

                    var serialBusy = new Series("CPU" + y);
                    serialBusy.ChartType = SeriesChartType.Spline;
                    serialBusy["PointWidth"] = "0.8";
                    // Set series tooltips
                    if (y < 10) {
                        serialBusy.LegendText = "0" + y;
                        serialBusy.ToolTip = "CPU 0" + y;
                    }
                    else {
                        serialBusy.LegendText = " " + y;
                        serialBusy.ToolTip = "CPU " + y;
                    }

                    serialBusy.IsXValueIndexed = true;
                    serialBusy.XValueType = ChartValueType.DateTime;
                    serialBusy.XAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");
                    serialBusy.YAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");

                    var myDataTable = dataTable.AsEnumerable().Where(x => x.Field<UInt64>("CPUNumber").Equals(y)).OrderBy(x => x.Field<DateTime>("Date & Time")).CopyToDataTable();
                    //loop through each date and check if we not missing any data.
                    for (var start = startTime; start < endTime; start = start.AddSeconds(interval)) {
                        if (myDataTable.AsEnumerable().Any(x => x.Field<DateTime>("Date & Time").Equals(start))) {
                            var averageValue = myDataTable.AsEnumerable().Where(x => x.Field<DateTime>("Date & Time").Equals(start)).Average(x => x.Field<double>("Queue"));
                            var dpAdjust = new DataPoint();
                            dpAdjust.XValue = start.ToOADate();
                            dpAdjust.YValues[0] = averageValue;
                            serialBusy.Points.Add(dpAdjust);
                        }
                        else {
                            var dpAdjust = new DataPoint();
                            dpAdjust.XValue = start.ToOADate();
                            dpAdjust.YValues[0] = 0;
                            serialBusy.Points.Add(dpAdjust);
                        }
                    }

                    chart.Series.Add(serialBusy);
                }
            }
            catch {
            }
            chart.ChartAreas.Add(chartarea);

            chart.ImageType = ChartImageType.Jpeg;
            //chart.ImageUrl = "TempImg\\ChartPic_#SEQ(100,5)";
            if (!Directory.Exists(path + "TempImg\\")) {
                Directory.CreateDirectory(path + "TempImg\\");
            }
            string saveLocation = path + "TempImg\\ChartPicBusy_" + DateTime.Now.Ticks + ".jpg";
            //Save the chart image.
            chart.SaveImage(saveLocation);
            return saveLocation;
        }

        private string CreateApplicationIntervalCPUBusyDundasChart(string systemSerial, DateTime[] dataDate, DateTime startTime, DateTime endTime, string path, long interval) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            var chart = new Chart();
            chart.Width = 900;
            chart.Height = 250;
            chart.Palette = ChartColorPalette.EarthTones;
            var chartarea = new ChartArea();
            chartarea.AxisY.LabelStyle.Font = new Font("Calibri", 8);
            chartarea.AxisX.LabelStyle.Font = new Font("Calibri", 8);
            chartarea.AxisY.IsLabelAutoFit = false;
            chartarea.AxisX.IsLabelAutoFit = false;
            chartarea.AxisY.LabelStyle.Format = "{#}%";

            chartarea.AxisY.MajorGrid.Enabled = true;
            chartarea.BorderWidth = 0;
            //chartarea.AxisY.Interval = 10;
            chartarea.AxisY.Title = "CPU Busy (%)";

            //AxisY2
            chartarea.AxisY2.Maximum = Double.NaN;
            chartarea.AxisY2.Enabled = AxisEnabled.True;
            chartarea.AxisY2.Minimum = 0;
            chartarea.AxisY2.RoundAxisValues();
            chartarea.AxisY2.LabelStyle.Font = new Font("Calibri", 8);
            chartarea.AxisY2.TitleFont = new Font("Calibri", 8);
            chartarea.AxisY2.IsLabelAutoFit = false;
            chartarea.AxisY2.LabelStyle.Format = "#,0";
            chartarea.AxisY2.Title = "Disk IOs/sec";
            chartarea.AxisY2.MajorGrid.Enabled = false;

            //Display Axis every 30 mins.
            if (interval == 1800)
                chartarea.AxisX.Interval = 1;
            else {
                if (interval > 1800) {
                    var newInterval = interval / 1800;
                    chartarea.AxisX.Interval = (int)newInterval;
                }
                else {
                    var newInterval = 1800 / interval;
                    chartarea.AxisX.Interval = (int)newInterval;
                }
            }
            chartarea.AxisX.LabelStyle.Angle = -60;
            if (dataDate.Length == 1) {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.MajorTickMark.Enabled = false;
                chartarea.AxisX.LabelStyle.Format = "HH:mm";
            }
            else if (dataDate.Length > 1 && dataDate.Length <= 7) {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.MajorTickMark.Enabled = false;
                chartarea.AxisX.LabelStyle.Format = "HH:mm";
            }
            else if (dataDate.Length <= 14) {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.IntervalType = DateTimeIntervalType.Days;
                chartarea.AxisX.IntervalOffset = 0;
                chartarea.AxisX.IntervalOffsetType = DateTimeIntervalType.Auto;
                chartarea.AxisX.LabelStyle.Format = "MMM/dd/yy\ndddd";
            }
            else {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.IntervalType = DateTimeIntervalType.Days;
                chartarea.AxisX.IntervalOffset = 0;
                chartarea.AxisX.IntervalOffsetType = DateTimeIntervalType.Auto;
                chartarea.AxisX.LabelStyle.Format = "MMM/dd/yy";
            }
            chartarea.AxisY.TitleFont = new Font("Calibri", 8);
            chartarea.AxisY.MajorGrid.LineColor = Color.Silver;
            chartarea.AxisX.MajorGrid.LineColor = Color.Silver;

            //Legend format.
            chart.Legends.Clear();
            chart.Legends.Add("Default");
            chart.Legends[0].LegendStyle = LegendStyle.Row;
            chart.Legends[0].IsDockedInsideChartArea = true;
            chart.Legends[0].Alignment = StringAlignment.Center;
            chart.Legends[0].Font = new Font("Calibri", 6);
            chart.Legends[0].BorderDashStyle = ChartDashStyle.NotSet;
            chart.Legends[0].Docking = Docking.Bottom;

            //Get data from detail table.
            var databaseMapService = new DatabaseMappingService(ConnectionString);
            string newConnectionString = databaseMapService.GetConnectionStringFor(systemSerial);
            
            var cpuEntityTable = new CPUEntityTable(newConnectionString);
            var dataTable = cpuEntityTable.GetApplicationBusy(startTime, endTime);


            try {
                var subData = dataTable.AsEnumerable().Select(x => x.Field<string>("ApplicationName")).Distinct();
                foreach (var y in subData) {
                    var serialBusy = new Series("DiskIO" + y);
                    serialBusy.ChartType = SeriesChartType.StackedColumn;
                    serialBusy["PointWidth"] = "0.8";
                    // Set series tooltips
                    serialBusy.LegendText = y;

                    serialBusy.IsXValueIndexed = true;
                    serialBusy.XValueType = ChartValueType.DateTime;
                    serialBusy.XAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");
                    serialBusy.YAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Secondary");

                    var myDataTable = dataTable.AsEnumerable().Where(x => x.Field<string>("ApplicationName").Equals(y)).OrderBy(x => x.Field<DateTime>("Date & Time")).CopyToDataTable();
                    //populate all the data.
                    for (var start = startTime; start < endTime; start = start.AddSeconds(interval)) {
                        if (myDataTable.AsEnumerable().Any(x => x.Field<DateTime>("Date & Time").Equals(start))) {
                            var averageValue = myDataTable.AsEnumerable().Where(x => x.Field<DateTime>("Date & Time").Equals(start)).Average(x => x.Field<double>("DiskIO"));
                            var dpAdjust = new DataPoint();
                            dpAdjust.XValue = Convert.ToDateTime(start).ToOADate();
                            dpAdjust.YValues[0] = averageValue;
                            serialBusy.Points.Add(dpAdjust);
                        }
                        else {
                            var dpAdjust = new DataPoint();
                            dpAdjust.XValue = Convert.ToDateTime(start).ToOADate();
                            dpAdjust.YValues[0] = 0;
                            serialBusy.Points.Add(dpAdjust);
                        }
                    }
                    chart.Series.Add(serialBusy);
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }

            try {
                var subData = dataTable.AsEnumerable().Select(x => x.Field<string>("ApplicationName")).Distinct();
                foreach (var y in subData) {
                    var serialBusy = new Series("Busy" + y);
                    serialBusy.ChartType = SeriesChartType.Spline;
                    serialBusy["PointWidth"] = "0.8";
                    // Set series tooltips
                    serialBusy.LegendText = y;

                    serialBusy.IsXValueIndexed = true;
                    serialBusy.XValueType = ChartValueType.DateTime;
                    serialBusy.XAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");
                    serialBusy.YAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");

                    var myDataTable = dataTable.AsEnumerable().Where(x => x.Field<string>("ApplicationName").Equals(y)).OrderBy(x => x.Field<DateTime>("Date & Time")).CopyToDataTable();
                    //populate all the data.
                    for (var start = startTime; start < endTime; start = start.AddSeconds(interval)) {
                        if (myDataTable.AsEnumerable().Any(x => x.Field<DateTime>("Date & Time").Equals(start))) {
                            var averageValue = myDataTable.AsEnumerable().Where(x => x.Field<DateTime>("Date & Time").Equals(start)).Average(x => x.Field<double>("CpuBusy"));
                            var dpAdjust = new DataPoint();
                            dpAdjust.XValue = Convert.ToDateTime(start).ToOADate();
                            dpAdjust.YValues[0] = averageValue;
                            serialBusy.Points.Add(dpAdjust);
                        }
                        else {
                            var dpAdjust = new DataPoint();
                            dpAdjust.XValue = Convert.ToDateTime(start).ToOADate();
                            dpAdjust.YValues[0] = 0;
                            serialBusy.Points.Add(dpAdjust);
                        }
                    }
                    chart.Series.Add(serialBusy);
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }

            chart.ChartAreas.Add(chartarea);

            chart.ImageType = ChartImageType.Jpeg;
            //chart.ImageUrl = "TempImg\\ChartPic_#SEQ(100,5)";
            if (!Directory.Exists(path + "TempImg\\")) {
                Directory.CreateDirectory(path + "TempImg\\");
            }
            string saveLocation = path + "TempImg\\ChartPicApplicationBusy_" + DateTime.Now.Ticks + ".jpg";
            //Save the chart image.
            chart.SaveImage(saveLocation);
            return saveLocation;
        }

        private string CreateIntervalIPUBusyDundasChart(string systemSerial, DateTime[] dataDate, DateTime startTime, DateTime endTime, string path, long interval) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            string dateFormat = "yyyy-MM-dd HH:mm:ss";
            var chart = new Chart();
            chart.Width = 900;
            chart.Height = 250;
            chart.Palette = ChartColorPalette.EarthTones;
            var chartarea = new ChartArea();
            chartarea.AxisY.LabelStyle.Font = new Font("Calibri", 8);
            chartarea.AxisX.LabelStyle.Font = new Font("Calibri", 8);
            chartarea.AxisY.IsLabelAutoFit = false;
            chartarea.AxisX.IsLabelAutoFit = false;
            chartarea.AxisY.LabelStyle.Format = "{#}%";

            chartarea.AxisY.MajorGrid.Enabled = true;
            chartarea.BorderWidth = 0;
            chartarea.AxisY.Maximum = 100;
            chartarea.AxisY.Interval = 10;
            chartarea.AxisY.Title = "IPU Busy (%)";
            //Display Axis every 30 mins.
            if (interval == 1800)
                chartarea.AxisX.Interval = 1;
            else {
                if (interval > 1800) {
                    var newInterval = interval / 1800;
                    chartarea.AxisX.Interval = (int)newInterval;
                }
                else {
                    var newInterval = 1800 / interval;
                    chartarea.AxisX.Interval = (int)newInterval;
                }
            }
            chartarea.AxisX.LabelStyle.Angle = -60;
            if (dataDate.Length == 1) {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.MajorTickMark.Enabled = false;
                chartarea.AxisX.LabelStyle.Format = "HH:mm";
            }
            else if (dataDate.Length > 1 && dataDate.Length <= 7) {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.MajorTickMark.Enabled = false;
                chartarea.AxisX.LabelStyle.Format = "HH:mm";
            }
            else if (dataDate.Length <= 14) {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.IntervalType = DateTimeIntervalType.Days;
                chartarea.AxisX.IntervalOffset = 0;
                chartarea.AxisX.IntervalOffsetType = DateTimeIntervalType.Auto;
                chartarea.AxisX.LabelStyle.Format = "MMM/dd/yy\ndddd";
            }
            else {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.IntervalType = DateTimeIntervalType.Days;
                chartarea.AxisX.IntervalOffset = 0;
                chartarea.AxisX.IntervalOffsetType = DateTimeIntervalType.Auto;
                chartarea.AxisX.LabelStyle.Format = "MMM/dd/yy";
            }
            chartarea.AxisY.TitleFont = new Font("Calibri", 8);
            chartarea.AxisY.MajorGrid.LineColor = Color.Silver;
            chartarea.AxisX.MajorGrid.LineColor = Color.Silver;

            //Legend format.
            chart.Legends.Clear();
            chart.Legends.Add("Default");
            chart.Legends[0].LegendStyle = LegendStyle.Row;
            chart.Legends[0].IsDockedInsideChartArea = true;
            chart.Legends[0].Alignment = StringAlignment.Center;
            chart.Legends[0].Font = new Font("Calibri", 6);
            chart.Legends[0].BorderDashStyle = ChartDashStyle.NotSet;
            chart.Legends[0].Docking = Docking.Bottom;

            //Get data from detail table.
            var databaseMapService = new DatabaseMappingService(ConnectionString);
            string newConnectionString = databaseMapService.GetConnectionStringFor(systemSerial);


            //Get TableNames.
            var databaseCheck = new Database(newConnectionString);
            var databaseName = Helper.FindKeyName(newConnectionString, "DATABASE");

            var cpuTrendTable = new CPUTrendTableRepository(newConnectionString);
            var dataTable = cpuTrendTable.GetIPUBusyInterval(startTime.ToString(dateFormat), endTime.ToString(dateFormat));
            
            DataTable converted = new DataTable();
            converted.Columns.Add("Date & Time");
            converted.Columns.Add("Busy");
            converted.Columns.Add("CPUIPU");
            converted.Columns[0].DataType = typeof(DateTime);
            converted.Columns[1].DataType = typeof(double);
            converted.Columns[2].DataType = typeof(string);
            for (int i = 0; i < dataTable.Rows.Count; i++) {
                object[] o = { dataTable.Rows[i][0],
                    dataTable.Rows[i][1],
                    dataTable.Rows[i][2] + "-" + dataTable.Rows[i][3] };
                converted.Rows.Add(o);
            }
            
            try {
                var subData = converted.AsEnumerable().Select(x => x.Field<string>("CPUIPU")).Distinct();
                foreach (var y in subData) {
                    var serialBusy = new Series("CPUIPU" + y);
                    serialBusy.ChartType = SeriesChartType.Spline;
                    serialBusy["PointWidth"] = "0.8";
                    // Set series tooltips
                    var split = y.ToString().Split('-');
                    var cpu = int.Parse(split[0]);
                    var ipu = int.Parse(split[1]);

                    if (cpu < 10) {
                        if (ipu < 10) {
                            serialBusy.LegendText = "0" + cpu + "-" + "0" + (ipu - 1);
                        }
                        else {
                            serialBusy.LegendText = "0" + cpu + "-" + (ipu - 1);
                        }
                    }
                    else {
                        if (ipu < 10) {
                            serialBusy.LegendText = cpu + "-" + "0" + (ipu - 1);
                        }
                        else {
                            serialBusy.LegendText = cpu + "-" + (ipu - 1);
                        }
                    }
                    serialBusy.IsXValueIndexed = true;
                    serialBusy.XValueType = ChartValueType.DateTime;
                    serialBusy.XAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");
                    serialBusy.YAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");

                    var myDataTable = converted.AsEnumerable().Where(x => x.Field<string>("CPUIPU").Equals(y)).OrderBy(x => x.Field<DateTime>("Date & Time")).CopyToDataTable();
                    //populate all the data.
                    for (var start = startTime; start < endTime; start = start.AddSeconds(interval)) {
                        if (myDataTable.AsEnumerable().Any(x => x.Field<DateTime>("Date & Time").Equals(start))) {
                            var averageValue = myDataTable.AsEnumerable().Where(x => x.Field<DateTime>("Date & Time").Equals(start)).Average(x => x.Field<double>("Busy"));
                            var dpAdjust = new DataPoint();
                            dpAdjust.XValue = Convert.ToDateTime(start).ToOADate();
                            dpAdjust.YValues[0] = averageValue;
                            serialBusy.Points.Add(dpAdjust);
                        }
                        else {
                            var dpAdjust = new DataPoint();
                            dpAdjust.XValue = Convert.ToDateTime(start).ToOADate();
                            dpAdjust.YValues[0] = 0;
                            serialBusy.Points.Add(dpAdjust);
                        }
                    }
                    chart.Series.Add(serialBusy);
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }

            chart.ChartAreas.Add(chartarea);

            chart.ImageType = ChartImageType.Jpeg;
            //chart.ImageUrl = "TempImg\\ChartPic_#SEQ(100,5)";
            if (!Directory.Exists(path + "TempImg\\")) {
                Directory.CreateDirectory(path + "TempImg\\");
            }
            string saveLocation = path + "TempImg\\ChartPicBusy_" + DateTime.Now.Ticks + ".jpg";
            //Save the chart image.
            chart.SaveImage(saveLocation);
            return saveLocation;
        }

        private string CreateIntervalIPUQueueDundasChart(string systemSerial, DateTime[] dataDate, DateTime startTime, DateTime endTime, string path, long interval) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            string dateFormat = "yyyy-MM-dd HH:mm:ss"; var chart = new Chart();
            chart.Width = 900;
            chart.Height = 250;
            chart.Palette = ChartColorPalette.EarthTones;
            var chartarea = new ChartArea();
            chartarea.AxisY.LabelStyle.Font = new Font("Calibri", 8);
            chartarea.AxisX.LabelStyle.Font = new Font("Calibri", 8);
            chartarea.AxisY.IsLabelAutoFit = false;
            chartarea.AxisX.IsLabelAutoFit = false;
            //chartarea.AxisY.LabelStyle.Format = "N0";

            chartarea.AxisY.MajorGrid.Enabled = true;
            chartarea.BorderWidth = 0;
            chartarea.AxisY.Title = "IPU Queue Length";
            //Display Axis every 30 mins.
            if (interval == 1800)
                chartarea.AxisX.Interval = 1;
            else {
                if (interval > 1800) {
                    var newInterval = interval / 1800;
                    chartarea.AxisX.Interval = (int)newInterval;
                }
                else {
                    var newInterval = 1800 / interval;
                    chartarea.AxisX.Interval = (int)newInterval;
                }
            }
            chartarea.AxisX.LabelStyle.Angle = -60;
            if (dataDate.Length == 1) {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.MajorTickMark.Enabled = false;
                chartarea.AxisX.LabelStyle.Format = "HH:mm";
            }
            else if (dataDate.Length > 1 && dataDate.Length <= 7) {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.MajorTickMark.Enabled = false;
                chartarea.AxisX.LabelStyle.Format = "HH:mm";
            }
            else if (dataDate.Length <= 14) {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.IntervalType = DateTimeIntervalType.Days;
                chartarea.AxisX.IntervalOffset = 0;
                chartarea.AxisX.IntervalOffsetType = DateTimeIntervalType.Auto;
                chartarea.AxisX.LabelStyle.Format = "MMM/dd/yy\ndddd";
            }
            else {
                chartarea.AxisX.MajorGrid.Enabled = false;
                chartarea.AxisX.IntervalType = DateTimeIntervalType.Days;
                chartarea.AxisX.IntervalOffset = 0;
                chartarea.AxisX.IntervalOffsetType = DateTimeIntervalType.Auto;
                chartarea.AxisX.LabelStyle.Format = "MMM/dd/yy";
            }
            chartarea.AxisY.TitleFont = new Font("Calibri", 8);
            chartarea.AxisY.MajorGrid.LineColor = Color.Silver;
            chartarea.AxisX.MajorGrid.LineColor = Color.Silver;

            //Legend format.
            chart.Legends.Clear();
            chart.Legends.Add("Default");
            chart.Legends[0].LegendStyle = LegendStyle.Row;
            chart.Legends[0].IsDockedInsideChartArea = true;
            chart.Legends[0].Alignment = StringAlignment.Center;
            chart.Legends[0].Font = new Font("Calibri", 6);
            chart.Legends[0].BorderDashStyle = ChartDashStyle.NotSet;
            chart.Legends[0].Docking = Docking.Bottom;
            chart.Legends[0].MaximumAutoSize = 100;

            //Get data from detail table.
            var databaseMapService = new DatabaseMappingService(ConnectionString);
            string newConnectionString = databaseMapService.GetConnectionStringFor(systemSerial);


            //Get TableNames.
            var databaseCheck = new Database(newConnectionString);
            var databaseName = Helper.FindKeyName(newConnectionString, "DATABASE");
            var cpuTrendTable = new CPUTrendTableRepository(newConnectionString);
            var dataTable = cpuTrendTable.GetIPUQueueInterval(startTime.ToString(dateFormat), endTime.ToString(dateFormat));

            DataTable converted = new DataTable();
            converted.Columns.Add("Date & Time");
            converted.Columns.Add("Busy");
            converted.Columns.Add("CPUIPU");
            converted.Columns[0].DataType = typeof(DateTime);
            converted.Columns[1].DataType = typeof(double);
            converted.Columns[2].DataType = typeof(string);
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                object[] o = { dataTable.Rows[i][0],
                    dataTable.Rows[i][1],
                    dataTable.Rows[i][2] + "-" + dataTable.Rows[i][3] };
                converted.Rows.Add(o);
            }


            try
            {
                var subData = converted.AsEnumerable().Select(x => x.Field<string>("CPUIPU")).Distinct();
                foreach (var y in subData) {
                    var serialBusy = new Series("CPU" + y);
                    serialBusy.ChartType = SeriesChartType.Spline;
                    serialBusy["PointWidth"] = "0.8";
                    // Set series tooltips
                    var split = y.ToString().Split('-');
                    var cpu = int.Parse(split[0]);
                    var ipu = int.Parse(split[1]);

                    if (cpu < 10) {
                        if (ipu < 10) {
                            serialBusy.LegendText = "0" + cpu + "-" + "0" + (ipu - 1);
                        }
                        else {
                            serialBusy.LegendText = "0" + cpu + "-" + (ipu - 1);
                        }
                    }
                    else {
                        if (ipu < 10) {
                            serialBusy.LegendText = cpu + "-" + "0" + (ipu - 1);
                        }
                        else {
                            serialBusy.LegendText = cpu + "-" + (ipu - 1);
                        }
                    }

                    serialBusy.IsXValueIndexed = true;
                    serialBusy.XValueType = ChartValueType.DateTime;
                    serialBusy.XAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");
                    serialBusy.YAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");

                    var myDataTable = converted.AsEnumerable().Where(x => x.Field<string>("CPUIPU").Equals(y)).OrderBy(x => x.Field<DateTime>("Date & Time")).CopyToDataTable();
                    //loop through each date and check if we not missing any data.
                    for (var start = startTime; start < endTime; start = start.AddSeconds(interval)) {
                        if (myDataTable.AsEnumerable().Any(x => x.Field<DateTime>("Date & Time").Equals(start))) {
                            var averageValue = myDataTable.AsEnumerable().Where(x => x.Field<DateTime>("Date & Time").Equals(start)).Average(x => x.Field<double>("Busy"));
                            var dpAdjust = new DataPoint();
                            dpAdjust.XValue = start.ToOADate();
                            dpAdjust.YValues[0] = averageValue;
                            serialBusy.Points.Add(dpAdjust);
                        }
                        else {
                            var dpAdjust = new DataPoint();
                            dpAdjust.XValue = start.ToOADate();
                            dpAdjust.YValues[0] = 0;
                            serialBusy.Points.Add(dpAdjust);
                        }
                    }

                    chart.Series.Add(serialBusy);
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
            chart.ChartAreas.Add(chartarea);
            chart.ImageType = ChartImageType.Jpeg;
            if (!Directory.Exists(path + "TempImg\\")) {
                Directory.CreateDirectory(path + "TempImg\\");
            }
            string saveLocation = path + "TempImg\\ChartPicBusy_" + DateTime.Now.Ticks + ".jpg";
            //Save the chart image.
            chart.SaveImage(saveLocation);
            return saveLocation;
        }

        internal string CreateStorageToday(Dictionary<DateTime, DailyDiskInfo> storageGraphData, string path, string longDatePattern) {
            var chart = new Chart();
            chart.Width = 900;
            chart.Height = 250;
            chart.Palette = ChartColorPalette.EarthTones;
            var chartarea = new ChartArea();
            chartarea.AxisY.LabelStyle.Font = new Font("Calibri", 8);
            chartarea.AxisX.LabelStyle.Font = new Font("Calibri", 8);
            chartarea.AxisY.IsLabelAutoFit = false;
            chartarea.AxisX.IsLabelAutoFit = false;
            chartarea.AxisY.LabelStyle.Format = "#,0";
            chartarea.AxisY.Minimum = 0;
            chartarea.AxisX.Interval = 1;

            chartarea.AxisY.MajorGrid.Enabled = true;
            chartarea.BorderWidth = 0;
            var max = storageGraphData.First().Value.UsedGB + storageGraphData.First().Value.FreeGB;
            if (max > 1024)
                chartarea.AxisY.Title = "Capacity TB";
            else
                chartarea.AxisY.Title = "Capacity GB";

            chartarea.AxisX.LabelStyle.Angle = -60;
            chartarea.AxisX.MajorGrid.Enabled = false;
            chartarea.AxisX.IntervalType = DateTimeIntervalType.Days;
            chartarea.AxisX.IntervalOffset = 0;
            chartarea.AxisX.IntervalOffsetType = DateTimeIntervalType.Auto;
            chartarea.AxisX.LabelStyle.Format = longDatePattern; //"MMM/dd/yy\ndddd";
            chartarea.AxisY.TitleFont = new Font("Calibri", 8);
            chartarea.AxisY.MajorGrid.LineColor = Color.Silver;
            chartarea.AxisX.MajorGrid.LineColor = Color.Silver;

            //Legend format.
            chart.Legends.Clear();
            chart.Legends.Add("Default");
            chart.Legends[0].LegendStyle = LegendStyle.Row;
            chart.Legends[0].IsDockedInsideChartArea = true;
            chart.Legends[0].Alignment = StringAlignment.Center;
            chart.Legends[0].Font = new Font("Calibri", 6);
            chart.Legends[0].BorderDashStyle = ChartDashStyle.NotSet;
            chart.Legends[0].Docking = Docking.Bottom;

            try {
                var serialBusy = new Series("Used");
                serialBusy.ChartType = SeriesChartType.StackedColumn;
                serialBusy["PointWidth"] = "0.8";
                serialBusy.LegendText = "Used";
                serialBusy.ToolTip = "Used";
                serialBusy.Color = Color.Red;
                serialBusy.Font = new Font("Calibri", 8.0f, FontStyle.Regular);
                serialBusy.IsValueShownAsLabel = true;
                serialBusy.Label = "test";

                serialBusy.IsXValueIndexed = true;
                serialBusy.XValueType = ChartValueType.DateTime;
                serialBusy.XAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");
                serialBusy.YAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");

                //populate all the data.
                foreach (var value in storageGraphData) {
                    var dpAdjust = new DataPoint();
                    dpAdjust.XValue = Convert.ToDateTime(value.Key).ToOADate();
                    if (max > 1024)
                        dpAdjust.YValues[0] = Convert.ToDouble(value.Value.UsedGB / 1024);
                    else
                        dpAdjust.YValues[0] = Convert.ToDouble(value.Value.UsedGB);

                    dpAdjust.Label = string.Format("{0:00.00}", value.Value.UsedPercent) + "%";
                    serialBusy.Points.Add(dpAdjust);
                }

                chart.Series.Add(serialBusy);
            }
            catch {
            }

            try {
                var serialBusy = new Series("Free");
                serialBusy.ChartType = SeriesChartType.StackedColumn;
                serialBusy["PointWidth"] = "0.8";
                serialBusy.LegendText = "Free";
                serialBusy.ToolTip = "Free";
                serialBusy.Color = Color.Green;
                serialBusy.Font = new Font("Calibri", 8.0f, FontStyle.Regular);
                serialBusy.IsValueShownAsLabel = true;

                serialBusy.IsXValueIndexed = true;
                serialBusy.XValueType = ChartValueType.DateTime;
                serialBusy.XAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");
                serialBusy.YAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");

                //populate all the data.
                foreach (var value in storageGraphData) {
                    var dpAdjust = new DataPoint();
                    dpAdjust.XValue = Convert.ToDateTime(value.Key).ToOADate();
                    if (max > 1024)
                        dpAdjust.YValues[0] = Convert.ToDouble(value.Value.FreeGB / 1024);
                    else
                        dpAdjust.YValues[0] = Convert.ToDouble(value.Value.FreeGB);
                    dpAdjust.Label = string.Format("{0:00.00}", value.Value.FreePercent) + "%";
                    serialBusy.Points.Add(dpAdjust);
                }

                chart.Series.Add(serialBusy);
            }
            catch {
            }

            chart.ChartAreas.Add(chartarea);
            chart.ImageType = ChartImageType.Jpeg;
            if (!Directory.Exists(path + "TempImg\\")) {
                Directory.CreateDirectory(path + "TempImg\\");
            }
            string saveLocation = path + "TempImg\\ChartPicStorage_" + DateTime.Now.Ticks + ".jpg";
            //Save the chart image.
            chart.SaveImage(saveLocation);
            return saveLocation;
        }

        internal string GetWeekOfMonth(DateTime startTime) {
            DayOfWeek dayOfWeek = startTime.DayOfWeek;
            DateTime dayStep = new DateTime(startTime.Year, startTime.Month, 1);
            int returnValue = 0;

            while (dayStep <= startTime) {
                if (dayStep.DayOfWeek == dayOfWeek) {
                    returnValue++;
                }

                dayStep = dayStep.AddDays(1);
            }
            var numberOfMonth = "";

            if (returnValue.Equals(1))
                numberOfMonth = "1st ";
            else if (returnValue.Equals(2))
                numberOfMonth = "2nd ";
            else if (returnValue.Equals(3))
                numberOfMonth = "3rd ";
            else
                numberOfMonth = returnValue + "th ";

            return numberOfMonth;
        }

        public Color GetCPUBusyAlertColor(string systemSerial, DateTime startDate, DateTime endDate, long interval, Dictionary<int, SystemWeekInfo> systemweekInfo,
            double businessTolerance, double batchTolerance, double otherTolerance, List<ForecastData> forecastData, bool alertException, ILog log)
        {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            string dateFormat = "yyyy-MM-dd HH:mm:ss";
            var sysUnrated = new DailySysUnratedService(ConnectionStringSPAM);
            var reportDate = new List<DateTime>();

            if ((Convert.ToDateTime(endDate).Subtract(startDate)).Days >= 1) {
                DataSet dset = sysUnrated.GetDataDateFor(1, startDate, endDate, systemSerial);
                for (int i = 0; i < dset.Tables["Interval"].Rows.Count; i++) {
                    reportDate.Add(Convert.ToDateTime(dset.Tables["Interval"].Rows[i]["DataDate"].ToString()));
                }
            }
            else {
                reportDate.Add(Convert.ToDateTime(startDate));
            }

            //Get data from detail table.
            var databaseMapService = new DatabaseMappingService(ConnectionString);
            string newConnectionString = databaseMapService.GetConnectionStringFor(systemSerial);


            //Get TableNames.
            var databaseCheck = new Database(newConnectionString);
            var databaseName = Helper.FindKeyName(newConnectionString, "DATABASE");
            var cpuTrendTable = new CPUTrendTableRepository(newConnectionString);
            var dataTable = cpuTrendTable.GetCPUBusyInterval(startDate.ToString(dateFormat), endDate.ToString(dateFormat));

            var cpuLists = dataTable.AsEnumerable().Select(x => x.Field<UInt64>("CPUNumber")).Distinct().ToList();

            var totalMaxValue = 0D;
            dataTable = dataTable.AsEnumerable().OrderBy(x => x.Field<DateTime>("Date & Time")).CopyToDataTable();
            var dataIntervals = dataTable.AsEnumerable().Select(x => x.Field<DateTime>("Date & Time")).Distinct().ToList();

            var alertExceptionColor = Color.White;
            int yellowCount = 0;
            int redCount = 0;
            if (dataIntervals.Count > 0) {
                foreach (var dataInterval in dataIntervals) {
                    foreach (var cpuNum in cpuLists) {
                        var cpuBusy = dataTable.AsEnumerable().Where(x => x.Field<UInt64>("CPUNumber").Equals(cpuNum) && 
                        x.Field<DateTime>("Date & Time").Equals(dataInterval)).Select(x => x.Field<double>("Busy")).FirstOrDefault();
                        if (forecastData.Count > 0) {
                            var isForecastData = forecastData.Any(x => x.ForecastDateTime.Equals(dataInterval) && 
                            (Convert.ToInt16(x.CpuNumber) == Convert.ToInt16(cpuNum)));
                            if (alertException && systemweekInfo[(int)dataInterval.DayOfWeek].IsWeekday && isForecastData) {
                                //var toleranceValue = GetSystemWeekInfoHourData(dataInterval.Hour, systemweekInfo[(int)dataInterval.DayOfWeek], businessTolerance, batchTolerance, otherTolerance);
                                var thresholdTypeId = GetThresholdTypeId(dataInterval.Hour, systemweekInfo[(int)dataInterval.DayOfWeek]);
                                var systemWeekThresholds = new SystemWeekThresholdsRepository();
                                var threadholds = systemWeekThresholds.GetCpuBusy(systemSerial, thresholdTypeId);
                                
                                var forecastDataSub = forecastData.Where(x => x.ForecastDateTime.Equals(dataInterval) && (Convert.ToInt16(x.CpuNumber) == Convert.ToInt16(cpuNum))).Select(x => x.CpuBusy).FirstOrDefault();
                                var stdDev = forecastData.Where(x => x.ForecastDateTime.Equals(dataInterval) && (Convert.ToInt16(x.CpuNumber) == Convert.ToInt16(cpuNum))).Select(x => x.StdDevCpuBusy).FirstOrDefault();

                                var defaultValueMajor = GetThreasholdDefaultValue(thresholdTypeId, "CPUBusyMajor");
                                var exceptionMajor = defaultValueMajor;
                                if (threadholds.Rows.Count > 0)
                                    exceptionMajor = threadholds.Rows[0].IsNull("CPUBusyMajor") ? defaultValueMajor : Convert.ToDouble(threadholds.Rows[0]["CPUBusyMajor"]);
                                var upperRange = forecastDataSub + exceptionMajor + stdDev;
                                var lowerRange = Math.Abs(forecastDataSub - (exceptionMajor + stdDev));

                                if (cpuBusy > upperRange) {
                                    log.InfoFormat("dataInterval: {0}, cpuNum: {1}, upperRange {2}, lowerRange {3}, cpuBusy {4}, RED",
                                        dataInterval, cpuNum, upperRange, lowerRange, cpuBusy);
                                    redCount++;
                                    alertExceptionColor = Color.Red;
                                }
                                else {
                                    var defaultValueMinor = GetThreasholdDefaultValue(thresholdTypeId, "CPUBusyMinor");
                                    var exceptionMinor = defaultValueMinor;
                                    if (threadholds.Rows.Count > 0)
                                        exceptionMinor = threadholds.Rows[0].IsNull("CPUBusyMinor") ? defaultValueMinor : Convert.ToDouble(threadholds.Rows[0]["CPUBusyMinor"]);
                                    var upperRangeSub = forecastDataSub + exceptionMinor + stdDev;
                                    var lowerRangeSub = Math.Abs(forecastDataSub - (exceptionMinor - stdDev));

                                    if (cpuBusy > upperRangeSub) {
                                        log.InfoFormat("dataInterval: {0}, cpuNum: {1}, upperRange {2}, lowerRange {3}, cpuBusy {4}, Yellow",
                                                        dataInterval, cpuNum, upperRange, lowerRange, cpuBusy);
                                        yellowCount++;
                                        if (alertExceptionColor != Color.Red) {
                                            alertExceptionColor = Color.Yellow;
                                        }
                                    }
                                }
                            }
                            else {
                                if (cpuBusy > totalMaxValue)
                                    totalMaxValue = cpuBusy;
                            }
                        }
                        else {
                            if (cpuBusy > totalMaxValue)
                                totalMaxValue = cpuBusy;
                        }
                    }
                }
            }
            log.InfoFormat("yellowCount: {0}, redCount {1}", yellowCount, redCount);
            if (alertException && forecastData.Count > 0) {
                return alertExceptionColor;
            }
            var color = Color.White;
            if (totalMaxValue > 79 && totalMaxValue < 90)
                color = Color.Yellow;
            else if (totalMaxValue >= 90)
                color = Color.Red;
            return color;
        }

        public Color GetIPUBusyAlertColor(string systemSerial, DateTime startDate, DateTime endDate, long interval, Dictionary<int, SystemWeekInfo> systemweekInfo,
            double businessTolerance, double batchTolerance, double otherTolerance, List<ForecastData> forecastData, bool alertException, ILog log) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            string dateFormat = "yyyy-MM-dd HH:mm:ss";
            var sysUnrated = new DailySysUnratedService(ConnectionStringSPAM);
            var reportDate = new List<DateTime>();

            if ((Convert.ToDateTime(endDate).Subtract(startDate)).Days >= 1) {
                DataSet dset = sysUnrated.GetDataDateFor(1, startDate, endDate, systemSerial);
                for (int i = 0; i < dset.Tables["Interval"].Rows.Count; i++) {
                    reportDate.Add(Convert.ToDateTime(dset.Tables["Interval"].Rows[i]["DataDate"].ToString()));
                }
            }
            else {
                reportDate.Add(Convert.ToDateTime(startDate));
            }

            //Get data from detail table.
            var databaseMapService = new DatabaseMappingService(ConnectionString);
            string newConnectionString = databaseMapService.GetConnectionStringFor(systemSerial);


            //Get TableNames.
            var databaseCheck = new Database(newConnectionString);
            var databaseName = Helper.FindKeyName(newConnectionString, "DATABASE");
            var cpuTrendTable = new CPUTrendTableRepository(newConnectionString);
            var dataTable = cpuTrendTable.GetIPUBusyInterval(startDate.ToString(dateFormat), endDate.ToString(dateFormat));

            var totalMaxValue = 0D;
            dataTable = dataTable.AsEnumerable().OrderBy(x => x.Field<DateTime>("Date & Time")).CopyToDataTable();
            var dataIntervals = dataTable.AsEnumerable().Select(x => x.Field<DateTime>("Date & Time")).Distinct().ToList();

            var alertExceptionColor = Color.White;
            int yellowCount = 0;
            int redCount = 0;
            if (dataTable.Rows.Count > 0) {
                foreach (DataRow row in dataTable.Rows) {
                    var ipuBusy = Convert.ToDouble(row["Busy"]);
                    if (forecastData.Count > 0) {
                        var cpuNum = Convert.ToInt32(row["CPUNumber"]);
                        var ipuNum = Convert.ToInt32(row["IPUNumber"]);
                        DateTime dateInterval = Convert.ToDateTime(row["Date & Time"]);

                        var isForecastData = forecastData.Any(x => x.ForecastDateTime.Equals(dateInterval) &&
                                                                    (Convert.ToInt16(x.CpuNumber) == Convert.ToInt16(cpuNum)) &&
                                                                    (Convert.ToInt16(x.IpuNumber) == Convert.ToInt16(ipuNum)));

                        if (alertException && systemweekInfo[(int)dateInterval.DayOfWeek].IsWeekday && isForecastData) {
                            //var toleranceValue = GetSystemWeekInfoHourData(dateInterval.Hour, systemweekInfo[(int)dateInterval.DayOfWeek], businessTolerance, batchTolerance, otherTolerance);
                                    
                            var thresholdTypeId = GetThresholdTypeId(dateInterval.Hour, systemweekInfo[(int)dateInterval.DayOfWeek]);
                            var systemWeekThresholds = new SystemWeekThresholdsRepository();
                            var threadholds = systemWeekThresholds.GetIpuBusy(systemSerial, thresholdTypeId);

                            var forecastDataSub = forecastData.Where(x => x.ForecastDateTime.Equals(dateInterval) &&
                                                                            (Convert.ToInt16(x.CpuNumber) == Convert.ToInt16(cpuNum)) &&
                                                                            (Convert.ToInt16(x.IpuNumber) == Convert.ToInt16(ipuNum))).Select(x => x.IpuBusy).FirstOrDefault();
                            var stdDev = forecastData.Where(x => x.ForecastDateTime.Equals(dateInterval) &&
                                                                        (Convert.ToInt16(x.CpuNumber) == Convert.ToInt16(cpuNum)) &&
                                                                        (Convert.ToInt16(x.IpuNumber) == Convert.ToInt16(ipuNum))).Select(x => x.StdDevIpuBusy).FirstOrDefault();

                            var defaultValueMajor = GetThreasholdDefaultValue(thresholdTypeId, "IPUBusyMajor");
                            var exceptionMajor = defaultValueMajor;
                            if (threadholds.Rows.Count > 0)
                                exceptionMajor = threadholds.Rows[0].IsNull("IPUBusyMajor") ? defaultValueMajor : Convert.ToDouble(threadholds.Rows[0]["IPUBusyMajor"]);
                            var upperRange = forecastDataSub + exceptionMajor + stdDev;
                            var lowerRange = Math.Abs(forecastDataSub - (exceptionMajor + stdDev));

                            if (ipuBusy > upperRange) {
                                log.InfoFormat("dataInterval: {0}, cpuNum: {1}, upperRange {2}, lowerRange {3}, " +
                                                "ipuBusy {4}, ipuNum {5}, RED",
                                                dateInterval, cpuNum, upperRange, lowerRange, ipuBusy, ipuNum);
                                redCount++;
                                alertExceptionColor = Color.Red;
                            }
                            else {
                                var defaultValueMinor = GetThreasholdDefaultValue(thresholdTypeId, "IPUBusyMinor");
                                var exceptionMinor = defaultValueMinor;
                                if (threadholds.Rows.Count > 0)
                                    exceptionMinor = threadholds.Rows[0].IsNull("IPUBusyMinor") ? defaultValueMinor : Convert.ToDouble(threadholds.Rows[0]["IPUBusyMinor"]);
                                var upperRangeSub = forecastDataSub + exceptionMinor + stdDev;
                                var lowerRangeSub = Math.Abs(forecastDataSub - (exceptionMinor - stdDev));

                                if (ipuBusy > upperRangeSub) {
                                    log.InfoFormat("dataInterval: {0}, cpuNum: {1}, upperRange {2}, lowerRange {3}, " +
                                                    "ipuBusy {4}, ipuNum {5}, Yellow",
                                                    dateInterval, cpuNum, upperRange, lowerRange, ipuBusy, ipuNum);
                                    yellowCount++;
                                    if (alertExceptionColor != Color.Red) {
                                        alertExceptionColor = Color.Yellow;
                                    }
                                }
                            }
                        }
                        else {
                            if (ipuBusy > totalMaxValue)
                                totalMaxValue = ipuBusy;
                        }
                    }
                    else {
                        if (ipuBusy > totalMaxValue)
                            totalMaxValue = ipuBusy;
                    }
                }
            }
            log.InfoFormat("yellowCount: {0}, redCount {1}", yellowCount, redCount);

            if (alertException && forecastData.Count > 0) return alertExceptionColor;

            var color = Color.White;
            if (totalMaxValue > 79 && totalMaxValue < 90)
                color = Color.Yellow;
            else if (totalMaxValue >= 90)
                color = Color.Red;

            return color;
        }

        public Color GetCPUQueueAlertColor(string systemSerial, DateTime startDate, DateTime endDate, long interval, Dictionary<int, SystemWeekInfo> systemweekInfo,
            double businessTolerance, double batchTolerance, double otherTolerance, List<ForecastData> forecastData, bool alertException, ILog log)
        {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            string dateFormat = "yyyy-MM-dd HH:mm:ss";
            var sysUnrated = new DailySysUnratedService(ConnectionStringSPAM);
            var reportDate = new List<DateTime>();

            if ((Convert.ToDateTime(endDate).Subtract(startDate)).Days >= 1) {
                DataSet dset = sysUnrated.GetDataDateFor(1, startDate, endDate, systemSerial);
                for (int i = 0; i < dset.Tables["Interval"].Rows.Count; i++) {
                    reportDate.Add(Convert.ToDateTime(dset.Tables["Interval"].Rows[i]["DataDate"].ToString()));
                }
            }
            else {
                reportDate.Add(Convert.ToDateTime(startDate));
            }

            //Get data from detail table.
            var databaseMapService = new DatabaseMappingService(ConnectionString);
            string newConnectionString = databaseMapService.GetConnectionStringFor(systemSerial);


            //Get TableNames.
            var databaseCheck = new Database(newConnectionString);
            var databaseName = Helper.FindKeyName(newConnectionString, "DATABASE");
            var cpuTrendTable = new CPUTrendTableRepository(newConnectionString);
            var dataTable = cpuTrendTable.GetCPUQueueInterval(startDate.ToString(dateFormat), endDate.ToString(dateFormat));
            var cpuLists = dataTable.AsEnumerable().Select(x => x.Field<UInt64>("CPUNumber")).Distinct().ToList();

            dataTable = dataTable.AsEnumerable().OrderBy(x => x.Field<DateTime>("Date & Time")).CopyToDataTable();
            var dataIntervals = dataTable.AsEnumerable().Select(x => x.Field<DateTime>("Date & Time")).Distinct().ToList();

            var totalMaxValue = 0D;
            var alertExceptionColor = Color.White;
            int yellowCount = 0;
            int redCount = 0;
            if (dataIntervals.Count > 0) {
                foreach (var dataInterval in dataIntervals) {
                    foreach (var cpuNum in cpuLists) {
                        var cpuQueue = dataTable.AsEnumerable().Where(x => x.Field<UInt64>("CPUNumber").Equals(cpuNum) && x.Field<DateTime>("Date & Time").Equals(dataInterval)).Select(x => x.Field<double>("Queue")).FirstOrDefault();
                        if (forecastData.Count > 0) {
                            var isForecastData = forecastData.Any(x => x.ForecastDateTime.Equals(dataInterval) && (Convert.ToInt16(x.CpuNumber) == Convert.ToInt16(cpuNum)));
                            if (alertException && systemweekInfo[(int)dataInterval.DayOfWeek].IsWeekday && isForecastData) {
                                //var toleranceValue = GetSystemWeekInfoHourData(dataInterval.Hour, systemweekInfo[(int)dataInterval.DayOfWeek], businessTolerance, batchTolerance, otherTolerance);
                                var thresholdTypeId = GetThresholdTypeId(dataInterval.Hour, systemweekInfo[(int)dataInterval.DayOfWeek]);
                                var systemWeekThresholds = new SystemWeekThresholdsRepository();
                                var threadholds = systemWeekThresholds.GetCpuQueueLength(systemSerial, thresholdTypeId);

                                var forecastDataSub = forecastData.Where(x => x.ForecastDateTime.Equals(dataInterval) && (Convert.ToInt16(x.CpuNumber) == Convert.ToInt16(cpuNum))).Select(x => x.Queue).FirstOrDefault();
                                var stdDev = forecastData.Where(x => x.ForecastDateTime.Equals(dataInterval) && (Convert.ToInt16(x.CpuNumber) == Convert.ToInt16(cpuNum))).Select(x => x.StdDevQueue).FirstOrDefault();

                                var defaultValueMajor = GetThreasholdDefaultValue(thresholdTypeId, "CPUQueueLengthMajor");
                                var exceptionMajor = defaultValueMajor;
                                if (threadholds.Rows.Count > 0)
                                    exceptionMajor = threadholds.Rows[0].IsNull("CPUQueueLengthMajor") ? defaultValueMajor : Convert.ToDouble(threadholds.Rows[0]["CPUQueueLengthMajor"]);
                                var upperRange = forecastDataSub + exceptionMajor + stdDev;
                                var lowerRange = Math.Abs(forecastDataSub - (exceptionMajor + stdDev));

                                if (cpuQueue > upperRange) {
                                    log.InfoFormat("dataInterval: {0}, cpuNum: {1}, upperRange {2}, lowerRange {3}, " +
                                                    "cpuQueue {4}, RED",
                                                    dataInterval, cpuNum, upperRange, lowerRange, cpuQueue);
                                    redCount++;
                                    alertExceptionColor = Color.Red;
                                }
                                else {
                                    var defaultValueMinor = GetThreasholdDefaultValue(thresholdTypeId, "CPUQueueLengthMinor");
                                    var exceptionMinor = defaultValueMinor;
                                    if (threadholds.Rows.Count > 0)
                                        exceptionMinor = threadholds.Rows[0].IsNull("CPUQueueLengthMinor") ? defaultValueMinor : Convert.ToDouble(threadholds.Rows[0]["CPUQueueLengthMinor"]);
                                    var upperRangeSub = forecastDataSub + exceptionMinor + stdDev;
                                    var lowerRangeSub = Math.Abs(forecastDataSub - (exceptionMinor - stdDev));

                                    if (cpuQueue > upperRangeSub) {
                                        log.InfoFormat("dataInterval: {0}, cpuNum: {1}, upperRange {2}, lowerRange {3}, " +
                                                    "cpuQueue {4}, Yellow",
                                                    dataInterval, cpuNum, upperRange, lowerRange, cpuQueue);
                                        yellowCount++;
                                        if (alertExceptionColor != Color.Red) {
                                            alertExceptionColor = Color.Yellow;
                                        }
                                    }
                                }
                            }
                            else {
                                if (cpuQueue > totalMaxValue)
                                    totalMaxValue = cpuQueue;
                            }
                        }
                        else {
                            if (cpuQueue > totalMaxValue)
                                totalMaxValue = cpuQueue;
                        }
                    }
                }
            }
            log.InfoFormat("yellowCount: {0}, redCount {1}", yellowCount, redCount);

            if (alertException && forecastData.Count > 0) return alertExceptionColor;

            var color = Color.White;
            if (totalMaxValue >= 5 && totalMaxValue < 10)
                color = Color.Yellow;
            else if (totalMaxValue >= 10)
                color = Color.Red;

            return color;
        }
        public Color GetIpuQueueAlertColor(string systemSerial, DateTime startDate, DateTime endDate, long interval, Dictionary<int, SystemWeekInfo> systemweekInfo,
            double businessTolerance, double batchTolerance, double otherTolerance, List<ForecastData> forecastData, bool alertException, ILog log) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            string dateFormat = "yyyy-MM-dd HH:mm:ss";
            var sysUnrated = new DailySysUnratedService(ConnectionStringSPAM);
            var reportDate = new List<DateTime>();

            if ((Convert.ToDateTime(endDate).Subtract(startDate)).Days >= 1) {
                DataSet dset = sysUnrated.GetDataDateFor(1, startDate, endDate, systemSerial);
                for (int i = 0; i < dset.Tables["Interval"].Rows.Count; i++) {
                    reportDate.Add(Convert.ToDateTime(dset.Tables["Interval"].Rows[i]["DataDate"].ToString()));
                }
            }
            else {
                reportDate.Add(Convert.ToDateTime(startDate));
            }

            //Get data from detail table.
            var databaseMapService = new DatabaseMappingService(ConnectionString);
            string newConnectionString = databaseMapService.GetConnectionStringFor(systemSerial);

            //Get TableNames.
            var databaseCheck = new Database(newConnectionString);
            var databaseName = Helper.FindKeyName(newConnectionString, "DATABASE");
            var cpuTrendTable = new CPUTrendTableRepository(newConnectionString);
            var dataTable = cpuTrendTable.GetIPUQueueInterval(startDate.ToString(dateFormat), endDate.ToString(dateFormat));

            var totalMaxValue = 0D;
            var alertExceptionColor = Color.White;
            int yellowCount = 0;
            int redCount = 0;
            if (dataTable.Rows.Count > 0) {
                foreach (DataRow row in dataTable.Rows) {
                    var ipuQueue = Convert.ToDouble(row["Queue"]);
                    if (forecastData.Count > 0) {
                        var cpuNum = Convert.ToInt32(row["CPUNumber"]);
                        var ipuNum = Convert.ToInt32(row["IPUNumber"]);
                        DateTime dateInterval = Convert.ToDateTime(row["Date & Time"]);
                        var isForecastData = forecastData.Any(x => x.ForecastDateTime.Equals(dateInterval) && (Convert.ToInt16(x.CpuNumber) == Convert.ToInt16(cpuNum)) && (Convert.ToInt16(x.IpuNumber) == Convert.ToInt16(ipuNum)));
                        if (alertException && systemweekInfo[(int)dateInterval.DayOfWeek].IsWeekday && isForecastData) {
                            var thresholdTypeId = GetThresholdTypeId(dateInterval.Hour, systemweekInfo[(int)dateInterval.DayOfWeek]);
                            var systemWeekThresholds = new SystemWeekThresholdsRepository();
                            var threadholds = systemWeekThresholds.GetIpuQueueLength(systemSerial, thresholdTypeId);

                            var forecastDataSub = forecastData.Where(x => x.ForecastDateTime.Equals(dateInterval) &&
                                                                            (Convert.ToInt16(x.CpuNumber) == Convert.ToInt16(cpuNum)) && 
                                                                            (Convert.ToInt16(x.IpuNumber) == Convert.ToInt16(ipuNum))).Select(x => x.IpuQueue).FirstOrDefault();
                            var stdDev = forecastData.Where(x => x.ForecastDateTime.Equals(dateInterval) &&
                                                                            (Convert.ToInt16(x.CpuNumber) == Convert.ToInt16(cpuNum)) && 
                                                                            (Convert.ToInt16(x.IpuNumber) == Convert.ToInt16(ipuNum))).Select(x => x.StdDevIpuQueue).FirstOrDefault();
                            var defaultValueMajor = GetThreasholdDefaultValue(thresholdTypeId, "IPUQueueLengthMajor");
                            var exceptionMajor = defaultValueMajor;
                            if (threadholds.Rows.Count > 0)
                                exceptionMajor = threadholds.Rows[0].IsNull("IPUQueueLengthMajor") ? defaultValueMajor : Convert.ToDouble(threadholds.Rows[0]["IPUQueueLengthMajor"]);
                            var upperRange = forecastDataSub + exceptionMajor + stdDev;
                            var lowerRange = Math.Abs(forecastDataSub - (exceptionMajor + stdDev));

                            if (ipuQueue > upperRange) {
                                log.InfoFormat("dataInterval: {0}, cpuNum: {1}, upperRange {2}, lowerRange {3}, " +
                                                "ipuQueue {4}, ipuNum {5}, RED",
                                                dateInterval, cpuNum, upperRange, lowerRange, ipuQueue, ipuNum);
                                redCount++;
                                alertExceptionColor = Color.Red;

                            }
                            else {
                                //Check half of toleranceValue.
                                var defaultValueMinor = GetThreasholdDefaultValue(thresholdTypeId, "IPUQueueLengthMinor");
                                var exceptionMinor = defaultValueMinor;
                                if (threadholds.Rows.Count > 0)
                                    exceptionMinor = threadholds.Rows[0].IsNull("IPUQueueLengthMinor") ? defaultValueMinor : Convert.ToDouble(threadholds.Rows[0]["IPUQueueLengthMinor"]);
                                var upperRangeSub = forecastDataSub + exceptionMinor + stdDev;
                                var lowerRangeSub = Math.Abs(forecastDataSub - (exceptionMinor + stdDev));

                                if (ipuQueue > upperRangeSub) {
                                    log.InfoFormat("dataInterval: {0}, cpuNum: {1}, upperRange {2}, lowerRange {3}, " +
                                                "ipuQueue {4}, ipuNum {5}, Yellow",
                                                dateInterval, cpuNum, upperRange, lowerRange, ipuQueue, ipuNum);
                                    yellowCount++;
                                    if (alertExceptionColor != Color.Red) {
                                        alertExceptionColor = Color.Yellow;
                                    }
                                }
                            }
                        }
                        else {
                            if (ipuQueue > totalMaxValue)
                                totalMaxValue = ipuQueue;
                        }
                    }
                    else {
                        if (ipuQueue > totalMaxValue)
                            totalMaxValue = ipuQueue;
                    }

                }
            }
            log.InfoFormat("yellowCount: {0}, redCount {1}", yellowCount, redCount);

            if (alertException && forecastData.Count > 0) return alertExceptionColor;
            var color = Color.White;
            if (totalMaxValue >= 5 && totalMaxValue < 10)
                color = Color.Yellow;
            else if (totalMaxValue >= 10)
                color = Color.Red;

            return color;
        }

        private void ExpectionBulkInsert(string databaseName, List<ExceptionView> exceptionList, string tempSaveLocation) {
            var databaseCheck = new Database(ConnectionStringSPAM);
            var tableName = "Exceptions";
            var exists = databaseCheck.CheckTableExists(tableName, databaseName);

            if (!exists) {
                databaseCheck.CreateExceptionsTable();
            }

            var pathToCsv = tempSaveLocation + "\\BulkInsertExceptions_" + DateTime.Now.Ticks + ".csv";
            var sb = new StringBuilder();

            foreach (var exception in exceptionList) {
                var displayRed = exception.DisplayRed == true ? "1" : "0";

                sb.Append(exception.FromTimestamp.ToString("yyyy-MM-dd HH:mm:ss") + "|" +
                          exception.EntityId + "|" +
                          exception.CounterId + "|" +
                          exception.Instance + "|" +
                          exception.Actual + "|" +
                          exception.Upper + "|" +
                          exception.Lower + "|" +
                          displayRed + Environment.NewLine);
            }
            File.AppendAllText(pathToCsv, sb.ToString());

            var dataTables = new DataTables(ConnectionStringSPAM);
            dataTables.InsertForecastData(tableName, pathToCsv);
        }

        public void ExpectionUniqueBulkInsert(List<ExceptionView> exceptionList, string tempSaveLocation) {
            var databaseName = Helper.FindKeyName(ConnectionStringSPAM, "DATABASE");
            var databaseCheck = new Database(ConnectionStringSPAM);

            var tableName = "Exceptions";
            var exists = databaseCheck.CheckTableExists(tableName, databaseName);

            if (!exists) {
                databaseCheck.CreateExceptionsTable();
            }

            var pathToCsv = tempSaveLocation + "\\BulkInsertExceptions_" + DateTime.Now.Ticks + ".csv";
            var sb = new StringBuilder();

            var exceptionDates = exceptionList.Select(x => x.FromTimestamp).Distinct().ToList();
            var cpuList = exceptionList.Select(x => x.Instance).Distinct().ToList();
            var newList = new List<ExceptionView>();

            foreach (var exceptionDate in exceptionDates) {
                foreach (var cpu in cpuList) {
                    var list = exceptionList.Where(x => x.FromTimestamp.Equals(exceptionDate) && x.Instance.Equals(cpu));

                    if (list.Any(x => x.IsException.Equals(true))) {
                        newList.AddRange(list);
                    }
                }
            }

            foreach (var exception in newList) {
                var displayRed = exception.DisplayRed == true ? "1" : "0";
                if (!exception.IsException)
                    displayRed = "2";

                sb.Append(exception.FromTimestamp.ToString("yyyy-MM-dd HH:mm:ss") + "|" +
                          exception.EntityId + "|" +
                          exception.CounterId + "|" +
                          exception.Instance + "|" +
                          exception.Actual + "|" +
                          exception.Upper + "|" +
                          exception.Lower + "|" +
                          displayRed + Environment.NewLine);
            }
            File.AppendAllText(pathToCsv, sb.ToString());

            var dataTables = new DataTables(ConnectionStringSPAM);
            dataTables.InsertForecastData(tableName, pathToCsv);
        }
    }
}