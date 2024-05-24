using System;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;

namespace RemoteAnalyst.SFTP {
    internal static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main() {
#if(!DEBUG)
            ServiceBase[] ServicesToRun = new ServiceBase[] {
                new SFTPService()
            };
            ServiceBase.Run(ServicesToRun);
            /*try {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new SFTPForm());
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }*/
#else
            var sftp = new BLL.SFTP();
            sftp.StartSFTP();
            //Thread.Sleep(Timeout.Infinite);
#endif
        }
    }
}