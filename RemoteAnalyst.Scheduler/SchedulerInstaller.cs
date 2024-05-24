using System.ComponentModel;
using System.ServiceProcess;

namespace RemoteAnalyst.Scheduler
{
    [RunInstaller(true)]
    public partial class SchedulerInstaller : System.Configuration.Install.Installer
    {
        public SchedulerInstaller()
        {
            InitializeComponent();

            var si = new ServiceInstaller();
            var spi = new ServiceProcessInstaller();

            si.ServiceName = "RemoteAnanlystScheduler"; // this must match the ServiceName specified in WindowsService1.
            si.DisplayName = "Remote Ananlyst Scheduler"; // this will be displayed in the Services Manager.
            si.StartType = ServiceStartMode.Automatic;

            spi.Account = ServiceAccount.LocalSystem; // run under the system account.
            spi.Password = null;
            spi.Username = null;

            this.Installers.Add(si);
            this.Installers.Add(spi);
        }
    }
}