using System.Data;
using RemoteAnalyst.TransMon.TransMonFactoryPattern.Service;

namespace RemoteAnalyst.TransMon.TransMonFactoryPattern.Model {
    public class Timestamp {
        private readonly IService _iService;

        public Timestamp() {
        }

        public Timestamp(IService iService) {
            _iService = iService;
        }

        public string From { get; set; }
        public string To { get; set; }

        public Timestamp GetSomeData() {
            DataTable table = _iService.Get();
            var timestamp = new Timestamp();

            foreach (DataRow row in table.Rows) {
                timestamp.From = row["From"].ToString();
                timestamp.To = row["To"].ToString();
            }
            return timestamp;
        }
    }
}