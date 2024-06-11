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
    public class MonthlyAppUnratedTests
    {
        string systemSerial;
        string connectionString;
        MonthlyAppUnrated monthlyAppUnrated;
        DateTime date;
        int attributeId;
        string obj;

        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc" + systemSerial + ";UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc" + systemSerial + ";User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            monthlyAppUnrated = new MonthlyAppUnrated(connectionString);
            date = Convert.ToDateTime("2021-02-01 00:00:00");
            attributeId = 2;
            obj = "0";
        }

        [Test]
        public void Test_CheckData()
        {
            // Arrange

            // Act
            bool result = monthlyAppUnrated.CheckData(systemSerial, date, attributeId, obj);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Test_InsertNewData()
        {
            // Arrange

            // Act
            monthlyAppUnrated.InsertNewData(systemSerial, date, attributeId, obj);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_GetHourlyData()
        {
            // Arrange

            // Act
            DataTable result = monthlyAppUnrated.GetHourlyData(systemSerial, attributeId, obj, date);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        [Ignore("Need to figure out how to do this")]
        public void Test_UpdateData_UsingDataTable()
        {
            // Arrange

            // Act
            monthlyAppUnrated.UpdateData(new DataTable());

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_UpdateData()
        {
            // Arrange

            // Act
            monthlyAppUnrated.UpdateData(0.0, 0.0, 1, 24, systemSerial, attributeId, obj, date, "Hour0=10");

            // Assert
            Assert.Pass();
        }
    }
}
