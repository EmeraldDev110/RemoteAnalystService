using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon;
using Amazon.RDS;
using Amazon.RDS.Model;
using RemoteAnalyst.AWS.Infrastructure;
using RemoteAnalyst.AWS.S3;

namespace RemoteAnalyst.AWS.RDS {
    public class AmazonRDS : IAmazonRDS {
        public int GetRDSAllocatedStorage(string databaseName) {
            var c = new AmazonRDSClient(Helper.GetRegionEndpoint());
            var request = new DescribeDBInstancesRequest();
            var response = c.DescribeDBInstances(request);
            var storage = response.DBInstances.Where(x => x.DBInstanceIdentifier == databaseName).Select(x => x.AllocatedStorage).FirstOrDefault();

            return storage;
        }

        public List<RdsInfo> GetRDSInstances() {
            var rdsInstances = new List<RdsInfo>();

            var c = new AmazonRDSClient(Helper.GetRegionEndpoint());
            var request = new DescribeDBInstancesRequest();
            var response = c.DescribeDBInstances(request);

            foreach (var instance in response.DBInstances) {
                if (instance.DBInstanceStatus == "available") {
                    rdsInstances.Add(new RdsInfo {
                        RdsName = instance.DBInstanceIdentifier,
                        RdsType = instance.DBInstanceClass,
                        IsAurora = (instance.Engine == "aurora" || instance.Engine == "aurora-mysql")

					});
                }
            }

            return rdsInstances;
        }
    }
}
