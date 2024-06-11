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
    public class ReportDownloadsTests
    {
        string connectionString;
        string systemSerial;
        ReportDownloads reportDownloads;
        int reportDownloadId;

        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc;UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc;User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            reportDownloads = new ReportDownloads(connectionString);
            reportDownloadId = 15028;
        }

        [Test]
        public void Test_InsertNewReport()
        {
            // Act
            int result = reportDownloads.InsertNewReport(systemSerial, DateTime.Today, DateTime.Now, 1, 1032);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.GreaterThan(0));
        }


        [Test]
        public void Test_UpdateFileLocation()
        {
            // Act
            reportDownloads.UpdateFileLocation(reportDownloadId, "Test_File");

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_UpdateStatus()
        {
            // Act
            reportDownloads.UpdateStatus(reportDownloadId, 3);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_GetProcessingIds()
        {
            // Act
            List<int> result = reportDownloads.GetProcessingIds();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetReportDownloads()
        {
            // Act
            DataTable result = reportDownloads.GetReportDetail(reportDownloadId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetReportRequestDate()
        {
            // Act
            DateTime result = reportDownloads.GetReportRequestDate(reportDownloadId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DateTime>());
            Assert.That(result, !Is.EqualTo(DateTime.MinValue));
        }

    }
}
