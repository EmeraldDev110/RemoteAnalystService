using NUnit.Framework;
using RemoteAnalyst.Repository.Concrete.Model;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;
using RemoteAnalyst.Repository.MySQLConcrete.RemoteAnalystSPAM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAS.Service.Tests.Concrete
{
    public class TrendCPUNormalizedTests
    {
        string systemSerial;
        string connectionString;
        string tableName;
        TrendCPUNormalized trendCPUNormalized;
        DateTime startDateTime;
        DateTime stopDateTime;

        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc" + systemSerial + ";UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc" + systemSerial + ";User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            trendCPUNormalized = new TrendCPUNormalized();
            tableName = "080627_CPU_2024_3_23";
            startDateTime = Convert.ToDateTime("2024-03-23 00:00:00");
            stopDateTime = Convert.ToDateTime("2024-03-23 01:00:00");
        }

        [Test]
        [Ignore("Need to figure out how to implement")]
        public void Test_CreateTrendCPUNormalizedTable()
        {
            // Arrange

            // Act
            trendCPUNormalized.CreateTrendCPUNormalizedTable(connectionString, tableName);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_CheckDuplicateDataFromNorTable()
        {
            // Arrange

            // Act
            bool result = trendCPUNormalized.CheckDuplicateDataFromNorTable(
                "080627_CPU_2024_3_30", 
                Convert.ToDateTime("2024-03-30 00:00:00"),
                Convert.ToDateTime("2024-03-30 01:00:00"), connectionString);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Test_GetCPUBaseData()
        {
            // Arrange

            // Act
            DataTable result = trendCPUNormalized.GetCPUBaseData(
                "080627_CPU_2024_3_30",
                Convert.ToDateTime("2024-03-30 00:00:00"),
                Convert.ToDateTime("2024-03-30 01:00:00"),
                Convert.ToDateTime("2024-03-30 00:00:00"),
                Convert.ToDateTime("2024-03-30 01:00:00"), connectionString);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }
    }
}
