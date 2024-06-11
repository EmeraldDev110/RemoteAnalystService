using NUnit.Framework;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAS.Service.Tests.Concrete
{
    public class XVDailyEntityCleanerRepositoryTests
    {
        string connectionString;
        string systemSerial;
        XVDailyEntityCleanerRepository xvDailyEntityCleanerRepository;

        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc;UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc;User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            xvDailyEntityCleanerRepository = new XVDailyEntityCleanerRepository(connectionString);
        }

        [Test]
        public void Test_GetAllSystemInformation()
        {
            // Arrange

            // Act
            var result = xvDailyEntityCleanerRepository.GetAllSystemInformation();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<System.Data.DataTable>());
        }

        [Test]
        public void Test_GetXVDailyTables()
        {
            // Arrange

            // Act
            var result = xvDailyEntityCleanerRepository.GetXVDailyTables(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<System.Data.DataTable>());
        }

        [Test]
        public void Test_DeleteXVDailyTableByName()
        {
            // Arrange

            // Act
            xvDailyEntityCleanerRepository.DeleteXVDailyTableByName("FAKE_TABLE");

            // Assert
            Assert.Pass();
        }
    }
}
