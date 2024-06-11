using NUnit.Framework;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;

namespace RAS.Service.Tests.Concrete
{
    public class SchedulesTests
    {
        string connectionString;
        int scheduleID;
        Schedules schedules;

        [SetUp]
        public void Setup()
        {
            scheduleID = 1;
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc;UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc;User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            schedules = new Schedules(connectionString);
        }

        [Test]
        public void Test_GetSchedules()
        {
            // Arrange

            // Act
            DataTable result = schedules.GetSchedules(12);  // 12 BatchSequence

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetPinInfo()
        {
            // Arrange

            // Act
            DataTable result = schedules.GetPinInfo(1550);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetQTParam()
        {
            // Arrange

            // Act
            string result = schedules.GetQTParam(508);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetDDParam()
        {
            // Arrange

            // Act
            string result = schedules.GetDDParam(scheduleID);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.GreaterThan(0));
        }
    }
}
