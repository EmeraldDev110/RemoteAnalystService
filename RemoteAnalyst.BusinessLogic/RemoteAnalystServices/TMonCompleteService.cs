using System;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class TMonCompleteService {
        private readonly TMonComplete tMonComplete;

        public TMonCompleteService(TMonComplete tMonComplete) {
            this.tMonComplete = tMonComplete;
        }

        public void InsertLog(string expectedTime, string systemSerial, DateTime finishedTime, string fileName) {
            tMonComplete.InsertCompleteLog(expectedTime, systemSerial, finishedTime, fileName);
        }
    }
}