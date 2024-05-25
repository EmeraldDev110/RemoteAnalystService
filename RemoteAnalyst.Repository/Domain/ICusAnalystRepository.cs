using System.Collections.Generic;
using System.Data;

namespace RemoteAnalyst.Repository.Domain
{
    public interface ICusAnalystRepository
    {
        IList<int> GetCustomers(int companyID);
        DataTable GetCustomerEmail(int customerID);
        string GetEmailAddress(int customerID);
        int GetCustomerID(string customerEmail);
        int GetCompanyID(int customerID);
        string GetLoginName(int customerID);
        string GetAdminEmail(string systemSerial);
        string GetUserName(string email);
    }
}
