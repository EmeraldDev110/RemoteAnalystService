using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace RemoteAnalyst.ReportGenerator
{
    [RunInstaller(true)]
    public partial class ReportGeneratorInstaller : System.Configuration.Install.Installer
    {
        public ReportGeneratorInstaller()
        {
            InitializeComponent();

            ServiceInstaller si = new ServiceInstaller();
            ServiceProcessInstaller spi = new ServiceProcessInstaller();

            si.ServiceName = "RemoteAnanlystReportGenerator"; // this must match the ServiceName specified in WindowsService1.
            si.DisplayName = "Remote Ananlyst Report Generator"; // this will be displayed in the Services Manager.
            si.StartType = ServiceStartMode.Automatic;

            spi.Account = ServiceAccount.LocalSystem; // run under the system account.
            spi.Password = null;
            spi.Username = null;

            this.Installers.Add(si);
            this.Installers.Add(spi);
        }
    }
}
