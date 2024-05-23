using System.Collections.Generic;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices
{
    public class ProfileDetailService
    {
        private readonly string ConnectionString = "";

        public ProfileDetailService(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string GetApplicationNameFor(int recordId) {
            var profileDetail = new ProfileDetail(ConnectionString);
            var applicationName = profileDetail.GetApplicationName(recordId);

            return applicationName;
        }
    }
}