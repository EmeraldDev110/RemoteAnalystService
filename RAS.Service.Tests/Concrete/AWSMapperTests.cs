using NUnit.Framework;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using RemoteAnalyst.Repository.Models;
using RemoteAnalyst.Repository.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAS.Service.Tests.Concrete
{
    public class AWSMapperTests
    {
        AwsMapperRepository awsMapper;

        [SetUp]
        public void Setup()
        {
            awsMapper = new AwsMapperRepository();
        }

        [Test]
        [Ignore("Only applicable to RA (AWS)")]
        public void Test_AWSMapper_GetLoaderInfo()
        {
            // Arrange
            string ec2Name = "pr13";

            // Act
            AwsMapper result = awsMapper.GetLoaderInfo(ec2Name);

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        [Ignore("Only applicable to RA (AWS)")]
        public void Test_AWSMapper_GetMaxLoaderSequenceNum()
        {
            // Arrange

            // Act
            int result = awsMapper.GetMaxLoaderSequenceNum();

            // Assert
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void Test_AWSMapper_InsertNewLoader()
        {
            // Arrange
            string ec2Name = "pr13";
            int sequenceNum = 1;

            // Act
            awsMapper.InsertNewLoader(ec2Name, sequenceNum);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void Test_AWSMapper_DeleteLoader()
        {
            // Arrange
            string ec2Name = "pr13";

            // Act
            awsMapper.DeleteLoader(ec2Name);

            // Assert
            Assert.Pass();
        }
    }
}
