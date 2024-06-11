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
    public class UWSLoadInfosTests
    {
        string connectionString;
        string systemSerial;
        UWSLoadInfos uwsLoadInfos;

        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc;UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc;User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;"; ;
            uwsLoadInfos = new UWSLoadInfos(connectionString);
        }

        [Test]
        public void Test_InsertData()
        {
            // Arrange

            // Act
            uwsLoadInfos.InsertData(systemSerial, DateTime.Today, DateTime.Now, DateTime.Today, DateTime.Now);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_GetLoadedTime()
        {
            // Arrange

            // Act
            DataTable result = uwsLoadInfos.GetLoadedTime(systemSerial, DateTime.Today, DateTime.Now);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
        }


        [Test]
        public void Test_CheckLoadedTime()
        {
            // Arrange

            // Act
            bool result = uwsLoadInfos.CheckLoadedTime(systemSerial, 
                Convert.ToDateTime("2024-05-29 00:00:00"), Convert.ToDateTime("2024-05-29 01:00:00"));

            // Assert
            Assert.That(result, Is.True);
        }
    }
}
