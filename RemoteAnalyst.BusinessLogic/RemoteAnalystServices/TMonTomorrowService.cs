using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class TMonTomorrowService {
        private readonly TMonTomorrow tMonTomorrow;

        public TMonTomorrowService(TMonTomorrow tMonTomorrow) {
            this.tMonTomorrow = tMonTomorrow;
        }

        public void PopulateTMonTomorrowFor(List<TMonTomorrowView> tMonTomorrowViews, string path) {
            IOrderedEnumerable<TMonTomorrowView> orderedTMonTomorrowViews = tMonTomorrowViews.OrderBy(x => x.ExpectedTime).ThenBy(x => x.SystemSerial);
            var table = new DataTable("TMonTomorrow");
            table.Columns.Add(new DataColumn("ExpectedTime", typeof (System.DateTime)));
            table.Columns.Add(new DataColumn("SystemSerial", typeof (string)));

            foreach (TMonTomorrowView tMonTomorrowView in orderedTMonTomorrowViews) {
                table.Rows.Add(tMonTomorrowView.ExpectedTime, tMonTomorrowView.SystemSerial);
            }
            tMonTomorrow.PopulateTMonTomorrow(table, path);
        }

        public TMonTomorrowView GetExpectedTimeFor() {
            DataTable tMonTomorrowV = tMonTomorrow.GetExpectedTime();
            var tMonTomorrowView = new TMonTomorrowView();
            foreach (DataRow dr in tMonTomorrowV.Rows) {
                tMonTomorrowView.ExpectedTime = Convert.ToDateTime(dr["ExpectedTime"]);
                tMonTomorrowView.SystemSerial = Convert.ToString(dr["SystemSerial"]);
            }

            return tMonTomorrowView;
        }

        public void DeleteExpectedTimeFor(DateTime expectedTime, string systemSerial) {
            tMonTomorrow.DeleteExpectedTime(expectedTime, systemSerial);
        }

        public void DeleteJobsTomorrowFor() {
            tMonTomorrow.DeleteJobsTomorrow();
        }
    }
}