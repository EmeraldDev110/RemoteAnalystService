using NUnit.Framework;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;
using RemoteAnalyst.Repository.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAS.Service.Tests.Concrete
{
    public class TrendTableTests
    {
        string systemSerial;
        string connectionString;
        CPUTrendTableRepository cpuTrendTable;
        string fromTimestamp;
        string toTimestamp;

        [SetUp]
        public void Setup()
        {
            fromTimestamp = "2024-03-29 00:00:00";
            toTimestamp = "2024-03-30 00:00:00";
            systemSerial = "080627";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc" + systemSerial + ";UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc" + systemSerial + ";User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            cpuTrendTable = new CPUTrendTableRepository(connectionString);
        }

        [Test]
        public void Test_GetCPUBusyInterval()
        {
            // Arrange

            // Act
            DataTable result = cpuTrendTable.GetCPUBusyInterval(
                fromTimestamp, toTimestamp
                //"2021-04-30 00:00:00", "2021-04-30 01:00:00"
                );
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }


        [Test]
        public void Test_GetCPUQueueInterval()
        {
            // Arrange

            // Act
            DataTable result = cpuTrendTable.GetCPUQueueInterval(
                fromTimestamp, toTimestamp
                //"2021-04-30 00:00:00", "2021-04-30 01:00:00"
                );
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetIPUBusyInterval()
        {
            // Arrange

            // Act
            DataTable result = cpuTrendTable.GetIPUBusyInterval(fromTimestamp, toTimestamp);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetIPUQueueInterval()
        {
            // Arrange

            // Act
            DataTable result = cpuTrendTable.GetIPUQueueInterval(fromTimestamp, toTimestamp);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }


        [Test]
        public void Test_GetDiskTrendPerInterval()
        {
            // Arrange
            DiskTrendTableRepository diskTrendTable = new DiskTrendTableRepository(connectionString);
            // Act
            DataTable result = diskTrendTable.GetDiskTrendPerInterval(fromTimestamp, toTimestamp);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

    }
}
