using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class InHouseConfigService {
        private readonly string _connectionString = "";
        public InHouseConfigService(string connectionString) {           
            _connectionString = connectionString;
        }
        public DataTable GetNonstopVolumnAndIpPair() {
            InHouseConfig inHouseConfig = new InHouseConfig(_connectionString);
            DataTable volumnIpPair = inHouseConfig.GetNonstopVolumnAndIpPair();
            return volumnIpPair;
        }
    }
}
