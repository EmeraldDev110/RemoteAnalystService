using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteAnalyst.UWSLoader.Core.Repository;

namespace RemoteAnalyst.UWSLoader.Core.BusinessLogic {
    class TempTableTimestampService {
        private readonly string _connectionString = "";

        public TempTableTimestampService(string connectionString) {
            _connectionString = connectionString;
        }

        public void DeleteTempTimeStampFor(string tableName) {
            var tempTableTimestamp = new TempTableTimestamp(_connectionString);
            tempTableTimestamp.DeleteTempTimeStamp(tableName);
        }

        public void InsertTempTimeStampFor(string tableName, DateTime startTime, DateTime stopTime) {
            var tempTableTimestamp = new TempTableTimestamp(_connectionString);
            tempTableTimestamp.InsertTempTimeStamp(tableName, startTime, stopTime);
        }
    }
}
