using System.Timers;
using RemoteAnalyst.ReportGenerator.BLL.Schedule;

namespace RemoteAnalyst.ReportGenerator.BLL {
    internal class JobSchedules {
        private Timer _timerDPAQueue;
        private Timer _timerQTQueue;

        public void StartScheduleTimers() {
            StartDPAQueue();
            StartQTQueue();
        }

        public void StopScheduleTimers() {
            _timerDPAQueue = null;
            _timerQTQueue = null;
        }

        private void StartDPAQueue() {
            var checkDPAQueue = new CheckDPAQueue();
            _timerDPAQueue = new Timer(60000); //Once an 60 sec.
            _timerDPAQueue.Elapsed += checkDPAQueue.TimerCheckDPAQueue_Elapsed;
            _timerDPAQueue.AutoReset = true;
            _timerDPAQueue.Enabled = true;
        }

        private void StartQTQueue() {
            var checkQTQueue = new CheckQTQueue();
            _timerQTQueue = new Timer(60000); //Once an 60 sec.
            _timerQTQueue.Elapsed += checkQTQueue.TimerCheckQTQueue_Elapsed;
            _timerQTQueue.AutoReset = true;
            _timerQTQueue.Enabled = true;
        }

    }
}