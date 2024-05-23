namespace RemoteAnalyst.AWS.Glacier {
    public interface IAmazonGlacier {
        string UploadToGlacier(string vaultName, string archiveDesc, string filePath);
        void DownloadFromGlacier(string vaultName, string archiveId, string filePath);
        void DeleteFromGlacier(string vaultName, string archiveId);
        void CreateVault(string folder);
        void DeleteVault(string folder);
        void FastGlacierDownload(string vaultName, string archiveId, string fileName);
    }
}