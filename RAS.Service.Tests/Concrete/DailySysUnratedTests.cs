using NUnit.Framework;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;
using RemoteAnalyst.Repository.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RAS.Service.Tests.Concrete
{
    public class DailySysUnratedTests
    {
        string systemSerial;
        string connectionString;
        DailySysUnratedRepository dailySysUnrated;

        DateTime startDate;
        DateTime endDate;
        DateTime oldDate;
        int attributeID;

        [SetUp]
        public void Setup()
        {
            startDate = new DateTime(2024, 3, 30);
            endDate = new DateTime(2024, 3, 31);
            systemSerial = "080627";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc" + systemSerial + ";UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc" + systemSerial + ";User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            dailySysUnrated = new DailySysUnratedRepository(connectionString);
            attributeID = 10;
            oldDate = Convert.ToDateTime("2024-03-30 00:00:00");
        }

        [Test]
        public void Test_GetAllSystemData()
        {
            // Arrange

            // Act
            DataTable result = dailySysUnrated.GetAllSystemData();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetDataDate()
        {
            // Arrange

            // Act
            DataSet result = dailySysUnrated.GetDataDate(attributeID, startDate, endDate, systemSerial);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataSet>());
            Assert.That(result.Tables.Count, Is.GreaterThan(0));
        }


        [Test]
        public void Test_GetHourlyData()
        {
            // Arrange

            // Act
            DataTable result = dailySysUnrated.GetHourlyData(systemSerial, 
                attributeID, "TMF Transactions", 3, 2024);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_DeleteData()
        {
            // Arrange

            // Act
            dailySysUnrated.DeleteData(oldDate);

            // Assert
            Assert.Pass();
        }


    }
}
