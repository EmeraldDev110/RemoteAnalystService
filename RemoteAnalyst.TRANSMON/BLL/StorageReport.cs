using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Timers;
using System.Web.UI.DataVisualization.Charting;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using RemoteAnalyst.BusinessLogic.Util;
using log4net;

namespace RemoteAnalyst.TransMon.BLL
{
    class StorageReport
    {
        private static readonly ILog Log = LogManager.GetLogger("TransMonLog");
        public void TimerRunStorageRep_Elapsed(object source, ElapsedEventArgs e)
        {
            if (DateTime.Now.Hour.Equals(4))
            {
                Log.Info("******** Scheduler is populating Storage Report data *********");
                SendStorageEmail(DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"));
            }
            else
            {
                Log.Info("Current time, not yet 4 AM, scheduler won't populate Storage Report.");
            }
        }

        public void SendStorageEmail(string date)
        {
            var email = new StorageEmail();
            var data = GetTop10StorageData(date);
#if !DEBUG
            var chartDir = CreateWeeklyTrendChart(DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"), ConnectionString.ServerPath);
#else
			var chartDir = CreateWeeklyTrendChart(DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"), @"C:\Temp");
#endif
            email.SendDailySummary(data, date, chartDir);
            if (File.Exists(chartDir))
                File.Delete(chartDir);
        }

        private List<StorageAnalysisView> GetTop10StorageData(string date)
        {
            var list = new List<StorageAnalysisView>();
            var storageRepo = new StorageAnalysis(ConnectionString.ConnectionStringDB);
            var storageTable = storageRepo.GetTop10StorageUsageBy(date);

            foreach (DataRow row in storageTable.Rows)
            {
                var storageView = new StorageAnalysisView
                {
                    SystemSerial = row["SystemSerial"].ToString(),
                    SystemName = row["SystemName"].ToString(),
                    CompanyName = row["CompanyName"].ToString(),
                    ActiveSizeInMB = int.Parse(row["ActiveSizeInMB"].ToString()),
                    TrendSizeInMB = int.Parse(row["TrendSizeInMB"].ToString()),
                    S3SizeInMB = int.Parse(row["S3SizeInMB"].ToString()),
                    GeneratedDate = ((DateTime)row["GeneratedDate"]).ToString("yyyy-MM-dd")
                };

                list.Add(storageView);
            }

            return list;
        }

        private List<GraphStorageView> GetStorageViewsBy(string systemName, string endDate, int period)
        {
            var list = new List<GraphStorageView>();
            var storageRepo = new StorageAnalysis(ConnectionString.ConnectionStringDB);
            var storageTable = storageRepo.GetStoragesBy(systemName, DateTime.Parse(endDate).AddDays(-6).ToString("yyyy-MM-dd"), endDate);

            foreach (DataRow row in storageTable.Rows)
            {
                var storageView = new GraphStorageView
                {
                    SystemName = row["SystemName"].ToString(),
                    StorageUsageInMB = int.Parse(row["StorageUsageInMB"].ToString()),
                    GeneratedDate = ((DateTime)row["GeneratedDate"]).ToString("yyyy-MM-dd")
                };

                list.Add(storageView);
            }

            return list;
        }

        public string CreateWeeklyTrendChart(string endDate, string path)
        {
            // Query companies ever appeared in the top 10 storage count for the past 7 days;
            // then do query for the past 7 days for each company.
            // i.e. each list item will be a list of the same company for the past 7 days.
            var storageRepo = new StorageAnalysis(ConnectionString.ConnectionStringDB);
            var top10SystemNames = storageRepo.GetSystemNamesInTopForPeriod(10, DateTime.Parse(endDate).AddDays(-6).ToString("yyyy-MM-dd"), endDate);

            // Load top 10 storage data from the past 7 days
            var top10SystemList = new List<List<GraphStorageView>>();
            foreach (DataRow system in top10SystemNames.Rows)
            {
                var storageViewsInPast7Days = GetStorageViewsBy(system["SystemName"].ToString(), endDate, 7);

                top10SystemList.Add(storageViewsInPast7Days);
            }

            // Chart configs
            var chart = new Chart();
            chart.Width = 700;
            chart.Height = 400;
            chart.Palette = ChartColorPalette.EarthTones;

            // Chart area configs
            var chartarea = new ChartArea();
            chartarea.BorderWidth = 0;

            // Y axis configs
            chartarea.AxisY.IsLabelAutoFit = false;
            chartarea.AxisY.LabelStyle.Font = new Font("Calibri", 10);
            chartarea.AxisY.MajorGrid.Enabled = true;
            chartarea.AxisY.MajorGrid.LineColor = Color.Silver;
            chartarea.AxisY.Title = "Total Storage (GB)";
            chartarea.AxisY.TitleFont = new Font("Calibri", 12);

            // X axis configs
            chartarea.AxisX.IsLabelAutoFit = false;
            chartarea.AxisX.IntervalType = DateTimeIntervalType.Days;
            chartarea.AxisX.LabelStyle.Angle = -60;
            chartarea.AxisX.LabelStyle.Font = new Font("Calibri", 10);
            chartarea.AxisX.LabelStyle.Format = "MM/dd/yyyy";
            chartarea.AxisX.MajorGrid.Enabled = false;
            chartarea.AxisX.MajorGrid.LineColor = Color.Silver;
            chartarea.AxisX.Minimum = DateTime.Parse(endDate).AddDays(-6).ToOADate();
            chartarea.AxisX.Maximum = DateTime.Parse(endDate).ToOADate();

            // Legend configs
            chart.Legends.Clear();
            chart.Legends.Add("Default");
            chart.Legends["Default"].IsDockedInsideChartArea = true;
            chart.Legends["Default"].Alignment = StringAlignment.Center;
            chart.Legends["Default"].Font = new Font("Calibri", 10);
            chart.Legends["Default"].BorderDashStyle = ChartDashStyle.NotSet;
            chart.Legends["Default"].Docking = Docking.Bottom;

            // Draw graph
            foreach (var systemList in top10SystemList)
            {
                var systemSeries = new Series(systemList[0].SystemName);
                systemSeries.ChartType = SeriesChartType.Spline;

                systemSeries.XValueType = ChartValueType.DateTime;
                systemSeries.XAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");
                systemSeries.YAxisType = (AxisType)Enum.Parse(typeof(AxisType), "Primary");
                foreach (var system in systemList)
                {
                    var dpAdjust = new DataPoint();
                    dpAdjust.XValue = DateTime.Parse(system.GeneratedDate).ToOADate();
                    dpAdjust.YValues[0] = Math.Round((float)system.StorageUsageInMB / 1024, 2);
                    systemSeries.Points.Add(dpAdjust);
                }

                chart.Series.Add(systemSeries);
            }

            chart.ChartAreas.Add(chartarea);
            chart.ImageType = ChartImageType.Png;

            // Save chart image
            string saveLocation = $@"{path}\ChartPic_StorageWeeklyTrend_{DateTime.Now.Ticks}.png";
            chart.SaveImage(saveLocation);
            return saveLocation;
        }
    }
}
