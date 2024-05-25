using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteAnalyst.UWSLoader.Core.Repository;

namespace RemoteAnalyst.UWSLoader.Core.BusinessLogic {
    class VProcVersionService {
        private readonly string _connectionString = "";

        public VProcVersionService(string connectionString) {
            _connectionString = connectionString;
        }

        public string GetVProcVersionFor(string vProcVersion) {
            string className = "";
            var vProcVersions = new VProcVersions(_connectionString);
            className = vProcVersions.GetClassName(vProcVersion);

            return className;
        }

        public string GetDataDictionaryFor(string vProcVersion) {
            string dataDictionary = "";
            var vProcVersions = new VProcVersions(_connectionString);
            dataDictionary = vProcVersions.GetDataDictionary(vProcVersion);

            return dataDictionary;
        }
    }
}
