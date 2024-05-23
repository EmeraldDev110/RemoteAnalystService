using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices {
    public class UWSLoadingStatusService {
        private readonly string _connectionString;

        public UWSLoadingStatusService(string connectionString) {
            _connectionString = connectionString;
        }

        public bool CheckUWSLoadingStatusFor(string systemSerial, string fileName) {
            var loadingStatus = new UWSLoadingStatus(_connectionString);
            bool status = loadingStatus.CheckUWSLoadingStatus(systemSerial, fileName);

            return status;
        }

        public void InsertUWSLoadingStatusFor(string systemSerial, string fileName) {
            var loadingStatus = new UWSLoadingStatus(_connectionString);
            loadingStatus.InsertUWSLoadingStatus(systemSerial, fileName);
        }

        public void DeleteUWSLoadingStatusFor(string systemSerial, string fileName) {
            var loadingStatus = new UWSLoadingStatus(_connectionString);
            loadingStatus.DeleteUWSLoadingStatus(systemSerial, fileName);
        }
    }
}