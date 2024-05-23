using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using Rebex.Net;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.FTPRelay.BLL {
    internal class UploadFile : IUploadFile {
        public string Upload(string ftpServerIp, string localFileName, string remoteFileName, string systemDirectory) {
            try {
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["PSFTP"])) {
                    UploadSFTP(ftpServerIp, localFileName, remoteFileName, systemDirectory);
                }
                else {
                    UploadPSFTP(ftpServerIp, localFileName, remoteFileName, systemDirectory);
                }
            }
            catch (Exception ex) {
                return ex.Message;
            }
            return "";
        }

        public string UploadSFTP(string ftpServerIp, string localFileName, string remoteFileName, string systemDirectory) {
            try {
                var decrypt = new Decrypt();
                //var ftpServer = decrypt.strDESDecrypt(ftpServerIp.Trim());
                string ftpServer = ftpServerIp.Trim();
                string logon = decrypt.strDESDecrypt(ConnectionString.FTPLogon);
                string password = decrypt.strDESDecrypt(ConnectionString.FTPPassword);

                using (var ftp = new Sftp()) {
                    ftp.Connect(ftpServer);
                    ftp.Login(logon, password);
                    bool exists = ftp.DirectoryExists(systemDirectory);
                    if (!exists) {
                        ftp.CreateDirectory(systemDirectory);
                    }

                    ftp.PutFile(localFileName, remoteFileName);
                    ftp.Disconnect();

                    //Decrease the FTP Count by 1
                    ConnectionString.FTPServers[ftpServerIp]--;
                }
            }
            catch (Exception ex) {
                return ex.Message;
            }
            return "";
        }

        public string UploadPSFTP(string ftpServerIp, string localFileName, string remoteFileName, string systemDirectory) {
            var decrypt = new Decrypt();
            string ftpServer = ftpServerIp.Trim();
            string logon = decrypt.strDESDecrypt(ConnectionString.FTPLogon);
            string password = decrypt.strDESDecrypt(ConnectionString.FTPPassword);

            //Build batch file.
            string fileName = "ftpcommand_" + DateTime.Now.Ticks + ".txt";
            string saveLocation = ConnectionString.ServerPath + "/" + fileName;

            var writer = new StreamWriter(saveLocation);
            writer.WriteLine("cd " + systemDirectory);
            writer.WriteLine("put \"" + localFileName + "\"");
            
            writer.Close();

            string arguments = "";
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["PSFTPLoadSetting"])) {
                arguments = ftpServer + " -v -load " + ConfigurationManager.AppSettings["PSFTPLoadSetting"] +
                            " -l " + logon + " -pw " + password + " -b " + fileName;
            }
            else {
                arguments = ftpServer + " -v -l " + logon + " -pw " + password + " -b " + saveLocation;
            }

            string sfptLocation = ConfigurationManager.AppSettings["PSFTP"];
            try {
                var process = new Process {
                    StartInfo = {
                        FileName = sfptLocation,
                        UseShellExecute = false,
                        CreateNoWindow = false,
                        Arguments = arguments
                    }
                };
                Process processStart = Process.Start(process.StartInfo);
                processStart.WaitForExit();
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            finally {
                File.Delete(fileName);
            }
            return "";
        }
    }
}