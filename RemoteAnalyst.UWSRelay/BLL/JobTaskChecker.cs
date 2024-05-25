using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using RemoteAnalyst.BusinessLogic.Util;

namespace RemoteAnalyst.UWSRelay.BLL {
    internal class JobTaskChecker {
        public void Timer_Elapsed(object source, ElapsedEventArgs e) {
            CheckTask();
        }

        public void CheckTask() {
            string systemLocation = ConnectionString.ServerPath;
            var folder = new DirectoryInfo(systemLocation);

            if (Directory.Exists(folder.FullName + "\\Tasks")) {
                var measureDic = new DirectoryInfo(folder.FullName + "\\Tasks");
                foreach (var file in measureDic.GetFiles().Where(x => x.CreationTime < DateTime.Now.AddMinutes(-600))) {
                    var archiveThread = new Thread(() => SendUWSFile(file));
                    archiveThread.IsBackground = true;
                    archiveThread.Start();
                }
            }
        }

        private void SendUWSFile(FileInfo file) {
            var systemSerial = "";
            var fileName = "";

            using (var reader = new StreamReader(file.FullName)) {
                while (!reader.EndOfStream) {
                    var line = reader.ReadLine();
                    var lineString = line.Split('=');
                    if (lineString[0].Equals("serial_number")) systemSerial = lineString[1];
                    else if (lineString[0].Equals("fetch_file")) fileName = lineString[1];
                }
            }

            if (systemSerial.Length > 0 && fileName.Length > 0) {
                //Delete the task file.
                file.Delete();

                string systemLocation = ConnectionString.FTPSystemLocation;
                var uwsFilePath = systemLocation + systemSerial + "\\UploadFolder\\" + fileName;

                var fileInfo = new FileInfo(uwsFilePath);
                var jobMeasure = new JobMeasures();
                jobMeasure.SendMeasureFile(systemSerial, fileInfo, true);
            }
        }
    }
}
