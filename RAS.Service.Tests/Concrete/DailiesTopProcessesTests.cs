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
    public class DailiesTopProcessesTests
    {
        string systemSerial;
        string connectionString;
        DailiesTopProcessRepository dailiesTopProcesses;
        DateTime startDateTime;
        DateTime stopDateTime;

        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc" + systemSerial + ";UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc" + systemSerial + ";User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            dailiesTopProcesses = new DailiesTopProcessRepository(connectionString);
            startDateTime = Convert.ToDateTime("2024-03-30 00:00:00");
            stopDateTime = Convert.ToDateTime("2024-03-30 01:00:00");
        }


        [Test]
        public void Test_GetProcessBusyData()
        {
            // Arrange

            // Act
            DataTable result = dailiesTopProcesses.GetProcessBusyData(startDateTime, stopDateTime);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetProcessQueueData()
        {
            // Arrange

            // Act
            DataTable result = dailiesTopProcesses.GetProcessQueueData(startDateTime, stopDateTime);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }
    }
}
