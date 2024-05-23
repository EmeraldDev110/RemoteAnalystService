using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using RemoteAnalyst.AWS.Infrastructure;

namespace RemoteAnalyst.AWS.EC2
{
    /// <summary>
    /// Calls Amazon EC2 SDK
    /// </summary>
    public class AmazonEC2 : IAmazonEC2
    {
        //private readonly Amazon.EC2.AmazonEC2 _ec2 = AWSClientFactory.CreateAmazonEC2Client(Helper.GetRegionEndpoint());
        private readonly Amazon.EC2.AmazonEC2Client _ec2 = new AmazonEC2Client(Helper.GetRegionEndpoint());

        /// <summary>
        /// Terminsate EC2 Instances
        /// </summary>
        /// <param name="instanceIDs">List of EC2 Instance IDs</param>
        public void TerminateEC2Instance(List<string> instanceIDs)
        {
            var termRequest = new TerminateInstancesRequest();
            termRequest.InstanceIds = instanceIDs;

            _ec2.TerminateInstances(termRequest);
        }

        public DateTime GetLaunchTime(string instanceId)
        {
            DateTime launchTime;    
            try {
                /*launchTime = Convert.ToDateTime(_ec2.DescribeInstances(
                                new DescribeInstancesRequest {InstanceIds = new List<string> {instanceId}}).
                                DescribeInstancesResult.Reservation.First().
                                RunningInstance.First().LaunchTime);*/
                launchTime = Convert.ToDateTime(_ec2.DescribeInstances(
                    new DescribeInstancesRequest {InstanceIds = new List<string> {instanceId}}).Reservations.First().Instances.First().LaunchTime);

            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }

            return launchTime;
        }

        /// <summary>
        /// Get Current EC2 Instance ID
        /// </summary>
        /// <returns></returns>
        public string GetEC2ID() {
            string instanceId = "";
            try {
#if (DEBUG)
                instanceId = "i-05308e751f305c299";
#else
                instanceId = new StreamReader(HttpWebRequest.Create
                                        ("http://169.254.169.254/latest/meta-data/instance-id")
                                        .GetResponse().GetResponseStream())
                                        .ReadToEnd();

#endif
            }
            catch { }
            return instanceId;
        }

		public void UpdateEC2tagByInstanceId(string instanceId, string key, string value) {
			var ec2 = new AmazonEC2Client(Helper.GetRegionEndpoint());
			var response = ec2.CreateTags(new CreateTagsRequest {
				Resources = new List<string> {
							instanceId
				},
				Tags = new List<Tag> {
					new Tag {
						Key = key,
						Value = value
					}
				}
			});
		}

        public List<EC2Information> GetLoaderEc2Information() {
            var ec2List = new List<EC2Information>();
            var ec2 = new AmazonEC2Client(Helper.GetRegionEndpoint());
            var req = new DescribeInstancesRequest();
            var result = ec2.DescribeInstances(req).Reservations;

			foreach (var reservation in result) {
                var instances = reservation.Instances;
                foreach (var instance in instances) {
                    if (instance.Tags.Any(x => x.Key.ToUpper() == "TYPE" && x.Value.ToUpper() == "LOADER") && instance.State.Name == "running") {
                        var ec2Info = new EC2Information {
                            InstanceName = instance.Tags.Where(x => x.Key == "Name").Select(x => x.Value).FirstOrDefault(),
                            InstanceId = instance.InstanceId,
                            InstanceType = instance.InstanceType
                        };
                        ec2List.Add(ec2Info);
                    }
                }
            }

            return ec2List;
        }


        //This function retrieves the EC2 Instances
        public static List<Instance> GetEC2Instances()
        {
            //int dynamic_ec2_run_limit_in_hours = 24;

            var ec2Client = new AmazonEC2Client(Helper.GetRegionEndpoint());
            List<Instance> instances = new List<Instance>();
            var request = new DescribeInstancesRequest();
            var response = ec2Client.DescribeInstances(request);
            foreach (var ec2 in response.Reservations)
            {
                foreach (Instance i in ec2.Instances)
                {
                    instances.Add(i);
                }
            }
            return instances;
        }
    }
}
