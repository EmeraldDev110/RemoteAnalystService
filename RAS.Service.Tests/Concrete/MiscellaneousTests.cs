using log4net;
using NUnit.Framework;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using RemoteAnalyst.Repository.Repositories;
using System;
using System.Collections.Generic;
using System.Data;

namespace RAS.Service.Tests.Concrete
{
    public class MiscellaneousTests
    {
        private static readonly ILog Log = LogManager.GetLogger("DBHouseKeeping");
        string connectionString;
        string profileConnectionString;
        string systemSerial;

        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            //profileConnectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc;UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            profileConnectionString = "Server=13.56.143.245;Database=pmc;User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc" + systemSerial + ";UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc" + systemSerial + ";User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
        }

        [Test]
        public void Test_AlertRecipients_GetEmails()
        {
            // Arrange
            AlertRecipientRepository ar = new AlertRecipientRepository();
            int processWatchId = 1;

            // Act
            List<string> result = ar.GetEmails(processWatchId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_CustomerOrders_GetNtsOrderIdBySystemSerialAndFileName()
        {
            // Arrange
            CustomerOrders ar = new CustomerOrders(profileConnectionString);
            string fileName = "Test_File";

            // Act
            int result = ar.GetNtsOrderIdBySystemSerialAndFileName(systemSerial, fileName);

            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void Test_DBAdministrator_GetClientConnection()
        {

            // Arrange
            DBAdministrator dba = new DBAdministrator(connectionString);
            string ipAddress = "localhost";

            // Act
            DataTable result = dba.GetClientConnection("pmc" + systemSerial, ipAddress);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_DataDictionary_GetColumns()
        {

            // Arrange
            DataDictionary dba = new DataDictionary(connectionString);

            // Act
            DataTable result = dba.GetColumns(1, "zmsbladedatadictionary");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }


        [Test]
        public void Test_DataDictionary_GetPathwayColumns()
        {

            // Arrange
            DataDictionary dba = new DataDictionary(profileConnectionString);

            // Act
            DataTable result = dba.GetPathwayColumns("pvcollects");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_Holidays_GetWorkDayFactor()
        {
            // Arrange
            Holidays dba = new Holidays(profileConnectionString);
            DateTime dateTime = new DateTime(2024, 12, 25);

            // Act
            DataTable result = dba.GetWorkDayFactor(systemSerial, dateTime);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }


        [Test]
        public void Test_InHouseConfig_GetNonstopVolumnAndIpPair()
        {
            // Arrange
            InHouseConfig dba = new InHouseConfig(profileConnectionString);

            // Act
            DataTable result = dba.GetNonstopVolumnAndIpPair();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }


        [Test]
        public void Test_NonStopInfo_GetNonStopInfo()
        {
            // Arrange
            NonStopInfo dba = new NonStopInfo(profileConnectionString);

            // Act
            DataTable result = dba.GetNonStopInfo();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_NullCheck_NullCheckForPathwayPramaterPvCollects()
        {
            // Arrange
            DateTime fromTimestamp = Convert.ToDateTime("2024-05-04 00:00:00");
            DateTime toTimestamp = Convert.ToDateTime("2024-05-05 00:00:00");
            NullCheck nullCheck = new NullCheck();
            // Act
            bool result = nullCheck.NullCheckForPathwayPramaterPvCollects(fromTimestamp, toTimestamp, connectionString);

            // Assert
            Assert.That(result, Is.True);
        }


        [Test]
        public void Test_NullCheck_NullCheckForPathwayPramaterPvPwyList()
        {
            // Arrange
            DateTime fromTimestamp = Convert.ToDateTime("2024-05-04 00:00:00");
            DateTime toTimestamp = Convert.ToDateTime("2024-05-05 00:00:00");
            NullCheck nullCheck = new NullCheck();
            // Act
            bool result = nullCheck.NullCheckForPathwayPramaterPvPwyList(fromTimestamp, toTimestamp, connectionString);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Test_PathwayDirectories_CheckDuplicateTime()
        {
            // Arrange
            DateTime fromTimestamp = Convert.ToDateTime("2024-05-04 00:00:00");
            DateTime toTimestamp = Convert.ToDateTime("2024-05-05 00:00:00");
            PathwayDirectories pathwayDirectories = new PathwayDirectories(profileConnectionString);
            // Act
            bool result = pathwayDirectories.CheckDuplicateTime(systemSerial, fromTimestamp, toTimestamp, "TestLocation");

            // Assert
            Assert.That(result, Is.True);
        }


        [Test]
        public void Test_PathwayDirectories_InsertPathwayDirectory()
        {
            // Arrange
            DateTime fromTimestamp = Convert.ToDateTime("2024-05-04 00:00:00");
            DateTime toTimestamp = Convert.ToDateTime("2024-05-05 00:00:00");
            PathwayDirectories pathwayDirectories = new PathwayDirectories(profileConnectionString);
            // Act
            pathwayDirectories.InsertPathwayDirectory(1, systemSerial, fromTimestamp, toTimestamp, "TestLocation");

            // Assert
            Assert.Pass();
        }


        [Test]
        public void Test_ProfileDetail_GetApplicationName()
        {
            // Arrange
            ProfileDetail pd = new ProfileDetail(profileConnectionString);

            // Act
            string result = pd.GetApplicationName(357);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.GreaterThan(0));
        }

        [Test]
        public void Test_QNMDirectories_InsertQNMDirectory()
        {
            // Arrange
            QNMDirectories dba = new QNMDirectories(profileConnectionString);
            int uwsid = 11;
            string location = "Test";
            DateTime startTime = DateTime.Today.AddDays(-1);
            DateTime stopTime = DateTime.Today;

            // Act
            dba.InsertQNMDirectory(uwsid, systemSerial, startTime, stopTime, location);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_QueryHelper_HasPrimaryKey()
        {
            // Arrange
            QueryHelper dba = new QueryHelper();
            string tableName = "080627_CPU_2024_3_29";

            // Act
            bool result = dba.HasPrimaryKey(connectionString, tableName, Log);

            // Assert
            Assert.That(result, Is.True);
        }


        [Test]
        public void Test_RAInfo_GetQueryValue()
        {
            // Arrange
            RAInfoRepository rainfo = new RAInfoRepository();

            // Act
            string result = rainfo.GetQueryValue("currentCollector");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.GreaterThan(0));
        }

        [Test]
        public void Test_RAInfo_GetMaxQueueFor()
        {
            // Arrange
            RAInfoRepository rainfo = new RAInfoRepository();

            // Act
            int result = rainfo.GetMaxQueue("MaxMonthliesAndWeekliesQueue");

            // Assert
            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void Test_RAInfo_GetValue()
        {
            // Arrange
            RAInfoRepository rainfo = new RAInfoRepository();

            // Act
            string result = rainfo.GetValue("currentCollector");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.GreaterThan(0));
        }

        [Test]
        public void Test_ReportActivity_InsertNewEntry()
        {
            // Arrange
            ReportActivity ra = new ReportActivity(profileConnectionString);
            string email = "pauluszemaitis@idelji.com";
            string systemSerial = "080627";
            DateTime from = Convert.ToDateTime("2024-05-28 00:00:00");
            DateTime to = Convert.ToDateTime("2024-05-29 00:00:00");

            // Act
            ra.InsertNewEntry(email, systemSerial, "Storage", "Capacity Summary", from, to);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_ReportDownloadLogs_InsertNewEntry()
        {
            // Arrange
            ReportDownloadLogs rdl = new ReportDownloadLogs(profileConnectionString);
            int reportDownloadId = 1;
            string message = "Analyses is emailed";
            DateTime logDate = Convert.ToDateTime("2024-05-28 00:00:00");

            // Act
            rdl.InsertNewLog(reportDownloadId, logDate, message);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_ReportDownloadLogs_GetFirstLogDate()
        {
            // Arrange
            ReportDownloadLogs rdl = new ReportDownloadLogs(profileConnectionString);
            int reportDownloadId = 1;

            // Act
            DateTime result = rdl.GetFirstLogDate(reportDownloadId);

            // Assert
            Assert.That(result, !Is.Null);
            Assert.That(result, !Is.EqualTo(DateTime.MinValue));
        }

        [Test]
        public void Test_ReportQueues_InsertNewQueueFor()
        {
            // Arrange
            ReportQueues rdl = new ReportQueues(profileConnectionString);

            // Act
            rdl.InsertNewQueue("Test_File", 3); // 3 - Report.Types.QT

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_ReportQueues_RemoveQueue()
        {
            // Arrange
            ReportQueues rdl = new ReportQueues(profileConnectionString);

            // Act
            rdl.RemoveQueue(1); 

            // Assert
            Assert.Pass();
        }

        [Test]
        [Ignore("Can't test since data not populated")]
        public void Test_ScheduleStorageDetail_GetIgnoreVolumes()
        {
            // Arrange
            ScheduleStorageDetail storageIgnoreVol = new ScheduleStorageDetail(profileConnectionString);
            int scheduleId = 14656;

            // Act
            string ignoreVolumes = storageIgnoreVol.GetIgnoreVolumes(scheduleId);

            // Assert
            Assert.That(ignoreVolumes, Is.Not.Null);
            Assert.That(ignoreVolumes.Length, Is.GreaterThan(0));
        }

        [Test]
        [Ignore("Can't test since data not populated")]
        public void Test_ScheduleStorageDetail_getStorageThreshold()
        {
            // Arrange
            ScheduleStorageDetail storageIgnoreVol = new ScheduleStorageDetail(profileConnectionString);
            int scheduleId = 14656;

            // Act
            DataTable result = storageIgnoreVol.getStorageThreshold(scheduleId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_SpecialDays_GetSpecialDays()
        {
            // Arrange
            SpecialDays specialDays = new SpecialDays(profileConnectionString);

            // Act
            DataTable result = specialDays.GetSpecialDays(systemSerial);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_SystemSerialConversions_GetNewSystemSerial()
        {
            // Arrange
            SystemSerialConversions ssc = new SystemSerialConversions(profileConnectionString);

            // Act
            string result = ssc.GetNewSystemSerial("000000");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.GreaterThan(0));
        }

        [Test]
        public void Test_SystemWeek_GetSystemWeek()
        {
            // Arrange
            SystemWeek systemWeek = new SystemWeek(profileConnectionString);

            // Act
            DataTable result = systemWeek.GetSystemWeek("078831");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_TMonComplete_InsertCompleteLog()
        {
            // Arrange
            TMonComplete rdl = new TMonComplete(profileConnectionString);
            string expectedTime = "0000";
            // Act
            rdl.InsertCompleteLog(expectedTime, systemSerial, DateTime.Now, "Test_File");

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_TMonDelay_InsertDelayLog()
        {
            // Arrange
            TMonDelay rdl = new TMonDelay(profileConnectionString);
            string expectedTime = "0000";
            // Act
            rdl.InsertDelayLog(expectedTime, systemSerial, DateTime.Now, "Test_File");

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_TMonFileNames_GetExpectedFileName()
        {
            // Arrange
            TMonFileNames ssc = new TMonFileNames(profileConnectionString);

            // Act
            string result = ssc.GetExpectedFileName(systemSerial, "900");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.GreaterThan(0));
        }

        [Test]
        public void Test_Transmon_GetTransmons()
        {
            // Arrange
            Transmon systemWeek = new Transmon(profileConnectionString);

            // Act
            DataTable result = systemWeek.GetTransmons();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        [Ignore ("Need to figure out how to do this")]
        public void Test_TransmonLogs_Insert()
        {
            // Arrange
            TransmonLogs ssc = new TransmonLogs(profileConnectionString);

            // Act
            int result = ssc.Insert("Test_TransmonLogs.csv");

            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void Test_TransmonLogs_GetSystemsResidual()
        {
            // Arrange
            TransmonLogs systemWeek = new TransmonLogs(profileConnectionString);

            // Act
            DataTable result = systemWeek.GetSystemsResidual(Convert.ToDateTime("2023-01-01 00:00:00"));

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_UploadMessages_InsertNewEntry()
        {
            // Arrange
            UploadMessages ssc = new UploadMessages(profileConnectionString);

            // Act
            ssc.InsertNewEntry(1, DateTime.Now, 0, "Test_Message");

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_UploadMessages_CheckMessageCount()
        {
            // Arrange
            UploadMessages systemWeek = new UploadMessages(profileConnectionString);

            // Act
            int result = systemWeek.CheckMessageCount(1, "Test_Message");

            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void Test_UploadStatus_GetStatusId()
        {
            // Arrange
            UploadStatus us = new UploadStatus(profileConnectionString);

            // Act
            int result = us.GetStatusId(1);

            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void Test_UploadStatus_DeleteEntry()
        {
            // Arrange
            UploadStatus us = new UploadStatus(profileConnectionString);

            // Act
            us.DeleteEntry(1);

            // Assert
            Assert.Pass();
        }


        [Test]
        public void Test_VisaTrendLoader_InsertEntry()
        {
            // Arrange
            VisaTrendLoader vtl = new VisaTrendLoader(profileConnectionString);

            // Act
            vtl.InsertEntry(systemSerial, DateTime.Today);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_VisaTrendLoader_CheckMessageCount()
        {
            // Arrange
            VisaTrendLoader vtl = new VisaTrendLoader(profileConnectionString);

            // Act
            bool result = vtl.CheckEntry(systemSerial, Convert.ToDateTime("2024-05-29 00:00:00"));

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Test_VProcVersions_GetVprocVersionFor()
        {
            // Arrange
            VProcVersions vProcVersions = new VProcVersions(profileConnectionString);

            // Act
            string result = vProcVersions.GetVprocVersion("T0951H01_30SEP2015_RASMCOLL_2015_1_0");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo("T0951H01_30SEP2015_RASMCOLL_2015_1_0"));
        }

        [Test]
        public void Test_VProcVersions_GetClassName()
        {
            // Arrange
            VProcVersions vProcVersions = new VProcVersions(profileConnectionString);

            // Act
            string result = vProcVersions.GetClassName("T0951H01_30SEP2015_RASMCOLL_2015_1_0");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo("HeaderInfoV1"));
        }

        [Test]
        public void Test_VProcVersions_GetDataDictionary()
        {
            // Arrange
            VProcVersions vProcVersions = new VProcVersions(profileConnectionString);

            // Act
            string result = vProcVersions.GetDataDictionary("T0951H01_30SEP2015_RASMCOLL_2015_1_0");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo("ZmsBladeDataDictionaryV1"));
        }

    }
}