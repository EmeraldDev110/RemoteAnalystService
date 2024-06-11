using NUnit.Framework;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAS.Service.Tests.Concrete
{
    public class ProcessEntityTableTests
    {
        string systemSerial;
        string connectionString;
        List<string> processTableNames;
        ProcessEntityTable processEntity;
        DateTime startDateTime;
        DateTime stopDateTime;
        int pageSizeBytes;
        long interval;
        bool isIPU;

        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc" + systemSerial + ";UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc" + systemSerial + ";User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            processTableNames = new List<string>() { "080627_PROCESS_2024_3_29" };
            startDateTime = Convert.ToDateTime("2024-03-29 00:00:00");
            stopDateTime = Convert.ToDateTime("2024-03-29 01:00:00");
            processEntity = new ProcessEntityTable(connectionString);
            pageSizeBytes = 16384;
            interval = 900;
            isIPU = false;
        }

        [Test]
        public void Test_GetAllProcessByBusy()
        {
            // Arrange

        // Act
            DataTable result = processEntity.GetAllProcessByBusy(processTableNames, startDateTime, stopDateTime);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_CheckIPUColumn()
        {
            // Arrange

            // Act
            int result = processEntity.CheckIPUColumn(processTableNames[0], "pmc080627");

            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        [Ignore("Required condition not met in data set")]
        public void Test_GetTop20ProcessByBusyStatic()
        {
            // Arrange

            // Act
            DataTable result = processEntity.GetTop20ProcessByBusyStatic(processTableNames, startDateTime, stopDateTime, pageSizeBytes, interval, isIPU);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        [Ignore ("Required condition not met in data set")]
        public void Test_GetTop20ProcessByBusyDynamic()
        {
            // Arrange

            // Act
            DataTable result = processEntity.GetTop20ProcessByBusyDynamic(processTableNames, startDateTime, stopDateTime, pageSizeBytes, interval, isIPU);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }
        [Test]
        public void Test_GetTop20ProcessByQueueStatic()
        {
            // Arrange

            // Act
            DataTable result = processEntity.GetTop20ProcessByQueueStatic(processTableNames, startDateTime, stopDateTime, pageSizeBytes, interval, isIPU);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetTop20ProcessByQueueDynamic()
        {
            // Arrange

            // Act
            DataTable result = processEntity.GetTop20ProcessByQueueDynamic(processTableNames, startDateTime, stopDateTime, pageSizeBytes, interval, isIPU);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        [Ignore("Required condition not met in data set")]
        public void Test_GetTop20ProcessByAbort()
        {
            // Arrange

            // Act
            DataTable result = processEntity.GetTop20ProcessByAbort(processTableNames, startDateTime, stopDateTime, pageSizeBytes, isIPU);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }
    }
}
