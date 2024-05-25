using System;
using DataBrowser.Entities;
using RemoteAnalyst.AWS.EC2;
using RemoteAnalyst.BusinessLogic.BLL;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;

namespace RemoteAnalyst.UWSLoader.BLL {
    /// <summary>
    /// Utility classes that get the file information of the data file.
    /// Manipulate LoadingStatus and LoadingStatusDetail table.
    /// </summary>
    public class JobProcess {
        private readonly string _connectionStr = ConnectionString.ConnectionStringDB;

        /// <summary>
        /// Get the file type and file size.
        /// Insert or delete the entry in LoadingStatusDetail table
        /// </summary>
        /// <param name="values"> If values = true, insert the entry. Else, delte the entry</param>
        /// <param name="uwsFileName"> File name of the data file. </param>
        /// <param name="tempUWSID"> UWS id of this data load. </param>
        /// <param name="loadType"> Type of this load. </param>
        /// <param name="systemSerial"> System serial number.</param>
        /// <returns> Return a bool value suggets whether the database manipulation is successful or not. </returns>
        public bool InsertLoadingStatusDetail(bool values, string uwsFileName, int tempUWSID, string loadType, string systemSerial) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            bool returnValue;
            string realFileName = string.Empty;
            //Get user's login.
            string userEmail = string.Empty;
            string fullFilePath = ConnectionString.SystemLocation + systemSerial + "\\" + uwsFileName;
            string type = string.Empty;
            long fileSize = 0;
            var fileInfo = new UWSFileInfo();

            if (loadType == "SYSTEM" || loadType == "PATHWAY") {
                //Change txt file to 101.
                if (values) {
                    type = fileInfo.GetFileType(fullFilePath);
                    fileSize = fileInfo.GetFileSize(fullFilePath);
                }
            }
            else if (loadType == "DISK") {
                //Change txt file to 101.
                //int tempPos = fileName.LastIndexOf("txt");
                //fileName = fileName.Substring(0, tempPos) + "101";
                type = "Disk";
                fileSize = fileInfo.GetFileSize(fullFilePath);
            }

            //values. True == increase, False == decrease.
            //Check if the values is +1 or -1
            var loadingDetailService = new LoadingStatusDetailService(_connectionStr);
            var loadingInfoService = new LoadingInfoService(_connectionStr);
            if (values) {
                string instanceID = "";

                if (!ConnectionString.IsLocalAnalyst) {
                    var ec2 = new AmazonEC2();
                    instanceID = ec2.GetEC2ID();
                }
                returnValue = loadingDetailService.InsertLoadingStatusFor(uwsFileName, userEmail.Trim(), DateTime.Now, systemSerial.Trim(), realFileName, tempUWSID, fileSize, type, instanceID);
                //Update Instance ID on LoadingInfo table.
                loadingInfoService.UpdateInstanceIDFor(tempUWSID, instanceID);
            }
            else {
                returnValue = loadingDetailService.DeleteLoadingInfoFor(uwsFileName.Trim());
            }

            return returnValue;
        }

        /// <summary>
        /// Update the LoadingStatusDetail table, set the status to '1' which means 'loading'.
        /// </summary>
        /// <param name="uwsFileName"> File name of the data file.</param>
        /// <param name="systemSerial"> System serial number.</param>
        /// <returns> Return a bool value suggets whether the database manipulation is successful or not.</returns>
        public bool UpdateLoadingStatusDetail(string uwsFileName, string systemSerial) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            var loadingDetailService = new LoadingStatusDetailService(_connectionStr);

            bool returnValue = loadingDetailService.UpdateLoadingStatusDetailFor("1", DateTime.Now, uwsFileName, systemSerial);

            return returnValue;
        }

        /// <summary>
        /// Change the current loads number.
        /// </summary>
        /// <param name="values">Values that to be added on the current loads number. It could be be +1 or -1</param>
        public void ChangeStatus(int values) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            string instanceID = "";

            if (!ConnectionString.IsLocalAnalyst) {
                var ec2 = new AmazonEC2();
                instanceID = ec2.GetEC2ID();
            }

            //Get current Load
            int currentLoad = GetCurrentLoad();

            if (currentLoad + values >= 0) {
                var loadingStatusService = new LoadingStatusService(_connectionStr);
                loadingStatusService.UpdateLoadingStatusFor(instanceID, currentLoad + values);
            }
        }

        /// <summary>
        /// Get the current loads number.
        /// </summary>
        /// <returns> Return an Int value which is the current loads number.</returns>
        private int GetCurrentLoad() {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            string instanceID = "";
            if (!ConnectionString.IsLocalAnalyst) {
                var ec2 = new AmazonEC2();
                instanceID = ec2.GetEC2ID();
            }

            var loadingStatusService = new LoadingStatusService(_connectionStr);
            int currentLoad = loadingStatusService.GetCurrentLoadFor(instanceID);

            return currentLoad;
        }
    }
}