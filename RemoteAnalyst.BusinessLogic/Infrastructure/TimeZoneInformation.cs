using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace RemoteAnalyst.BusinessLogic.Infrastructure
{
    public class TimeZoneInformation
    {
        private static TimeZoneInformation[] s_zones;
        private static readonly object s_lockZones = new object();
        private string m_daylightName;
        private string m_displayName;
        private int m_index;
        private string m_name;
        private string m_standardName;
        private TZI m_tzi;

        /// <summary>
        /// Get the currently selected time zone
        /// </summary>
        public static TimeZoneInformation CurrentTimeZone
        {
            get
            {
                // The currently selected time zone information can
                // be retrieved using the Win32 GetTimeZoneInformation call,
                // but it only gives us names, offsets and dates - crucially,
                // not the Index.

                TIME_ZONE_INFORMATION tziNative;
                TimeZoneInformation[] zones = EnumZones();

                NativeMethods.GetTimeZoneInformation(out tziNative);

                // Getting the identity is tricky; the best we can do
                // is a match on the properties.

                // If the OS 'Automatically adjust clock for daylight saving changes' checkbox
                // is unchecked, the structure returned by GetTimeZoneInformation has
                // the DaylightBias and DaylightName members set the same as the corresponding
                // Standard members. Therefore we check against both values in case this has
                // been done.

                for (int idx = 0; idx < zones.Length; ++idx)
                {
                    if (zones[idx].m_standardName == tziNative.StandardName &&
                        (zones[idx].m_daylightName == tziNative.DaylightName ||
                         zones[idx].m_standardName == tziNative.DaylightName))
                    {
                        return zones[idx];
                    }
                }

                return null;
            }
        }


        /// <summary>
        /// The zone's name.
        /// </summary>
        public string Name
        {
            get { return m_name; }
        }

        /// <summary>
        /// The zone's display name, e.g. '(GMT) Greenwich Mean Time : Dublin, Edinburgh, Lisbon, London'.
        /// </summary>
        public string DisplayName
        {
            get { return m_displayName; }
        }

        /// <summary>
        /// The zone's index. No obvious pattern.
        /// </summary>
        public int Index
        {
            get { return m_index; }
        }

        /// <summary>
        /// The zone's name during 'standard' time (not daylight savings).
        /// </summary>
        public string StandardName
        {
            get { return m_standardName; }
        }

        /// <summary>
        /// The zone's name during daylight savings time.
        /// </summary>
        public string DaylightName
        {
            get { return m_daylightName; }
        }

        /// <summary>
        /// The offset from UTC. Local = UTC + Bias.
        /// </summary>
        public int Bias
        {
            // Biases in the registry are defined as UTC = local + bias
            // We return as Local = UTC + bias
            get { return -m_tzi.bias; }
        }

        /// <summary>
        /// The offset from UTC during standard time.
        /// </summary>
        public int StandardBias
        {
            get { return -(m_tzi.bias + m_tzi.standardBias); }
        }

        /// <summary>
        /// The offset from UTC during daylight time.
        /// </summary>
        public int DaylightBias
        {
            get { return -(m_tzi.bias + m_tzi.daylightBias); }
        }

        /// <summary>
        /// Get a TimeZoneInformation for a supplied index.
        /// </summary>
        /// <param name="index">The time zone to find.</param>
        /// <returns>The corresponding TimeZoneInformation.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if the index is not found.</exception>
        public static TimeZoneInformation FromIndex(int index)
        {
            TimeZoneInformation[] zones = EnumZones();

            for (int i = 0; i < zones.Length; ++i)
            {
                if (zones[i].Index == index)
                    return zones[i];
            }

            throw new ArgumentOutOfRangeException("index", index, "Unknown time zone index");
        }


        /// <summary>
        /// Enumerate the available time zones
        /// </summary>
        /// <returns>The list of known time zones</returns>
        public static TimeZoneInformation[] EnumZones()
        {
            if (s_zones == null)
            {
                lock (s_lockZones)
                {
                    if (s_zones == null)
                    {
                        IList<TimeZoneInformation> zones = new List<TimeZoneInformation>();

                        /*using (RegistryKey key =
                            Registry.LocalMachine.OpenSubKey(
                                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Time Zones"))
                        {
                            string[] zoneNames = key.GetSubKeyNames();


                            foreach (string zoneName in zoneNames)
                            {
                                using (RegistryKey subKey = key.OpenSubKey(zoneName))
                                {
                                    var tzi = new TimeZoneInformation();

                                    tzi.m_name = zoneName;
                                    tzi.m_displayName = (string) subKey.GetValue("Display");
                                    tzi.m_standardName = (string) subKey.GetValue("Std");
                                    tzi.m_daylightName = (string) subKey.GetValue("Dlt");
                                    tzi.m_index = (int) (subKey.GetValue("Index"));

                                    tzi.InitTzi((byte[]) subKey.GetValue("Tzi"));

                                    zones.Add(tzi);
                                }
                            }
                        }*/

                        ReadOnlyCollection<TimeZoneInfo> timeZones = TimeZoneInfo.GetSystemTimeZones();
                        foreach (TimeZoneInfo timeZone in timeZones) {
                            var tzi = new TimeZoneInformation {
                                m_name = timeZone.DisplayName,
                                m_displayName = timeZone.DisplayName,
                                m_standardName = timeZone.StandardName,
                                m_daylightName = timeZone.DaylightName,
                                m_index = GetTimeZoneIndex(timeZone.DisplayName)
                            };

                            zones.Add(tzi);
                        }

                        s_zones = new TimeZoneInformation[zones.Count];

                        s_zones = zones.ToArray();
                        //zones.CopyTo(s_zones,);
                    }
                }
            }

            return s_zones;
        }

        private static int GetTimeZoneIndex(string zoneName) {
            int timezoneIndex = 0;

            int startIndex = zoneName.IndexOf(')') + 2;
            string filteredZoneName = zoneName.Substring(startIndex, zoneName.Length - startIndex);
            switch (filteredZoneName) {
                case "Kabul":
                    timezoneIndex = 175;
                    break;
                case "Alaska":
                    timezoneIndex = 3;
                    break;
                case "Kuwait, Riyadh":
                    timezoneIndex = 150;
                    break;
                case "Abu Dhabi, Muscat":
                    timezoneIndex = 165;
                    break;
                case "Baghdad":
                    timezoneIndex = 158;
                    break;
                case "Buenos Aires":
                    timezoneIndex = -2147483572;
                    break;
                case "Yerevan":
                    timezoneIndex = -2147483574;
                    break;
                case "Atlantic Time (Canada)":
                    timezoneIndex = 50;
                    break;
                case "Darwin":
                    timezoneIndex = 245;
                    break;
                case "Canberra, Melbourne, Sydney":
                    timezoneIndex = 255;
                    break;
                case "Baku":
                    timezoneIndex = -2147483584;
                    break;
                case "Azores":
                    timezoneIndex = 80;
                    break;
                case "Salvador":
                    timezoneIndex = -2147483558;
                    break;
                case "Dhaka":
                    timezoneIndex = -2147483565;
                    break;
                case "Saskatchewan":
                    timezoneIndex = 25;
                    break;
                case "Cape Verde Is.":
                    timezoneIndex = 83;
                    break;
                case "Caucasus Standard Time":
                    timezoneIndex = 170;
                    break;
                case "Adelaide":
                    timezoneIndex = 250;
                    break;
                case "Central America":
                    timezoneIndex = 33;
                    break;
                case "Astana":
                    timezoneIndex = 195;
                    break;
                case "Cuiaba":
                    timezoneIndex = -2147483576;
                    break;
                case "Belgrade, Bratislava, Budapest, Ljubljana, Prague":
                    timezoneIndex = 95;
                    break;
                case "Sarajevo, Skopje, Warsaw, Zagreb":
                    timezoneIndex = 100;
                    break;
                case "Solomon Is., New Caledonia":
                    timezoneIndex = 280;
                    break;
                case "Central Time (US & Canada)":
                    timezoneIndex = 20;
                    break;
                case "Guadalajara, Mexico City, Monterrey":
                    timezoneIndex = -2147483581;
                    break;
                case "Beijing, Chongqing, Hong Kong, Urumqi":
                    timezoneIndex = 210;
                    break;
                case "International Date Line West":
                    timezoneIndex = 0;
                    break;
                case "Nairobi":
                    timezoneIndex = 155;
                    break;
                case "Brisbane":
                    timezoneIndex = 260;
                    break;
                case "E. Europe":
                    timezoneIndex = 115;
                    break;
                case "Brasilia":
                    timezoneIndex = 65;
                    break;
                case "Eastern Time (US & Canada)":
                    timezoneIndex = 35;
                    break;
                case "Cairo":
                    timezoneIndex = 120;
                    break;
                case "Ekaterinburg":
                    timezoneIndex = 180;
                    break;
                case "Fiji":
                    timezoneIndex = 285;
                    break;
                case "Helsinki, Kyiv, Riga, Sofia, Tallinn, Vilnius":
                    timezoneIndex = 125;
                    break;
                case "Tbilisi":
                    timezoneIndex = -2147483577;
                    break;
                case "Dublin, Edinburgh, Lisbon, London":
                    timezoneIndex = 85;
                    break;
                case "Greenland":
                    timezoneIndex = 73;
                    break;
                case "Monrovia, Reykjavik":
                    timezoneIndex = 90;
                    break;
                case "Athens, Bucharest":
                    timezoneIndex = 130;
                    break;
                case "Hawaii":
                    timezoneIndex = 2;
                    break;
                case "Chennai, Kolkata, Mumbai, New Delhi":
                    timezoneIndex = 190;
                    break;
                case "Tehran":
                    timezoneIndex = 160;
                    break;
                case "Jerusalem":
                    timezoneIndex = 135;
                    break;
                case "Amman":
                    timezoneIndex = -2147483582;
                    break;
                case "Kaliningrad, Minsk":
                    timezoneIndex = -2147483559;
                    break;
                case "Petropavlovsk-Kamchatsky - Old":
                    timezoneIndex = -2147483566;
                    break;
                case "Seoul":
                    timezoneIndex = 230;
                    break;
                case "Tripoli":
                    timezoneIndex = -2147483557;
                    break;
                case "Magadan":
                    timezoneIndex = -2147483561;
                    break;
                case "Port Louis":
                    timezoneIndex = -2147483569;
                    break;
                case "Guadalajara, Mexico City, Monterrey - Old":
                    timezoneIndex = 30;
                    break;
                case "Chihuahua, La Paz, Mazatlan - Old":
                    timezoneIndex = 13;
                    break;
                case "Mid-Atlantic - Old":
                    timezoneIndex = 75;
                    break;
                case "Beirut":
                    timezoneIndex = -2147483583;
                    break;
                case "Montevideo":
                    timezoneIndex = -2147483575;
                    break;
                case "Casablanca":
                    timezoneIndex = -2147483571;
                    break;
                case "Mountain Time (US & Canada)":
                    timezoneIndex = 10;
                    break;
                case "Chihuahua, La Paz, Mazatlan":
                    timezoneIndex = -2147483580;
                    break;
                case "Yangon (Rangoon)":
                    timezoneIndex = 203;
                    break;
                case "Novosibirsk":
                    timezoneIndex = 201;
                    break;
                case "Windhoek":
                    timezoneIndex = -2147483578;
                    break;
                case "Kathmandu":
                    timezoneIndex = 193;
                    break;
                case "Auckland, Wellington":
                    timezoneIndex = 290;
                    break;
                case "Newfoundland":
                    timezoneIndex = 60;
                    break;
                case "Irkutsk":
                    timezoneIndex = 227;
                    break;
                case "Krasnoyarsk":
                    timezoneIndex = 207;
                    break;
                case "Santiago":
                    timezoneIndex = 56;
                    break;
                case "Pacific Time (US & Canada)":
                    timezoneIndex = 4;
                    break;
                case "Baja California":
                    timezoneIndex = -2147483579;
                    break;
                case "Islamabad, Karachi":
                    timezoneIndex = -2147483570;
                    break;
                case "Asuncion":
                    timezoneIndex = -2147483567;
                    break;
                case "Brussels, Copenhagen, Madrid, Paris":
                    timezoneIndex = 105;
                    break;
                case "Moscow, St. Petersburg, Volgograd":
                    timezoneIndex = 145;
                    break;
                case "Cayenne, Fortaleza":
                    timezoneIndex = 70;
                    break;
                case "Bogota, Lima, Quito":
                    timezoneIndex = 45;
                    break;
                case "Georgetown, La Paz, Manaus, San Juan":
                    timezoneIndex = 55;
                    break;
                case "Samoa":
                    timezoneIndex = 1;
                    break;
                case "Bangkok, Hanoi, Jakarta":
                    timezoneIndex = 205;
                    break;
                case "Kuala Lumpur, Singapore":
                    timezoneIndex = 215;
                    break;
                case "Harare, Pretoria":
                    timezoneIndex = 140;
                    break;
                case "Sri Jayawardenepura":
                    timezoneIndex = 200;
                    break;
                case "Damascus":
                    timezoneIndex = -2147483562;
                    break;
                case "Taipei":
                    timezoneIndex = 220;
                    break;
                case "Hobart":
                    timezoneIndex = 265;
                    break;
                case "Osaka, Sapporo, Tokyo":
                    timezoneIndex = 235;
                    break;
                case "Nuku'alofa":
                    timezoneIndex = 300;
                    break;
                case "Istanbul":
                    timezoneIndex = -2147483560;
                    break;
                case "Ulaanbaatar":
                    timezoneIndex = -2147483563;
                    break;
                case "Indiana (East)":
                    timezoneIndex = 40;
                    break;
                case "Arizona":
                    timezoneIndex = 15;
                    break;
                case "Coordinated Universal Time":
                    timezoneIndex = -2147483568;
                    break;
                case "Coordinated Universal Time+12":
                    timezoneIndex = -2147479528;
                    break;
                case "Coordinated Universal Time-02":
                    timezoneIndex = -2147479550;
                    break;
                case "Coordinated Universal Time-11":
                    timezoneIndex = -2147479541;
                    break;
                case "Caracas":
                    timezoneIndex = -2147483573;
                    break;
                case "Vladivostok":
                    timezoneIndex = 270;
                    break;
                case "Perth":
                    timezoneIndex = 225;
                    break;
                case "West Central Africa":
                    timezoneIndex = 113;
                    break;
                case "Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna":
                    timezoneIndex = 110;
                    break;
                case "Ashgabat, Tashkent":
                    timezoneIndex = 185;
                    break;
                case "Guam, Port Moresby":
                    timezoneIndex = 275;
                    break;
                case "Yakutsk":
                    timezoneIndex = 240;
                    break;
            }

            return timezoneIndex;
        }

        private static string GetTimeZoneID(int index) {
            string displayName = "";
            #region Switch to Get DisplayNam
            switch (index) {
                case 175: displayName = "(GMT+04:30) Kabul";
                    break;
                case 3: displayName = "(GMT-09:00) Alaska";
                    break;
                case 150: displayName = "(GMT+03:00) Kuwait, Riyadh";
                    break;
                case 165: displayName = "(GMT+04:00) Abu Dhabi, Muscat";
                    break;
                case 158: displayName = "(GMT+03:00) Baghdad";
                    break;
                case -2147483572: displayName = "(GMT-03:00) Buenos Aires";
                    break;
                case -2147483574: displayName = "(GMT+04:00) Yerevan";
                    break;
                case 50: displayName = "(GMT-04:00) Atlantic Time (Canada)";
                    break;
                case 245: displayName = "(GMT+09:30) Darwin";
                    break;
                case 255: displayName = "(GMT+10:00) Canberra, Melbourne, Sydney";
                    break;
                case -2147483584: displayName = "(GMT+04:00) Baku";
                    break;
                case 80: displayName = "(GMT-01:00) Azores";
                    break;
                case -2147483558: displayName = "(GMT-03:00) Salvador";
                    break;
                case -2147483565: displayName = "(GMT+06:00) Dhaka";
                    break;
                case 25: displayName = "(GMT-06:00) Saskatchewan";
                    break;
                case 83: displayName = "(GMT-01:00) Cape Verde Is.";
                    break;
                case 170: displayName = "(GMT+04:00) Caucasus Standard Time";
                    break;
                case 250: displayName = "(GMT+09:30) Adelaide";
                    break;
                case 33: displayName = "(GMT-06:00) Central America";
                    break;
                case 195: displayName = "(GMT+06:00) Astana";
                    break;
                case -2147483576: displayName = "(GMT-04:00) Cuiaba";
                    break;
                case 95: displayName = "(GMT+01:00) Belgrade, Bratislava, Budapest, Ljubljana, Prague";
                    break;
                case 100: displayName = "(GMT+01:00) Sarajevo, Skopje, Warsaw, Zagreb";
                    break;
                case 280: displayName = "(GMT+11:00) Solomon Is., New Caledonia";
                    break;
                case 20: displayName = "(GMT-06:00) Central Time (US & Canada)";
                    break;
                case -2147483581: displayName = "(GMT-06:00) Guadalajara, Mexico City, Monterrey - New";
                    break;
                case 210: displayName = "(GMT+08:00) Beijing, Chongqing, Hong Kong, Urumqi";
                    break;
                case 0: displayName = "(GMT-12:00) International Date Line West";
                    break;
                case 155: displayName = "(GMT+03:00) Nairobi";
                    break;
                case 260: displayName = "(GMT+10:00) Brisbane";
                    break;
                case 115: displayName = "(GMT+02:00) E. Europe";
                    break;
                case 65: displayName = "(GMT-03:00) Brasilia";
                    break;
                case 35: displayName = "(GMT-05:00) Eastern Time (US & Canada)";
                    break;
                case 120: displayName = "(GMT+02:00) Cairo";
                    break;
                case 180: displayName = "(GMT+06:00) Ekaterinburg";
                    break;
                case 285: displayName = "(GMT+12:00) Fiji";
                    break;
                case 125: displayName = "(GMT+02:00) Helsinki, Kyiv, Riga, Sofia, Tallinn, Vilnius";
                    break;
                case -2147483577: displayName = "(GMT+04:00) Tbilisi";
                    break;
                case 85: displayName = "(GMT) Greenwich Mean Time : Dublin, Edinburgh, Lisbon, London";
                    break;
                case 73: displayName = "(GMT-03:00) Greenland";
                    break;
                case 90: displayName = "(GMT) Monrovia, Reykjavik";
                    break;
                case 130: displayName = "(GMT+02:00) Athens, Bucharest";
                    break;
                case 2: displayName = "(GMT-10:00) Hawaii";
                    break;
                case 190: displayName = "(GMT+05:30) Chennai, Kolkata, Mumbai, New Delhi";
                    break;
                case 160: displayName = "(GMT+03:30) Tehran";
                    break;
                case 135: displayName = "(GMT+02:00) Jerusalem";
                    break;
                case -2147483582: displayName = "(GMT+03:00) Amman";
                    break;
                case -2147483559: displayName = "(GMT+03:00) Kaliningrad, Minsk";
                    break;
                case -2147483566: displayName = "(GMT+12:00) Petropavlovsk-Kamchatsky - Old";
                    break;
                case 230: displayName = "(GMT+09:00) Seoul";
                    break;
                case -2147483557: displayName = "(GMT+02:00) Tripoli";
                    break;
                case -2147483561: displayName = "(GMT+12:00) Magadan";
                    break;
                case -2147483569: displayName = "(GMT+04:00) Port Louis";
                    break;
                case 30: displayName = "(GMT-06:00) Guadalajara, Mexico City, Monterrey - Old";
                    break;
                case 13: displayName = "(GMT-07:00) Chihuahua, La Paz, Mazatlan - Old";
                    break;
                case 75: displayName = "(GMT-02:00) Mid-Atlantic - Old";
                    break;
                case -2147483583: displayName = "(GMT+02:00) Beirut";
                    break;
                case -2147483575: displayName = "(GMT-03:00) Montevideo";
                    break;
                case -2147483571: displayName = "(GMT) Casablanca";
                    break;
                case 10: displayName = "(GMT-07:00) Mountain Time (US & Canada)";
                    break;
                case -2147483580: displayName = "(GMT-07:00) Chihuahua, La Paz, Mazatlan - New";
                    break;
                case 203: displayName = "(GMT+06:30) Yangon (Rangoon)";
                    break;
                case 201: displayName = "(GMT+07:00) Novosibirsk";
                    break;
                case -2147483578: displayName = "(GMT+01:00) Windhoek";
                    break;
                case 193: displayName = "(GMT+05:45) Kathmandu";
                    break;
                case 290: displayName = "(GMT+12:00) Auckland, Wellington";
                    break;
                case 60: displayName = "(GMT-03:30) Newfoundland";
                    break;
                case 227: displayName = "(GMT+09:00) Irkutsk";
                    break;
                case 207: displayName = "(GMT+08:00) Krasnoyarsk";
                    break;
                case 56: displayName = "(GMT-04:00) Santiago";
                    break;
                case 4: displayName = "(GMT-08:00) Pacific Time (US & Canada)";
                    break;
                case -2147483579: displayName = "(GMT-08:00) Baja California";
                    break;
                case -2147483570: displayName = "(GMT+05:00) Islamabad, Karachi";
                    break;
                case -2147483567: displayName = "(GMT-04:00) Asuncion";
                    break;
                case 105: displayName = "(GMT+01:00) Brussels, Copenhagen, Madrid, Paris";
                    break;
                case 145: displayName = "(GMT+04:00) Moscow, St. Petersburg, Volgograd";
                    break;
                case 70: displayName = "(GMT-03:00) Cayenne, Fortaleza";
                    break;
                case 45: displayName = "(GMT-05:00) Bogota, Lima, Quito";
                    break;
                case 55: displayName = "(GMT-04:00) Georgetown, La Paz, Manaus, San Juan";
                    break;
                case 1: displayName = "(GMT+13:00) Samoa";
                    break;
                case 205: displayName = "(GMT+07:00) Bangkok, Hanoi, Jakarta";
                    break;
                case 215: displayName = "(GMT+08:00) Kuala Lumpur, Singapore";
                    break;
                case 140: displayName = "(GMT+02:00) Harare, Pretoria";
                    break;
                case 200: displayName = "(GMT+05:30) Sri Jayawardenepura";
                    break;
                case -2147483562: displayName = "(GMT+02:00) Damascus";
                    break;
                case 220: displayName = "(GMT+08:00) Taipei";
                    break;
                case 265: displayName = "(GMT+10:00) Hobart";
                    break;
                case 235: displayName = "(GMT+09:00) Osaka, Sapporo, Tokyo";
                    break;
                case 300: displayName = "(GMT+13:00) Nuku'alofa";
                    break;
                case -2147483560: displayName = "(GMT+02:00) Istanbul";
                    break;
                case -2147483563: displayName = "(GMT+08:00) Ulaanbaatar";
                    break;
                case 40: displayName = "(GMT-05:00) Indiana (East)";
                    break;
                case 15: displayName = "(GMT-07:00) Arizona";
                    break;
                case -2147483568: displayName = "(GMT) Coordinated Universal Time";
                    break;
                case -2147479528: displayName = "(GMT+12:00) Coordinated Universal Time+12";
                    break;
                case -2147479550: displayName = "(GMT-02:00) Coordinated Universal Time-02";
                    break;
                case -2147479541: displayName = "(GMT-11:00) Coordinated Universal Time-11";
                    break;
                case -2147483573: displayName = "(GMT-04:30) Caracas";
                    break;
                case 270: displayName = "(GMT+11:00) Vladivostok";
                    break;
                case 225: displayName = "(GMT+08:00) Perth";
                    break;
                case 113: displayName = "(GMT+01:00) West Central Africa";
                    break;
                case 110: displayName = "(GMT+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna";
                    break;
                case 185: displayName = "(GMT+05:00) Ashgabat, Tashkent";
                    break;
                case 275: displayName = "(GMT+10:00) Guam, Port Moresby";
                    break;
                case 240: displayName = "(GMT+10:00) Yakutsk";
                    break;
            }
            #endregion

            string id = "";

            #region switch to get ID
            switch (displayName) {
                case "(GMT-12:00) International Date Line West": id = "Dateline Standard Time";
                    break;
                case "(GMT-11:00) Coordinated Universal Time-11": id = "UTC-11";
                    break;
                case "(GMT-10:00) Hawaii": id = "Hawaiian Standard Time";
                    break;
                case "(GMT-09:00) Alaska": id = "Alaskan Standard Time";
                    break;
                case "(GMT-08:00) Baja California": id = "Pacific Standard Time (Mexico)";
                    break;
                case "(GMT-08:00) Pacific Time (US & Canada)": id = "Pacific Standard Time";
                    break;
                case "(GMT-07:00) Arizona": id = "US Mountain Standard Time";
                    break;
                case "(GMT-07:00) Chihuahua, La Paz, Mazatlan - New": id = "Mountain Standard Time (Mexico)";
                    break;
                case "(GMT-07:00) Chihuahua, La Paz, Mazatlan - Old": id = "Mexico Standard Time 2";
                    break;
                case "(GMT-07:00) Mountain Time (US & Canada)": id = "Mountain Standard Time";
                    break;
                case "(GMT-06:00) Central America": id = "Central America Standard Time";
                    break;
                case "(GMT-06:00) Central Time (US & Canada)": id = "Central Standard Time";
                    break;
                case "(GMT-06:00) Guadalajara, Mexico City, Monterrey - New": id = "Central Standard Time (Mexico)";
                    break;
                case "(GMT-06:00) Guadalajara, Mexico City, Monterrey - Old": id = "Mexico Standard Time";
                    break;
                case "(GMT-06:00) Saskatchewan": id = "Canada Central Standard Time";
                    break;
                case "(GMT-05:00) Bogota, Lima, Quito": id = "SA Pacific Standard Time";
                    break;
                case "(GMT-05:00) Eastern Time (US & Canada)": id = "Eastern Standard Time";
                    break;
                case "(GMT-05:00) Indiana (East)": id = "US Eastern Standard Time";
                    break;
                case "(GMT-04:30) Caracas": id = "Venezuela Standard Time";
                    break;
                case "(GMT-04:00) Asuncion": id = "Paraguay Standard Time";
                    break;
                case "(GMT-04:00) Atlantic Time (Canada)": id = "Atlantic Standard Time";
                    break;
                case "(GMT-04:00) Cuiaba": id = "Central Brazilian Standard Time";
                    break;
                case "(GMT-04:00) Georgetown, La Paz, Manaus, San Juan": id = "SA Western Standard Time";
                    break;
                case "(GMT-04:00) Santiago": id = "Pacific SA Standard Time";
                    break;
                case "(GMT-03:30) Newfoundland": id = "Newfoundland Standard Time";
                    break;
                case "(GMT-03:00) Brasilia": id = "E. South America Standard Time";
                    break;
                case "(GMT-03:00) Buenos Aires": id = "Argentina Standard Time";
                    break;
                case "(GMT-03:00) Cayenne, Fortaleza": id = "SA Eastern Standard Time";
                    break;
                case "(GMT-03:00) Greenland": id = "Greenland Standard Time";
                    break;
                case "(GMT-03:00) Montevideo": id = "Montevideo Standard Time";
                    break;
                case "(GMT-03:00) Salvador": id = "Bahia Standard Time";
                    break;
                case "(GMT-02:00) Coordinated Universal Time-02": id = "UTC-02";
                    break;
                case "(GMT-02:00) Mid-Atlantic - Old": id = "Mid-Atlantic Standard Time";
                    break;
                case "(GMT-01:00) Azores": id = "Azores Standard Time";
                    break;
                case "(GMT-01:00) Cape Verde Is.": id = "Cape Verde Standard Time";
                    break;
                case "(GMT) Casablanca": id = "Morocco Standard Time";
                    break;
                case "(GMT) Coordinated Universal Time": id = "UTC";
                    break;
                case "(GMT) Greenwich Mean Time : Dublin, Edinburgh, Lisbon, London": id = "GMT Standard Time";
                    break;
                case "(GMT) Monrovia, Reykjavik": id = "Greenwich Standard Time";
                    break;
                case "(GMT+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna": id = "W. Europe Standard Time";
                    break;
                case "(GMT+01:00) Belgrade, Bratislava, Budapest, Ljubljana, Prague": id = "Central Europe Standard Time";
                    break;
                case "(GMT+01:00) Brussels, Copenhagen, Madrid, Paris": id = "Romance Standard Time";
                    break;
                case "(GMT+01:00) Sarajevo, Skopje, Warsaw, Zagreb": id = "Central European Standard Time";
                    break;
                case "(GMT+01:00) West Central Africa": id = "W. Central Africa Standard Time";
                    break;
                case "(GMT+01:00) Windhoek": id = "Namibia Standard Time";
                    break;
                case "(GMT+02:00) Athens, Bucharest": id = "GTB Standard Time";
                    break;
                case "(GMT+02:00) Beirut": id = "Middle East Standard Time";
                    break;
                case "(GMT+02:00) Cairo": id = "Egypt Standard Time";
                    break;
                case "(GMT+02:00) Damascus": id = "Syria Standard Time";
                    break;
                case "(GMT+02:00) E. Europe": id = "E. Europe Standard Time";
                    break;
                case "(GMT+02:00) Harare, Pretoria": id = "South Africa Standard Time";
                    break;
                case "(GMT+02:00) Helsinki, Kyiv, Riga, Sofia, Tallinn, Vilnius": id = "FLE Standard Time";
                    break;
                case "(GMT+02:00) Istanbul": id = "Turkey Standard Time";
                    break;
                case "(GMT+02:00) Jerusalem": id = "Israel Standard Time";
                    break;
                case "(GMT+02:00) Tripoli": id = "Libya Standard Time";
                    break;
                case "(GMT+03:00) Amman": id = "Jordan Standard Time";
                    break;
                case "(GMT+03:00) Baghdad": id = "Arabic Standard Time";
                    break;
                case "(GMT+03:00) Kaliningrad, Minsk": id = "Kaliningrad Standard Time";
                    break;
                case "(GMT+03:00) Kuwait, Riyadh": id = "Arab Standard Time";
                    break;
                case "(GMT+03:00) Nairobi": id = "E. Africa Standard Time";
                    break;
                case "(GMT+03:30) Tehran": id = "Iran Standard Time";
                    break;
                case "(GMT+04:00) Abu Dhabi, Muscat": id = "Arabian Standard Time";
                    break;
                case "(GMT+04:00) Baku": id = "Azerbaijan Standard Time";
                    break;
                case "(GMT+04:00) Caucasus Standard Time": id = "Caucasus Standard Time";
                    break;
                case "(GMT+04:00) Moscow, St. Petersburg, Volgograd": id = "Russian Standard Time";
                    break;
                case "(GMT+04:00) Port Louis": id = "Mauritius Standard Time";
                    break;
                case "(GMT+04:00) Tbilisi": id = "Georgian Standard Time";
                    break;
                case "(GMT+04:00) Yerevan": id = "Armenian Standard Time";
                    break;
                case "(GMT+04:30) Kabul": id = "Afghanistan Standard Time";
                    break;
                case "(GMT+05:00) Ashgabat, Tashkent": id = "West Asia Standard Time";
                    break;
                case "(GMT+05:00) Islamabad, Karachi": id = "Pakistan Standard Time";
                    break;
                case "(GMT+05:30) Chennai, Kolkata, Mumbai, New Delhi": id = "India Standard Time";
                    break;
                case "(GMT+05:30) Sri Jayawardenepura": id = "Sri Lanka Standard Time";
                    break;
                case "(GMT+05:45) Kathmandu": id = "Nepal Standard Time";
                    break;
                case "(GMT+06:00) Astana": id = "Central Asia Standard Time";
                    break;
                case "(GMT+06:00) Dhaka": id = "Bangladesh Standard Time";
                    break;
                case "(GMT+06:00) Ekaterinburg": id = "Ekaterinburg Standard Time";
                    break;
                case "(GMT+06:30) Yangon (Rangoon)": id = "Myanmar Standard Time";
                    break;
                case "(GMT+07:00) Bangkok, Hanoi, Jakarta": id = "SE Asia Standard Time";
                    break;
                case "(GMT+07:00) Novosibirsk": id = "N. Central Asia Standard Time";
                    break;
                case "(GMT+08:00) Beijing, Chongqing, Hong Kong, Urumqi": id = "China Standard Time";
                    break;
                case "(GMT+08:00) Krasnoyarsk": id = "North Asia Standard Time";
                    break;
                case "(GMT+08:00) Kuala Lumpur, Singapore": id = "Singapore Standard Time";
                    break;
                case "(GMT+08:00) Perth": id = "W. Australia Standard Time";
                    break;
                case "(GMT+08:00) Taipei": id = "Taipei Standard Time";
                    break;
                case "(GMT+08:00) Ulaanbaatar": id = "Ulaanbaatar Standard Time";
                    break;
                case "(GMT+09:00) Irkutsk": id = "North Asia East Standard Time";
                    break;
                case "(GMT+09:00) Osaka, Sapporo, Tokyo": id = "Tokyo Standard Time";
                    break;
                case "(GMT+09:00) Seoul": id = "Korea Standard Time";
                    break;
                case "(GMT+09:30) Adelaide": id = "Cen. Australia Standard Time";
                    break;
                case "(GMT+09:30) Darwin": id = "AUS Central Standard Time";
                    break;
                case "(GMT+10:00) Brisbane": id = "E. Australia Standard Time";
                    break;
                case "(GMT+10:00) Canberra, Melbourne, Sydney": id = "AUS Eastern Standard Time";
                    break;
                case "(GMT+10:00) Guam, Port Moresby": id = "West Pacific Standard Time";
                    break;
                case "(GMT+10:00) Hobart": id = "Tasmania Standard Time";
                    break;
                case "(GMT+10:00) Yakutsk": id = "Yakutsk Standard Time";
                    break;
                case "(GMT+11:00) Solomon Is., New Caledonia": id = "Central Pacific Standard Time";
                    break;
                case "(GMT+11:00) Vladivostok": id = "Vladivostok Standard Time";
                    break;
                case "(GMT+12:00) Auckland, Wellington": id = "New Zealand Standard Time";
                    break;
                case "(GMT+12:00) Coordinated Universal Time+12": id = "UTC+12";
                    break;
                case "(GMT+12:00) Fiji": id = "Fiji Standard Time";
                    break;
                case "(GMT+12:00) Magadan": id = "Magadan Standard Time";
                    break;
                case "(GMT+12:00) Petropavlovsk-Kamchatsky - Old": id = "Kamchatka Standard Time";
                    break;
                case "(GMT+13:00) Nuku'alofa": id = "Tonga Standard Time";
                    break;
                case "(GMT+13:00) Samoa": id = "Samoa Standard Time";
                    break;
            }
            #endregion

            return id;
        }
        public override string ToString()
        {
            return m_displayName;
        }

        /// <summary>
        /// Initialise the m_tzi member.
        /// </summary>
        /// <param name="info">The Tzi data from the registry.</param>
        private void InitTzi(byte[] info)
        {
            if (info.Length != Marshal.SizeOf(m_tzi))
            {
                throw new ArgumentException("Information size is incorrect", "info");
            }

            // Could have sworn there's a Marshal operation to pack bytes into
            // a structure, but I can't see it. Do it manually.

            GCHandle h = GCHandle.Alloc(info, GCHandleType.Pinned);

            try
            {
                m_tzi = (TZI) Marshal.PtrToStructure(h.AddrOfPinnedObject(), typeof (TZI));
            }
            finally
            {
                h.Free();
            }
        }


        private TIME_ZONE_INFORMATION TziNative()
        {
            var tziNative = new TIME_ZONE_INFORMATION();

            tziNative.Bias = m_tzi.bias;
            tziNative.StandardDate = m_tzi.standardDate;
            tziNative.StandardBias = m_tzi.standardBias;
            tziNative.DaylightDate = m_tzi.daylightDate;
            tziNative.DaylightBias = m_tzi.daylightBias;

            return tziNative;
        }


        /// <summary>
        /// Convert a time interpreted as UTC to a time in this time zone.
        /// </summary>
        /// <param name="utc">The UTC time to convert.</param>
        /// <returns>The corresponding local time in this zone.</returns>
        public DateTime FromUniversalTime(DateTime utc)
        {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            // Convert to SYSTEMTIME
            SYSTEMTIME stUTC = DateTimeToSystemTime(utc);

            // Set up the TIME_ZONE_INFORMATION

            TIME_ZONE_INFORMATION tziNative = TziNative();

            SYSTEMTIME stLocal;

            NativeMethods.SystemTimeToTzSpecificLocalTime(ref tziNative, ref stUTC, out stLocal);

            // Convert back to DateTime
            return SystemTimeToDateTime(ref stLocal);
        }


        /// <summary>
        /// Convert a time from UTC to the time zone with the supplied index.
        /// </summary>
        /// <param name="index">The time zone index.</param>
        /// <param name="utc">The time to convert.</param>
        /// <returns>The converted time.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is not found.</exception>
        public static DateTime FromUniversalTime(int index, DateTime utc)
        {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            TimeZoneInformation tzi = FromIndex(index);

            return tzi.FromUniversalTime(utc);
        }


        /// <summary>
        /// Convert a time interpreted as a local time in this zone to the equivalent UTC.
        /// Note that there may be different possible interpretations at the daylight
        /// time boundaries.
        /// </summary>
        /// <param name="local">The local time to convert.</param>
        /// <returns>The corresponding UTC.</returns>
        /// <exception cref="NotSupportedException">Thrown if the method failed due to missing platform support.</exception>
        public DateTime ToUniversalTime(DateTime local)
        {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            SYSTEMTIME stLocal = DateTimeToSystemTime(local);

            TIME_ZONE_INFORMATION tziNative = TziNative();

            SYSTEMTIME stUTC;

            try
            {
                NativeMethods.TzSpecificLocalTimeToSystemTime(ref tziNative, ref stLocal, out stUTC);

                return SystemTimeToDateTime(ref stUTC);
            }
            catch (EntryPointNotFoundException e)
            {
                throw new NotSupportedException("This method is not supported on this operating system", e);
            }
        }


        /// <summary>
        /// Convert a time from the time zone with the supplied index to UTC.
        /// </summary>
        /// <param name="index">The time zone index.</param>
        /// <param name="utc">The time to convert.</param>
        /// <returns>The converted time.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is not found.</exception>
        /// <exception cref="NotSupportedException">Thrown if the method failed due to missing platform support.</exception>
        public static DateTime ToUniversalTime(int index, DateTime local)
        {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            TimeZoneInformation tzi = FromIndex(index);

            return tzi.ToUniversalTime(local);
        }

        public static DateTime ToLocalTime(int index, DateTime local) {
            var timezoneId = GetTimeZoneID(index);
            var localTime = DateTime.Now;

            if (timezoneId.Length != 0) {
                var tz = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);

                if (tz != null) {
                    localTime = TimeZoneInfo.ConvertTime(local, TimeZoneInfo.Local, tz);
                }
            }
            return localTime;
        }

        private static SYSTEMTIME DateTimeToSystemTime(DateTime dt)
        {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            SYSTEMTIME st;
            var ft = new System.Runtime.InteropServices.ComTypes.FILETIME();

            ft.dwHighDateTime = (int) (dt.Ticks >> 32);
            ft.dwLowDateTime = (int) (dt.Ticks & 0xFFFFFFFFL);

            NativeMethods.FileTimeToSystemTime(ref ft, out st);

            return st;
        }


        private static DateTime SystemTimeToDateTime(ref SYSTEMTIME st)
        {
            //Force all datetime to be in US format.
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            var ft = new System.Runtime.InteropServices.ComTypes.FILETIME();

            NativeMethods.SystemTimeToFileTime(ref st, out ft);

            var dt = new DateTime((((long) ft.dwHighDateTime) << 32) | (uint) ft.dwLowDateTime);

            return dt;
        }

        /// <summary>
        /// A container for P/Invoke declarations.
        /// </summary>
        private struct NativeMethods
        {
            private const string KERNEL32 = "kernel32.dll";

            [DllImport(KERNEL32)]
            public static extern uint GetTimeZoneInformation(out TIME_ZONE_INFORMATION
                lpTimeZoneInformation);

            [DllImport(KERNEL32)]
            public static extern bool SystemTimeToTzSpecificLocalTime(
                [In] ref TIME_ZONE_INFORMATION lpTimeZone,
                [In] ref SYSTEMTIME lpUniversalTime,
                out SYSTEMTIME lpLocalTime);

            [DllImport(KERNEL32)]
            public static extern bool SystemTimeToFileTime(
                [In] ref SYSTEMTIME lpSystemTime,
                out System.Runtime.InteropServices.ComTypes.FILETIME lpFileTime);

            [DllImport(KERNEL32)]
            public static extern bool FileTimeToSystemTime(
                [In] ref System.Runtime.InteropServices.ComTypes.FILETIME lpFileTime,
                out SYSTEMTIME lpSystemTime);

            /// <summary>
            /// Convert a local time to UTC, using the supplied time zone information.
            /// Windows XP and Server 2003 and later only.
            /// </summary>
            /// <param name="lpTimeZone">The time zone to use.</param>
            /// <param name="lpLocalTime">The local time to convert.</param>
            /// <param name="lpUniversalTime">The resultant time in UTC.</param>
            /// <returns>true if successful, false otherwise.</returns>
            [DllImport(KERNEL32)]
            public static extern bool TzSpecificLocalTimeToSystemTime(
                [In] ref TIME_ZONE_INFORMATION lpTimeZone,
                [In] ref SYSTEMTIME lpLocalTime,
                out SYSTEMTIME lpUniversalTime);
        }

        /// <summary>
        /// The standard Windows SYSTEMTIME structure.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEMTIME
        {
            public readonly UInt16 wYear;
            public readonly UInt16 wMonth;
            public readonly UInt16 wDayOfWeek;
            public readonly UInt16 wDay;
            public readonly UInt16 wHour;
            public readonly UInt16 wMinute;
            public readonly UInt16 wSecond;
            public readonly UInt16 wMilliseconds;
        }

        /// <summary>
        /// The standard Win32 TIME_ZONE_INFORMATION structure.
        /// Thanks to www.pinvoke.net.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct TIME_ZONE_INFORMATION
        {
            [MarshalAs(UnmanagedType.I4)] public Int32 Bias;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public readonly string StandardName;
            public SYSTEMTIME StandardDate;
            [MarshalAs(UnmanagedType.I4)] public Int32 StandardBias;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public readonly string DaylightName;
            public SYSTEMTIME DaylightDate;
            [MarshalAs(UnmanagedType.I4)] public Int32 DaylightBias;
        }

        /// <summary>
        /// The layout of the Tzi value in the registry.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct TZI
        {
            public readonly int bias;
            public readonly int standardBias;
            public readonly int daylightBias;
            public readonly SYSTEMTIME standardDate;
            public readonly SYSTEMTIME daylightDate;
        }
    }

    public class TimeZoneComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            TimeZoneInformation tzx, tzy;

            tzx = x as TimeZoneInformation;
            tzy = y as TimeZoneInformation;

            if (tzx == null || tzy == null)
            {
                throw new ArgumentException("Parameter null or wrong type");
            }

            int biasDifference = tzx.Bias - tzy.Bias;

            if (biasDifference == 0)
            {
                return tzx.DisplayName.CompareTo(tzy.DisplayName);
            }
            return biasDifference;
        }
    }
}