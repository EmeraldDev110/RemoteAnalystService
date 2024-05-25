using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace RemoteAnalyst.UWSRelay.BLL {
    class Helper {
        internal bool IsFileinUse(FileInfo file) {
            FileStream stream = null;

            try {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException) {
                //the file is unavailable because it is:
                //1. still being written to
                //2. being processed by another thread
                //3. does not exist (has already been processed)
                return true;
            }
            finally {
                if (stream != null) stream.Close();
            }
            return false;
        }

        internal bool IsFileSizeIncreasing(FileInfo file) {
            try { 
                var f = new FileInfo(file.FullName);
                var currentSize = f.Length;

                Thread.Sleep(5000); //5 SECONDS
                var f2 = new FileInfo(file.FullName);
                var newFileSize = f2.Length;

                if (currentSize.Equals(newFileSize)) return false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string RemoveNULL(string input) {
            if (!string.IsNullOrEmpty(input)) {
                var sb = new StringBuilder(input.Length);
                foreach (char c in input) {
                    sb.Append(Char.IsControl(c) ? ' ' : c);
                }
                input = sb.ToString();
            }
            return input;
        }
    }
}
