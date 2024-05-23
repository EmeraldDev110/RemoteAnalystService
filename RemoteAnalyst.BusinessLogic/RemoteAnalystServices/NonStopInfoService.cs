using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class NonStopInfoService {
        private readonly string _connectionString = "";

        public NonStopInfoService(string connectionString)
        {
            _connectionString = connectionString;
        }
        public DataTable GetNonStopInfoFor() {
            var nonStopInfo = new NonStopInfo(_connectionString);
            var nonStopDate = nonStopInfo.GetNonStopInfo();

            return nonStopDate;
        }
    }
}
