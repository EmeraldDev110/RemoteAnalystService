using NUnit.Framework;
using RemoteAnalyst.Repository.Repositories;
using System;
using System.Collections.Generic;
using System.Data;

namespace RAS.Service.Tests.Concrete
{
    public class SystemTblTests
    {
        string connectionString;
        SystemRepository systemTbl;
        string systemSerial;

        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc;UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc;User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            systemTbl = new SystemRepository();
        }

        [Test]
        public void Test_GetCompanyName()
        {
            // Arrange

            // Act
            string result = systemTbl.GetCompanyName(systemSerial);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.GreaterThan(0));

        }

        [Test]
        public void Test_GetCompanyID()
        {
            // Arrange

            // Act
            int result = systemTbl.GetCompanyID(systemSerial);

            // Assert
            Assert.That(result, Is.GreaterThan(0));

        }

        [Test]
        public void Test_GetRetentionDay()
        {
            // Arrange

            // Act
            int result = systemTbl.GetRetentionDay(systemSerial);

            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetEndDate()
        {
            // Arrange

            // Act
            IDictionary<string, string> result = systemTbl.GetEndDate(systemSerial);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Dictionary<string, string>>());
        }

        [Test]
        public void Test_GetMeasFH()
        {
            // Arrange

            // Act
            string result = systemTbl.GetMeasFH(systemSerial);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetAttachmentInEmail()
        {
            // Arrange

            // Act
            bool result = systemTbl.GetAttachmentInEmail(systemSerial);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Test_GetSystemName()
        {
            // Arrange

            // Act
            string result = systemTbl.GetSystemName(systemSerial);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetExpiredSystem()
        {
            // Arrange

            // Act
            IDictionary<string, string> result = systemTbl.GetExpiredSystem();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Dictionary<string, string>>());
        }

        [Test]
        public void Test_AllowOverlappingData()
        {
            // Arrange

            // Act
            bool result = systemTbl.AllowOverlappingData(systemSerial);

            // Assert
            Assert.That(result, !Is.True);
        }

        [Test]
        public void Test_isProcessDirectlySystem()
        {
            // Arrange

            // Act
            bool result = systemTbl.isProcessDirectlySystem(systemSerial);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void Test_GetTimeZone()
        {
            // Arrange

            // Act
            int result = systemTbl.GetTimeZone(systemSerial);

            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        [Ignore ("This field for RADVNS (080627) not populated)")]
        public void Test_GetCountryCode()
        {
            // Arrange

            // Act
            string result = systemTbl.GetCountryCode(systemSerial);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetArchiveRetensionValue()
        {
            // Arrange

            // Act
            int result = systemTbl.GetArchiveRetensionValue(systemSerial);

            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetTrendMonth()
        {
            // Arrange

            // Act
            int result = systemTbl.GetTrendMonth(systemSerial);

            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void Test_IsNTSSystem()
        {
            // Arrange

            // Act
            bool result = systemTbl.IsNTSSystem(systemSerial);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Test_GetTolerance()
        {
            // Arrange
            // Act
            DataTable result = systemTbl.GetTolerance(systemSerial);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetAllCompanySystemSerialAndName()
        {
            // Arrange
            // Act
            DataTable result = systemTbl.GetAllCompanySystemSerialAndName();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetLoadLimit()
        {
            // Arrange

            // Act
            int result = systemTbl.GetLoadLimit(systemSerial);

            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }
    }
}
