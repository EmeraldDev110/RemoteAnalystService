using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using RemoteAnalyst.UWSLoader.Core.Repository;

namespace RemoteAnalyst.UWSLoader.Core.BusinessLogic {
    class LoadingInfoService {
        private readonly string _connectionString = "";

        public LoadingInfoService(string connectionString) {
            _connectionString = connectionString;
        }
        
        public void UpdateFor(int uwsID, long uwsFileSize, string systemName, DateTime startTime, DateTime stopTime,
            int type) {
            var loadingInfo = new LoadingInfo(_connectionString);
            loadingInfo.Update(uwsID, uwsFileSize, systemName, startTime, stopTime, type);
        }
    }
}
