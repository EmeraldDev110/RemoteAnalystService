using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class TMonFileNamesService {
        private readonly TMonFileNames tMonFileNames;

        public TMonFileNamesService(TMonFileNames tMonFileNames) {
            this.tMonFileNames = tMonFileNames;
        }

        public string GetExpectedFileNameFor(string systemSerial, string interval) {
            return tMonFileNames.GetExpectedFileName(systemSerial, interval);
        }
    }
}