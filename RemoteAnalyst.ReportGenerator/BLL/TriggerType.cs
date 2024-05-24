using System.IO;

namespace RemoteAnalyst.ReportGenerator.BLL {
    internal class TriggerType {
        internal bool CheckTriggerType(string fileLocation) {
            bool isNewType = false;
            if (File.Exists(fileLocation)) {
                //Read each line and add it trendView.
                using (var reader = new StreamReader(fileLocation)) {
                    string type = reader.ReadLine();
                    if (type.StartsWith("QT") || type.StartsWith("DPA")) {
                        isNewType = true;
                    }
                }
            }

            return isNewType;
        }
    }
}