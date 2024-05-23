using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Web.UI.DataVisualization.Charting;
using System.Xml;
using RemoteAnalyst.AWS.S3;
using RemoteAnalyst.BusinessLogic.Infrastructure;

namespace RemoteAnalyst.BusinessLogic.Util {

    /// <summary>
    /// ReadXML class is a utility class that reads the 'XMLVALUE.xml' file to get the connection strings and system configuration information.
    /// </summary>
    public class ReadXML {
        /// <summary>
        /// Read the config file.
        /// Assign the values to Connectiong class.
        /// </summary>
        public static void ImportDataFromXML() {
            AnsibleVaultDecoder ansibleVaultDecoder = new AnsibleVaultDecoder();
#if (!DEBUG)
            string filePath = AppContext.BaseDirectory; // e.g. C:\\Program Files (x86)\\Local Analyst\\Local Analyst Services\\Dynamic Report Generator\\
#else
            string filePath = "C:\\Users\\Idel3\\source\\repos\\RemoteAnalyst Service Git\\";
#endif
            filePath = filePath.Substring(0, filePath.LastIndexOf("\\"));
            filePath = filePath.Substring(0, filePath.LastIndexOf("\\"));
            filePath += "\\LA-Config";
            if (!File.Exists(filePath + "\\encryptedVaultPass.xml")) throw new Exception("Cannot find encryptedVaultPass.xml file.");
            string encryptedVaultPass = "";
            var decrypt = new Decrypt();
            using (var stm = ArgumentUtil.GetInputStream(filePath + "\\encryptedVaultPass.xml"))
            using (var sr = new StreamReader(stm, Encoding.UTF8))
            {
                encryptedVaultPass = sr.ReadToEnd();
            }

            string vaultPass = decrypt.strDESDecrypt(encryptedVaultPass);
            string xml = ansibleVaultDecoder.decodeXmlFile(filePath, vaultPass);
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            // Create an XmlNamespaceManager to resolve the default namespace.
            var nsmgr = new XmlNamespaceManager(doc.NameTable);

            // Select and display all book titles.
            XmlElement root = doc.DocumentElement;
            nsmgr.AddNamespace("ra", "urn:RemoteAnalyst-schema");
            XmlNodeList nodeList = root.SelectNodes("/ra:RemoteAnalyst/ra:Element", nsmgr);
            if (nodeList.Count > 0) { // If XML does not have RA schema, then assume it is LA
                ParseRemoteAnalystConfiguration(nodeList);
            }
            else
            {
                nsmgr.AddNamespace("ra", "urn:PMC-schema");
                nodeList = root.SelectNodes("/ra:PMC/ra:Element", nsmgr);
                //ParseLocalAnalystConfiguration(nodeList);
                ParseRemoteAnalystConfiguration(nodeList);
            }
        }


        /// <summary>
        /// ImportDataFromXML reads XMLValue.xml and populate static values.
        /// </summary>
        public static void ImportDataFromXMLS3() {
            var s3 = new AmazonS3(ConfigurationManager.AppSettings["S3XML"]);
            //TextReader xmlString = s3.ReadS3StreamAsString(ConfigurationManager.AppSettings["S3XMLName"]);
            string xmlString = s3.ReadS3StreamAsString(ConfigurationManager.AppSettings["S3XMLName"]);

            var doc = new XmlDocument();
            doc.LoadXml(xmlString);
            //doc.Load(xmlString);
            // Create an XmlNamespaceManager to resolve the default namespace.
            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("ra", "urn:RemoteAnalyst-schema");

            // Select and display all book titles.
            XmlElement root = doc.DocumentElement;
            XmlNodeList nodeList = root.SelectNodes("/ra:RemoteAnalyst/ra:Element", nsmgr);
            ParseRemoteAnalystConfiguration(nodeList);
        }

        internal static void ParseRemoteAnalystConfiguration(XmlNodeList nodeList)
        {
            for (int x = 0; x < nodeList.Count; x++)
            {
                string id = nodeList.Item(x).ChildNodes[0].InnerText;
                string values = nodeList.Item(x).ChildNodes[1].InnerText;

                switch (id)
                {
                    case "ConnectionStringDB":
                        ConnectionString.ConnectionStringDB = values;
                        break;
                    case "ConnectionStringTrend":
                        ConnectionString.ConnectionStringTrend = values;
                        break;
                    case "ConnectionStringSPAM":
                        ConnectionString.ConnectionStringSPAM = values;
                        break;
                    // Saurabh 06/09
                    case "ConnectionStringComparative":
                        ConnectionString.ConnectionStringComparative = values;
                        break;
                    case "ServerPath":
                        values = !values.EndsWith("\\") ? values + "\\" : values;
                        ConnectionString.ServerPath = values;
                        ConnectionString.SystemLocation = ConnectionString.ServerPath + "Systems//";
                        ConnectionString.ZIPLocation = ConnectionString.ServerPath + "UWS//";
                        break;
                    case "FTPSystemLocation":
                        values = !values.EndsWith("/") ? values + "/" : values;
                        ConnectionString.FTPSystemLocation = values;
                        break;
                    case "EmailServer":
                        ConnectionString.EmailServer = values;
                        break;
                    case "WebSite":
                        ConnectionString.WebSite = values;
                        break;
                    case "AdvisorEmail":
                        ConnectionString.AdvisorEmail = values;
                        break;
                    case "SupportEmail":
                        ConnectionString.SupportEmail = values;
                        break;
                    case "SaleEmail":
                        ConnectionString.SaleEmail = values;
                        break;
                    case "MailTo":
                        ConnectionString.MailTo = values;
                        break;
                    case "WebSiteAddress":
                        ConnectionString.WebSite = values;
                        ConnectionString.WebSiteAddress = values;
                        break;
                    case "MainDBIPAddress":
                        ConnectionString.MainDBIPAddress = values;
                        break;
                    case "WatchFolder":
                        ConnectionString.WatchFolder = values;
                        break;
                    case "EmailPort":
                        ConnectionString.EmailPort = Convert.ToInt32(values);
                        break;
                    case "EmailUser":
                        ConnectionString.EmailUser = values;
                        break;
                    case "EmailPassword":
                        ConnectionString.EmailPassword = values;
                        break;
                    case "EmailIsSSL":
                        ConnectionString.EmailIsSSL = Convert.ToBoolean(values);
                        break;
                    case "WatchFolderMeasure":
                        ConnectionString.JobWatcherMeasure = values;
                        break;
                    case "EmailAuthentication":
                        ConnectionString.EmailAuthentication = Convert.ToBoolean(values);
                        break;
                    case "SQSError":
                        ConnectionString.SQSError = values;
                        break;
                    case "SQSLoad":
                        ConnectionString.SQSLoad = values;
                        break;
                    case "SQSBatch":
                        ConnectionString.SQSBatch = values;
                        break;
                    case "SQSMultiLoad":
                        ConnectionString.SQSMultiLoad = values;
                        break;
                    case "SQSRdsMove":
                        ConnectionString.SQSRdsMove = values;
                        break;
                    case "SQSReport":
                        ConnectionString.SQSReport = values;
                        break;
                    case "SQSManagement":
                        ConnectionString.SQSManagement = values;
                        break;
                    case "S3ErrorLog":
                        ConnectionString.S3ErrorLog = values;
                        break;
                    case "S3Reports":
                        ConnectionString.S3Reports = values;
                        break;
                    case "S3WorkSpace":
                        ConnectionString.S3WorkSpace = values;
                        break;
                    case "S3FTP":
                        ConnectionString.S3FTP = values;
                        break;
                    case "S3UWS":
                        ConnectionString.S3UWS = values;
                        break;
                    case "PrimaryEC2UWSLoader":
                        ConnectionString.PrimaryEC2 = values;
                        break;
                    case "VaultName":
                        ConnectionString.VaultName = values;
                        break;
                    case "SNSProcessWatch":
                        ConnectionString.SNSProcessWatch = values;
                        break;
                    case "IsLocalAnalyst":
                        ConnectionString.IsLocalAnalyst = Convert.ToBoolean(values);
                        break;
                    case "NetworkStorageLocation":
                        values = !values.EndsWith("\\") ? values + "\\" : values;
                        //values = !values.StartsWith("\\") ? "\\" + values : values;
                        //values = !values.StartsWith("\\\\") ? "\\" + values : values;
                        ConnectionString.NetworkStorageLocation = values;
                        break;
                    case "DatabasePrefix":
                        ConnectionString.DatabasePrefix = values;
                        break;
                    case "SNSStgRDSLoaderARN":
                        ConnectionString.SNSStgRDSLoaderARN = values;
                        break;
                    case "MailGunSendAPIKey":
                        ConnectionString.MailGunSendAPIKey = values;
                        break;
                    case "MailGunSendDomain":
                        ConnectionString.MailGunSendDomain = values;
                        break;
                    case "SNSProdTriggerReportARN":
                        ConnectionString.SNSProdTriggerReportARN = values;
                        break;
                    case "EC2TerminateAllowTime":
                        ConnectionString.EC2TerminateAllowTime = Convert.ToInt32(values);
                        break;
                    case "SQSReportOptimizer":
                        ConnectionString.SQSReportOptimizer = values;
                        break;
                    case "SNSLambdaLoader":
                        ConnectionString.SNSLambdaLoader = values;
                        break;
                    case "IsVISA":
                        ConnectionString.IsProcessDirectlySystem = Convert.ToBoolean(values);
                        break;
                    case "S3RAFTP":
                        ConnectionString.S3RAFTP = values;
                        break;
                }
            }
        }
    
        internal static void ParseLocalAnalystConfiguration(XmlNodeList nodeList)
        {
            var encrypt = new Decrypt();
            var val = "";
            for (int x = 0; x < nodeList.Count; x++)
            {
                string id = nodeList.Item(x).ChildNodes[0].InnerText;
                string values = nodeList.Item(x).ChildNodes[1].InnerText;

                switch (id)
                {
                    case "ConnectionStringDB":
                        ConnectionString.ConnectionStringDB = encrypt.strDESDecrypt(values);
                        break;
                    case "ConnectionStringTrend":
                        ConnectionString.ConnectionStringTrend = encrypt.strDESDecrypt(values);
                        break;
                    case "ConnectionStringSPAM":
                        ConnectionString.ConnectionStringSPAM = encrypt.strDESDecrypt(values);
                        break;
                    // Saurabh 06/09
                    case "ConnectionStringComparative":
                        ConnectionString.ConnectionStringComparative = encrypt.strDESDecrypt(values);
                        break;
                    case "ServerPath":
                        val = encrypt.strDESDecrypt(values);
                        val = !val.EndsWith("\\") ? val + "\\" : val;
                        ConnectionString.ServerPath = val;
                        ConnectionString.SystemLocation = ConnectionString.ServerPath + "Systems//";
                        ConnectionString.ZIPLocation = ConnectionString.ServerPath + "UWS//";
                        break;
                    case "FTPSystemLocation":
                        val = encrypt.strDESDecrypt(values);
                        val = !val.EndsWith("/") ? val + "/" : val;
                        ConnectionString.FTPSystemLocation = val;
                        break;
                    case "EmailServer":
                        ConnectionString.EmailServer = encrypt.strDESDecrypt(values);
                        break;
                    case "WebSite":
                        ConnectionString.WebSite = encrypt.strDESDecrypt(values);
                        break;
                    case "SupportEmail":
                        ConnectionString.SupportEmail = encrypt.strDESDecrypt(values);
                        break;
                    case "MailTo":
                        ConnectionString.MailTo = encrypt.strDESDecrypt(values);
                        break;
                    case "WebSiteAddress":
                        ConnectionString.WebSite = encrypt.strDESDecrypt(values);
                        ConnectionString.WebSiteAddress = encrypt.strDESDecrypt(values);
                        break;
                    case "MainDBIPAddress":
                        ConnectionString.MainDBIPAddress = encrypt.strDESDecrypt(values);
                        break;
                    case "SaleEmail":
                        ConnectionString.SaleEmail = encrypt.strDESDecrypt(values);
                        break;
                    case "WatchFolder":
                        ConnectionString.WatchFolder = encrypt.strDESDecrypt(values);
                        break;
                    case "EmailPort":
                        ConnectionString.EmailPort = Convert.ToInt32(encrypt.strDESDecrypt(values));
                        break;
                    case "EmailUser":
                        ConnectionString.EmailUser = encrypt.strDESDecrypt(values);
                        break;
                    case "EmailPassword":
                        ConnectionString.EmailPassword = encrypt.strDESDecrypt(values);
                        break;
                    case "EmailIsSSL":
                        ConnectionString.EmailIsSSL = Convert.ToBoolean(encrypt.strDESDecrypt(values));
                        break;
                    case "WatchFolderMeasure":
                        ConnectionString.JobWatcherMeasure = encrypt.strDESDecrypt(values);
                        break;
                    case "EmailAuthentication":
                        ConnectionString.EmailAuthentication = Convert.ToBoolean(encrypt.strDESDecrypt(values));
                        break;
                    case "AdvisorEmail":
                        ConnectionString.AdvisorEmail = encrypt.strDESDecrypt(values);
                        break;
                    case "UploadQueue":
                        ConnectionString.UploadQueue = Convert.ToInt32(encrypt.strDESDecrypt(values));
                        break;
                    case "IsLocalAnalyst":
                        ConnectionString.IsLocalAnalyst = Convert.ToBoolean(encrypt.strDESDecrypt(values));
                        break;
                    case "NetworkStorageLocation":
                        val = encrypt.strDESDecrypt(values);
                        val = !val.EndsWith("\\") ? val + "\\" : val;
                        //val = !val.StartsWith("\\") ? "\\" + val : val;
                        //val = !val.StartsWith("\\\\") ? "\\" + val : val;
                        ConnectionString.NetworkStorageLocation = val;
                        break;
                    case "VaultName":
                        ConnectionString.VaultName = encrypt.strDESDecrypt(values);
                        break;
                    case "DatabasePrefix":
                        ConnectionString.DatabasePrefix = encrypt.strDESDecrypt(values);
                        break;
                    case "MailGunSendAPIKey":
                        ConnectionString.MailGunSendAPIKey = values;
                        break;
                    case "MailGunSendDomain":
                        ConnectionString.MailGunSendDomain = values;
                        break;
                    case "MaxRetries":
                        ConnectionString.MaxRetries = Convert.ToInt32(values);
                        break;
                    case "RetryInterval":
                        ConnectionString.RetryInterval = Convert.ToInt32(values);
                        break;
                    case "MaxFileWaitTime":
                        ConnectionString.MaxFileWaitTime = Convert.ToInt32(values);
                        break;
                    case "FTPLogon":
                        ConnectionString.FTPLogon = values;
                        break;
                    case "FTPPassword":
                        ConnectionString.FTPPassword = values;
                        break;
                }
            }
        }
    }
}