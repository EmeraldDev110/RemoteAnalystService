using System.ComponentModel;
using System.ServiceProcess;

namespace RemoteAnalyst.FTPRelay {
    [RunInstaller(true)]
    public partial class FTPRelayInstaller : System.Configuration.Install.Installer {
        public FTPRelayInstaller() {
            InitializeComponent();

            var si = new ServiceInstaller();
            var spi = new ServiceProcessInstaller();

            si.ServiceName = "Remote_Analyst_FTP_Relay"; 
            si.DisplayName = "Remote Analyst FTP Relay"; 
            si.StartType = ServiceStartMode.Automatic;

            spi.Account = ServiceAccount.LocalSystem; 
            spi.Password = null;
            spi.Username = null;

            Installers.Add(si);
            Installers.Add(spi);
        }
    }
}