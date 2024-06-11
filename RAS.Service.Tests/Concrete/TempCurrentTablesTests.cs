using NUnit.Framework;
using RemoteAnalyst.Repository.Repositories;
using System;

namespace RAS.Service.Tests.Concrete
{
    public class TempCurrentTablesTests
    {
        string systemSerial;
        string connectionString;
        string tableName;
        TempCurrentTableRepository tempCurrentTables;

        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc" + systemSerial + ";UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc" + systemSerial + ";User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            tempCurrentTables = new TempCurrentTableRepository(connectionString);
            tableName = "080627_CPU_2024_3_23";
        }

        [Test]
        public void Test_InsertCurrentTable()
        {
            // Arrange

            // Act
            tempCurrentTables.InsertCurrentTable(tableName, 1, 900, DateTime.Today, systemSerial, "L01");

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_GetInterval()
        {
            // Arrange

            // Act
            long result = tempCurrentTables.GetInterval("080627_CPU_2024_3_29");

            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }


        [Test]
        public void Test_DeleteCurrentTable()
        {
            // Arrange

            // Act
            tempCurrentTables.DeleteCurrentTable(tableName);

            // Assert
            Assert.Pass();
        }
    }
}
