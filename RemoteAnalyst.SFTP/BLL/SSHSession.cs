using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using SBSSHCommon;
using SBSSHHandlers;
using SBSSHKeyStorage;
using SBSSHServer;
using SBStringList;
using SBUtils;
using Timer = System.Timers.Timer;
using __Global = SBSSHConstants.__Global;
using log4net;

namespace RemoteAnalyst.SFTP.BLL {
    public class SSHSession : IDisposable {
        private static readonly ILog Log = LogManager.GetLogger("SFTPServiceLog");
        #region Public properties

        public object Data { get; set; }

        public string Status {
            get {
                return m_Status;
            }
        }

        public string Username {
            get {
                return m_Username;
            }
        }

        public string Host {
            get {
                return m_Host;
            }
        }

        public string ClientSoftware {
            get {
                return m_ClientSoftware;
            }
        }

        public DateTime StartTime {
            get {
                return m_StartTime;
            }
        }

        #endregion

        #region Public events

        public delegate void SessionClosedHandler(SSHSession sender);

        public delegate void SessionInfoChangedHandler(SSHSession sender);

        public event SessionClosedHandler SessionClosed;

        public event SessionInfoChangedHandler SessionInfoChanged;

        #endregion

        #region Public methods

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="socket">Socket object for accepted TCP connection</param>
        /// <param name="id">Session identifier</param>
        public SSHSession(Socket socket, int id) {
            Data = null;
            m_Socket = socket;
            try {
                m_Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new LingerOption(true, 300));
            }
            catch (Exception exc) {
                Log.ErrorFormat("SSHSession() : {0}", exc.Message);
            }

            m_Host = ((IPEndPoint) (socket.RemoteEndPoint)).Address + ":" + ((IPEndPoint) (socket.RemoteEndPoint)).Port;
            m_StartTime = DateTime.Now;

            m_SocketSessionTimer = new Timer();
            m_SocketSessionTimer.Elapsed += OnSocketSessionTimeout;
            m_SocketSessionTimer.AutoReset = true;

            m_synch = new object();

            SetupServer();

            m_Thread = new Thread(ReadThread);
            m_Thread.Name = "SSH Server worker thread";
            m_Opened = true;
            m_Thread.Start();
        }

        /// <summary>
        /// Starts session
        /// </summary>
        public void Start() {
            SessionInfoChanged(this);
            RestartSocketSessionTimer();
        }

        /// <summary>
        /// Triggers SessionClosed event
        /// </summary>
        public void CloseSession() {
            if (m_Opened) {
                try {
                    CleanUp();
                    if (this.SessionClosed != null) {
                        this.SessionClosed(this);
                    }
                }
                finally {
                    m_Opened = false;
                }
            }
        }

        #endregion

        #region SSH Server authentication processing

        /// <summary>
        /// Is fired when user performs an authentication attempt
        /// </summary>
        /// <param name="Sender">ElSSHServer object</param>
        /// <param name="Username">User login name</param>
        /// <param name="AuthType">Used authentication type</param>
        /// <param name="Accept">Set to true, if user is allowed to perform this type of authentication</param>
        private void SSHServer_OnAuthAttempt(object Sender, string Username, int AuthType, ref bool Accept) {
            UserInfo user = null;
            if (Globals.Settings.FindUser(ref user, Username)) {
                Accept = (user.AuthTypes | AuthType) > 0;
                m_Username = Username;
                m_ClientSoftware = m_SSHServer.ClientSoftwareName;
                SessionInfoChanged(this);
            }
            else {
                Accept = false;
            }
        }

        /// <summary>
        /// Is fired when user authentication attempt fails
        /// </summary>
        /// <param name="Sender">ElSSHServer objects</param>
        /// <param name="AuthenticationType">Authentication type that failed</param>
        private void SSHServer_OnAuthFailed(object Sender, int AuthenticationType) {
            Log.InfoFormat("Authentication attempt ({0}) failed", Globals.AuthTypeToStr(AuthenticationType));
        }

        /// <summary>
        /// Is fired when user tries password authentication
        /// </summary>
        /// <param name="Sender">ElSSHServer object</param>
        /// <param name="Username">User login name</param>
        /// <param name="Password">User password</param>
        /// <param name="Accept">Set to true, if the provided password is valid</param>
        /// <param name="ForceChangePassword">Set to true to force user to change his password</param>
        private void SSHServer_OnAuthPassword(object Sender, string Username, string Password, ref bool Accept, ref bool ForceChangePassword) {
            UserInfo user = null;
            if (Globals.Settings.FindUser(ref user, Username)) {
                Accept = (user.AuthTypes & __Global.SSH_AUTH_TYPE_PASSWORD) > 0;
                Accept = Accept & user.PasswordValid(Password);
                if (Accept) {
                    int authFlag;
                    if (m_authInfo[Username] == null) {
                        authFlag = 0;
                    }
                    else {
                        authFlag = (int) m_authInfo[Username];
                    }
                    authFlag = authFlag | __Global.SSH_AUTH_TYPE_PASSWORD;
                    if ((user.AuthTypes & __Global.SSH_AUTH_TYPE_KEYBOARD) > 0) {
                        authFlag = authFlag | __Global.SSH_AUTH_TYPE_KEYBOARD;
                    }
                    m_authInfo[Username] = authFlag;
                }
            }
            else {
                Accept = false;
            }
        }

        /// <summary>
        /// Is fired when user tries public key authentication
        /// </summary>
        /// <param name="Sender">ElSSHServer object</param>
        /// <param name="Username">User login name</param>
        /// <param name="Key">User's public key</param>
        /// <param name="Accept">Set to true if the provided public key is valid</param>
        private void SSHServer_OnAuthPublicKey(object Sender, string Username, TElSSHKey Key, ref bool Accept) {
            UserInfo user = null;

            if (Globals.Settings.FindUser(ref user, Username)) {
                Accept = (user.AuthTypes & __Global.SSH_AUTH_TYPE_PUBLICKEY) > 0;
                Accept = Accept & user.KeyValid(Key);
                if (Accept) {
                    int authFlag;
                    if (m_authInfo[Username] == null) {
                        authFlag = 0;
                    }
                    else {
                        authFlag = (int) m_authInfo[Username];
                    }
                    authFlag = authFlag | __Global.SSH_AUTH_TYPE_PUBLICKEY;
                    m_authInfo[Username] = authFlag;
                }
            }
            else {
                Accept = false;
            }
        }

        /// <summary>
        /// Is fired when user tries keyboard-interactive authentication
        /// </summary>
        /// <param name="Sender">ElSSHServer object</param>
        /// <param name="Username">User login name</param>
        /// <param name="Submethods">Names of submethods that that the client wishes to use</param>
        /// <param name="Name">Set this to authentication title</param>
        /// <param name="Instruction">Set this to authentication instruction</param>
        /// <param name="Requests">Add the desired requests to this list</param>
        /// <param name="Echoes">Set the bits of this object depending on the corresponding responses should be echoed</param>
        private void SSHServer_OnAuthKeyboard(object Sender, string Username, TElStringList Submethods,
            ref string Name, ref string Instruction, TElStringList Requests, TElBits Echoes) {
            UserInfo user = null;
            if ((Globals.Settings.FindUser(ref user, Username)) && ((user.AuthTypes & __Global.SSH_AUTH_TYPE_KEYBOARD) > 0)) {
                Name = "Keyboard-interactive authentication";
                Instruction = "Please enter the following information";
                Requests.Add("Username: ");
                Requests.Add("Password: ");
                Echoes.Size = 2;
                Echoes[0] = true;
                Echoes[1] = false;
            }
        }

        /// <summary>
        /// Is fired when the keyboard-interactive response is received from client
        /// </summary>
        /// <param name="Sender">ElSSHServer object</param>
        /// <param name="Requests">Requests list from the last keyboard-interactive request</param>
        /// <param name="Responses">User's responses</param>
        /// <param name="Name">Set this to next authentication stage title</param>
        /// <param name="Instruction">Set this to next authentication stage instructions</param>
        /// <param name="NewRequests">Add requests for next authentication stage to this list</param>
        /// <param name="Echoes">Set echo bits accordingly</param>
        /// <param name="Accept">Set to true if the responses are valid, or to false if the authentication process should be continued</param>
        private void SSHServer_OnAuthKeyboardResponse(object Sender, TElStringList Requests, TElStringList Responses,
            ref string Name, ref string Instruction, TElStringList NewRequests, TElBits Echoes, ref bool Accept) {
            Accept = false;
            if ((Responses != null) && (Responses.Count == 2)) {
                string Username = Responses[0];
                string Password = Responses[1];
                UserInfo user = null;
                if (Globals.Settings.FindUser(ref user, Username)) {
                    Accept = (user.AuthTypes & __Global.SSH_AUTH_TYPE_KEYBOARD) > 0;
                    Accept = Accept & user.PasswordValid(Password);
                    if (Accept) {
                        int authFlag;
                        if (m_authInfo[Username] == null) {
                            authFlag = 0;
                        }
                        else {
                            authFlag = (int) m_authInfo[Username];
                        }
                        authFlag = authFlag | __Global.SSH_AUTH_TYPE_KEYBOARD;
                        if ((user.AuthTypes & __Global.SSH_AUTH_TYPE_PASSWORD) > 0) {
                            authFlag = authFlag | __Global.SSH_AUTH_TYPE_PASSWORD;
                        }
                        m_authInfo[Username] = authFlag;
                    }
                }
            }
        }

        /// <summary>
        /// Queries if further client authentication is needed
        /// </summary>
        /// <param name="Sender">ElSSHServer object</param>
        /// <param name="Username">User login name</param>
        /// <param name="Needed">Set to true if further authentication is needed, or to false if the authentication stage is completed</param>
        private void SSHServer_OnFurtherAuthNeeded(object Sender, string Username, ref bool Needed) {
            UserInfo user = null;
            Needed = true;
            if (Globals.Settings.FindUser(ref user, Username)) {
                if (m_authInfo[Username] != null) {
                    var authFlag = (int) m_authInfo[Username];
                    if ((user.AuthAll) && (authFlag == user.AuthTypes)) {
                        Needed = false;
                    }
                    else if ((!user.AuthAll) && ((authFlag & user.AuthTypes) != 0)) {
                        Needed = false;
                    }
                }
            }
        }

        #endregion SSH Server authentication handling

        #region SSH Server socket-related processing

        /// <summary>
        /// Is fired when ElSSHServer has data to write to socket
        /// </summary>
        /// <param name="Sender">ElSSHServer object</param>
        /// <param name="Buffer">Data to write to socket</param>
        private void SSHServer_OnSend(object Sender, byte[] Buffer) {
            try {
                int toSend = Buffer.Length;
                int sent;
                int ptr = 0;
                while (toSend > 0) {
                    sent = m_Socket.Send(Buffer, ptr, toSend, SocketFlags.None);
                    ptr += sent;
                    toSend -= sent;
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("Socket send operation failed: {0}", ex);
                if (m_SSHServer.Active) {
                    m_SSHServer.OnSend -= SSHServer_OnSend;
                    Monitor.Enter(m_lock);
                    try {
                        m_SSHServer.Close(true);
                    }
                    finally {
                        Monitor.Exit(m_lock);
                    }
                }
                else {
                    CloseSession();
                }
            }
        }

        /// <summary>
        /// Is fired when ElSSHServer needs some data to be read from socket
        /// </summary>
        /// <param name="Sender">ElSSHServer object</param>
        /// <param name="Buffer">Place where to put received data</param>
        /// <param name="MaxSize">Maximal amount of data to receive</param>
        /// <param name="Written">Number of bytes actually written</param>
        private void SSHServer_OnReceive(object Sender, ref byte[] Buffer, int MaxSize, out int Written) {
            try {
                if (m_Socket.Poll(100000, SelectMode.SelectRead)) {
                    Written = m_Socket.Receive(Buffer, MaxSize, SocketFlags.None);
                    if (Written == 0) {
                        if (m_SSHServer.Active) {
                            m_SSHServer.OnSend -= SSHServer_OnSend;
                            Monitor.Enter(m_lock);
                            try {
                                m_SSHServer.Close(true);
                            }
                            finally {
                                Monitor.Exit(m_lock);
                            }
                        }
                        else {
                            CloseSession();
                        }
                    }
                    else {
                        RestartSocketSessionTimer();
                    }
                }
                else {
                    Written = 0;
                }
            }
            catch (Exception ex) {
                Written = 0;
                Log.ErrorFormat("Socket receive operation failed: {0}", ex);
                if (m_SSHServer.Active) {
                    m_SSHServer.OnSend -= SSHServer_OnSend;
                    Monitor.Enter(m_lock);
                    try {
                        m_SSHServer.Close(true);
                    }
                    finally {
                        Monitor.Exit(m_lock);
                    }
                }
                else {
                    CloseSession();
                }
            }
        }

        #endregion

        #region SSH Server general-purpose event handlers

        /// <summary>
        /// Is fired when SSH session is closed
        /// </summary>
        /// <param name="Sender">ElSSHServer object</param>
        private void SSHServer_OnCloseConnection(object Sender) {
            CloseSession();
            m_Error = true;
        }

        /// <summary>
        /// Is fired if some error occurs during SSH communication
        /// </summary>
        /// <param name="Sender">ElSSHServer object</param>
        /// <param name="ErrorCode">Error code</param>
        private void SSHServer_OnError(object Sender, int ErrorCode) {
            Log.ErrorFormat("SSH protocol error #" + ErrorCode);
            m_Error = true;
        }

        #endregion

        #region SSH Server connection-layer event handlers

        /// <summary>
        /// Is fired when a client requests SSH subsystem
        /// </summary>
        /// <param name="Sender">ElSSHServer object</param>
        /// <param name="Connection">Logical connection object</param>
        /// <param name="Subsystem">Subsystem name</param>
        private void SSHServer_OnOpenSubsystem(object Sender, TElSSHTunnelConnection Connection, string Subsystem) {
            Log.InfoFormat("Subsystem {0} opened", Subsystem);
            if (Subsystem == "sftp") {
                var sess = new SFTPSession(Connection);
                m_Status += "SFTP ";
                SessionInfoChanged(this);
            }
        }

        /// <summary>
        /// Is fired when a client requests shell
        /// </summary>
        /// <param name="Sender">ElSSHServer object</param>
        /// <param name="Connection">Logical connection object</param>
        private void SSHServer_OnOpenShell(object Sender, TElSSHTunnelConnection Connection) {
            Log.Info("Shell requested");
            var thread = new TElSSHSubsystemThread(new TElShellSSHSubsystemHandler(Connection, true), Connection, true);
            thread.Handler.OnUnsafeOperationStart += Handler_OnUnsafeOperationStart;
            thread.Handler.OnUnsafeOperationEnd += Handler_OnUnsafeOperationEnd;
            thread.Resume();
            m_Status += "Shell ";
            SessionInfoChanged(this);
        }

        private void m_SSHServer_OnOpenCommand(object Sender, TElSSHTunnelConnection Connection, string Command) {
            Log.Info("Command requested");
            var thread = new TElSSHSubsystemThread(new TElShellSSHSubsystemHandler(Connection, true), Connection, true);
            ((TElShellSSHSubsystemHandler) (thread.Handler)).Command = Command;
            thread.Handler.OnUnsafeOperationStart += Handler_OnUnsafeOperationStart;
            thread.Handler.OnUnsafeOperationEnd += Handler_OnUnsafeOperationEnd;
            thread.Resume();
            m_Status += "Command ";
            SessionInfoChanged(this);
        }

        private void Handler_OnUnsafeOperationStart(object Sender) {
            Monitor.Enter(m_lock);
        }

        private void Handler_OnUnsafeOperationEnd(object Sender) {
            Monitor.Exit(m_lock);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Sets up server properties
        /// </summary>
        private void SetupServer() {
            short i;
            lock (Globals.Settings) {
                try {
                    var Key = new TElSSHKey();
                    byte[] BServerKey = SBUtils.__Global.BytesOfString(Globals.Settings.ServerKey);
                    int Result = Key.LoadPrivateKey(BServerKey, BServerKey.Length, "");
                    if (Result == 0) {
                        m_HostKeys.Add(Key);
                    }
                }
                catch (Exception exc) {
                    Log.ErrorFormat("SSHSession.SetupServer - invalid private key"
                               + exc.Message, true);
                }
            }
            m_SSHServer.KeyStorage = m_HostKeys;
            m_SSHServer.AllowedSubsystems.Add("sftp");
            m_SSHServer.SoftwareName = "SSHBlackbox.10";
            m_SSHServer.ForceCompression = Globals.Settings.ForceCompression;
            m_SSHServer.OnAuthAttempt += SSHServer_OnAuthAttempt;
            m_SSHServer.OnAuthFailed += SSHServer_OnAuthFailed;
            m_SSHServer.OnAuthPassword += SSHServer_OnAuthPassword;
            m_SSHServer.OnAuthPublicKey += SSHServer_OnAuthPublicKey;
            m_SSHServer.OnAuthKeyboard += SSHServer_OnAuthKeyboard;
            m_SSHServer.OnAuthKeyboardResponse += SSHServer_OnAuthKeyboardResponse;
            m_SSHServer.OnFurtherAuthNeeded += SSHServer_OnFurtherAuthNeeded;
            m_SSHServer.OnSend += SSHServer_OnSend;
            m_SSHServer.OnReceive += SSHServer_OnReceive;
            m_SSHServer.OnCloseConnection += SSHServer_OnCloseConnection;
            m_SSHServer.OnError += SSHServer_OnError;
            m_SSHServer.OnOpenSubsystem += SSHServer_OnOpenSubsystem;
            m_SSHServer.OnOpenShell += SSHServer_OnOpenShell;
            m_SSHServer.OnOpenCommand += m_SSHServer_OnOpenCommand;
            m_SSHServer.OnOpenServerForwarding += m_SSHServer_OnOpenServerForwarding;
            m_SSHServer.OnServerForwardingRequest += m_SSHServer_OnServerForwardingRequest;
            m_SSHServer.OnServerForwardingCancel += m_SSHServer_OnServerForwardingCancel;
            m_SSHServer.OnBeforeOpenClientForwarding += m_SSHServer_OnBeforeOpenClientForwarding;
            m_SSHServer.OnOpenClientForwarding += m_SSHServer_OnOpenClientForwarding;
        }

        private void m_SSHServer_OnOpenClientForwarding(object Sender, TElSSHTunnelConnection Connection, string DestHost, int DestPort, string SrcHost, int SrcPort) {
            Log.InfoFormat("Opening local forwarding connection. Destination: {0}:{1}, Src {2}:{3}",
                                DestHost, DestPort, SrcHost, SrcPort);
            var newConnection = new LocalPortForwardingConnection(DestHost, DestPort, Connection, m_lock);
            newConnection.Closed += localForwarding_Closed;
            lock (m_LocalForwardingSessions) {
                m_LocalForwardingSessions.Add(newConnection);
            }
            newConnection.Start();
        }

        private void localForwarding_Closed(object sender, EventArgs e) {
            lock (m_LocalForwardingSessions) {
                m_LocalForwardingSessions.Remove(sender);
            }
        }

        private void m_SSHServer_OnBeforeOpenClientForwarding(object Sender, string DestHost, int DestPort, string SrcHost, int SrcPort, ref bool Accept) {
            Log.InfoFormat("Local forwarding requested. Destination: {0}:{1}, Src {2}:{3}",
                                DestHost, DestPort, SrcHost, SrcPort);
            Accept = true;
        }

        private void m_SSHServer_OnServerForwardingRequest(object Sender, string Address, int Port, ref bool Accept, ref int RealPort) {
            Log.InfoFormat("Remote forwarding request received. Requested interface: {0}, port: {1}", Address, Port);
            // trying to open a listening port
            Socket listeningSocket = null;
            try {
                listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listeningSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
                listeningSocket.Listen(5);
                Log.Info("Listening socket has been set up successfully");
                Accept = true;
            }
            catch (Exception x) {
                Log.ErrorFormat("Failed to set up a listening socket: {0}", x.Message);
                Accept = false;
            }
            // creating a forwarding session object
            if (Accept) {
                var sess = new RemotePortForwardingSession(listeningSocket, m_SSHServer, Address, Port);
                lock (m_RemoteForwardingSessions) {
                    m_RemoteForwardingSessions.Add(sess);
                }
                sess.Start();
            }
        }

        private void m_SSHServer_OnServerForwardingCancel(object Sender, string Address, int Port) {
            Log.InfoFormat("Remote forwarding cancellation request received. Interface: {0}, port: {1}", Address, Port);
            // looking for opened server forwarding with specified Address and Port
            RemotePortForwardingSession sess = null;
            lock (m_RemoteForwardingSessions) {
                for (int i = 0; i < m_RemoteForwardingSessions.Count; i++) {
                    if ((((RemotePortForwardingSession) m_RemoteForwardingSessions[i]).Address == Address) ||
                        (((RemotePortForwardingSession) m_RemoteForwardingSessions[i]).Port == Port)) {
                        sess = (RemotePortForwardingSession) m_RemoteForwardingSessions[i];
                        break;
                    }
                }
                if (sess != null) {
                    m_RemoteForwardingSessions.Remove(sess);
                }
            }
            if (sess != null) {
                sess.Shutdown();
            }
        }

        private void m_SSHServer_OnOpenServerForwarding(object Sender, TElSSHTunnelConnection Connection) {
            Log.Info("Forwarded connection (channel) established, starting I/O loop");
            var conn = new RemotePortForwardingConnection(Connection, m_lock);
            conn.Closed += conn_Closed;
            conn.Start();
        }

        private void conn_Closed(object sender, EventArgs e) {
            Log.Info("Forwarded connection (channel) closed");
        }

        /// <summary>
        /// Session thread function
        /// </summary>
        private void ReadThread() {
            try {
                m_Error = false;
                Monitor.Enter(m_lock);
                try {
                    m_SSHServer.Open();
                }
                finally {
                    Monitor.Exit(m_lock);
                }
                while ((m_Socket != null) && (m_Socket.Connected) && (!m_Error)) {
                    if (m_Socket.Poll(1000000, SelectMode.SelectRead)) {
                        Monitor.Enter(m_lock);
                        try {
                            m_SSHServer.DataAvailable();
                        }
                        finally {
                            Monitor.Exit(m_lock);
                        }
                        Thread.Sleep(0);
                    }
                    else {
                        Thread.Sleep(50);
                    }
                }
            }
            catch (Exception ex) {
                if (!(ex is ThreadAbortException)) {
                    Log.ErrorFormat("ReadThread : {0}", ex);
                    CloseSession();
                }
                else {
                    if (this.SessionClosed != null) {
                        this.SessionClosed(this);
                        m_Thread = null;
                    }
                }
            }
        }

        private void RestartSocketSessionTimer() {
            m_SocketSessionTimer.Enabled = false;
            m_SocketSessionTimer.Interval = 5 * 60 * 1000;
            m_SocketSessionTimer.Enabled = true;
        }

        private void CleanUp() {
            CloseRemoteForwardingSessions();
            if (m_SSHServer.Active) {
                Monitor.Enter(m_lock);
                try {
                    m_SSHServer.Close(true);
                }
                finally {
                    Monitor.Exit(m_lock);
                }
            }

            DoSocketShutdown();
            m_SSHServer.OnSend -= SSHServer_OnSend;

            if (m_Thread != null) {
                m_SocketSessionTimer.Enabled = false;
                m_SocketSessionTimer.Elapsed -= OnSocketSessionTimeout;
                m_SocketSessionTimer.Close();
                m_Thread.Abort();
                //m_Thread.Join(1200);	
                //m_Thread = null;											
            }
        }

        private void CloseRemoteForwardingSessions() {
            while (m_RemoteForwardingSessions.Count > 0) {
                var sess = (RemotePortForwardingSession) m_RemoteForwardingSessions[0];
                try {
                    sess.Shutdown();
                }
                catch (Exception) {
                }
                m_RemoteForwardingSessions.RemoveAt(0);
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
            CloseSession();
        }

        private void DoSocketShutdown() {
            try {
                if (m_Socket != null) //&& (m_Socket.Connected)) 
                {
                    Log.Info("Closing the socket");
                    m_Socket.Shutdown(SocketShutdown.Both);
                    m_Socket.Close();
                    m_Socket = null;
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("DoSocketShutdown : {0}", ex);
            }
        }

        private void CloseSocket() {
            if (m_Socket != null) {
                try {
                    if (m_Socket.Connected) {
                        Log.Info("Closing the socket");
                        m_Socket.Shutdown(SocketShutdown.Both);
                        m_Socket.Close();
                        m_Socket = null;
                    }
                }
                catch (Exception ex) {
                    Log.ErrorFormat("CloseSocket() : {0}", ex);
                }
            }
        }

        private void OnSocketSessionTimeout(object sender, ElapsedEventArgs e) {
            m_SocketSessionTimer.Stop();
            Log.Info("Socket closed by timeout");
            CloseSession();
            //m_Thread.Abort();
        }

        #endregion

        #region Class members

        private readonly string m_Host = "";
        private readonly TElSSHMemoryKeyStorage m_HostKeys = new TElSSHMemoryKeyStorage();
        private readonly ArrayList m_LocalForwardingSessions = new ArrayList();
        private readonly ArrayList m_RemoteForwardingSessions = new ArrayList();
        private readonly TElSSHServer m_SSHServer = new TElSSHServer();
        private readonly Timer m_SocketSessionTimer;
        private readonly DateTime m_StartTime;
        private readonly Hashtable m_authInfo = new Hashtable();
        private readonly object m_lock = new object();
        private string m_ClientSoftware = "";
        private bool m_Error;
        private bool m_Opened;
        private Socket m_Socket;
        private string m_Status = "";
        private Thread m_Thread;
        private string m_Username = "";
        private object m_synch;

        #endregion
    }

    internal class RemotePortForwardingConnection {
        private readonly object m_Lock;
        private readonly object m_RecvBufferLock = new object();
        private readonly Socket m_Socket;
        private TElSSHTunnelConnection m_Connection;
        private Thread m_IOThread;
        private byte[] m_RecvBuffer;
        private byte[] m_SendBuffer;
        private bool m_Terminated;

        public RemotePortForwardingConnection(TElSSHTunnelConnection connection, object serverLock) {
            m_Connection = connection;
            m_Connection.OnData += m_Connection_OnData;
            m_Connection.OnClose += m_Connection_OnClose;
            m_Socket = (Socket) connection.Data;
            m_Lock = serverLock;
        }

        private void m_Connection_OnClose(object Sender, TSSHCloseType CloseType) {
            m_Connection = null;
            m_Terminated = true;
        }

        private void m_Connection_OnData(object Sender, byte[] Buffer) {
            lock (m_RecvBufferLock) {
                byte[] newBuf = null;
                if (m_RecvBuffer == null) {
                    newBuf = Buffer;
                }
                else {
                    newBuf = new byte[m_RecvBuffer.Length + Buffer.Length];
                    Array.Copy(m_RecvBuffer, 0, newBuf, 0, m_RecvBuffer.Length);
                    Array.Copy(Buffer, 0, newBuf, m_RecvBuffer.Length, Buffer.Length);
                }
                m_RecvBuffer = newBuf;
            }
        }

        public void Start() {
            m_IOThread = new Thread(IOLoop);
            m_IOThread.Start();
        }

        public void Shutdown() {
            m_Terminated = true;
        }

        public event EventHandler Closed;

        private void IOLoop() {
            var sockBuf = new byte[65536];
            try {
                while (!m_Terminated) {
                    try {
                        // sending cached data to socket
                        lock (m_RecvBufferLock) {
                            if ((m_RecvBuffer != null) && (m_RecvBuffer.Length > 0)) {
                                int idx = 0;
                                while (idx < m_RecvBuffer.Length) {
                                    int sent = m_Socket.Send(m_RecvBuffer, idx, m_RecvBuffer.Length - idx, SocketFlags.None);
                                    idx += sent;
                                }
                                m_RecvBuffer = null;
                            }
                        }
                        // reading data from socket
                        if (m_Socket.Poll(100000, SelectMode.SelectRead)) {
                            int recvd = m_Socket.Receive(sockBuf, 0, sockBuf.Length, SocketFlags.None);
                            if (recvd <= 0) {
                                // socket closed gracefully
                                m_Terminated = true;
                            }
                            if (m_SendBuffer == null) {
                                m_SendBuffer = new byte[recvd];
                                Array.Copy(sockBuf, 0, m_SendBuffer, 0, recvd);
                            }
                            else {
                                var newBuf = new byte[m_SendBuffer.Length + recvd];
                                Array.Copy(m_SendBuffer, 0, newBuf, 0, m_SendBuffer.Length);
                                Array.Copy(sockBuf, 0, newBuf, m_SendBuffer.Length, recvd);
                                m_SendBuffer = newBuf;
                            }
                        }
                        // sending cached data to connection
                        if ((m_Connection != null) && (m_Connection.CanSend())) {
                            Monitor.Enter(m_Lock);
                            try {
                                m_Connection.SendData(m_SendBuffer);
                            }
                            finally {
                                Monitor.Exit(m_Lock);
                            }
                            m_SendBuffer = null;
                        }
                        // sleeping
                        Thread.Sleep(0);
                    }
                    catch (Exception ex) {
                        m_Terminated = true;
                    }
                }
            }
            finally {
                // closing the socket if it hasn't been closed yet
                try {
                    if (m_Socket.Connected) {
                        m_Socket.Close();
                    }
                }
                catch (Exception) {
                }
                // closing the connection if it hasn't been shut down yet
                try {
                    if (m_Connection != null) {
                        m_Connection.Close(false);
                    }
                }
                catch (Exception) {
                }
                m_Connection = null;
            }
            if (Closed != null) {
                Closed(this, null);
            }
        }
    }

    internal class RemotePortForwardingSession {
        private readonly Thread m_AccThread;
        private readonly string m_Address = "";
        private readonly Socket m_ListeningSocket;
        private readonly int m_Port;
        private readonly TElSSHServer m_Server;
        private bool m_Terminated;

        public RemotePortForwardingSession(Socket listeningSocket, TElSSHServer server, string address, int port) {
            m_ListeningSocket = listeningSocket;
            m_AccThread = new Thread(AccLoop);
            m_Address = address;
            m_Port = port;
            m_Server = server;
        }

        public string Address {
            get {
                return m_Address;
            }
        }

        public int Port {
            get {
                return m_Port;
            }
        }

        public void Start() {
            m_AccThread.Start();
        }

        public void Shutdown() {
            m_Terminated = true;
        }

        protected void AccLoop() {
            while (!m_Terminated) {
                try {
                    Socket accSocket = null;
                    if (m_ListeningSocket.Poll(500000, SelectMode.SelectRead)) {
                        accSocket = m_ListeningSocket.Accept();
                        if (accSocket != null) {
                            m_Server.OpenServerForwarding("", ((IPEndPoint) m_ListeningSocket.LocalEndPoint).Port,
                                ((IPEndPoint) accSocket.RemoteEndPoint).Address.ToString(), ((IPEndPoint) accSocket.RemoteEndPoint).Port,
                                accSocket);
                        }
                    }
                }
                catch (Exception) {
                    // ignoring all exceptions for the sake of simplicity
                }
                Thread.Sleep(0);
            }
        }
    }

    internal class LocalPortForwardingConnection {
        private static readonly ILog Log = LogManager.GetLogger("SFTPServiceLog");
        private readonly string m_DestAddress;
        private readonly int m_DestPort;
        private readonly object m_Lock;
        private readonly object m_RecvBufferLock = new object();
        private readonly Thread m_SendRecvThread;
        private TElSSHTunnelConnection m_Connection;
        private byte[] m_RecvBuffer;
        private byte[] m_SendBuffer;
        private Socket m_Socket;
        private bool m_Terminated;

        public LocalPortForwardingConnection(string DestAddress, int DestPort, TElSSHTunnelConnection connection, object serverLock) {
            m_DestAddress = DestAddress;
            m_DestPort = DestPort;
            m_Connection = connection;
            m_Connection.OnData += m_Connection_OnData;
            m_Connection.OnClose += m_Connection_OnClose;
            m_Lock = serverLock;

            m_SendRecvThread = new Thread(SendRecvLoop);
        }

        public event EventHandler Closed;

        private void m_Connection_OnClose(object Sender, TSSHCloseType CloseType) {
            m_Connection = null;
            m_Terminated = true;
        }

        private void m_Connection_OnData(object Sender, byte[] Buffer) {
            lock (m_RecvBufferLock) {
                byte[] newBuf = null;
                if (m_RecvBuffer == null) {
                    newBuf = Buffer;
                }
                else {
                    newBuf = new byte[m_RecvBuffer.Length + Buffer.Length];
                    Array.Copy(m_RecvBuffer, 0, newBuf, 0, m_RecvBuffer.Length);
                    Array.Copy(Buffer, 0, newBuf, m_RecvBuffer.Length, Buffer.Length);
                }
                m_RecvBuffer = newBuf;
            }
        }

        protected void SendRecvLoop() {
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try {
                m_Socket.Connect(m_DestAddress, m_DestPort);
            }
            catch (Exception e) {
                Log.ErrorFormat("Cannot open local port forwarding connection : {0}", e.Message);
                m_Connection.Close(false);
                return;
            }

            int recvSize = 0, sendSize = 0;
            Log.InfoFormat("Forwarded connection to {0}:{1} opened.", m_DestAddress, m_DestPort);

            var sockBuf = new byte[65536];
            try {
                while (!m_Terminated) {
                    try {
                        // sending cached data to socket
                        lock (m_RecvBufferLock) {
                            if ((m_RecvBuffer != null) && (m_RecvBuffer.Length > 0)) {
                                int idx = 0;
                                while (idx < m_RecvBuffer.Length) {
                                    int sent = m_Socket.Send(m_RecvBuffer, idx, m_RecvBuffer.Length - idx, SocketFlags.None);
                                    idx += sent;
                                    sendSize += sent;
                                }
                                m_RecvBuffer = null;
                            }
                        }
                        // reading data from socket
                        if (m_Socket.Poll(100000, SelectMode.SelectRead)) {
                            int recvd = m_Socket.Receive(sockBuf, 0, sockBuf.Length, SocketFlags.None);
                            if (recvd <= 0) {
                                // socket closed gracefully
                                m_Terminated = true;
                            }
                            if (m_SendBuffer == null) {
                                m_SendBuffer = new byte[recvd];
                                Array.Copy(sockBuf, 0, m_SendBuffer, 0, recvd);
                            }
                            else {
                                var newBuf = new byte[m_SendBuffer.Length + recvd];
                                Array.Copy(m_SendBuffer, 0, newBuf, 0, m_SendBuffer.Length);
                                Array.Copy(sockBuf, 0, newBuf, m_SendBuffer.Length, recvd);
                                m_SendBuffer = newBuf;
                            }

                            if (recvd > 0) {
                                recvSize += recvd;
                            }
                        }
                        // sending cached data to connection
                        if ((m_Connection != null) && (m_Connection.CanSend())) {
                            Monitor.Enter(m_Lock);
                            try {
                                m_Connection.SendData(m_SendBuffer);
                            }
                            finally {
                                Monitor.Exit(m_Lock);
                            }
                            m_SendBuffer = null;
                        }
                        // sleeping
                        Thread.Sleep(0);
                    }
                    catch (Exception ex) {
                        m_Terminated = true;
                    }
                }

                Log.InfoFormat("Local forwarded connection closed. Send data : {0},  received : {1}", sendSize, recvSize);
            }
            finally {
                // closing the socket if it hasn't been closed yet
                try {
                    if (m_Socket.Connected) {
                        m_Socket.Close();
                    }
                }
                catch (Exception) {
                }
                // closing the connection if it hasn't been shut down yet
                try {
                    if (m_Connection != null) {
                        m_Connection.Close(false);
                    }
                }
                catch (Exception) {
                }
                m_Connection = null;
            }
            if (Closed != null) {
                Closed(this, null);
            }
        }

        public void Start() {
            m_SendRecvThread.Start();
        }

        public void Shutdown() {
            m_Terminated = true;
        }
    }
}