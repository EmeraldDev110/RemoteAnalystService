using System;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class UWSFileCountService {
        private readonly string _connectionString = "";

        public UWSFileCountService(string connectionString) {
            _connectionString = connectionString;
        }

        public void InsertFileInfoFor(string systemSerial, string fileName, long fileSize) {
            //Get DataDate and expectedFileCount.
            int fileMonth = Convert.ToInt32(fileName.Substring(2, 2)); //Month.
            int fileDay = Convert.ToInt32(fileName.Substring(4, 2)); //Day.
            int fileYear = DateTime.Now.Year;

            if (fileMonth > DateTime.Now.Month) {
                //use the previous year.
                fileYear = DateTime.Now.Year - 1;
            }

            var dataDate = new DateTime(fileYear, fileMonth, fileDay);

            int expectedFileCount = 1;

            if (fileSize > 100000000) {
                expectedFileCount = 6;
            }

            var uwsFileCounts = new UWSFileCounts(_connectionString);
            uwsFileCounts.InsertFileInfo(systemSerial, dataDate, fileName, fileSize, expectedFileCount);
        }

        public bool CheckDuplicateFor(string systemSerial, DateTime dataDate) {
            var uwsFileCounts = new UWSFileCounts(_connectionString);
            var duplicated = uwsFileCounts.CheckDuplicate(systemSerial, dataDate);

            return duplicated;
        }

        public bool CheckDuplicateFor(string systemSerial, string fileName) {
            var uwsFileCounts = new UWSFileCounts(_connectionString);
            var duplicated = uwsFileCounts.CheckDuplicate(systemSerial, fileName);

            return duplicated;
        }

        public void UpdateActualFileCountFor(string systemSerial, DateTime dataDate) {
            var uwsFileCounts = new UWSFileCounts(_connectionString);
            uwsFileCounts.UpdateActualFileCount(systemSerial, dataDate);
        }

        public bool CheckCurrentCountFor(string systemSerial, DateTime dataDate) {
            var uwsFileCounts = new UWSFileCounts(_connectionString);

            var expectedFileCount = uwsFileCounts.GetExpectedFileCount(systemSerial, dataDate);
            var actualFileCount = uwsFileCounts.GetActualFileCount(systemSerial, dataDate);

            bool okayToLoad = false;
            if (expectedFileCount.Equals(actualFileCount)) {
                okayToLoad = true;
            }

            return okayToLoad;
        }
    }
}