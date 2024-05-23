using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using log4net;
using RemoteAnalyst.Repository.Repositories;
using RemoteAnalyst.Repository.Models;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class BatchService {

        private readonly string _connectionString = "";
        private readonly bool _isLocalAnalyst = false;
        private readonly ILog _log;

        public BatchService(string connectionString, ILog log, bool isLocalAnalyst) {
            _connectionString = connectionString;
            _isLocalAnalyst = isLocalAnalyst;
            _log = log;
        }

        public List<BatchView> GetAllSystemInformationForBatch() {
            _log.Info("Retrieving All System Information");

            List<BatchView> batchList = new List<BatchView>();
            try {
                var batchRepository = new Batch(_connectionString, _log, _isLocalAnalyst);
                DataTable systemTable = batchRepository.GetAllSystemInformationForBatch();

                foreach (DataRow row in systemTable.Rows) {
                    string connectionString = row["MySQLConnectionString"].ToString();

                    batchList.Add(new BatchView {
                        SystemName = row["SystemName"].ToString(),
                        SystemSerial = row["SystemSerial"].ToString(),
                        TimeZone = Convert.ToInt32(row["TimeZone"]),
                        ConnectionString = connectionString
                    });

                }
            } catch (Exception ex) {
                _log.ErrorFormat("Error Retrieving All SystemInformation {0}", ex);
            }
            _log.InfoFormat("System Information Retrieved with a system Count of: {0}", batchList.Count);
            return batchList;
        }

        public List<BatchView> GetBatchInformationBySystem() {
            _log.Info("Returning Batch Information By System");
            List<BatchView> batchList = new List<BatchView>();

            try {
                var batchRepository = new Batch(_connectionString, _log, _isLocalAnalyst);
                DataTable systemTable = batchRepository.GetBatchInformationBySystem();
                foreach (DataRow row in systemTable.Rows) {
                    batchList.Add(new BatchView {
                        BatchSequenceProfileId = Convert.ToInt32(row["BatchSequenceProfileId"]),
                        BatchName = row["Name"].ToString(),
                        StartWindowStart = DateTime.Parse("1111-11-11 " + row["StartWindowStart"].ToString()),
                        StartWindowEnd = DateTime.Parse("1111-11-11 " + row["StartWindowEnd"].ToString()),
                        StartWindowDoW = row["StartWindowDoW"].ToString().ToCharArray(),
                        ExpectedFinishBy = DateTime.Parse("1111-11-11 " + row["ExpectedFinishBy"].ToString()),
                        AlertIfDoesNotStartOnTime = Convert.ToInt32(row["AlertIfDoesNotStartOnTime"]) == 1,
                        AlertIfDoesNotFinishOnTime = Convert.ToInt32(row["AlertIfDoesNotFinishOnTime"]) == 1,
                        AlertIfOrderNotFollowed = Convert.ToInt32(row["AlertIfOrderNotFollowed"]) == 1,
                        EmailList = row["EmailList"].ToString(),
                        ProgramFiles = row["ProgramFiles"].ToString()
                    });

                }
            } catch (Exception ex) {
                _log.ErrorFormat("Error Retrieving Batch Information By System {0}", ex);
            }

            _log.InfoFormat("Returned system batch information with a count of: {0}", batchList.Count);
            return batchList;
        }

        public BatchView GetBatchInformationByName(string batchSequenceName) {
            _log.Info("Returning Batch Information By Name");
            BatchView newBatch = new BatchView();

            try {
                var batchRepository = new Batch(_connectionString, _log, _isLocalAnalyst);
                DataTable systemTable = batchRepository.GetBatchInformationByName(batchSequenceName);
                foreach (DataRow row in systemTable.Rows) {
                    newBatch.BatchSequenceProfileId = Convert.ToInt32(row["BatchSequenceProfileId"]);
                    newBatch.BatchName = row["Name"].ToString();
                    newBatch.StartWindowStart = DateTime.Parse("1111-11-11 " + row["StartWindowStart"].ToString());
                    newBatch.StartWindowEnd = DateTime.Parse("1111-11-11 " + row["StartWindowEnd"].ToString());
                    newBatch.StartWindowDoW = row["StartWindowDoW"].ToString().ToCharArray();
                    newBatch.ExpectedFinishBy = DateTime.Parse("1111-11-11 " + row["ExpectedFinishBy"].ToString());
                    newBatch.AlertIfDoesNotStartOnTime = Convert.ToInt32(row["AlertIfDoesNotStartOnTime"]) == 1;
                    newBatch.AlertIfDoesNotFinishOnTime = Convert.ToInt32(row["AlertIfDoesNotFinishOnTime"]) == 1;
                    newBatch.AlertIfOrderNotFollowed = Convert.ToInt32(row["AlertIfOrderNotFollowed"]) == 1;
                    newBatch.EmailList = row["EmailList"].ToString();
                    newBatch.ProgramFiles = row["ProgramFiles"].ToString();
                }
            }
            catch (Exception ex) {
                _log.ErrorFormat("Error Retrieving Batch Information By Name {0}", ex);
            }

            _log.Info("Returned system batch information by name");
            return newBatch;
        }

        public void InsertBatchTrendData(List<BatchTrendView> batchTrend, int BatchSequenceProfileId) {
            try {
                _log.Info("Inserting Batch Trend Information into database");
                
                var batchRepository = new Batch(_connectionString, _log, _isLocalAnalyst);
                string query = BuildBatchTrendInsertionQueryForMultipleRows(batchTrend, BatchSequenceProfileId);
                if (query.Length > 0) batchRepository.InsertBatchTrendData(query);
                _log.Info("Batch Trend Data Inserted into database");
                
            } catch(Exception ex) {
                _log.ErrorFormat("Error Inserting Batch Trend Data {0}", ex);
            }
        }

        public string BuildBatchTrendInsertionQueryForMultipleRows(List<BatchTrendView> batchTrendList, int BatchSequenceProfileId) {
            var query = new StringBuilder();
            try {
                query.Append("INSERT INTO BatchSequenceTrend (BatchSequenceProfileId, ProgramFile, DataDate, StartTime, EndTime, Duration) VALUES ");
                foreach (var batchTrend in batchTrendList) {
                    query.Append($"({BatchSequenceProfileId}, '{batchTrend.FullFileName}', '{batchTrend.DataDate.ToString("yyyy-MM-dd H:mm:ss")}', '{batchTrend.StartTime.ToString("yyyy-MM-dd H:mm:ss")}', '{batchTrend.EndTime.ToString("yyyy-MM-dd H:mm:ss")}', {batchTrend.Duration}),");
                }
                if (batchTrendList.Count > 0) query.Length--;
                else query.Length = 0;
            } catch (Exception ex) {
                _log.ErrorFormat("Error Building Insertion Query for Batch {0}", ex);
            }
            return query.ToString();
        }

        public List<BatchTrendView> GetProcessesTrendInformationByBatchId(BatchView batch, string systemSerial, int dayOffset) {
            List<BatchTrendView> trendList = new List<BatchTrendView>();

            try {
#if (DEBUG)
                DateTime currentDate = Convert.ToDateTime("10/17/2021 01:00:00 AM");
#else        
                DateTime currentDate = DateTime.Now.AddDays(dayOffset * -1);
#endif
                DateTime dateTwoDaysAgo = currentDate.AddDays(-2);
                DateTime dateOneDayAgo = currentDate.AddDays(-1);

                string tableNameFrom2DaysAgo = systemSerial + '_' + "PROCESS" + '_' + dateTwoDaysAgo.Year + '_' + dateTwoDaysAgo.Month + '_' + dateTwoDaysAgo.Day;
                string tableNameFrom1DayAgo = systemSerial + '_' + "PROCESS" + '_' + dateOneDayAgo.Year + '_' + dateOneDayAgo.Month + '_' + dateOneDayAgo.Day;
                var dataBaseName = FindDatabaseName(_connectionString);
                var tableOneDayAgoExists = MySqlTableExists(dataBaseName, tableNameFrom1DayAgo);
                var tableTwoDayAgoExists = MySqlTableExists(dataBaseName, tableNameFrom2DaysAgo);
                if (!tableOneDayAgoExists || !tableTwoDayAgoExists) return trendList;

                var batchRepository = new Batch(_connectionString, _log, _isLocalAnalyst);
                string[] programFiles = batch.ProgramFiles.Split(',');

                foreach (var programFile in programFiles) {
                    string query = CreateQueryForProcesses(batch, systemSerial, programFile, dayOffset);
                    var trendTable = batchRepository.GetProcessesTrendInformationByBatchId(query);
                    if(trendTable != null && trendTable.Rows.Count > 0) { 
                        var row = trendTable.Rows[0];
                        if (row["StartTime"].ToString() == "") continue;
                        trendList.Add(new BatchTrendView {
                            FullFileName = programFile,
                            StartTime = DateTime.Parse(row["StartTime"].ToString()),
                            EndTime = DateTime.Parse(row["EndTime"].ToString()),
                            Duration = Convert.ToInt32(row["Duration"]),
                            DataDate = DateTime.Now.AddDays(-2 + (dayOffset * -1))
                        });
                    }
                }
            }
            catch (Exception ex) {
                _log.ErrorFormat("Error Retrieving process Trend Information By Batch Id {0}", ex);
            }

            return trendList;
        }

        public string CreateQueryForProcesses(BatchView batch, string systemSerial, string programFileName, int dayOffset) {
            var query = new StringBuilder();
            try
            {
#if (DEBUG)
                DateTime currentDate = Convert.ToDateTime("10/17/2021 01:00:00 AM");
#else        
                DateTime currentDate = DateTime.Now.AddDays(dayOffset * -1);
#endif
                DateTime dateTwoDaysAgo = currentDate.AddDays(-2);
                DateTime dateOneDayAgo = currentDate.AddDays(-1);

                string tableNameFrom2DaysAgo = '`' + systemSerial + '_' + "PROCESS" + '_' + dateTwoDaysAgo.Year + '_' + dateTwoDaysAgo.Month + '_' + dateTwoDaysAgo.Day + '`';
                string tableNameFrom1DayAgo = '`' + systemSerial + '_' + "PROCESS" + '_' + dateOneDayAgo.Year + '_' + dateOneDayAgo.Month + '_' + dateOneDayAgo.Day + '`';

                DateTime startWindowStartTwoDaysAgo = new DateTime(dateTwoDaysAgo.Year, dateTwoDaysAgo.Month, dateTwoDaysAgo.Day, batch.StartWindowStart.Hour, batch.StartWindowStart.Minute, 0);
                DateTime startWindowStartOneDayAgo = new DateTime(dateOneDayAgo.Year, dateOneDayAgo.Month, dateOneDayAgo.Day, batch.StartWindowStart.Hour, batch.StartWindowStart.Minute, 0);
                DateTime expectedFinishBy = new DateTime(dateTwoDaysAgo.Year, dateTwoDaysAgo.Month, dateTwoDaysAgo.Day, batch.ExpectedFinishBy.Hour, batch.ExpectedFinishBy.Minute, 0);

                if (DateTime.Compare(batch.StartWindowStart, batch.ExpectedFinishBy) >= 0 || DateTime.Compare(batch.StartWindowStart, batch.StartWindowEnd) >= 0 || DateTime.Compare(batch.StartWindowEnd, batch.ExpectedFinishBy) >= 0) {
                    expectedFinishBy = expectedFinishBy.AddDays(1);
                }

                string subQueryForListOfFiles2DaysAgo = CreateSubQueryForListOfFilesByProgramFileName(programFileName, tableNameFrom2DaysAgo);
                string subQueryForListOfFiles1DayAgo = CreateSubQueryForListOfFilesByProgramFileName(programFileName, tableNameFrom1DayAgo);

                query.Append("SELECT MIN(FromTimestamp) AS StartTime, MAX(ToTimestamp) AS EndTime, TIMESTAMPDIFF(SECOND, MIN(FromTimestamp), MAX(ToTimestamp)) AS Duration FROM ");
                query.Append("((SELECT Filename, MIN(FromTimestamp) AS FromTimestamp, MAX(ToTimestamp) AS ToTimestamp FROM ");
                query.Append(tableNameFrom2DaysAgo);
                query.Append($" {subQueryForListOfFiles2DaysAgo}");
                query.Append(" GROUP BY CPUNUM, PIN, PROCESSNAME, VOLUME, SUBVOL, FILENAME)");
                query.Append(" UNION ");
                query.Append("(SELECT Filename, MIN(FromTimestamp) AS FromTimestamp, MAX(ToTimestamp) AS ToTimestamp FROM ");
                query.Append(tableNameFrom1DayAgo);
                query.Append($"{subQueryForListOfFiles1DayAgo}");
                query.Append(" GROUP BY CPUNUM, PIN, PROCESSNAME, VOLUME, SUBVOL, FILENAME)");
                query.Append($" ) AS subqueryB");
                query.Append($" WHERE FROMTIMESTAMP > '{startWindowStartTwoDaysAgo.ToString("yyyy-MM-dd HH:mm:ss")}' AND TOTIMESTAMP < '{expectedFinishBy.ToString("yyyy-MM-dd HH:mm:ss")}' ");

            }
            catch (Exception ex) {
                _log.ErrorFormat("Error Creating Query for Processes {0}", ex);
            }

            return query.ToString();
        }

        public string CreateSubQueryForListOfFilesByProgramFileName(string programFileName, string SQLTableName) {
            var subQuery = new StringBuilder();
            try {

                string[] VolSubFile = programFileName.Split('.');
                string volume = VolSubFile[0];
                string subVolume = VolSubFile[1];
                string fileName = VolSubFile[2];

                subQuery.Append(" WHERE ");
                if (volume.Contains("*")) {
                    subQuery.Append(" (Volume LIKE '" + volume.Replace('*', '%') + "'");
                } else {
                    subQuery.Append(" (Volume = '" + volume + "'");
                }

                if (subVolume.Contains("*")) {
                    if (subQuery.Length > 0) subQuery.Append(" AND ");
                    subQuery.Append(" SubVol LIKE '" + subVolume.Replace('*', '%') + "'");
                } else {
                    if (subQuery.Length > 0) subQuery.Append(" AND ");
                    subQuery.Append(" SubVol = '" + subVolume + "'");
                }

                if (fileName.Contains("*")) {
                    if (subQuery.Length > 0) subQuery.Append(" AND ");
                    subQuery.Append(" FileName LIKE '" + fileName.Replace('*', '%') + "')");
                } else {
                    if (subQuery.Length > 0) subQuery.Append(" AND ");
                    subQuery.Append(" FileName = '" + fileName + "')");
                }
            } catch (Exception ex) {
                _log.ErrorFormat("Error Building subquery for list of programs {0}", ex);
            }
            return subQuery.ToString();
        }

        public int GetRDSRetentionDays(string systemSerial) {
            var batchRepository = new Batch(_connectionString, _log, _isLocalAnalyst);
            return batchRepository.GetRDSRetentionDays(systemSerial);
        }

        public string FindDatabaseName(string mysqlConnectionString) {
            var batchRepository = new Batch(_connectionString, _log, _isLocalAnalyst);
            return batchRepository.FindDatabaseName(mysqlConnectionString);
        }

        public bool MySqlTableExists(string databaseName, string tableName) {
            var exists = false;
            try {
                var batchRepository = new Batch(_connectionString, _log, _isLocalAnalyst);
                exists = batchRepository.MySqlTableExists(databaseName, tableName);
            } catch (Exception ex) {
                _log.ErrorFormat("Error Checking if table: {0} exists {1}", tableName, ex);
            }
            return exists;
        }

        public void CreateBatchTables() {
            try {
                var batchRepository = new Batch(_connectionString, _log, _isLocalAnalyst);
                batchRepository.CreateBatchTables();
            } catch (Exception ex) {
                _log.Error("Error Creating Batch Table {0}", ex);
            }

        }

        public void CreateBatchTablesIfNotExist()
        {
            try
            {
                var dataBaseName = FindDatabaseName(_connectionString);
                var tablesExists = MySqlTableExists(dataBaseName, "BatchSequenceProfile");
                if (!tablesExists) CreateBatchTables();
            }
            catch (Exception ex)
            {
                _log.Error("Error Checking Batch Tables {0}", ex);
            }

        }
    }
}
