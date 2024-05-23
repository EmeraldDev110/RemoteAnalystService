using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class UploadFileNameServices {
        private readonly string _connectionString = "";

        public UploadFileNameServices(string connectionString) {
            _connectionString = connectionString;
        }

        public int GetOrderIdFor(string fileName) {
            var uploads = new UploadFileNames(_connectionString);
            var orderId = uploads.GetOrderId(fileName);

            return orderId;
        }

        public void UpdateLoadStatusFor(string fileName) {
            var uploads = new UploadFileNames(_connectionString);
            uploads.UpdateLoadStatus(fileName);
        }

        public Dictionary<string, bool> CheckLoadedFor(int orderId) {
            var uploads = new UploadFileNames(_connectionString);
            var loadedList = uploads.CheckLoaded(orderId);

            return loadedList;
        }

        public void DeleteEntriesFor(int orderId) {
            var uploads = new UploadFileNames(_connectionString);
            uploads.DeleteEntries(orderId);
        }
    }
}
