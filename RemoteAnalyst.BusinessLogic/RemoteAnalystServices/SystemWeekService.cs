using System;
using System.Collections.Generic;
using RemoteAnalyst.Repository.Concrete.RemoteAnalystdb;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices {
    public class SystemWeekService {
        private readonly string _systemSerial;
        private readonly string _mainConnectionString;

        public SystemWeekService(string systemSerial, string mainConnectionString) {
            _systemSerial = systemSerial;
            _mainConnectionString = mainConnectionString;
        }

        public Dictionary<int, SystemWeekInfo> GetSystemWeek() {

            var systemWeek = new SystemWeek(_mainConnectionString);
            var systemWeekData = systemWeek.GetSystemWeek(_systemSerial);

            var systemWeekInfo = new Dictionary<int, SystemWeekInfo>();
            if (systemWeekData.Rows.Count > 0) {
                #region Sunday

                systemWeekInfo.Add(0, new SystemWeekInfo {
                    IsWeekday = Convert.ToBoolean(systemWeekData.Rows[0]["Sunday"]),
                    Hour00 = Convert.ToInt32(systemWeekData.Rows[0]["H00"]),
                    Hour01 = Convert.ToInt32(systemWeekData.Rows[0]["H01"]),
                    Hour02 = Convert.ToInt32(systemWeekData.Rows[0]["H02"]),
                    Hour03 = Convert.ToInt32(systemWeekData.Rows[0]["H03"]),
                    Hour04 = Convert.ToInt32(systemWeekData.Rows[0]["H04"]),
                    Hour05 = Convert.ToInt32(systemWeekData.Rows[0]["H05"]),
                    Hour06 = Convert.ToInt32(systemWeekData.Rows[0]["H06"]),
                    Hour07 = Convert.ToInt32(systemWeekData.Rows[0]["H07"]),
                    Hour08 = Convert.ToInt32(systemWeekData.Rows[0]["H08"]),
                    Hour09 = Convert.ToInt32(systemWeekData.Rows[0]["H09"]),
                    Hour10 = Convert.ToInt32(systemWeekData.Rows[0]["H10"]),
                    Hour11 = Convert.ToInt32(systemWeekData.Rows[0]["H11"]),
                    Hour12 = Convert.ToInt32(systemWeekData.Rows[0]["H12"]),
                    Hour13 = Convert.ToInt32(systemWeekData.Rows[0]["H13"]),
                    Hour14 = Convert.ToInt32(systemWeekData.Rows[0]["H14"]),
                    Hour15 = Convert.ToInt32(systemWeekData.Rows[0]["H15"]),
                    Hour16 = Convert.ToInt32(systemWeekData.Rows[0]["H16"]),
                    Hour17 = Convert.ToInt32(systemWeekData.Rows[0]["H17"]),
                    Hour18 = Convert.ToInt32(systemWeekData.Rows[0]["H18"]),
                    Hour19 = Convert.ToInt32(systemWeekData.Rows[0]["H19"]),
                    Hour20 = Convert.ToInt32(systemWeekData.Rows[0]["H20"]),
                    Hour21 = Convert.ToInt32(systemWeekData.Rows[0]["H21"]),
                    Hour22 = Convert.ToInt32(systemWeekData.Rows[0]["H22"]),
                    Hour23 = Convert.ToInt32(systemWeekData.Rows[0]["H23"])
                });

                #endregion

                #region Monday

                systemWeekInfo.Add(1, new SystemWeekInfo {
                    IsWeekday = Convert.ToBoolean(systemWeekData.Rows[0]["Monday"]),
                    Hour00 = Convert.ToInt32(systemWeekData.Rows[0]["H00"]),
                    Hour01 = Convert.ToInt32(systemWeekData.Rows[0]["H01"]),
                    Hour02 = Convert.ToInt32(systemWeekData.Rows[0]["H02"]),
                    Hour03 = Convert.ToInt32(systemWeekData.Rows[0]["H03"]),
                    Hour04 = Convert.ToInt32(systemWeekData.Rows[0]["H04"]),
                    Hour05 = Convert.ToInt32(systemWeekData.Rows[0]["H05"]),
                    Hour06 = Convert.ToInt32(systemWeekData.Rows[0]["H06"]),
                    Hour07 = Convert.ToInt32(systemWeekData.Rows[0]["H07"]),
                    Hour08 = Convert.ToInt32(systemWeekData.Rows[0]["H08"]),
                    Hour09 = Convert.ToInt32(systemWeekData.Rows[0]["H09"]),
                    Hour10 = Convert.ToInt32(systemWeekData.Rows[0]["H10"]),
                    Hour11 = Convert.ToInt32(systemWeekData.Rows[0]["H11"]),
                    Hour12 = Convert.ToInt32(systemWeekData.Rows[0]["H12"]),
                    Hour13 = Convert.ToInt32(systemWeekData.Rows[0]["H13"]),
                    Hour14 = Convert.ToInt32(systemWeekData.Rows[0]["H14"]),
                    Hour15 = Convert.ToInt32(systemWeekData.Rows[0]["H15"]),
                    Hour16 = Convert.ToInt32(systemWeekData.Rows[0]["H16"]),
                    Hour17 = Convert.ToInt32(systemWeekData.Rows[0]["H17"]),
                    Hour18 = Convert.ToInt32(systemWeekData.Rows[0]["H18"]),
                    Hour19 = Convert.ToInt32(systemWeekData.Rows[0]["H19"]),
                    Hour20 = Convert.ToInt32(systemWeekData.Rows[0]["H20"]),
                    Hour21 = Convert.ToInt32(systemWeekData.Rows[0]["H21"]),
                    Hour22 = Convert.ToInt32(systemWeekData.Rows[0]["H22"]),
                    Hour23 = Convert.ToInt32(systemWeekData.Rows[0]["H23"])
                });

                #endregion

                #region Tuesday

                systemWeekInfo.Add(2, new SystemWeekInfo {
                    IsWeekday = Convert.ToBoolean(systemWeekData.Rows[0]["Tuesday"]),
                    Hour00 = Convert.ToInt32(systemWeekData.Rows[0]["H00"]),
                    Hour01 = Convert.ToInt32(systemWeekData.Rows[0]["H01"]),
                    Hour02 = Convert.ToInt32(systemWeekData.Rows[0]["H02"]),
                    Hour03 = Convert.ToInt32(systemWeekData.Rows[0]["H03"]),
                    Hour04 = Convert.ToInt32(systemWeekData.Rows[0]["H04"]),
                    Hour05 = Convert.ToInt32(systemWeekData.Rows[0]["H05"]),
                    Hour06 = Convert.ToInt32(systemWeekData.Rows[0]["H06"]),
                    Hour07 = Convert.ToInt32(systemWeekData.Rows[0]["H07"]),
                    Hour08 = Convert.ToInt32(systemWeekData.Rows[0]["H08"]),
                    Hour09 = Convert.ToInt32(systemWeekData.Rows[0]["H09"]),
                    Hour10 = Convert.ToInt32(systemWeekData.Rows[0]["H10"]),
                    Hour11 = Convert.ToInt32(systemWeekData.Rows[0]["H11"]),
                    Hour12 = Convert.ToInt32(systemWeekData.Rows[0]["H12"]),
                    Hour13 = Convert.ToInt32(systemWeekData.Rows[0]["H13"]),
                    Hour14 = Convert.ToInt32(systemWeekData.Rows[0]["H14"]),
                    Hour15 = Convert.ToInt32(systemWeekData.Rows[0]["H15"]),
                    Hour16 = Convert.ToInt32(systemWeekData.Rows[0]["H16"]),
                    Hour17 = Convert.ToInt32(systemWeekData.Rows[0]["H17"]),
                    Hour18 = Convert.ToInt32(systemWeekData.Rows[0]["H18"]),
                    Hour19 = Convert.ToInt32(systemWeekData.Rows[0]["H19"]),
                    Hour20 = Convert.ToInt32(systemWeekData.Rows[0]["H20"]),
                    Hour21 = Convert.ToInt32(systemWeekData.Rows[0]["H21"]),
                    Hour22 = Convert.ToInt32(systemWeekData.Rows[0]["H22"]),
                    Hour23 = Convert.ToInt32(systemWeekData.Rows[0]["H23"])
                });

                #endregion

                #region Wednesday

                systemWeekInfo.Add(3, new SystemWeekInfo {
                    IsWeekday = Convert.ToBoolean(systemWeekData.Rows[0]["Wednesday"]),
                    Hour00 = Convert.ToInt32(systemWeekData.Rows[0]["H00"]),
                    Hour01 = Convert.ToInt32(systemWeekData.Rows[0]["H01"]),
                    Hour02 = Convert.ToInt32(systemWeekData.Rows[0]["H02"]),
                    Hour03 = Convert.ToInt32(systemWeekData.Rows[0]["H03"]),
                    Hour04 = Convert.ToInt32(systemWeekData.Rows[0]["H04"]),
                    Hour05 = Convert.ToInt32(systemWeekData.Rows[0]["H05"]),
                    Hour06 = Convert.ToInt32(systemWeekData.Rows[0]["H06"]),
                    Hour07 = Convert.ToInt32(systemWeekData.Rows[0]["H07"]),
                    Hour08 = Convert.ToInt32(systemWeekData.Rows[0]["H08"]),
                    Hour09 = Convert.ToInt32(systemWeekData.Rows[0]["H09"]),
                    Hour10 = Convert.ToInt32(systemWeekData.Rows[0]["H10"]),
                    Hour11 = Convert.ToInt32(systemWeekData.Rows[0]["H11"]),
                    Hour12 = Convert.ToInt32(systemWeekData.Rows[0]["H12"]),
                    Hour13 = Convert.ToInt32(systemWeekData.Rows[0]["H13"]),
                    Hour14 = Convert.ToInt32(systemWeekData.Rows[0]["H14"]),
                    Hour15 = Convert.ToInt32(systemWeekData.Rows[0]["H15"]),
                    Hour16 = Convert.ToInt32(systemWeekData.Rows[0]["H16"]),
                    Hour17 = Convert.ToInt32(systemWeekData.Rows[0]["H17"]),
                    Hour18 = Convert.ToInt32(systemWeekData.Rows[0]["H18"]),
                    Hour19 = Convert.ToInt32(systemWeekData.Rows[0]["H19"]),
                    Hour20 = Convert.ToInt32(systemWeekData.Rows[0]["H20"]),
                    Hour21 = Convert.ToInt32(systemWeekData.Rows[0]["H21"]),
                    Hour22 = Convert.ToInt32(systemWeekData.Rows[0]["H22"]),
                    Hour23 = Convert.ToInt32(systemWeekData.Rows[0]["H23"])
                });

                #endregion

                #region Thursday

                systemWeekInfo.Add(4, new SystemWeekInfo {
                    IsWeekday = Convert.ToBoolean(systemWeekData.Rows[0]["Thursday"]),
                    Hour00 = Convert.ToInt32(systemWeekData.Rows[0]["H00"]),
                    Hour01 = Convert.ToInt32(systemWeekData.Rows[0]["H01"]),
                    Hour02 = Convert.ToInt32(systemWeekData.Rows[0]["H02"]),
                    Hour03 = Convert.ToInt32(systemWeekData.Rows[0]["H03"]),
                    Hour04 = Convert.ToInt32(systemWeekData.Rows[0]["H04"]),
                    Hour05 = Convert.ToInt32(systemWeekData.Rows[0]["H05"]),
                    Hour06 = Convert.ToInt32(systemWeekData.Rows[0]["H06"]),
                    Hour07 = Convert.ToInt32(systemWeekData.Rows[0]["H07"]),
                    Hour08 = Convert.ToInt32(systemWeekData.Rows[0]["H08"]),
                    Hour09 = Convert.ToInt32(systemWeekData.Rows[0]["H09"]),
                    Hour10 = Convert.ToInt32(systemWeekData.Rows[0]["H10"]),
                    Hour11 = Convert.ToInt32(systemWeekData.Rows[0]["H11"]),
                    Hour12 = Convert.ToInt32(systemWeekData.Rows[0]["H12"]),
                    Hour13 = Convert.ToInt32(systemWeekData.Rows[0]["H13"]),
                    Hour14 = Convert.ToInt32(systemWeekData.Rows[0]["H14"]),
                    Hour15 = Convert.ToInt32(systemWeekData.Rows[0]["H15"]),
                    Hour16 = Convert.ToInt32(systemWeekData.Rows[0]["H16"]),
                    Hour17 = Convert.ToInt32(systemWeekData.Rows[0]["H17"]),
                    Hour18 = Convert.ToInt32(systemWeekData.Rows[0]["H18"]),
                    Hour19 = Convert.ToInt32(systemWeekData.Rows[0]["H19"]),
                    Hour20 = Convert.ToInt32(systemWeekData.Rows[0]["H20"]),
                    Hour21 = Convert.ToInt32(systemWeekData.Rows[0]["H21"]),
                    Hour22 = Convert.ToInt32(systemWeekData.Rows[0]["H22"]),
                    Hour23 = Convert.ToInt32(systemWeekData.Rows[0]["H23"])
                });

                #endregion

                #region Friday

                systemWeekInfo.Add(5, new SystemWeekInfo {
                    IsWeekday = Convert.ToBoolean(systemWeekData.Rows[0]["Friday"]),
                    Hour00 = Convert.ToInt32(systemWeekData.Rows[0]["H00"]),
                    Hour01 = Convert.ToInt32(systemWeekData.Rows[0]["H01"]),
                    Hour02 = Convert.ToInt32(systemWeekData.Rows[0]["H02"]),
                    Hour03 = Convert.ToInt32(systemWeekData.Rows[0]["H03"]),
                    Hour04 = Convert.ToInt32(systemWeekData.Rows[0]["H04"]),
                    Hour05 = Convert.ToInt32(systemWeekData.Rows[0]["H05"]),
                    Hour06 = Convert.ToInt32(systemWeekData.Rows[0]["H06"]),
                    Hour07 = Convert.ToInt32(systemWeekData.Rows[0]["H07"]),
                    Hour08 = Convert.ToInt32(systemWeekData.Rows[0]["H08"]),
                    Hour09 = Convert.ToInt32(systemWeekData.Rows[0]["H09"]),
                    Hour10 = Convert.ToInt32(systemWeekData.Rows[0]["H10"]),
                    Hour11 = Convert.ToInt32(systemWeekData.Rows[0]["H11"]),
                    Hour12 = Convert.ToInt32(systemWeekData.Rows[0]["H12"]),
                    Hour13 = Convert.ToInt32(systemWeekData.Rows[0]["H13"]),
                    Hour14 = Convert.ToInt32(systemWeekData.Rows[0]["H14"]),
                    Hour15 = Convert.ToInt32(systemWeekData.Rows[0]["H15"]),
                    Hour16 = Convert.ToInt32(systemWeekData.Rows[0]["H16"]),
                    Hour17 = Convert.ToInt32(systemWeekData.Rows[0]["H17"]),
                    Hour18 = Convert.ToInt32(systemWeekData.Rows[0]["H18"]),
                    Hour19 = Convert.ToInt32(systemWeekData.Rows[0]["H19"]),
                    Hour20 = Convert.ToInt32(systemWeekData.Rows[0]["H20"]),
                    Hour21 = Convert.ToInt32(systemWeekData.Rows[0]["H21"]),
                    Hour22 = Convert.ToInt32(systemWeekData.Rows[0]["H22"]),
                    Hour23 = Convert.ToInt32(systemWeekData.Rows[0]["H23"])
                });

                #endregion

                #region Saturday

                systemWeekInfo.Add(6, new SystemWeekInfo {
                    IsWeekday = Convert.ToBoolean(systemWeekData.Rows[0]["Saturday"]),
                    Hour00 = Convert.ToInt32(systemWeekData.Rows[0]["H00"]),
                    Hour01 = Convert.ToInt32(systemWeekData.Rows[0]["H01"]),
                    Hour02 = Convert.ToInt32(systemWeekData.Rows[0]["H02"]),
                    Hour03 = Convert.ToInt32(systemWeekData.Rows[0]["H03"]),
                    Hour04 = Convert.ToInt32(systemWeekData.Rows[0]["H04"]),
                    Hour05 = Convert.ToInt32(systemWeekData.Rows[0]["H05"]),
                    Hour06 = Convert.ToInt32(systemWeekData.Rows[0]["H06"]),
                    Hour07 = Convert.ToInt32(systemWeekData.Rows[0]["H07"]),
                    Hour08 = Convert.ToInt32(systemWeekData.Rows[0]["H08"]),
                    Hour09 = Convert.ToInt32(systemWeekData.Rows[0]["H09"]),
                    Hour10 = Convert.ToInt32(systemWeekData.Rows[0]["H10"]),
                    Hour11 = Convert.ToInt32(systemWeekData.Rows[0]["H11"]),
                    Hour12 = Convert.ToInt32(systemWeekData.Rows[0]["H12"]),
                    Hour13 = Convert.ToInt32(systemWeekData.Rows[0]["H13"]),
                    Hour14 = Convert.ToInt32(systemWeekData.Rows[0]["H14"]),
                    Hour15 = Convert.ToInt32(systemWeekData.Rows[0]["H15"]),
                    Hour16 = Convert.ToInt32(systemWeekData.Rows[0]["H16"]),
                    Hour17 = Convert.ToInt32(systemWeekData.Rows[0]["H17"]),
                    Hour18 = Convert.ToInt32(systemWeekData.Rows[0]["H18"]),
                    Hour19 = Convert.ToInt32(systemWeekData.Rows[0]["H19"]),
                    Hour20 = Convert.ToInt32(systemWeekData.Rows[0]["H20"]),
                    Hour21 = Convert.ToInt32(systemWeekData.Rows[0]["H21"]),
                    Hour22 = Convert.ToInt32(systemWeekData.Rows[0]["H22"]),
                    Hour23 = Convert.ToInt32(systemWeekData.Rows[0]["H23"])
                });

                #endregion
            }
            else {
                //Create dummay vaules.
                #region Sunday

                systemWeekInfo.Add(0, new SystemWeekInfo {
                    IsWeekday = false,
                    Hour00 = 0,
                    Hour01 = 0,
                    Hour02 = 0,
                    Hour03 = 0,
                    Hour04 = 0,
                    Hour05 = 0,
                    Hour06 = 0,
                    Hour07 = 0,
                    Hour08 = 0,
                    Hour09 = 0,
                    Hour10 = 0,
                    Hour11 = 0,
                    Hour12 = 0,
                    Hour13 = 0,
                    Hour14 = 0,
                    Hour15 = 0,
                    Hour16 = 0,
                    Hour17 = 0,
                    Hour18 = 0,
                    Hour19 = 0,
                    Hour20 = 0,
                    Hour21 = 0,
                    Hour22 = 0,
                    Hour23 = 0
                });

                #endregion

                #region Monday

                systemWeekInfo.Add(1, new SystemWeekInfo {
                    IsWeekday = true,
                    Hour00 = 0,
                    Hour01 = 0,
                    Hour02 = 0,
                    Hour03 = 0,
                    Hour04 = 0,
                    Hour05 = 0,
                    Hour06 = 0,
                    Hour07 = 0,
                    Hour08 = 0,
                    Hour09 = 0,
                    Hour10 = 0,
                    Hour11 = 0,
                    Hour12 = 0,
                    Hour13 = 0,
                    Hour14 = 0,
                    Hour15 = 0,
                    Hour16 = 0,
                    Hour17 = 0,
                    Hour18 = 0,
                    Hour19 = 0,
                    Hour20 = 0,
                    Hour21 = 0,
                    Hour22 = 0,
                    Hour23 = 0
                });

                #endregion

                #region Tuesday

                systemWeekInfo.Add(2, new SystemWeekInfo {
                    IsWeekday = true,
                    Hour00 = 0,
                    Hour01 = 0,
                    Hour02 = 0,
                    Hour03 = 0,
                    Hour04 = 0,
                    Hour05 = 0,
                    Hour06 = 0,
                    Hour07 = 0,
                    Hour08 = 0,
                    Hour09 = 0,
                    Hour10 = 0,
                    Hour11 = 0,
                    Hour12 = 0,
                    Hour13 = 0,
                    Hour14 = 0,
                    Hour15 = 0,
                    Hour16 = 0,
                    Hour17 = 0,
                    Hour18 = 0,
                    Hour19 = 0,
                    Hour20 = 0,
                    Hour21 = 0,
                    Hour22 = 0,
                    Hour23 = 0
                });

                #endregion

                #region Wednesday

                systemWeekInfo.Add(3, new SystemWeekInfo {
                    IsWeekday = true,
                    Hour00 = 0,
                    Hour01 = 0,
                    Hour02 = 0,
                    Hour03 = 0,
                    Hour04 = 0,
                    Hour05 = 0,
                    Hour06 = 0,
                    Hour07 = 0,
                    Hour08 = 0,
                    Hour09 = 0,
                    Hour10 = 0,
                    Hour11 = 0,
                    Hour12 = 0,
                    Hour13 = 0,
                    Hour14 = 0,
                    Hour15 = 0,
                    Hour16 = 0,
                    Hour17 = 0,
                    Hour18 = 0,
                    Hour19 = 0,
                    Hour20 = 0,
                    Hour21 = 0,
                    Hour22 = 0,
                    Hour23 = 0
                });

                #endregion

                #region Thursday

                systemWeekInfo.Add(4, new SystemWeekInfo {
                    IsWeekday = true,
                    Hour00 = 0,
                    Hour01 = 0,
                    Hour02 = 0,
                    Hour03 = 0,
                    Hour04 = 0,
                    Hour05 = 0,
                    Hour06 = 0,
                    Hour07 = 0,
                    Hour08 = 0,
                    Hour09 = 0,
                    Hour10 = 0,
                    Hour11 = 0,
                    Hour12 = 0,
                    Hour13 = 0,
                    Hour14 = 0,
                    Hour15 = 0,
                    Hour16 = 0,
                    Hour17 = 0,
                    Hour18 = 0,
                    Hour19 = 0,
                    Hour20 = 0,
                    Hour21 = 0,
                    Hour22 = 0,
                    Hour23 = 0
                });

                #endregion

                #region Friday

                systemWeekInfo.Add(5, new SystemWeekInfo {
                    IsWeekday = true,
                    Hour00 = 0,
                    Hour01 = 0,
                    Hour02 = 0,
                    Hour03 = 0,
                    Hour04 = 0,
                    Hour05 = 0,
                    Hour06 = 0,
                    Hour07 = 0,
                    Hour08 = 0,
                    Hour09 = 0,
                    Hour10 = 0,
                    Hour11 = 0,
                    Hour12 = 0,
                    Hour13 = 0,
                    Hour14 = 0,
                    Hour15 = 0,
                    Hour16 = 0,
                    Hour17 = 0,
                    Hour18 = 0,
                    Hour19 = 0,
                    Hour20 = 0,
                    Hour21 = 0,
                    Hour22 = 0,
                    Hour23 = 0
                });

                #endregion

                #region Saturday

                systemWeekInfo.Add(6, new SystemWeekInfo {
                    IsWeekday = false,
                    Hour00 = 0,
                    Hour01 = 0,
                    Hour02 = 0,
                    Hour03 = 0,
                    Hour04 = 0,
                    Hour05 = 0,
                    Hour06 = 0,
                    Hour07 = 0,
                    Hour08 = 0,
                    Hour09 = 0,
                    Hour10 = 0,
                    Hour11 = 0,
                    Hour12 = 0,
                    Hour13 = 0,
                    Hour14 = 0,
                    Hour15 = 0,
                    Hour16 = 0,
                    Hour17 = 0,
                    Hour18 = 0,
                    Hour19 = 0,
                    Hour20 = 0,
                    Hour21 = 0,
                    Hour22 = 0,
                    Hour23 = 0
                });

                #endregion
            }
            return systemWeekInfo;
        }
    }

    public class SystemWeekInfo {
        public bool IsWeekday { get; set; }
        public int Hour00 { get; set; }
        public int Hour01 { get; set; }
        public int Hour02 { get; set; }
        public int Hour03 { get; set; }
        public int Hour04 { get; set; }
        public int Hour05 { get; set; }
        public int Hour06 { get; set; }
        public int Hour07 { get; set; }
        public int Hour08 { get; set; }
        public int Hour09 { get; set; }
        public int Hour10 { get; set; }
        public int Hour11 { get; set; }
        public int Hour12 { get; set; }
        public int Hour13 { get; set; }
        public int Hour14 { get; set; }
        public int Hour15 { get; set; }
        public int Hour16 { get; set; }
        public int Hour17 { get; set; }
        public int Hour18 { get; set; }
        public int Hour19 { get; set; }
        public int Hour20 { get; set; }
        public int Hour21 { get; set; }
        public int Hour22 { get; set; }
        public int Hour23 { get; set; }
    }
}
