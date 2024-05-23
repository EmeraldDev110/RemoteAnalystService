using System;
using System.Data;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices
{
    public class PvCollectsService
    {
        private readonly PvCollects _pvCollects;

        public PvCollectsService(string connectionString)
        {
            _pvCollects = new PvCollects(connectionString);
        }

        public CollectionInfo GetIntervalFor(DateTime fromTimestamp, DateTime toTimestamp)
        {
            var table = _pvCollects.GetInterval(fromTimestamp, toTimestamp);
            var collectorInfo = new CollectionInfo();
            foreach (DataRow row in table.Rows)
            {
                collectorInfo.IntervalNumber = int.Parse(row["IntervalNn"].ToString());
                collectorInfo.IntervalType = row["IntervalHOrM"].ToString();
            }
            return collectorInfo;
        }
    }
}