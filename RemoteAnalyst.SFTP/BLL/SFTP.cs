using log4net;
using SBUtils;

namespace RemoteAnalyst.SFTP.BLL {
    public class SFTP {
        private static readonly ILog Log = LogManager.GetLogger("SFTPServiceLog");
        public void StartSFTP() {
            __Global.SetLicenseKey("D81949B44AC446B16270B8C4F0EF8DD6866601BE173172716B15D9FD7514033FCFAD98E5AAA8A2DCC89D20081870603643141AD7EE99174DDA1AE688CBE5C5508ACE2B7200821C345B257A5F4C9605BC68482D4C01D7D4C620C9CC001064444DD1E25859685FD8BF65967029D090D7E932F283C0F36E083E85CC8AC03899DF825C6A37E3150CB2B2874B1F3BC432FE2E14C507AD3148BD9C36A8C97C2247B6346F4441C8FE4D2321ADE20F57A6CA5EE9B8AFEF636A6C8E8010A1582D5AF97FE0877DE214AF5EDA2D5DA6E70B7A963832770E0EBF22491C4B3249FF87BA3972C4645AA850F7CA33B739418654A5EC66A106AF7F67E7C0A04A40AA126FDE449EEE");
            Globals.SessionStarted += Globals_SessionStarted;
            Globals.SessionClosed += Globals_SessionClosed;
            Globals.SessionInfoChanged += Globals_SessionInfoChanged;
            if (Globals.ServerStarted) {
                Log.Info("*****  Stop SFTP Service  ******");
                Globals.StopSSHListener();
            }
            else {
                Log.Info("*****  Start SFTP Service  ******");
                Globals.StartSSHListener();
            }
        }

        private static void Globals_SessionStarted(SSHSession session) {
            Log.Info("SSH session started");
        }

        private static void Globals_SessionClosed(SSHSession session) {
            Log.Info("SSH session closed");
        }

        private static void Globals_SessionInfoChanged(SSHSession session) {
            Log.Info("SSH session changed");
        }
    }
}