using System.Collections.Generic;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices
{
    public class ReportGroupDetailService
    {
        private readonly string _connectionString;

        public ReportGroupDetailService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IList<int> GetReportIDsFor(int groupID)
        {
            var reportGroupDetail = new ReportGroupDetail(_connectionString);
            IList<int> reports = reportGroupDetail.GetReportIDs(groupID);

            return reports;
        }
    }
}