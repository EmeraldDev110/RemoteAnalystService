using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices
{
    public class MeasureVersionsService
    {
        private readonly string ConnectionString = "";

        public MeasureVersionsService(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string GetMeasureDBTableNameFor(string version)
        {
            var measureVersions = new MeasureVersions(ConnectionString);
            string retVal = measureVersions.GetMeasureDBTableName(version);
            return retVal;
        }
    }
}