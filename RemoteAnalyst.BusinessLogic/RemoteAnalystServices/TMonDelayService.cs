using System;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class TMonDelayService {
        private readonly TMonDelay tMonDelay;

        public TMonDelayService(TMonDelay tMonDelay) {
            this.tMonDelay = tMonDelay;
        }

        public void InsertLog(string expectedTime, string systemSerial, DateTime delayDate, string fileName) {
            tMonDelay.InsertDelayLog(expectedTime, systemSerial, delayDate, fileName);
        }
    }
}