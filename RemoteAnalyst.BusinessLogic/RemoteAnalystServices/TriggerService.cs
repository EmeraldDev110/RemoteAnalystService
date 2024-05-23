using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteAnalyst.BusinessLogic.ModelView;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class TriggerService {
        private readonly string _connectionString = "";

        public TriggerService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void InsertFor(string systemSerial, int triggerType, string fileType, string fileLocation, int uploadId, int uwsId)
        {
            var trigger = new Triggers(_connectionString);
            trigger.Insert(systemSerial, triggerType, fileType, fileLocation, uploadId, uwsId);
        }

        public void InsertFor(string systemSerial, int triggerType, string message) {
            var trigger = new Triggers(_connectionString);
            trigger.Insert(systemSerial, triggerType, message);
        }

        public TriggerView GetTriggerFor(int triggerType) {
            var trigger = new Triggers(_connectionString);
            var data = trigger.GetTrigger(triggerType);

            var triggerView = new TriggerView();
            if (data.Rows.Count > 0) {
                triggerView.TriggerId = Convert.ToInt32(data.Rows[0]["TriggerId"]);
                triggerView.SystemSerial = !data.Rows[0].IsNull("SystemSerial") ? data.Rows[0]["SystemSerial"].ToString() : "";
                triggerView.FileType = !data.Rows[0].IsNull("FileType") ? data.Rows[0]["FileType"].ToString() : "";
                triggerView.FileLocation = !data.Rows[0].IsNull("FileLocation") ? data.Rows[0]["FileLocation"].ToString() : "";
                triggerView.UploadId = !data.Rows[0].IsNull("UploadId") ? Convert.ToInt32(data.Rows[0]["UploadId"]) : 0;
                triggerView.Message = !data.Rows[0].IsNull("Message") ? data.Rows[0]["Message"].ToString() : "";
                triggerView.CustomerId = !data.Rows[0].IsNull("CustomerId") ? Convert.ToInt32(data.Rows[0]["CustomerId"]) : 0;
                triggerView.InsertDate = !data.Rows[0].IsNull("InsertDate") ? Convert.ToDateTime(data.Rows[0]["InsertDate"]) : DateTime.MinValue;
                triggerView.UwsId = !data.Rows[0].IsNull("UWSID") ? Convert.ToInt32(data.Rows[0]["UWSID"]) : 0;
            }

            return triggerView;
        }

        public void DeleteTriggerFor(int triggerId) {
            var trigger = new Triggers(_connectionString);
            trigger.DeleteTriiger(triggerId);
        }
    }
}
