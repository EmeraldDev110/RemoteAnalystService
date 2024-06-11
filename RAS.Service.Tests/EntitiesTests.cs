using NUnit.Framework;
using RemoteAnalyst.Repository.MySQLConcrete.RemoteAnalystSPAM;
using RemoteAnalyst.Repository.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAS.Service.Tests
{
    public class EntitiesTests
    {
        string connectionString;
        string systemSerial;
        string tableName;
        string className;
        Entities entities;
        DateTime fromDateTime;
        DateTime toDateTime;
        // string volume, string subVol, string fileName

        [SetUp]
        public void Setup()
        {
            fromDateTime = Convert.ToDateTime("2024-03-29 00:00:00");
            toDateTime = Convert.ToDateTime("2024-03-29 01:00:00");
            systemSerial = "080627";
            tableName = systemSerial + "_CPU_2024_3_29";
            className = "CPUEntity";
            //  connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc" + systemSerial + ";UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc" + systemSerial + ";User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            entities = new Entities(connectionString);
        }

        [Test]
        public void Test_Entities_GetTimeIntervalCountPerEntity()
        {
            // Arrange
            List<string> entityTables = new List<string> { tableName };

            // Act
            DataTable result = entities.GetTimeIntervalCountPerEntity(entityTables);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_Entities_GetCPUCount()
        {
            // Arrange

            // Act
            int result = entities.GetCPUCount(className, tableName, fromDateTime, toDateTime);

            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void Test_Entities_CheckTime()
        {
            // Arrange
            
            // Act
            bool result = entities.CheckTime(className, tableName, fromDateTime, toDateTime);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Test_Entities_GetOpenerCPUCount()
        {
            // Arrange

            // Act
            int result = entities.GetOpenerCPUCount("080627_FILE_2024_3_29", fromDateTime, toDateTime);

            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }

    }
}
