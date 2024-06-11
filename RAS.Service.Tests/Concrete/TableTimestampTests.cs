using NUnit.Framework;
using RemoteAnalyst.Repository.Concrete.Model;
using RemoteAnalyst.Repository.Repositories;
using System;
using System.Collections.Generic;
using System.Data;

namespace RAS.Service.Tests.Concrete
{
    public class TableTimeStampTests
    {
        string systemSerial;
        string connectionString;
        string tableName;
        string deletedTableName;
        TableTimestampRepository tableTimestamp;
        DateTime startDateTime;
        DateTime stopDateTime;

        [SetUp]
        public void Setup()
        {
            systemSerial = "080627";
            //connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc" + systemSerial + ";UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc" + systemSerial + ";User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            tableTimestamp = new TableTimestampRepository(connectionString);
            tableName = "080627_CPU_2024_3_23";
            deletedTableName = "080627_CPU_2024_3_22";
            startDateTime = Convert.ToDateTime("2024-03-23 00:00:00");
            stopDateTime = Convert.ToDateTime("2024-03-23 01:00:00");
        }

        [Test]
        public void Test_DeleteEntry()
        {
            // Arrange

            // Act
            tableTimestamp.DeleteEntry(deletedTableName);

            // Assert
            Assert.Pass();
        }

        [Test]
        [Ignore ("Not sure how to implement)")]
        public void Test_DeleteEntry_Parameters()
        {
            // Arrange
            List<TableTimestampQueryParameter> parameters = new List<TableTimestampQueryParameter>();

            // Act
            tableTimestamp.DeleteEntry(parameters);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_InsetEntryFor()
        {
            // Arrange

            // Act
            tableTimestamp.InsetEntryFor(deletedTableName, startDateTime, stopDateTime, 0, "");

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_CheckTimeOverLap()
        {
            // Arrange

            // Act
            bool result = tableTimestamp.CheckTimeOverLap(tableName, startDateTime, stopDateTime);

            // Assert
            Assert.That(result, !Is.True);
        }

        [Test]
        public void Test_CheckTempTimeOverLap()
        {
            // Arrange

            // Act
            bool result = tableTimestamp.CheckTempTimeOverLap(tableName, startDateTime, stopDateTime);

            // Assert
            Assert.That(result, !Is.True);
        }

        [Test]
        public void Test_CheckDuplicate()
        {
            // Arrange

            // Act
            bool result = tableTimestamp.CheckDuplicate(tableName, startDateTime, stopDateTime);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Test_GetTimestampsFor()
        {
            // Arrange

            // Act
            DataTable result = tableTimestamp.GetTimestampsFor(tableName, startDateTime, stopDateTime);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_UpdateStatusUsingTableName()
        {
            // Arrange

            // Act
            tableTimestamp.UpdateStatusUsingTableName(tableName, startDateTime, stopDateTime, 0);

            // Assert
            Assert.Pass();
        }

        [Test]
        [Ignore("Not sure how to implement)")]
        public void Test_UpdateStatusUsingTableName_Parameters()
        {
            // Arrange
            List<TableTimestampQueryParameter> parameters = new List<TableTimestampQueryParameter>();

            // Act
            tableTimestamp.DeleteEntry(parameters);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_GetArchiveDetailsPerTable()
        {
            // Arrange

            // Act
            DataTable result = tableTimestamp.GetArchiveDetailsPerTable(tableName);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetGetLoadedData()
        {
            // Arrange

            // Act
            DataTable result = tableTimestamp.GetGetLoadedData(Convert.ToDateTime("2024-03-30 00:00:00"),
                Convert.ToDateTime("2024-03-31 00:00:00"));

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetLoadedFileData()
        {
            // Arrange
            DateTime loadedStart = Convert.ToDateTime("2024-03-30 00:00:00");
            DateTime loadedEnd = Convert.ToDateTime("2024-03-31 00:00:00");

            // Act
            DataTable result = tableTimestamp.GetLoadedFileData(loadedStart, loadedEnd);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
        }

        [Test]
        [Ignore("Need to figure out how to implement")]
        public void Test_AddFileNameColumnToTableTimeStampTable()
        {
            // Arrange

            // Act
            tableTimestamp.AddFileNameColumnToTableTimestampTable();

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_CheckMySqlColumn()
        {
            // Arrange

            // Act
            bool result = tableTimestamp.CheckMySqlColumn("pmc080627", "TableTimestamp", "FileName");

            // Assert
            Assert.That(result, Is.True);
        }


    }
}
