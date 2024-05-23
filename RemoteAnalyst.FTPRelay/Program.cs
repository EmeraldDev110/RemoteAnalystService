using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.FTPRelay.BLL;

namespace RemoteAnalyst.FTPRelay {
    static class Program {
        private static void Main() {
#if (!DEBUG)
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new FTPRelayService() 
            };
            ServiceBase.Run(ServicesToRun);
#else

#endif
        }
    }
}
