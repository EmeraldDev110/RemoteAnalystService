using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using McMaster.Extensions.CommandLineUtils;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.BusinessLogic.Util;
using System.Xml;

namespace AnalystManager
{
    [Command("encryptConnStr", "encrypt string")]
    class EncryptConnStringCommand
    {
        [Option("-h|--hostname", "Hostname", CommandOptionType.SingleValue)]
        public static string Hostname { get; set; }
        [Option("-p|--port", "Port", CommandOptionType.SingleValue)]
        public static string Port { get; set; }
        [Option("-d|--database", "Database", CommandOptionType.SingleValue)]
        public static string Database { get; set; }
        [Option("-u|--username", "Username", CommandOptionType.SingleValue)]
        public static string Username { get; set; }
        [Option("-pw|--password", "Password", CommandOptionType.SingleValue)]
        public static string Password { get; set; }
        public int OnExecute()
        {
            var decrypt = new Decrypt();
            try
            {
                string encryptedString = decrypt.strDESEncrypt(String.Format("SERVER={0};PORT={1};DATABASE={2};UID={3};PASSWORD={4};", Hostname, Port, Database, Username, Password));

                Console.Write(encryptedString);
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return 1;
            }
        }
    }
    [Command("encryptXML", "Encrypt XMLValue.xml")]
    class EncryptXMLCommand
    {
        [Option("-i|--input", "input string", CommandOptionType.SingleValue)]
        public static string InputString { get; set; }
        [Option("-o|--output", "output file path", CommandOptionType.SingleValue)]
        public static string OutputPath { get; set; }
        [Option("-p|--password", "encryption password", CommandOptionType.SingleValue)]
        public static string Password { get; set; }
        public int OnExecute()
        {
            AnsibleVaultDecoder ansibleVaultDecoder = new AnsibleVaultDecoder();
            try
            {
                ansibleVaultDecoder.encodeXmlFile(OutputPath, InputString, Password);

                // Create encryptedVaultPass file
                var decrypt = new Decrypt();
                string vaultPass = decrypt.strDESEncrypt(Password);

                var writer = new StreamWriter(OutputPath + "\\encryptedVaultPass.xml");
                writer.Write(vaultPass);
                writer.Flush();
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return 1;
            }
        }
    }
    [Command("decode", "decode ansible vault data")]
    class DecodeCommand
    {
        [Option("-i|--input", "encrypted XMLValue.xml path", CommandOptionType.SingleValue)]
        public static string InputPath { get; set; }
        [Option("-p|--password", "encryption password", CommandOptionType.SingleValue)]
        public static string Password { get; set; }
        public int OnExecute()
        {
            try
            {
                AnsibleVaultDecoder ansibleVaultDecoder = new AnsibleVaultDecoder();
                string xml = ansibleVaultDecoder.decodeXmlFile(InputPath, Password);
                var doc = new XmlDocument();
                doc.LoadXml(xml);
                Console.Out.WriteLine(doc.InnerXml);
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return 1;
            }
        }
    }
    [Command("updateXMLValue", "update XMLValue.xml for the new version of Local Analyst")]
    class UpdateXMLValueCommand
    {
        [Option("-i|--input", "path to old XMLValue.xml file", CommandOptionType.SingleValue)]
        public static string InputPath { get; set; }
        [Option("-o|--output", "path to new LA-Config directory", CommandOptionType.SingleValue)]
        public static string OutputPath { get; set; }
        [Option("-p|--password", "encryption password", CommandOptionType.SingleValue)]
        public static string Password { get; set; }
        public int OnExecute()
        {
            try
            {
                if (!File.Exists(InputPath)) throw new Exception("File does not exist.");
                AnsibleVaultDecoder ansibleVaultDecoder = new AnsibleVaultDecoder();
                ansibleVaultDecoder.ConvertXMLValueToAnsibleVault(InputPath, OutputPath, Password);
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return 1;
            }
        }
    }
    [Subcommand(typeof(EncryptXMLCommand), typeof(DecodeCommand), typeof(EncryptConnStringCommand), typeof(UpdateXMLValueCommand))]
    [Command("analyst-manager")]
    [VersionOptionFromMember(MemberName = "VersionString")]
    class MainCommand
    {
        public string VersionString => $"dotnet-anv {typeof(Program).Assembly.GetName().Version.ToString()}";
    }
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            return await CommandLineApplication.ExecuteAsync<MainCommand>(args).ConfigureAwait(false);
        }
    }
}