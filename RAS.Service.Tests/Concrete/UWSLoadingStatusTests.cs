using NUnit.Framework;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAS.Service.Tests.Concrete
{
    public class UWSLoadingStatusTests
    {
        string systemSerial;
        string connectionString;
        UWSLoadingStatus uwsLoadingStatus;

        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc" + systemSerial + ";UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc" + systemSerial + ";User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
        }

        [Test]
        public void Test_InsertUWSLoadingStatus()
        {
            // Arrange
            uwsLoadingStatus = new UWSLoadingStatus(connectionString);

            // Act
            uwsLoadingStatus.InsertUWSLoadingStatus(systemSerial, "Test_File");

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_CheckUWSLoadingStatus()
        {
            // Arrange
            uwsLoadingStatus = new UWSLoadingStatus(connectionString);

            // Act
            bool result = uwsLoadingStatus.CheckUWSLoadingStatus(systemSerial, "Test_File");

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Test_DeleteUWSLoadingStatus()
        {
            // Arrange
            uwsLoadingStatus = new UWSLoadingStatus(connectionString);

            // Act
            uwsLoadingStatus.DeleteUWSLoadingStatus(systemSerial, "Test_File");

            // Assert
            Assert.Pass();
        }
    }
}
