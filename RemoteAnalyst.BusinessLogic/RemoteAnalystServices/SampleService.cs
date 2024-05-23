using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices
{
    public class SampleService
    {
        private readonly Sample sample;
        private string ConnectionString = "";

        public SampleService(string connectionString)
        {
            ConnectionString = connectionString;
            sample = new Sample(ConnectionString);
        }

        public int GetMaxNSIDFor()
        {
            int nsID = 0;
            nsID = sample.GetMaxNSID();
            return nsID;
        }

        public void InsertNewEntryPathwayFor(string UwsSystemName, int nsid, string _UWSPath)
        {
            sample.InsertNewEntryPathway(UwsSystemName, nsid, _UWSPath);
        }
    }
}