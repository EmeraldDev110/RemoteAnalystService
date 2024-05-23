using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RemoteAnalyst.BusinessLogic.Enums;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.BLL {
    /// <summary>
    /// UWSFileInfo is an utility class that gets the file information from the UWS data file
    /// </summary>
    public class UWSFileInfo {
        /// <summary>
        /// Get the file size of the UWS data file.
        /// </summary>
        /// <param name="uwsName"> Full file path of the UWS data file.</param>
        /// <returns>Return a long value which is the size of the data file.</returns>
        public long GetFileSize(string uwsName) {
            long fileSize = 0;

            //Get's the UWS File name.
            if (File.Exists(uwsName)) {
                var finfo = new FileInfo(uwsName);
                fileSize = finfo.Length;
            }

            return fileSize;
        }

        /// <summary>
        /// Get the file type of the UWS file.
        /// </summary>
        /// <param name="uwsName"> Full path of the UWS data file. </param>
        /// <returns> Return a string value which is the type of the UWS data file.</returns>
        public string GetFileType(string uwsName) {
            string type;

            using (var sr = new StreamReader(uwsName)) {
                string line = sr.ReadLine();
                line += sr.ReadLine();
                line += sr.ReadLine();
                line += sr.ReadLine();
                line += sr.ReadLine();
                //Check the if UWS File is SYSTEM OR PATHWAY.
                //there is a "Collection State String" on second line of UWS File for Pathway.
                //If watch it's a Pathway collection and if not it's a System collection.
                if (line.IndexOf("COLLECTION State String") == -1) {
                    type = "System";
                }
                else {
                    type = "Pathway";
                }
            }

            return type;
        }

        /// <summary>
        /// Check if the file is old UWS version.
        /// </summary>
        /// <param name="uwsName"> Full path of the UWS data file. </param>
        /// <returns> Return a short value which is the code of the UWS file version</returns>
        public UWS.Types UwsFileVersionNew(string uwsName) {
            using (var sr = new StreamReader(uwsName)) {
                //Read first 5 lines.
                string line = sr.ReadLine();
                line += sr.ReadLine();
                line += sr.ReadLine();
                line += sr.ReadLine();
                line += sr.ReadLine();

                if (line.Contains("07032007")) {
                    //New SPAM.
                    return UWS.Types.Version2007;
                }
                if (line.Contains("02262009")) {
                    //SPAM.
                    return UWS.Types.Version2009;
                }
                if (line.IndexOf("COLLECTION State String") != -1) {
                    return UWS.Types.Pathway;
                }
                //New type.
                return UWS.Types.Version2013;
            }
        }

        public bool IsLatestUWSFile(string uwsName) {
            bool isUWS = false;

            using (var sr = new StreamReader(uwsName))
            {
                //Read first 5 lines.
                string line = sr.ReadLine();
                line += sr.ReadLine();
                line += sr.ReadLine();
                line += sr.ReadLine();
                line += sr.ReadLine();

                if (line.Contains("T2080H02_01Jul2013_TPDCCVTR_ABC")) {
                    //New SPAM.
                    isUWS = true;
                }
            }

            return isUWS;
        }

        public bool IsValidUWSFile(string uwsLocation, string connectionString) {
            bool isUWS = false;

            try {
                //Check for duplicated from LoadingStatusDetail table(On Que) using UWS file name.
                //duplicated = CheckDuplicatedUWS(fileName);
                //if (!duplicated) {
                using (var sr = new StreamReader(uwsLocation)) {
                    string line = sr.ReadLine();
                    //If it doesn't have "RAP P2C2E2 2003*", append second line.
                    if (line.IndexOf("RAP P2C2E2 2003*") == -1) {
                        line += sr.ReadLine();
                        line += sr.ReadLine();
                        line += sr.ReadLine();
                        line += sr.ReadLine();
                    }

                    //Check to see if the uploaded file is UWS File.
                    if (line.IndexOf("RAP P2C2E2 2003*") != -1) {
                        isUWS = true;
                    }
                    else {
                        //Check for new format.
                        string vProc = RemoveNULL(line.Substring(84, 50).Trim()).Trim();

                        var vProcVersions = new VProcVersionService(connectionString);
                        string vProcs = vProcVersions.GetVprocVersionFor(vProc);

                        if (vProcs.Contains(vProc)) {
                            isUWS = true;
                        }
                    }
                }
            }
            catch {
                isUWS = false;
            }

            return isUWS;
        }

        public string UWSSystemSerial(string uwsLocation, UWS.Types uwsType) {
            string systemSerial = "";
            try {
                //Check for duplicated from LoadingStatusDetail table(On Que) using UWS file name.
                //duplicated = CheckDuplicatedUWS(fileName);
                //if (!duplicated) {
                using (var sr = new StreamReader(uwsLocation)) {
                    string line = sr.ReadLine();
                    //If it doesn't have "RAP P2C2E2 2003*", append second line.
                    if (line.IndexOf("RAP P2C2E2 2003*") == -1) {
                        line += sr.ReadLine();
                        line += sr.ReadLine();
                        line += sr.ReadLine();
                        line += sr.ReadLine();
                    }

                    //Check for new file.
                    if (UWS.Types.Version2007 == uwsType) {
                        //new format. Skip for now.
                        systemSerial = line.Substring(18, 9).Trim();
                    }
                    else if (UWS.Types.Version2009 == uwsType) {
                        systemSerial = line.Substring(9, 9).Trim();
                    }
                    else if (UWS.Types.Pathway == uwsType) {
                        systemSerial = line.Substring(9, 9).Trim();
                    }
                    else if (UWS.Types.Version2013 == uwsType) {
                        systemSerial = RemoveNULL(line.Substring(28, 20).Trim()).Trim();
                    }
                }

                //check if char is vaild.
                for (int x = 0; x < systemSerial.Length; x++) {
                    if (!char.IsLetterOrDigit(systemSerial[x])) {
                        //Delete the char.
                        systemSerial = systemSerial.Remove(x, 1);
                        x--;
                    }
                }

                //System Serial Number Fix
                int sysSerialLength = systemSerial.Length;
                if (sysSerialLength < 6) {
                    //Prepend Leading Zeroes
                    for (int i = 0; i < (6 - sysSerialLength); i++)
                        systemSerial = "0" + systemSerial;
                }
            }
            catch { }

            return systemSerial;
        }

        private string RemoveNULL(string input) {
            if (!string.IsNullOrEmpty(input)) {
                var sb = new StringBuilder(input.Length);
                foreach (char c in input) {
                    sb.Append(Char.IsControl(c) ? ' ' : c);
                }
                input = sb.ToString();
            }
            return input;
        }
    }
}