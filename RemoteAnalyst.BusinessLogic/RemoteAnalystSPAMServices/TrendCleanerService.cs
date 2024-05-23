using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices {
    public class TrendCleanerService {
        private readonly string _connectionString;
        public TrendCleanerService(string connectionString) {
            _connectionString = connectionString;
        }

        public void DeleteDataFor(string trendTableName, string dateColumnName, DateTime oldDate) {
            var trendCleanerRepository = new TrendCleaner(_connectionString);
            trendCleanerRepository.DeleteDataFor(trendTableName, dateColumnName, oldDate);
        }
    }
}
