using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using log4net;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.UWSRelay.BLL {
    class FTPFile {
        private readonly string _systemSerial;
        private readonly string _systemName;
        private readonly string _currentLocation;
        
        public FTPFile(string systemSerial, string systemName, string currentLocation) {
            _systemSerial = systemSerial;
            _systemName = systemName;
            _currentLocation = currentLocation;
        }

        public static string RemovePassword(string connectionString)
        {
            try
            {
                if (String.IsNullOrEmpty(connectionString))
                {
                    return connectionString;
                }
                if ((connectionString.Contains("PASSWORD") && connectionString.Contains(";")) || (connectionString.Contains("password") && connectionString.Contains(";")))
                {
                    List<string> strlist = connectionString.Split(';').ToList();
                    for (int i = 0; i < strlist.Count; i++)
                    {
                        if (strlist[i].Contains("PASSWORD") || connectionString.Contains("password"))
                        {
                            strlist.Remove(strlist[i]);
                            break;
                        }
                    }
                    string concat = String.Join(";", strlist.ToArray());
                    return concat;
                }
                else
                {
                    return connectionString;
                }
            }
            catch (Exception e)
            {
                return connectionString;
            }
        }

        private XmlDocument HttpCallToNonStopUMP(ILog log, List<string> nonStopSaveLocation, string outputSubVol, string fileCode, string ipAddress, string monitorPort, int ntsOrderID, string systemSerial, string systemName, string measFH) {
            string url = "http://" + ipAddress + ":" + monitorPort + "/homepage?";
            //string responseFromServer = "";
            log.InfoFormat("url: {0}", url);
            
            var xmlDoc = new XmlDocument();

            try {
                var encoding = new ASCIIEncoding();
                string urlParameter = BuildString(nonStopSaveLocation, outputSubVol, nonStopSaveLocation.Count, fileCode, ntsOrderID, systemSerial, systemName, measFH);
                log.InfoFormat("urlParameter: {0}", urlParameter);
                
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
            }
            catch (Exception ex) {
                log.ErrorFormat("Error: {0}", ex.Message);
                
                // ErrorLog.Write("ERROR", "Exception making call to NonStop: {0}", ex.Message);
                throw new Exception(ex.Message);
            }

            return xmlDoc;
        }

        private string BuildString(IEnumerable<string> dataLocation, string outputSubVol, int measureFileCount, string fileCode, int ntsOrderID, string systemSerial, string systemName, string measFH) {
            var strb = new StringBuilder();

            strb.Append("command=PROCESS_COLLECTION&");
            strb.Append("response_type=json&");
            strb.Append("output_subvol=" + outputSubVol + "&");
            strb.Append("trigger_transfer=Y&");
            strb.Append("requestor=3&");
            if (ntsOrderID > 0)
                strb.Append("nts_order_id=" + ntsOrderID + "&");

            strb.Append("process_in_parallel=Y&");

            if (fileCode.Equals("1729"))
                strb.Append("is_pak_files=Y&");
            else
                strb.Append("is_pak_files=N&");

            //Add measfh infor.
            strb.Append("measfh_file_location=" + measFH + "&");

            strb.Append("number_of_files=" + measureFileCount + "&");

            strb.Append("files_for_processing=" + string.Join(",", dataLocation) + "&");

            if (systemName.IndexOf('\\') > -1) {
                systemName = systemName.Substring(1);
            }
            strb.Append("serial_number=" + systemSerial + "&node_name=" + systemName);

            return strb.ToString();
        }

        internal XmlDocument HttpCallToNonStop(ILog log, string dataLocation, int measureFileCount, string ipAddress, string port) {
            string url = "http://" + ipAddress + ":" + port + "/homepage?";
            //string responseFromServer = "";
            log.InfoFormat("url: {0}", url);
            
            XmlDocument xmlDoc = new XmlDocument();

            try {
                ASCIIEncoding encoding = new ASCIIEncoding();
                string urlParameter = BuildString(dataLocation, measureFileCount);
                log.InfoFormat("urlParameter: {0}", urlParameter);
                
                byte[] data = encoding.GetBytes(urlParameter);

                // Prepare web request.
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
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
                //reader.Close();
                dataStream.Close();
                //response.Close();
            }
            catch (Exception ex) {
                log.ErrorFormat("Error: {0}", ex.Message);
                
                // ErrorLog.Write("ERROR", "Exception making call to NonStop: {0}", ex.Message);
                throw new Exception(ex.Message);
            }

            return xmlDoc;
        }

        private string BuildString(string dataLocation, int measureFileCount) {

            var strb = new StringBuilder();

            /*strb.Append("DATA-LOCATION=" + dataLocation + "&");
            strb.Append("NODE-NAME=" + _systemName + "&");
            strb.Append("MEASURE-FILE-COUNT=" + measureFileCount + "&");
            strb.Append("SERIAL-NUMBER=" + _systemSerial + "&");
            strb.Append("MEASFH=" + _measFH);*/
            return strb.ToString();
        }

        internal string UploadFile(string measureFileName, ILog log, string fileCode, string measFH, bool isRetry = false) {
            string NSFileName = "";
            string saveLocation = "";
            string fileName = "";
            long fileSize = 0;
            DateTime startUpTime = new DateTime(1979, 1, 1);
            DateTime stopUpTime = new DateTime(1979, 1, 1);

            var nonstopInfo = new NonStopInfoService(ConnectionString.ConnectionStringDB);
            DataTable nonStopData = nonstopInfo.GetNonStopInfoFor();

            string ipAddress = "";
            string user = "";
            string password = "";
            string volume = "";
            string monitorPort = "";

            log.InfoFormat("nonStopData.Rows.Count: {0}", nonStopData.Rows.Count);

            if (nonStopData.Rows.Count > 0) {
                ipAddress = nonStopData.Rows[0]["IPAddress"].ToString();
                user = nonStopData.Rows[0]["User"].ToString();
                var encrypt = new Decrypt();
                password = encrypt.strDESDecrypt( nonStopData.Rows[0]["Password"].ToString());
                volume = nonStopData.Rows[0]["Volume"].ToString();
                monitorPort = nonStopData.Rows[0]["MonitorPort"].ToString();
            }
            log.InfoFormat("In Upload FIle: {0}", ConnectionString.IsProcessDirectlySystem);

            if (ConnectionString.IsProcessDirectlySystem) {
                Dictionary<string, string> volumeNonStopIpPairs = new Dictionary<string, string>();
                foreach(var i in ConnectionString.Vols) {
                    if(i.Value == ipAddress) {
                        volumeNonStopIpPairs.Add(i.Key, i.Value);
                    }
                }
                ////Change the volumne name.
                ////if (ConnectionString.VolumeOrder > 7)
                //if (ConnectionString.VolumeOrder > 3)
                //    ConnectionString.VolumeOrder = 0;

                //log.InfoFormat("ConnectionString.VolumeOrder: {0}", ConnectionString.VolumeOrder);
                //

                var tempLocation = volumeNonStopIpPairs.ElementAt(ConnectionString.VolumeOrder % volumeNonStopIpPairs.Count);
                volume = tempLocation.Key;
                ipAddress = tempLocation.Value;
                log.InfoFormat("volume: {0}", volume);
                log.InfoFormat("237 ipAddress: {0}", ipAddress);
                

                ConnectionString.VolumeOrder++;
            }
            log.Info("line 245");

            int pext = 0;
            int sext = 0;
            int mext = 0;
            int fid = 0;
            var ftp = new CustomFTP();
            log.InfoFormat("line 252 ipAddress: {0}", ipAddress);
            log.InfoFormat("user: {0}", user);
            log.InfoFormat("password: {0}", password);
            log.InfoFormat("volume: {0}", volume);
            

            bool login;
            bool download;
            string strMsg = "";
            bool connect;
            fileName = _currentLocation + "\\" + measureFileName;
            var fileInfo = new FileInfo(fileName);

            string outputSubVol;
            var nonStopFiles = new List<string>();

            try {

                //Upload
                if (!ConnectionString.IsProcessDirectlySystem)
                    connect = ftp.Connect(ipAddress);
                else {
                    if (isRetry)
                        connect = ftp.Connect(ipAddress);
                    else
                        connect = true;
                }


                if (connect) {
                    if (!ConnectionString.IsProcessDirectlySystem)
                        login = ftp.Login(user, password);
                    else {
                        if (isRetry)
                            login = ftp.Login(user, password);
                        else
                            login = true;
                    }

                    if (login) {

                        var tick = fileInfo.Length.ToString();
                        //saveLocation = volume + "." + _systemName.Substring(0, 2) + tick.Substring(tick.Length - 6, 6);
                        //saveLocation = volume + "." + "R" + _systemSerial.Substring(_systemSerial.Length - 3, 3) + tick.Substring(tick.Length - 4, 4);
                        saveLocation = volume + "." + "R" + _systemSerial;

                        if (tick.Length > 7)
                            outputSubVol = volume + "." + "R" + tick.Substring(tick.Length - 7, 7);
                        else {
                            var timeTick = DateTime.Now.Ticks.ToString();
                            outputSubVol = volume + "." + "R" + timeTick.Substring(timeTick.Length - 7, 7);
                        }
                        //outputSubVol = volume + "." + "R" + _systemSerial.Substring(_systemSerial.Length - 3, 3)  + tick.Substring(tick.Length - 4, 4);

                        NSFileName = measureFileName.Split('_')[0];
                        if (NSFileName.Contains('.'))
                            NSFileName = NSFileName.Split('.')[0];

                        pext = 5000;
                        sext = 1000;
                        mext = 500;

                        double size = Convert.ToDouble(fileInfo.Length);
                        double allow_size = 10240000;
                        if (size > allow_size) {
                            sext = Convert.ToInt32(Math.Ceiling((size - (pext * 2048)) / ((2048) * (mext - 1))));
                        }
                        if (sext < 1) {
                            sext = 1000;
                        }

                        fid = 4;

                        log.InfoFormat("fileName: {0}", fileName);
                        log.InfoFormat("NSFileName: {0}", NSFileName);
                        log.InfoFormat("FileSize: {0}", size);
                        log.InfoFormat("saveLocation: {0}", saveLocation);
                        log.InfoFormat("fileCode: {0}", fileCode);
                        log.InfoFormat("pext: {0}", pext);
                        log.InfoFormat("sext: {0}", sext);
                        log.InfoFormat("mext: {0}", mext);
                        log.InfoFormat("fid: {0}", fid);
                        

                        var fi = new FileInfo(fileName);
                        fileSize = fi.Length;

                        var uploadStartTime = DateTime.MinValue;
                        try {
                            if (!ConnectionString.IsProcessDirectlySystem)
                                if (fileCode.Equals("101")) ftp.SendCommand("TYPE A");
                                else ftp.SendCommand("TYPE I");
                            else {
                                if (isRetry)
                                    if (fileCode.Equals("101")) ftp.SendCommand("TYPE A");
                                    else ftp.SendCommand("TYPE I");
                            }

                            startUpTime = DateTime.Now;

                            try {
                                //Insert the information to UWSFileCounts
                                var uwsFileCounts = new UWSFileCountService(ConnectionString.ConnectionStringDB);
                                var duplicate = uwsFileCounts.CheckDuplicateFor(_systemSerial, NSFileName);

                                if (!duplicate)
                                    uwsFileCounts.InsertFileInfoFor(_systemSerial, NSFileName, fileSize);
                            }
                            catch (Exception ex) {
                                log.ErrorFormat("Failed to Insert UWSFileCounts. {0}", ex);
                            }

                            //Uploading
                            uploadStartTime = DateTime.Now;

                            log.Info("Uploading to NonStop");

                            if (!ConnectionString.IsProcessDirectlySystem)
                                download = ftp.Upload(fileName, NSFileName, saveLocation, fileCode, pext, sext, mext, log);
                            else {
                                if (isRetry)
                                    download = ftp.Upload(fileName, NSFileName, saveLocation, fileCode, pext, sext, mext, log);
                                else
                                    download = true;
                            }

                            stopUpTime = DateTime.Now;
                            DateTime uploadEndTime = stopUpTime;
                            TimeSpan uploadSpan = uploadEndTime.Subtract(uploadStartTime);
                            double uploadRate = fileSize / uploadSpan.TotalMilliseconds / 1000;
                            string upDuration = String.Format("{0:00}:{1:00}:{2:00}", uploadSpan.TotalHours, uploadSpan.Minutes, uploadSpan.Seconds);
                            string strRate = String.Format("{0:0.00}", uploadRate);
                            log.InfoFormat("Uploaded {0} in {1} at {2} MB/s", 
                                NSFileName, upDuration, strRate);
                            if (!download) {
                                log.Info("Upload Failed");
                                
                                //Update DB Entry status
                                strMsg = "Failed to Upload " + NSFileName;
                                //BLL.ErrorEmails emailReports = new ErrorEmails();
                                //emailReports.EmailErrorMessage("Error on  FTP", "Failed to Upload " + NSFileName);

                                return strMsg;
                            }

                            nonStopFiles.Add(saveLocation + "." + NSFileName);
                            //Delete the file that was FTPed.
                            try {
                                if (!ConnectionString.IsProcessDirectlySystem) {
                                    if (File.Exists(fileName))
                                        File.Delete(fileName);
                                }
                            }
                            catch (Exception ex) {
                                log.ErrorFormat("Failed to delete the file. Error message: {0}", ex.InnerException.Message);
                            }

                        }
                        catch (Exception ex) {
                            if (ex.Message.Contains("Read Error: Closing connection")) {
                                //Go to next file upload.
                                log.InfoFormat("Closing connection error, continue to next file. Error message: {0}", ex.InnerException.Message);
                                

                                connect = ftp.Connect(ipAddress);
                                login = ftp.Login(user, password);

                                stopUpTime = DateTime.Now;
                                DateTime uploadEndTime = stopUpTime;
                                TimeSpan uploadSpan = uploadEndTime.Subtract(uploadStartTime);
                                double uploadRate = fileSize / uploadSpan.TotalMilliseconds / 1000;
                                string upDuration = String.Format("{0:00}:{1:00}:{2:00}", uploadSpan.TotalHours, uploadSpan.Minutes, uploadSpan.Seconds);
                                string StrRate = String.Format("{0:0.00}", uploadRate);
                                log.InfoFormat("Uploaded {0} in {1} at {2} MB/s",
                                NSFileName, upDuration, StrRate);
                            }
                            else {
                                log.ErrorFormat("Error: {0}", ex.Message);
                                
                                strMsg = ex.Message;
                                stopUpTime = DateTime.Now;
                                //BLL.ErrorEmails emailReports = new ErrorEmails();
                                //emailReports.EmailErrorMessage("Error on  FTP", ex.Message);
                                return strMsg;
                            }
                        }
                        //after download load the uws file.
                    }
                    else {
                        //Failed to login.
                        //Update DB Entry status
                        log.Info("Failed to Connect");
                        return "Failed to Connect";
                    }
                }
                else {
                    //Connect to FTP Server failed.
                    //Update DB Entry status
                    log.Info("Failed to connect to NonStop server");
                    
                    return "Failed to connect to NonStop server";
                }

                try {
                    if (!ConnectionString.IsProcessDirectlySystem) {
                        HttpCallToNonStopUMP(log, nonStopFiles, outputSubVol, fileCode, ipAddress, monitorPort, 0, _systemSerial, _systemName, measFH);
                    }
                    else {
                        if (isRetry) {
                            log.Info("Calling HttpCallToNonStopUMP");
                            
                            HttpCallToNonStopUMP(log, nonStopFiles, outputSubVol, fileCode, ipAddress, monitorPort, 0, _systemSerial, _systemName, measFH);
                        }
                        else {
                            log.Info("Calling CreateTaskSlip");
                            
                            //Create a file on Task Folder.
                            CreateTaskSlip(log, nonStopFiles, outputSubVol, fileCode, 0, _systemSerial, _systemName, measFH, fileInfo.Name);
                        }
                    }
                }
                catch (Exception ex) {
                    log.ErrorFormat("Failed to send HTTP Request {0}", ex);
                    return "Failed to send HTTP Request";
                }
                return "";
            }
            catch (Exception ex) {
                //Download Failed.
                //Update DB Entry status
                log.ErrorFormat("ERROR: {0}", ex.Message);
                
                return ex.Message;
            }
        }

        private void CreateTaskSlip(ILog log, List<string> nonStopFiles, string outputSubVol, string fileCode, int i, string systemSerial, string systemName, string measFh, string fileName) {
            var taskFileLocation = ConnectionString.ServerPath + @"\Tasks\";

            if (!Directory.Exists(taskFileLocation)) {
                Directory.CreateDirectory(taskFileLocation);
            }

            var taskFileName = "TS" + DateTime.Now.Ticks + ".txt"; //ts02535.txt
            using (var taskWriter = new StreamWriter(taskFileLocation + taskFileName)) {
                taskWriter.WriteLine("command=PROCESS_COLLECTION");
                taskWriter.WriteLine("output_subvol=" + outputSubVol);
                taskWriter.WriteLine("trigger_transfer=Y");
                taskWriter.WriteLine("requestor=3");
                taskWriter.WriteLine("process_in_parallel=Y");
                if (fileCode.Equals("1729"))
                    taskWriter.WriteLine("is_pak_files=Y");
                else
                    taskWriter.WriteLine("is_pak_files=N");
                taskWriter.WriteLine("measfh_file_location=" + measFh);
                taskWriter.WriteLine("number_of_files=" + nonStopFiles.Count);
                taskWriter.WriteLine("files_for_processing=" + string.Join(",", nonStopFiles));
                taskWriter.WriteLine("serial_number=" + systemSerial);
                taskWriter.WriteLine("node_name=" + systemName);
                taskWriter.WriteLine("fetch_file=" + fileName);
                taskWriter.WriteLine("fetch_from=Systems/" + systemSerial + "/UploadFolder");
            }
        }

        internal string UploadFiles(Dictionary<string, long> measureFiles, ILog log, string fileCode, string measFH) {
            string NSFileName = "";
            string saveLocation = "";
            string fileName = "";
            long fileSize = 0;
            DateTime startUpTime = new DateTime(1979, 1, 1);
            DateTime stopUpTime = new DateTime(1979, 1, 1);
            log.InfoFormat("nonstopinfo connection string {0}", ConnectionString.ConnectionStringDB);

            var nonstopInfo = new NonStopInfoService(ConnectionString.ConnectionStringDB);
            DataTable nonStopData = nonstopInfo.GetNonStopInfoFor();
            log.Info("line 531 - UploadFiles");

            string ipAddress = "";
            string user = "";
            string password = "";
            string volume = "";
            string monitorPort = "";

            log.InfoFormat("line 541 - nonStopData.Rows.Count {0}", nonStopData.Rows.Count);
            if (nonStopData.Rows.Count > 0) {
                ipAddress = nonStopData.Rows[0]["IPAddress"].ToString();
                user = nonStopData.Rows[0]["User"].ToString();
                var encrypt = new Decrypt();
                password = encrypt.strDESDecrypt(nonStopData.Rows[0]["Password"].ToString());
                volume = nonStopData.Rows[0]["Volume"].ToString();
                monitorPort = nonStopData.Rows[0]["MonitorPort"].ToString();
            }
            else {
                // log message saying nonstop monitor information is not specified
            }

			int pext = 0;
            int sext = 0;
            int mext = 0;
            int fid = 0;
            var ftp = new CustomFTP();
            log.InfoFormat("559 ipAddress: {0}", ipAddress);
            log.InfoFormat("user: {0}", user);
            log.InfoFormat("password: {0}", password);
            

            bool login;
            bool download;
            string strMsg = "";
            bool connect;
            try {

                //Upload
                connect = ftp.Connect(ipAddress);

                string outputSubVol;
                var nonStopFiles = new List<string>();

                if (connect) {
                    login = ftp.Login(user, password);

                    if (login) {
                        var tick = DateTime.Now.Ticks.ToString();
                        saveLocation = volume + ".RAM" + _systemName.Substring(0, 1) + tick.Substring(tick.Length - 4, 4);
                        outputSubVol = volume + "." + "R" + tick.Substring(tick.Length - 7, 7);

                        foreach (KeyValuePair<string, long> kv in measureFiles) {
                            fileName = _currentLocation + "\\" + kv.Key;
                            NSFileName = kv.Key.Split('_')[0];
                            pext = 5000;
                            sext = 1000;
                            mext = 500;

                            double size = Convert.ToDouble(kv.Value);
                            double allow_size = 10240000;
                            if (size > allow_size) {
                                sext = Convert.ToInt32(Math.Ceiling((size - (pext * 2048)) / ((2048) * (mext - 1))));
                            }
                            if (sext < 1) {
                                sext = 1000;
                            }

                            fid = 4;

                            log.InfoFormat("fileName: {0}", fileName);
                            log.InfoFormat("NSFileName: {0}", NSFileName);
                            log.InfoFormat("FileSize: {0}", size);
                            log.InfoFormat("saveLocation: {0}", saveLocation);
                            log.InfoFormat("outputSubVol: {0}", outputSubVol);
                            log.InfoFormat("fileCode: {0}", fileCode);
                            log.InfoFormat("pext: {0}", pext);
                            log.InfoFormat("sext: {0}", sext);
                            log.InfoFormat("mext: {0}", mext);
                            log.InfoFormat("fid: {0}", fid);
                            

                            var fi = new FileInfo(fileName);
                            fileSize = fi.Length;

                            var uploadStartTime = DateTime.MinValue;
                            try {
                                if (fileCode.Equals("101"))
                                    ftp.SendCommand("TYPE A");
                                else
                                    ftp.SendCommand("TYPE I");
                                startUpTime = DateTime.Now;

                                //Uploading
                                uploadStartTime = DateTime.Now;

                                log.Info("Uploading to NonStop");

                                download = ftp.Upload(fileName, NSFileName, saveLocation, fileCode, pext, sext, mext, log);

                                stopUpTime = DateTime.Now;
                                DateTime uploadEndTime = stopUpTime;
                                TimeSpan uploadSpan = uploadEndTime.Subtract(uploadStartTime);
                                double uploadRate = fileSize / uploadSpan.TotalMilliseconds / 1000;
                                string upDuration = String.Format("{0:00}:{1:00}:{2:00}", uploadSpan.TotalHours, uploadSpan.Minutes, uploadSpan.Seconds);
                                string StrRate = String.Format("{0:0.00}", uploadRate);
                                log.InfoFormat("Uploaded {0} in {1} at {2} MB/s",
                                                NSFileName, upDuration, StrRate);

                                if (!download) {
                                    log.Info("Upload Failed");
                                    
                                    //Update DB Entry status
                                    strMsg = "Failed to Upload " + NSFileName;
                                    //BLL.ErrorEmails emailReports = new ErrorEmails();
                                    //emailReports.EmailErrorMessage("Error on  FTP", "Failed to Upload " + NSFileName);

                                    return strMsg;
                                }

                                nonStopFiles.Add(saveLocation + "." + NSFileName);
                                //Delete the file that was FTPed.
                                try {
                                    if (!ConnectionString.IsProcessDirectlySystem) {
                                        if (File.Exists(fileName))
                                            File.Delete(fileName);
                                    }
                                }
                                catch (Exception ex) {
                                    log.ErrorFormat("Failed to delete the file. Error message: {0}", ex.InnerException.Message);
                                    
                                }
                            }
                            catch (Exception ex) {
                                if (ex.Message.Contains("Read Error: Closing connection")) {
                                    //Go to next file upload.
                                    log.ErrorFormat("Closing connection error, continue to next file. Error message: {0}", ex.InnerException.Message);
                                    

                                    connect = ftp.Connect(ipAddress);
                                    login = ftp.Login(user, password);

                                    stopUpTime = DateTime.Now;
                                    DateTime uploadEndTime = stopUpTime;
                                    TimeSpan uploadSpan = uploadEndTime.Subtract(uploadStartTime);
                                    double uploadRate = fileSize / uploadSpan.TotalMilliseconds / 1000;
                                    string upDuration = String.Format("{0:00}:{1:00}:{2:00}", uploadSpan.TotalHours, uploadSpan.Minutes, uploadSpan.Seconds);
                                    string StrRate = String.Format("{0:0.00}", uploadRate);
                                    log.InfoFormat("Uploaded {0} in {1} at {2} MB/s", NSFileName, upDuration, StrRate);
                                }
                                else {
                                    log.InfoFormat("Error: {0}", ex.Message);
                                    
                                    strMsg = ex.Message;
                                    stopUpTime = DateTime.Now;
                                    //BLL.ErrorEmails emailReports = new ErrorEmails();
                                    //emailReports.EmailErrorMessage("Error on  FTP", ex.Message);
                                    return strMsg;
                                }
                            }
                            //after download load the uws file.
                        }
                    }
                    else {
                        //Failed to login.
                        //Update DB Entry status
                        log.Info("Failed to Connect");
                        
                        return "Failed to Connect";
                    }
                }
                else {
                    //Connect to FTP Server failed.
                    //Update DB Entry status
                    log.Info("Failed to connect to NonStop server");
                    
                    return "Failed to connect to NonStop server";
                }

                try {
                    //HttpCallToNonStopUMP(log, saveLocation, measureFiles.Count, ipAddress, monitorPort);
                    HttpCallToNonStopUMP(log, nonStopFiles, outputSubVol, fileCode, ipAddress, monitorPort, 0, _systemSerial, _systemName, measFH);
                }
                catch (Exception ex) {
                    log.Info("Failed to send HTTP Request");
                    
                    return "Failed to send HTTP Request";
                }
                return "";
            }
            catch (Exception ex) {
                //Download Failed.
                //Update DB Entry status
                log.ErrorFormat("ERROR: {0}", ex.Message);
                
                return ex.Message;
            }
        }
    }
}
