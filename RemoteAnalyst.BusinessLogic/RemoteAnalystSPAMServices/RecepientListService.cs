using System;
using System.Collections.Generic;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices
{
    public class RecepientListService
    {
        private readonly string _connectionString = "";

        public RecepientListService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IList<string> GetEmailListFor(int deliveryID)
        {
            var recepientList = new RecepientList(_connectionString);
            IList<string> emailList = recepientList.GetEmailList(deliveryID);

            return emailList;
        }
    }
}