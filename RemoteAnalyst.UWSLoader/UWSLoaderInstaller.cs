using System.ComponentModel;
using System.ServiceProcess;

namespace RemoteAnalyst.UWSLoader {
    [RunInstaller(true)]
    public partial class UWSLoaderInstaller : System.Configuration.Install.Installer {
        public UWSLoaderInstaller() {
            InitializeComponent();
            var si = new ServiceInstaller();
            var spi = new ServiceProcessInstaller();

            si.ServiceName = "UWSLoader"; // this must match the ServiceName specified in WindowsService1.
            si.DisplayName = "Remote Analyst UWS Loader"; // this will be displayed in the Services Manager.
            si.StartType = ServiceStartMode.Automatic;

            spi.Account = ServiceAccount.LocalSystem; // run under the system account.
            spi.Password = null;
            spi.Username = null;

            this.Installers.Add(si);
            this.Installers.Add(spi);
        }
    }
}