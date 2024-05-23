namespace RemoteAnalyst.AWS.SNS {
    public interface IAmazonSNS {
        void SendToTopic(string subject, string message, string topicARN);
    }
}