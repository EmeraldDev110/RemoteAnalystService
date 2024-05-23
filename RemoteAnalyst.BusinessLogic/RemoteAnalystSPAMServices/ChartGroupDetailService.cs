using System.Collections.Generic;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices
{
    public class ChartGroupDetailService
    {
        private readonly string _connectionString;

        public ChartGroupDetailService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IList<int> GetChartIDsFor(int groupID)
        {
            var chartGroupDetail = new ChartGroupDetail(_connectionString);
            IList<int> charts = chartGroupDetail.GetChartIDs(groupID);

            return charts;
        }
    }
}