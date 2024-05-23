using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;

namespace RemoteAnalyst.AWS.CloudWatch {
    public class AmazonCloudWatch : IAmazonCloudWatch {
        public double GetRDSCpuBusy(string databaseName) {
            var avgCpuBusy = 0d;
            try {
                var client = new AmazonCloudWatchClient(Helper.GetRegionEndpoint());

                var dimension = new Dimension {
                    Name = "DBInstanceIdentifier",
                    Value = databaseName
                };

                var request = new GetMetricStatisticsRequest {
                    Dimensions = new List<Dimension> {dimension},
                    MetricName = "CPUUtilization",
                    Namespace = "AWS/RDS",
                    // Get statistics by day.   
                    Period = 60,
                    // Get statistics for the past 15 mins.
                    StartTime = DateTime.UtcNow - TimeSpan.FromMinutes(2),
                    EndTime = DateTime.UtcNow,
                    Statistics = new List<string>() {"Maximum"},
                    Unit = StandardUnit.Percent
                };

                var response = client.GetMetricStatistics(request);

                if (response.Datapoints.Count > 0) {
                    foreach (var point in response.Datapoints) {
                        avgCpuBusy += point.Maximum;
                    }
                    avgCpuBusy /= Convert.ToDouble(response.Datapoints.Count);
                }
            }
            catch {
                avgCpuBusy = 100; //When we can't get the cpu busy, return 100 so that loader won't use this RDS.
            }
            return avgCpuBusy;
        }

        public double GetEC2CpuBusy(string instanceId) {
            var client = new AmazonCloudWatchClient(Helper.GetRegionEndpoint());

            var dimension = new Dimension {
                Name = "InstanceId",
                Value = instanceId
            };
            // https://docs.aws.amazon.com/AmazonCloudWatch/latest/APIReference/API_GetMetricStatistics.html
            var request = new GetMetricStatisticsRequest {
                Dimensions = new List<Dimension> { dimension },
                MetricName = "CPUUtilization",
                Namespace = "AWS/EC2",
                Period = 1800,
                // Get statistics for the past 15 mins.
                StartTime = DateTime.UtcNow - TimeSpan.FromMinutes(15),
                EndTime = DateTime.UtcNow,
                Statistics = new List<string>() { "Maximum" },
                Unit = StandardUnit.Percent
            };

            var response = client.GetMetricStatistics(request);

            var avgCpuBusy = 0d;
            if (response.Datapoints.Count > 0) {
                foreach (var point in response.Datapoints) {
                    avgCpuBusy += point.Maximum;
                }
                avgCpuBusy /= Convert.ToDouble(response.Datapoints.Count);
            }

            return avgCpuBusy;
        }

        public double GetEC2CPUCreditBalance(string instanceId)
        {
            var client = new AmazonCloudWatchClient(Helper.GetRegionEndpoint());

            var dimension = new Dimension
            {
                Name = "InstanceId",
                Value = instanceId
            };
            // https://docs.aws.amazon.com/AmazonCloudWatch/latest/APIReference/API_GetMetricStatistics.html
            var request = new GetMetricStatisticsRequest
            {
                Dimensions = new List<Dimension> { dimension },
                MetricName = "CPUCreditBalance",
                Namespace = "AWS/EC2",
                Period = 1800,
                // Get statistics for the past 15 mins.
                StartTime = DateTime.UtcNow - TimeSpan.FromMinutes(15),
                EndTime = DateTime.UtcNow,
                Statistics = new List<string>() { "Minimum" },
                Unit = StandardUnit.Count
            };

            var response = client.GetMetricStatistics(request);

            var avgCPUCreditBalance = 0d;
            if (response.Datapoints.Count > 0)
            {
                foreach (var point in response.Datapoints)
                {
                    avgCPUCreditBalance += point.Minimum;
                }
                avgCPUCreditBalance /= Convert.ToDouble(response.Datapoints.Count);
            }
            return avgCPUCreditBalance;
        }

        public double GetRDSFreeSpace(string databaseName) {
            var client = new AmazonCloudWatchClient(Helper.GetRegionEndpoint());

            var dimension = new Dimension {
                Name = "DBInstanceIdentifier",
                Value = databaseName
            };

            var request = new GetMetricStatisticsRequest {
                Dimensions = new List<Dimension> { dimension },
                MetricName = "FreeStorageSpace",
                Namespace = "AWS/RDS",
                // Get statistics by day.   
                Period = 1800,
                // Get statistics for the past 15 mins.
                StartTime = DateTime.UtcNow - TimeSpan.FromMinutes(15),
                EndTime = DateTime.UtcNow,
                Statistics = new List<string>() { "Average" },
                Unit = StandardUnit.Bytes
            };

            var response = client.GetMetricStatistics(request);

            var totalFreeSpace = 0d;
            if (response.Datapoints.Count > 0) {
                foreach (var point in response.Datapoints) {
                    totalFreeSpace += point.Average;
                }
            }

            return totalFreeSpace / 1024 / 1024 / 1024;
        }

        public double GetAuroraFreeSpace(string databaseName) {
            var client = new AmazonCloudWatchClient(Helper.GetRegionEndpoint());

            var dimension = new Dimension {
                Name = "DbClusterIdentifier",
                Value = databaseName + "-cluster"
            };

            var request = new GetMetricStatisticsRequest {
                Dimensions = new List<Dimension> { dimension },
                MetricName = "VolumeBytesUsed",
                Namespace = "AWS/RDS",
                // Get statistics by day.   
                Period = 1800,
                // Get statistics for the past 15 mins.
                StartTime = DateTime.UtcNow - TimeSpan.FromMinutes(30),
                EndTime = DateTime.UtcNow,
                Statistics = new List<string>() { "Average" },
                Unit = StandardUnit.Bytes
            };

            var response = client.GetMetricStatistics(request);

            var totalFreeSpace = 0d;
            if (response.Datapoints.Count > 0) {
                foreach (var point in response.Datapoints) {
                    totalFreeSpace += point.Average;
                }
            }

            return totalFreeSpace / 1024 / 1024 / 1024;
        }

		public double GetAuroraUsedSpace(string databaseName) {
			var client = new AmazonCloudWatchClient(Helper.GetRegionEndpoint());

			var idDimension = new Dimension {
				Name = "DbClusterIdentifier",
				Value = databaseName + "-cluster"
			};

			var engineDimension = new Dimension {
				Name = "EngineName",
				Value = "aurora"
			};

			var request = new GetMetricStatisticsRequest {
				Dimensions = new List<Dimension> { idDimension, engineDimension },
				MetricName = "VolumeBytesUsed",
				Namespace = "AWS/RDS",
				Period = 1800,
				// Get statistics for the past 15 mins starting from an hour ago.
				StartTime = DateTime.UtcNow - TimeSpan.FromHours(1) - TimeSpan.FromMinutes(15),
				EndTime = DateTime.UtcNow - TimeSpan.FromHours(1),
				Statistics = new List<string>() { "Average" },
				Unit = StandardUnit.Bytes
			};

			var response = client.GetMetricStatistics(request);

			double totalUsedSpace = 0d;
			if (response.Datapoints.Count > 0) {
				foreach (var point in response.Datapoints) {
					totalUsedSpace = point.Average;
				}
			}

			return totalUsedSpace / 1024 / 1024 / 1024;
		}


		public double GetRDSCpuBusyAverage(string databaseName) {
            var client = new AmazonCloudWatchClient(Helper.GetRegionEndpoint());

            var dimension = new Dimension {
                Name = "DBInstanceIdentifier",
                Value = databaseName
            };

            //Get totalMinutes from Midnight.
            var totalMinutesToday = DateTime.Now.TimeOfDay.TotalMinutes;

            var request = new GetMetricStatisticsRequest {
                Dimensions = new List<Dimension> { dimension },
                MetricName = "CPUUtilization",
                Namespace = "AWS/RDS",
                // Get statistics by day.   
                Period = 1800,
                // Get statistics for the past 15 mins.
                StartTime = DateTime.UtcNow - TimeSpan.FromMinutes(totalMinutesToday),
                EndTime = DateTime.UtcNow,
                Statistics = new List<string>() { "Average" },
                Unit = StandardUnit.Percent
            };

            var response = client.GetMetricStatistics(request);

            var avgCpuBusy = 0d;
            if (response.Datapoints.Count > 0) {
                foreach (var point in response.Datapoints) {
                    avgCpuBusy += point.Average;
                }
                avgCpuBusy /= response.Datapoints.Count;
            }

            return avgCpuBusy;
        }

        public double GetRDSCpuBusyPeak(string databaseName) {
            var client = new AmazonCloudWatchClient(Helper.GetRegionEndpoint());

            var dimension = new Dimension {
                Name = "DBInstanceIdentifier",
                Value = databaseName
            };

            //Get totalMinutes from Midnight.
            var totalMinutesToday = DateTime.Now.TimeOfDay.TotalMinutes;

            var request = new GetMetricStatisticsRequest {
                Dimensions = new List<Dimension> { dimension },
                MetricName = "CPUUtilization",
                Namespace = "AWS/RDS",
                // Get statistics by day.   
                Period = 1800,
                // Get statistics for the past 15 mins.
                StartTime = DateTime.UtcNow - TimeSpan.FromMinutes(totalMinutesToday),
                EndTime = DateTime.UtcNow,
                Statistics = new List<string>() { "Maximum" },
                Unit = StandardUnit.Percent
            };

            var response = client.GetMetricStatistics(request);

            var avgCpuBusy = 0d;
            if (response.Datapoints.Count > 0) {
                foreach (var point in response.Datapoints) {
                    if (point.Maximum > avgCpuBusy)
                        avgCpuBusy = point.Maximum;
                }
            }

            return avgCpuBusy;
        }


        public double GetEC2CpuBusyAverage(string ec2InstanceId) {
            var client = new AmazonCloudWatchClient(Helper.GetRegionEndpoint());

            var dimension = new Dimension {
                Name = "InstanceId",
                Value = ec2InstanceId
            };

            //Get totalMinutes from Midnight.
            var totalMinutesToday = DateTime.Now.TimeOfDay.TotalMinutes;

            var request = new GetMetricStatisticsRequest {
                Dimensions = new List<Dimension> { dimension },
                MetricName = "CPUUtilization",
                Namespace = "AWS/EC2",
                // Get statistics by day.   
                Period = 1800,
                // Get statistics for the past 15 mins.
                StartTime = DateTime.UtcNow - TimeSpan.FromMinutes(totalMinutesToday),
                EndTime = DateTime.UtcNow,
                Statistics = new List<string>() { "Average" },
                Unit = StandardUnit.Percent
            };

            var response = client.GetMetricStatistics(request);

            var avgCpuBusy = 0d;
            if (response.Datapoints.Count > 0) {
                foreach (var point in response.Datapoints) {
                    avgCpuBusy += point.Average;
                }
                avgCpuBusy /= response.Datapoints.Count;
            }

            return avgCpuBusy;
        }

        public double GetEC2CpuBusyPeak(string ec2InstanceId) {
            var client = new AmazonCloudWatchClient(Helper.GetRegionEndpoint());

            var dimension = new Dimension {
                Name = "InstanceId",
                Value = ec2InstanceId
            };

            //Get totalMinutes from Midnight.
            var totalMinutesToday = DateTime.Now.TimeOfDay.TotalMinutes;

            var request = new GetMetricStatisticsRequest {
                Dimensions = new List<Dimension> { dimension },
                MetricName = "CPUUtilization",
                Namespace = "AWS/EC2",
                // Get statistics by day.   
                Period = 1800,
                // Get statistics for the past 15 mins.
                StartTime = DateTime.UtcNow - TimeSpan.FromMinutes(totalMinutesToday),
                EndTime = DateTime.UtcNow,
                Statistics = new List<string>() { "Maximum" },
                Unit = StandardUnit.Percent
            };

            var response = client.GetMetricStatistics(request);

            var avgCpuBusy = 0d;
            if (response.Datapoints.Count > 0) {
                foreach (var point in response.Datapoints) {
                    if (point.Maximum > avgCpuBusy)
                        avgCpuBusy = point.Maximum;
                }
            }

            return avgCpuBusy;
        }
    }
}
