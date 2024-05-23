using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ionic.Zip;

namespace RemoteAnalyst.BusinessLogic.BLL {
    public class Zipper {
        public void CreateZipFile(List<string> fileList, string saveLocation) {
            using (var zip = new ZipFile()) {
                zip.ParallelDeflateThreshold = -1;
                zip.UseZip64WhenSaving = Zip64Option.Always;
                foreach (var file in fileList) {
                    zip.AddFile(file, string.Empty); //Need to add string.empty to second parameter to make the file display on top of the zip layer.
                }
                zip.Save(saveLocation);
            }
        }
    }
}
