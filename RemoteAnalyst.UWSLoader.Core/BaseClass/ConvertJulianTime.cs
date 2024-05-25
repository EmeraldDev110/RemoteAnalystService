using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteAnalyst.UWSLoader.Core.BaseClass {
    class ConvertJulianTime {
        private const double SecsPerDay = 86400;
        private const double MicSPerDay = SecsPerDay * 1000000;
        private const double MaxDateSerial = 2447127.0D;
        private const double ODBZeroMS = 211592606400000.0D;
        private const double ODBZeroMicS = 2.115926064E+17;
        private const double ODBZeroTick = 21159260640000.0D;
        private const double ODBZeroDateSerial = 33970;
        private const double ODBZeroSec = 211592606400.0D;
        private const double ODBZeroDays = ODBZeroSec / 86400.0D;
        private const double WinZeroMicS = (ODBZeroDays - ODBZeroDateSerial) * MicSPerDay;

        public DateTime OBDTimeStampToDBDate(long obdTimeStamp) {
            double odDate;
            DateTime timeStamp;
            double cjtimeStamp = CJTimestamp(obdTimeStamp);

            odDate = (cjtimeStamp - ODBZeroMicS) / MicSPerDay + ODBZeroDateSerial;
            if (odDate < ODBZeroDateSerial) {
                odDate = ODBZeroDateSerial;
            }
            else if (odDate > MaxDateSerial) {
                odDate = MaxDateSerial;
            }

            timeStamp = DateTime.FromOADate(odDate);
            if (timeStamp.Millisecond >= 500) {
                timeStamp = timeStamp.AddSeconds(1);
            }

            return timeStamp;
        }

        public double CJTimestamp(long obdTimeStamp) {
            double cjTimeStamp = 0;

            if (obdTimeStamp < ODBZeroDateSerial) // ODBTimestamp
            {
                cjTimeStamp = obdTimeStamp * 1000000 + ODBZeroMicS;
            }
            else if (obdTimeStamp < ODBZeroDays) // Windows Date/Time:  Days since 1/1/1900
            {
                cjTimeStamp = obdTimeStamp * MicSPerDay + WinZeroMicS;
            }
            else if (obdTimeStamp < ODBZeroSec) // ODBTimestamp:  Secs since 1/1/1993
            {
                cjTimeStamp = obdTimeStamp * 1000000 + ODBZeroMicS;
            }
            else if (obdTimeStamp < ODBZeroTick) // Juliantimestamp in secs:  Secs since ca 4700 BC
            {
                cjTimeStamp = obdTimeStamp * 1000000;
            }
            else if (obdTimeStamp < ODBZeroMS) // Juliantimestamp in ticks: Ticks since ca 4700 BC
            {
                cjTimeStamp = obdTimeStamp * 10000;
            }
            else if (obdTimeStamp < ODBZeroMicS) // Juliantimestmp in MS:  MS since ca 4700 BC
            {
                cjTimeStamp = obdTimeStamp * 1000;
            }

            return cjTimeStamp;
        }

        public int JulianTimeStampToOBDTimeStamp(long tempString) {
            int obdTimeStamp = 0;
            const Decimal Zerocur = 21159260640000M;
            obdTimeStamp = Convert.ToInt32((tempString - Zerocur) * 0.01M + 0.5M);

            return obdTimeStamp;
        }
    }
}
