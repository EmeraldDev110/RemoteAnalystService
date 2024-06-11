using NUnit.Framework;
using RemoteAnalyst.Repository.MySQLConcrete.RemoteAnalystdb;
using System.Collections.Generic;
using System.Data;

namespace RAS.Service.Tests
{
    public class MiscellaneousTests
    {
        string connectionString;
        string systemSerial;

        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc" + systemSerial + ";UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc" + systemSerial + ";User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
        }

        [Test]
        [Ignore("Since Transactions functionality has been removed")]
        public void Test_TransactionProfileEmail_GetTransactionProfileEmail()
        {
            // Arrange
            TransactionProfileEmail tpe = new TransactionProfileEmail(connectionString);

            // Act
            List<string> result = tpe.GetTransactionProfileEmail(357);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<List<string>>());
            Assert.That(result.Count, Is.GreaterThan(0));
        }

        [Test]
        [Ignore ("Since Transactions functionality has been removed")]
        public void Test_TransactionProfiles_GetTransactionProfileInfo()
        {
            // Arrange
            TransactionProfiles tp = new TransactionProfiles(connectionString);

            // Act
            DataTable result = tp.GetTransactionProfileInfo(systemSerial);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        [Ignore("Since Transactions functionality has been removed")]
        public void Test_TransactionProfiles_GetTransactionProfileInfo_Specifc()
        {
            // Arrange
            TransactionProfiles tp = new TransactionProfiles(connectionString);
            int profileId = 357;

            // Act
            DataTable result = tp.GetTransactionProfileInfo(systemSerial, profileId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        [Ignore("Since Transactions functionality has been removed")]
        public void Test_TransactionProfiles_GetTransactionProfileName()
        {
            // Arrange
            TransactionProfiles tp = new TransactionProfiles(connectionString);
            int profileId = 357;

            // Act
            string result = tp.GetTransactionProfileName(profileId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.GreaterThan(0));
        }

        [Test]
        public void Test_CurrentTables_GetFileTableList()
        {
            // Arrange
            RemoteAnalyst.Repository.MySQLConcrete.RemoteAnalystSPAM.CurrentTables ct = new RemoteAnalyst.Repository.MySQLConcrete.RemoteAnalystSPAM.CurrentTables(connectionString);

            // Act
            Dictionary<string, long> result = ct.GetFileTableList();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Dictionary<string, long>>());
            Assert.That(result.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_CurrentTables_GetProcessTableList()
        {
            // Arrange
            RemoteAnalyst.Repository.MySQLConcrete.RemoteAnalystSPAM.CurrentTables ct = new RemoteAnalyst.Repository.MySQLConcrete.RemoteAnalystSPAM.CurrentTables(connectionString);

            // Act
            Dictionary<string, long> result = ct.GetProcessTableList();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Dictionary<string, long>>());
            Assert.That(result.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_TableTimestamps_GetStartEndTime()
        {
            // Arrange
            RemoteAnalyst.Repository.MySQLConcrete.RemoteAnalystSPAM.TableTimestamps tt = new RemoteAnalyst.Repository.MySQLConcrete.RemoteAnalystSPAM.TableTimestamps(connectionString);

            // Act
            DataTable result = tt.GetStartEndTime("080627_FILE_2024_3_29");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

    }
}