using System;
using System.Collections.Generic;
using System.IO;
using RemoteAnalyst.BusinessLogic.BLL;
using RemoteAnalyst.BusinessLogic.ModelView;

namespace RemoteAnalyst.Trigger.JobPool {
    public class Triggers {
        public void WriteLoadMessage(string jobPoolPath, string systemSerial, string triggerMessage) {
            string buildFileName = jobPoolPath + "\\jobauto_" + systemSerial + "_" + DateTime.Now.Ticks;
            string fileNameTxt = buildFileName + ".txt";
            string fileName101 = buildFileName + ".101";

            using (var writer = new StreamWriter(fileName101)) {
                writer.Write(triggerMessage);
                
            }

            using (var writer = new StreamWriter(fileNameTxt)) {
                writer.Write(triggerMessage);
                
            }
        }

        public void WriteReportMessage(string jobPoolPath, int reportType, string systemSerial, string triggerMessage) {
            string buildFileName = "";

            switch (reportType) {
                case 1: //No System Serial for Trend, because there can be multiple systems on trigger file.
                    buildFileName = jobPoolPath + "\\jobReportTrend_" + DateTime.Now.Ticks;
                    break;
                case 2:
                    buildFileName = jobPoolPath + "\\jobReportStorage_" + systemSerial + "_" + DateTime.Now.Ticks;
                    break;
                case 3:
                    buildFileName = jobPoolPath + "\\jobReportQT_" + systemSerial + "_" + DateTime.Now.Ticks;
                    break;
                case 4:
                    buildFileName = jobPoolPath + "\\jobReport_" + systemSerial + "_" + DateTime.Now.Ticks;
                    break;
                case 5:
                    buildFileName = jobPoolPath + "\\jobReportPathway_" + systemSerial + "_" + DateTime.Now.Ticks;
                    break;
            }

            string fileNameTxt = buildFileName + ".txt";
            string fileName101 = buildFileName + ".101";

            using (var writer = new StreamWriter(fileName101)) {
                writer.Write(triggerMessage);
                
            }

            using (var writer = new StreamWriter(fileNameTxt)) {
                writer.Write(triggerMessage);
                
            }
        }

        public List<UWSTriggerView> ReadOldTriggerFile(string jobPoolPath, string systemPath, string fileName) {
            var triggers = new List<UWSTriggerView>();
            var util = new FileUtil(jobPoolPath);
            string uwsFileName = util.GetUWSFileName(fileName);
            string systemSerial = util.GetSystemSerial(fileName);
            string cpuInfo = util.GetCPUFileName(fileName);
            string ossInfo = util.GetOSSJRNLName(fileName);
            string DISCOPENFileName = util.GetDISCOPENFileName(fileName);

            if (uwsFileName.Length > 0) {
                string uwsFullPath = systemPath + systemSerial + "\\" + uwsFileName;
                var uwsFileInfo = new UWSFileInfo();

                var trigger = new UWSTriggerView();
                if (fileName.Contains("jobdisk"))
                    trigger.TriggerType = "DISK";
                else
                    trigger.TriggerType = uwsFileInfo.GetFileType(uwsFullPath).ToUpper();
                trigger.FileName = uwsFullPath;
                trigger.SystemSerial = systemSerial;
                triggers.Add(trigger);
            }
            if (cpuInfo.Trim().Length > 1) {
                //string cpuInfoFullPath = systemPath + "\\" + systemSerial + "\\" + cpuInfo;
                var trigger = new UWSTriggerView {TriggerType = "CPUINFO", FileName = cpuInfo, SystemSerial = systemSerial};
                triggers.Add(trigger);
            }
            if (ossInfo.Trim().Length > 1)
            {
                string ossInfoFullPath = systemPath + systemSerial + "\\" + ossInfo;
                var trigger = new UWSTriggerView { TriggerType = "JOURNAL", FileName = ossInfoFullPath, SystemSerial = systemSerial };
                triggers.Add(trigger);
            }
            if (DISCOPENFileName.Trim().Length > 1)
            {
                string DISCOPENFullPath = systemPath + systemSerial + "\\" + DISCOPENFileName;
                var trigger = new UWSTriggerView { TriggerType = "SYSTEM", FileName = DISCOPENFullPath, SystemSerial = systemSerial };
                triggers.Add(trigger);
            }
            return triggers;
        }
    }
}