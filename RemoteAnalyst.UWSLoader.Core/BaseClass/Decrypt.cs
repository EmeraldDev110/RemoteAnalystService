using System;

namespace RemoteAnalyst.UWSLoader.Core.BaseClass
{
    public class Decrypt
    {
        #region private variables

        /*DES private variables-------------------------------------------------------------------------*/

        private readonly char[] cMap =
        {
            '8', '5', 'R', 'X', '/', 'f', 'z', 'S', 'D', 'V', 't', 'O', 'u', 'G', 'J', 'p',
            '.', 'B', 'I', 'Y', '4', 'y', 'L', 'C', 'i', 'h', '3', 'N', 'U', '2', 'F', 'Z', 'P', 'a', 'Q', 'A', '6', 'l',
            'k', 'w', 'd', 'e', 'b', 'n', '9', '7', '1', 'M', 'o', 'H', 'x', 'j', 's', 'W', 'T', '0', 'v', 'm', 'K', 'q',
            'c', 'g', 'E', 'r'
        };

        private readonly char[,,] cSBoxes =
        {
            {
                {
//0-unused
                    '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0'
                },
                {
                    '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0'
                },
                {
                    '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0'
                },
                {
                    '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0'
                }
            },
            {
                {
//1
                    (char) 14, (char) 4, (char) 13, (char) 1, (char) 2, (char) 15, (char) 11, (char) 8, (char) 3,
                    (char) 10, (char) 6, (char) 12, (char) 5, (char) 9, (char) 0, (char) 7
                },
                {
                    (char) 0, (char) 15, (char) 7, (char) 4, (char) 14, (char) 2, (char) 13, (char) 1, (char) 10,
                    (char) 6, (char) 12, (char) 11, (char) 9, (char) 5, (char) 3, (char) 8
                },
                {
                    (char) 4, (char) 1, (char) 14, (char) 8, (char) 13, (char) 6, (char) 2, (char) 11, (char) 15,
                    (char) 12, (char) 9, (char) 7, (char) 3, (char) 10, (char) 5, (char) 0
                },
                {
                    (char) 15, (char) 12, (char) 8, (char) 2, (char) 4, (char) 9, (char) 1, (char) 7, (char) 5,
                    (char) 11, (char) 3, (char) 14, (char) 10, (char) 0, (char) 6, (char) 13
                }
            },
            {
                {
//2
                    (char) 15, (char) 1, (char) 8, (char) 14, (char) 6, (char) 11, (char) 3, (char) 4, (char) 9,
                    (char) 7, (char) 2, (char) 13, (char) 12, (char) 0, (char) 5, (char) 10
                },
                {
                    (char) 3, (char) 13, (char) 4, (char) 7, (char) 15, (char) 2, (char) 8, (char) 14, (char) 12,
                    (char) 0, (char) 1, (char) 10, (char) 6, (char) 9, (char) 11, (char) 5
                },
                {
                    (char) 0, (char) 14, (char) 7, (char) 11, (char) 10, (char) 4, (char) 13, (char) 1, (char) 5,
                    (char) 8, (char) 12, (char) 6, (char) 9, (char) 3, (char) 2, (char) 15
                },
                {
                    (char) 13, (char) 8, (char) 10, (char) 1, (char) 3, (char) 15, (char) 4, (char) 2, (char) 11,
                    (char) 6, (char) 7, (char) 12, (char) 0, (char) 5, (char) 14, (char) 9
                }
            },
            {
                {
//3
                    (char) 10, (char) 0, (char) 9, (char) 14, (char) 6, (char) 3, (char) 15, (char) 5, (char) 1,
                    (char) 13, (char) 12, (char) 7, (char) 11, (char) 4, (char) 2, (char) 8
                },
                {
                    (char) 13, (char) 7, (char) 0, (char) 9, (char) 3, (char) 4, (char) 6, (char) 10, (char) 2, (char) 8,
                    (char) 5, (char) 14, (char) 12, (char) 11, (char) 15, (char) 1
                },
                {
                    (char) 13, (char) 6, (char) 4, (char) 9, (char) 8, (char) 15, (char) 3, (char) 0, (char) 11,
                    (char) 1, (char) 2, (char) 12, (char) 5, (char) 10, (char) 14, (char) 7
                },
                {
                    (char) 1, (char) 10, (char) 13, (char) 0, (char) 6, (char) 9, (char) 8, (char) 7, (char) 4,
                    (char) 15, (char) 14, (char) 3, (char) 11, (char) 5, (char) 2, (char) 12
                }
            },
            {
                {
//4
                    (char) 7, (char) 13, (char) 14, (char) 3, (char) 0, (char) 6, (char) 9, (char) 10, (char) 1,
                    (char) 2, (char) 8, (char) 5, (char) 11, (char) 12, (char) 4, (char) 15
                },
                {
                    (char) 13, (char) 8, (char) 11, (char) 5, (char) 6, (char) 15, (char) 0, (char) 3, (char) 4,
                    (char) 7, (char) 2, (char) 12, (char) 1, (char) 10, (char) 14, (char) 9
                },
                {
                    (char) 10, (char) 6, (char) 9, (char) 0, (char) 12, (char) 11, (char) 7, (char) 13, (char) 15,
                    (char) 1, (char) 3, (char) 14, (char) 5, (char) 2, (char) 8, (char) 4
                },
                {
                    (char) 3, (char) 15, (char) 0, (char) 6, (char) 10, (char) 1, (char) 13, (char) 8, (char) 9,
                    (char) 4, (char) 5, (char) 11, (char) 12, (char) 7, (char) 2, (char) 14
                }
            },
            {
                {
//5
                    (char) 2, (char) 12, (char) 4, (char) 1, (char) 7, (char) 10, (char) 11, (char) 6, (char) 8,
                    (char) 5, (char) 3, (char) 15, (char) 13, (char) 0, (char) 14, (char) 9
                },
                {
                    (char) 14, (char) 11, (char) 2, (char) 12, (char) 4, (char) 7, (char) 13, (char) 1, (char) 5,
                    (char) 0, (char) 15, (char) 10, (char) 3, (char) 9, (char) 8, (char) 6
                },
                {
                    (char) 4, (char) 2, (char) 1, (char) 11, (char) 10, (char) 13, (char) 7, (char) 8, (char) 15,
                    (char) 9, (char) 12, (char) 5, (char) 6, (char) 3, (char) 0, (char) 14
                },
                {
                    (char) 11, (char) 8, (char) 12, (char) 7, (char) 1, (char) 14, (char) 2, (char) 13, (char) 6,
                    (char) 15, (char) 0, (char) 9, (char) 10, (char) 4, (char) 5, (char) 3
                }
            },
            {
                {
//6
                    (char) 12, (char) 1, (char) 10, (char) 15, (char) 9, (char) 2, (char) 6, (char) 8, (char) 0,
                    (char) 13, (char) 3, (char) 4, (char) 14, (char) 7, (char) 5, (char) 11
                },
                {
                    (char) 10, (char) 15, (char) 4, (char) 2, (char) 7, (char) 12, (char) 9, (char) 5, (char) 6,
                    (char) 1, (char) 13, (char) 14, (char) 0, (char) 11, (char) 3, (char) 8
                },
                {
                    (char) 9, (char) 14, (char) 15, (char) 5, (char) 2, (char) 8, (char) 12, (char) 3, (char) 7,
                    (char) 0, (char) 4, (char) 10, (char) 1, (char) 13, (char) 11, (char) 6
                },
                {
                    (char) 4, (char) 3, (char) 2, (char) 12, (char) 9, (char) 5, (char) 15, (char) 10, (char) 11,
                    (char) 14, (char) 1, (char) 7, (char) 6, (char) 0, (char) 8, (char) 13
                }
            },
            {
                {
//7
                    (char) 4, (char) 11, (char) 2, (char) 14, (char) 15, (char) 0, (char) 8, (char) 13, (char) 3,
                    (char) 12, (char) 9, (char) 7, (char) 5, (char) 10, (char) 6, (char) 1
                },
                {
                    (char) 13, (char) 0, (char) 11, (char) 7, (char) 4, (char) 9, (char) 1, (char) 10, (char) 14,
                    (char) 3, (char) 5, (char) 12, (char) 2, (char) 15, (char) 8, (char) 6
                },
                {
                    (char) 1, (char) 4, (char) 11, (char) 13, (char) 12, (char) 3, (char) 7, (char) 14, (char) 10,
                    (char) 15, (char) 6, (char) 8, (char) 0, (char) 5, (char) 9, (char) 2
                },
                {
                    (char) 6, (char) 11, (char) 13, (char) 8, (char) 1, (char) 4, (char) 10, (char) 7, (char) 9,
                    (char) 5, (char) 0, (char) 15, (char) 14, (char) 2, (char) 3, (char) 12
                }
            },
            {
                {
//8
                    (char) 13, (char) 2, (char) 8, (char) 4, (char) 6, (char) 15, (char) 11, (char) 1, (char) 10,
                    (char) 9, (char) 3, (char) 14, (char) 5, (char) 0, (char) 12, (char) 7
                },
                {
                    (char) 1, (char) 15, (char) 13, (char) 8, (char) 10, (char) 3, (char) 7, (char) 4, (char) 12,
                    (char) 5, (char) 6, (char) 11, (char) 0, (char) 14, (char) 9, (char) 2
                },
                {
                    (char) 7, (char) 11, (char) 4, (char) 1, (char) 9, (char) 12, (char) 14, (char) 2, (char) 0,
                    (char) 6, (char) 10, (char) 13, (char) 15, (char) 3, (char) 5, (char) 8
                },
                {
                    (char) 2, (char) 1, (char) 14, (char) 7, (char) 4, (char) 10, (char) 8, (char) 13, (char) 15,
                    (char) 12, (char) 9, (char) 0, (char) 3, (char) 5, (char) 6, (char) 11
                }
            }
        };

        private readonly int[] iEBit =
        {
            0, 32, 1, 2, 3, 4, 5, 4, 5, 6, 7, 8, 9, 8, 9, 10, 11, 12, 13, 12, 13, 14, 15, 16,
            17, 16, 17, 18, 19, 20, 21, 20, 21, 22, 23, 24, 25, 24, 25, 26, 27, 28, 29, 28, 29, 30, 31, 32, 1
        };

        private readonly int[] iPermFinalF =
        {
            0, 16, 7, 20, 21, 29, 12, 28, 17, 1, 15, 23, 26, 5, 18, 31, 10, 2, 8, 24,
            14, 32, 27, 3, 9, 19, 13, 30, 6, 22, 11, 4, 25
        };

        private readonly int[] iPermIP =
        {
            0, 58, 50, 42, 34, 26, 18, 10, 2, 60, 52, 44, 36, 28, 20, 12, 4, 62, 54, 46,
            38, 30, 22, 14, 6, 64, 56, 48, 40, 32, 24, 16, 8, 57, 49, 41, 33, 25, 17, 9, 1, 59, 51, 43, 35, 27, 19, 11,
            3, 61, 53, 45, 37, 29, 21, 13, 5, 63, 55, 47, 39, 31, 23, 15, 7
        };

        private readonly int[] iPermIPInv =
        {
            0, 40, 8, 48, 16, 56, 24, 64, 32, 39, 7, 47, 15, 55, 23, 63, 31, 38, 6, 46,
            14, 54, 22, 62, 30, 37, 5, 45, 13, 53, 21, 61, 29, 36, 4, 44, 12, 52, 20, 60, 28, 35, 3, 43, 11, 51, 19, 59,
            27, 34, 2, 42, 10, 50, 18, 58, 26, 33, 1, 41, 9, 49, 17, 57, 25
        };

        private readonly int[] iPermPC1 =
        {
            0, 57, 49, 41, 33, 25, 17, 9, 1, 58, 50, 42, 34, 26, 18, 10, 2, 59, 51, 43,
            35, 27, 19, 11, 3, 60, 52, 44, 36, 63, 55, 47, 39, 31, 23, 15, 7, 62, 54, 46, 38, 30, 22, 14, 6, 61, 53, 45,
            37, 29, 21, 13, 5, 28, 20, 12, 4
        };

        private readonly int[] iPermPC2 =
        {
            0, 14, 17, 11, 24, 1, 5, 3, 28, 15, 6, 21, 10, 23, 19, 12, 4, 26, 8, 16, 7,
            27, 20, 13, 2, 41, 52, 31, 37, 47, 55, 30, 40, 51, 45, 33, 48, 44, 49, 39, 56, 34, 53, 46, 42, 50, 36, 29,
            32
        };

        private readonly short[] siJumble = {-13, -7, -19, 29, -31, 5, -37, 17, -11, 23, -19};
        private readonly long[] uliKnLeft = new long[17];
        private readonly long[] uliKnRight = new long[17];
        private long uliKeyLeft;
        private long uliKeyRight;

        #endregion

        /*This function initializes the DES variables---------------------------------------------------*/

        private void vInitializeVariables()
        {
            int iLoopA;

            uliKeyLeft = 0;
            uliKeyRight = 0;
            for (iLoopA = 0; iLoopA < 17; iLoopA++)
            {
                uliKnLeft[iLoopA] = 0;
                uliKnRight[iLoopA] = 0;
            }
        }

        /*This is the function that encrypts/decrypts the 64-bit block----------------------------------*/

        public string strBlockEncode(String strPlain, bool bEncrypt)
        {
            long uliTextLeft;
            long uliTextRight;
            long uliTemp;
            long uliCipLeft;
            long uliCipRight;
            int iLoopA;
            var cBitsTempA = new char[65];
            var uliLn = new long[17];
            var uliRn = new long[17];
            String strCipher;
            char cTemp;

            uliTextLeft = 0;
            uliTextRight = 0;

            for (iLoopA = 0; iLoopA < 4; iLoopA++)
            {
                uliTextLeft = 256*uliTextLeft + strPlain[iLoopA];
                uliTextRight = 256*uliTextRight + strPlain[iLoopA + 4];
            }

            /*Preparing L0 and R0 Using IP permutation--------------------------------------------------*/
            uliTemp = (long) 1 << 31;
            for (iLoopA = 1; iLoopA <= 32; iLoopA++)
            {
                if ((uliTextLeft & uliTemp) != 0) cBitsTempA[iLoopA] = (char) 1;
                else cBitsTempA[iLoopA] = (char) 0;
                if ((uliTextRight & uliTemp) != 0) cBitsTempA[iLoopA + 32] = (char) 1;
                else cBitsTempA[iLoopA + 32] = (char) 0;
                uliTemp = uliTemp >> 1;
            }


            uliLn[0] = 0;
            uliRn[0] = 0;
            for (iLoopA = 1; iLoopA <= 32; iLoopA++)
            {
                uliLn[0] = uliLn[0]*2 + cBitsTempA[iPermIP[iLoopA]];
                uliRn[0] = uliRn[0]*2 + cBitsTempA[iPermIP[iLoopA + 32]];
            }


            /*Preparing L1-L16 and R1-R16---------------------------------------------------------------*/
            for (iLoopA = 1; iLoopA <= 16; iLoopA++)
            {
                uliLn[iLoopA] = uliRn[iLoopA - 1];
                if (bEncrypt)
                    uliRn[iLoopA] = uliLn[iLoopA - 1] ^
                                    uliFFunction(uliRn[iLoopA - 1], uliKnLeft[iLoopA], uliKnRight[iLoopA]);
                else
                    uliRn[iLoopA] = uliLn[iLoopA - 1] ^
                                    uliFFunction(uliRn[iLoopA - 1], uliKnLeft[17 - iLoopA], uliKnRight[17 - iLoopA]);
            }


            /*L16 and R16 ready. Proceeding to do the final permutation---------------------------------*/
            uliTemp = (long) 1 << 31;
            for (iLoopA = 1; iLoopA <= 32; iLoopA++)
            {
                if ((uliRn[16] & uliTemp) != 0) cBitsTempA[iLoopA] = (char) 1;
                else cBitsTempA[iLoopA] = (char) 0;
                if ((uliLn[16] & uliTemp) != 0) cBitsTempA[iLoopA + 32] = (char) 1;
                else cBitsTempA[iLoopA + 32] = (char) 0;
                uliTemp = uliTemp >> 1;
            }
            uliCipLeft = 0;
            uliCipRight = 0;
            for (iLoopA = 1; iLoopA <= 32; iLoopA++)
            {
                uliCipLeft = 2*uliCipLeft + cBitsTempA[iPermIPInv[iLoopA]];
                uliCipRight = 2*uliCipRight + cBitsTempA[iPermIPInv[iLoopA + 32]];
            }

            strCipher = string.Empty;

            cTemp = (char) (uliCipLeft/16777216 & 0xFF);
            strCipher = strCipher + cTemp;
            cTemp = (char) (uliCipLeft/65536 & 0xFF);
            strCipher = strCipher + cTemp;
            cTemp = (char) (uliCipLeft/256 & 0xFF);
            strCipher = strCipher + cTemp;
            cTemp = (char) (uliCipLeft & 0x000000FF);
            strCipher = strCipher + cTemp;

            cTemp = (char) (uliCipRight/16777216 & 0xFF);
            strCipher = strCipher + cTemp;
            cTemp = (char) (uliCipRight/65536 & 0xFF);
            strCipher = strCipher + cTemp;
            cTemp = (char) (uliCipRight/256 & 0xFF);
            strCipher = strCipher + cTemp;
            cTemp = (char) (uliCipRight & 0x000000FF);
            strCipher = strCipher + cTemp;


            return strCipher;
        }


        /*This is the function that encrypts a string---------------------------------------------------*/

        public string strStringEncode(String strSource, bool bEncrypt)
        {
            int iLen;
            int iPad;
            int iLoopA;
            String strTemp;
            int iBlockCount;
            int iTemp;
            String strDest;
            char cTemp;
            String strEncKey;


            /*Hardcoding the key------------------------------------------------------------------------*/
            strEncKey = string.Empty;
            strEncKey += (char) (0x0C);
            strEncKey += (char) (0x2A);
            strEncKey += (char) (0x0C);
            strEncKey += (char) (0x2D);
            strEncKey += (char) (0x33);
            strEncKey += (char) (0x42);
            strEncKey += (char) (0x87);
            strEncKey += (char) (0x61);
            vSetKey(strEncKey);

            iLen = strSource.Length;

            if (bEncrypt)
            {
                iPad = 8 - (iLen%8);
                strTemp = strSource;
                for (iLoopA = 0; iLoopA < (iPad - 1); iLoopA++) strTemp = strTemp + " ";
                cTemp = (char) iPad;
                strTemp = strTemp + cTemp;

                iBlockCount = (iLen + iPad)/8;

                strDest = string.Empty;
                for (iLoopA = 0; iLoopA < iBlockCount; iLoopA++)
                    strDest = strDest + strBlockEncode(strTemp.Substring(iLoopA*8, 8), true);

                strTemp = strDest;
                strDest = string.Empty;

                for (iLoopA = 0; iLoopA < iBlockCount*8; iLoopA++)
                {
                    iTemp = (strTemp[iLoopA] & 0xF0)/16;
                    if (iTemp >= 10) iTemp = iTemp - 10 + 65;
                    else iTemp = iTemp + 48;
                    cTemp = (char) iTemp;
                    strDest = strDest + cTemp;

                    iTemp = (strTemp[iLoopA] & 0x0F);
                    if (iTemp >= 10) iTemp = iTemp - 10 + 65;
                    else iTemp = iTemp + 48;
                    cTemp = (char) iTemp;
                    strDest = strDest + cTemp;
                }
            }
            else
            {
                //skipped error checks....
                var cTempBuf = new char[strSource.Length];
                for (iLoopA = 0; iLoopA < iLen/2; iLoopA++)
                {
                    cTempBuf[iLoopA] = (char) 0;
                    iTemp = strSource[iLoopA*2];
                    if (iTemp >= 'A') iTemp = iTemp - 'A' + 10;
                    else iTemp = iTemp - '0';

                    cTempBuf[iLoopA] = (char) (iTemp*16);

                    iTemp = strSource[iLoopA*2 + 1];
                    if (iTemp >= 'A') iTemp = iTemp - 'A' + 10;
                    else iTemp = iTemp - '0';

                    cTempBuf[iLoopA] += (char) iTemp;
                }
                cTempBuf[iLoopA] = (char) 0;
                iBlockCount = iLen/16;
                strTemp = new String(cTempBuf);
                strDest = string.Empty;
                for (iLoopA = 0; iLoopA < iBlockCount; iLoopA++)
                    strDest += strBlockEncode(strTemp.Substring(8*iLoopA), false);
                iPad = strDest[iLen/2 - 1];

                iLen = iLen/2 - iPad;
                strDest = strDest.Substring(0, iLen);
            }
            return strDest;
        }


        /*This function will set the key to be used for the encryption/decription functions-------------*/

        private void vSetKey(String strKey)
        {
            int iLoopA;
            int iLoopB;
            long uliTemp;
            var uliCn = new long[17];
            var uliDn = new long[17];
            var cBitsTempA = new char[65];
            //char cBitsTempB[65];

            vInitializeVariables();

            /*Preparing the key-------------------------------------------------------------------------*/
            for (iLoopA = 0; iLoopA < 4; iLoopA++)
            {
                //uliKeyLeft=256*uliKeyLeft+(int)strKey.charAt(iLoopA);
                //uliKeyRight=256*uliKeyRight+(int)strKey.charAt(iLoopA+4);
                uliKeyLeft = 256*uliKeyLeft + strKey[iLoopA];
                uliKeyRight = 256*uliKeyRight + strKey[iLoopA + 4];
            }

            /*Preparing C0 and D0 Using PC1 permutation-------------------------------------------------*/
            uliTemp = (long) 1 << 31;
            for (iLoopA = 1; iLoopA <= 32; iLoopA++)
            {
                if ((uliKeyLeft & uliTemp) != 0) cBitsTempA[iLoopA] = (char) 1;
                else cBitsTempA[iLoopA] = (char) 0;
                if ((uliKeyRight & uliTemp) != 0) cBitsTempA[iLoopA + 32] = (char) 1;
                else cBitsTempA[iLoopA + 32] = (char) 0;
                uliTemp = uliTemp >> 1;
            }

            uliCn[0] = 0;
            uliDn[0] = 0;
            for (iLoopA = 1; iLoopA <= 28; iLoopA++)
            {
                uliCn[0] = uliCn[0]*2 + cBitsTempA[iPermPC1[iLoopA]];
                uliDn[0] = uliDn[0]*2 + cBitsTempA[iPermPC1[iLoopA + 28]];
            }


            /*Preparing C1-C16 and D1-D16---------------------------------------------------------------*/
            for (iLoopA = 1; iLoopA <= 16; iLoopA++)
            {
                uliCn[iLoopA] = uliCn[iLoopA - 1] << 1;
                uliDn[iLoopA] = uliDn[iLoopA - 1] << 1;
                if ((uliCn[iLoopA] & ((long) 1 << 28)) != 0) uliCn[iLoopA] = uliCn[iLoopA] | 1;
                if ((uliDn[iLoopA] & ((long) 1 << 28)) != 0) uliDn[iLoopA] = uliDn[iLoopA] | 1;

                if ((iLoopA != 1) && (iLoopA != 2) && (iLoopA != 9) && (iLoopA != 16))
                {
                    uliCn[iLoopA] = uliCn[iLoopA] << 1;
                    uliDn[iLoopA] = uliDn[iLoopA] << 1;
                    if ((uliCn[iLoopA] & ((long) 1 << 28)) != 0) uliCn[iLoopA] = uliCn[iLoopA] | 1;
                    if ((uliDn[iLoopA] & ((long) 1 << 28)) != 0) uliDn[iLoopA] = uliDn[iLoopA] | 1;
                }
            }

            /*Preparing K1-K16 Using PC2 permutation----------------------------------------------------*/
            for (iLoopA = 1; iLoopA <= 16; iLoopA++)
            {
                uliTemp = (long) 1 << 27;
                for (iLoopB = 1; iLoopB <= 28; iLoopB++)
                {
                    if ((uliCn[iLoopA] & uliTemp) != 0) cBitsTempA[iLoopB] = (char) 1;
                    else cBitsTempA[iLoopB] = (char) 0;
                    if ((uliDn[iLoopA] & uliTemp) != 0) cBitsTempA[iLoopB + 28] = (char) 1;
                    else cBitsTempA[iLoopB + 28] = (char) 0;
                    uliTemp = uliTemp >> 1;
                }

                uliKnLeft[iLoopA] = 0;
                uliKnRight[iLoopA] = 0;
                for (iLoopB = 1; iLoopB <= 24; iLoopB++)
                {
                    uliKnLeft[iLoopA] = 2*uliKnLeft[iLoopA] + cBitsTempA[iPermPC2[iLoopB]];
                    uliKnRight[iLoopA] = 2*uliKnRight[iLoopA] + cBitsTempA[iPermPC2[iLoopB + 24]];
                }
            }
            /*Key processing complete-------------------------------------------------------------------*/
        }


        /*This function does an SBox Lookup based on the 6 bit value------------------------------------*/

        private char cSBoxLookup(int iBox, char cValue)
        {
            int iCol;
            int iRow;

            iCol = 0;
            iRow = 0;

            if ((cValue & 0x20) != 0) iRow += 2;
            if ((cValue & 0x01) != 0) iRow += 1;

            cValue = (char) (cValue >> 1);
            iCol = cValue & 0x0F;

            return cSBoxes[iBox, iRow, iCol];
        }


        /*This function is used in the computation of L1-L16 and R1-R16---------------------------------*/

        private long uliFFunction(long uliR, long uliKLeft, long uliKRight)
        {
            int iLoopA;
            long uliTemp;
            long uliRxLeft;
            long uliRxRight;
            long uliPreBoxLeft;
            long uliPreBoxRight;
            var cBitsTempA = new char[65];
            //char cBitsTempB[]=new char[65];
            var cBoxValues = new char[9];
            long uliFinal;

            /*Expanding R and using E bit function------------------------------------------------------*/
            uliTemp = (long) 1 << 31;
            for (iLoopA = 1; iLoopA <= 32; iLoopA++)
            {
                if ((uliR & uliTemp) != 0) cBitsTempA[iLoopA] = (char) 1;
                else cBitsTempA[iLoopA] = (char) 0;
                uliTemp = uliTemp >> 1;
            }

            /*Computing Rx=E(R)-------------------------------------------------------------------------*/
            uliRxLeft = 0;
            uliRxRight = 0;
            for (iLoopA = 1; iLoopA <= 24; iLoopA++)
            {
                uliRxLeft = uliRxLeft*2 + cBitsTempA[iEBit[iLoopA]];
                uliRxRight = uliRxRight*2 + cBitsTempA[iEBit[iLoopA + 24]];
            }

            /*Computing K+Rx----------------------------------------------------------------------------*/
            uliPreBoxLeft = uliRxLeft ^ uliKLeft;
            uliPreBoxRight = uliRxRight ^ uliKRight;

            /*Making Boxes of K+Rx----------------------------------------------------------------------*/
            for (iLoopA = 1; iLoopA <= 4; iLoopA++)
            {
                cBoxValues[5 - iLoopA] = (char) (uliPreBoxLeft & 0x3F);
                cBoxValues[9 - iLoopA] = (char) (uliPreBoxRight & 0x3F);
                uliPreBoxLeft = uliPreBoxLeft >> 6;
                uliPreBoxRight = uliPreBoxRight >> 6;
            }

            /*Mapping 6-bit Box Inputs to 4-bit using S-Box Lookups-------------------------------------*/
            uliFinal = 0;
            for (iLoopA = 1; iLoopA <= 8; iLoopA++)
            {
                cBoxValues[iLoopA] = cSBoxLookup(iLoopA, cBoxValues[iLoopA]);
                uliFinal = uliFinal*16 + cBoxValues[iLoopA];
            }

            /*Doing the final permutation of the 32-bit value-------------------------------------------*/
            uliTemp = (long) 1 << 31;
            for (iLoopA = 1; iLoopA <= 32; iLoopA++)
            {
                if ((uliFinal & uliTemp) != 0) cBitsTempA[iLoopA] = (char) 1;
                else cBitsTempA[iLoopA] = (char) 0;
                uliTemp = uliTemp >> 1;
            }

            uliFinal = 0;
            for (iLoopA = 1; iLoopA <= 32; iLoopA++)
            {
                uliFinal = uliFinal*2 + cBitsTempA[iPermFinalF[iLoopA]];
            }
            return uliFinal;
        }


        public string strDESDecrypt(string strSource)
        {
            /*This is the function that decrypts a string---------------------------------------------------*/
            string values = strStringEncode(strSource, false);
            return values;
        }

        public string strDESEncrypt(string strSource)
        {
            /*This is the function that encrypts a string---------------------------------------------------*/
            string values = strStringEncode(strSource, true);
            return values;
        }

        public string strEncrypt(string strPlainPwd)
        {
            var siBitStr = new short[80];
            var siXPose = new short[6];
            char cTemp;
            char cSlider;
            int iPosition;
            short siOrigLen;
            short siBase;
            int iSeed;
            int iPad;
            int iMapIndex;
            int iLoopA;
            int iLoopB;
            String strCipher = string.Empty;

            /*Calculating length seed and padding-------------------------------------------------------*/
            siOrigLen = (short) strPlainPwd.Length;
            if ((siOrigLen < 1) || (siOrigLen > 8)) return "#1";
            iPad = (6 - ((siOrigLen*8)%6))%6;
            //iSeed=(int)Math.floor(64*Math.Random());
            var random = new Random();
            iSeed = (int) Math.Floor((double) 64*random.Next());

            iPosition = 12;
            /*Writing password to bit string from position 12-------------------------------------------*/
            for (iLoopA = 0; iLoopA < siOrigLen; iLoopA++)
            {
                cTemp = strPlainPwd[iLoopA];
                if (cTemp > 255) return "#2";
                cSlider = (char) 256;
                for (iLoopB = 0; iLoopB < 8; iLoopB++)
                {
                    cSlider = (char) (cSlider >> 1);

                    if ((cSlider & cTemp) > 0) siBitStr[iPosition++] = 1;
                    else siBitStr[iPosition++] = 0;
                }
            }
            for (iLoopA = 0; iLoopA < iPad; iLoopA++) siBitStr[iPosition++] = (short) (iLoopA%2);
            /*Writing Seed to bit string at positions 0,1,2 and 6,7,8-----------------------------------*/
            cSlider = (char) 64;
            for (iLoopA = 0; iLoopA < 6; iLoopA++)
            {
                cSlider = (char) (cSlider >> 1);
                if ((cSlider & iSeed) > 0) siBitStr[iLoopA] = 1;
                else siBitStr[iLoopA] = 0;
            }
            siBitStr[6] = siBitStr[3];
            siBitStr[7] = siBitStr[4];
            siBitStr[8] = siBitStr[5];
            siBitStr[9] = 0;
            siBitStr[10] = 0;
            siBitStr[11] = 0;

            /*Writing Padding to bit string at positions 3,4,5------------------------------------------*/
            cSlider = (char) 8;
            for (iLoopA = 0; iLoopA < 3; iLoopA++)
            {
                cSlider = (char) (cSlider >> 1);
                if ((cSlider & iPad) > 0) siBitStr[3 + iLoopA] = 1;
                else siBitStr[3 + iLoopA] = 0;
            }

            /*Our bit string is complete. Proceeding to do bit transposition and also final cipher------*/
            for (iLoopA = 0; iLoopA < ((short) (iPosition/6)); iLoopA++)
            {
                siBase = (short) (iLoopA*6);
                for (iLoopB = 0; iLoopB < 6; iLoopB++) siXPose[iLoopB] = siBitStr[siBase + iLoopB];
                siBitStr[siBase + 0] = siXPose[5];
                siBitStr[siBase + 1] = siXPose[0];
                siBitStr[siBase + 2] = siXPose[2];
                siBitStr[siBase + 3] = siXPose[4];
                siBitStr[siBase + 4] = siXPose[1];
                siBitStr[siBase + 5] = siXPose[3];

                iMapIndex = 0;
                for (iLoopB = 0; iLoopB < 6; iLoopB++) iMapIndex = iMapIndex*2 + siBitStr[iLoopB + siBase];
                if (iLoopA > 1) iMapIndex = ((iMapIndex + iSeed + siJumble[iLoopA - 2]) + 64)%64;
                strCipher += cMap[iMapIndex];
            }

            return strCipher;
        }
    }
}