using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Ionic.Zip;
using RemoteAnalyst.BusinessLogic.Util;
using log4net;

namespace RemoteAnalyst.UWSLoader.SPAM.BLL {
    /// <summary>
    /// Zipper class provides zip functions to zip the UWS file.
    /// </summary>
    internal static class Zipper {
        internal static string CreateZipFile(string systemSerial, 
            string fileLocation, string zipFileName, ILog log) {
            string location = "";
            try {
                //Get folderName.
                string folderName = ConnectionString.ZIPLocation + systemSerial + "\\";
                //Get zip name.
                string zipName = zipFileName;

                #region Create ZIP file and insert installation files.

                //Check for directory.
                if (!Directory.Exists(folderName)) {
                    Directory.CreateDirectory(folderName);
                }

                //Delet file if exist.
                if (File.Exists(folderName + zipName)) {
                    File.Delete(folderName + zipName);
                }

                location = folderName + zipName;

                using (var zip = new ZipFile()) {
                    zip.ParallelDeflateThreshold = -1;
                    zip.UseZip64WhenSaving = Zip64Option.Always;
                    zip.AddFile(fileLocation, string.Empty); //Need to add string.empty to second parameter to make the file display on top of the zip layer.
                    zip.Save(location);
                }

                #endregion
            }
            catch (Exception ex) {
                log.Error("****************************************");
                log.ErrorFormat("Error: {0}", ex);
                location = "";
            }

            return location;
        }
    }
}