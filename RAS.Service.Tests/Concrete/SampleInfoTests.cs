using NUnit.Framework;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAS.Service.Tests.Concrete
{
    public class SampleInfoTests
    {
        string connectionString;
        string systemSerial;
        SampleInfo sampleInfo;

        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc;UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc;User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            sampleInfo = new SampleInfo(connectionString);
        }

        [Test]
        public void Test_SampleInfo_InsertNewEntry()
        {
            // Arrange

            // Act
            sampleInfo.InsertNewEntry(1, "RADVNS1", systemSerial, DateTime.Today, DateTime.Now, 900, 1, "SYSCONTENT", 1);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_SampleInfo_CheckDuplicateData()
        {
            // Arrange

            // Act
            bool result = sampleInfo.CheckDuplicateData(systemSerial, 
                Convert.ToDateTime("2024-05-27 00:00:00"),
                Convert.ToDateTime("2024-05-27 01:00:00"),
                false);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Test_SampleInfo_UpdateExpireInfo()
        {
            // Arrange

            // Act
            sampleInfo.UpdateExpireInfo(DateTime.Today, "1");

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_SampleInfo_UpdateStopTime()
        {
            // Arrange

            // Act
            sampleInfo.UpdateStopTime("2024-05-27 00:00:00", "1");

            // Assert
            Assert.Pass();
        }
    }
}
