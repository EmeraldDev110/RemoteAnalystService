using System;
using System.IO;

namespace RemoteAnalyst.UWSRelay.BLL {
    /// <summary>
    /// FileUtil contains file operation function.
    /// Use these functions to get the information we need from the job file.
    /// </summary>
    public class FileUtil {
        private readonly string _jobpoolPath;
        public FileUtil(string watchFolder) {
            _jobpoolPath = watchFolder;
        }
        /// <summary>
        /// Get the data file name from job file
        /// The file name is always the 2nd line on the job file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string ReadFirstLine(string fileName) {
            //Get information from File.
            string jobpoolPath = _jobpoolPath;
            string tempfile = jobpoolPath + "\\" + fileName;
            bool isOpen = true;
            string fileType = "";
            StreamReader fsr = null;
            try {
                //Opne StreamReader.
                while (isOpen) {
                    try {
                        fsr = new StreamReader(tempfile);
                        isOpen = false;
                    }
                    catch {
                        isOpen = true;
                    }
                }
                //Get file name from 3rd line.
                fileType = fsr.ReadLine();

                fsr.Close();
                fsr.Dispose();
            }
            catch {
            }
            return fileType;
        }

        /// <summary>
        /// Get the data file name from job file
        /// The file name is always the 2nd line on the job file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string GetUWSFileName(string fileName) {
            //Get information from File.
            string jobpoolPath = _jobpoolPath;
            string tempfile = jobpoolPath + "\\" + fileName;
            string uwsFileName = string.Empty;
            bool isOpen = true;
            StreamReader fsr = null;

            //Opne StreamReader.
            while (isOpen) {
                try {
                    fsr = new StreamReader(tempfile);
                    isOpen = false;
                }
                catch {
                    isOpen = true;
                }
            }
            //Get file name from 3rd line.
            fsr.ReadLine();
            uwsFileName = Convert.ToString(fsr.ReadLine());

            fsr.Close();
            fsr.Dispose();
            return uwsFileName;
        }
        /// <summary>
        /// Get the system serial number from job file.
        /// The system serial is always the 3rd line of the file
        /// </summary>
        /// <param name="triggerFileName"></param>
        /// <returns></returns>
        public string GetSystemSerial(string triggerFileName) {
            string jobpoolPath = _jobpoolPath;
            string tempfile = jobpoolPath + "\\" + triggerFileName;
            string systemSerial = string.Empty;
            bool isOpen = true;
            StreamReader fsr = null;

            //Opne StreamReader.
            while (isOpen) {
                try {
                    fsr = new StreamReader(tempfile);
                    isOpen = false;
                }
                catch {
                    isOpen = true;
                }
            }
            fsr.ReadLine();
            fsr.ReadLine();
            systemSerial = Convert.ToString(fsr.ReadLine());

            fsr.Close();
            fsr.Dispose();
            return systemSerial;
        }
        /// <summary>
        /// Get the CPUInfo file name from job file.
        /// The system serial is always the 4th line of the file
        /// </summary>
        /// <param name="triggerFileName"></param>
        /// <returns></returns>
        public string GetCPUFileName(string triggerFileName) {
            string jobpoolPath = _jobpoolPath;
            string tempfile = jobpoolPath + "\\" + triggerFileName;
            string CPUFileName = string.Empty;
            bool isOpen = true;
            StreamReader fsr = null;

            try {
                //Opne StreamReader.
                while (isOpen) {
                    try {
                        fsr = new StreamReader(tempfile);
                        isOpen = false;
                    }
                    catch {
                        isOpen = true;
                    }
                }
                fsr.ReadLine();
                fsr.ReadLine();
                fsr.ReadLine();
                CPUFileName = Convert.ToString(fsr.ReadLine());

                fsr.Close();
                fsr.Dispose();
            }
            catch { }
            return CPUFileName;
        }

        public string GetOSSFileName(string triggerFileName) {
            string jobpoolPath = _jobpoolPath;
            string tempfile = jobpoolPath + "\\" + triggerFileName;
            string ossFileName = string.Empty;
            bool isOpen = true;
            StreamReader fsr = null;

            try {
                //Opne StreamReader.
                while (isOpen) {
                    try {
                        fsr = new StreamReader(tempfile);
                        isOpen = false;
                    }
                    catch {
                        isOpen = true;
                    }
                }
                fsr.ReadLine();
                fsr.ReadLine();
                fsr.ReadLine();
                fsr.ReadLine();
                if (fsr.Peek() != -1)
                    ossFileName = Convert.ToString(fsr.ReadLine());

                fsr.Close();
                fsr.Dispose();
            }
            catch {
            }
            return ossFileName;
        }

        public string GetDISCOPENFileName(string triggerFileName) {
            string jobpoolPath = _jobpoolPath;
            string tempfile = jobpoolPath + "\\" + triggerFileName;
            string DISCOPENFileName = string.Empty;
            bool isOpen = true;
            StreamReader fsr = null;

            try {
                //Opne StreamReader.
                while (isOpen) {
                    try {
                        fsr = new StreamReader(tempfile);
                        isOpen = false;
                    }
                    catch {
                        isOpen = true;
                    }
                }
                fsr.ReadLine();
                fsr.ReadLine();
                fsr.ReadLine();
                fsr.ReadLine();
                fsr.ReadLine();
                if (fsr.Peek() != -1)
                    DISCOPENFileName = Convert.ToString(fsr.ReadLine());

                fsr.Close();
                fsr.Dispose();
            }
            catch {
            }
            return DISCOPENFileName;
        }
    }
}
