using log4net;
using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ThreadState = System.Threading.ThreadState;

namespace RemoteAnalyst.SFTP.BLL {
    internal class ServerListener : IDisposable {
        private static readonly ILog Log = LogManager.GetLogger("SFTPServiceLog");
        public ServerListener() {
            m_synch = new object();
            m_arrSessions = new ArrayList();
            m_Event = new ManualResetEvent(false);
            m_Thread = new Thread(Listen);
            m_Thread.Name = "SSH_Server_Listener_Thread";
        }

        #region Public events

        public delegate void SessionClosedHandler(SSHSession sender);

        public delegate void SessionInfoChangedHandler(SSHSession sender);

        public delegate void SessionStartedHandler(SSHSession sender);

        public event SessionStartedHandler SessionStarted;

        public event SessionClosedHandler SessionClosed;

        public event SessionInfoChangedHandler SessionInfoChanged;

        #endregion

        #region IDisposable Members

        public void Dispose() {
            if ((m_Thread != null) && (m_Thread.ThreadState != ThreadState.Unstarted)) {
                m_Event.Reset();
                Stop();

                //kill thread
                m_Thread.Abort();
                m_Thread.Join(1000);
                m_Event.Close();

                //kill sockets (sessions)
                lock (m_synch) {
                    for (int i = 0, N = m_arrSessions.Count; i < N; ++i) {
                        //stop all sessions
                        var session = (SSHSession) m_arrSessions[i];
                        Debug.Assert(session != null);
                        session.SessionClosed -= OnSessionClosed;
                        session.CloseSession();
                        session.Dispose();
                    }
                    m_arrSessions.Clear();
                }
                m_Thread = null;
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Starts listening for incoming connections
        /// </summary>
        /// <returns>true, if the listening socket was successfully allocated</returns>
        public bool Start() {
            try {
                Log.Info("Starting SSH server listener...");
                m_SocketListener = new TcpListener(IPAddress.Parse(Globals.Settings.ServerHost),
                    Globals.Settings.ServerPort);
                m_SocketListener.Start();
                m_Thread.Start();
                m_Event.Set();
                /*if (Globals.main != null) {
                    lock (Globals.main) {
                        Globals.main.tbTop.Buttons[0].Enabled = false;
                        Globals.main.tbTop.Buttons[1].Enabled = true;
                    }
                }*/
                Globals.ServerStarted = true;
                Log.Info("SSH server listener started.");
                return true;
            }
            catch (Exception exc) {
                Log.ErrorFormat("ServerListener.Start : {0}", exc.Message);
                return false;
            }
        }

        /// <summary>
        /// Stops listening for incoming connections
        /// </summary>
        public void Stop() {
            try {
                if (m_SocketListener != null) {
                    //stop listener
                    Log.InfoFormat("Stopping socket listener");
                    m_SocketListener.Stop();
                    /*if (Globals.main != null) {
                        lock (Globals.main) {
                            Globals.main.tbTop.Buttons[0].Enabled = true;
                            Globals.main.tbTop.Buttons[1].Enabled = false;
                        }
                    }*/
                    Globals.ServerStarted = false;
                    Log.InfoFormat("Socket listener stopped");
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("Exception while stopping socket listener : {0}", ex);
            }
        }

        #endregion

        #region Private methods

        private Socket AcceptSocket() {
            Socket socket = null;
            try {
                if (m_SocketListener.Pending()) {
                    socket = m_SocketListener.AcceptSocket();
                }
            }
            catch (Exception ex) {
                Log.ErrorFormat("AcceptSocket : {0}", ex);
            }
            return socket;
        }

        private void Listen() {
            while (m_Event.WaitOne()) {
                Socket socket = AcceptSocket();
                if (socket != null) {
                    Log.Info("ServerListener.Listen : New connection available");
                    lock (m_synch) {
                        if (m_arrSessions.Count < m_MaxSocketSessions) {
                            //create new session and start it in a new thread							
                            var session = new SSHSession(socket, ++m_ClientCount);
                            session.SessionClosed += OnSessionClosed;
                            session.SessionInfoChanged += OnSessionInfoChanged;
                            m_arrSessions.Add(session);
                            Log.InfoFormat("Connection accepted. Active connections {0} from {1}",
                                 m_arrSessions.Count, m_MaxSocketSessions);
                            session.Start();
                            this.SessionStarted(session);
                        }
                        else {
                            Log.Info("New connection rejected");
                            try {
                                socket.Shutdown(SocketShutdown.Both);
                                socket.Close();
                            }
                            catch (Exception) {
                                ;
                            }
                        }
                    }
                }
                else {
                    Thread.Sleep(500);
                }
            }
        }

        #endregion

        #region Event handlers

        private void OnSessionClosed(SSHSession sender) {
            lock (m_synch) {
                m_arrSessions.Remove(sender);
                Log.Info("ServerListener.OnSessionClosed()");
            }
            if (this.SessionClosed != null) {
                this.SessionClosed(sender);
            }
            sender.SessionClosed -= OnSessionClosed;
            sender.SessionInfoChanged -= OnSessionInfoChanged;
        }

        private void OnSessionInfoChanged(SSHSession sender) {
            this.SessionInfoChanged(sender);
        }

        #endregion

        #region Class members

        private readonly ManualResetEvent m_Event;
        private readonly ArrayList m_arrSessions;
        private readonly object m_synch;
        private int m_ClientCount;
        private int m_MaxSocketSessions = 20;
        private TcpListener m_SocketListener;
        private Thread m_Thread;

        #endregion
    }
}