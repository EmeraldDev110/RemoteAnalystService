using System.Collections.Generic;
using System.Data;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices
{
    public class CusAnalystService
    {
        private readonly string _connectionString = "";

        public CusAnalystService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IList<int> GetCustomersFor(int companyID)
        {
            var cusAnalyst = new CusAnalystRepository();

            IList<int> customerIDs = new List<int>();
            customerIDs = cusAnalyst.GetCustomers(companyID);

            return customerIDs;
        }

        public CusAnalystView GetCustomerEmailFor(int customerID)
        {
            var cusAnalyst = new CusAnalystRepository();
            var view = new CusAnalystView();

            DataTable customerEmail = cusAnalyst.GetCustomerEmail(customerID);

            foreach (DataRow dr in customerEmail.Rows)
            {
                view.FisrtName = dr["fname"].ToString();
                view.LastName = dr["lname"].ToString();
                view.Email = dr["email"].ToString();
            }

            return view;
        }

        public string GetEmailAddressFor(int customerID)
        {
            var cusAnalyst = new CusAnalystRepository();
            string Email = cusAnalyst.GetEmailAddress(customerID);

            return Email;
        }

        /// <summary>
        /// Get customer's id from email.
        /// </summary>
        /// <param name="customerID"></param>
        /// <returns>Email address as string.</returns>
        public int GetCustomerIDFor(string customerEmail)
        {
            var cusAnalyst = new CusAnalystRepository();
            int customerID = cusAnalyst.GetCustomerID(customerEmail);

            return customerID;
        }

        /// <summary>
        /// Get Customer's company ID.
        /// </summary>
        /// <param name="customerID"></param>
        /// <returns>CompanyID AS INT</returns>
        public int GetCompanyIDFor(int customerID)
        {
            var cusAnalyst = new CusAnalystRepository();
            int companyID = cusAnalyst.GetCompanyID(customerID);

            return companyID;
        }


        /// <summary>
        /// Get Customer's LoginName
        /// </summary>
        /// <param name="customerID"></param>
        /// <returns>LoginName AS string</returns>
        public string GetLoginNameFor(int customerID)
        {
            var cusAnalyst = new CusAnalystRepository();
            string loginName = cusAnalyst.GetLoginName(customerID);

            return loginName;
        }

        /// <summary>
        /// Get Company Admin Email.
        /// </summary>
        /// <param name="System Serial"></param>
        /// <returns>Admin Email AS string</returns>
        public string GetAdminEmailFor(string systemSerial)
        {
            var cusAnalyst = new CusAnalystRepository();
            string AdminEmail = cusAnalyst.GetAdminEmail(systemSerial);

            return AdminEmail;
        }

        public string GetUserNameFor(string email)
        {
            var cusAnalyst = new CusAnalystRepository();
            string userName = cusAnalyst.GetUserName(email);

            return userName;
        }
    }
}