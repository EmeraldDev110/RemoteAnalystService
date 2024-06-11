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
    public class TriggersTests
    {
        string connectionString;
        Triggers triggers;
        string systemSerial;

        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc;UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc;User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            triggers = new Triggers(connectionString);
        }

        [Test]
        public void Test_DeleteTriiger()
        {
            // Arrange

            // Act
            triggers.DeleteTriiger(1);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_GetTrigger()
        {
            // Arrange

            // Act
            DataTable result = triggers.GetTrigger(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_Insert()
        {
            // Arrange

            // Act
            triggers.Insert(systemSerial, 1, "Test_Message");

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_Insert_Details()
        {
            // Arrange

            // Act
            triggers.Insert(systemSerial, 1, "1", "Test_File_Location", 0, 1);

            // Assert
            Assert.Pass();
        }
    }
}
