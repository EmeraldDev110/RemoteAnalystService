using NUnit.Framework;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAS.Service.Tests.Concrete
{
    public class UploadsTests
    {
        int uploadID;
        string connectionString;
        Uploads uploads;

        [SetUp]
        public void Setup()
        {
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc;UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc;User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            uploadID = 1;
            uploads = new Uploads(connectionString);
        }

        [Test]
        public void Test_GetCustomerID()
        {
            int result = uploads.GetCustomerID(uploadID);
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void Test_UpdateLoadedDate()
        {
            uploads.UpdateLoadedDate(uploadID);
            Assert.Pass();
        }

        [Test]
        public void Test_UpdateLoadedStatus()
        {
            uploads.UpdateLoadedStatus(uploadID, "Test_Status");
            Assert.Pass();
        }

        [Test]
        public void Test_UploadCollectionStartTime()
        {
            uploads.UploadCollectionStartTime(uploadID, DateTime.Today);
            Assert.Pass();
        }

        [Test]
        public void Test_UploadCollectionToTime()
        {
            uploads.UploadCollectionToTime(uploadID, DateTime.Now);
            Assert.Pass();
        }
    }
}
