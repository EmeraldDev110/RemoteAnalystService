using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteAnalyst.Trigger.JobPool {
    public class FileUtil {
        private string WatchFolder;

        public FileUtil(string watchFolder) {
            WatchFolder = watchFolder;
        }

        /// <summary>
        /// Get the data file name from job file
        /// The file name is always the 2nd line on the job file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string GetUWSFileName(string fileName) {
            //Get information from File.
            string tempfile = WatchFolder + "\\" + fileName;
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
            string tempfile = WatchFolder + "\\" + triggerFileName;
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
        internal string GetCPUFileName(string triggerFileName) {
            string tempfile = WatchFolder + "\\" + triggerFileName;
            string CPUFileName = string.Empty;
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
            fsr.ReadLine();
            if (fsr.Peek() != -1)
                CPUFileName = Convert.ToString(fsr.ReadLine());

            fsr.Close();
            fsr.Dispose();
            return CPUFileName;
        }

        /// <summary>
        /// Get the OSS file name from job file.
        /// The system serial is always the 5th line of the file
        /// </summary>
        /// <param name="triggerFileName"></param>
        /// <returns></returns>
        internal string GetOSSJRNLName(string triggerFileName) {
            string tempfile = WatchFolder + "\\" + triggerFileName;
            string ossFileName = string.Empty;
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
            fsr.ReadLine();
            fsr.ReadLine();
            if(fsr.Peek() != -1)
                ossFileName = Convert.ToString(fsr.ReadLine());

            fsr.Close();
            fsr.Dispose();
            return ossFileName;
        }

        public string GetDISCOPENFileName(string triggerFileName)
        {
            string jobpoolPath = WatchFolder;
            string tempfile = jobpoolPath + "\\" + triggerFileName;
            string DISCOPENFileName = string.Empty;
            bool isOpen = true;
            StreamReader fsr = null;

            //Opne StreamReader.
            while (isOpen)
            {
                try
                {
                    fsr = new StreamReader(tempfile);
                    isOpen = false;
                }
                catch
                {
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
            return DISCOPENFileName;
        }
    }
}
