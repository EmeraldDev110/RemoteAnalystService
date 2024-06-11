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
    public class MonthlyDiskTests
    {
        string systemSerial;
        string connectionString;
        MonthlyDisk monthlyDisk;
        DateTime date;
        string diskName;

        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc" + systemSerial + ";UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc" + systemSerial + ";User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            monthlyDisk = new MonthlyDisk(connectionString);
            date = Convert.ToDateTime("2024-03-29 00:00:00");
            diskName = "$SYSTEM";
        }

        [Test]
        public void Test_CheckData()
        {
            // Arrange

            // Act
            bool result = monthlyDisk.CheckData(systemSerial, date, diskName);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Test_InsertNewData()
        {
            // Arrange

            // Act
            monthlyDisk.InsertNewData(systemSerial, date, diskName);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_UpdateData()
        {
            // Arrange

            // Act
            monthlyDisk.UpdateData(systemSerial, diskName, date, 0.0, 0.0, 0.0, 0.0, 0.0);

            // Assert
            Assert.Pass();
        }


        [Test]
        public void Test_DeleteData()
        {
            // Arrange
            DateTime oldDate = Convert.ToDateTime("2023-05-30 00:00:00");
            // Act
            monthlyDisk.DeleteData(oldDate);

            // Assert
            Assert.Pass();
        }
    }
}
