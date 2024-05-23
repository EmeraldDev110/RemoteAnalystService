using System.Collections.Generic;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices
{
    public class ReportEntitieService
    {
        private readonly string ConnectionString = "";

        public ReportEntitieService(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public IList<int> GetReportWithFileEntityFor()
        {
            var reportEntities = new ReportEntities(ConnectionString);
            IList<int> reportWithFileEntity = reportEntities.GetReportWithFileEntity();
            return reportWithFileEntity;
        }

    }
}