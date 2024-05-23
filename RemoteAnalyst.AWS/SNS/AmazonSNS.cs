using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace RemoteAnalyst.AWS.SNS {
    public class AmazonSNS : IAmazonSNS {
        //private static readonly AmazonSimpleNotificationService snsClient = AWSClientFactory.CreateAmazonSNSClient(Helper.GetRegionEndpoint());
        private static readonly AmazonSimpleNotificationServiceClient snsClient = new AmazonSimpleNotificationServiceClient(Helper.GetRegionEndpoint());

        public void SendToTopic(string subject, string message, string topicARN) {
            var pubRequest = new PublishRequest {
                TopicArn = topicARN,
                Subject = subject,
                Message = message
            };
            snsClient.Publish(pubRequest);
        }
    }
}