using System;
using System.Text;
using System.IO;

namespace RemoteAnalyst.BusinessLogic.Util
{
    static class ArgumentUtil
    {
        public static Stream GetInputStream(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                return File.OpenRead(filePath);
            }
            else
            {
                return Console.OpenStandardInput();
            }
        }
        public static Stream GetOutputStream(string filePath, bool truncate)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                if (truncate)
                {
                    return File.Create(filePath);
                }
                else
                {
                    var ret = File.OpenWrite(filePath);
                    ret.Seek(0, SeekOrigin.End);
                    return ret;
                }
            }
            else
            {
                return Console.OpenStandardOutput();
            }
        }
        public static byte[] GetPassword(string password)
        {
            if (!string.IsNullOrEmpty(password))
            {
                return Encoding.UTF8.GetBytes(password);
            }
            else
            {
                throw new ArgumentException("Password cannot be empty.");
            }
        }
        public static string EolOptionToEolString(string eol)
        {
            if (string.IsNullOrEmpty(eol))
            {
                return null;
            }
            if (eol.Equals("crlf", StringComparison.OrdinalIgnoreCase))
            {
                return "\r\n";
            }
            else if (eol.Equals("lf", StringComparison.OrdinalIgnoreCase))
            {
                return "\n";
            }
            else if (eol.Equals("cr", StringComparison.OrdinalIgnoreCase))
            {
                return "\r";
            }
            else
            {
                return null;
            }
        }
    }
}
