using System;
using System.Collections.Generic;

namespace RemoteAnalyst.BusinessLogic.Util
{
    static class ByteUtil
    {
        public static byte ConvertToByte(char sp)
        {
            if ('0' <= sp && sp <= '9')
            {
                return (byte)(sp - '0');
            }
            else if ('A' <= sp && sp <= 'F')
            {
                return (byte)(sp - 'A');
            }
            else if ('a' <= sp && sp <= 'f')
            {
                return (byte)(sp - 'a' + 10);
            }
            else
            {
                throw new Exception($"invalid value:{sp}({(int)sp})");
            }
        }
        public static byte[] ConvertToBytes(ArraySegment<string> lines)
        {
            var lst = new List<byte>(4096);
            foreach (var l in lines)
            {
                ConvertToBytes(l, lst);
            }
            return lst.ToArray();
        }
        public static void ConvertToBytes(string l, List<byte> lst)
        {
            var sp = l.AsSpan();
            for (int i = 0; i < l.Length / 2; i++)
            {
                lst.Add((byte)((ConvertToByte(sp[i * 2]) << 4) | ConvertToByte(sp[i * 2 + 1])));
            }
        }
        public static byte[] ConvertToBytes(string l)
        {
            var ret = new byte[l.Length / 2];
            var sp = l.AsSpan();
            for (int i = 0; i < l.Length / 2; i++)
            {
                ret[i] = (byte)((ConvertToByte(sp[i * 2]) << 4) | ConvertToByte(sp[i * 2 + 1]));
            }
            return ret;
        }
        static char ByteToHexChar(byte b)
        {
            if (b < 10)
            {
                return (char)('0' + b);
            }
            else if (b < 16)
            {
                return (char)('a' + b - 10);
            }
            else
            {
                throw new ArgumentOutOfRangeException($"invalid byte value:{(int)b}");
            }
        }
        public static void ConvertToHexChars(ReadOnlySpan<byte> data, Span<char> dest)
        {
            if (data.Length * 2 > dest.Length)
            {
                throw new ArgumentOutOfRangeException("buffer length must be at least two times to data");
            }
            for (int i = 0; i < data.Length; i++)
            {
                dest[i * 2] = ByteToHexChar((byte)(data[i] >> 4));
                dest[i * 2 + 1] = ByteToHexChar((byte)(data[i] & 0xf));
            }
        }
        public static string ConvertToHexString(ReadOnlySpan<byte> data)
        {
            //return string.Create(data.Length * 2, data.ToArray(), (c, state) =>
            //{
            //    for (int i = 0; i < state.Length; i++)
            //    {
            //        c[i * 2] = ByteToHexChar((byte)(state[i] >> 4));
            //        c[i * 2 + 1] = ByteToHexChar((byte)(state[i] & 0xf));
            //    }
            //});

            char[] c = new char[data.Length * 2];
            byte[] state = data.ToArray();
            for (int i = 0; i < data.ToArray().Length; i++)
            {
                c[i * 2] = ByteToHexChar((byte)(state[i] >> 4));
                c[i * 2 + 1] = ByteToHexChar((byte)(state[i] & 0xf));
            }
            return new string(c);
        }
    }
}
