using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class SystemSerialConversionService {
        private readonly string _connectionString;

        public SystemSerialConversionService(string connectionString) {
            _connectionString = connectionString;
        }
        public string GetConvertionSystemSerialFor(string systemSerial) {
            var systemSerialConversions = new SystemSerialConversions(_connectionString);
            string newSystemSerial = systemSerialConversions.GetNewSystemSerial(systemSerial);

            return newSystemSerial;
        }
    }
}
