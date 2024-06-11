using NUnit.Framework;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAS.Service.Tests.Concrete
{
    public class LoadingStatusTests
    {
        string connectionString;
        string instanceID;
        LoadingStatus loadingStatus;

        [SetUp]
        public void Setup()
        {
            instanceID = "i-08ff82f13f2c428bd";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc;UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc;User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            loadingStatus = new LoadingStatus(connectionString);
        }

        [Test]
        public void UpdateLoadingStatusTest()
        {
            loadingStatus.UpdateLoadingStatus(instanceID, 1);
            Assert.Pass();
        }

        [Test]
        public void GetCurrentLoadTest()
        {
            int currentLoad = loadingStatus.GetCurrentLoad(instanceID);
            Assert.That(currentLoad, Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void CheckLoadingTest()
        {
            bool retVal = loadingStatus.CheckLoading(instanceID);
            Assert.That(retVal, Is.True);
        }

        [Test]
        public void CheckCurrentLoads()
        {
            int currentLoad = loadingStatus.CheckCurrentLoads(instanceID);
            Assert.That(currentLoad, Is.GreaterThanOrEqualTo(0));
        }

    }
}
