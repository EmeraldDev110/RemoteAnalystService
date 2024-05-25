using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DataBrowser.Context;
using DataBrowser.Entities;
using log4net;
using MySQLDataBrowser.Model;
using RemoteAnalyst.UWSLoader.Core.BaseClass;
using RemoteAnalyst.UWSLoader.Core.BusinessLogic;

namespace RemoteAnalyst.UWSLoader.Core.DiskBrowser {
    class DiskBrowserLoader {
        ILog Log = LogManager.GetLogger("DiskBrowser");

        internal void LoadDiskBrowserData(string systemSerial, string newConnectionString, string mySqlConnectionString, string cpuTableName, string discTableName, long sampleInterval, string systemFolder) {
            Log.Info("Create DISC Browser table");
            //Use cpu intervals to loop and populate the from & to timestamps
            var cpuEntityTableService = new CPUEntityTableService(newConnectionString);
            var discEntityTableService = new DISCEntityTableService(mySqlConnectionString);

            List<MultiDays> cpuIntervalList = cpuEntityTableService.GetCPUEntityTableIntervalListFor(cpuTableName);
            var discBrowserTableName = discTableName.Replace("DISC", "DISKBROWSER");

            List<MultiDays> discBrowserIntervalList = discEntityTableService.GetDISCEntityTableIntervalListFor(discBrowserTableName);
            List<MultiDays> intervalList = cpuIntervalList.Except(discBrowserIntervalList).ToList();

            Log.InfoFormat("intervalList: {0}", intervalList.Count);

            var discBrowserData = new List<DISCBrowserView>();
            var discService = new DISKService(new DataContext(newConnectionString), cpuTableName);

            foreach (var interval in intervalList) {
                try {
                    var data = discService.GetDISCBrowserData(cpuTableName, interval.StartDate, interval.EndDate, sampleInterval, mySqlConnectionString, Log);
                    discBrowserData = discBrowserData.Concat(data).ToList();
                }
                catch (Exception ex) {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("cpuTableName: {0}");
                    sb.AppendLine("StartDate: {1}");
                    sb.AppendLine("EndDate: {2}");
                    sb.AppendLine("sampleInterval: {3}");
                    sb.AppendLine("mySqlConnectionString: {4}");

                    Log.ErrorFormat(sb.ToString(),
                        cpuTableName,
                        interval.StartDate, interval.EndDate,
                        sampleInterval, MySQLServices.RemovePassword(mySqlConnectionString));
                    Log.ErrorFormat("Error on GetDISCBrowserData: {0} ", ex);
                }
            }

            Log.InfoFormat("Start populating {0}", discBrowserTableName);
            var discBrowser = new DISCBrowser(new DataContext(mySqlConnectionString), discBrowserTableName);
            discBrowser.PopulateDISCBrowserTable(discBrowserData, systemFolder + systemSerial + @"\");
            Log.InfoFormat("******  Disk browser data load completed  **********");
        }

    }
}
