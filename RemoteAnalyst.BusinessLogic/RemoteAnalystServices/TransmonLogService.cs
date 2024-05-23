using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using RemoteAnalyst.BusinessLogic.ModelView;
using System.IO;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class TransmonLogService {
        private readonly string _connectionString;

        public TransmonLogService(string connectionString) {
            _connectionString = connectionString;
        }

		public Dictionary<string, int> GetSystemResidual(DateTime adjustedTime) {
			var trasmonLogs = new TransmonLogs(_connectionString);
			var systemsResiduals = trasmonLogs.GetSystemsResidual(adjustedTime);
			Dictionary<string, int> systemSerialAndResidual = new Dictionary<string, int>();
			foreach(DataRow row in systemsResiduals.Rows) {
				if (!systemSerialAndResidual.ContainsKey(row["SystemSerial"].ToString())) {
					systemSerialAndResidual.Add((row["SystemSerial"].ToString()), int.Parse(row["InProgressCount"].ToString()));
				}
			}
			return systemSerialAndResidual;
		}

        public void Insert(string path, DateTime adjustedTime, List<TransmonView> transmonView) {

            string pathToCsv = path + @"\BulkInsertTransmonLogs_" + DateTime.Now.Ticks + ".csv";
            var sb = new StringBuilder();
            if (transmonView.Count > 0) {
                foreach (var view in transmonView) {
                    var row = new StringBuilder();
                    row.Append(view.SystemSerial + "|");
                    row.Append(adjustedTime.ToString("yyyy-MM-dd HH:mm:ss") + "|");
                    row.Append(view.ExpectedFileCount + "|");
                    row.Append(view.LoadedFileCount + "|");
                    row.Append(view.InProgressFileCount + "|");
                    row.Append(view.TotalFileSize);
                    sb.Append(row + Environment.NewLine);
                }
            }
            File.AppendAllText(pathToCsv, sb.ToString());

            var transmonLogs = new TransmonLogs(_connectionString);
            transmonLogs.Insert(pathToCsv);

            if(File.Exists(pathToCsv))
                File.Delete(pathToCsv);
        }
    }
}
