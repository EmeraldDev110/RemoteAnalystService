using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;
using RemoteAnalyst.TransMon.TransMonFactoryPattern.Model;

namespace RemoteAnalyst.TransMon.TransMonFactoryPattern.Service {
    internal class TransmonService {
        public List<TransmonView> GetTransmonsFor() {
            var transmon = new Transmon(ConnectionString.ConnectionStringDB);
            var dataTable = transmon.GetTransmons();

            var transmonList = new List<TransmonView>();
            foreach (DataRow row in dataTable.Rows) {
                transmonList.Add(new TransmonView {
                    SystemSerial = row["SystemSerial"].ToString(),
                    IntervalInMinutes = Convert.ToInt32(row["Interval"]),
                    ExpectedFileCount = Convert.ToInt32(row["ExpectedCount"]),
                    AllowanceTimeInMinutes = Convert.ToInt32(row["Allowance"])
                });
            }

            return transmonList;
        }
    }
}
