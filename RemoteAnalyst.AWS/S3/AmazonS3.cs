using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Amazon;
using Amazon.S3;
using Amazon.S3.IO;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace RemoteAnalyst.AWS.S3
{
    /// <summary>
    /// Amazon S3
    /// </summary>
    public class AmazonS3 : IAmazonS3
    {

        private readonly string _bucketName = "";

        //private readonly Amazon.S3.AmazonS3 _client = AWSClientFactory.CreateAmazonS3Client(Helper.GetRegionEndpoint());
        private readonly Amazon.S3.AmazonS3Client _client = new AmazonS3Client(Helper.GetRegionEndpoint());

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bucketName">S3 Bucket Name</param>
        public AmazonS3(string bucketName)
        {
            _bucketName = bucketName;
        }

        /// <summary>
        /// Write message to S3
        /// </summary>
        /// <param name="fileName">Key Name</param>
        /// <param name="fileBody">Message</param>
        public void WriteToS3(string fileName, string fileBody)
        {
            try
            {
                var request = new PutObjectRequest {
                    ContentBody = fileBody, 
                    BucketName = _bucketName, 
                    Key = fileName, 
                    StorageClass = S3StorageClass.ReducedRedundancy
                };
                // Put object
                _client.PutObject(request);

            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                     amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    throw new Exception("An error occurred with the message " + amazonS3Exception.Message +
                                        " when writing an object. Please check the provided AWS Credentials.");
                }
                else
                {
                    throw new Exception("An error occurred with the message " + amazonS3Exception.Message +
                                        " when writing an object");
                }
            }
        }

        public void CreateSubFolder(string folder) {
            try
            {
                var request = new PutObjectRequest {
                    BucketName = _bucketName,
                    Key = folder,
                    InputStream = new MemoryStream()
                };
                // Put object
                _client.PutObject(request);
            }
            catch (AmazonS3Exception amazonS3Exception) {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                     amazonS3Exception.ErrorCode.Equals("InvalidSecurity"))) {
                    throw new Exception("An error occurred with the message " + amazonS3Exception.Message +
                    " when writing an object. Please check the provided AWS Credentials.");
                }
                else {
                    throw new Exception("An error occurred with the message " + amazonS3Exception.Message +
                                        " when writing an object");

                }
            }
        }


        /// <summary>
        /// Put file to S3
        /// </summary>
        /// <param name="fileName">Key Name</param>
        /// <param name="fullFileLocation">Location File Location</param>
        public void WriteToS3WithLocaFile(string fileName, string fullFileLocation)
        {
            try {
                var request = new PutObjectRequest();
                request.BucketName = _bucketName;
                request.Key = fileName;
                request.FilePath = fullFileLocation;
                request.StorageClass = S3StorageClass.ReducedRedundancy;
                //Set the time out value to 10 minutes.
                //request.Timeout = 600000;
                //request.Timeout = new TimeSpan(0, 0, 10);

                // Put object
                _client.PutObject(request);
            }
            catch (AmazonS3Exception amazonS3Exception) {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                     amazonS3Exception.ErrorCode.Equals("InvalidSecurity"))) {
                    System.Diagnostics.EventLog.WriteEntry("UWSLoader amazon exception", amazonS3Exception.Message + " when writing an object" + amazonS3Exception.StackTrace);

                }
                else {
                    System.Diagnostics.EventLog.WriteEntry("UWSLoader amazon exception", amazonS3Exception.Message + " when writing an object" + amazonS3Exception.StackTrace);
                    throw new Exception("An error occurred with the message " + amazonS3Exception.Message +
                                        " when writing an object"+ amazonS3Exception.StackTrace);
                }
            }
            catch (Exception ex) {
                System.Diagnostics.EventLog.WriteEntry("UWSLoader ", ex.Message + " when writing an object" + ex.StackTrace);
                throw new Exception("An error occurred with the message " + ex.Message + " when writing an object" + ex.StackTrace);
            }
        }

        public void WriteToS3MultiThreads(string fileName, string fullFileLocation) {
            /*var request = new TransferUtilityUploadRequest();
            var transferUtility = new TransferUtility(_client);
            try
            {
                AsyncCallback callback = new AsyncCallback(uploadComplete);
                request.FilePath = fullFileLocation;
                request.BucketName = _bucketName;
                request.Key = fileName;
                //request.Timeout = 3600000;
                request.StorageClass = S3StorageClass.ReducedRedundancy;
                IAsyncResult ar = transferUtility.BeginUpload(request, callback, null);
                transferUtility.EndUpload(ar);
            }
            catch (AmazonS3Exception amazonS3Exception) {
                throw new Exception("An error occurred with the message " + amazonS3Exception.Message +
                                    " when reading an object");
            }*/
            try {
                var transferUtility = new TransferUtility(Helper.GetRegionEndpoint());
                //transferUtility.S3Client.PutBucket(new PutBucketRequest { BucketName = _bucketName });

                var request = new TransferUtilityUploadRequest {
                    BucketName = _bucketName,
                    FilePath = fullFileLocation,
                    Key = fileName,
                    //Timeout = TimeSpan.FromMinutes(60),
                    StorageClass = S3StorageClass.ReducedRedundancy
            };

                transferUtility.Upload(request);
            }
            catch (AmazonS3Exception amazonS3Exception) {
                throw new Exception("An error occurred with the message " + amazonS3Exception.Message +
                                    " when reading an object");
            }
            catch (Exception ex) {
                throw new Exception("An error occurred with the message " + ex.Message +
                                    " when reading an object");
            }
        }

        /// <summary>
        /// Get File size.
        /// </summary>
        /// <param name="keyName">Key Name</param>
        /// <returns>File Size</returns>
        private long GetS3FileSize(string keyName) {
            long fileSize;
            try {
                var request = new ListObjectsRequest {
                    BucketName = _bucketName, 
                    Prefix = keyName,
                };

                var response = _client.ListObjects(request);
                fileSize = response.S3Objects.FirstOrDefault().Size;
            }
            catch (AmazonS3Exception amazonS3Exception) {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                     amazonS3Exception.ErrorCode.Equals("InvalidSecurity"))) {
                    throw new Exception("Please check the provided AWS Credentials.");
                }
                throw new Exception("An error occurred with the message '" + amazonS3Exception.Message + "' when listing objects");
            }
            return fileSize;
        }

        /// <summary>
        /// Save file from S3
        /// </summary>
        /// <param name="keyName">Key Name</param>
        /// <returns></returns>
        public string ReadS3StreamAsString(string keyName) {
            string xmlValue;
            TextReader read;
            try
            {
                var request = new GetObjectRequest();
                request.BucketName = _bucketName;
                request.Key = keyName;

                using (GetObjectResponse response = _client.GetObject(request)) {
                    var stream = response.ResponseStream;
                    var reader = new StreamReader(stream);
                    //read = new StreamReader(stream);
                    xmlValue = reader.ReadToEnd();
                }
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                     amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    throw new Exception("Please check the provided AWS Credentials.");
                }
                throw new Exception("An error occurred with the message " + amazonS3Exception.Message +
                                    " when reading an object");
            }
            return xmlValue;
        }


        public string ReadS3(string keyName, string saveLocation) {
            var fullSaveLocation = DownloadFile(keyName, saveLocation);
            try {
                //Compare file size.
                var fileSize = GetS3FileSize(keyName);
                var fileInfo = new FileInfo(fullSaveLocation);

                if (!fileInfo.Length.Equals(fileSize)) {
                    var success = false;
                    var maxRetry = 3;

                    for (var x = 0; x < maxRetry; x++) {
                        fullSaveLocation = DownloadFile(keyName, saveLocation);

                        fileSize = GetS3FileSize(keyName);
                        fileInfo = new FileInfo(fullSaveLocation);

                        if (fileInfo.Length.Equals(fileSize)) {
                            success = true;
                            break;
                        }
                    }

                    if(!success)
                        throw new Exception("Error: File Cannot be fully download.");
                }
            }
            catch (AmazonS3Exception amazonS3Exception) {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                     amazonS3Exception.ErrorCode.Equals("InvalidSecurity"))) {
                    throw new Exception("Please check the provided AWS Credentials.");
                }
                throw new Exception("An error occurred with the message " + amazonS3Exception.Message +
                                    " when reading an object");
            }
            return fullSaveLocation;
        }

        /// <summary>
        /// Delete file from S3
        /// </summary>
        /// <param name="keyName">Key Name</param>
        public void DeleteS3(string keyName)
        {
            try
            {
                var request = new DeleteObjectRequest() {
                    BucketName = _bucketName,
                    Key = keyName
                };

                _client.DeleteObject(request);
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                     amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    throw new Exception("Please check the provided AWS Credentials.");
                }
                throw new Exception("An error occurred with the message " + amazonS3Exception.Message +
                                    " when deleting an object");
            }
        }

        public List<string> GetFileList(string systemSerial) {
            var request = new ListObjectsRequest() {
                BucketName = _bucketName,
                Prefix = "Systems/" + systemSerial + "/"
            };

            ListObjectsResponse response = _client.ListObjects(request);

            var fileKeys = new List<string>();
            foreach (var responseS3Object in response.S3Objects) {
                if (responseS3Object.Key.Contains("_" + systemSerial + "_")) {
                    fileKeys.Add(responseS3Object.Key);
                }
            }

            return fileKeys;
        }
        private void uploadComplete(IAsyncResult result)
        {
            var x = result;
        }

        private string DownloadFile(string keyName, string saveLocation) {
            var request = new GetObjectRequest {BucketName = _bucketName, Key = keyName};
            using (var response = _client.GetObject(request)) {
                using (new StreamReader(response.ResponseStream)) {
                    saveLocation = Path.Combine(saveLocation, keyName);
                    //Delete the file and download it again.
                    if (File.Exists(saveLocation))
                        File.Delete(saveLocation);

                    response.WriteResponseStreamToFile(saveLocation);
                }
            }

            return saveLocation;
        }

		public long GetS3FolderSizes(string directory) {
			long size = 0;
			try {
				ListObjectsRequest request = new ListObjectsRequest {
					BucketName = _bucketName,
					Prefix = directory
				};
				do {
					var listResponse = _client.ListObjects(request);

					foreach (S3Object obj in listResponse.S3Objects) {
						size += obj.Size;
					}
					if (listResponse.IsTruncated) {
						request.Marker = listResponse.NextMarker;
					}
					else {
						request = null;
					}
				} while (request != null);
			}
			catch (AmazonS3Exception amazonS3Exception) {
				if (amazonS3Exception.ErrorCode != null &&
					(amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
						amazonS3Exception.ErrorCode.Equals("InvalidSecurity"))) {
					throw new Exception("Please check the provided AWS Credentials.");
				}
				throw new Exception("An error occurred with the message '" + amazonS3Exception.Message + "' when listing objects");
			}
			return size;
		}


		public float GetFolderSize(string directory)
        {
            string command = $@"aws s3 ls s3://{_bucketName}/{directory} --recursive --summarize";

            System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo = procStartInfo;
            proc.Start();
            string result = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();

            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("   Total Size: ([0-9]*)");
            System.Text.RegularExpressions.Match match = regex.Match(result);

            if (match.Success)
            {
                return float.Parse(match.Groups[1].Value);
            }
            else
            {
                throw new Exception($"[Error] - at RemoteAnalyst.AWS.S3.GetFolderSize: match not successful.\nbucket: {_bucketName}, directory: {directory}");
            }

        }

        public List<S3Object> GetS3Objects(string systemSerial) {
            if(systemSerial == null) {
                return null;
            }
            var request = new ListObjectsRequest() {
                BucketName = _bucketName,
                Prefix = "Systems/" + systemSerial + "/"
            };
            List<S3Object> s3Objects = new List<S3Object>();
            do {
                // Build your call out to S3 and store the response
                ListObjectsResponse response = _client.ListObjects(request);
                if (response == null) break;

                s3Objects.AddRange(response.S3Objects);
                // If the response is truncated, we'll make another request 
                // and pull the next batch of keys
                if (response.IsTruncated) {
                    request.Marker = response.NextMarker;
                } else {
                    request = null;
                }
            } while (request != null);
            return s3Objects;
        }
    }
}