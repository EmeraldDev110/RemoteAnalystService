using System;
using System.Collections.Generic;

namespace RemoteAnalyst.BusinessLogic.Util {
    public class ConnectionString {
        public static string ConnectionStringDB { get; set; }
        public static string ConnectionStringTrend { get; set; } 
        public static string ConnectionStringSPAM { get; set; }
        public static string ConnectionStringComparative { get; set; }
		public static string TempDatabaseConnectionString { get; set; }
		public static string MasterDatabaseConnectionString { get; set; }

        public static string ServerPath { get; set; }
        public static string EmailServer { get; set; }
        public static string WebSite { get; set; }
        public static string SupportEmail { get; set; }
        public static string MailTo { get; set; }
        public static string WebSiteAddress { get; set; }
        public static string SaleEmail { get; set; }
        public static string WatchFolder { get; set; }
        public static int EmailPort { get; set; }
        public static string EmailUser { get; set; }
        public static string EmailPassword { get; set; }

        public static string JobWatcherMeasure { get; set; }
        public static string SystemLocation { get; set; }
        public static bool EmailAuthentication { get; set; }
        public static string AdvisorEmail { get; set; }

        public static string MaxDISCOPENQueue { get; set; }
        //SQS Queues
        public static string SQSError { get; set; }
        public static string SQSLoad { get; set; }
        public static string SQSMultiLoad { get; set; }
        public static string SQSReport { get; set; }
		public static string SQSRdsMove { get; set; }
        public static string SQSManagement { get; set; }
        public static string SQSReportOptimizer { get; set; }

        //S3 Bucket Name.
        public static string S3ErrorLog { get; set; }
        public static string S3Reports { get; set; }
        public static string S3WorkSpace { get; set; }
        public static string S3FTP { get; set; }
        public static string S3UWS { get; set; }

        //EC2 Instance ID.
        public static string PrimaryEC2 { get; set; }

        public static string ZIPLocation { get; set; }
        
        //Amazon Glacier
        public static string VaultName { get; set; }

        //Linked DB addresses
        public static string MainDBIPAddress { get; set; }

        public static string SNSProcessWatch { get; set; }

        public static bool IsLocalAnalyst { get; set; }

        public static string NetworkStorageLocation { get; set; }

        public static bool EmailIsSSL { get; set; } = true;
        public static string DatabasePrefix { get; set; }
		public static string SNSStgRDSLoaderARN { get; set; }
        public static string MailGunSendAPIKey { get; set; }
        public static string MailGunSendDomain { get; set; }
		
		public static int UploadQueue { get; set; }
		public static string SNSLambdaLoader { get; set; }
		public static bool IsProcessDirectlySystem { get; set; }

        public static Dictionary<string, string> Vols = new Dictionary<string, string>();

        public static int VolumeOrder { get; set; }

        public static Dictionary<string, string> MeasureList = new Dictionary<string, string>();
        
        public static int CurrentUploadCount = 0;

        public static int TaskCounter { get; set; }

        public static string SNSProdTriggerReportARN { get; set; }

        public static string SQSBatch { get; set; }
        public static int EC2TerminateAllowTime { get; set; }

        public static int MaxRetries { get; set; }
        
        public static int RetryInterval { get; set; }

        public static double MaxFileWaitTime { get; set; }

        public static Dictionary<string, int> FTPServers = new Dictionary<string, int>();

        public static string FTPLogon { get; set; }

        public static string FTPPassword { get; set; }
        public static string S3RAFTP { get; set; }
        public static string FTPSystemLocation { get; set; }
    }
}