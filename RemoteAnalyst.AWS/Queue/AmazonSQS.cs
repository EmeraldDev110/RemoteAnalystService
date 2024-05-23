using System;
using System.Collections.Generic;
using System.Linq;
using Amazon;
using Amazon.Runtime.Internal;
using Amazon.SQS;
using Amazon.SQS.Model;
using RemoteAnalyst.AWS.Queue.View;

namespace RemoteAnalyst.AWS.Queue {
    /// <summary>
    /// Amazon SQS
    /// </summary>
    public class AmazonSQS : IAmazonSQS {
        //private readonly Amazon.SQS.AmazonSQS _sqs = AWSClientFactory.CreateAmazonSQSClient(Helper.GetRegionEndpoint());
        private readonly Amazon.SQS.AmazonSQSClient _sqs = new AmazonSQSClient(Helper.GetRegionEndpoint());
        /// <summary>
        /// Gets Amazon SQS URL
        /// </summary>
        /// <param name="queueName">SQS Queue Name</param>
        /// <returns>Queue URL</returns>
        public string GetAmazonSQSUrl(string queueName) {
            string url = "";
            try {
                var listQueuesRequest = new ListQueuesRequest();
                ListQueuesResponse listQueuesResponse = _sqs.ListQueues(listQueuesRequest);

                /*if (listQueuesResponse.IsSetListQueuesResult()) {
                    ListQueuesResult listQueuesResult = listQueuesResponse.ListQueuesResult;
                    foreach (String queueUrl in listQueuesResult.QueueUrl) {
                        if (queueUrl.Contains(queueName)) {
                            url = queueUrl;
                            break;
                        }
                    }
                }*/

                foreach (var queueUrl in listQueuesResponse.QueueUrls) {
                    if (queueUrl.Contains(queueName)) {
                        url = queueUrl;
                        break;
                    }
                }
            }
            catch (AmazonSQSException amazonSQSException) {
                if (amazonSQSException.ErrorCode != null &&
                    (amazonSQSException.ErrorCode.Equals("InvalidAccessKeyId") ||
                     amazonSQSException.ErrorCode.Equals("InvalidSecurity"))) {
                    throw new Exception("Please check the provided AWS Credentials.");
                }
                throw new Exception("An error occurred with the message " + amazonSQSException.Message +
                                    " when Getting a queue");
            }
            return url;
        }

        /// <summary>
        /// Write Message to SQS
        /// </summary>
        /// <param name="queueUrl">SQS URL</param>
        /// <param name="message">Message</param>
        public void WriteMessage(string queueUrl, string message) {
            try {
                var sendMessageRequest = new SendMessageRequest {
                    QueueUrl = queueUrl,
                    MessageBody = message
                };
                _sqs.SendMessage(sendMessageRequest);
            }
            catch (AmazonSQSException amazonSQSException) {
                if (amazonSQSException.Message.Contains("MessageGroupId")) {
                    WriteMessageWithGroupName(queueUrl, message, "A");
                }
                else {
                    if (amazonSQSException.ErrorCode != null &&
                        (amazonSQSException.ErrorCode.Equals("InvalidAccessKeyId") ||
                         amazonSQSException.ErrorCode.Equals("InvalidSecurity"))) {
                        throw new Exception("Please check the provided AWS Credentials.");
                    }
                    throw new Exception("An error occurred with the message " + amazonSQSException.Message +
                                        " when Writing a queue");
                }
            }
        }

        public string WriteMessageWithGroupName(string queueUrl, string message, string groupId) {
            var messageId = "";
            try {
                var sendMessageRequest = new SendMessageRequest {
                    QueueUrl = queueUrl,
                    MessageBody = message
                };
                sendMessageRequest.MessageGroupId = groupId;
                sendMessageRequest.MessageDeduplicationId = DateTime.Now.Ticks.ToString();

                var test = _sqs.SendMessage(sendMessageRequest);
                messageId = test.MessageId;
            }
            catch (AmazonSQSException amazonSQSException) {
                if (amazonSQSException.ErrorCode != null &&
                    (amazonSQSException.ErrorCode.Equals("InvalidAccessKeyId") ||
                     amazonSQSException.ErrorCode.Equals("InvalidSecurity"))) {
                    throw new Exception("Please check the provided AWS Credentials.");
                }
                throw new Exception("An error occurred with the message " + amazonSQSException.Message +
                                    " when Writing a queue");
            }

            return messageId;
        }

        /// <summary>
        /// Read SQS Message
        /// </summary>
        /// <param name="queueUrl">SQS URL</param>
        /// <returns>MessageView</returns>
        public MessageView ReadMessage(string queueUrl) {
            var view = new MessageView();
            try {
                //Receiving a message
                var receiveMessageRequest = new ReceiveMessageRequest();
                receiveMessageRequest.QueueUrl = queueUrl;
                receiveMessageRequest.VisibilityTimeout = 10;

                ReceiveMessageResponse receiveMessageResponse = _sqs.ReceiveMessage(receiveMessageRequest);
                /*if (receiveMessageResponse.IsSetReceiveMessageResult()) {
                    ReceiveMessageResult receiveMessageResult = receiveMessageResponse.ReceiveMessageResult;
                    //Read first Message.
                    if (receiveMessageResult.Message.Count > 0) {
                        view.ReceiptHandle = receiveMessageResponse.ReceiveMessageResult.Message[0].ReceiptHandle;
                        view.Body = receiveMessageResponse.ReceiveMessageResult.Message[0].Body;
                    }
                }*/
                if (receiveMessageResponse.Messages.Count > 0) {
                    //Read first Message.
                    view.ReceiptHandle = receiveMessageResponse.Messages.FirstOrDefault().ReceiptHandle;
                    view.Body = receiveMessageResponse.Messages.FirstOrDefault().Body;
                }
            }
            catch (AmazonSQSException amazonSQSException) {
                if (amazonSQSException.ErrorCode != null &&
                    (amazonSQSException.ErrorCode.Equals("InvalidAccessKeyId") ||
                     amazonSQSException.ErrorCode.Equals("InvalidSecurity"))) {
                    throw new Exception("Please check the provided AWS Credentials.");
                }
                throw new Exception("An error occurred with the message " + amazonSQSException.Message +
                                    " when Reading a queue");
            }
            return view;
        }

		public List<MessageView> ReadNoMoreThanTenMessages(string queueUrl) {
			List<MessageView> messageViews = new List<MessageView>();
			try {
				//Receiving a message
				var receiveMessageRequest = new ReceiveMessageRequest();
				receiveMessageRequest.QueueUrl = queueUrl;
				receiveMessageRequest.VisibilityTimeout = 10;
				receiveMessageRequest.MaxNumberOfMessages = 10;
				ReceiveMessageResponse receiveMessageResponse = _sqs.ReceiveMessage(receiveMessageRequest);
				
				
				for (int i = 0; i < receiveMessageResponse.Messages.Count; i++) {
					var view = new MessageView {
						ReceiptHandle = receiveMessageResponse.Messages[i].ReceiptHandle,
						Body = receiveMessageResponse.Messages[i].Body
					};
					messageViews.Add(view);
				}
			
			}
			catch (AmazonSQSException amazonSQSException) {
				if (amazonSQSException.ErrorCode != null &&
					(amazonSQSException.ErrorCode.Equals("InvalidAccessKeyId") ||
					 amazonSQSException.ErrorCode.Equals("InvalidSecurity"))) {
					throw new Exception("Please check the provided AWS Credentials.");
				}
				throw new Exception("An error occurred with the message " + amazonSQSException.Message +
									" when Reading a queue");
			}
			return messageViews;
		}

		public MessageView ReadAllMessage(string queueUrl) {
            var view = new MessageView();
            try {
                //Receiving a message
                var receiveMessageRequest = new ReceiveMessageRequest();
                receiveMessageRequest.QueueUrl = queueUrl;
                receiveMessageRequest.VisibilityTimeout = 10;

                ReceiveMessageResponse receiveMessageResponse = _sqs.ReceiveMessage(receiveMessageRequest);
                /*if (receiveMessageResponse.IsSetReceiveMessageResult()) {
                    ReceiveMessageResult receiveMessageResult = receiveMessageResponse.ReceiveMessageResult;
                    //Read first Message.
                    if (receiveMessageResult.Message.Count > 0) {
                        view.ReceiptHandle = receiveMessageResponse.ReceiveMessageResult.Message[0].ReceiptHandle;
                        view.Body = receiveMessageResponse.ReceiveMessageResult.Message[0].Body;
                    }
                }*/
                if (receiveMessageResponse.Messages.Count > 0) {
                    //Read first Message.
                    view.ReceiptHandle = receiveMessageResponse.Messages.FirstOrDefault().ReceiptHandle;
                    view.Body = receiveMessageResponse.Messages.FirstOrDefault().Body;
                }
            }
            catch (AmazonSQSException amazonSQSException) {
                if (amazonSQSException.ErrorCode != null &&
                    (amazonSQSException.ErrorCode.Equals("InvalidAccessKeyId") ||
                     amazonSQSException.ErrorCode.Equals("InvalidSecurity"))) {
                    throw new Exception("Please check the provided AWS Credentials.");
                }
                throw new Exception("An error occurred with the message " + amazonSQSException.Message +
                                    " when Reading a queue");
            }
            return view;
        }

        /// <summary>
        /// Delete SQS Message
        /// </summary>
        /// <param name="queueUrl">SQS URL</param>
        /// <param name="messageRecieptHandle">Message Handler</param>
        public void DeleteMessage(string queueUrl, string messageRecieptHandle) {
            try {
                var deleteRequest = new DeleteMessageRequest();
                deleteRequest.QueueUrl = queueUrl;
                deleteRequest.ReceiptHandle = messageRecieptHandle;
                _sqs.DeleteMessage(deleteRequest);
            }
            catch (AmazonSQSException amazonSQSException) {
                if (amazonSQSException.ErrorCode != null &&
                    (amazonSQSException.ErrorCode.Equals("InvalidAccessKeyId") ||
                     amazonSQSException.ErrorCode.Equals("InvalidSecurity"))) {
                    throw new Exception("Please check the provided AWS Credentials.");
                }
                throw new Exception("An error occurred with the message " + amazonSQSException.Message +
                                    " when Deleting a queue");
            }
        }
    }
}