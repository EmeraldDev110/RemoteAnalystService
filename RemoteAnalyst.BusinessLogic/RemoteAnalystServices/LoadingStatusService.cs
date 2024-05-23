using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices
{
    public class LoadingStatusService
    {
        private readonly string _connectionString = "";

        public LoadingStatusService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void UpdateLoadingStatusFor(string instanceID, int value)
        {
            var loadingStatus = new LoadingStatus(_connectionString);
            loadingStatus.UpdateLoadingStatus(instanceID, value);
        }

        public int GetCurrentLoadFor(string instanceID)
        {
            var loadingStatus = new LoadingStatus(_connectionString);
            int retVal = loadingStatus.GetCurrentLoad(instanceID);
            return retVal;
        }

        public bool CheckLoadingFor(string instanceID)
        {
            var status = new LoadingStatus(_connectionString);
            bool retVal = status.CheckLoading(instanceID);
            return retVal;
        }

        public bool IsLoadAvailableFor(string instanceID) {
            var status = new LoadingStatus(_connectionString);
            bool isOkayToLoad = false;
            int currentLoad = status.CheckCurrentLoads(instanceID);
            if (currentLoad > 0)
                isOkayToLoad = true;

            return isOkayToLoad;
        }

    }
}