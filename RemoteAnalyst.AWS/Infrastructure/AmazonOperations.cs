using System.Configuration;
using RemoteAnalyst.AWS.Queue;
using RemoteAnalyst.AWS.S3;

namespace RemoteAnalyst.AWS.Infrastructure
{
    public class AmazonOperations
    {
        public void WriteErrorQueue(string savePath)
        {
            string errorQName = ConfigurationManager.AppSettings["SQSError"];
            var sqs = new AmazonSQS();
            string queueUrl = sqs.GetAmazonSQSUrl(errorQName);

            if (queueUrl.Length > 0)
            {
                sqs.WriteMessage(queueUrl, savePath);
            }
        }

        public void WriteToS3(string fileName, string fileLocation) {
            string s3ErrorLog = ConfigurationManager.AppSettings["S3ErrorLog"];
            var s3 = new AmazonS3(s3ErrorLog);
            s3.WriteToS3WithLocaFile(fileName, fileLocation);
        }
    }
}