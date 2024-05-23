using System;
using System.Collections.Generic;
using System.Data;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices
{
    public class StorageReportService
    {
        private readonly string _connectionString;

        public StorageReportService(string connectionStriong)
        {
            _connectionString = connectionStriong;
        }

        public bool CheckCapacitiesFor(int deliveryID)
        {
            bool exists = false;
            var storageReport = new StorageReport(_connectionString);
            exists = storageReport.CheckCapacities(deliveryID);

            return exists;
        }

        public IList<StorageReportView> GetSchduleDataFor(int deliveryID)
        {
            var storageReport = new StorageReport(_connectionString);
            DataTable schedules = storageReport.GetSchduleData(deliveryID);
            IList<StorageReportView> allSchedules = new List<StorageReportView>();

            foreach (DataRow dr in schedules.Rows)
            {
                var view = new StorageReportView();
                view.StorageID = Convert.ToInt32(dr["SR_StorageID"]);
                view.GroupDisk = Convert.ToInt32(dr["SR_GroupDisk"]);
                view.GroupDiskID = Convert.ToInt32(dr["SR_GroupDiskID"]);
                view.CustomerID = Convert.ToInt32(dr["SR_CustomerID"]);
                allSchedules.Add(view);
            }

            return allSchedules;
        }
    }
}