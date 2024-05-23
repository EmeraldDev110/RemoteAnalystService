using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class UploadService {
        private readonly string _connectionString;

        public UploadService(string connectionString) {
            _connectionString = connectionString;
        }

        public void UpdateUploadInformation(string systemSerial, string fileName, DateTime collectionStartTime, DateTime collectionToTime, 
            string advisorEmail, string supportEmail, string website, string emailServer, 
            int emailPort, string emailUser, string emailPassword, bool emailAuth, string systemLocation,
            string serverPath, bool isSSL, bool isLocalAnalyst,
            string mailGunSendAPIKey, string mailGunSendDomain) {
            var uploadFileNameService = new UploadFileNameServices(_connectionString);
            var uploadStatusService = new UploadStatusService(_connectionString);
            var orderId = uploadFileNameService.GetOrderIdFor(fileName);
            var uploadMessage = new UploadMessagesService(_connectionString);

            if (orderId > 0) {
                //1. Update FileNameStatus.
                uploadFileNameService.UpdateLoadStatusFor(fileName);


                //2. Update Collection Start and Stop.
                UploadCollectionStartTimeFor(orderId, collectionStartTime);
                UploadCollectionToTimeFor(orderId, collectionToTime);

                //3. Check Upload Status. If the status is 118,
                var statusId = uploadStatusService.GetStatusIdFor(orderId);

                if (statusId == 118) {
                    //4. Check UploadFileName Loaded Status. If all 1, delete the entries.
                    var loadedDictionary = uploadFileNameService.CheckLoadedFor(orderId);
                    if (loadedDictionary.All(x => x.Value)) {
                        //Update status and delete all the data.
                        UpdateLoadedDateFor(orderId);
                        UpdateLoadedStatusFor(orderId, "Loaded");

                        uploadFileNameService.DeleteEntriesFor(orderId);
                        uploadStatusService.DeleteEntryFor(orderId);
                        uploadMessage.InsertNewEntryFor(orderId, DateTime.Now, "Finish Loading to Database");

                        SendEmail(orderId, systemSerial, collectionStartTime, collectionToTime,
                            advisorEmail, supportEmail, website, emailServer,
                            emailPort, emailUser, emailPassword, emailAuth, systemLocation,
                            serverPath, isSSL, isLocalAnalyst,
                            mailGunSendAPIKey, mailGunSendDomain);
                    }
                }
            }
        }

        public void UpdateUploadFailInformation(string systemSerial, string fileName, DateTime collectionStartTime, DateTime collectionToTime,
            string advisorEmail, string supportEmail, string website, string emailServer,
            int emailPort, string emailUser, string emailPassword, bool emailAuth, string systemLocation,
            string serverPath, bool isSSL, bool isLocalAnalyst,
            string mailGunSendAPIKey, string mailGunSendDomain) {

            var uploadFileNameService = new UploadFileNameServices(_connectionString);
            var uploadStatusService = new UploadStatusService(_connectionString);
            var orderId = uploadFileNameService.GetOrderIdFor(fileName);
            var uploadMessage = new UploadMessagesService(_connectionString);

            if (orderId > 0) {
                //1. Update FileNameStatus.
                uploadFileNameService.UpdateLoadStatusFor(fileName);


                //2. Update Collection Start and Stop.
                UploadCollectionStartTimeFor(orderId, collectionStartTime);
                UploadCollectionToTimeFor(orderId, collectionToTime);

                //3. Check Upload Status. If the status is 118,
                var statusId = uploadStatusService.GetStatusIdFor(orderId);

                if (statusId == 118) {
                    //4. Check UploadFileName Loaded Status. If all 1, delete the entries.
                    var loadedDictionary = uploadFileNameService.CheckLoadedFor(orderId);
                    if (loadedDictionary.All(x => x.Value)) {
                        //Update status and delete all the data.
                        UpdateLoadedDateFor(orderId);
                        UpdateLoadedStatusFor(orderId, "Failed");

                        uploadFileNameService.DeleteEntriesFor(orderId);
                        uploadStatusService.DeleteEntryFor(orderId);
                        uploadMessage.InsertNewEntryFor(orderId, DateTime.Now, "Unable to load data, interval for the measure data must be at least 1 minute.");

                        SendFailEmail(orderId, systemSerial, collectionStartTime, collectionToTime,
                            advisorEmail, supportEmail, website, emailServer,
                            emailPort, emailUser, emailPassword, emailAuth, systemLocation,
                            serverPath, isSSL, isLocalAnalyst, 
                            mailGunSendAPIKey, mailGunSendDomain);
                    }
                }
            }
        }


        public void SendEmail(int orderId, string systemSerial, DateTime collectionStartTime, DateTime collectionToTime,
            string advisorEmail, string supportEmail, string website, string emailServer,
            int emailPort, string emailUser, string emailPassword, bool emailAuth, string systemLocation,
            string serverPath, bool isSSL, bool isLocalAnalyst,
            string mailGunSendAPIKey, string mailGunSendDomain) {
            //Send email to the customer.
            var uploads = new Uploads(_connectionString);
            var customerId = uploads.GetCustomerID(orderId);

            var customerService = new CusAnalystService(_connectionString);
            var customerEmail = customerService.GetCustomerEmailFor(customerId);

            var systemService = new System_tblService(_connectionString);
            var systemName = systemService.GetSystemNameFor(systemSerial);

            var uploadEmail = new UploadEmail(advisorEmail, supportEmail, website, emailServer,
                emailPort, emailUser, emailPassword, emailAuth, systemLocation, serverPath, 
                isSSL, isLocalAnalyst, mailGunSendAPIKey, mailGunSendDomain);
            uploadEmail.SendLoadLoadEmail(customerEmail.Email, customerEmail.FisrtName + " " + customerEmail.LastName, 
                   systemSerial, systemName, collectionStartTime, collectionToTime);
        }

        public void SendFailEmail(int orderId, string systemSerial, DateTime collectionStartTime, DateTime collectionToTime,
            string advisorEmail, string supportEmail, string website, string emailServer,
            int emailPort, string emailUser, string emailPassword, bool emailAuth, string systemLocation,
            string serverPath, bool isSSL, bool isLocalAnalyst,
            string mailGunSendAPIKey, string mailGunSendDomain) {
            //Send email to the customer.
            var uploads = new Uploads(_connectionString);
            var customerId = uploads.GetCustomerID(orderId);

            var customerService = new CusAnalystService(_connectionString);
            var customerEmail = customerService.GetCustomerEmailFor(customerId);

            var systemService = new System_tblService(_connectionString);
            var systemName = systemService.GetSystemNameFor(systemSerial);

            var uploadEmail = new UploadEmail(advisorEmail, supportEmail, website, emailServer,
                emailPort, emailUser, emailPassword, emailAuth, systemLocation, serverPath,
                isSSL, isLocalAnalyst, mailGunSendAPIKey, mailGunSendDomain);
            uploadEmail.SendLoadFailEmail(customerEmail.Email, customerEmail.FisrtName + " " + customerEmail.LastName,
                systemSerial, systemName, collectionStartTime, collectionToTime);
        }


        public int GetCustomerIDFor(int uploadID) {
            var uploads = new Uploads(_connectionString);
            var customerID = uploads.GetCustomerID(uploadID);

            return customerID;
        }

        public void UpdateLoadedDateFor(int uploadID) {
            var uploads = new Uploads(_connectionString);
            uploads.UpdateLoadedDate(uploadID);
        }

        public void UpdateLoadedStatusFor(int uploadID, string message) {
            var uploads = new Uploads(_connectionString);
            uploads.UpdateLoadedStatus(uploadID, message);
        }

        public void UploadCollectionStartTimeFor(int uploadID, DateTime collectionStartTime) {
            var uploads = new Uploads(_connectionString);
            uploads.UploadCollectionStartTime(uploadID, collectionStartTime);
        }

        public void UploadCollectionToTimeFor(int uploadID, DateTime collectionToTime) {
            var uploads = new Uploads(_connectionString);
            uploads.UploadCollectionToTime(uploadID, collectionToTime);
        }

        public bool CheckHttpCallToNonStop(string ipAddress, int monitorPort) {
            string url = "http://" + ipAddress + ":" + monitorPort + "/homepage?";

            var isHttpCall = false;

            try {
                var encoding = new ASCIIEncoding();
                string urlParameter = "command=GET_VERSION_INFO";
                byte[] data = encoding.GetBytes(urlParameter);

                // Prepare web request.
                var myRequest = (HttpWebRequest)WebRequest.Create(url);
                myRequest.Method = "POST";
                myRequest.ContentType = "application/x-www-form-urlencoded";
                myRequest.ContentLength = data.Length;
                myRequest.Timeout = 600000;
                myRequest.Proxy = null;
                Stream dataStream = myRequest.GetRequestStream();

                //Send the data using stream.
                dataStream.Write(data, 0, data.Length);
                dataStream.Close();

                //Clean up the streams.
                dataStream.Close();
                isHttpCall = true;
            }
            catch {
                isHttpCall = false;
            }

            return isHttpCall;
        }
    }
}
