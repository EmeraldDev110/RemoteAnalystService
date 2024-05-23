namespace RemoteAnalyst.FTPRelay.BLL {
    internal interface IUploadFile {
        string Upload(string ftpServer, string localFileName, string remoteFileName, string systemDirectory);
    }
}