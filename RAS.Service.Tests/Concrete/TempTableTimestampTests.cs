using NUnit.Framework;
using RemoteAnalyst.Repository.Repositories;
using System;

namespace RAS.Service.Tests.Concrete
{
    public class TempTableTimestampTests
    {
        string systemSerial;
        string connectionString;
        string tableName;
        TempTableTimestampRepository tempTableTimestamp;
        DateTime startDateTime;
        DateTime stopDateTime;

        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc" + systemSerial + ";UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc" + systemSerial + ";User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            tempTableTimestamp = new TempTableTimestampRepository(connectionString);
            tableName = "080627_CPU_2024_3_23";
            startDateTime = Convert.ToDateTime("2024-03-23 00:00:00");
            stopDateTime = Convert.ToDateTime("2024-03-23 01:00:00");
        }

        [Test]
        public void Test_InsertTempTimeStamp()
        {
            // Arrange

            // Act
            tempTableTimestamp.InsertTempTimeStamp(tableName, startDateTime, stopDateTime, "");

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_DeleteTempTimeStamp()
        {
            // Arrange

            // Act
            tempTableTimestamp.DeleteTempTimeStamp(tableName);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_CheckMySqlColumn()
        {
            // Arrange

            // Act
            bool result = tempTableTimestamp.CheckMySqlColumn("pmc080627", tableName, "FileName");

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        [Ignore("Need to figure out how to implement")]
        public void Test_AddFileNameColumnToTempTableTimestampTable()
        {
            // Arrange

            // Act
            tempTableTimestamp.AddFileNameColumnToTempTableTimestampTable();

            // Assert
            Assert.Pass();
        }
    }
}
