using NUnit.Framework;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;
using RemoteAnalyst.Repository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAS.Service.Tests.Concrete
{
    public class DailyCPUDatasTests
    {
        string systemSerial;
        string connectionString;
        string databaseName;
        DailyCPUDataRepository dailyCPUDatas;

        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            databaseName = "pmc" + systemSerial;
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc" + systemSerial + ";UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc" + systemSerial + ";User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            dailyCPUDatas = new DailyCPUDataRepository(connectionString);
        }

        [Test]
        public void Test_CheckTableName()
        {
            // Arrange

            // Act
            bool result = dailyCPUDatas.CheckTableName(databaseName);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        [Ignore("Need to figure out how to implement")]
        public void Test_CreateDailyCPUDatas()
        {
            // Arrange

            // Act
            dailyCPUDatas.CreateDailyCPUDatas();

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_CheckDailiesTopProcessesTableName()
        {
            // Arrange

            // Act
            bool result = dailyCPUDatas.CheckDailiesTopProcessesTableName(databaseName);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        [Ignore("Need to figure out how to implement")]
        public void Test_CreateDailiesTopProcessesTable()
        {
            // Arrange

            // Act
            dailyCPUDatas.CreateDailiesTopProcessesTable();

            // Assert
            Assert.Pass();
        }


    }
}
