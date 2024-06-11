using NUnit.Framework;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAS.Service.Tests.Concrete
{
    public class LoadingStatusDetailTests
    {
        string connectionString;
        string systemSerial;
        string uwsFileName;
        string instanceID;
        LoadingStatusDetail loadingStatusDetail;

        [SetUp]
        public void Setup(){
            systemSerial = "080627";
            instanceID = "i-0228d14942aa024fc";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc;UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc;User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            loadingStatusDetail = new LoadingStatusDetail(connectionString);
            uwsFileName = "u0668574.402";
        }

        [Test]
        public void Test_GetProcessingTime()
        {

            // Arrange

            // Act
            IDictionary<int, DateTime> result = loadingStatusDetail.GetProcessingTime(uwsFileName, systemSerial);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void Test_GetProcessingTime_LoadingQueID()
        {

            // Arrange

            // Act
            int result = loadingStatusDetail.GetCurrentQueueLength(instanceID);

            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void Test_CheckDuplicatedUWS()
        {

            // Arrange

            // Act
            bool result = loadingStatusDetail.CheckDuplicatedUWS("UMMV02_212583859979545177_080984_0530_1900_0530_2000_12E_26512632_U4948824.402");

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Test_InsertLoadingStatus()
        {

            // Arrange

            // Act
            bool result = loadingStatusDetail.InsertLoadingStatus(
                uwsFileName, "pauluszematis@idelji.com", DateTime.Now, systemSerial, "jobPoolName", 1, 0, "0", "");

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Test_DeleteLoadingInfo()
        {

            // Arrange

            // Act
            loadingStatusDetail.DeleteLoadingInfo(uwsFileName);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_UpdateLoadingStatusDetail()
        {

            // Arrange

            // Act
            bool result = loadingStatusDetail.UpdateLoadingStatusDetail("0", DateTime.Now, uwsFileName, systemSerial);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Test_UpdateFileSize()
        {

            // Arrange

            // Act
            loadingStatusDetail.UpdateFileSize(uwsFileName, 0);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_GetLoadingStatusDetail()
        {
            // Arrange

            // Act
            DataTable result = loadingStatusDetail.GetLoadingStatusDetail(instanceID);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetStoppedJobs()
        {
            // Arrange

            // Act
            DataTable result = loadingStatusDetail.GetStoppedJobs(instanceID);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetCurrentLoadCount()
        {
            // Arrange

            // Act
            int result = loadingStatusDetail.GetCurrentLoadCount(systemSerial, instanceID);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.GreaterThan(0));
        }
    }
}
