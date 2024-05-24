using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NonStopSPAM.BLL;
using NonStopSPAM.BLL.BaseClass;
using RemoteAnalyst.AWS.S3;
using RemoteAnalyst.AWS.EC2;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;
using log4net;

namespace RemoteAnalyst.ReportGenerator.BLL
{
    /// <summary>
    /// Generate DPA by calling PerformanceLib.exe
    /// </summary>
    internal class GenerateDPA
    {
        private readonly string _systemSerial = "";
        private const string errorMessage = "We are currently experiencing a problem generating DPA report. Please be advised that our support team is actively working on resolving this issue and will provide you an update shortly. We apologize for any inconvenience caused.";
        private const string localAnalystErrorMessage = "We are currently experiencing a problem generating DPA report. Please contact support for assistance.";


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="systemSerial">System Serial Number</param>
        public GenerateDPA(string systemSerial)
        {
            _systemSerial = systemSerial;
        }

        /// <summary>
        /// Generate DPA
        /// </summary>
        /// <param name="param">DPA Parameters</param>
        /// <param name="log">ILog</param>
        /// <param name="emails">List of Emails</param>
        /// <param name="ntsOrderId">NTS Order Id</param>
        public void ReportCall(ReportParameters param, ILog log, List<string> emails, 
            int ntsOrderId, bool isLocalAnalyst, bool attachmentInEmail)
        {
            //Check if selected reports / charts needs DISCOPEN Entity.
            var reportEntities = new ReportEntitieService(param.ConnectionString);
            var chartEntities = new ChartEntitieService(param.ConnectionString);

            var reportDownload = new ReportDownloadService(ConnectionString.ConnectionStringDB);

            //Check if you need to add Index.
            //1. Check if selected report/chart uses File entity.

            IList<int> reportWithFileEntity = reportEntities.GetReportWithFileEntityFor();
            IList<int> chartWithFileEntity = chartEntities.GetChartWithFileEntityFor();

            List<int> reportList = param.ReportIDs.Intersect(reportWithFileEntity).ToList();
            List<int> chartList = param.ChartIDs.Intersect(chartWithFileEntity).ToList();

            var database = new Database(param.ConnectionString);
            var dataTables = new DataTableService(param.ConnectionString);
            var databaseName = Helper.FindKeyName(param.ConnectionString, "DATABASE");
            if (reportList.Count > 0 || chartList.Count > 0)
            {
                //2. if yes, look through the File table and check if index exists. If not create one.
                for (DateTime day = param.StartTime.Date; day.Date <= param.EndTime.Date; day = day.AddDays(1))
                {
                    string fileTableName = param.SystemSerial + "_FILE_" + day.Year + "_" + day.Month + "_" + day.Day;
                    bool checkTableExists = database.CheckTableExists(fileTableName, databaseName);
                    if (checkTableExists)
                    {
                        dataTables.CreateFileIndexFor(fileTableName);
                    }
                }
            }

            //Set Row limit.
            //Check to see if report need tempTables.
            try
            {
                var tempTable = new CreateTempTable();
                param.TempReportTables = new Dictionary<int, string>();
                tempTable.CheckTempTable(param);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("LoadDPA Temp Tables: {0}", ex);
            }

            int numThreads = 0;
            if (param.ReportIDs.Count > 0)
            {
                numThreads++;
            }
            if (param.ChartIDs.Count > 0)
            {
                numThreads++;
            }

            //Munually control the event.
            var resetEvents = new ManualResetEvent[numThreads];
            int startThread = 0;

            string systemName = param.SystemName;

            try
            {
#if DEBUG
                //var createReport1 = new CreateNTSReport(param, log);
                //createReport1.ProcessReport();
#endif
                if (param.ReportIDs.Count > 0)
                {
                    log.Info("Calling Reports");
                    
                    var createReport = new CreateNTSReport(param);
                    //Set event value to false, and reset it on the class.
                    resetEvents[startThread] = new ManualResetEvent(false);
                    ThreadPool.QueueUserWorkItem(createReport.ProcessReport, resetEvents[startThread]);
                    startThread++;
                }

                if (param.ChartIDs.Count > 0)
                {
                    log.Info("Calling Charts");
                    
                    var createChart = new CreateNTSChart(param);
                    resetEvents[startThread] = new ManualResetEvent(false);
                    ThreadPool.QueueUserWorkItem(createChart.ProcessChart, resetEvents[startThread]);
                    // ReSharper disable once RedundantAssignment
                    startThread++;
                }

                //Wait untill all the threads on thread pool is done.
                // ReSharper disable once CoVariantArrayConversion
                WaitHandle.WaitAll(resetEvents);

                log.Info("Delete Temp Tables");
                
                //Delete Temp Tables.
                var dataTableService = new DataTableService(param.ConnectionString);
                foreach (KeyValuePair<int, string> d in param.TempReportTables)
                {
                    dataTableService.DeleteTempTableFor(d.Value);
                }


                var reportDownloadLogService = new ReportDownloadLogService(ConnectionString.ConnectionStringDB);
                var reportStartTime = reportDownloadLogService.GetFirstLogDateFor(param.ReportDownloadId);
                var totalReportTime = DateTime.Now - reportStartTime;

                var hours = (totalReportTime.Days * 24) + totalReportTime.Hours;
                var minutes = totalReportTime.Minutes;
                var newMessage = "Analyses is completed. ET (hh:mm): " + hours.ToString("D2") + ":" + minutes.ToString("D2");
                reportDownloadLogService.InsertNewLogFor(param.ReportDownloadId, DateTime.Now, newMessage);

                log.Info("Create help html file");
                
                //Create help html file.
                //var objCreateHelp = new CreateHelp();
                //objCreateHelp.CreateCHMHelp(param);

                //Zip the Excel files.
                var zipReports = new ZIPReports();
                string zipLocation = zipReports.ZipDPAExcelFiles(_systemSerial, systemName, param.FolderName, param.StartTime,
                    param.EndTime, log, param.ExcelName);

                if (zipLocation.Equals(""))
                {
                    throw new Exception();
                }

                log.InfoFormat("zipLocation: {0}", zipLocation);
                
                var fileInfo = new FileInfo(zipLocation);

                string zipSaveLocation = "";
                if (ConnectionString.IsLocalAnalyst)
                {
                    //Save to network location.
                    var networkSaveLocation = ConnectionString.NetworkStorageLocation;
                    if (!Directory.Exists(networkSaveLocation + "Systems/" + _systemSerial + "/" + param.ReportDownloadId + "/"))
                        Directory.CreateDirectory(networkSaveLocation + "Systems/" + _systemSerial + "/" + param.ReportDownloadId + "/");

                    zipSaveLocation = networkSaveLocation + "Systems/" + _systemSerial + "/" + param.ReportDownloadId + "/" + fileInfo.Name;
                    fileInfo.CopyTo(zipSaveLocation, true);
                }
                else
                {
                    //Save the file to S3.
                    //if (ntsOrderId == 0) {
                    if (!string.IsNullOrEmpty(ConnectionString.S3Reports))
                    {
                        var s3 = new AmazonS3(ConnectionString.S3Reports);
                        s3.WriteToS3WithLocaFile("Systems/" + _systemSerial + "/" + param.ReportDownloadId + "/" + fileInfo.Name, zipLocation);
                        //Build S3 full URL
                        zipSaveLocation = "Systems/" + _systemSerial + "/" + param.ReportDownloadId + "/" + fileInfo.Name;
                    }
                }
                log.InfoFormat("zipSaveLocation: {0}", zipSaveLocation);
                

                log.InfoFormat("_systemSerial: {0}", _systemSerial);
                log.InfoFormat("StartTime: {0}", param.StartTime);
                log.InfoFormat("EndTime: {0}", param.EndTime);
                

                //if (ntsOrderId == 0) {
                var reportDownloads = new ReportDownloadService(ConnectionString.ConnectionStringDB);
                reportDownloads.UpdateFileLocationFor(param.ReportDownloadId, zipSaveLocation);
                foreach (string s in emails)
                {
                    log.InfoFormat("Send Email to: {0}", s);
                    
                }

                //Email Report.
                var emailReport = new EmailReport();
                if (fileInfo.Length < 5242880 && attachmentInEmail)
                {
                        emailReport.SendDPAReportEmail(zipLocation, emails, systemName, param.StartTime, param.EndTime, param.Intervals);
                }
                else {
                    emailReport.SendDPAReportNotification(emails, _systemSerial, systemName, param.StartTime, param.EndTime, param.ReportDownloadId, attachmentInEmail);
                }

                foreach (string s in emails)
                {
                    reportDownloadLogService.InsertNewLogFor(param.ReportDownloadId, DateTime.Now, "Analyses is emailed to " + s + ".");
                }
                reportDownload.UpdateStatusFor(param.ReportDownloadId, 1);
            }
            catch (Exception ex)
            {
                reportDownload.UpdateStatusFor(param.ReportDownloadId, 2);
                var reportDownloadLogService = new ReportDownloadLogService(ConnectionString.ConnectionStringDB);
                var errorEmails = new ErrorEmails();

                if (ConnectionString.IsLocalAnalyst)
                {
                    reportDownloadLogService.InsertNewLogFor(param.ReportDownloadId, DateTime.Now, localAnalystErrorMessage);
                    errorEmails.SendReportErrorEmail(systemName, param.StartTime, param.EndTime, ex.Message, "DPA", "");
                }
                else
                {
                    reportDownloadLogService.InsertNewLogFor(param.ReportDownloadId, DateTime.Now, errorMessage);
                    var ec2 = new AmazonEC2();
                    string instanceID = ec2.GetEC2ID();
                    errorEmails.SendReportErrorEmail(systemName, param.StartTime, param.EndTime, ex.Message, "DPA", instanceID);
                }

                throw new Exception(ex.Message);
            }
            finally
            {
                //writer.Close();
                GC.Collect();
            }
        }
    }
}