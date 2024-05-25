using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using RemoteAnalyst.Repository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Repository.Infrastructure
{
    public class Constants
    {
        private static Constants instance;

        public Int32 BulkLoaderSize { get; }

        private Constants(string profileDBConnectionString) {
            var raInfo = new RAInfoRepository();
            try
            {
                BulkLoaderSize = Convert.ToInt32(raInfo.GetValue("BulkLoaderSize"));
            }
            catch {
                BulkLoaderSize = 1000000; 
            }
        }
        
        // public const int BulkLoaderSize = 100000;
        public static Constants getInstance(string connectionString)
        {
            if (instance == null)
            {
                instance = new Constants(connectionString);
            }
            return instance;
        }

    }
}
