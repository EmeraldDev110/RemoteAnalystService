using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteAnalyst.UWSLoader.Core.Repository;

namespace RemoteAnalyst.UWSLoader.Core.BusinessLogic {
    class MeasureVersionsService {
        private readonly string _connectionString;

        public MeasureVersionsService(string connectionString) {
            _connectionString = connectionString;
        }

        public string GetMeasureDBTableNameFor(string version) {
            var measureVersions = new MeasureVersions(_connectionString);
            string retVal = measureVersions.GetMeasureDBTableName(version);
            return retVal;
        }
    }
}
