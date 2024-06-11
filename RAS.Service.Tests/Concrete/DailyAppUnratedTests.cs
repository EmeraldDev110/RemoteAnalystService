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
    public class DailyAppUnratedTests
    {
        string systemSerial;
        string connectionString;
        DailyAppUnratedRepository dailiesAppUnrated;
        DateTime oldDate;

        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc" + systemSerial + ";UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc" + systemSerial + ";User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            dailiesAppUnrated = new DailyAppUnratedRepository(connectionString);
            oldDate = Convert.ToDateTime("2024-03-28 00:00:00");
        }


        [Test]
        public void Test_GetAllApplicationData()
        {
            // Arrange

            // Act
            DataTable result = dailiesAppUnrated.GetAllApplicationData();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetHourlyData()
        {
            // Arrange

            // Act
            DataTable result = dailiesAppUnrated.GetHourlyData(systemSerial, 23, "0", 4, 2024);

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
            dailiesAppUnrated.DeleteData(oldDate);

            // Assert
            Assert.Pass();
        }
    }
}
