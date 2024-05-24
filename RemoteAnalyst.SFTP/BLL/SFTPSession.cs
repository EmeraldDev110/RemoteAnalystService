using System;
using System.Collections;
using System.IO;
using log4net;
using SBSftpCommon;
using SBSftpHandler;
using SBSSHCommon;
using SBSSHHandlers;
using SBStringList;
using __Global = SBSftpCommon.__Global;

namespace RemoteAnalyst.SFTP.BLL {
    public class SFTPSession {
        private static readonly ILog Log = LogManager.GetLogger("SFTPServiceLog");
        public SFTPSession(TElSSHTunnelConnection conn) {
            Type tp = typeof (TElSFTPSSHSubsystemHandler);
            m_thread = new TElSSHSubsystemThread(new TElSFTPSSHSubsystemHandler(conn, true), conn, true);
            var handler = (TElSFTPSSHSubsystemHandler) m_thread.Handler;
            handler.Server.OnClose += Server_OnClose;
            handler.Server.OnCloseHandle += Server_OnCloseHandle;
            handler.Server.OnCreateDirectory += Server_OnCreateDirectory;
            handler.Server.OnError += Server_OnError;
            handler.Server.OnFindClose += Server_OnFindClose;
            handler.Server.OnFindFirst += Server_OnFindFirst;
            handler.Server.OnFindNext += Server_OnFindNext;
            handler.Server.OnOpen += Server_OnOpen;
            handler.Server.OnOpenFile += Server_OnOpenFile;
            handler.Server.OnReadFile += Server_OnReadFile;
            handler.Server.OnRemove += Server_OnRemove;
            handler.Server.OnRenameFile += Server_OnRenameFile;
            handler.Server.OnRequestAbsolutePath += Server_OnRequestAbsolutePath;
            handler.Server.OnRequestAttributes += Server_OnRequestAttributes;
            handler.Server.OnRequestAttributes2 += Server_OnRequestAttributes2;
            handler.Server.OnWriteFile += Server_OnWriteFile;
            handler.Server.Versions = __Global.sbSFTP3 | __Global.sbSFTP4 | __Global.sbSFTP5 | __Global.sbSFTP6;
            Log.Info("SFTP server started");
            m_thread.Resume();
        }

        #region SFTP server event handlers

        /// <summary>
        /// Is fired when the SFTP connection is gracefully closed
        /// </summary>
        /// <param name="Sender">ElSFTPServer object</param>
        private void Server_OnClose(object Sender) {
            Log.Info("SFTP connection closed");
        }

        /// <summary>
        /// Is fired when 'Close handle' request is received from client
        /// </summary>
        /// <param name="Sender">ElSFTPServer object</param>
        /// <param name="Data">User-defined Data associated with file operation</param>
        /// <param name="ErrorCode">Operation result code (please see the documentation)</param>
        /// <param name="Comment">Operation result comment (please see the documentation)</param>
        private void Server_OnCloseHandle(object Sender, object Data, ref int ErrorCode, ref string Comment) {
            try {
                ((Stream) Data).Close();
                ErrorCode = 0;
            }
            catch (Exception ex) {
                ErrorCode = __Global.SSH_ERROR_FAILURE;
                Comment = ex.Message;
            }
        }

        /// <summary>
        /// Is fired when 'Create directory' request is received from client
        /// </summary>
        /// <param name="Sender">ElSFTPServer object</param>
        /// <param name="Path">Path to create</param>
        /// <param name="Attributes">Desired attributes</param>
        /// <param name="ErrorCode">Operation result code (please see the documentation)</param>
        /// <param name="Comment">Operation result comment (please see the documentation)</param>
        private void Server_OnCreateDirectory(object Sender, string Path, TElSftpFileAttributes Attributes, ref int ErrorCode, ref string Comment) {
            string realpath = CanonicalizePath(Path, true);
            try {
                Directory.CreateDirectory(realpath);
                ErrorCode = 0;
            }
            catch (Exception ex) {
                ErrorCode = __Global.SSH_ERROR_PERMISSION_DENIED;
                Comment = ex.Message;
            }
        }

        /// <summary>
        /// Is fired when some error occurs during SFTP communication
        /// </summary>
        /// <param name="Sender">ElSFTPServer object</param>
        /// <param name="ErrorCode">Error code</param>
        /// <param name="Comment">Error comment</param>
        private void Server_OnError(object Sender, int ErrorCode, string Comment) {
            Log.ErrorFormat("SFTP client error: {0}", ErrorCode);
        }

        /// <summary>
        /// Is fired when the directory browse operation is closed
        /// </summary>
        /// <param name="Sender">ElSFTPServer object</param>
        /// <param name="SearchRec">User-specific data associated with browse session</param>
        /// <param name="ErrorCode">Operation result code (please see the documentation)</param>
        /// <param name="Comment">Operation result comment (please see the documentation)</param>
        private void Server_OnFindClose(object Sender, object SearchRec, ref int ErrorCode, ref string Comment) {
            ErrorCode = 0;
        }

        /// <summary>
        /// Is fired when the directory browse operation is initiated by client
        /// </summary>
        /// <param name="Sender">ElSFTPServer object</param>
        /// <param name="Path">Directory to be browsed</param>
        /// <param name="Data">User-specific data associated with browse session</param>
        /// <param name="Info">Fill this object with directory item (either file or directory) properties</param>
        /// <param name="ErrorCode">Operation result code (please see the documentation)</param>
        /// <param name="Comment">Operation result comment (please see the documentation)</param>
        private void Server_OnFindFirst(object Sender, string Path, ref object Data, TElSftpFileInfo Info, ref int ErrorCode, ref string Comment) {
            string localpath = CanonicalizePath(Path, true);
            try {
                string[] lst = Directory.GetDirectories(localpath);
                m_dirList = new string[lst.Length + 2];
                m_dirList[0] = ".";
                m_dirList[1] = "..";
                for (int i = 0; i < lst.Length; i++) {
                    m_dirList[i + 2] = lst[i];
                }
                m_fileList = Directory.GetFiles(localpath);
                m_fileIndex = 0;
                m_dirIndex = 0;
                FillNextFileInfo(Info, ref ErrorCode);
                Data = null;

                ErrorCode = 0;
                Comment = "";
            }
            catch (Exception ex) {
                ErrorCode = __Global.SSH_ERROR_NO_SUCH_FILE;
                Comment = ex.Message;
            }
        }

        /// <summary>
        /// Is consequently fired until the whole directory is browsed
        /// </summary>
        /// <param name="Sender">ElSFTPServer object</param>
        /// <param name="Data">User-specific data associated with browse session</param>
        /// <param name="Info">Fill this object with directory item (either file or directory) properties</param>
        /// <param name="ErrorCode">Operation result code (please see the documentation)</param>
        /// <param name="Comment">Operation result comment (please see the documentation)</param>
        private void Server_OnFindNext(object Sender, object Data, TElSftpFileInfo Info, ref int ErrorCode, ref string Comment) {
            FillNextFileInfo(Info, ref ErrorCode);
        }

        /// <summary>
        /// Is fired when SFTP session is established
        /// </summary>
        /// <param name="Sender">ElSFTPServer object</param>
        private void Server_OnOpen(object Sender) {
            Log.Info("SFTP connection established");
        }

        /// <summary>
        /// Is fired when file open operation is requested by client
        /// </summary>
        /// <param name="Sender">ElSFTPServer object</param>
        /// <param name="Path">File to open</param>
        /// <param name="Modes">File open modes</param>
        /// <param name="Access">Specifies file blocking parameters</param>
        /// <param name="DesiredAccess">Desired file access</param>
        /// <param name="Attributes">Initial file attributes</param>
        /// <param name="Data">User-specific data associated with file operation</param>
        /// <param name="ErrorCode">Operation result code (please see the documentation)</param>
        /// <param name="Comment">Operation result comment (please see the documentation)</param>
        private void Server_OnOpenFile(object Sender, string Path, int Modes, int Access, uint DesiredAccess, TElSftpFileAttributes Attributes, ref object Data, ref int ErrorCode, ref string Comment) {
            string realpath = CanonicalizePath(Path, true);
            FileMode mode;
            FileAccess access;
            if (((Modes & __Global.fmCreate) == __Global.fmCreate) &&
                ((Modes & __Global.fmTruncate) == __Global.fmTruncate)) {
                mode = FileMode.Create;
            }
            else if (((Modes & __Global.fmCreate) == __Global.fmCreate) &&
                     ((Modes & __Global.fmExcl) == __Global.fmExcl)) {
                if (File.Exists(realpath)) {
                    Comment = "Cannot open file";
                    ErrorCode = __Global.SSH_ERROR_FILE_ALREADY_EXISTS;
                    return;
                }
                mode = FileMode.Create;
            }
            else {
                if ((Modes & __Global.fmCreate) == __Global.fmCreate) {
                    mode = FileMode.Create;
                }
                else {
                    mode = FileMode.Open;
                }
            }
            if (((Modes & __Global.fmRead) == __Global.fmRead) &&
                ((Modes & __Global.fmWrite) == __Global.fmWrite)) {
                access = FileAccess.ReadWrite;
            }
            else if ((Modes & __Global.fmWrite) == __Global.fmWrite) {
                access = FileAccess.Write;
            }
            else {
                access = FileAccess.Read;
            }
            try {
                Data = new FileStream(realpath, mode, access);
                if ((Modes & __Global.fmAppend) == __Global.fmAppend) {
                    ((FileStream) Data).Position = ((FileStream) Data).Length;
                }
                ErrorCode = 0;
            }
            catch (Exception ex) {
                ErrorCode = __Global.SSH_ERROR_FAILURE;
                Comment = ex.Message;
            }
        }

        /// <summary>
        /// Is fired when a 'Read file' command is received from user
        /// </summary>
        /// <param name="Sender">ElSFTPServer object</param>
        /// <param name="Data">User-specific data associated with file operation</param>
        /// <param name="Offset">File start offset</param>
        /// <param name="Buffer">Place where to put file data</param>
        /// <param name="BufferOffset">Buffer offset</param>
        /// <param name="Count">Maximal count of bytes to read from file</param>
        /// <param name="Read">Actual bytes read</param>
        /// <param name="ErrorCode">Operation result code (please see the documentation)</param>
        /// <param name="Comment">Operation result comment (please see the documentation)</param>
        private void Server_OnReadFile(object Sender, object Data, long Offset, ref byte[] Buffer, int BufferOffset, int Count, ref int Read, ref int ErrorCode, ref string Comment) {
            try {
                var strm = (Stream) Data;
                strm.Position = Offset;
                Read = strm.Read(Buffer, BufferOffset, Count);
                if (Read > 0) {
                    ErrorCode = 0;
                }
                else {
                    ErrorCode = __Global.SSH_ERROR_EOF;
                }
            }
            catch (Exception ex) {
                ErrorCode = __Global.SSH_ERROR_FAILURE;
                Comment = ex.Message;
            }
        }

        /// <summary>
        /// Is fired, when 'Remove' request is received from client
        /// </summary>
        /// <param name="Sender">ElSFTPServer object</param>
        /// <param name="Path">Path to file or directory to remove</param>
        /// <param name="ErrorCode">Operation result code (please see the documentation)</param>
        /// <param name="Comment">Operation result comment (please see the documentation)</param>
        private void Server_OnRemove(object Sender, string Path, ref int ErrorCode, ref string Comment) {
            bool success = false;
            string localpath = CanonicalizePath(Path, true);
            try {
                if (Directory.Exists(localpath)) {
                    Directory.Delete(localpath);
                    success = true;
                }
                if ((!success) && (File.Exists(localpath))) {
                    File.Delete(localpath);
                    success = true;
                }
                if (!success) {
                    throw new Exception("File does not exist");
                }
                ErrorCode = 0;
            }
            catch (Exception ex) {
                ErrorCode = __Global.SSH_ERROR_FAILURE;
                Comment = ex.Message;
            }
        }

        /// <summary>
        /// Is fired when the 'Rename file' is requested from client
        /// </summary>
        /// <param name="Sender">ElSFTPServer object</param>
        /// <param name="OldPath">Path to file/directory to rename</param>
        /// <param name="NewPath">New name</param>
        /// <param name="Flags">Rename flags</param>
        /// <param name="ErrorCode">Operation result code (please see the documentation)</param>
        /// <param name="Comment">Operation result comment (please see the documentation)</param>
        private void Server_OnRenameFile(object Sender, string OldPath, string NewPath, int Flags, ref int ErrorCode, ref string Comment) {
            bool success = false;
            string localold = CanonicalizePath(OldPath, true);
            string localnew = CanonicalizePath(NewPath, true);
            try {
                if (File.Exists(localold)) {
                    File.Move(localold, localnew);
                    success = true;
                }
                if ((!success) && (Directory.Exists(localold))) {
                    Directory.Move(localold, localnew);
                    success = true;
                }
                if (!success) {
                    throw new Exception("File does not exist");
                }
                ErrorCode = 0;
            }
            catch (Exception ex) {
                ErrorCode = __Global.SSH_ERROR_FAILURE;
                Comment = ex.Message;
            }
        }

        /// <summary>
        /// Is fired when a client requests to canonicalize relative path to absolute
        /// </summary>
        /// <param name="Sender">ElSFTPServer object</param>
        /// <param name="Path">Relative path</param>
        /// <param name="AbsolutePath">Converted absolute path</param>
        /// <param name="Control">Path canonicalization parameters</param>
        /// <param name="ComposePath"></param>
        /// <param name="ErrorCode">Operation result code (please see the documentation)</param>
        /// <param name="Comment">Operation result comment (please see the documentation)</param>
        private void Server_OnRequestAbsolutePath(object Sender, string Path, ref string AbsolutePath, TSBSftpRealpathControl Control, TElStringList ComposePath, ref int ErrorCode, ref string Comment) {
            AbsolutePath = CanonicalizePath(Path, false);
            ErrorCode = 0;
        }

        private void GetAttributes(string Path, TElSftpFileAttributes Attributes) {
            if (Directory.Exists(Path)) {
                var info = new DirectoryInfo(Path);
                Attributes.Size = 0;
                Attributes.Directory = true;
                Attributes.FileType = TSBSftpFileType.ftDirectory;
                Attributes.MTime = info.LastWriteTime;
                Attributes.ATime = info.LastAccessTime;
                Attributes.CTime = info.CreationTime;

                Attributes.UserWrite = true;
                Attributes.GroupWrite = true;
                Attributes.OtherWrite = true;
            }
            else if (File.Exists(Path)) {
                var info = new FileInfo(Path);
                Attributes.Size = info.Length;
                Attributes.Directory = false;
                Attributes.FileType = TSBSftpFileType.ftFile;
                Attributes.MTime = info.LastWriteTime;
                Attributes.ATime = info.LastAccessTime;
                Attributes.CTime = info.CreationTime;
            }
            else {
                throw new Exception("File does not exist");
            }

            Attributes.UserRead = true;
            Attributes.GroupRead = true;
            Attributes.OtherRead = true;

            Attributes.IncludedAttributes = __Global.saPermissions | __Global.saSize |
                                            __Global.saMTime | __Global.saATime | __Global.saCTime;
        }

        /// <summary>
        /// Is fired when file attributes are requested by client
        /// </summary>
        /// <param name="Sender">ElSFTPServer object</param>
        /// <param name="Path">Path to file/directory</param>
        /// <param name="FollowSymLinks">True, if symbolic links should be processed</param>
        /// <param name="Attributes">Object to put file attributes</param>
        /// <param name="ErrorCode">Operation result code (please see the documentation)</param>
        /// <param name="Comment">Operation result comment (please see the documentation)</param>
        private void Server_OnRequestAttributes(object Sender, string Path, bool FollowSymLinks, TElSftpFileAttributes Attributes, ref int ErrorCode, ref string Comment) {
            string localpath = CanonicalizePath(Path, true);
            try {
                if (File.Exists(localpath) || Directory.Exists(localpath)) {
                    GetAttributes(localpath, Attributes);
                    ErrorCode = 0;
                }
                else {
                    ErrorCode = __Global.SSH_ERROR_NO_SUCH_FILE;
                    Comment = "No such file";
                }
            }
            catch (Exception ex) {
                ErrorCode = __Global.SSH_ERROR_FAILURE;
                Comment = ex.Message;
            }
        }

        /// <summary>
        /// Is fired when 'Write' operation is requested by client
        /// </summary>
        /// <param name="Sender">ElSFTPServer object</param>
        /// <param name="Data">User-specific data associated with file operation</param>
        /// <param name="Offset">File write offset</param>
        /// <param name="Buffer">Data to write</param>
        /// <param name="BufferOffset">Data start index</param>
        /// <param name="Count">Length of data to write</param>
        /// <param name="ErrorCode">Operation result code (please see the documentation)</param>
        /// <param name="Comment">Operation result comment (please see the documentation)</param>
        private void Server_OnWriteFile(object Sender, object Data, long Offset, byte[] Buffer, int BufferOffset, int Count, ref int ErrorCode, ref string Comment) {
            try {
                var strm = (Stream) Data;
                strm.Position = Offset;
                strm.Write(Buffer, BufferOffset, Count);
                ErrorCode = 0;
            }
            catch (Exception ex) {
                ErrorCode = __Global.SSH_ERROR_FAILURE;
                Comment = ex.Message;
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Canonicalizes relative path
        /// </summary>
        /// <param name="relpath">Path to canonicalize</param>
        /// <param name="local">If true, the path will be converted to local ('C:\...'), otherwise to remote ('/...') form</param>
        /// <returns>the converted path</returns>
        private string CanonicalizePath(string relpath, bool local) {
            var delims = new char[1];
            delims[0] = '/';
            string[] pathelems = relpath.Split(delims, 1024);
            var lst = new ArrayList();
            for (int i = 0; i < pathelems.Length; i++) {
                string elem = pathelems[i];
                if (elem == "..") {
                    if (lst.Count > 0) {
                        lst.RemoveAt(lst.Count - 1);
                    }
                }
                else if ((elem != ".") && (elem != "")) {
                    lst.Add(elem);
                }
            }
            string result;
            string separator;
            if (local) {
                var tana = @"C:\RemoteAnalyst\Systems\076391\";
                var tand = @"C:\RemoteAnalyst\Systems\076638\";

                //Check if Remote Analyst Directory exists.
                if (!Directory.Exists(tana)) {
                    //Create System Directory.
                    Directory.CreateDirectory(tana);
                }
                if (!Directory.Exists(tand)) {
                    //Create System Directory.
                    Directory.CreateDirectory(tand);
                }

                var remoteAnalystDirectory = @"C:\RemoteAnalyst\";
                result = remoteAnalystDirectory;
                separator = "\\";
            }
            else {
                result = "/";
                separator = "/";
            }
            for (int i = 0; i < lst.Count; i++) {
                result = result + (string) lst[i];
                if (i != lst.Count - 1) {
                    result = result + separator;
                }
            }
            return result;
        }

        /// <summary>
        /// Return the UNIX-style file information string
        /// </summary>
        /// <param name="info">file information object</param>
        /// <returns>the UNIX-style string</returns>
        private string UnixPath(TElSftpFileInfo info) {
            string result = "";
            if (info.Attributes.Directory) {
                result = "drwxrwxrwx   1 user     group    ";
            }
            else {
                result = "-rwxrwxrwx   1 user     group    ";
            }
            string szstr = info.Attributes.Size.ToString();
            while (szstr.Length < 8) {
                szstr = "0" + szstr;
            }
            result = result + szstr + " ";
            result = result + " " + info.Attributes.MTime.ToString("MMM dd HH:mm") + " " + info.Name;
            return result;
        }

        /// <summary>
        /// Fills ElSftpFileInfo object with file information
        /// </summary>
        /// <param name="info">object to fill</param>
        /// <param name="ErrorCode">operation result code</param>
        private void FillNextFileInfo(TElSftpFileInfo info, ref int ErrorCode) {
            TElSftpFileAttributes attrs = info.Attributes;
            if (m_dirIndex < m_dirList.Length) {
                info.Name = Path.GetFileName(m_dirList[m_dirIndex]);
                attrs.CTime = Directory.GetCreationTime(m_dirList[m_dirIndex]);
                attrs.ATime = Directory.GetLastAccessTime(m_dirList[m_dirIndex]);
                attrs.MTime = Directory.GetLastWriteTime(m_dirList[m_dirIndex]);
                attrs.Size = 0;
                attrs.Directory = true;
                attrs.FileType = TSBSftpFileType.ftDirectory;
                m_dirIndex++;
            }
            else if (m_fileIndex < m_fileList.Length) {
                info.Name = Path.GetFileName(m_fileList[m_fileIndex]);
                attrs.CTime = File.GetCreationTime(m_fileList[m_fileIndex]);
                attrs.ATime = File.GetLastAccessTime(m_fileList[m_fileIndex]);
                attrs.MTime = File.GetLastWriteTime(m_fileList[m_fileIndex]);
                var i = new FileInfo(m_fileList[m_fileIndex]);
                attrs.Size = i.Length;
                attrs.Directory = false;
                attrs.FileType = TSBSftpFileType.ftFile;
                m_fileIndex++;
            }
            else {
                ErrorCode = __Global.SSH_ERROR_EOF;
                return;
            }
            attrs.UserExecute = true;
            attrs.UserRead = true;
            attrs.UserWrite = true;
            attrs.GroupExecute = true;
            attrs.GroupRead = true;
            attrs.GroupWrite = true;
            attrs.OtherExecute = true;
            attrs.OtherRead = true;
            attrs.OtherWrite = true;
            attrs.IncludedAttributes = __Global.saPermissions | __Global.saSize |
                                       __Global.saMTime | __Global.saATime | __Global.saCTime;
            info.LongName = UnixPath(info);
        }

        #endregion

        #region Class members

        private int m_dirIndex;
        private string[] m_dirList;
        private int m_fileIndex;
        private string[] m_fileList;
        private TElSSHSubsystemThread m_thread;

        #endregion

        private void Server_OnRequestAttributes2(object Sender, object Data, TElSftpFileAttributes Attributes, ref int ErrorCode, ref string Comment) {
            if (Data is FileStream) {
                try {
                    var f = (FileStream) Data;
                    string path = f.Name;
                    GetAttributes(path, Attributes);
                    ErrorCode = 0;
                }
                catch (Exception ex) {
                    ErrorCode = __Global.SSH_ERROR_FAILURE;
                    Comment = ex.Message;
                }
            }
        }
    }
}