using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using RemoteAnalyst.AWS.Queue;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.UWSRelay.BLL;

namespace RemoteAnalyst.UWSRelay
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            
#if (DEBUG==false)
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new UWSWatcherServices() 
            };
            ServiceBase.Run(ServicesToRun);
#endif
#if DEBUG

#endif
        }
    }
}