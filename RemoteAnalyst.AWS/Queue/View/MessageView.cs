namespace RemoteAnalyst.AWS.Queue.View
{
    public class MessageView
    {
        public string ReceiptHandle { get; set; }
        public string Body { get; set; }
        public string QueueURL { get; set; }
    }
}