using NUnit.Framework;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAS.Service.Tests.Concrete
{
    public class UploadFileNamesTests
    {
        string connectionString;
        string systemSerial;
        UploadFileNames uploadFileNames;

        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc;UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc;User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            uploadFileNames = new UploadFileNames(connectionString);
        }

        [Test]
        public void Test_GetOrderId()
        {
            // Arrange

            // Act
            int result = uploadFileNames.GetOrderId("TestFile");

            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void Test_CheckLoaded()
        {
            // Arrange

            // Act
            Dictionary<string, bool> result = uploadFileNames.CheckLoaded(1);

            // Assert
            Assert.That(result, !Is.Null);
        }

        [Test]
        public void Test_UpdateLoadStatus()
        {
            // Arrange

            // Act
            uploadFileNames.UpdateLoadStatus("Test_File");

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_DeleteEntries()
        {
            // Arrange

            // Act
            uploadFileNames.DeleteEntries(1);

            // Assert
            Assert.Pass();
        }
    }
}
