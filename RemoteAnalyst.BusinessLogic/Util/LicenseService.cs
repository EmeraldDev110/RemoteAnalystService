using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;

namespace RemoteAnalyst.BusinessLogic.Util {
    public static class LicenseService {
        private const string ProductName = "ProductName";
        private const string ProductVersion = "ProductVersion";
        private const string VersionNumber = "VersionNumber";
        private const string ProductIndentifierKey = "ProductIndentifierKey";
        private const string LocalAnalystKey = "LocalAnalyst";
        private const string PMCKey = "PMC";

        public static bool IsValidProductIndentifierKey(string connectionString) {
            var raInfo = new RAInfoService(connectionString);
            var productionName = raInfo.GetValueFor(ProductName);
            var productVersion = raInfo.GetValueFor(ProductVersion);
            var versionNumber = raInfo.GetValueFor(VersionNumber);
            var productIndentifierKey = raInfo.GetValueFor(ProductIndentifierKey);
            var verified = false;

            if (productionName.Length > 0 && productVersion.Length > 0 && versionNumber.Length > 0) {
                if (productIndentifierKey.Length > 0) {
                    var decrypt = new Decrypt();
                    var decrptedKeyTemp = decrypt.strDESDecrypt(productIndentifierKey);

                    var decrptedKeys = decrptedKeyTemp.Split('|');
                    if (productionName.Equals(decrptedKeys[0]) && productVersion.Equals(decrptedKeys[1]) && versionNumber.Equals(decrptedKeys[2]))
                        verified = true;
                }
            }
            return verified;
        }

        public static bool CheckRemoteAnalyst(string connectionString) {
            var raInfo = new RAInfoService(connectionString);
            var productName = raInfo.GetValueFor(ProductName);
            return productName.Equals(LocalAnalystKey);
        }

        public static bool CheckLocalAnalyst(string connectionString) {
            var raInfo = new RAInfoService(connectionString);
            var productionName = raInfo.GetValueFor(ProductName);
            return productionName.Equals(LocalAnalystKey);
        }

        public static bool CheckPMC(string connectionString) {
            var raInfo = new RAInfoService(connectionString);
            var productionName = raInfo.GetValueFor(ProductName);
            return productionName.Equals(PMCKey);
        }

        public static bool IsLocalAnalystOrPMC(string connectionString) {
            var raInfo = new RAInfoService(connectionString);
            var productionName = raInfo.GetValueFor(ProductName);
            return productionName.Equals(LocalAnalystKey) || productionName.Equals(PMCKey);
        }

        public static string GetProductName(string connectionString) {
            var raInfo = new RAInfoService(connectionString);
            var productName = raInfo.GetValueFor(ProductName);
            return productName;
        }
    }
}
