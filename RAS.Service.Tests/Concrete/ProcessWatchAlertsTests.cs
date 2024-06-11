using log4net;
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
    public class ProcessWatchAlertsTests
    {
        string connectionString;
        string profileConnectionString;
        string systemSerial;
        DateTime startTime;
        DateTime endTime;
        string processTable;
        string volume;
        string subVol;
        string fileName;
        ProcessWatchAlerts processWatchAlerts;
        
        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            //profileConnectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc;UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            profileConnectionString = "Server=13.56.143.245;Database=pmc;User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            //  connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc" + systemSerial + ";UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc" + systemSerial + ";User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            processWatchAlerts = new ProcessWatchAlerts(profileConnectionString, connectionString);
            startTime = Convert.ToDateTime("2024-03-29 00:00:00");
            endTime = Convert.ToDateTime("2024-03-30 00:00:00");
            processTable = "080627_PROCESS_2024_3_29";
            volume = "$WORK5";
            subVol = "AAZWVPR";
            fileName = "LISTAPP";
        }

        [Test]
        public void Test_GetProcessWatch()
        {
            // Act
            DataTable result = processWatchAlerts.GetProcessWatch(systemSerial);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetStartedBy()
        {
            // Act
            DataTable result = processWatchAlerts.GetStartedBy(
                Convert.ToDateTime("2024-03-29 01:00:00"),
                processTable, startTime, endTime, volume, subVol, fileName);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetStoppedBy()
        {
            // Act
            DataTable result = processWatchAlerts.GetStoppedBy(
                Convert.ToDateTime("2024-03-29 01:00:00"),
                processTable, startTime, endTime, volume, subVol, fileName);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetProcessCount()
        {
            // Act
            DataTable result = processWatchAlerts.GetProcessCount(processTable, startTime,
                endTime, volume, subVol, fileName);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetProcessBusy()
        {
            // Act
            DataTable result = processWatchAlerts.GetProcessBusy(processTable, startTime, 
                endTime, volume, subVol, fileName);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetAbortTrans()
        {
            // Act
            DataTable result = processWatchAlerts.GetAbortTrans(processTable, startTime,
                endTime, volume, subVol, fileName);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }
    }
}
