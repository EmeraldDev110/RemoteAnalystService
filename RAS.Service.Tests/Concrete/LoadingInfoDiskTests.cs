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
    public class LoadingInfoDiskTests
    {
        string connectionString;
        string systemSerial;
        LoadingInfoDisk loadingInfoDisk;

        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc;UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc;User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            loadingInfoDisk = new LoadingInfoDisk(connectionString);
        }

        [Test]
        public void Test_GetUWSFileName()
        {

            // Arrange

            // Act
            DataTable result = loadingInfoDisk.GetUWSFileName(systemSerial);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_DeleteLoadingInfoDisk()
        {
            // Arrange

            // Act
            loadingInfoDisk.DeleteLoadingInfoDisk(23);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_UpdateFailedToLoadDisk()
        {
            // Arrange

            // Act
            loadingInfoDisk.UpdateFailedToLoadDisk("u0668574.402");

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_UpdateLoadingInfoDisk()
        {
            // Arrange

            // Act
            loadingInfoDisk.UpdateLoadingInfoDisk("u0668574.402");

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_Insert()
        {
            // Arrange

            // Act
            loadingInfoDisk.Insert(systemSerial, 1023, "Test_File", 0);

            // Assert
            Assert.Pass();
        }
    }
}
