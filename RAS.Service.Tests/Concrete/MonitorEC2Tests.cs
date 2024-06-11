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
    public class MonitorEC2Tests
    {
        string connectionString;
        string systemSerial;
        string systemName;
        MonitorEC2 monitorEC2;

        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            systemName = "RADVNS1";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc;UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc;User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            monitorEC2 = new MonitorEC2(connectionString);
        }

        [Test]
        [Ignore("For AWS RA only")]
        public void Test_InsertEntry()
        {
            // Arrange
            string instanceId = "i-08ff82f13f2c428bd";
            string ec2Name = "RADVNS1";
            string instanceName = "RADVNS1";
            double cpuBusy = 0.0;
            int todayLoadCount = 0;
            double todayLoadSize = 0.0;
            double cpuBusyAverage = 0.0;
            double cpuBusyPeak = 0.0;

            // Act
            monitorEC2.InsertEntry(instanceId, ec2Name, instanceName, cpuBusy, todayLoadCount, todayLoadSize, cpuBusyAverage, cpuBusyPeak);

            // Assert
            Assert.Pass();
        }

        [Test]
        [Ignore("For AWS RA only")]
        public void Test_UpdateEntry()
        {
            // Arrange
            string instanceId = "i-08ff82f13f2c428bd";
            string ec2Name = "RADVNS1";
            string instanceName = "RADVNS1";
            double cpuBusy = 0.0;
            int todayLoadCount = 0;
            double todayLoadSize = 0.0;
            double cpuBusyAverage = 0.0;
            double cpuBusyPeak = 0.0;

            // Act
            monitorEC2.UpdateEntry(instanceId, ec2Name, instanceName, cpuBusy, todayLoadCount, todayLoadSize, cpuBusyAverage, cpuBusyPeak);

            // Assert
            Assert.Pass();
        }

        [Test]
        [Ignore("For AWS RA only")]
        public void Test_CheckDataEntry()
        {
            // Arrange
            string instanceId = "i-08ff82f13f2c428bd";

            // Act
            bool result = monitorEC2.CheckDataEntry(instanceId);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        [Ignore("For AWS RA only")]
        public void Test_DeleteAllEntry()
        {
            // Arrange

            // Act
            monitorEC2.DeleteAllEntry();

            // Assert
            Assert.Pass();
        }

        [Test]
        [Ignore("For AWS RA only")]
        public void Test_GetEC2LoaderIPInformation()
        {
            // Arrange

            // Act
            DataTable result = monitorEC2.GetEC2LoaderIPInformation();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));

        }

        [Test]
        [Ignore("For AWS RA only")]
        public void Test_CheckIsActive()
        {
            // Arrange
            string instanceId = "i-08ff82f13f2c428bd";

            // Act
            bool result = monitorEC2.CheckIsActive(instanceId);

            // Assert
            Assert.That(result, Is.True);

        }
    }
}
