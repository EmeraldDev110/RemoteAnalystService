using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace RemoteAnalyst.UWSRelay
{
    [RunInstaller(true)]
    public partial class UWSWatcherInstaller : System.Configuration.Install.Installer
    {
        public UWSWatcherInstaller()
        {
            InitializeComponent();

            var si = new ServiceInstaller();
            var spi = new ServiceProcessInstaller();

            si.ServiceName = "UWSRelay"; // this must match the ServiceName specified in WindowsService1.
            si.DisplayName = "Remote Ananlyst UWS Relay"; // this will be displayed in the Services Manager.
            si.StartType = ServiceStartMode.Automatic;

            spi.Account = ServiceAccount.LocalSystem; // run under the system account.
            spi.Password = null;
            spi.Username = null;

            this.Installers.Add(si);
            this.Installers.Add(spi);
        }
    }
}
