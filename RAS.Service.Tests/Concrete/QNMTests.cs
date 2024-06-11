using log4net;
using NUnit.Framework;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;

namespace RAS.Service.Tests.Concrete
{
    public class QNMTests
    {
        private static readonly ILog Log = LogManager.GetLogger("DBHouseKeeping");
        string systemSerial;
        string connectionString;
        QNM qnm;
        string tableName;
        DateTime startDateTime;
        DateTime stopDateTime;


        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc" + systemSerial + ";UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc" + systemSerial + ";User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            qnm = new QNM(@"C:\RemoteAnalyst\", connectionString);
            tableName = "QNM_About"; 
            startDateTime = Convert.ToDateTime("2024-03-29 00:00:00");
            stopDateTime = Convert.ToDateTime("2024-03-29 01:00:00");
        }

        [Test]
        public void Test_CheckTableExists()
        {
            // Arrange

            // Act
            bool result = qnm.CheckTableExists(tableName, "pmc080627");

            // Assert
            Assert.That(result, Is.True);
        }


        [Test]
        public void Test_GetAllUniquePIFNames()
        {
            // Arrange

            // Act
            List<string> result = qnm.GetAllUniquePIFNames();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<List<string>>());
            Assert.That(result.Count, Is.GreaterThan(0));
        }

        [Test]
        [Ignore("Need to figure out how to implement")]
        public void Test_InsertData()
        {
            // Arrange

            // Act
            bool result = qnm.InsertData("QNM_About", new DataTable(), Log);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        [Ignore ("Need to figure out how to implement")]
        public void Test_InsertAbout()
        {
            // Arrange

            // Act
            qnm.InsertAbout(new DataTable(), "L01");

            // Assert
            Assert.Pass();
        }

        [Test]
        [Ignore("Need to figure out how to implement")]
        public void Test_CheckAboutExists()
        {
            // Arrange

            // Act
            bool result = qnm.CheckAboutExists(new DataTable());

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Test_CheckDetailsExists()
        {
            // Arrange
            string detailsTableName = "qnm_climdetail_2024_3_29";

            // Act
            bool result = qnm.CheckDetailsExists(detailsTableName, 
                Convert.ToDateTime("2024-03-29 01:00:00"),
                Convert.ToDateTime("2024-03-29 02:00:00"));

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Test_CheckIndexExists()
        {
            // Arrange
            string detailsTableName = "qnm_climcpudetail_2024_3_29";

            // Act
            bool result = qnm.CheckIndexExists(detailsTableName);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        [Ignore("Need to figure out how to implement")]
        public void Test_CreateTable()
        {
            // Arrange
            string cmdText = "";

            // Act
            qnm.CreateTable(cmdText);

            // Assert
            Assert.Pass();
        }

        [Test]
        [Ignore("Need to figure out how to implement")]
        public void Test_CreateIndex()
        {
            // Arrange
            string detailsTableName = "qnm_climcpudetail_2024_3_23";
            string indexName = "";
            List<string> indexCols = new List<string>();

            // Act
            qnm.CreateIndex(detailsTableName, indexName, indexCols, Log);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_GetDeleteDates()
        {
            // Arrange

            // Act
            IList<DateTime> result = qnm.GetDeleteDates(
                Convert.ToDateTime("2024-03-30 00:00:00"));

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<IList<DateTime>>());
            Assert.That(result.Count, Is.GreaterThan(0));
        }
    }
}
