using NUnit.Framework;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static log4net.Appender.RollingFileAppender;

namespace RAS.Service.Tests.Concrete
{
    public class LoadingInfoTests
    {
        string connectionString;
        string systemSerial;
        string systemName;
        LoadingInfo loadingInfo;
        DateTime startTime;
        DateTime stopTime;
        string uwsID;

        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            systemName = "RADVNS1";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc;UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc;User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            loadingInfo = new LoadingInfo(connectionString);
            startTime = Convert.ToDateTime("2024-03-29 00:00:00");
            stopTime = Convert.ToDateTime("2024-03-31 00:00:00");
            uwsID = "22";
        }
        
        [Test]
        public void Test_GetMaxUWSID()
        {
            // Arrange

            // Act
            int result = loadingInfo.GetMaxUWSID();

            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        //[Ignore ("Probably not used, since the query is invalid")]
        public void Test_GetSystemInfo()
        {

            // Arrange

            // Act
            DataTable result = loadingInfo.GetSystemInfo(uwsID);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetLoadingPeriod()
        {

            // Arrange

            // Act
            DataTable result = loadingInfo.GetLoadingPeriod(uwsID);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetUWSRetentionDay()
        {

            // Arrange

            // Act
            IDictionary<string, int> result = loadingInfo.GetUWSRetentionDay();

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Test_GetExpertReportRetentionDay()
        {

            // Arrange

            // Act
            IDictionary<string, int> result = loadingInfo.GetExpertReportRetentionDay();

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Test_GetQNMRetentionDay()
        {

            // Arrange

            // Act
            IDictionary<string, int> result = loadingInfo.GetQNMRetentionDay();

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Test_GetPathwayRetentionDay()
        {

            // Arrange

            // Act
            IDictionary<string, int> result = loadingInfo.GetPathwayRetentionDay();

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Test_GetUWSFileName()
        {

            // Arrange
            DateTime uploadedtime = Convert.ToDateTime("2024-04-09 14:45:24");

            // Act
            List<string> result = loadingInfo.GetUWSFileName(systemSerial, uploadedtime);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_Insert()
        {

            // Arrange
            int tempUWSID = 1;
            int customerId = 1032;

            // Act
            loadingInfo.Insert(tempUWSID, customerId);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_Update()
        {
            // Arrange

            // Act
            loadingInfo.Update("Test_FilePath", systemSerial, "0", "4", uwsID);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_UpdateByUWSID()
        {
            // Arrange

            // Act
            loadingInfo.Update(uwsID);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_UpdateCollectionTime_ByType()
        {
            // Arrange

            // Act
            loadingInfo.UpdateCollectionTime(Convert.ToInt16(uwsID), systemName, startTime, stopTime, 4);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_UpdateCollectionTime()
        {
            // Arrange

            // Act
            loadingInfo.UpdateCollectionTime(Convert.ToInt16(uwsID), startTime, stopTime);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_Update_ByType()
        {
            // Arrange

            // Act
            loadingInfo.Update(Convert.ToInt16(uwsID), systemName, startTime, stopTime, 4);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_UpdateLoadedTime()
        {
            // Arrange

            // Act
            loadingInfo.UpdateLoadedTime(Convert.ToInt16(uwsID));

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_UpdateFileStat()
        {
            // Arrange

            // Act
            loadingInfo.UpdateFileStat(systemSerial, stopTime);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_UpdateLoadingStatus()
        {
            // Arrange

            // Act
            loadingInfo.UpdateLoadingStatus(Convert.ToInt16(uwsID), "Sned");

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_UpdateStopTime()
        {
            // Arrange

            // Act
            loadingInfo.UpdateStopTime("2024-05-28 00:00:00", Convert.ToInt16(uwsID));

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_UpdateUWSRelayTime()
        {
            // Arrange

            // Act
            loadingInfo.UpdateUWSRelayTime(systemSerial, Convert.ToInt16(uwsID), DateTime.Now, DateTime.Now, "Test_File", 0, 0, "Test_RDS");

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_UpdateUWSRelayTime_CollectionTime()
        {
            // Arrange

            // Act
            loadingInfo.UpdateUWSRelayTime(systemSerial, Convert.ToInt16(uwsID), DateTime.Now, DateTime.Now, startTime, stopTime, "Test_File", 0, "Test_RDS");

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_GetLoadingInfo()
        {
            // Arrange

            // Act
            DataTable result = loadingInfo.GetLoadingInfo(Convert.ToInt16(uwsID));

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        [Ignore("Not used anymore")]
        public void Test_GetUMPFullFileName()
        {
            // Arrange

            // Act
            string result = loadingInfo.GetUMPFullFileName("", systemSerial, false);

            // Assert
            Assert.That(result.Length, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetLoadCompleteTime()
        {
            // Arrange

            // Act
            string result = loadingInfo.GetLoadCompleteTime("PK0012.402", systemSerial);

            // Assert
            Assert.That(result.Length, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetLoadingInfo_ByTime()
        {

            // Arrange

            // Act
            DataTable result = loadingInfo.GetLoadingInfo(systemSerial, startTime, stopTime);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetLoadFailedInfo()
        {

            // Arrange

            // Act
            DataTable result = loadingInfo.GetLoadFailedInfo(Convert.ToDateTime("2024-05-22 19:00:00"));

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetLoadedInfo()
        {

            // Arrange

            // Act
            DataTable result = loadingInfo.GetLoadedInfo(systemSerial, Convert.ToDateTime("2024-04-10 13:00:00"), Convert.ToDateTime("2024-04-10 14:00:00"));

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetInProgressInfo()
        {

            // Arrange

            // Act
            DataTable result = loadingInfo.GetInProgressInfo(systemSerial, Convert.ToDateTime("2024-04-10 13:00:00"), Convert.ToDateTime("2024-04-10 14:00:00"));

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_UpdateStatus()
        {
            // Arrange
            List<LoadingInfoParameter> updateLoadingInfoStatusList = new List<LoadingInfoParameter>();
            updateLoadingInfoStatusList.Add(new LoadingInfoParameter
            {
                SystemSerial = systemSerial,
                SampleType = 0,
                StartTime = startTime
            });
            // Act
            loadingInfo.UpdateStatus(updateLoadingInfoStatusList);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_UpdateReloadTime()
        {
            // Arrange

            // Act
            loadingInfo.UpdateReloadTime(systemSerial, uwsID, "u3076337.402", DateTime.Now);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_UpdateInstanceID()
        {
            // Arrange

            // Act
            loadingInfo.UpdateInstanceID(Convert.ToInt32(uwsID), "");

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_GetLoadInfoForToday()
        {
            // Arrange

            // Act
            List<long> results = loadingInfo.GetLoadInfoForToday("i-08ff82f13f2c428bd", Convert.ToDateTime("2024-04-10 00:00:00"));

            // Assert
            Assert.That(results, !Is.Null);
            Assert.That(results.Count, Is.GreaterThan(0));
        }

        [Test]
        [Ignore("For AWS RA only")]
        public void Test_GetRdsLoadInfoForToday()
        {
            // Arrange

            // Act
            List<long> results = loadingInfo.GetRdsLoadInfoForToday("", DateTime.Now);

            // Assert
            Assert.That(results.Count, Is.GreaterThan(0));
        }

        [Test]
        [Ignore("For AWS RA only")]
        public void Test_GetRdsOtherLoadInfoForToday()
        {
            // Arrange

            // Act
            List<long> results = loadingInfo.GetRdsOtherLoadInfoForToday("", DateTime.Now);

            // Assert
            Assert.That(results.Count, Is.GreaterThan(0));
        }


        [Test]
        public void Test_DeleteLoadingInfos()
        {
            // Arrange

            // Act
            loadingInfo.DeleteLoadingInfoOlderThanXDaysAgo(300);
            loadingInfo.DeleteLoadingInfoByUWSID(23);
            loadingInfo.DeleteLoadingInfoByFileName("u0664118.180");

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_GetFirstUWSLoadRecordByUWSName()
        {
            // Arrange

            // Act
            string result = loadingInfo.GetFirstUWSLoadRecordByUWSName("u0668574.402");

            // Assert
            Assert.That(result.Length, Is.GreaterThan(0));
        }

        [Test]
        [Ignore("Not used anymore")]
        public void Test_GetLoadHistoryForTransmonList()
        {
            // Arrange

            // Act
            DataTable result = loadingInfo.GetLoadHistoryForTransmonList(DateTime.Now);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        [Ignore("Not used anymore")]
        public void Test_GetLoadHistory()
        {
            // Arrange

            // Act
            DataTable result = loadingInfo.GetLoadHistory();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetInProcessingLoad()
        {
            // Arrange

            // Act
            DataTable result = loadingInfo.GetInProcessingLoad();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_BulkInsertMySQL()
        {
            // Arrange
            DataTable bulkTable = new DataTable();

            // Act
            loadingInfo.BulkInsertMySQL(bulkTable, "080627_CPU_2024_3_29");

            // Assert
            Assert.Pass();
        }
    }
}
