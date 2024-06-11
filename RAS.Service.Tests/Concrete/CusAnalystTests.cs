using NUnit.Framework;
using RemoteAnalyst.Repository.Repositories;
using System.Collections.Generic;
using System.Data;

namespace RAS.Service.Tests.Concrete
{
    public class CusAnalystTests
    {
        CusAnalystRepository cusAnalyst;
        int customerId;

        [SetUp]
        public void Setup()
        {
            var systemSerial = "080627";
            //var connectionString = "SERVER=10.26.97.160;PORT=3306;DATABASE=pmc" + systemSerial + ";UID=localanalyst;PASSWORD=pit.Mud-1972;Allow User Variables=true";
            var connectionString = "Server=13.56.143.245;Database=pmc" + systemSerial + ";User Id=localanalyst;Password=UpWork24;Trusted_Connection=False;Integrated Security=False;Encrypt=False;persist security info=True;";
            cusAnalyst = new CusAnalystRepository();
            customerId = 1037;
        }

        [Test]
        public void Test_GetCustomers()
        {
            // Arrange
            int companyID = 265;

            // Act
            IList<int> result = cusAnalyst.GetCustomers(companyID);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.GreaterThan(0));
        }   

        [Test]
        public void Test_GetCustomerEmail()
        {
            // Arrange
            int customerId = 1037;

            // Act
            DataTable result = cusAnalyst.GetCustomerEmail(customerId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<DataTable>());
            Assert.That(result.Rows.Count, Is.GreaterThan(0));
            // Assert.Pass();
        }

        [Test]
        public void Test_GetEmailAddress()
        {
            // Arrange

            // Act
            string result = cusAnalyst.GetEmailAddress(customerId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.GreaterThan(0));
        }

        [Test]
        public void Test_GetCustomerID()
        {
            // Arrange
            string customerEmail = "vishalkudchadkar@idelji.com";

            // Act
            int result = cusAnalyst.GetCustomerID(customerEmail);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.GreaterThan(0));
        }

        [Test]
        public void GetCompanyID()
        {
            // Arrange
            int customerId = 1037;

            // Act
            int result = cusAnalyst.GetCompanyID(customerId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.GreaterThan(0));
            // Assert.Pass();
        }

        [Test]
        public void GetLoginName()
        {
            // Arrange
            int customerId = 1037;

            // Act
            string result = cusAnalyst.GetLoginName(customerId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.GreaterThan(0));
        }


        [Test]
        public void GetAdminEmail()
        {
            // Arrange
            string systemSerial = "080627";

            // Act
            string result = cusAnalyst.GetAdminEmail(systemSerial);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.GreaterThan(0));
        }


        [Test]
        public void Test_GetUserName()
        {
            // Arrange
            string email = "pauluszemaitis@idelji.com";

            // Act
            string result = cusAnalyst.GetUserName(email);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.GreaterThan(0));
        }
    }
}