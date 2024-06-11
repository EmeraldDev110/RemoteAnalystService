using NUnit.Framework;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAS.Service.Tests.Concrete
{
    public class NotificationPreferencesTests
    {
        string connectionString;
        string systemSerial;
        int customerID;
        NotificationPreferences notificationPreferences;

        [SetUp]
        public void Setup()
        {
            systemSerial = "078066";
            customerID = 731;
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc;UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc;User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            notificationPreferences = new NotificationPreferences(connectionString);
        }

        [Test]
        public void Test_CheckIsEveryLoad()
        {

            // Arrange

            // Act
            DataTable result = notificationPreferences.CheckIsEveryLoad(systemSerial, customerID);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_CheckIsDailyLoad()
        {

            // Arrange

            // Act
            bool result = notificationPreferences.CheckIsDailyLoad(systemSerial, customerID);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        [Ignore("Not used anymore")]
        public void Test_GetEveryHourSystems()
        {

            // Arrange

            // Act
            DataTable result = notificationPreferences.GetEveryHourSystems();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        [Ignore("Not used anymore")]
        public void Test_GetEveryDailySystems()
        {

            // Arrange

            // Act
            DataTable result = notificationPreferences.GetEveryDailySystems();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        [Ignore("Not used anymore")]
        public void Test_GetEveryWeeklySystems()
        {

            // Arrange

            // Act
            DataTable result = notificationPreferences.GetEveryWeeklySystems();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }
    }
}
