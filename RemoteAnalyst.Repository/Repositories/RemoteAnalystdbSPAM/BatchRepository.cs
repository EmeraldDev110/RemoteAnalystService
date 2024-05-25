using System;
using System.Collections.Generic;
using RemoteAnalyst.Repository.Models;
using RemoteAnalyst.Repository.Helpers;
using NHibernate;
using NHibernate.Criterion;
using System.Data;
using System.Linq;
using NHibernate.Linq;
using System.Configuration;
using RemoteAnalyst.Repository.Resources;
using log4net;

namespace RemoteAnalyst.Repository.Repositories
{
    public class BatchRepository
    {
        private readonly bool _isLocalAnalyst = false;
        private readonly string _connectionString;
        private readonly ILog _log;

        public BatchRepository(string connectionString, ILog log, bool isLocalAnalyst)
        {
            _connectionString = connectionString;
            _log = log;
            _isLocalAnalyst = isLocalAnalyst;
        }

        public string decryptPassword(string connectionString)
        {
            if (_isLocalAnalyst)
            {
                var decrypt = new Decrypt();
                var decryptedString = decrypt.strDESDecrypt(connectionString);
                return decryptedString;
            }
            else
            {
                return connectionString;
            }
        }

        public DataTable GetAllSystemInformationForBatch()
        {
            try
            {
                DatabaseMapping databaseMapping = null;
                Models.System system = null;

                using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
                {

                    ICollection<object[]> res = session.QueryOver(() => databaseMapping)
                        .JoinQueryOver(() => databaseMapping.System, () => system)
                        .Select(
                            _ => system.SystemName,
                            _ => system.SystemSerial,
                            _ => system.TimeZone,
                            _ => databaseMapping.ConnectionString
                        ).List<object[]>();
                    foreach (object[] row in res)
                    {
                        string connectionString = row[3] as string;
                        if (!string.IsNullOrEmpty(connectionString))
                            row[3] = decryptPassword(connectionString);
                    }
                    return CollectionHelper.ListToDataTable(res, new List<string>() { "SystemName", "SystemSerial", "TimeZone", "ConnectionString" });
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.StackTrace);
                return new DataTable();
            }
        }

        public DataTable GetBatchInformationBySystem()
        {
            try
            {
                using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
                {
                    //IList<BatchSequenceAlertRecipient> alertRecipients = session.QueryOver<BatchSequenceAlertRecipient>().List<BatchSequenceAlertRecipient>();
                    //IDictionary<int, List<string>> emailDict = new Dictionary<int, List<string>>();
                    //foreach (BatchSequenceAlertRecipient alertRecipient in alertRecipients)
                    //{
                    //    if (!emailDict.ContainsKey(alertRecipient.BatchSequenceProfileId))
                    //        emailDict[alertRecipient.BatchSequenceProfileId] = new List<string>();

                    //    if (!emailDict[alertRecipient.BatchSequenceProfileId].Contains(alertRecipient.EmailAddress))
                    //        emailDict[alertRecipient.BatchSequenceProfileId].Add(alertRecipient.EmailAddress);
                    //}

                    //IList<BatchSequenceAlertProgram> alertPrograms = session
                    //    .QueryOver<BatchSequenceAlertProgram>()
                    //    .OrderBy(x => x.Order).Asc
                    //    .List<BatchSequenceAlertProgram>();
                    //IDictionary<int, List<string>> programDict = new Dictionary<int, List<string>>();
                    //foreach (BatchSequenceAlertProgram alertProgram in alertPrograms)
                    //{
                    //    if (!programDict.ContainsKey(alertProgram.BatchSequenceProfileId))
                    //        programDict[alertProgram.BatchSequenceProfileId] = new List<string>();

                    //    if (!programDict[alertProgram.BatchSequenceProfileId].Contains(alertProgram.ProgramFile))
                    //        programDict[alertProgram.BatchSequenceProfileId].Add(alertProgram.ProgramFile);
                    //}

                    ICollection<BatchSequenceProfile> batchSequenceProfiles = session.CreateCriteria(typeof(BatchSequenceProfile)).List<BatchSequenceProfile>();
                    foreach (BatchSequenceProfile profile in batchSequenceProfiles)
                    {
                        //profile.EmailList = string.Join(",", emailDict[profile.BatchSequenceProfileId]);
                        //profile.ProgramFiles = string.Join(",", programDict[profile.BatchSequenceProfileId]);
                        profile.EmailList = string.Join(",", profile.AlertRecipients.AsEnumerable().Select(x => x.EmailAddress).ToArray());
                        profile.ProgramFiles = string.Join(",", profile.AlertPrograms.AsEnumerable().Select(x => x.ProgramFile).ToArray());
                        profile.AlertRecipients.Clear();
                        profile.AlertPrograms.Clear();
                    }

                    return CollectionHelper.ToDataTable(batchSequenceProfiles);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.StackTrace);
                return new DataTable();
            }
        }

        public DataTable GetBatchInformationByName(string batchSequenceName)
        {
            try
            {
                using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
                {
                    //BatchSequenceProfile batchSequenceProfile = session
                    //    .CreateCriteria(typeof(BatchSequenceProfile))
                    //    .Add(Restrictions.Eq("Name", batchSequenceName))
                    //    .UniqueResult<BatchSequenceProfile>();

                    //IList<BatchSequenceAlertRecipient> alertRecipients = session
                    //    .QueryOver<BatchSequenceAlertRecipient>()
                    //    .Where(x => x.BatchSequenceProfileId == batchSequenceProfile.BatchSequenceProfileId)
                    //    .List<BatchSequenceAlertRecipient>();
                    //IDictionary<int, List<string>> emailDict = new Dictionary<int, List<string>>();
                    //foreach (BatchSequenceAlertRecipient alertRecipient in alertRecipients)
                    //{
                    //    if (!emailDict.ContainsKey(alertRecipient.BatchSequenceProfileId))
                    //        emailDict[alertRecipient.BatchSequenceProfileId] = new List<string>();

                    //    if (!emailDict[alertRecipient.BatchSequenceProfileId].Contains(alertRecipient.EmailAddress))
                    //        emailDict[alertRecipient.BatchSequenceProfileId].Add(alertRecipient.EmailAddress);
                    //}

                    //IList<BatchSequenceAlertProgram> alertPrograms = session
                    //    .QueryOver<BatchSequenceAlertProgram>()
                    //    .Where(x => x.BatchSequenceProfileId == batchSequenceProfile.BatchSequenceProfileId)
                    //    .OrderBy(x => x.Order).Asc
                    //    .List<BatchSequenceAlertProgram>();
                    //IDictionary<int, List<string>> programDict = new Dictionary<int, List<string>>();
                    //foreach (BatchSequenceAlertProgram alertProgram in alertPrograms)
                    //{
                    //    if (!programDict.ContainsKey(alertProgram.BatchSequenceProfileId))
                    //        programDict[alertProgram.BatchSequenceProfileId] = new List<string>();

                    //    if (!programDict[alertProgram.BatchSequenceProfileId].Contains(alertProgram.ProgramFile))
                    //        programDict[alertProgram.BatchSequenceProfileId].Add(alertProgram.ProgramFile);
                    //}

                    //batchSequenceProfile.EmailList = string.Join(",", emailDict[batchSequenceProfile.BatchSequenceProfileId]);
                    //batchSequenceProfile.ProgramFiles = string.Join(",", programDict[batchSequenceProfile.BatchSequenceProfileId]);

                    BatchSequenceProfile batchSequenceProfile = session
                        .CreateCriteria(typeof(BatchSequenceProfile))
                        .Add(Restrictions.Eq("Name", batchSequenceName))
                        .UniqueResult<BatchSequenceProfile>();

                    batchSequenceProfile.EmailList = string.Join(",", batchSequenceProfile.AlertRecipients.AsEnumerable().Select(x => x.EmailAddress).ToArray());
                    batchSequenceProfile.ProgramFiles = string.Join(",", batchSequenceProfile.AlertPrograms.AsEnumerable().Select(x => x.ProgramFile).ToArray());
                    batchSequenceProfile.AlertRecipients.Clear();
                    batchSequenceProfile.AlertPrograms.Clear();
                    return CollectionHelper.ToDataTable(batchSequenceProfile);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.StackTrace);
                return new DataTable();
            }
        }

        public int GetRDSRetentionDays(string systemSerial)
        {
            int RDSRetentionDays = 0;

            try
            {
                using (ISession session = NHibernateHelper.OpenSession())
                {
                    RDSRetentionDays = session.QueryOver<Models.System>()
                        .Where(x => x.SystemSerial == systemSerial)
                        .Select(x => x.ExpertReportRetentionDay)
                        .SingleOrDefault<int>();
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.StackTrace);
            }

            return RDSRetentionDays;
        }

        public BatchSequenceTrend GetProcessesTrendInformationByProgramFile(string systemSerial, string programFileName,
            DateTime startWindowStartTwoDaysAgo, DateTime startWindowStartOneDayAgo, DateTime expectedFinishBy, int dayOffset)
        {
            // Check if table exist
            //bool tableOneDayAgoExists = NHibernateHelper.CheckTableExists(tableNameFrom1DayAgo, _connectionString);
            //bool tableTwoDayAgoExists = NHibernateHelper.CheckTableExists(tableNameFrom2DaysAgo, _connectionString);

            string[] VolSubFile = programFileName.Split('.');
            string volume = VolSubFile[0];
            string subVolume = VolSubFile[1];
            string fileName = VolSubFile[2];
            ICollection<object[]> queryRes = new List<object[]>();

            DataTable dataTable = new DataTable();
            try
            {
                using (ISession session = NHibernateHelper.OpenSessionForPartioned("ProcessEntity", "process", _connectionString, startWindowStartTwoDaysAgo, systemSerial))
                {
                    ICollection<object[]> fromDayRes = session.CreateCriteria<ProcessEntity>()
                        .Add(Restrictions.Like("Volume", volume.Replace('*', '%')))
                        .Add(Restrictions.Like("SubVol", subVolume.Replace('*', '%')))
                        .Add(Restrictions.Like("FileName", fileName.Replace('*', '%')))
                        .Add(Restrictions.Ge("FromTimestamp", startWindowStartTwoDaysAgo))
                        .Add(Restrictions.Lt("ToTimestamp", expectedFinishBy))
                        .SetProjection(Projections.ProjectionList()
                            .Add(Projections.Alias(Projections.Min("FromTimestamp"), "FromTimestamp"))
                            .Add(Projections.Alias(Projections.Max("ToTimestamp"), "ToTimestamp"))
                            .Add(Projections.GroupProperty("Volume"))
                            .Add(Projections.GroupProperty("SubVol"))
                            .Add(Projections.GroupProperty("FileName"))
                        )
                        .List<object[]>();
                    queryRes = queryRes.Concat(fromDayRes).ToList();
                }
                using (ISession session = NHibernateHelper.OpenSessionForPartioned("ProcessEntity", "process", _connectionString, startWindowStartOneDayAgo, systemSerial))
                {
                    ICollection<object[]> toDayRes = session.CreateCriteria<ProcessEntity>()
                        .Add(Restrictions.Like("Volume", volume.Replace('*', '%')))
                        .Add(Restrictions.Like("SubVol", subVolume.Replace('*', '%')))
                        .Add(Restrictions.Like("FileName", fileName.Replace('*', '%')))
                        .Add(Restrictions.Ge("FromTimestamp", startWindowStartTwoDaysAgo))
                        .Add(Restrictions.Lt("ToTimestamp", expectedFinishBy))
                        .SetProjection(Projections.ProjectionList()
                            .Add(Projections.Alias(Projections.Min("FromTimestamp"), "FromTimestamp"))
                            .Add(Projections.Alias(Projections.Max("ToTimestamp"), "ToTimestamp"))
                            .Add(Projections.GroupProperty("Volume"))
                            .Add(Projections.GroupProperty("SubVol"))
                            .Add(Projections.GroupProperty("FileName"))
                        )
                        .List<object[]>();
                    queryRes = queryRes.Concat(toDayRes).ToList();
                }
                dataTable = CollectionHelper.ListToDataTable(queryRes, new List<string> { "FromTimestamp", "ToTimestamp", "Volume", "SubVol", "FileName" });
                string startTimeString = dataTable.AsEnumerable().Min(x => x.Field<string>("FromTimestamp"));
                string endTimeString = dataTable.AsEnumerable().Max(x => x.Field<string>("ToTimestamp"));
                DateTime startTime = DateTime.Parse(startTimeString);
                DateTime endTime = DateTime.Parse(endTimeString);
                int duration = (int)(endTime - startTime).TotalSeconds;
                return new BatchSequenceTrend()
                {
                    ProgramFile = programFileName,
                    StartTime = startTime,
                    EndTime = endTime,
                    Duration = duration,
                    DataDate = DateTime.Now.AddDays(-2 + (dayOffset * -1))
                };

            }
            catch (Exception ex)
            {
                _log.Error(ex.StackTrace);
                throw new Exception(ex.Message);
            }
        }

        public void InsertBatchTrendData(List<BatchSequenceTrend> batchTrendList, int batchSequenceProfileId)
        {
            try
            {
                using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
                using (ITransaction transaction = session.BeginTransaction())
                {
                    foreach (BatchSequenceTrend trend in batchTrendList)
                    {
                        trend.BatchSequenceProfileId = batchSequenceProfileId;
                        session.Save(trend);
                    }
                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.StackTrace);
                throw new Exception(ex.Message);
            }

        }

        public bool CheckBatchTablesExists()
        {
            try
            {
                using (ISession session = NHibernateHelper.OpenSessionCustom(_connectionString))
                {
                    session.Query<BatchSequenceProfile>().FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("doesn't exist")) return false;
                _log.Error(ex.StackTrace);
                throw ex;
            }
            return true;
        }

        public void CreateBatchTables()
        {
            // TODO
            try
            {
                //NHibernateHelper.CreateTables(_connectionString, typeof(BatchSequenceProfile).Assembly);
                //NHibernateHelper.CreateTables(_connectionString, typeof(BatchSequenceAlertProgram).Assembly);
                //NHibernateHelper.CreateTables(_connectionString, typeof(BatchSequenceAlertRecipient).Assembly);
                //NHibernateHelper.CreateTables(_connectionString, typeof(BatchSequenceTrend).Assembly);
            }
            catch (Exception ex)
            {
                _log.Error(ex.StackTrace);
                throw new Exception(ex.Message);
            }
        }

        internal void CreateBatchSequenceProfileTable()
        {
            // TODO
            string sqlStr = @"CREATE TABLE `BatchSequenceProfile` (
                              `BatchSequenceProfileId` int(11) NOT NULL AUTO_INCREMENT,
                              `Name` varchar(100) DEFAULT NULL,
                              `StartWindowStart` time DEFAULT NULL,
                              `StartWindowEnd` time DEFAULT NULL,
                              `StartWindowDoW` char(7) DEFAULT NULL,
                              `ExpectedFinishBy` time DEFAULT NULL,
                              `AlertIfDoesNotStartOnTime` tinyint(4) DEFAULT NULL,
                              `AlertIfOrderNotFollowed` tinyint(4) DEFAULT NULL,
                              `AlertIfDoesNotFinishOnTime` tinyint(4) DEFAULT NULL,
                              PRIMARY KEY (`BatchSequenceProfileId`),
                              UNIQUE KEY `Name_UNIQUE` (`Name`)
                            ) ENGINE=InnoDB AUTO_INCREMENT=17 DEFAULT CHARSET=latin1;";

            //using (var connection = new MySqlConnection(_connectionString))
            //{
            //    var command = new MySqlCommand(sqlStr, connection) { CommandTimeout = 0 };
            //    connection.Open();
            //    command.ExecuteNonQuery();
            //}
        }

        internal void CreateBatchSequenceAlertProgramsTable()
        {
            // TODO
            string sqlStr = @"CREATE TABLE `BatchSequenceAlertPrograms` (
                              `BatchSequenceProfileId` int(11) NOT NULL,
                              `ProgramFile` varchar(40) NOT NULL,
                              `Order` int(11) DEFAULT NULL,
                              PRIMARY KEY (`BatchSequenceProfileId`,`ProgramFile`)
                            ) ENGINE=InnoDB DEFAULT CHARSET=latin1;";

            //using (var connection = new MySqlConnection(_connectionString))
            //{
            //    var command = new MySqlCommand(sqlStr, connection) { CommandTimeout = 0 };
            //    connection.Open();
            //    command.ExecuteNonQuery();
            //}
        }

        internal void CreateBatchSequenceAlertRecipientsTable()
        {
            // TODO
            string sqlStr = @"CREATE TABLE `BatchSequenceAlertRecipients` (
                              `BatchSequenceProfileId` int(11) NOT NULL,
                              `EmailAddress` varchar(255) NOT NULL,
                              PRIMARY KEY (`BatchSequenceProfileId`,`EmailAddress`),
                              CONSTRAINT `BatchSequenceProfileId` FOREIGN KEY (`BatchSequenceProfileId`) REFERENCES `BatchSequenceProfile` (`BatchSequenceProfileId`) ON DELETE CASCADE ON UPDATE CASCADE
                            ) ENGINE=InnoDB DEFAULT CHARSET=latin1;";

            //using (var connection = new MySqlConnection(_connectionString))
            //{
            //    var command = new MySqlCommand(sqlStr, connection) { CommandTimeout = 0 };
            //    connection.Open();
            //    command.ExecuteNonQuery();
            //}
        }

        internal void CreateBatchSequenceTrendTable()
        {
            // TODO
            string sqlStr = @"CREATE TABLE `BatchSequenceTrend` (
                              `BatchSequenceProfileId` int(11) NOT NULL,
                              `ProgramFile` varchar(40) NOT NULL,
                              `DataDate` datetime NOT NULL,
                              `StartTime` datetime DEFAULT NULL,
                              `EndTime` datetime DEFAULT NULL,
                              `Duration` int(11) DEFAULT NULL,
                              PRIMARY KEY (`BatchSequenceProfileId`,`ProgramFile`,`DataDate`),
                              CONSTRAINT `BatchSequenceTrend_BatchSequenceProfileId_ProgramFile` FOREIGN KEY (`BatchSequenceProfileId`, `ProgramFile`) REFERENCES `BatchSequenceAlertPrograms` (`BatchSequenceProfileId`, `ProgramFile`) ON DELETE CASCADE ON UPDATE CASCADE
                            ) ENGINE=InnoDB DEFAULT CHARSET=latin1;
";

            //using (var connection = new MySqlConnection(_connectionString))
            //{
            //    var command = new MySqlCommand(sqlStr, connection) { CommandTimeout = 0 };
            //    connection.Open();
            //    command.ExecuteNonQuery();
            //}
        }

    }
}
