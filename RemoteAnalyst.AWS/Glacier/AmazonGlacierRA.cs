using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Amazon;
using Amazon.Glacier;
using Amazon.Glacier.Model;
using Amazon.Glacier.Transfer;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using log4net;
using Newtonsoft.Json;

namespace RemoteAnalyst.AWS.Glacier {
    public class AmazonGlacierRA : IAmazonGlacier {
        private static readonly ILog Log = LogManager.GetLogger("GlacierProcess");

        private AmazonGlacierClient client;
        private AmazonSimpleNotificationServiceClient snsClient;
        private AmazonSQSClient sqsClient;
        private string topicArn;
		private string queueUrl;
		private string queueArn;
        private const string SQS_POLICY =
            "{" +
            "    \"Version\" : \"2012-10-17\"," +
            "    \"Statement\" : [" +
            "        {" +
            "            \"Sid\" : \"sns-rule\"," +
            "            \"Effect\" : \"Allow\"," +
            "            \"Principal\" : \"*\"," +
            "            \"Action\"    : \"sqs:SendMessage\"," +
            "            \"Resource\"  : \"{QuernArn}\"," +
            "            \"Condition\" : {" +
            "                \"ArnLike\" : {" +
            "                    \"aws:SourceArn\" : \"{TopicArn}\"" +
            "                }" +
            "            }" +
            "        }" +
            "    ]" +
            "}";


        public string UploadToGlacier(string vaultName, string archiveDesc, string filePath) {
            string archiveId = "";
            try {
                var manager = new ArchiveTransferManager(Helper.GetRegionEndpoint());
                archiveId = manager.Upload(vaultName, archiveDesc, filePath).ArchiveId;
            }
            catch (AmazonS3Exception amazonS3Exception) {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                     amazonS3Exception.ErrorCode.Equals("InvalidSecurity"))) {
                    throw new Exception("Invalid credentials");
                }
                throw new Exception("An error occurred with the message " + amazonS3Exception.Message +
                                    " when writing an object");
            }
            catch (AmazonGlacierException e) {
                throw new Exception("Exception Occurred when uploading to Glacier: " + e);
            }
            catch (AmazonServiceException e) {
                throw new Exception("Exception Occurred when uploading to Glacier: " + e);
            }
            catch (Exception e) {
                throw new Exception("Exception Occurred when uploading to Glacier: " + e);
            }
            return archiveId;
        }

        public void DownloadFromGlacier(string vaultName, string archiveId, string filePath) {
            try {
                var manager = new ArchiveTransferManager(Helper.GetRegionEndpoint());
                manager.Download(vaultName, archiveId, filePath);
            }
            catch (AmazonS3Exception amazonS3Exception) {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                     amazonS3Exception.ErrorCode.Equals("InvalidSecurity"))) {
                    throw new Exception("Invalid credentials");
                }
                throw new Exception("An error occurred with the message " + amazonS3Exception.Message +
                                    " when reading an object");
            }
            catch (AmazonGlacierException e) {
                throw new Exception("Exception Occurred when downloading from Glacier: " + e);
            }
            catch (AmazonServiceException e) {
                throw new Exception("Exception Occurred when downloading from Glacier: " + e);
            }
            catch (Exception e) {
                throw new Exception("Exception Occurred when downloading from Glacier: " + e);
            }
        }

        public void DeleteFromGlacier(string vaultName, string archiveId) {
            try {
                var manager = new ArchiveTransferManager(Helper.GetRegionEndpoint());
                manager.DeleteArchive(vaultName, archiveId);
            }
            catch (AmazonS3Exception amazonS3Exception) {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                     amazonS3Exception.ErrorCode.Equals("InvalidSecurity"))) {
                    throw new Exception("Invalid credentials");
                }
                throw new Exception("An error occurred with the message " + amazonS3Exception.Message +
                                    " when reading an object");
            }
            catch (AmazonGlacierException e) {
                throw new Exception("Exception Occurred when deleting from Glacier: " + e);
            }
            catch (AmazonServiceException e) {
                throw new Exception("Exception Occurred when deleting from Glacier: " + e);
            }
            catch (Exception e) {
                throw new Exception("Exception Occurred when deleting from Glacier: " + e);
            }
        }

        public void CreateVault(string vaultName) {
            try {
                var manager = new ArchiveTransferManager(Helper.GetRegionEndpoint());
                manager.CreateVault(vaultName);
            }
            catch (AmazonS3Exception amazonS3Exception) {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                     amazonS3Exception.ErrorCode.Equals("InvalidSecurity"))) {
                    throw new Exception("Invalid credentials");
                }
                throw new Exception("An error occurred with the message " + amazonS3Exception.Message +
                                    " when reading an object");
            }
            catch (AmazonGlacierException e) {
                Log.ErrorFormat("{0}", e);
            }
            catch (AmazonServiceException e) {
                Log.ErrorFormat("{0}", e);
            }
            catch (Exception e) {
                Log.ErrorFormat("{0}", e);
            }
        }

        public void DeleteVault(string vaultName) {
            try {
                var manager = new ArchiveTransferManager(Helper.GetRegionEndpoint());
                manager.DeleteVault(vaultName);
            }
            catch (AmazonS3Exception amazonS3Exception) {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                     amazonS3Exception.ErrorCode.Equals("InvalidSecurity"))) {
                    throw new Exception("Invalid credentials");
                }
                throw new Exception("An error occurred with the message " + amazonS3Exception.Message +
                                    " when reading an object");
            }
            catch (AmazonGlacierException e) {
                Log.ErrorFormat("{0}", e);
            }
            catch (AmazonServiceException e) {
                Log.ErrorFormat("{0}", e);
            }
            catch (Exception e) {
                Log.ErrorFormat("{0}", e);
            }
        }

        public void FastGlacierDownload(string vaultName, string archiveId, string fileName) {

            try {
                using (client = new AmazonGlacierClient(Helper.GetRegionEndpoint())) {
                    SetupTopicAndQueue();
                    RetrieveArchive(client, vaultName, archiveId, fileName);
                }
            }
            catch (AmazonGlacierException e) {
                throw new Exception("Exception Occurred when downloading from Glacier: " + e);
            }
            catch (AmazonServiceException e) {
                throw new Exception("Exception Occurred when downloading from Glacier: " + e);
            }
            catch (Exception e) {
                throw new Exception("Exception Occurred when downloading from Glacier: " + e);
            }
            finally {
                // Delete SNS topic and SQS queue.
                snsClient.DeleteTopic(new DeleteTopicRequest { TopicArn = topicArn });
                sqsClient.DeleteQueue(new DeleteQueueRequest { QueueUrl = queueUrl });
            }
        }

        private void SetupTopicAndQueue() {
            snsClient = new AmazonSimpleNotificationServiceClient(Helper.GetRegionEndpoint());
            sqsClient = new AmazonSQSClient(Helper.GetRegionEndpoint());

            long ticks = DateTime.Now.Ticks;
            topicArn = snsClient.CreateTopic(new CreateTopicRequest { Name = "GlacierDownload-" + ticks }).TopicArn;

            var createQueueRequest = new CreateQueueRequest();
            createQueueRequest.QueueName = "GlacierDownload-" + ticks;
            CreateQueueResponse createQueueResponse = sqsClient.CreateQueue(createQueueRequest);
            queueUrl = createQueueResponse.QueueUrl;

            var getQueueAttributesRequest = new GetQueueAttributesRequest();
            getQueueAttributesRequest.AttributeNames = new List<string> { "QueueArn" };
            getQueueAttributesRequest.QueueUrl = queueUrl;
            GetQueueAttributesResponse response = sqsClient.GetQueueAttributes(getQueueAttributesRequest);
            queueArn = response.QueueARN;

            // Setup the Amazon SNS topic to publish to the SQS queue.
            snsClient.Subscribe(new SubscribeRequest {
                Protocol = "sqs",
                Endpoint = queueArn,
                TopicArn = topicArn
            });

            // Add policy to the queue so SNS can send messages to the queue.
            string policy = SQS_POLICY.Replace("{TopicArn}", topicArn).Replace("{QuernArn}", queueArn);

            sqsClient.SetQueueAttributes(new SetQueueAttributesRequest {
                QueueUrl = queueUrl,
                Attributes = new Dictionary<string, string> {
                    {QueueAttributeName.Policy, policy}
                }
            });
        }

        private void RetrieveArchive(AmazonGlacierClient client, string vaultName, string archiveId, string fileName) {
            // Initiate job.
            var initJobRequest = new InitiateJobRequest {
                VaultName = vaultName,
                JobParameters = new JobParameters {
                    Type = "archive-retrieval",
                    ArchiveId = archiveId,
                    Description = "This job is to download archive.",
                    SNSTopic = topicArn,
                    Tier = "Bulk"
				}
            };
            InitiateJobResponse initJobResponse = client.InitiateJob(initJobRequest);
            string jobId = initJobResponse.JobId;

            // Check queue for a message and if job completed successfully, download archive.
            ProcessQueue(jobId, client, vaultName, fileName);
        }

        private void ProcessQueue(string jobId, AmazonGlacierClient client, string vaultName, string fileName) {
            var receiveMessageRequest = new ReceiveMessageRequest { QueueUrl = queueUrl, MaxNumberOfMessages = 1 };
            bool jobDone = false;
            while (!jobDone) {
                Log.Info("Poll SQS queue");
                ReceiveMessageResponse receiveMessageResponse = sqsClient.ReceiveMessage(receiveMessageRequest);
                if (receiveMessageResponse.Messages.Count == 0) {
                    Thread.Sleep(5000);
                    continue;
                }
                Log.Info("Got message");
                Message message = receiveMessageResponse.Messages[0];
                var outerLayer = JsonConvert.DeserializeObject<Dictionary<string, string>>(message.Body);
                var fields = JsonConvert.DeserializeObject<Dictionary<string, object>>(outerLayer["Message"]);
                var statusCode = fields["StatusCode"] as string;

                if (string.Equals(statusCode, GlacierUtils.JOB_STATUS_SUCCEEDED, StringComparison.InvariantCultureIgnoreCase)) {
                    DownloadOutput(jobId, client, vaultName, fileName); // Save job output to the specified file location.
                }
                else if (string.Equals(statusCode, GlacierUtils.JOB_STATUS_FAILED, StringComparison.InvariantCultureIgnoreCase)) { 
                    Log.Info("Job failed... cannot download the archive.");
                }

                jobDone = true;
                sqsClient.DeleteMessage(new DeleteMessageRequest { QueueUrl = queueUrl, ReceiptHandle = message.ReceiptHandle });
            }
        }

        private void DownloadOutput(string jobId, AmazonGlacierClient client, string vaultName, string fileName) {
            var getJobOutputRequest = new GetJobOutputRequest {
                JobId = jobId,
                VaultName = vaultName
            };

            GetJobOutputResponse getJobOutputResponse = client.GetJobOutput(getJobOutputRequest);
            using (Stream webStream = getJobOutputResponse.Body) {
                using (Stream fileToSave = File.OpenWrite(fileName)) {
                    CopyStream(webStream, fileToSave);
                }
            }
        }

        public void CopyStream(Stream input, Stream output) {
            var buffer = new byte[65536];
            int length;
            while ((length = input.Read(buffer, 0, buffer.Length)) > 0) {
                output.Write(buffer, 0, length);
            }
        }
    }
}