using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.UWSLoader.Core.BusinessLogic {
    class Helper {
        public static short Reverse(short tempShort) {
            //This function will reverse the byte and get the right value.
            var bShort = new byte[2];
            var nShort = new byte[2];

            bShort = BitConverter.GetBytes(tempShort);
            nShort[0] = bShort[1];
            nShort[1] = bShort[0];
            short returnShort = Convert.ToInt16(BitConverter.ToInt16(nShort, 0));

            return returnShort;
        }

        public static int Reverse(int tempInteger) {
            //This function will reverse the byte and get the right value.
            var bInteger = new byte[4];
            var nInteger = new byte[4];

            bInteger = BitConverter.GetBytes(tempInteger);
            nInteger[0] = bInteger[3];
            nInteger[1] = bInteger[2];
            nInteger[2] = bInteger[1];
            nInteger[3] = bInteger[0];
            int returnInt = Convert.ToInt32(BitConverter.ToInt32(nInteger, 0));

            return returnInt;
        }

        public static long Reverse(long tempLong) {
            //This function will reverse the byte and get the right value.
            var bLong = new byte[8];
            var nLong = new byte[8];

            bLong = BitConverter.GetBytes(tempLong);
            nLong[0] = bLong[7];
            nLong[1] = bLong[6];
            nLong[2] = bLong[5];
            nLong[3] = bLong[4];
            nLong[4] = bLong[3];
            nLong[5] = bLong[2];
            nLong[6] = bLong[1];
            nLong[7] = bLong[0];
            long returnLong = Convert.ToInt64(BitConverter.ToInt64(nLong, 0));

            return returnLong;
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
