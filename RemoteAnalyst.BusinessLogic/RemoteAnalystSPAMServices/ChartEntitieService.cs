using System.Collections.Generic;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices
{
    public class ChartEntitieService
    {
        private readonly string ConnectionString = "";

        public ChartEntitieService(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public IList<int> GetChartWithFileEntityFor()
        {
            var chartEntities = new ChartEntities(ConnectionString);

            IList<int> chartWithFileEntity = chartEntities.GetChartWithFileEntity();

            return chartWithFileEntity;
        }

    }
}