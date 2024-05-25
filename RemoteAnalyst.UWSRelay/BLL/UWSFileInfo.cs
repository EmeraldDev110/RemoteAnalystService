using System.IO;
using RemoteAnalyst.BusinessLogic.Enums;

namespace RemoteAnalyst.UWSRelay.BLL {
    /// <summary>
    /// UWSFileInfo is an utility class that gets the file information from the UWS data file
    /// </summary>
    public class UWSFileInfo {
        /// <summary>
        /// Get the file type of the UWS file.
        /// </summary>
        /// <param name="uwsName"> Full path of the UWS data file. </param>
        /// <returns> Return a string value which is the type of the UWS data file.</returns>
        public string GetFileType(string uwsName) {
            string type = string.Empty;
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
        public short UwsFileVersionNew(string uwsName) {
            using (var sr = new StreamReader(uwsName)) {
                string line = sr.ReadLine();
                if (line.IndexOf("RAP P2C2E2 2003*") == -1) {
                    line += sr.ReadLine();
                }

                if (line.Contains("02262009")) {
                    //New SPAM.
                    return 3;
                }
                if (line.Contains("07032007")) {
                    //SPAM.
                    return 2;
                }
                //old ra
                return 1;
            }
        }

        public UWS.Types UwsFileVersionNewUWSRelay(string uwsName) {
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
    }
}
