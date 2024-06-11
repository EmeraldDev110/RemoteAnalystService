using NUnit.Framework;
using RemoteAnalyst.Repository.MySQLConcrete.RemoteAnalystSPAM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAS.Service.Tests
{
    public class FileEntitiyTests
    {
        string connectionString;
        string systemSerial;
        string tableName;
        string fileTableName;
        FileEntityRepository fileEntity;
        DateTime fromDateTime;
        DateTime toDateTime;
        int fileInterval;
        int transactionRatio;

        [SetUp]
        public void Setup()
        {   
            string volume = "$DSMSCM";
            string subVol = "MQ";
            string fileName = "AMQSINI";
            fileInterval = 900;
            short transactionsCounter = 1;
            transactionRatio = 1;

            fromDateTime = Convert.ToDateTime("2024-03-29 00:00:00");
            toDateTime = Convert.ToDateTime("2024-03-29 01:00:00");
            systemSerial = "080627";
            tableName = "080627_CPU_2024_3_29";
            fileTableName = "080627_FILE_2024_3_29";
            // connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc" + systemSerial + ";UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            connectionString = "Server=13.56.143.245;Database=pmc" + systemSerial + ";User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";

            fileEntity = new FileEntityRepository(connectionString, 
                                            transactionsCounter,
                                            volume, subVol, fileName);
        }

        [Test]
        public void Test_FileEntity_GetAnyTPS()
        {
            // Arrange

            // Act
            double result = fileEntity.GetAnyTPS(fileTableName, 
                fromDateTime, toDateTime, transactionRatio, fileInterval);

            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void Test_FileEntity_GetOpenerProgramTPS()
        {
            // Arrange
            string openerVolume = "$OSS";
            string openerSubVol = "ZYQ00000";
            string openerFileName = "Z0004N5S";
               
            // Act
            double result = fileEntity.GetOpenerProgramTPS(fileTableName,
                           fromDateTime, toDateTime, transactionRatio,
                           openerVolume, openerSubVol, openerFileName,
                           fileInterval);

            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }
        [Test]
        public void Test_FileEntity_GetOpenerProcessTPS()
        {
            // Arrange
            string process = "$X2M9";

            // Act
            double result = fileEntity.GetOpenerProcessTPS(fileTableName,
                           fromDateTime, toDateTime, transactionRatio,
                           process, fileInterval);

            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }
    }
}

