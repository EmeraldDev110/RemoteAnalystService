using NUnit.Framework;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;
using RemoteAnalyst.Repository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;

namespace RAS.Service.Tests.Concrete
{
    public class CurrentTablesTests
    {
        string systemSerial;
        string connectionString;
        CurrentTableRepository currentTables;
        string tableName;
        DateTime startDateTime;
        DateTime stopDateTime;
        long interval;

        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc" + systemSerial + ";UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc" + systemSerial + ";User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            currentTables = new CurrentTableRepository(connectionString);
            tableName = "080627_CPU_2024_3_23";
            startDateTime = Convert.ToDateTime("2024-03-23 00:00:00");
            stopDateTime = Convert.ToDateTime("2024-03-23 01:00:00");
            interval = 900;
        }

        [Test]
        public void Test_CurrentTables_InsertEntry()
        {
            // Arrange

            // Act
            currentTables.InsertEntry(tableName, 1, interval, startDateTime, systemSerial, "L01");

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_GetInterval()
        {
            // Arrange

            // Act
            long result = currentTables.GetInterval(tableName);

            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetLatestIntervl()
        {
            // Arrange

            // Act
            long result = currentTables.GetLatestIntervl();

            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetEntities()
        {
            // Arrange

            // Act
            List<int> result = currentTables.GetEntities(startDateTime, stopDateTime, interval);

            // Assert
            Assert.That(result, !Is.Null);
            Assert.That(result.Count, Is.GreaterThan(0));
        }
    }
}
