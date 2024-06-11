using NUnit.Framework;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;
using RemoteAnalyst.Repository.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;

namespace RAS.Service.Tests.Concrete
{
    public class MiscellaneousRASpamTests
    {
        string systemSerial;
        string connectionString;
        DateTime startDateTime;
        DateTime stopDateTime;

        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc" + systemSerial + ";UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc" + systemSerial + ";User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            startDateTime = Convert.ToDateTime("2024-03-29 00:00:00");
            stopDateTime = Convert.ToDateTime("2024-03-29 01:00:00");
        }

        [Test]
        public void Test_ChartEntities_GetChartWithFileEntity()
        {
            // Arrange
            ChartEntities chartEntities = new ChartEntities(connectionString);

            // Act
            IList<int> result = chartEntities.GetChartWithFileEntity();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<IList<int>>());
            Assert.That(result.Count, Is.GreaterThan(0));
        }


        [Test]
        [Ignore("Not used anymore")]
        public void Test_ChartGroupDetail_GetChartWithFileEntity()
        {
            // Arrange
            ChartGroupDetail chartGroupDetails = new ChartGroupDetail(connectionString);

            // Act
            IList<int> result = chartGroupDetails.GetChartIDs(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<IList<int>>());
            Assert.That(result.Count, Is.GreaterThan(0));
        }


        [Test]
        [Ignore("Not used anymore")]
        public void Test_DetailDiskForForecast_GetQueueLength()
        {
            // Arrange
            DetailDiskForForecast detailDiskForForecast = new DetailDiskForForecast(connectionString);

            // Act
            DataTable result = detailDiskForForecast.GetQueueLength(Convert.ToDateTime("2021-04-04 00:00:00"), Convert.ToDateTime("2021-04-05 00:00:00"));

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        [Ignore("Not used anymore")]
        public void Test_DetailProcessForForecast_GetProcessData()
        {
            // Arrange
            DetailProcessForForecast detailProcessForForecast = new DetailProcessForForecast(connectionString);

            // Act
            DataTable result = detailProcessForForecast.GetProcessData(startDateTime, stopDateTime);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        [Ignore("Not used anymore")]
        public void Test_DetailTmfForForecast_GetTmfData()
        {
            // Arrange
            DetailTmfForForecast detailTmfForForecast = new DetailTmfForForecast(connectionString);

            // Act
            DataTable result = detailTmfForForecast.GetTmfData(startDateTime, stopDateTime);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_DiskInfo_GetAveragedUsedGB()
        {
            // Arrange
            DiskInfo diskInfo = new DiskInfo(connectionString);

            // Act
            double result = diskInfo.GetAveragedUsedGB(systemSerial, "$SYSTEM");

            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void Test_Entities_GetEntityID()
        {
            // Arrange
            EntityRepository entities = new EntityRepository(connectionString);

            // Act
            int result = entities.GetEntityID("CPU");

            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void Test_Exceptions_GetException()
        {
            // Arrange
            Exceptions exceptions = new Exceptions(connectionString);

            // Act
            DataTable result = exceptions.GetException(Convert.ToDateTime("2021-04-13 12:50:00"), Convert.ToDateTime("2021-04-13 13:00:00"), "CPU", "Queue");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_MeasureVersions_GetEntityID()
        {
            // Arrange
            MeasureVersions measureVersions = new MeasureVersions(connectionString);

            // Act
            string result = measureVersions.GetMeasureDBTableName("CPU");

            // Assert
            Assert.That(result, !Is.Null);
            Assert.That(result.Length, Is.GreaterThan(0));
        }

        [Test]
        public void Test_Pathway_GetListOfPathwayTables()
        {
            // Arrange
            Pathway pathway = new Pathway(connectionString);

            // Act
            List<string> result = pathway.GetListOfPathwayTables();

            // Assert
            Assert.That(result, !Is.Null);
            Assert.That(result.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_Pathway_DeleteData()
        {
            // Arrange
            Pathway pathway = new Pathway(connectionString);
            DateTime oldDate = Convert.ToDateTime("2023-05-30 00:00:00");

            // Act
            pathway.DeleteData(oldDate, "pvcpumany");

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_PvCollects_GetInterval()
        {
            // Arrange
            PvCollects exceptions = new PvCollects(connectionString);

            // Act
            DataTable result = exceptions.GetInterval(Convert.ToDateTime("2024-05-04 00:00:00"), Convert.ToDateTime("2024-05-05 00:00:00"));

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        [Ignore("Not used anymore")]
        public void Test_RecepientList_GetReportWithFileEntity()
        {
            // Arrange
            RecepientList recepientList = new RecepientList(connectionString);

            // Act
            IList<string> result = recepientList.GetEmailList(1);

            // Assert
            Assert.That(result, !Is.Null);
            Assert.That(result.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_ReportEntities_GetReportWithFileEntity()
        {
            // Arrange
            ReportEntities reportEntities = new ReportEntities(connectionString);

            // Act
            IList<int> result = reportEntities.GetReportWithFileEntity();

            // Assert
            Assert.That(result, !Is.Null);
            Assert.That(result.Count, Is.GreaterThan(0));
        }

        [Test]
        [Ignore("Not used anymore")]
        public void Test_ReportGroupDetail_GetReportIDs()
        {
            // Arrange
            ReportGroupDetail rgd = new ReportGroupDetail(connectionString);

            // Act
            IList<int> result = rgd.GetReportIDs(1);

            // Assert
            Assert.That(result, !Is.Null);
            Assert.That(result.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_OSSJRNL_CreateOSSTableAndIndex()
        {
            // Arrange
            OSSJRNL ossjrnl = new OSSJRNL(connectionString);
            string tableName = "080627_OSSJRNL_2024_3_23";

            // Act
            ossjrnl.CreateOSSTable(tableName);
            ossjrnl.CreateOSSIndex(tableName);

            //Assert
            Assert.Pass();
        }

        [Test]
        public void Test_OSSJRNL_CheckDuplicate()
        {
            // Arrange
            OSSJRNL ossjrnl = new OSSJRNL(connectionString);
            string tableName = "080627_OSSJRNL_2024_3_23";

            // Act
            bool result = ossjrnl.CheckDuplicate(tableName);

            //Assert
            Assert.That(result, !Is.True);
        }

        [Test]
        [Ignore("Not used anymore")]
        public void Test_StorageReport_CheckCapacities()
        {
            // Arrange
            StorageReport storageReport = new StorageReport(connectionString);

            // Act
            bool result = storageReport.CheckCapacities(1);

            //Assert
            Assert.That(result, !Is.True);
        }

        [Test]
        [Ignore("Not used anymore")]
        public void Test_StorageReport_GetSchduleData()
        {
            // Arrange
            StorageReport storageReport = new StorageReport(connectionString);

            // Act
            DataTable result = storageReport.GetSchduleData(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_UWSArchive_InsertArchiveID()
        {
            // Arrange
            UWSArchive uwsArchive = new UWSArchive(connectionString);

            // Act
            uwsArchive.InsertArchiveID(startDateTime, stopDateTime, "archiveID", DateTime.Now, 0);

            // Assert
            Assert.Pass();
        }

    }
}
