using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySqlConnector;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class UploadStatusService {
        private readonly string _connectionString = "";

        public UploadStatusService(string connectionString) {
            _connectionString = connectionString;
        }

        public int GetStatusIdFor(int orderId) {
            var uploadStatus = new UploadStatus(_connectionString);
            var statusId = uploadStatus.GetStatusId(orderId);

            return statusId;
        }
        public void DeleteEntryFor(int orderId) {
            var uploadStatus = new UploadStatus(_connectionString);
            uploadStatus.DeleteEntry(orderId);
        }

    }
}
