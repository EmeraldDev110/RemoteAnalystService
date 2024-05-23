using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Ionic.Zip;

namespace RemoteAnalyst.BusinessLogic.Util {
    public static class Helper {
        public const string _mySQLTimeFormat = "yyyy-MM-dd HH:mm:ss";
        public const string _SERVERKEYNAME = "SERVER";
        public const string _PORTKEYNAME = "PORT";
        public const string _DATABASEKEYNAME = "DATABASE";
        public const string _LOCALHOST = "localhost";
        public const string _LOCALPORT = "3306";

        public static string FindKeyName(string connStr, string keyWord) {
            string databaseName = "";
            string[] tempNames = connStr.Split(';');
            foreach (string s in tempNames) {
                if (s.ToUpper().Contains(keyWord)) {
                    databaseName = s.Split('=')[1];
                }
            }
            return databaseName;
        }

        public static string CreateZipFile(string systemSerial, string fileLocation, string zipFileName, string zipLocation) {
            string location = "";
            try {
                string folderName = zipLocation + systemSerial + "\\";

                if (!Directory.Exists(folderName)) {
                    Directory.CreateDirectory(folderName);
                }

                if (File.Exists(folderName + zipFileName)) {
                    File.Delete(folderName + zipFileName);
                }
                location = folderName + zipFileName;

                using (var zip = new ZipFile()) {
                    zip.AddFile(fileLocation, string.Empty); //Need to add string.empty to second parameter to make the file display on top of the zip layer.
                    zip.Save(location);
                }
            }
            catch {
                location = "";
            }

            return location;
        }

        public static DateTime GetLastMonthDate(DateTime startDate) {
            //var weekOfMonth = (startDate.Day + ((int)startDate.DayOfWeek)) / 7 + 1;
            var weekOfMonthCal = startDate;
            var beginningOfMonth = new DateTime(weekOfMonthCal.Year, weekOfMonthCal.Month, 1);
            while (weekOfMonthCal.Date.AddDays(1).DayOfWeek != CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
                weekOfMonthCal = weekOfMonthCal.AddDays(1);

            var weekOfMonth = (int)Math.Truncate((double)weekOfMonthCal.Subtract(beginningOfMonth).TotalDays / 7f) + 1;

            var dayOfWeek = (int)startDate.DayOfWeek;

            var lastMonth = startDate.AddMonths(-1);
            var dt1 = new DateTime(lastMonth.Year, lastMonth.Month, 1, lastMonth.Hour, lastMonth.Minute, lastMonth.Second);
            var day = (weekOfMonth - 1) * 7 + dayOfWeek - (int)dt1.DayOfWeek;
            var newDate = dt1.AddDays(day >= 0 ? day : day + 7);

            return newDate;
        }

        public static int GetWeekOfMonth(DateTime dateTime) {
            DayOfWeek dayOfWeek = dateTime.DayOfWeek;
            DateTime dayStep = new DateTime(dateTime.Year, dateTime.Month, 1);
            int returnValue = 0;

            while (dayStep <= dateTime) {
                if (dayStep.DayOfWeek == dayOfWeek) {
                    returnValue++;
                }

                dayStep = dayStep.AddDays(1);
            }

            return returnValue;
        }

        public static DateTime RoundUp(DateTime dt, TimeSpan d) {
            return new DateTime(((dt.Ticks + d.Ticks - 1) / d.Ticks) * d.Ticks);
        }
    }
}