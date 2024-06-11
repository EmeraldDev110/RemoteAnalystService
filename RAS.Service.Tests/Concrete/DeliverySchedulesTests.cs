using NUnit.Framework;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAS.Service.Tests.Concrete
{
    public class DeliverySchedulesTests
    {
        string systemSerial;
        string connectionString;
        DeliverySchedules deliverySchedules;

        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc" + systemSerial + ";UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc" + systemSerial + ";User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            deliverySchedules = new DeliverySchedules(connectionString);
        }

        [Test]
        [Ignore("Not used anymore")]
        public void Test_GetSchdules()
        {
            // Arrange

            // Act
            DataTable result = deliverySchedules.GetSchdules(3);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        [Ignore("Not used anymore")]
        public void Test_GetSchduleData()
        {
            // Arrange

            // Act
            DataTable result = deliverySchedules.GetSchduleData(3);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        [Ignore("Not used anymore")]
        public void Test_GetQTSchdule()
        {
            // Arrange

            // Act
            DataTable result = deliverySchedules.GetQTSchdule();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        [Ignore("Not used anymore")]
        public void Test_GetDPASchdule()
        {
            // Arrange

            // Act
            DataTable result = deliverySchedules.GetDPASchdule();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        [Ignore("Not used anymore")]
        public void Test_GetTPSSchdule()
        {
            // Arrange

            // Act
            DataTable result = deliverySchedules.GetTPSSchdule();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }
    }
}
