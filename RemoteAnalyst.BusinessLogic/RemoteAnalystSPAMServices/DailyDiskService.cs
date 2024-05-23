using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices {
    public class DailyDiskService {
        private readonly string _connectionString;

        public DailyDiskService(string connectionStringTrend) {
            _connectionString = connectionStringTrend;
        }

        public IList<DailyDiskView> GetDiskNamesFor() {
            var dailyDisk = new DailyDisk(_connectionString);
            DataTable disks = dailyDisk.GetDiskNames();
            IList<DailyDiskView> allDisks = new List<DailyDiskView>();

            foreach (DataRow dr in disks.Rows) {
                var view = new DailyDiskView();
                view.SystemSerial = dr["DD_SystemSerialNum"].ToString();
                view.DiskName = dr["DD_DiskName"].ToString();

                allDisks.Add(view);
            }

            return allDisks;
        }

        public bool CheckDataFor(string systemSerial, string diskName, int month, int year) {
            var dailyDisk = new DailyDisk(_connectionString);
            bool exists = dailyDisk.CheckData(systemSerial, diskName, month, year);

            return exists;
        }

        public double GetUsedGBFor(string systemSerial, string diskName, int month, int year, int order) {
            var dailyDisk = new DailyDisk(_connectionString);
            string sort = "";

            if (order == 1) {
                sort = "ASC";
            }
            else {
                sort = "DESC";
            }
            double usedGB = dailyDisk.GetUsedGB(systemSerial, diskName, month, year, sort);

            return usedGB;
        }

        public double GetAveragedUsedGBFor(string systemSerial, string diskName, int month, int year) {
            var dailyDisk = new DailyDisk(_connectionString);
            double avgUsedGB = dailyDisk.GetAveragedUsedGB(systemSerial, diskName, month, year);

            return avgUsedGB;
        }

        public void DeleteDataFor(DateTime oldDate) {
            var dailyDisk = new DailyDisk(_connectionString);
            dailyDisk.DeleteData(oldDate);
        }

        public string GetDailyDiskInfoFor(string systemSerial, DateTime startDate, string[] ignoreVolumes, DataTable scheduleStorageThresholds) {
            var dailyDisk = new DailyDisk(_connectionString);
            var diskInfo = dailyDisk.GetDailyDiskInfo(startDate.ToString("yyyy-MM-dd"));

            var yesterdayDate = startDate.AddDays(-1);
            var lastWeekDate = startDate.AddDays(-7);
            var lastMonthDate = Helper.GetLastMonthDate(startDate);

            var process = new StringBuilder();

            if (diskInfo.Rows.Count > 0) {
                process.Append("<div><table class=main CellPadding=2 CellSpacing=0 Border=1 style='FONT-SIZE: 7pt; FONT-FAMILY: Calibri; '> ");
                process.Append("<tr style=\"background-color:LightGrey;border-color:Black;border-width:1px;border-style:solid;\">\n");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Volume</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Free (GB)</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Used (GB)</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">% Used</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Delta (GB) 1 day</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Delta (GB) 1 week</th>");
                process.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Delta (GB) 1 month</th>");
                process.Append("</tr>\n");

                var rowIndex = 0;
                var tempDiskNames = diskInfo.AsEnumerable().Select(x => x.Field<string>("DiskName")).ToList();
                var stringDiskNames = new List<string>();
                foreach (var tempDiskName in tempDiskNames) {
                    stringDiskNames.Add("'" + tempDiskName + "'");
                }
                var diskNames = string.Join(",", stringDiskNames);
                var previousUsedGb = dailyDisk.GetUsedGB(systemSerial, yesterdayDate.ToString("yyyy-MM-dd"), lastWeekDate.ToString("yyyy-MM-dd"), lastMonthDate.ToString("yyyy-MM-dd"), diskNames);
                //var lastWeekUsedGB = dailyDisk.GetUsedGB(lastWeekDate.ToString("yyyy-MM-dd"), diskName);
                //var lastMonthUsedGB = dailyDisk.GetUsedGB(lastMonthDate.ToString("yyyy-MM-dd"), diskName);

                foreach (DataRow row in diskInfo.Rows) {
                    var diskName = row["DiskName"].ToString();
                    var ignoreThisDisk = false;
                    // Define a regular expression for repeated words.
                    foreach (var ignoreVolumeItem in ignoreVolumes) {
                        var ignoreVolume = ignoreVolumeItem.Trim();
                        if (ignoreVolume.Length > 0) { 
                            if(ignoreVolume.StartsWith("$")) {
                                // To escape $ since in regex it has a special meaning
                                ignoreVolume = "\\" + ignoreVolume; 
                            }
                            // * in regex is .*
                            ignoreVolume = ignoreVolume.Replace("*", ".*");
                            Regex rx = new Regex(ignoreVolume,
                              RegexOptions.Compiled | RegexOptions.IgnoreCase);
                            // Find matches.
                            MatchCollection matches = rx.Matches(diskName);
                            if (matches.Count > 0) {
                                ignoreThisDisk = true;
                                break;
                            }
                        }
                    }
                    if (ignoreThisDisk) {
                        continue;
                    }
                    var usedPercent = Convert.ToDouble(row["UsedPercent"]);
                    foreach (DataRow r in scheduleStorageThresholds.Rows)
                    {
                        string vol = Convert.ToString(r["Volume"]);
                        double thresh = Convert.ToDouble(r["Threshold"]);
                        if (vol == diskName && thresh <= usedPercent) {
                            ignoreThisDisk = true;                        
                        }
                        else if (vol.Contains('*'))
                        {
                            String[] split = vol.Split('*');
                            if (diskName.StartsWith(split[0]) && thresh <= usedPercent)
                            {
                                ignoreThisDisk=true;
                            }
                        }
                    }
                    if (ignoreThisDisk)
                    {
                        continue;
                    }

                    var yesterdayUsedGb = previousUsedGb.AsEnumerable().Where(x => x.Field<string>("DiskName") == diskName).Select(x => x.Field<double>("Yesterday")).FirstOrDefault();
                    var lastWeekUsedGb = previousUsedGb.AsEnumerable().Where(x => x.Field<string>("DiskName") == diskName).Select(x => x.Field<double>("LastWeek")).FirstOrDefault();
                    var lastMonthUsedGb = previousUsedGb.AsEnumerable().Where(x => x.Field<string>("DiskName") == diskName).Select(x => x.Field<double>("LastMonth")).FirstOrDefault();

                    if (rowIndex % 2 == 0)
                        process.Append("<tr>\n");
                    else
                        process.Append("<tr style='background-color: #E6E6E6;'>\n");

                    process.Append("<td align=\"left\">" + diskName + "</td>\n");
                    process.Append("<td align=\"right\">" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["Capacity"]) - Convert.ToDouble(row["Used"]), 2)) + "</td>\n");
                    process.Append("<td align=\"right\">" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["Used"]), 2)) + "</td>\n");
                    if (usedPercent > 79 && usedPercent < 90)
                        process.Append("<td align=\"right\"  style=\"white-space:nowrap;background-color: yellow;\">" + string.Format("{0:#,##0.00}", Math.Round(usedPercent, 2)) + "</td>\n");
                    else if (usedPercent >= 90)
                        process.Append("<td align=\"right\"  style=\"white-space:nowrap;background-color: red;\">" + string.Format("{0:#,##0.00}", Math.Round(usedPercent, 2)) + "</td>\n");
                    else
                        process.Append("<td align=\"right\">" + string.Format("{0:#,##0.00}", Math.Round(usedPercent, 2)) + "</td>\n");
                    process.Append("<td align=\"right\">" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["Used"]) - yesterdayUsedGb, 2)) + "</td>\n");
                    process.Append("<td align=\"right\">" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["Used"]) - lastWeekUsedGb, 2)) + "</td>\n");
                    process.Append("<td align=\"right\">" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["Used"]) - lastMonthUsedGb, 2)) + "</td>\n");

                    process.Append("</tr>\n");
                    rowIndex++;
                }
                process.Append("</table></div><br>");
                if(rowIndex == 0) {
                    process = new StringBuilder();
                }
            }
            return process.ToString();
        }

        public Dictionary<DateTime, DailyDiskInfo> GetDiskGraphData(DateTime currentDate) {
            var dailyDisk = new DailyDisk(_connectionString);
            var diskInfo = dailyDisk.GetFreeUsedGB(currentDate.ToString("yyyy-MM-dd"), currentDate.AddDays(-10).ToString("yyyy-MM-dd"));
            var diskInfos = new Dictionary<DateTime, DailyDiskInfo>();

            foreach (DataRow dataRow in diskInfo.Rows) {
                diskInfos.Add(Convert.ToDateTime(dataRow["StorageDate"]), new DailyDiskInfo {
                    FreeGB = Convert.ToDouble(dataRow["Free"]) - Convert.ToDouble(diskInfo.Rows[0]["Used"]),
                    UsedGB = Convert.ToDouble(dataRow["Used"]),
                    UsedPercent = Math.Round((Convert.ToDouble(dataRow["Used"]) / Convert.ToDouble(dataRow["Free"])) * 100, 2),
                    FreePercent = 100 - Math.Round((Convert.ToDouble(dataRow["Used"]) / Convert.ToDouble(dataRow["Free"])) * 100, 2)
                });
            }
            /*if (diskInfo.Rows.Count > 0) {
                diskInfos.Add("Today", new DailyDiskInfo {
                    FreeGB = Convert.ToDouble(diskInfo.Rows[0]["Free"]) - Convert.ToDouble(diskInfo.Rows[0]["Used"]),
                    UsedGB = Convert.ToDouble(diskInfo.Rows[0]["Used"])
                });
            }

            var yesterdayDate = currentDate.AddDays(-1);
            diskInfo = dailyDisk.GetFreeUsedGB(yesterdayDate.ToString("yyyy-MM-dd"));
            if (diskInfo.Rows.Count > 0) {
                diskInfos.Add("OneDayAgo", new DailyDiskInfo {
                    FreeGB = Convert.ToDouble(diskInfo.Rows[0]["Free"]) - Convert.ToDouble(diskInfo.Rows[0]["Used"]),
                    UsedGB = Convert.ToDouble(diskInfo.Rows[0]["Used"])
                });
            }

            var lastWeekDate = currentDate.AddDays(-7);
            diskInfo = dailyDisk.GetFreeUsedGB(lastWeekDate.ToString("yyyy-MM-dd"));
            if (diskInfo.Rows.Count > 0) {
                diskInfos.Add("LastWeek", new DailyDiskInfo {
                    FreeGB = Convert.ToDouble(diskInfo.Rows[0]["Free"]) - Convert.ToDouble(diskInfo.Rows[0]["Used"]),
                    UsedGB = Convert.ToDouble(diskInfo.Rows[0]["Used"])
                });
            }

            var lastMonthDate = Helper.GetLastMonthDate(currentDate);
            diskInfo = dailyDisk.GetFreeUsedGB(lastMonthDate.ToString("yyyy-MM-dd"));
            if (diskInfo.Rows.Count > 0) {
                diskInfos.Add("LastMonth", new DailyDiskInfo {
                    FreeGB = Convert.ToDouble(diskInfo.Rows[0]["Free"]) - Convert.ToDouble(diskInfo.Rows[0]["Used"]),
                    UsedGB = Convert.ToDouble(diskInfo.Rows[0]["Used"])
                });
            }*/

            return diskInfos;
        }
    }
}