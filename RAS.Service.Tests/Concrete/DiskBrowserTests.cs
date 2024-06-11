using NUnit.Framework;
using RemoteAnalyst.Repository.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAS.Service.Tests.Concrete
{
    public class DiskBrowserTests
    {
        string systemSerial;
        string connectionString;
        List<string> diskBrowserables;
        DiskBrowserRepository diskBrowser;
        DateTime startDateTime;
        DateTime stopDateTime;

        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc" + systemSerial + ";UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc" + systemSerial + ";User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            diskBrowserables = new List<string>() { "080627_DISKBROWSER_2024_3_29" };
            startDateTime = Convert.ToDateTime("2024-03-29 00:00:00");
            stopDateTime = Convert.ToDateTime("2024-03-29 01:00:00");
            diskBrowser = new DiskBrowserRepository(connectionString);
        }

        [Test]
        public void Test_DISCEntityTable_GetDISCEntityTableIntervalList()
        {
            // Arrange

            // Act
            DataTable result = diskBrowser.GetTop20Disks(diskBrowserables, startDateTime, stopDateTime);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_DISCEntityTable_GetQueueLength()
        {
            // Arrange

            // Act
            DataTable result = diskBrowser.GetQueueLength(diskBrowserables, startDateTime, stopDateTime);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_DISCEntityTable_GetDP2Busy()
        {
            // Arrange

            // Act
            DataTable result = diskBrowser.GetDP2Busy(diskBrowserables, startDateTime, stopDateTime);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }
    }
}
