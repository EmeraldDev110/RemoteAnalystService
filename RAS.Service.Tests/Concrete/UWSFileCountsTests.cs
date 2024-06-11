using NUnit.Framework;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAS.Service.Tests.Concrete
{
    public class UWSFileCountsTests
    {
        string connectionString;
        string systemSerial;
        UWSFileCounts uwsFileCounts;
        DateTime dataDate;

        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc;UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc;User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            uwsFileCounts = new UWSFileCounts(connectionString);
            dataDate = Convert.ToDateTime("2024-05-29 00:00:00");
        }

        [Test]
        public void Test_InsertFileInfo()
        {
            // Arrange

            // Act
            uwsFileCounts.InsertFileInfo(systemSerial, DateTime.Today, "Test_File", 0, 0);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_CheckDuplicate()
        {
            // Arrange

            // Act
            bool result = uwsFileCounts.CheckDuplicate(systemSerial, "Test_File");

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Test_CheckDuplicate_ByDate()
        {
            // Arrange

            // Act
            bool result = uwsFileCounts.CheckDuplicate(systemSerial, dataDate);

            // Assert
            Assert.That(result, Is.True);
        }


        [Test]
        public void Test_UpdateActualFileCount()
        {
            // Arrange

            // Act
            uwsFileCounts.UpdateActualFileCount(systemSerial, DateTime.Today);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_GetExpectedFileCount()
        {
            // Arrange

            // Act
            int result = uwsFileCounts.GetExpectedFileCount(systemSerial, dataDate);

            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetActualFileCount()
        {
            // Arrange

            // Act
            int result = uwsFileCounts.GetActualFileCount(systemSerial, dataDate);

            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }
    }
}
