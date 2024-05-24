using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zip;
using log4net;

namespace RemoteAnalyst.ReportGenerator.BLL
{
    /// <summary>
    /// ZIP reports
    /// </summary>
    internal class ZIPReports
    {
        /// <summary>
        /// ZIP QT report
        /// </summary>
        /// <param name="systemSerial">System Serial Number</param>
        /// <param name="systemName">System Name</param>
        /// <param name="fileLocation">QT File Location</param>
        /// <param name="startTime">Report Start Time</param>
        /// <param name="endTime">Report Stop Time</param>
        /// <param name="location">Save Location</param>
        /// <returns></returns>
        internal string CreateQTZipFile(string systemSerial, string systemName, List<string> fileLocation,
            DateTime startTime, DateTime endTime, string location)
        {
            try
            {
                //if systemname starts with \, remove it.
                systemName = systemName.Replace('\\', ' ').Trim();
                //Get folderName.
                string folderName = location;
                //Get zip name.
                string zipName;

                if (startTime > DateTime.MinValue && endTime > DateTime.MinValue)
                {
                    zipName = "\\" + systemName + "(" + systemSerial + ") - QT for " +
                              startTime.ToString("yyyy-MM-dd HHmm") + " to " +
                              endTime.ToString("yyyy-MM-dd HHmm") + ".zip";
                }
                else
                {
                    zipName = "\\QT Reports for System " + systemName + "(" + systemSerial + ")" + ".zip";
                }

                //Delet file if exist.
                if (File.Exists(folderName + zipName))
                    File.Delete(folderName + zipName);

                location = folderName + zipName;

                using (var zip = new ZipFile())
                {
                    foreach (string file in fileLocation)
                    {
                        zip.AddFile(file, string.Empty);
                            //Need to add string.empty to second parameter to make the file display on top of the zip layer.
                    }
					zip.BufferSize = 20 * 1024 * 1024; //Set buffer to be 20M because sometime code stuck at zipping step.
					zip.Save(location);
                }
            }
            catch
            {
                location = "";
            }

            return location;
        }

        /// <summary>
        /// Get list of reports to be zipped.
        /// </summary>
        /// <param name="systemSerial">System Serial Number</param>
        /// <param name="systemName">System Name</param>
        /// <param name="location">Excel Location</param>
        /// <param name="startTime">Report Start Time</param>
        /// <param name="endTime">Report Stop Time</param>
        /// <param name="log">ILog</param>
        /// <param name="fileName">Excep File Name</param>
        /// <returns></returns>
        internal string ZipDPAExcelFiles(string systemSerial, string systemName, string location, DateTime startTime,
            DateTime endTime, ILog log, string fileName)
        {
            log.Info("Build location");
            

            string reportName = fileName.Replace("*", "Reports");
            string chartName = fileName.Replace("*", "Charts");
            string iReportName = fileName.Replace("*", "iReport");

            var files = new List<string>();
            if (File.Exists(location + "\\" + reportName))
                files.Add(location + "\\" + reportName);

            if (File.Exists(location + "\\" + chartName))
                files.Add(location + "\\" + chartName);

            if (File.Exists(location + "\\" + iReportName))
                files.Add(location + "\\" + iReportName);

            //if (File.Exists(location + "\\DPA.txt"))
            //    files.Add(location + "\\DPA.txt");

            log.InfoFormat("files: {0}", files.Count);
            
            string saveLocation = "";
            try
            {
                //ZipFiles zip = new ZipFiles();
                saveLocation = CreateDPAZipFile(systemSerial, systemName, files, startTime, endTime, location, log);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error: {0}", ex);
                
            }
            return saveLocation;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="systemSerial">System Serial Number</param>
        /// <param name="systemName">System Name</param>
        /// <param name="fileLocation">Excel Location</param>
        /// <param name="startTime">Report Start Time</param>
        /// <param name="endTime">Report Stop Time</param>
        /// <param name="location">Save Location</param>
        /// <param name="log">ILog</param>
        /// <returns>ZIP Excel Location</returns>
        public string CreateDPAZipFile(string systemSerial, string systemName, List<string> fileLocation,
            DateTime startTime, DateTime endTime, string location, ILog log)
        {
            try
            {
                //if systemname starts with \, remove it.
                systemName = systemName.Replace('\\', ' ').Trim();
                //Get folderName.
                string folderName = location;
                //Get zip name.
                string zipName;

                if (startTime > DateTime.MinValue && endTime > DateTime.MinValue)
                {
                    zipName = "\\" + systemName + "(" + systemSerial + ")" +
                              " - DPA for " + startTime.ToString("yyyy-MM-dd HHmm") + " to " +
                              endTime.ToString("yyyy-MM-dd HHmm") + ".zip";
                }
                else
                {
                    zipName = "\\DPA Reports for System " + systemName + "(" + systemSerial + ")" + ".zip";
                }

                //Delet file if exist.
                if (File.Exists(folderName + zipName))
                    File.Delete(folderName + zipName);

                location = folderName + zipName;

                using (var zip = new ZipFile())
                {
                    foreach (string file in fileLocation)
                    {
                        zip.AddFile(file, string.Empty);
                            //Need to add string.empty to second parameter to make the file display on top of the zip layer.
                    }
					zip.BufferSize = 20 * 1024 * 1024; //Set buffer to be 20M because sometime code stuck at zipping step.
					zip.Save(location);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("ZIP Error: {0}", ex);
                
                location = "";
            }

            return location;
        }

        internal string CreatePathwayZipFile(string systemSerial, string systemName, List<string> fileLocation, string fileName, string location, ILog log) {
            try {
                //if systemname starts with \, remove it.
                systemName = systemName.Replace('\\', ' ').Trim();
                //Get folderName.
                var folderName = location;
                //Get zip name.
                var zipName = fileName.Replace(".xls", ".zip");


                //Delet file if exist.
                if (File.Exists(folderName + zipName))
                    File.Delete(folderName + zipName);

                location = folderName + "\\" + zipName;

                using (var zip = new ZipFile()) {
                    foreach (string file in fileLocation) {
                        zip.AddFile(file, string.Empty);
                        //Need to add string.empty to second parameter to make the file display on top of the zip layer.
                    }
                    zip.Save(location);
                }
            }
            catch (Exception ex) {
                log.ErrorFormat("ZIP Error: {0}", ex);
                
                location = "";
            }

            return location;
        }

        internal string CreateQNMZipFile(string systemSerial, string systemName, 
            List<string> fileLocation, string folderName, DateTime startTime, 
            DateTime endTime, ILog log) {
            string saveLocation;

            try {
                string zipName;

                if (startTime > DateTime.MinValue && endTime > DateTime.MinValue) {
                    zipName = "\\EventPro Reports for System " + systemName + "(" + systemSerial + ")" +
                                     " from " + startTime.ToString("MMM dd yyyy HHmm") +
                                     " to " + endTime.ToString("MMM dd yyyy HHmm") + ".zip";
                }
                else {
                    zipName = "\\EventPro Reports for System " + systemName + "(" + systemSerial + ")" + ".zip";
                }


                //Delet file if exist.
                if (File.Exists(folderName + zipName))
                    File.Delete(folderName + zipName);

                saveLocation = folderName + "\\" + zipName;

                using (var zip = new ZipFile()) {
                    foreach (string file in fileLocation) {
                        zip.AddFile(file, string.Empty);
                        //Need to add string.empty to second parameter to make the file display on top of the zip layer.
                    }
                    zip.Save(saveLocation);
                }
            }
            catch (Exception ex) {
                log.ErrorFormat("ZIP Error: {0}", ex);
                saveLocation = "";
            }

            return saveLocation;
        }

        internal string CreateEventProZipFile(string systemSerial, string systemName, 
            List<string> fileLocation, string folderName, DateTime startTime, 
            DateTime endTime, ILog log) {
            string saveLocation;

            try {
                string zipName;

                if (startTime > DateTime.MinValue && endTime > DateTime.MinValue) {
                    zipName = "\\QNM Report for System " + systemName + "(" + systemSerial + ")" +
                                    " from " + startTime.ToString("MMM dd yyyy HHmm") + " to " +
                                    endTime.ToString("MMM dd yyyy HHmm") + ".zip";
                }
                else {
                    zipName = "\\QNM Report for System " + systemName + "(" + systemSerial + ")" + ".zip";
                }


                //Delet file if exist.
                if (File.Exists(folderName + zipName))
                    File.Delete(folderName + zipName);

                saveLocation = folderName + "\\" + zipName;

                using (var zip = new ZipFile()) {
                    foreach (string file in fileLocation) {
                        zip.AddFile(file, string.Empty);
                        //Need to add string.empty to second parameter to make the file display on top of the zip layer.
                    }
                    zip.Save(saveLocation);
                }
            }
            catch (Exception ex) {
                log.ErrorFormat("ZIP Error: {0}", ex);
                saveLocation = "";
            }

            return saveLocation;
        }

        internal string CreateTraceZipFile(List<string> fileLocation, string folderName, ILog log) {
            string saveLocation;

            try {
                string zipName = "";

                //Get file name and use it on zipName.
                foreach (var fileInfo in fileLocation.Select(s => new FileInfo(s)).Where(fileInfo => !fileInfo.Name.Equals("TRACE_Help.html"))) {
                    zipName = fileInfo.Name.Replace("xls", "zip");
                    break;
                }


                //Delet file if exist.
                if (File.Exists(folderName + zipName))
                    File.Delete(folderName + zipName);

                saveLocation = folderName + "\\" + zipName;

                using (var zip = new ZipFile()) {
                    foreach (var file in fileLocation) {
                        zip.AddFile(file, string.Empty);
                        //Need to add string.empty to second parameter to make the file display on top of the zip layer.
                    }
                    zip.Save(saveLocation);
                }
            }
            catch (Exception ex) {
                log.ErrorFormat("ZIP Error: {0}", ex);                
                saveLocation = "";
            }

            return saveLocation;
        }
    }
}