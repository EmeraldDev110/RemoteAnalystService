using NUnit.Framework;
using RemoteAnalyst.Repository.Concrete;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using RemoteAnalyst.Repository.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAS.Service.Tests.Concrete
{
    public class SystemWeekThresholdsTests
    {
        string connectionString;
        SystemWeekThresholdsRepository systemWeekThresholds;
        string systemSerial;
        int thresholdTypeId;

        [SetUp]
        public void Setup()
        {
            thresholdTypeId = 0; // Business
            systemSerial = "080627";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc;UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc;User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            systemWeekThresholds = new SystemWeekThresholdsRepository();
        }

        [Test]
        public void Test_GetCpuBusy()
        {
            // Arrange

            // Act
            DataTable result = systemWeekThresholds.GetCpuBusy(systemSerial, thresholdTypeId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
            // Assert.Pass();
        }

        [Test]
        public void Test_GetCpuQueueLength()
        {
            // Arrange

            // Act
            DataTable result = systemWeekThresholds.GetCpuQueueLength(systemSerial, thresholdTypeId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
            // Assert.Pass();
        }

        [Test]
        public void Test_GetIpuBusy()
        {
            // Arrange

            // Act
            DataTable result = systemWeekThresholds.GetIpuBusy(systemSerial, thresholdTypeId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
            // Assert.Pass();
        }

        [Test]
        public void Test_GetIpuQueueLength()
        {
            // Arrange

            // Act
            DataTable result = systemWeekThresholds.GetIpuQueueLength(systemSerial, thresholdTypeId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
            // Assert.Pass();
        }

        [Test]
        public void Test_GetDiskQueueLength()
        {
            // Arrange

            // Act
            DataTable result = systemWeekThresholds.GetDiskQueueLength(systemSerial, thresholdTypeId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
            // Assert.Pass();
        }

        [Test]
        public void Test_GetDiskDP2()
        {
            // Arrange

            // Act
            DataTable result = systemWeekThresholds.GetDiskDP2(systemSerial, thresholdTypeId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
            // Assert.Pass();
        }

        [Test]
        public void Test_GetStorage()
        {
            // Arrange

            // Act
            DataTable result = systemWeekThresholds.GetStorage(systemSerial, thresholdTypeId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
            // Assert.Pass();
        }

    }
}
