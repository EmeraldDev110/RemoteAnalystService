using System;
using System.ServiceProcess;
using System.Windows.Forms;

namespace RemoteAnalyst.TransMon {
    internal static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main() {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new TransMonService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}