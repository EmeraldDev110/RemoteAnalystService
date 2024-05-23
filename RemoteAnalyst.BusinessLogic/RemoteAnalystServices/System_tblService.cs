using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.Repository.Repositories;

namespace RemoteAnalyst.BusinessLogic.RemoteAnalystServices
{
    public class System_tblService
    {
        private readonly string _connectionString;

        public System_tblService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Dictionary<string, string> GetLicenseDateFor()
        {
            SystemRepository systemTable = new SystemRepository();
            var dicLicense = new Dictionary<string, string>();

            dicLicense = systemTable.GetLicenseDate();

            return dicLicense;
        }

        public int GetCompanyIDFor(string systemSerial)
        {
            var systemTable = new SystemRepository();
            int returnValue = systemTable.GetCompanyID(systemSerial);

            return returnValue;
        }

        public string GetCompanyNameFor(string systemSerial)
        {
            var systemTable = new SystemRepository();
            string returnValue = systemTable.GetCompanyName(systemSerial);

            return returnValue;
        }

        public int GetRetentionDayFor(string systemSerial)
        {
            var systemTable = new SystemRepository();
            int returnValue = systemTable.GetRetentionDay(systemSerial);
            return returnValue;
        }

        public IDictionary<string, string> GetEndDateFor(string systemSerial)
        {
            var systemTable = new SystemRepository();
            IDictionary<string, string> systemTblServices = systemTable.GetEndDate(systemSerial);
            return systemTblServices;
        }

        public string GetSystemNameFor(string systemSerial)
        {
            var systemTable = new SystemRepository();
            string returnValue = systemTable.GetSystemName(systemSerial);

            return returnValue;
        }

        public string GetMeasFHFor(string systemSerial) {
            var systemTable = new SystemRepository();
            string returnValue = systemTable.GetMeasFH(systemSerial);

            if (returnValue.Length.Equals(0))
                returnValue = "H06";
            return returnValue;
        }

        public bool GetAttachmentInEmailFor(string systemSerial)
        {
            var systemTable = new SystemRepository();
            bool returnValue = systemTable.GetAttachmentInEmail(systemSerial);

            return returnValue;
        }

        public List<string> GetExpiredSystemFor(bool isLocalAnalyst)
        {
            var systemTable = new SystemRepository();
            Dictionary<string, string> expiredSystemEncrypt = systemTable.GetExpiredSystem();
            
            var expiredSystem = new List<string>();

            foreach (KeyValuePair<string, string> s in expiredSystemEncrypt)
            {
                //Decrypt the End date.
                var decrypt = new Decrypt();
                string decryptInfo = decrypt.strDESDecrypt(s.Value);
                string decryptDate = decryptInfo.Split(' ')[1].Trim();

                try
                {
                    if (isLocalAnalyst) {
                        if (Convert.ToDateTime(decryptDate).AddYears(1) < DateTime.Today) {
                            expiredSystem.Add(s.Key);
                        }
                    }
                    else {
                        if (Convert.ToDateTime(decryptDate) < DateTime.Today) {
                            expiredSystem.Add(s.Key);
                        }
                    }
                }
                catch
                {
                }
            }

            return expiredSystem;
        }

        public List<string> GetActiveSystemFor(bool isLocalAnalyst = false) {
            var systemTable = new SystemRepository();
            Dictionary<string, string> expiredSystemEncrypt = systemTable.GetExpiredSystem(isLocalAnalyst);
            var activeSystemList = new List<string>();

            foreach (KeyValuePair<string, string> s in expiredSystemEncrypt) {
                var systemKey = s.Value;
                //Decrypt the End date.
                var decrypt = new Decrypt();
                string decryptInfo = decrypt.strDESDecrypt(systemKey);
                string decryptDate = decryptInfo.Split(' ')[1].Trim();

                try {
                    if (Convert.ToDateTime(decryptDate) >= DateTime.Today) {
                        activeSystemList.Add(s.Key);
                    }
                } catch { }
            }

            return activeSystemList;
        }
        
        public bool isProcessDirectlySystemFor(string systemSerial) {
            var systemTable = new SystemRepository();
            bool isProcessDirectlySystem = systemTable.isProcessDirectlySystem(systemSerial);

            return isProcessDirectlySystem;
        }
        
        public bool AllowOverlappingDataFor(string systemSerial) {
            var systemTable = new SystemRepository();
            bool overlapping = systemTable.AllowOverlappingData(systemSerial);
#if (DEBUG)
            return overlapping;
#else
            return overlapping;
#endif
        }

        public int GetTimeZoneFor(string systemSerial) {
            var systemTable = new SystemRepository();
            int timeZoneIndex = systemTable.GetTimeZone(systemSerial);

            return timeZoneIndex;
        }

        public string GetLongDatePatternFor(string systemSerial) {
            var systemTable = new SystemRepository();
            var countryCode = systemTable.GetCountryCode(systemSerial);
            
            var ci = new CultureInfo(countryCode);
            var dateFormatString = ci.DateTimeFormat.LongDatePattern;
            return dateFormatString;
        }

        public string GetMonthDayPatternFor(string systemSerial) {
            var systemTable = new SystemRepository();
            var countryCode = systemTable.GetCountryCode(systemSerial);

            var ci = new CultureInfo(countryCode);
            var dateFormatString = ci.DateTimeFormat.MonthDayPattern;
            return dateFormatString;
        }

        public int GetArchiveRetensionValueFor(string systemSerial) {
            var systemTable = new SystemRepository();
            int archiveRetention = systemTable.GetArchiveRetensionValue(systemSerial);
            return archiveRetention;
        }

        public int GetTrendMonthsFor(string systemSerial) {
            var systemTable = new SystemRepository();
            int trendMonths = systemTable.GetTrendMonth(systemSerial);
            return trendMonths;
        }

        public bool IsNTSSystemFor(string systemSerial) {
            var systemTable = new SystemRepository();
            var isNTS = systemTable.IsNTSSystem(systemSerial);
            return isNTS;
        }

        public DataTable GetToleranceFor(string systemSerial) {
            var systemTable = new SystemRepository();
            var tolerance = systemTable.GetTolerance(systemSerial);

            return tolerance;
        }

		public Dictionary<string, string> GetSystemSerialAndCompanyName() {
			var systemTable = new SystemRepository();
			var systemSerialAndNameDataTable = systemTable.GetAllCompanySystemSerialAndName();
			var systemSerialAndNameDic = new Dictionary<string, string>();
			foreach(DataRow system in systemSerialAndNameDataTable.Rows) {
				systemSerialAndNameDic.Add((system["SystemSerial"] == DBNull.Value) ? "" : system["SystemSerial"].ToString(), (system["CompanyName"] == DBNull.Value) ? "" : system["CompanyName"].ToString());
			}
			return systemSerialAndNameDic;
		}

        public int GetPerSystemLoadLimit(string systemSerial)
        {
            var systemTable = new SystemRepository();
            int returnValue = systemTable.GetLoadLimit(systemSerial);
            return returnValue;
        }

    }
}