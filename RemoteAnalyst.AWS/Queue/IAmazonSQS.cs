using RemoteAnalyst.AWS.Queue.View;

namespace RemoteAnalyst.AWS.Queue
{
    internal interface IAmazonSQS
    {
        string GetAmazonSQSUrl(string queueName);
        void WriteMessage(string queueUrl, string message);
        MessageView ReadMessage(string queueUrl);
        void DeleteMessage(string queueUrl, string messageRecieptHandle);
    }
}