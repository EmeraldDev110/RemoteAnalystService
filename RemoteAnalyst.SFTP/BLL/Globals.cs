using System;
using SBUtils;

namespace RemoteAnalyst.SFTP.BLL {
    public class Globals {
        #region Public properties

        public static DemoSettings Settings {
            get {
                return m_Settings;
            }
        }

        public static bool ServerStarted { get; set; }

        private static ServerListener SSHListener {
            get {
                return m_SSHListener;
            }
        }

        #endregion

        #region Public events

        public delegate void SessionClosedHandler(SSHSession session);

        public delegate void SessionInfoChangedHandler(SSHSession session);

        public delegate void SessionStartedHandler(SSHSession session);

        public static event SessionStartedHandler SessionStarted;

        public static event SessionClosedHandler SessionClosed;

        public static event SessionInfoChangedHandler SessionInfoChanged;

        #endregion

        #region Public methods

        static Globals() {
            ServerStarted = false;
            m_Settings = new DemoSettings();
            m_Settings.LoadSettings();
        }

        public Globals() {
            //#error Please pick the evaluation license key from <SecureBlackbox>\LicenseKey.txt file and place it here. If the evaluation key expires, you can request an extension using the form on https://www.eldos.com/sbb/keyreq/
            __Global.SetLicenseKey("D81949B44AC446B16270B8C4F0EF8DD6866601BE173172716B15D9FD7514033FCFAD98E5AAA8A2DCC89D20081870603643141AD7EE99174DDA1AE688CBE5C5508ACE2B7200821C345B257A5F4C9605BC68482D4C01D7D4C620C9CC001064444DD1E25859685FD8BF65967029D090D7E932F283C0F36E083E85CC8AC03899DF825C6A37E3150CB2B2874B1F3BC432FE2E14C507AD3148BD9C36A8C97C2247B6346F4441C8FE4D2321ADE20F57A6CA5EE9B8AFEF636A6C8E8010A1582D5AF97FE0877DE214AF5EDA2D5DA6E70B7A963832770E0EBF22491C4B3249FF87BA3972C4645AA850F7CA33B739418654A5EC66A106AF7F67E7C0A04A40AA126FDE449EEE");
        }

        public static void StartSSHListener() {
            SSHListener.SessionClosed += SSHListener_SessionClosed;
            SSHListener.SessionStarted += SSHListener_SessionStarted;
            SSHListener.SessionInfoChanged += SSHListener_SessionInfoChanged;
            SSHListener.Start();
        }

        public static void StopSSHListener() {
            lock (SSHListener) {
                try {
                    m_SSHListener.Dispose();
                }
                catch (Exception) {
                }
                m_SSHListener = new ServerListener();
            }
        }

        public static string AuthTypeToStr(int AuthType) {
            switch (AuthType) {
                case SBSSHConstants.__Global.SSH_AUTH_TYPE_RHOSTS:
                    return "Rhosts";
                case SBSSHConstants.__Global.SSH_AUTH_TYPE_PUBLICKEY:
                    return "PublicKey";
                case SBSSHConstants.__Global.SSH_AUTH_TYPE_PASSWORD:
                    return "Password";
                case SBSSHConstants.__Global.SSH_AUTH_TYPE_HOSTBASED:
                    return "Hostbased";
                case SBSSHConstants.__Global.SSH_AUTH_TYPE_KEYBOARD:
                    return "Keyboard-interactive";
                default:
                    return "Unknown";
            }
        }

        #endregion

        #region Event handlers

        private static void SSHListener_SessionClosed(SSHSession sender) {
            SessionClosed(sender);
        }

        private static void SSHListener_SessionStarted(SSHSession sender) {
            SessionStarted(sender);
        }

        private static void SSHListener_SessionInfoChanged(SSHSession sender) {
            SessionInfoChanged(sender);
        }

        #endregion

        #region Class members

        private static readonly DemoSettings m_Settings;
        //        private static frmMain m_main = null;
        private static ServerListener m_SSHListener = new ServerListener();

        #endregion
    }
}