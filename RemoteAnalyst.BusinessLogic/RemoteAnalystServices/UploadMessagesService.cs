using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class UploadMessagesService {
        private readonly string _connectionString;

        public UploadMessagesService(string connectionString)
        {
            _connectionString = connectionString;
        }
        public void InsertNewEntryFor(int uploadID, DateTime timeStamp, string message) {
            var uploadMessages = new UploadMessages(_connectionString);
            uploadMessages.InsertNewEntry(uploadID, timeStamp, (int)Source.Types.Windows, message);
        }

        public bool CheckIfAllFilesLoaded(int uploadID) {
            bool loaded = false;
            var uploadMessages = new UploadMessages(_connectionString);

            //Check how many "Processing of" we have on UploadMessage.
            var processCount = uploadMessages.CheckMessageCount(uploadID, "Processing of");

            //Check how many "Done Processing of" we have on UploadMessage.
            var doneProcessing = uploadMessages.CheckMessageCount(uploadID, "Done Processing of");

            //Check how many "Finish Loading" we have on Upload Message.
            var finishLoading = uploadMessages.CheckMessageCount(uploadID, "Finish Loading");

            //If all the numbers are equal, the load is complete and we can send email to customer.

            if (processCount.Equals(doneProcessing) && doneProcessing.Equals(finishLoading))
                loaded = true;
            else if (processCount.Equals(0) && doneProcessing.Equals(0) && finishLoading > 0) {
                loaded = true;
            }

            return loaded;
        }
    }
}
