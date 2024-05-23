using System;
using System.Collections.Generic;
using System.Data;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using RemoteAnalyst.AWS.S3;
using RemoteAnalyst.AWS.RDS;
using RemoteAnalyst.AWS.Infrastructure;
using log4net;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices
{
    public class StorageAnalysisService {
        private readonly string _connectionString;
        private readonly StorageAnalysis _storageAnalysis;
		private readonly string _s3ReportBucketName;
		private readonly string _s3FTPBucketName;
		private readonly string _s3UWSBucketName;
        private ILog _log;

        public StorageAnalysisService(string connectionString, ILog log) {
            _connectionString = connectionString;
            _log = log;
            _storageAnalysis = new StorageAnalysis(_connectionString);
        }

		public StorageAnalysisService(string connectionString, ILog log,
            string s3ReportBucketName, string s3FTPBucketName, string s3UWSBucketName) {
			_connectionString = connectionString;
            _log = log;
            _storageAnalysis = new StorageAnalysis(_connectionString);
			_s3ReportBucketName = s3ReportBucketName;
			_s3FTPBucketName = s3FTPBucketName;
			_s3UWSBucketName = s3UWSBucketName;
		}

		public List<TempStorageTableView> GetStorageSizes() {
            var trendTable = _storageAnalysis.GetTrendSize();
            var dbTable = _storageAnalysis.GetDBSize();
            var list = new List<TempStorageTableView>();
            foreach (DataRow trendRow in trendTable.Rows) {
                try {
                    var sizeAnalyses = new TempStorageTableView {
                        SystemSerial = trendRow["SystemSerial"].ToString(),
                        TrendSizeInMB = int.Parse(trendRow["TotalSize"].ToString())
                    };

                    foreach (DataRow dbRow in dbTable.Rows) {
                        // make sure to only include numbers as serial number
                        if (dbRow["SystemSerial"].ToString() == trendRow["SystemSerial"].ToString() && int.TryParse(dbRow["SystemSerial"].ToString(), out int n)) {
                            // save that
                            sizeAnalyses.ActiveSizeInMB = int.Parse(dbRow["TotalSize"].ToString()) - int.Parse(trendRow["TotalSize"].ToString());

                            long s3InBytes = GetS3StorageSizes(dbRow["SystemSerial"].ToString());
                            float mediate = s3InBytes;
                            sizeAnalyses.S3SizeInMB = float.Parse(Math.Ceiling(mediate / 1000 / 1000).ToString());
                            break;
                        }
                    }

                    list.Add(sizeAnalyses);
                }
                catch (Exception e) {
                    _log.ErrorFormat("Error in GetStorageSizes: {0}", e.Message);
                }
            }

            return list;
        }

        public double GetActiveStorage() {
            double activeSizeInMb = 0;
            var trendTable = _storageAnalysis.GetTrendSize();
            var dbTable = _storageAnalysis.GetDBSize();

            foreach (DataRow trendRow in trendTable.Rows) {
                foreach (DataRow dbRow in dbTable.Rows) {
                    // make sure to only include numbers as serial number
                    if (dbRow["SystemSerial"].ToString() == trendRow["SystemSerial"].ToString() && int.TryParse(dbRow["SystemSerial"].ToString(), out int n)) {
                        // save that
                        activeSizeInMb += double.Parse(dbRow["TotalSize"].ToString()) - double.Parse(trendRow["TotalSize"].ToString());
                        break;
                    }
                }
            }

            return activeSizeInMb;
        }

        private long GetS3StorageSizes(string system_serial) {
            long reports = 0;
            long ftp = 0;
            long uws = 0;

            var amazonS3_reports = new AmazonS3(_s3ReportBucketName);
            try {
                reports = amazonS3_reports.GetS3FolderSizes($@"Systems/{system_serial}");
            }
            catch (Exception e) {
                _log.ErrorFormat("Error in GetS3StorageSizes:{0}", e.Message);
            }

            var amazonS3_ftp = new AmazonS3(_s3FTPBucketName);
            try {
                ftp = amazonS3_ftp.GetS3FolderSizes($@"Systems/{system_serial}");
            }
            catch (Exception e) {
                _log.ErrorFormat("Error in GetS3StorageSizes:{0}", e.Message);                
            }

            var amazonS3_uws = new AmazonS3(_s3UWSBucketName);
            try {
                uws = amazonS3_uws.GetS3FolderSizes($@"Systems/{system_serial}");
            }
            catch (Exception e) {
                _log.ErrorFormat("Error in GetS3StorageSizes:{0}", e.Message);                
            }

            return (reports + ftp + uws);
        }

        public void InsertData(List<TempStorageTableView> storageSizes) {
            foreach (TempStorageTableView row in storageSizes) {
                // put activeSize into table
                try {
                    _storageAnalysis.Insert(row.SystemSerial, row.ActiveSizeInMB, row.TrendSizeInMB, row.S3SizeInMB);
                }
                catch (Exception ex) {
                    throw new Exception(ex.Message);
                }
            }
        }

        public DataTable GetAllRdsRealNames() {
            var storageAnalysis = new StorageAnalysis(_connectionString);
            return storageAnalysis.GetAllRdsRealName();
        }

        public List<RdsInfo> GetRDSInstanceInformation() { 
            var rds = new AmazonRDS();
            return rds.GetRDSInstances();
        }
    }
}
