using System;
using System.Collections;
using System.IO;
using log4net;
using SBSSHKeyStorage;
using __Global = SBUtils.__Global;

namespace RemoteAnalyst.SFTP.BLL {
    public class DemoSettings {

        private static readonly ILog Log = LogManager.GetLogger("SFTPServiceLog");
        #region Public properties

        /// <summary>
        /// Indicates if demo settings are loaded
        /// </summary>
        public bool Empty {
            get {
                return m_Empty;
            }
        }

        /// <summary>
        /// Stores information about registered users
        /// </summary>
        public ArrayList Users {
            get {
                return m_Users;
            }
        }

        /// <summary>
        /// Private key of the host
        /// </summary>
        public string ServerKey {
            get {
                return m_ServerKey;
            }
            set {
                m_ServerKey = value;
            }
        }

        /// <summary>
        /// Server socket address
        /// </summary>
        public string ServerHost {
            get {
                return m_ServerHost;
            }
            set {
                m_ServerHost = value;
            }
        }

        /// <summary>
        /// Server port
        /// </summary>
        public int ServerPort {
            get {
                return m_ServerPort;
            }
            set {
                m_ServerPort = value;
            }
        }

        /// <summary>
        /// Compression flag
        /// </summary>
        public bool ForceCompression { get; set; }

        #endregion

        #region Public methods

        /// <summary>
        /// Searches for user in the registered user list
        /// </summary>
        /// <param name="user">search result</param>
        /// <param name="UserName">user login name</param>
        /// <returns>true if the user with UserName account was found in the list, false otherwise</returns>
        public bool FindUser(ref UserInfo user, string UserName) {
            if (UserName == null) {
                return false;
            }
            for (int i = 0; i < Users.Count; i++) {
                if (UserName.Equals(((UserInfo) Users[i]).Name)) {
                    user = (UserInfo) Users[i];
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Adds new user information record to the list
        /// </summary>
        /// <returns>true on success</returns>
        public bool AddUser() {
            Users.Add(new UserInfo());
            return true;
        }

        /// <summary>
        /// Removes user information record from the list
        /// </summary>
        /// <param name="UserNumber">record index in the list</param>
        /// <returns>true on success, false otherwise</returns>
        public bool RemoveUser(int UserNumber) {
            try {
                Users.RemoveAt(UserNumber);
                return true;
            }
            catch (Exception) {
                return (false);
            }
        }

        /// <summary>
        /// Sets the new server host key
        /// </summary>
        /// <param name="HostKey">private host key string</param>
        /// <returns>true if the key was set successfully, false otherwise</returns>
        public bool SetHostKey(string HostKey) {
            try {
                var Key = new TElSSHKey();
                int Result = Key.LoadPrivateKey(__Global.BytesOfString(HostKey), HostKey.Length, "");
                return Result == 0;
            }
            catch (Exception exc) {
                Log.ErrorFormat("SetHostKey : {0}", exc.Message);
            }
            return false;
        }

        /// <summary>
        /// Loads demo settings from configuration files
        /// </summary>
        /// <returns>true on success</returns>
        public bool LoadSettings() {
            try {
                /*string AppPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                AppPath = AppPath + "\\SecureBlackbox\\";
                if (!Directory.Exists(AppPath)) {
                    Directory.CreateDirectory(AppPath);
                }
                m_UsersFile = AppPath + "sshsrvusers";
                m_ServerKeyFile = AppPath + "SSHSrvHostkey";
                m_IPFile = AppPath + "sshbindings";
                if ((!File.Exists(m_UsersFile)) || (!File.Exists(m_ServerKeyFile))) {
                    m_Empty = true;
                }
                else {
                    m_Empty = false;
                }*/
                // Read user information
                /*var sr = new StreamReader(m_UsersFile);
                string line = null;
                while ((line = sr.ReadLine()) != null) {
                    if (line.Length > 0) {
                        var ui = new UserInfo(line);
                        Users.Add(ui);
                    }
                }*/

                //sr.Close();
                // Read server public key
                /*try {
                    sr = new StreamReader(m_ServerKeyFile);
                    string AServerKey = sr.ReadToEnd();
                    sr.Close();
                    var Key = new TElSSHKey();
                    int Result = Key.LoadPrivateKey(m_ServerKeyFile, "");
                    if (Result == 0) {
                        m_ServerKey = AServerKey;
                    }
                }
                catch (Exception exc) {
                    Logger.Log("Unable to load server key " + exc.Message, true);
                    return false;
                }*/

                // Read user information
                var ui = new UserInfo("raupload:FD1A781F:e4d5a1d8b6b60dcf382bc0ade8f9c143:password:");
                Users.Add(ui);

                // Read server public key
                m_ServerKey = 
@"-----BEGIN RSA PRIVATE KEY-----
MIICXQIBAAKBgQDY2S9PEsbmYbsQtBVURE4YOgEHHE9VSu9FPPNGrX6IY4tvRT3F
XPgujPAlbZ3CrgLYU/0E5gX74XRnY71aZBvIn5RPigH9Z3d00E0yRZTpjeK9Iz5n
E5cuGqs/zf4JkWMQLRboR170A+pnSi7kT/zzgf6PbbxDWXLSG2covirHIwIEAAEA
AQKBgQDOj7P9Asns0tO6yZA3wQkTEs2/1DvN55+cuL6UaNfLW+eKis6YnkLbNO3c
+Vn6BIA5SWgPrn2svcqAYgYRgKLk64ntaVIm4x0mfT82f4DCDZZQRPW9yp+QhsMs
dS/uAYMfi6Lq57tCW9db50nVbJ+DZjb7uHcsRMkwikHpfPIgYQJBAN/5FRgQM68g
r+XaivT38emQ9y5EK3Y2RNUC16gLvRQXCNRFakvgqDm3w0fU4yJ6Ox7x0HUli6wF
Lm1p0EWdTs0CQQD320tLQHjIcklkZXusC8M1ZstOqQb583EYThh+iXRssU/vUjpg
DPW6P0WdtnLKYlMPI6Lw1a9pGq86xACKJ42vAkAwM1Ku0w3MaqRwOxAcmB+fvGr1
sgYIcrVtgicXKy+N20czJ50wpzCM+1czZkVbbiK7Dh9mlqXwZ00Oju8bjDchAkAv
tn9gFSErNRT7xq1wCTOi7A7nLZKyOzLiQuZkNYG8CsYgg+vI4bXMDLISEjU20Ia8
u1d6lSgXS5O/9EtGTSbxAkB0F7Iepn4PA536BcsNbs5+OQRTBe7uIJXn681kICde
LyRxKmJndtEHh6mzG2kFk2DSEWDq2UtbeQGHQS3iwwef
-----END RSA PRIVATE KEY-----";

                // Read IP address
                m_ServerHost = "0.0.0.0";
                m_ServerPort = 22;
                return !Empty;
                
                // Read IP address
                /*try {
                    sr = new StreamReader(m_IPFile);
                    string AServerHost = sr.ReadToEnd();
                    sr.Close();
                    string[] par = AServerHost.Split(':');
                    m_ServerHost = par[0];
                    m_ServerPort = int.Parse(par[1]);
                }
                catch (Exception exc) {
                    Logger.Log("Unable to read host:posrt " + exc.Message, true);
                    m_ServerHost = "0.0.0.0";
                    m_ServerPort = 22;
                }*/
            }
            catch (Exception exc) {
                Log.ErrorFormat("Error reading settings : {0}", exc.Message);
                return false;
            }
        }

        /// <summary>
        /// Saves server configuration settings to files
        /// </summary>
        public void SaveSettings() {
            try {
                // Users
                var sw = new StreamWriter(m_UsersFile, false);
                foreach (UserInfo user in Users) {
                    sw.WriteLine(user.ToString());
                }
                sw.Close();
                // Server private key
                sw = new StreamWriter(m_ServerKeyFile, false);
                sw.Write(ServerKey);
                sw.Close();
                // Server ip bindings
                sw = new StreamWriter(m_IPFile, false);
                sw.Write(ServerHost + ":" + ServerPort);
                sw.Close();
            }
            catch (Exception exc) {
                Log.ErrorFormat("SaveSettings : {0}", exc.Message);
            }
        }

        #endregion

        #region Class members

        // User information file
        private readonly ArrayList m_Users = new ArrayList();
        private bool m_Empty = true;
        private string m_IPFile = "";
        private string m_ServerHost = "127.0.0.1";
        private string m_ServerKey = "";
        private string m_ServerKeyFile = "";
        private int m_ServerPort = 22;
        private string m_UsersFile = "";

        public DemoSettings() {
            ForceCompression = false;
        }

        #endregion
    }
}