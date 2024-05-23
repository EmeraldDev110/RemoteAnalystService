using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdbSPAM;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices {
    public class QnmService {
        private readonly string _connectionString;

        public QnmService(string connectionStringTrend) {
            _connectionString = connectionStringTrend;
        }

        public List<DateTime> GetDeleteDatesFor(DateTime retentionDate) {
            var deleteList = new List<DateTime>();
            var qnm = new QNM("", _connectionString);
            deleteList = qnm.GetDeleteDates(retentionDate);


            return deleteList;
        }
    }
}
