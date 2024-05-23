using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices {
    public class DiskBrowserService {
        private readonly string _connectionString;

        public DiskBrowserService(string connectionString) {
            _connectionString = connectionString;
        }

        public string GetTop20DisksFor(string systemSerial, DateTime startTime, DateTime stopTime, bool isLocalAnalyst, string monthDayPattern, string websiteAddress) {
            var databaseCheck = new Database(_connectionString);
            var databaseName = Helper.FindKeyName(_connectionString, "DATABASE");

            var diskBrowserables = new List<string>();
            for (var start = startTime.Date; start <= stopTime.Date; start = start.AddDays(1)) {
                var cpuTableName = systemSerial + "_DISKBROWSER_" + start.Year + "_" + start.Month + "_" + start.Day;
                var exists = databaseCheck.CheckTableExists(cpuTableName, databaseName);

                if (exists)
                    diskBrowserables.Add(cpuTableName);
            }

            var diskBusy = new StringBuilder();

            if (diskBrowserables.Count > 0) {
                var diskBrowser = new DiskBrowserRepository(_connectionString);
                var diskBusyData = diskBrowser.GetTop20Disks(diskBrowserables, startTime, stopTime);
                
                if (diskBusyData.Rows.Count > 0) {
                    diskBusy.Append("<div><table class=main CellPadding=2 CellSpacing=0 Border=1 style='FONT-SIZE: 7pt; FONT-FAMILY: Calibri; '> ");
                    diskBusy.Append("<tr style=\"background-color:LightGrey;border-color:Black;border-width:1px;border-style:solid;\">\n");
                    diskBusy.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Disk</th>");
                    diskBusy.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Queue Length</th>");
                    diskBusy.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Busiest file</th>");
                    diskBusy.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Physical IO / Sec</th>");
                    diskBusy.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Logical IO / Sec</th>");
                    diskBusy.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Cache Hit Rate</th>");
                    diskBusy.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">DP2 Busy %</th>");
                    diskBusy.Append("<th align=\"center\" scope=\"col\" style=\"text-decoration:underline;white-space:nowrap;\">Interval period</th>");
                    diskBusy.Append("</tr>\n");

                    var rowIndex = 0;
                    var encrypt = new Decrypt();
                    var encryptSystemSerial = encrypt.strDESEncrypt(systemSerial);

                    foreach (DataRow row in diskBusyData.Rows) {
                        if (rowIndex%2 == 0)
                            diskBusy.Append("<tr>\n");
                        else
                            diskBusy.Append("<tr style='background-color: #E6E6E6;'>\n");
                        //diskBusy.Append("<tr>\n");
                        var deviceName = row["DeviceName"];
                        var encryptStartTime = encrypt.strDESEncrypt(row["FromTimestamp"].ToString());
                        if(!isLocalAnalyst)
                            diskBusy.Append("<td align=\"left\">" + "<a href='" + websiteAddress + "/EmailLogin.aspx?User=@User&System=" + encryptSystemSerial + "&StartDate=" + encryptStartTime + "&DeviceName=" + deviceName + @"'>" + deviceName + "</a></td>\n");
                        else
                            diskBusy.Append("<td align=\"left\">" + "<a href='" + websiteAddress + "/EmailLogin.aspx?User=@User&System=" + encryptSystemSerial + "&StartDate=" + encryptStartTime + "&DeviceName=" + deviceName + @"'>" + deviceName + "</a></td>\n");
                        diskBusy.Append("<td align=\"right\">" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["QueueLength"]), 2)) + "</td>\n");
                        diskBusy.Append("<td align=\"left\">" + row["BusiestFilePhysicalName"] + "</td>\n");
                        diskBusy.Append("<td align=\"right\">" + string.Format("{0:#,##0.00}", row["PhysicalIORate"]) + "</td>\n");
                        diskBusy.Append("<td align=\"right\">" + string.Format("{0:#,##0.00}", row["LogicalIORate"]) + "</td>\n");
                        diskBusy.Append("<td align=\"right\">" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["CacheHitRate"]), 2)) + "</td>\n");
                        diskBusy.Append("<td align=\"right\">" + string.Format("{0:#,##0.00}", Math.Round(Convert.ToDouble(row["DP2Busy"]), 2)) + "</td>\n");
                        diskBusy.Append("<td align=\"left\">" + Convert.ToDateTime(row["FromTimestamp"]).ToString(monthDayPattern + " HH:mm") + "</td>\n");
                        diskBusy.Append("</tr>\n");
                        rowIndex++;
                    }
                    diskBusy.Append("</table></div>");
                }
            }
            
            return diskBusy.ToString();
        }
    }
}
