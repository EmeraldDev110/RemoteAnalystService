using System;
using System.IO;
using System.Threading;
using log4net;
using RemoteAnalyst.BusinessLogic.BLL;
using RemoteAnalyst.BusinessLogic.Enums;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;

namespace RemoteAnalyst.UWSLoader.BLL {

    /// <summary>
    /// Watcher is a utility class that check duplicate data.
    /// Update LoadinInfo and LoadingInfoDisk tables.
    /// </summary>
    internal class Watcher {

        /// <summary>
        /// Get the current max UWS ID in the LoadingInfo table
        /// </summary>
        /// <returns> Return an Int value which is the Max UWS ID.</returns>
        public int GetMaxUWSID() {
            var loadingInfoService = new LoadingInfoService(ConnectionString.ConnectionStringDB);
            return loadingInfoService.GetMaxUWSIDFor();
        }

        /// <summary>
        /// Check if current loaded file exists in LoadingStatusDetail table.
        /// </summary>
        /// <param name="fileName"> File name of the UWS data file.</param>
        /// <returns> Return a bool value suggests whether exists in LoadingStatusDetail table. </returns>
        public bool CheckDuplicatedUWS(string fileName) {
            string connectionString = ConnectionString.ConnectionStringDB;
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            var loadingStatusDetail = new LoadingStatusDetailService(connectionString);
            bool returnValue = false;
            returnValue = loadingStatusDetail.CheckDuplicatedUWSFor(fileName);
            return returnValue;
        }

        /// <summary>
        /// Read the data file and get the detailed info, such as start and stop time of the UWS file.
        /// Check if that period of data exists in SampleInfo table.
        /// </summary>
        /// <param name="systemSerial"> System serial number.</param>
        /// <param name="uwsFileName"> Full file path of the UWS data file.</param>
        /// <param name="log"> Log file. </param>
        /// <returns> Return a bool value suggets whether the period is duplicated. </returns>
        public bool CheckDuplicateFromSampleInfo(string systemSerial, string uwsFileName, ILog log) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            //Open Jobpool file to get customer logon, systemSerial number and uwsfile name.
            string connectionString = ConnectionString.ConnectionStringDB;
            bool isSystem = false;
            var startTime = new DateTime();
            var endTime = new DateTime();
            bool result = false;

            #region Open UWS File to get startDate and endDate.

            //filePath = localPath + "\\Customer\\" + customerLogin + "\\" + fileName;
            log.InfoFormat("filePath: {0}", uwsFileName);
            
            var collectionType = new UWSFileInfo();
            UWS.Types checkSPAM = collectionType.UwsFileVersionNew(uwsFileName);

            if (checkSPAM == UWS.Types.Pathway) {
                using (var sr = new StreamReader(uwsFileName)) {
                    string line = sr.ReadLine();
                    //append second line.
                    line += sr.ReadLine();
                    line += sr.ReadLine();
                    line += sr.ReadLine();
                    line += sr.ReadLine();

                    //Check to see if the uploaded file is UWS File.
                    if (line.IndexOf("RAP P2C2E2 2003*") != -1) {
                        int indexStart = line.IndexOf("RAP P2C2E2 2003*");
                        string tempString = line.Substring(indexStart, 200);
                        string[] tempArray = tempString.Split('*');
                        string[] tempArray2 = tempArray[1].Split('.');
                        //startTime
                        string[] tempStartDate = tempArray2[1].Split('-'); //Start Date
                        string[] tempStartHour = tempArray2[2].Split(':'); //Start Hours
                        string tempStartMilliSec = tempArray2[3].Substring(0, 3); //Start MilliSec.

                        startTime = new DateTime(Convert.ToInt32(tempStartDate[0]), Convert.ToInt32(tempStartDate[1]),
                            Convert.ToInt32(tempStartDate[2]), Convert.ToInt32(tempStartHour[0].Substring(0, 2)), Convert.ToInt32(tempStartHour[0].Substring(2, 2))
                            , Convert.ToInt32(tempStartHour[1]), Convert.ToInt32(tempStartMilliSec));
                        //EndDate
                        int tempIndex = tempArray2[3].IndexOf("20");
                        string[] tempEndDate = tempArray2[3].Substring(tempIndex).Split('-');
                        string[] tempEndHour = tempArray2[4].Split(':');
                        string tempEndMilliSec = tempArray2[5].Substring(0, 3); //End MilliSec.

                        tempIndex = tempEndDate[0].IndexOf("20"); //Get the index point fo year.
                        endTime = new DateTime(Convert.ToInt32(tempEndDate[0].Substring(tempIndex, 4)), Convert.ToInt32(tempEndDate[1]),
                            Convert.ToInt32(tempEndDate[2]), Convert.ToInt32(tempEndHour[0].Substring(0, 2)), Convert.ToInt32(tempEndHour[0].Substring(2, 2))
                            , Convert.ToInt32(tempEndHour[1]), Convert.ToInt32(tempEndMilliSec));

                        //Since in SQL SERVER 2000 round up the millisecond, Check the first number of millisecond
                        //and if it's greater or equal to 5 add one second to the time.
                        if (Convert.ToInt32(tempStartMilliSec[0].ToString()) >= 5) {
                            startTime = startTime.AddSeconds(1);
                        }
                        if (Convert.ToInt32(tempEndMilliSec[0].ToString()) >= 5) {
                            endTime = endTime.AddSeconds(1);
                        }

                        //Check the if UWS File is SYSTEM OR PATHWAY.
                        //there is a "Collection State String" on second line of UWS File for Pathway.
                        //If watch it's a Pathway collection and if not it's a System collection.
                        if (line.IndexOf("COLLECTION State String") == -1) {
                            isSystem = true;
                        }
                    }
                }

                #endregion

                //Check Duplicated
                var sampleInfo = new SampleInfoService(connectionString);
                result = sampleInfo.CheckDuplicateDataFor(systemSerial, startTime, endTime, isSystem);
            }
            else if (checkSPAM == UWS.Types.Version2007) {
                result = false;
            }
            else if (checkSPAM == UWS.Types.Version2013) {
                using (var sr = new StreamReader(uwsFileName)) {
                    string line = sr.ReadLine();
                    //append second line.
                    line += sr.ReadLine();
                    line += sr.ReadLine();
                    line += sr.ReadLine();
                    line += sr.ReadLine();

                    //Check to see if the uploaded file is UWS File.
                    if (line.IndexOf("RAP P2C2E2 2003*") != -1) {
                        //Get SystemType
                        if (line.IndexOf("COLLECTION State String") == -1) {
                            isSystem = true;
                        }

                        try {
                            //Get Start & End Time
                            string startString = line.Substring(line.IndexOf("Start: ") + 7, 23); //+7 because I don't want start:  to be on the string.
                            startTime = Convert.ToDateTime(startString.Trim());
                            string endString = line.Substring(line.IndexOf("Stop: ") + 6, 23); //+6 because I don't want stop:  to be on the string.
                            endTime = Convert.ToDateTime(endString.Trim());

                            //Check for microsecond. if it's greater then 500, add one second.
                            if (startTime.Millisecond >= 500) {
                                startTime = startTime.AddSeconds(1);
                            }
                            if (endTime.Millisecond >= 500) {
                                endTime = endTime.AddSeconds(1);
                            }

                            //Check Duplicated
                            var sampleInfo = new SampleInfoService(connectionString);
                            result = sampleInfo.CheckDuplicateDataFor(systemSerial, startTime, endTime, isSystem);
                        }
                        catch {
                            result = false;
                        }
                    }
                }
            }
            return result;
        }

        //public bool CheckDuplicateFromSampleInfoManual(string diskFileName)
        //{
        //    //Force all datetime to be in US format.
        //    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
        //    //Open Jobpool file to get customer logon, systemSerial number and uwsfile name.
        //    string connectionString = ConnectionString.ConnectionStringDB;
        //    string customerLogin = string.Empty;
        //    string filePath = string.Empty;
        //    string systemSerial = string.Empty;
        //    bool isSystem = false;
        //    DateTime startTime = new DateTime();
        //    DateTime endTime = new DateTime();
        //    bool result = false;

        //    #region open jobpool file.
        //    string localPath = ConnectionString.ServerPath;
        //    string jobpoolPath = ConnectionString.WatchFolder;
        //    string filePathjobpool = jobpoolPath + "\\" + jobpoolName;
        //    using (StreamReader sr = new StreamReader(filePathjobpool))
        //    {
        //        customerLogin = sr.ReadLine().Trim();
        //        sr.ReadLine();
        //        filePath = sr.ReadLine().Trim();
        //    }
        //    #endregion
        //    #region Open UWS File to get startDate and endDate.
        //    if (!File.Exists(filePath))
        //    {
        //        //Skip this function.
        //        result = true;
        //    }
        //    else
        //    {
        //        UWSFileInfo collectionType = new UWSFileInfo();
        //        short checkSPAM = collectionType.UwsFileVersionNew(filePath, systemSerial);

        //        if (checkSPAM == 1)
        //        {
        //            using (StreamReader sr = new StreamReader(filePath))
        //            {
        //                string line = sr.ReadLine();
        //                //If it doesn't have "RAP P2C2E2 2003*", append second line.
        //                if (line.IndexOf("RAP P2C2E2 2003*") == -1)
        //                {
        //                    line += sr.ReadLine();
        //                    line += sr.ReadLine();
        //                    line += sr.ReadLine();
        //                    line += sr.ReadLine();
        //                }

        //                //Check to see if the uploaded file is UWS File.
        //                if (line.IndexOf("RAP P2C2E2 2003*") != -1)
        //                {
        //                    systemSerial = line.Substring(9, 9).ToString().Trim();

        //                    //check if char is vaild.
        //                    for (int x = 0; x < systemSerial.Length; x++)
        //                    {
        //                        if (!char.IsLetterOrDigit(systemSerial[x]))
        //                        {
        //                            //Delete the char.
        //                            systemSerial = systemSerial.Remove(x, 1);
        //                            x--;
        //                        }
        //                    }

        //                    int indexStart = line.IndexOf("RAP P2C2E2 2003*");
        //                    string tempString = line.Substring(indexStart, 200);
        //                    string[] tempArray = tempString.Split('*');
        //                    string[] tempArray2 = tempArray[1].Split('.');
        //                    //startTime
        //                    string[] tempStartDate = tempArray2[1].ToString().Split('-'); //Start Date
        //                    string[] tempStartHour = tempArray2[2].ToString().Split(':'); //Start Hours
        //                    string tempStartMilliSec = tempArray2[3].ToString().Substring(0, 3); //Start MilliSec.

        //                    startTime = new DateTime(Convert.ToInt32(tempStartDate[0]), Convert.ToInt32(tempStartDate[1]),
        //                        Convert.ToInt32(tempStartDate[2]), Convert.ToInt32(tempStartHour[0].Substring(0, 2)), Convert.ToInt32(tempStartHour[0].Substring(2, 2))
        //                        , Convert.ToInt32(tempStartHour[1]), Convert.ToInt32(tempStartMilliSec));
        //                    //EndDate
        //                    int tempIndex = tempArray2[3].ToString().IndexOf("20");
        //                    string[] tempEndDate = tempArray2[3].ToString().Substring(tempIndex).Split('-');
        //                    string[] tempEndHour = tempArray2[4].ToString().Split(':');
        //                    string tempEndMilliSec = tempArray2[5].ToString().Substring(0, 3);	//End MilliSec.

        //                    tempIndex = tempEndDate[0].IndexOf("20"); //Get the index point fo year.
        //                    endTime = new DateTime(Convert.ToInt32(tempEndDate[0].Substring(tempIndex, 4)), Convert.ToInt32(tempEndDate[1]),
        //                        Convert.ToInt32(tempEndDate[2]), Convert.ToInt32(tempEndHour[0].Substring(0, 2)), Convert.ToInt32(tempEndHour[0].Substring(2, 2))
        //                        , Convert.ToInt32(tempEndHour[1]), Convert.ToInt32(tempEndMilliSec));

        //                    //Since in SQL SERVER 2000 round up the millisecond, Check the first number of millisecond
        //                    //and if it's greater or equal to 5 add one second to the time.
        //                    if (Convert.ToInt32(tempStartMilliSec[0].ToString()) >= 5)
        //                        startTime = startTime.AddSeconds(1);
        //                    if (Convert.ToInt32(tempEndMilliSec[0].ToString()) >= 5)
        //                        endTime = endTime.AddSeconds(1);

        //                    //Check the if UWS File is SYSTEM OR PATHWAY.
        //                    //there is a "Collection State String" on second line of UWS File for Pathway.
        //                    //If watch it's a Pathway collection and if not it's a System collection.
        //                    if (line.IndexOf("COLLECTION State String") == -1)
        //                        isSystem = true;
        //                }
        //            }
        //            //Check Duplicated
        //            SampleInfoService sampleInfo = new SampleInfoService(connectionString);
        //            result = sampleInfo.CheckDuplicateDataFor(systemSerial, startTime, endTime, isSystem);
        //        }
        //        else if (checkSPAM == 2)
        //        {
        //            result = false;
        //        }
        //        else if (checkSPAM == 3)
        //        {
        //            using (StreamReader sr = new StreamReader(filePath))
        //            {
        //                string line = sr.ReadLine();
        //                //append second line.
        //                line += sr.ReadLine();
        //                line += sr.ReadLine();
        //                line += sr.ReadLine();
        //                line += sr.ReadLine();

        //                //Check to see if the uploaded file is UWS File.
        //                if (line.IndexOf("RAP P2C2E2 2003*") != -1)
        //                {
        //                    if (line.IndexOf("COLLECTION State String") == -1)
        //                        isSystem = true;
        //                }
        //                try
        //                {
        //                    //Get Start & End Time
        //                    string startString = line.Substring(line.IndexOf("Start: ") + 7, 23);   //+7 because I don't want start:  to be on the string.
        //                    startTime = Convert.ToDateTime(startString.Trim());
        //                    string endString = line.Substring(line.IndexOf("Stop: ") + 6, 23);      //+6 because I don't want stop:  to be on the string.
        //                    endTime = Convert.ToDateTime(endString.Trim());

        //                    //Check for microsecond. if it's greater then 500, add one second.
        //                    if (startTime.Millisecond >= 500)
        //                    {
        //                        startTime = startTime.AddSeconds(1);
        //                    }
        //                    if (endTime.Millisecond >= 500)
        //                    {
        //                        endTime = endTime.AddSeconds(1);
        //                    }

        //                    //Check Duplicated
        //                    SampleInfoService sampleInfo = new SampleInfoService(connectionString);
        //                    result = sampleInfo.CheckDuplicateDataFor(systemSerial, startTime, endTime, isSystem);
        //                }
        //                catch
        //                {
        //                    result = false;
        //                }
        //            }
        //        }
        //    }
        //    #endregion

        //    return result;
        //}
        
        /// <summary>
        /// Create new entry for this load in LoadingInfo table.
        /// </summary>
        /// <param name="fileName"> File name of the data file.</param>
        /// <param name="tempUWSID"> UWS ID of this entry.</param>
        public void InsertLoadingInfo(string fileName, int tempUWSID) {
            //Force all datetime to be in US format.
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            string connectionString = ConnectionString.ConnectionStringDB;

            //For now, all the customer id in [LoadingInfo] is 0
            int customerID = 0;
            //defind class.
            //CusAnalystService cust = new CusAnalystService(connectionString);
            //int customerID = cust.GetCustomerIDFor(userEmail);

            var loadingInfoService = new LoadingInfoService(connectionString);

            loadingInfoService.InsertFor(tempUWSID, customerID);
            //Insert into SampleInfo, and get tempUWSID.
        }

        /// <summary>
        /// Create entry for this Disk load in LoadingInfoDisk table.
        /// </summary>
        /// <param name="systemSerial"> System serial number.</param>
        /// <param name="diskFileName"> File name of the disk file.</param>
        public void InsertLoadingInfoDisk(string systemSerial, string diskFileName) {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            string connectionString = ConnectionString.ConnectionStringDB;
            string filePath = ConnectionString.SystemLocation + systemSerial + "\\" + diskFileName;
            //defind class.
            var cust = new CusAnalystService(connectionString);
            var fileInfo = new UWSFileInfo();

            int customerID = 0;
            long fileSize = fileInfo.GetFileSize(filePath);

            var loadingInfoDiskService = new LoadingInfoDiskService(connectionString);
            loadingInfoDiskService.InsertFor(systemSerial, customerID, diskFileName, fileSize);
        }

        /// <summary>
        /// Get the current number of the orders that waiting in queue.
        /// </summary>
        /// <returns> Return an Int value which is the queue length.</returns>
        public int CurrentLoadingQue(string instanceID) {
            //Force all datetime to be in US format.
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            var loadingStatusDetailService = new LoadingStatusDetailService(ConnectionString.ConnectionStringDB);
            int queNumber = loadingStatusDetailService.GetCurrentQueueLengthFor(instanceID);
            return queNumber;
        }

        /// <summary>
        /// Check whether current load is smaller than max load that UWS loader could handle.
        /// </summary>
        /// <returns> Returns a bool value suggests whether current load is smaller than max load.</returns>
        public bool CheckLoading(string instanceID) {
            //Force all datetime to be in US format.
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            var loadingStatusService = new LoadingStatusService(ConnectionString.ConnectionStringDB);
            bool loading = loadingStatusService.CheckLoadingFor(instanceID);

            return loading;
        }

        //public void SendErrorMessageManual(string jobpoolName)
        //{
        //    //Force all datetime to be in US format.
        //    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
        //    //Get emailAddress.
        //    string emailAddress = string.Empty;
        //    string advisoremail = string.Empty;
        //    //Open Jobpool file to get customer logon, systemSerial number and uwsfile name.
        //    string customerLogin = string.Empty;
        //    string filePath = string.Empty;
        //    string systemSerial = string.Empty;
        //    DateTime startTime = new DateTime();
        //    DateTime endTime = new DateTime();

        //    #region open jobpool file.
        //    string localPath = ConnectionString.ServerPath;
        //    string jobpoolPath = ConnectionString.WatchFolder;
        //    string filePathjobpool = jobpoolPath + "\\" + jobpoolName;
        //    using (StreamReader sr = new StreamReader(filePathjobpool))
        //    {
        //        customerLogin = sr.ReadLine().Trim();
        //        sr.ReadLine();
        //        filePath = sr.ReadLine().Trim();
        //    }
        //    #endregion
        //    #region Open UWS File to get startDate and endDate.
        //    using (StreamReader sr = new StreamReader(filePath))
        //    {
        //        string line = sr.ReadLine();
        //        //If it doesn't have "RAP P2C2E2 2003*", append second line.
        //        if (line.IndexOf("RAP P2C2E2 2003*") == -1)
        //        {
        //            line += sr.ReadLine();
        //        }
        //        //Check to see if the uploaded file is UWS File.
        //        if (line.IndexOf("RAP P2C2E2 2003*") != -1)
        //        {
        //            systemSerial = line.Substring(9, 9).ToString().Trim();

        //            //check if char is vaild.
        //            for (int x = 0; x < systemSerial.Length; x++)
        //            {
        //                if (!char.IsLetterOrDigit(systemSerial[x]))
        //                {
        //                    //Delete the char.
        //                    systemSerial = systemSerial.Remove(x, 1);
        //                    x--;
        //                }
        //            }

        //            int indexStart = line.IndexOf("RAP P2C2E2 2003*");
        //            string tempString = line.Substring(indexStart, 200);
        //            string[] tempArray = tempString.Split('*');
        //            string[] tempArray2 = tempArray[1].Split('.');
        //            //startTime
        //            string[] tempStartDate = tempArray2[1].ToString().Split('-'); //Start Date
        //            string[] tempStartHour = tempArray2[2].ToString().Split(':'); //Start Hours
        //            int tempStartSec = Convert.ToInt32(tempArray2[3].ToString().Substring(0, 3)); //Start Sec.

        //            startTime = new DateTime(Convert.ToInt32(tempStartDate[0]), Convert.ToInt32(tempStartDate[1]),
        //                Convert.ToInt32(tempStartDate[2]), Convert.ToInt32(tempStartHour[0].Substring(0, 2)), Convert.ToInt32(tempStartHour[0].Substring(2, 2))
        //                , Convert.ToInt32(tempStartHour[1]));
        //            //EndDate
        //            int tempIndex = tempArray2[3].ToString().IndexOf("20");
        //            string[] tempEndDate = tempArray2[3].ToString().Substring(tempIndex).Split('-');
        //            string[] tempEndHour = tempArray2[4].ToString().Split(':');
        //            int tmepEndSec = Convert.ToInt32(tempArray2[5].ToString().Substring(0, 3));

        //            tempIndex = tempEndDate[0].IndexOf("20"); //Get the index point fo year.
        //            endTime = new DateTime(Convert.ToInt32(tempEndDate[0].Substring(tempIndex, 4)), Convert.ToInt32(tempEndDate[1]),
        //                Convert.ToInt32(tempEndDate[2]), Convert.ToInt32(tempEndHour[0].Substring(0, 2)), Convert.ToInt32(tempEndHour[0].Substring(2, 2))
        //                , Convert.ToInt32(tempEndHour[1]));
        //        }
        //    }
        //    #endregion

        //    CustomerInfo cust = new CustomerInfo();
        //    emailAddress = cust.GetEmailAddress(Convert.ToInt32(customerLogin));

        //    //create an email object, and set the mail server
        //    string emailServer = ConnectionString.EmailServer;
        //    EmailMessage msg = new EmailMessage(emailServer);

        //    //if (emailServer.ToUpper().Contains("GMAIL")) {
        //    //New Config for using Gmail account.
        //    msg.Port = ConnectionString.EmailPort;
        //    msg.Username = ConnectionString.EmailUser;
        //    msg.Password = ConnectionString.EmailPassword;
        //    AdvancedIntellect.Ssl.SslSocket ssl = new AdvancedIntellect.Ssl.SslSocket();
        //    msg.LoadSslSocket(ssl, true);
        //    msg.ValidateAddress = false;
        //    if (ConnectionString.EmailAuthentication)
        //        msg.SmtpAuthentication = SmtpAuthentication.AuthLogin;
        //    else
        //        msg.SmtpAuthentication = SmtpAuthentication.None;
        //    //}

        //    string connectionString = ConnectionString.ConnectionStringDB;
        //    string cmdText = "SELECT queryValue FROM RAInfo WHERE queryKey = 'advisoremail'";

        //    using (SqlConnection connection = new MySqlConnection(connectionString))
        //    {
        //        SqlCommand command = new MySqlCommand(cmdText, connection);
        //        var reader;
        //        connection.Open();
        //        reader = command.ExecuteReader();
        //        if (reader.Read())
        //            advisoremail = reader["queryValue"].ToString();
        //        reader.Close();
        //        connection.Close();
        //    }

        //    //set the From address
        //    msg.FromAddress = advisoremail;

        //    //set the To address
        //    msg.To = emailAddress;

        //    msg.IgnoreRecipientErrors = true;
        //    msg.EmbedImage("ralogo", @localPath + "\\Images-Work\\RALogo.gif");
        //    //set the subject and body
        //    msg.Subject = "Duplicate Analysis";

        //    EmailHeaderFooter email = new EmailHeaderFooter();
        //    Systems sysInfo = new Systems();
        //    string systemName = sysInfo.GetSystemName(systemSerial);

        //    //email body
        //    msg.HtmlBodyPart += email.EmailHeader();
        //    msg.HtmlBodyPart += "An analysis with the following characteristics already exists:";
        //    msg.HtmlBodyPart += "<UL>";
        //    msg.HtmlBodyPart += "	<LI>";
        //    msg.HtmlBodyPart += "		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>Node: " + systemName + "</DIV>";
        //    msg.HtmlBodyPart += "	<LI>";
        //    msg.HtmlBodyPart += "		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>Start Time: " + startTime.ToString() + "</DIV>";
        //    msg.HtmlBodyPart += "	<LI>";
        //    msg.HtmlBodyPart += "		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>End Time: " + endTime.ToString() + "</DIV>";
        //    msg.HtmlBodyPart += "	</LI>";
        //    msg.HtmlBodyPart += "</UL>";
        //    msg.HtmlBodyPart += email.EmailFooter();

        //    //build Admin Email body.
        //    string emailBody = string.Empty;
        //    emailBody += "An analysis with the following characteristics already exists:";
        //    emailBody += "<UL>";
        //    emailBody += "	<LI>";
        //    emailBody += "		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>Node: " + systemName + "</DIV>";
        //    emailBody += "	<LI>";
        //    emailBody += "		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>Start Time: " + startTime.ToString() + "</DIV>";
        //    emailBody += "	<LI>";
        //    emailBody += "		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>End Time: " + endTime.ToString() + "</DIV>";
        //    emailBody += "	</LI>";
        //    emailBody += "</UL>";

        //    //send the email
        //    msg.Send();

        //    //Send Email to Admins.
        //    CreateSendErrorEmail errorEmail = new CreateSendErrorEmail(emailBody);
        //}

        //public void SendErrorMessage(string jobpoolName)
        //{
        //    //Force all datetime to be in US format.
        //    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
        //    //Get emailAddress.
        //    string emailAddress = string.Empty;
        //    string customerLogin = string.Empty;
        //    string systemSerial = string.Empty;
        //    string fileName = string.Empty;
        //    string advisoremail = string.Empty;
        //    DateTime startTime = new DateTime();
        //    DateTime endTime = new DateTime();

        //    #region open jobpool file.
        //    string localPath = ConnectionString.ServerPath;
        //    string jobpoolPath = ConnectionString.WatchFolder;
        //    string filePath = jobpoolPath + "\\" + jobpoolName;
        //    using (StreamReader sr = new StreamReader(filePath))
        //    {
        //        customerLogin = sr.ReadLine().Trim();
        //        fileName = sr.ReadLine().Trim();
        //        systemSerial = sr.ReadLine().Trim();
        //    }
        //    #endregion

        //    #region Open UWS File to get startDate and endDate.
        //    //filePath = localPath + "\\Customer\\" + customerLogin + "\\" + fileName;
        //    string systemLocation = ConnectionString.SystemLocation;
        //    filePath = systemLocation + systemSerial + "\\" + fileName;

        //    //Move file to new directory.
        //    Helper helper = new Helper();
        //    helper.MoveFile(systemSerial, filePath, customerLogin, fileName);

        //    UWSFileInfo collectionType = new UWSFileInfo();
        //    short checkSPAM = collectionType.UwsFileVersionNew(filePath, systemSerial);

        //    if (checkSPAM == 1)
        //    {
        //        using (StreamReader sr = new StreamReader(filePath))
        //        {
        //            string line = sr.ReadLine();
        //            //If it doesn't have "RAP P2C2E2 2003*", append second line.
        //            if (line.IndexOf("RAP P2C2E2 2003*") == -1)
        //            {
        //                line += sr.ReadLine();
        //            }
        //            //Check to see if the uploaded file is UWS File.
        //            if (line.IndexOf("RAP P2C2E2 2003*") != -1)
        //            {
        //                int indexStart = line.IndexOf("RAP P2C2E2 2003*");
        //                string tempString = line.Substring(indexStart, 200);
        //                string[] tempArray = tempString.Split('*');
        //                string[] tempArray2 = tempArray[1].Split('.');
        //                //startTime
        //                string[] tempStartDate = tempArray2[1].ToString().Split('-'); //Start Date
        //                string[] tempStartHour = tempArray2[2].ToString().Split(':'); //Start Hours
        //                int tempStartSec = Convert.ToInt32(tempArray2[3].ToString().Substring(0, 3)); //Start Sec.

        //                startTime = new DateTime(Convert.ToInt32(tempStartDate[0]), Convert.ToInt32(tempStartDate[1]),
        //                    Convert.ToInt32(tempStartDate[2]), Convert.ToInt32(tempStartHour[0].Substring(0, 2)), Convert.ToInt32(tempStartHour[0].Substring(2, 2))
        //                    , Convert.ToInt32(tempStartHour[1]));
        //                //EndDate
        //                int tempIndex = tempArray2[3].ToString().IndexOf("20");
        //                string[] tempEndDate = tempArray2[3].ToString().Substring(tempIndex).Split('-');
        //                string[] tempEndHour = tempArray2[4].ToString().Split(':');
        //                int tmepEndSec = Convert.ToInt32(tempArray2[5].ToString().Substring(0, 3));

        //                tempIndex = tempEndDate[0].IndexOf("20"); //Get the index point fo year.
        //                endTime = new DateTime(Convert.ToInt32(tempEndDate[0].Substring(tempIndex, 4)), Convert.ToInt32(tempEndDate[1]),
        //                    Convert.ToInt32(tempEndDate[2]), Convert.ToInt32(tempEndHour[0].Substring(0, 2)), Convert.ToInt32(tempEndHour[0].Substring(2, 2))
        //                    , Convert.ToInt32(tempEndHour[1]));
        //            }
        //        }

        //    #endregion

        //        //Get customerID.
        //        CustomerInfo custInfo = new CustomerInfo();
        //        int customerID = custInfo.GetCustomerID(customerLogin);
        //        CustomerInfo cust = new CustomerInfo();
        //        emailAddress = cust.GetEmailAddress(customerID);

        //        //create an email object, and set the mail server
        //        string emailServer = ConnectionString.EmailServer;
        //        EmailMessage msg = new EmailMessage(emailServer);

        //        //if (emailServer.ToUpper().Contains("GMAIL")) {
        //        //New Config for using Gmail account.
        //        msg.Port = ConnectionString.EmailPort;
        //        msg.Username = ConnectionString.EmailUser;
        //        msg.Password = ConnectionString.EmailPassword;
        //        AdvancedIntellect.Ssl.SslSocket ssl = new AdvancedIntellect.Ssl.SslSocket();
        //        msg.LoadSslSocket(ssl, true);
        //        msg.ValidateAddress = false;
        //        if (ConnectionString.EmailAuthentication)
        //            msg.SmtpAuthentication = SmtpAuthentication.AuthLogin;
        //        else
        //            msg.SmtpAuthentication = SmtpAuthentication.None;
        //        //}

        //        string connectionString = ConnectionString.ConnectionStringDB;
        //        string cmdText = "SELECT queryValue FROM RAInfo WHERE queryKey = 'advisoremail'";

        //        using (SqlConnection connection = new MySqlConnection(connectionString))
        //        {
        //            SqlCommand command = new MySqlCommand(cmdText, connection);
        //            var reader;
        //            connection.Open();
        //            reader = command.ExecuteReader();
        //            if (reader.Read())
        //                advisoremail = reader["queryValue"].ToString();
        //            reader.Close();
        //            connection.Close();
        //        }

        //        //set the From address
        //        msg.FromAddress = advisoremail;

        //        //set the To address
        //        msg.To = emailAddress;

        //        msg.IgnoreRecipientErrors = true;
        //        msg.EmbedImage("ralogo", @localPath + "\\Images-Work\\RALogo.gif");
        //        //set the subject and body
        //        msg.Subject = "Duplicate Analysis";

        //        EmailHeaderFooter email = new EmailHeaderFooter();

        //        Systems sysInfo = new Systems();
        //        string systemName = sysInfo.GetSystemName(systemSerial);

        //        //email body
        //        msg.HtmlBodyPart += email.EmailHeader();
        //        msg.HtmlBodyPart += "An analysis with the following characteristics already exists:";
        //        msg.HtmlBodyPart += "<UL>";
        //        msg.HtmlBodyPart += "	<LI>";
        //        msg.HtmlBodyPart += "		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>Node: " + systemName + "</DIV>";
        //        msg.HtmlBodyPart += "	<LI>";
        //        msg.HtmlBodyPart += "		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>Start Time: " + startTime.ToString() + "</DIV>";
        //        msg.HtmlBodyPart += "	<LI>";
        //        msg.HtmlBodyPart += "		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>End Time: " + endTime.ToString() + "</DIV>";
        //        msg.HtmlBodyPart += "	</LI>";
        //        msg.HtmlBodyPart += "</UL>";
        //        msg.HtmlBodyPart += email.EmailFooter();

        //        //build Admin Email body.
        //        string emailBody = string.Empty;
        //        emailBody += "An analysis with the following characteristics already exists:";
        //        emailBody += "<UL>";
        //        emailBody += "	<LI>";
        //        emailBody += "		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>Node: " + systemName + "</DIV>";
        //        emailBody += "	<LI>";
        //        emailBody += "		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>Start Time: " + startTime.ToString() + "</DIV>";
        //        emailBody += "	<LI>";
        //        emailBody += "		<DIV style='FONT-SIZE: 9pt; FONT-FAMILY: Arial'>End Time: " + endTime.ToString() + "</DIV>";
        //        emailBody += "	</LI>";
        //        emailBody += "</UL>";

        //        //send the email
        //        msg.Send();

        //        //Send Email to Admins.
        //        CreateSendErrorEmail errorEmail = new CreateSendErrorEmail(emailBody);
        //    }
        //}
    }
}