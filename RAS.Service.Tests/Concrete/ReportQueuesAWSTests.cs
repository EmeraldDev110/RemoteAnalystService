using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using System.Data;
using System.Net;
using System.Security.Cryptography;

namespace RAS.Service.Tests.Concrete
{
    public class ReportQueuesAWSTests
    {
        string connectionString;
        string instanceID;
        ReportQueuesAWS reportQueuesAWS;
        int typeID;
        int queueID;

        [SetUp]
        public void Setup()
        {
            typeID = 1;
            queueID = 1;
            instanceID = "i-08ff82f13f2c428bd";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc;UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc;User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            reportQueuesAWS = new ReportQueuesAWS(connectionString);
        }

        [Test]
        public void Test_GetCurrentQueues()
        {

            // Act
            DataTable result = reportQueuesAWS.GetCurrentQueues(typeID, instanceID);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_UpdateOrders()
        {

            // Act
            reportQueuesAWS.UpdateOrders(queueID);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_GetProcessingOrder()
        {

            // Act
            int result = reportQueuesAWS.GetProcessingOrder(typeID, instanceID);

            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void Test_InsertNewQueue()
        {

            // Act
            reportQueuesAWS.InsertNewQueue("Test_Message", typeID, instanceID);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_CheckOtherQueues()
        {

            // Act
            bool result = reportQueuesAWS.CheckOtherQueues(instanceID);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Test_GetCurrentCount()
        {

            // Act
            int result = reportQueuesAWS.GetCurrentCount(typeID, instanceID);

            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void Test_RemoveQueue()
        {

            // Act
            reportQueuesAWS.RemoveQueue(queueID);

            // Assert
            Assert.Pass();
        }
    }
}
