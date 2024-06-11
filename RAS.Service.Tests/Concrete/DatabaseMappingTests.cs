using NUnit.Framework;
using RemoteAnalyst.Repository.Models;
using RemoteAnalyst.Repository.Repositories;
using System.Collections.Generic;
using System.Data;

namespace RAS.Service.Tests.Concrete
{
    public class DatabaseMappingTests
    {
        DatabaseMappingRepository dm;
        string systemSerial;
        string connectionString;
        string systemConnectionString;

        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc;UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc;User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            //systemConnectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc" + systemSerial + ";UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            systemConnectionString = "Server=13.56.143.245;Database=pmc" + systemSerial + ";User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            dm = new DatabaseMappingRepository();
        }

        [Test]
        public void Test_GetConnectionString()
        {
            // Arrange

            // Act
            string result = dm.GetConnectionString(systemSerial);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetAllConnectionString()
        {
            // Arrange

            // Act
            ICollection<DatabaseMapping> result = dm.GetAllConnectionStrings();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_CheckDatabase()
        {
            // Arrange

            // Act
            bool result = dm.CheckDatabase(systemConnectionString);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Test_CheckConnection()
        {
            // Arrange

            // Act
            string result = dm.CheckConnection(connectionString);

            // Assert
            Assert.That(result.Length, Is.EqualTo(0));
        }

        [Test]
        public void Test_InsertNewEntry()
        {
            // Arrange

            // Act
            dm.InsertNewEntry("000000", connectionString);

            // Assert
            Assert.Pass();
        }

        //[Test]
        //public void Test_GetMySQLConnectionString_BySystemSerial()
        //{
        //    // Arrange

        //    // Act
        //    string result = dm.GetMySQLConnectionString(systemSerial);

        //    // Assert
        //    Assert.That(result, Is.Not.Null);
        //    Assert.That(result.Length, Is.GreaterThan(0));
        //}
        
        //[Test]
        //public void Test_GetMySQLConnectionString()
        //{
        //    // Arrange

        //    // Act
        //    string result = dm.GetMySQLConnectionString();

        //    // Assert
        //    Assert.That(result, Is.Not.Null);
        //    Assert.That(result.Length, Is.GreaterThan(0));
        //}

        //[Test]
        //public void Test_UpdateMySQLConnectionString()
        //{
        //    // Arrange

        //    // Act
        //    dm.UpdateMySQLConnectionString(systemSerial, connectionString);

        //    // Assert
        //    Assert.Pass();
        //}

        [Test]
        public void Test_GetAllDatabaseConnection()
        {
            // Arrange

            // Act
            DataTable result = dm.GetAllDatabaseConnection();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }


        [Test]
        [Ignore("Only applicable to RA (AWS)")]

        public void Test_GetRdsConnectionString()
        {
            // Arrange
            string rdsName = "pmc";

            // Act
            string result = dm.GetRdsConnectionString(rdsName);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.GreaterThan(0));
        }
    }
}