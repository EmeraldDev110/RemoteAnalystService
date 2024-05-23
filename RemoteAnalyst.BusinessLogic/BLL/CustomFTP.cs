using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RemoteAnalyst.BusinessLogic.BLL {
    public class CustomFTP {

        #region  Class Variables
        private bool bConnected = false;			// indicate connection to host
        private bool bVerbrose = true;				// send feedback
        private int nPortFTP = 21;					// default FTP port
        //private TcpClient m_tcpClient = null;		// command channel
        private StreamReader m_comRead = null;		// text reader
        private StreamWriter m_comWrite = null; 	// text writer
        private bool bLoggedIn = false;
        #endregion

        // Delegate to supply all commands sent and replies to user
        public delegate void CmdEventHandler(string sCmd);

        // receive all commands
        public event CmdEventHandler cmdEvent;

        public bool Connect(string sHost) {
            if (bConnected) return false;

            // send all commands and replies to CmdEvent handlers
            bVerbrose = true;
            try {
                // create a TcpClient control for Transport connection
                // m_tcpClient = new TcpClient(sHost, nPortFTP); // use default port 21
                // m_tcpClient.Connect(ip, nPortFTP);

                TcpClient m_tcpClient = new TcpClient(sHost, nPortFTP);

                m_tcpClient.ReceiveTimeout = 2000; // wait 2 seconds before aborting
                m_tcpClient.SendTimeout = 2000; // wait 2 seconds before aborting
                m_comRead = new StreamReader(m_tcpClient.GetStream());  // text reader
                m_comWrite = new StreamWriter(m_tcpClient.GetStream()); // text writer
                bConnected = true;
            }
            catch {
                return false;
            }

            string sReply = ReadReply(); // 220 reply(multiline) if successful
            if (sReply[0] != '2') return false;

            return true;
        }

        // Closes TcpClient and cleans up
        internal void Cleanup() {
            if (!bConnected) return;

            m_comRead.Close();		// close text reader
            m_comWrite.Close();		// close text writer
            //m_tcpClient.Close();	// close command channel
            bConnected = false;
            bLoggedIn = false;
        }

        // Log in user to a connected remote FTP server
        public bool Login(string sUsername, string sPassword) {
            if (!bConnected) return false;

            // the server must reply with 331
            string sReply = SendCommand("USER " + sUsername);
            if (sReply[0] != '3') return false;

            // the server must reply with 230, which is a successful login
            sReply = SendCommand("PASS " + sPassword);
            if (sReply[0] != '2') return false;

            bLoggedIn = true;
            return true;
        }

        // Sends a command to remote host and waits for reply
        // <param name="sCmd">command to server</param>
        internal string SendCommand(string sCmd) {
            if (!bConnected) return "000";

            WriteLog(sCmd);
            try {
                m_comWrite.WriteLine(sCmd);
                m_comWrite.Flush();	// send the data
            }
            catch {
                Cleanup(); // disconnect and cleanup
                //throw new FtpClientException("Write Failed: Closing connection", ex);
            }
            return ReadReply();	// wait for reply from Host
        }

        internal bool SetCurrentDirectory(string sDirectory) {
            if (!bLoggedIn) return false;

            string sReply = SendCommand("GUARDIAN");
            sReply = SendCommand("CWD " + sDirectory);

            // server must reply with 250, else the directory does not exist
            if (sReply[0] != '2') return false;

            return true;
        }

        #region LOCAL METHODS
        // Read entire (multi-line) replies from server
        private string ReadReply() {
            string s = "";
            try {
                s = m_comRead.ReadLine();              // get first line of reply
                string sEnd = s.Substring(0, 3) + " "; // save reply number plus space
                while (s.Substring(0, 4) != sEnd) {
                    WriteLog(s);				       // log line
                    s = m_comRead.ReadLine();	       // read multi-line replies
                }
                WriteLog(s);					       // log last line
            }
            catch (Exception ex) {
                Cleanup(); // disconnect and cleanup
                throw new Exception("Read Error: Closing connection", ex);
            }

            if (s.Length < 4) throw new FtpClientException("Invalid Reply From Server");
            if (s[0] == '2') WriteLog(""); // add blank line - end of sequence

            return s;	// return last line read
        }

        // create socket for data transfer
        // returns null on error
        private Socket CreateDataSocket() {
            // returns: "227 Entering Passive Mode (204,127,12,38,13,193)."
            string sReply = SendCommand("PASV"); // request a data connection
            if (sReply[0] != '2') throw new FtpClientException(sReply);

            // extract IP Address and Port number
            int n1 = sReply.IndexOf("(");
            int n2 = sReply.IndexOf(")");
            string[] sa = sReply.Substring(n1 + 1, n2 - n1 - 1).Split(',');
            string sIPAddress = sa[0] + "." + sa[1] + "." + sa[2] + "." + sa[3];
            int nPort = int.Parse(sa[4]) * 256 + int.Parse(sa[5]);

            Socket socket = null;	// data transfer socket
            try {	// connect to host data channel
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(new IPEndPoint(IPAddress.Parse(sIPAddress), nPort));
            }
            catch (Exception ex) {
                if (socket != null) socket.Close();
                WriteLog(ex.Message);
                throw new FtpClientException("Error creating data connection", ex);
            }
            return socket;
        }

        // supply commands and replies to "cmdEvent" subscribers
        private void WriteLog(string sLog) {
            if (cmdEvent != null && bVerbrose) cmdEvent(sLog);
        }
        #endregion

        // FTP exception class
        internal class FtpClientException : Exception {
            // An instance of FtpClientException
            // <param name="sMsg">Explains what happend</param>
            public FtpClientException(string sMsg) : base(sMsg) { }

            // An instance of FtpClientException
            // <param name="sMsg">Explains what happend</param>
            // <param name="ex">InnerException</param>
            public FtpClientException(string sMsg, Exception ex) : base(sMsg, ex) { }
        } // end-class
    }
}
