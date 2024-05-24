using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using log4net;
using RemoteAnalyst.SFTP.BLL;
using SBUtils;

namespace RemoteAnalyst.SFTP {
    public partial class SFTPForm : Form {
        private static readonly ILog Log = LogManager.GetLogger("SFTPServiceLog"); 
        public SFTPForm() {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e) {
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            StartSFTP();
        }

        public void StartSFTP() {
            //__Global.SetLicenseKey("451E2FF25A7B8A83B15E7FDC458016F43BB7D1C7759122BB1845A9E9EDB22A32A914636009D26D5D9B2395C1F0A1795EA4E7B711688AA180BAD335D2938F593E99ECF4EC5F27BA15E1C7E627E1EE2BFEFF674F7D055485A2F5C176B7150BE149EA23426138818F0E517D8059FD8A2EE265FF473B5C807DC6345F8BBCF92A26BBF693A01857F949F10C54E35EE09868D1FD5CC0376C13EC2B665BAB12EA38D15A0A3AB55B74F83DB62B09BBD10B3EE452E871E421C26AE52A15A1F698DCEAE5C3DD79A3D96BED0E7EA696F0BFEC998BAD583E6093A12B19E1CD0F2FBBDCBED868490075056395ECBDC5E5B0950A32731B89FA79D968B6689AC6076AB41A273D18");
            __Global.SetLicenseKey("D81949B44AC446B16270B8C4F0EF8DD6866601BE173172716B15D9FD7514033FCFAD98E5AAA8A2DCC89D20081870603643141AD7EE99174DDA1AE688CBE5C5508ACE2B7200821C345B257A5F4C9605BC68482D4C01D7D4C620C9CC001064444DD1E25859685FD8BF65967029D090D7E932F283C0F36E083E85CC8AC03899DF825C6A37E3150CB2B2874B1F3BC432FE2E14C507AD3148BD9C36A8C97C2247B6346F4441C8FE4D2321ADE20F57A6CA5EE9B8AFEF636A6C8E8010A1582D5AF97FE0877DE214AF5EDA2D5DA6E70B7A963832770E0EBF22491C4B3249FF87BA3972C4645AA850F7CA33B739418654A5EC66A106AF7F67E7C0A04A40AA126FDE449EEE");
            Globals.SessionStarted += Globals_SessionStarted;
            Globals.SessionClosed += Globals_SessionClosed;
            Globals.SessionInfoChanged += Globals_SessionInfoChanged;

            lblMessage.Text = "Starting SFTP Server at " + DateTime.Now;
            Globals.StartSSHListener();
        }

        public void StopSFTP() {
            lblMessage.Text = "Stopped SFTP Server at " + DateTime.Now;
            Globals.StopSSHListener();
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

        private void btnStop_Click(object sender, EventArgs e) {
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            StopSFTP();
        }
    }
}
