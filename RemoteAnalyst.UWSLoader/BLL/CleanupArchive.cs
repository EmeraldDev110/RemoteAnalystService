using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using RemoteAnalyst.AWS.Infrastructure;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.UWSLoader.BLL
{
    class CleanupArchive
    {
        private static readonly ILog Log = LogManager.GetLogger("Cleaner");
        LoadingInfoService loadingInfo = new LoadingInfoService(ConnectionString.ConnectionStringDB);
        public void Timer_Elapsed(object source, System.Timers.ElapsedEventArgs e)
        {
            int currHour = DateTime.Now.Hour;

            if (currHour.Equals(5))
            {
                DeleteArchive();
            }
        }
        public void DeleteArchive()
        {
            try
            {
                Log.Info("In DeleteArchive");
                IDictionary<string, int> retentionDays = loadingInfo.GetUWSRetentionDayFor();
                Log.InfoFormat("retentionDays count: {0}", retentionDays.Count);
                foreach (KeyValuePair<string, int> kv in retentionDays)
                {
                    Log.InfoFormat("calling CleanupArchiveFromLoadingInfo: {0}|{1}", kv.Key, kv.Value);
                    CleanupArchiveFromLoadingInfo(kv.Key, kv.Value);
                    //DeleteFromAnalystRpts. We no longer have Analyst Report.
                    Log.InfoFormat("calling DeleteDiskData: {0}|{1}", kv.Key, kv.Value);
                    DeleteDiskData(kv.Key, kv.Value);
                    Log.InfoFormat("calling DeleteLogFile: {0}|{1}", kv.Key, kv.Value);
                    DeleteLogFile(kv.Key, kv.Value);
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("DeleteArchive Error: {0}", ex.Message);
                AmazonOperations amazon = new AmazonOperations();
                amazon.WriteErrorQueue("DeleteArchive Error: " + ex.Message);
            }
        }

        private void DeleteDiskData(string systemSerial, int uwsRetentionDay)
        {

            //Get data information from LoadingInfoDisk.
            LoadingInfoDiskService loadingInfo = new LoadingInfoDiskService(ConnectionString.ConnectionStringDB);
            IList<LoadingInfoDiskView> diskServices = loadingInfo.GetUWSFileNameFor(systemSerial);

            foreach (LoadingInfoDiskView service in diskServices)
            {
                DateTime uploadedTime = Convert.ToDateTime(service.UploadTime);
                string fileName = service.FileName;

                //Check date.
                DateTime retentionDate = uploadedTime.AddDays(uwsRetentionDay);

                if (retentionDate < DateTime.Today)
                {
                    //Build location and delete file.
                    string path = ConnectionString.ServerPath;
                    string systemLocation = ConnectionString.SystemLocation;
                    string fileLocation = systemLocation + systemSerial + "\\" + fileName;

                    if (File.Exists(fileLocation))
                    {
                        File.Delete(fileLocation);

                        //Delete from LoadingInfoDisk.
                        loadingInfo.DeleteLoadingInfoDiskFor(service.DiskUWSID);
                    }
                }
            }
        }

        private void DeleteLogFile(string systemSerial, int uwsRetentionDay)
        {
            string path = ConnectionString.ServerPath;
            string systemLocation = ConnectionString.SystemLocation;
            string fileLocation = systemLocation + systemSerial;

            try
            {
                string[] files = Directory.GetFiles(fileLocation);
                foreach (string s in files)
                {
                    DateTime createdDate = File.GetCreationTime(s);

                    DateTime retentionDate = createdDate.AddDays(uwsRetentionDay);

                    if (retentionDate < DateTime.Today)
                    {
                        if (File.Exists(s))
                        {
                            File.Delete(s);
                        }
                    }
                }

                //also delete 101 files
                files = Directory.GetFiles(fileLocation, "*.101");
                foreach (string s in files) {
                    DateTime createdDate = File.GetCreationTime(s);

                    DateTime retentionDate = createdDate.AddDays(uwsRetentionDay);

                    if (retentionDate < DateTime.Today) {
                        if (File.Exists(s)) {
                            File.Delete(s);
                        }
                    }
                }
            }
            catch
            {
                //skip
            }
        }


        private void CleanupArchiveFromLoadingInfo(string systemSerial, int uwsRetentionDay)
        {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            //If retentinoday is 0, get the system default from RAInfo table.
            if (uwsRetentionDay.ToString() == "0")
            {
                //UWSRetentionDay
                RAInfoService raInfo = new RAInfoService(ConnectionString.ConnectionStringDB);
                uwsRetentionDay = Convert.ToInt32(raInfo.GetQueryValueFor("UWSRetentionDay"));  //This is hard coded value for getting default Retention Day.
            }

            //Calcualte the RetentionDay.
            DateTime dtEndDate = new DateTime();
            dtEndDate = DateTime.Today;
            dtEndDate = dtEndDate.AddDays(-uwsRetentionDay);

            //Delete the uws file.
            DeleteUWS(systemSerial, dtEndDate);

            loadingInfo.UpdateFileStatFor(systemSerial, dtEndDate);
        }

        private void DeleteUWS(string systemSerial, DateTime dtEndDate)
        {
            List<string> fileNames = loadingInfo.GetUWSFileNameFor(systemSerial, dtEndDate);

            foreach (string fileName in fileNames) {
                if (fileName.Contains("\\Systems\\" + systemSerial + "\\")) {
                    if (File.Exists(fileName))
                        File.Delete(fileName);
                }
                else {
                    //Try adding system path.
                    if (File.Exists(ConnectionString.ServerPath + "\\" + fileName))
                        File.Delete(ConnectionString.ServerPath + "\\" + fileName);
                }
            }
        }
    }
}
