using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class VisaTrendLoaderService {
        private readonly string _connectionString = "";

        public VisaTrendLoaderService(string connectionString) {
            _connectionString = connectionString;
        }

        public void InsertEntry(string systemSerial, DateTime dataDate) {
            var visaTrendLoader = new VisaTrendLoader(_connectionString);
            visaTrendLoader.InsertEntry(systemSerial, dataDate);
        }

        public bool CheckEntry(string systemSerial, DateTime dataDate) {
            var visaTrendLoader = new VisaTrendLoader(_connectionString);
            bool exists = visaTrendLoader.CheckEntry(systemSerial, dataDate);

            return exists;
        }
    }
}
