using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using RemoteAnalyst.Repository.Repositories;
using System.Collections.Generic;
using System.Data;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class RAInfoService {
        private readonly string _connectionString = "";

        public RAInfoService(string connectionString) {
            _connectionString = connectionString;
        }

        public string GetQueryValueFor(string key) {
            var raInfo = new RAInfoRepository();
            string retVal = raInfo.GetQueryValue(key);

            return retVal;
        }

        public int GetMaxQueueFor(string key) {
            var raInfo = new RAInfoRepository();
            int maxQueue = raInfo.GetMaxQueue(key);

            return maxQueue;
        }

        public string GetValueFor(string key) {
            var raInfo = new RAInfoRepository();
            var value = raInfo.GetValue(key);
            return value;
        }
    }
}