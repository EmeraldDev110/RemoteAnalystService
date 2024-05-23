using System;
using System.Collections.Generic;
using System.Data;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices
{
    public class LoadingInfoDiskService
    {
        private readonly string ConnectionString = "";

        public LoadingInfoDiskService(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public IList<LoadingInfoDiskView> GetUWSFileNameFor(string systemSerial)
        {
            IList<LoadingInfoDiskView> fileNames = new List<LoadingInfoDiskView>();

            var loadingInfoDisk = new LoadingInfoDisk(ConnectionString);

            DataTable fileName = loadingInfoDisk.GetUWSFileName(systemSerial);

            foreach (DataRow dr in fileName.Rows)
            {
                var view = new LoadingInfoDiskView();
                view.DiskUWSID = Convert.ToInt32(dr["DiskUWSID"]);
                view.FileName = Convert.ToString(dr["FileName"]);
                view.UploadTime = Convert.ToDateTime(dr["UploadedTime"]);

                fileNames.Add(view);
            }

            return fileNames;
        }

        public void DeleteLoadingInfoDiskFor(int uwsID)
        {
            var loadingInfoDisk = new LoadingInfoDisk(ConnectionString);
            loadingInfoDisk.DeleteLoadingInfoDisk(uwsID);
        }

        public void UpdateFailedToLoadDiskFor(string fileName)
        {
            var loadingInfoDisk = new LoadingInfoDisk(ConnectionString);
            loadingInfoDisk.UpdateFailedToLoadDisk(fileName);
        }

        public void UpdateLoadingInfoDiskFor(string fileName)
        {
            var loadingInfoDisk = new LoadingInfoDisk(ConnectionString);
            loadingInfoDisk.UpdateLoadingInfoDisk(fileName);
        }

        public void InsertFor(string systemSerial, int customerID, string fileName, long fileSize)
        {
            var loadingInfoDisk = new LoadingInfoDisk(ConnectionString);
            loadingInfoDisk.Insert(systemSerial, customerID, fileName, fileSize);
        }
    }
}