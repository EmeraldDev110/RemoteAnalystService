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
    public class StorageAnalysisTests
    {
        string connectionString;
        string systemSerial;
        StorageAnalysis storageAnalysis;

        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc;UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc;User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            storageAnalysis = new StorageAnalysis(connectionString);
        }

        [Test]
        [Ignore("Applicable only for AWS (RA)")]
        public void Test_GetTrendSize()
        {
            // Arrange

            // Act
            DataTable result = storageAnalysis.GetTrendSize();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        [Ignore("Applicable only for AWS (RA)")]
        public void Test_GetDBSize()
        {
            // Arrange

            // Act
            DataTable result = storageAnalysis.GetDBSize();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        [Ignore("Applicable only for AWS (RA)")]
        public void Test_Insert()
        {
            // Arrange

            // Act
            storageAnalysis.Insert(systemSerial, 0, 0, (float)0);

            // Assert
            Assert.Pass();
        }

        [Test]
        [Ignore ("Applicable only for AWS (RA)")]
        public void Test_GetAllRdsRealName()
        {
            // Arrange

            // Act
            DataTable result = storageAnalysis.GetAllRdsRealName();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        [Ignore("Applicable only for AWS (RA)")]
        public void Test_GetTop10StorageUsageBy()
        {
            // Arrange

            // Act
            DataTable result = storageAnalysis.GetTop10StorageUsageBy("2024-05-28");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }
        
        [Test]
        [Ignore("Applicable only for AWS (RA)")]
        public void Test_GetStoragesBy()
        {
            // Arrange

            // Act
            DataTable result = storageAnalysis.GetStoragesBy("\\RADVNS1", "2024-05-28", "2024-05-29");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        [Ignore("Applicable only for AWS (RA)")]
        public void Test_GetSystemNamesInTopForPeriod()
        {
            // Arrange

            // Act
            DataTable result = storageAnalysis.GetSystemNamesInTopForPeriod(10, "2024-05-28", "2024-05-29");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }
    }
}
