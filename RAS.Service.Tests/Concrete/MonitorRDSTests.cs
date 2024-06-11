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
    public class MonitorRDSTests
    {
        string connectionString;
        string systemSerial;
        string systemName;
        MonitorRDS monitorRDS;

        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            systemName = "RADVNS1";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc;UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc;User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            monitorRDS = new MonitorRDS(connectionString);
        }

        [Test]
        [Ignore("Only applicable to RA (AWS)")]
        public void Test_InsertEntry()
        {
            // Arrange
            string rdsName = "pr13";
            string rdsRealName = "RADVNS1";
            double cpuBusy = 0.0;
            double gbSize = 0.0;
            double freeSpace = 0.0;
            string todayLoadCount = "0";
            string todayLoadSize = "0";
            double cpuBusyAverage = 0.0;
            double cpuBusyPeak = 0.0;
            string displaySpace = "0";

            // Act
            monitorRDS.InsertEntry(rdsName, rdsRealName, cpuBusy, gbSize, freeSpace, 
                todayLoadCount, todayLoadSize, cpuBusyAverage, cpuBusyPeak, displaySpace);

            // Assert
            Assert.Pass();
        }

        [Test]
        [Ignore("Only applicable to RA (AWS)")]
        public void UpdateEntryNoCpuBusy()
        {
            // Arrange
            string rdsName = "pr13";
            string rdsRealName = "RADVNS1";
            double gbSize = 0.0;
            double freeSpace = 0.0;
            string todayLoadCount = "0";
            string todayLoadSize = "0";
            double cpuBusyAverage = 0.0;
            double cpuBusyPeak = 0.0;
            string displaySpace = "0";

            // Act
            monitorRDS.UpdateEntryNoCpuBusy(rdsName, rdsRealName, gbSize, freeSpace,
                todayLoadCount, todayLoadSize, cpuBusyAverage, cpuBusyPeak, displaySpace);

            // Assert
            Assert.Pass();
        }

        [Test]
        [Ignore("Only applicable to RA (AWS)")]
        public void Test_UpdateEntry()
        {
            // Arrange
            string rdsName = "pr13";
            string rdsRealName = "RADVNS1";
            double cpuBusy = 0.0;
            double gbSize = 0.0;
            double freeSpace = 0.0;
            string todayLoadCount = "0";
            string todayLoadSize = "0";
            double cpuBusyAverage = 0.0;
            double cpuBusyPeak = 0.0;
            string displaySpace = "0";

            // Act
            monitorRDS.UpdateEntry(rdsName, rdsRealName, cpuBusy, gbSize, freeSpace,
                todayLoadCount, todayLoadSize, cpuBusyAverage, cpuBusyPeak, displaySpace);

            // Assert
            Assert.Pass();
        }

        [Test]
        [Ignore("Only applicable to RA (AWS)")]
        public void Test_CheckDataEntry()
        {
            // Arrange
            string rdsName = "pr13";

            // Act
            bool result = monitorRDS.CheckDataEntry(rdsName);

            // Assert
            Assert.That(result, Is.True);
        }

    }
}
