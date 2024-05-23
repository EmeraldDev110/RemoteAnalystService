using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using RemoteAnalyst.BusinessLogic.Infrastructure;

namespace RemoteAnalyst.BusinessLogic.Util
{
    public class AnsibleVaultDecoder
    {
        public string decodeXmlFile(string filePath, string password)
        {
            if (!File.Exists(filePath + "\\XMLValue.xml")) throw new Exception("Cannot find XMLValue.xml file.");
            var codec = new AnsibleVaultCodec();
            string xml = "";

            using (var stm = ArgumentUtil.GetInputStream(filePath + "\\XMLValue.xml"))
            using (var sr = new StreamReader(stm, Encoding.UTF8))
            {
                xml = codec.Decode(sr, ArgumentUtil.GetPassword(password));
            }
            return xml;
        }
        public void encodeXmlFile(string filePath, string xml, string vaultPass)
        {
            xml = System.Xml.Linq.XElement.Parse(xml).ToString();
            var codec = new AnsibleVaultCodec();

            using (var stm = GenerateStreamFromString(xml))
            using (var ostm = ArgumentUtil.GetOutputStream(filePath + "\\XMLValue.xml", true))
            {
                using (var sw = new StreamWriter(ostm, new UTF8Encoding(false)))
                {
                    var lst = new List<byte>();
                    var buf = new byte[4096];
                    while (true)
                    {
                        var bytesread = stm.Read(buf, 0, 4096);
                        if (bytesread <= 0)
                        {
                            break;
                        }
                        lst.AddRange(buf.Take(bytesread));
                    }
                    var eolString = ArgumentUtil.EolOptionToEolString("");
                    if (eolString != null)
                    {
                        sw.NewLine = eolString;
                    }
                    codec.Encode(lst.ToArray(), ArgumentUtil.GetPassword(vaultPass), CreateSalt(), sw, "", 80);
                }
            }
            byte[] CreateSalt()
            {
                using (var rng = RandomNumberGenerator.Create())
                {
                    var data = new byte[32];
                    rng.GetBytes(data);
                    return data;
                }
            }
            Stream GenerateStreamFromString(string s)
            {
                var stream = new MemoryStream();
                var writer = new StreamWriter(stream);
                writer.Write(s);
                writer.Flush();
                stream.Position = 0;
                return stream;
            }
        }
        public void ConvertXMLValueToAnsibleVault(string filePath, string outputPath, string password)
        {
            string val = "";
            var decrypt = new Decrypt();


            // Get FTP System location
            string ftpLocation = "";
            string ftpSystemLocation = "";
            string xml = "";
            var doc = new XmlDocument();
            XmlNodeList nodeList;
            XmlElement root;
            if (Directory.Exists("C:\\LA-Config"))
            {
                if (File.Exists("C:\\LA-Config\\XMLValue-MySQL.xml"))
                    xml = File.ReadAllText("C:\\LA-Config\\XMLValue-MySQL.xml");
            }
            else if (Directory.Exists("C:\\PMC-Config"))
            {
                if (File.Exists("C:\\PMC-Config\\XMLValue-MySQL.xml"))
                    xml = File.ReadAllText("C:\\PMC-Config\\XMLValue-MySQL.xml");
            }

            if (!string.IsNullOrEmpty(xml))
            {
                doc.LoadXml(xml);
                root = doc.DocumentElement;
                var nsmgr1 = new XmlNamespaceManager(doc.NameTable);

                nsmgr1.AddNamespace("ra", "urn:PMC-schema");
                nodeList = root.SelectNodes("/ra:PMC/ra:Element", nsmgr1);
                foreach (XmlNode aNode in nodeList)
                {
                    var id = aNode.ChildNodes[0].InnerText;
                    var values = aNode.ChildNodes[1].InnerText;
                    if (id == "ServerPath")
                    {
                        ftpLocation = decrypt.strDESDecrypt(values);
                    }
                    else if (id == "SystemLocation")
                    {
                        ftpSystemLocation = decrypt.strDESDecrypt(values);
                    }
                }
            }

            xml = File.ReadAllText(filePath);

            doc = new XmlDocument();
            doc.LoadXml(xml);
            root = doc.DocumentElement;

            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("ra", "urn:PMC-schema");
            nodeList = root.SelectNodes("/ra:PMC/ra:Element", nsmgr);
            foreach (XmlNode aNode in nodeList)
            {
                var id = aNode.ChildNodes[0].InnerText;
                var values = aNode.ChildNodes[1].InnerText;

                switch (id)
                {
                    case "ConnectionStringDB":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "ConnectionStringTrend":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "ConnectionStringSPAM":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "ConnectionStringComparative":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "ServerPath":
                        val = decrypt.strDESDecrypt(values);
                        val = !val.EndsWith("\\") ? val + "\\" : val;
                        aNode.ChildNodes[1].InnerText = val;
                        XmlNode aNodeClone = aNode.Clone();
                        aNodeClone.ChildNodes[0].InnerText = "FTPSystemLocation";
                        aNodeClone.ChildNodes[1].InnerText = !string.IsNullOrEmpty(ftpSystemLocation) ? ftpSystemLocation : aNodeClone.ChildNodes[1].InnerText + @"FTP\Systems\";
                        XmlNode aNodeClone1 = aNode.Clone();
                        aNodeClone1.ChildNodes[0].InnerText = "FTPLocation";
                        aNodeClone1.ChildNodes[1].InnerText = !string.IsNullOrEmpty(ftpLocation) ? ftpLocation : aNodeClone1.ChildNodes[1].InnerText + @"FTP\";
                        aNode.ParentNode.AppendChild(aNodeClone1);
                        break;
                    case "FTPSystemLocation":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "EmailServer":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "WebSite":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "SupportEmail":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "MailTo":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "WebSiteAddress":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "MainDBIPAddress":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "SaleEmail":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "WatchFolder":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "EmailPort":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "EmailUser":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "EmailPassword":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "EmailIsSSL":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "WatchFolderMeasure":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "EmailAuthentication":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "AdvisorEmail":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "UploadQueue":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "IsLocalAnalyst":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "NetworkStorageLocation":
                        val = decrypt.strDESDecrypt(values);
                        val = !val.EndsWith("\\") ? val + "\\" : val;
                        //val = !val.StartsWith("\\") ? "\\" + val : val;
                        //val = !val.StartsWith("\\\\") ? "\\" + val : val;

                        aNode.ChildNodes[1].InnerText = val;
                        break;
                    case "VaultName":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "DatabasePrefix":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "LogPath":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "SystemLocation":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "ZIPLocation":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "StrIP":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "IsEmailUseBefore220Welcome":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "MaxTrendQueueKey":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "MaxStorageQueueKey":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "MaxDPAQueueKey":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "MaxQTQueueKey":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "MaxDISCOPENQueueKey":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "MaxPathwayQueueKey":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "MaxSCMQueueKey":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "MaxQNMQueueKey":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "MaxMonthliesAndWeekliesQueueKey":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "AWS":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "DatabaseSaveLocation":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "TempDatabaseConnectionString":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "MasterDatabaseConnectionString":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "IsPerformanceManagement":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "SystemCount":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "SystemName_0":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "SystemSerial_0":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        XmlNode aNodeClone2 = aNode.Clone();
                        aNodeClone2.ChildNodes[0].InnerText = "SystemSerial";
                        aNode.ParentNode.AppendChild(aNodeClone2);
                        break;
                    case "SystemTimeZone_0":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "SystemMeasFH_0":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "SystemLocation_0":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "NonStopIP_0":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "MonitorPort_0":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "StoreVolume_0":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "FTPUserName_0":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "FTPPassword_0":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "FTPPort_0":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "FTPMeasFhVolume_0":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "MeasFhSubVolume_0":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "UserFirstName":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "UserLastName":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "UserEmailAddress":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "UserPassword":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "CompanyName":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "CompanyAddress1":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "CompanyAddress2":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "CompanyCity":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "CompanyState":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "CompanyPostalCode":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "CompanyCountry":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                    case "CompanyContactNumber":
                        aNode.ChildNodes[1].InnerText = decrypt.strDESDecrypt(values);
                        break;
                }
            }
            string decryptedXml = System.Xml.Linq.XElement.Parse(doc.InnerXml).ToString();
            //Console.Out.WriteLine(decryptedXml);
            encodeXmlFile(outputPath, decryptedXml, password);

            // Create encryptedVaultPass file
            string vaultPass = decrypt.strDESEncrypt(password);

            var writer = new StreamWriter(outputPath + "\\encryptedVaultPass.xml");
            writer.Write(vaultPass);
            writer.Flush();
        }
    }
}
