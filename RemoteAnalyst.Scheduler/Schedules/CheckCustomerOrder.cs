using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.Scheduler.Schedules {
    internal class CheckCustomerOrder {
        public void Timer_Elapsed(object source, System.Timers.ElapsedEventArgs e) {
            CheckOrder();
        }

        private void CheckOrder() {
            
        }
    }
}
