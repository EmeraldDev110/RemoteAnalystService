namespace RemoteAnalyst.AWS.S3
{
    internal interface IAmazonS3
    {
        void WriteToS3(string fileName, string fileBody);
        void WriteToS3WithLocaFile(string fileName, string fileLocation);

        void CreateSubFolder(string folder);

        string ReadS3(string keyName, string saveLocation);
        void DeleteS3(string keyName);

        void WriteToS3MultiThreads(string fileName, string fullFileLocation);
    }
}