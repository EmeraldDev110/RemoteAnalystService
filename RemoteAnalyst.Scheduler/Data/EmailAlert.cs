using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteAnalyst.Scheduler.Data
{
    internal class EmailAlert
    {

        public string message { get; set; }
        public string source { get; set; }
        public Timer emailTimer;
        AutoResetEvent autoEvent;

        public EmailAlert(string message, string source, Timer emailTimer, AutoResetEvent autoEvent)
        {
            message = this.message;
            source = this.source;
            emailTimer = this.emailTimer;
            autoEvent = this.autoEvent;
        }
    }
}
