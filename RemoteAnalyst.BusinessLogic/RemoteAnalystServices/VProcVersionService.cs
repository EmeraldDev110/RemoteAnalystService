using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class VProcVersionService {
        private readonly string ConnectionString = "";

        public VProcVersionService(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string GetVProcVersionFor(string vProcVersion) {
            string className = "";
            var vProcVersions = new VProcVersions(ConnectionString);
            className = vProcVersions.GetClassName(vProcVersion);

            return className;
        }

        public string GetDataDictionaryFor(string vProcVersion) {
            string dataDictionary = "";
            var vProcVersions = new VProcVersions(ConnectionString);
            dataDictionary = vProcVersions.GetDataDictionary(vProcVersion);

            return dataDictionary;
        }

        public string GetVprocVersionFor(string vProcVersion) {
            string dataDictionary = "";
            var vProcVersions = new VProcVersions(ConnectionString);
            dataDictionary = vProcVersions.GetVprocVersion(vProcVersion);

            return dataDictionary;
        }
    }
}
